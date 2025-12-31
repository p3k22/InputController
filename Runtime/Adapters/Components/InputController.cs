namespace P3k.InputController.Adapters.Components
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;
   using P3k.InputController.Abstractions.Interfaces.Core;
   using P3k.InputController.Implementations.DataContainers;
   using P3k.InputController.Implementations.Services;
   using P3k.InputController.Implementations.Utilities;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   public sealed class InputController : MonoBehaviour, IInputController
   {
      private const string DEFAULT_PROFILE = "default";

      private InputActionAsset _asset;

      private InputBindingService _bindingService;

      [SerializeField]
      private InputConfig _config;

      [SerializeField]
      private string _currentProfile = DEFAULT_PROFILE;

      private readonly Dictionary<string, IInputState> _inputs = new();

      private readonly HashSet<string> _missingIdsLogged = new();

      private InputProfileService _profiles;

      private InputBindingQueryService _query;

      public string CurrentProfile => _currentProfile;

      public IReadOnlyList<IInputDefinition> Inputs => _config != null ? _config.Inputs : null;

      private IInputConfig Config
      {
         get
         {
            if (_config is IInputConfig config)
            {
               return config;
            }

            throw new InvalidOperationException($"{nameof(_config)} must implement {nameof(IInputConfig)}");
         }
      }

      private void Awake()
      {
         _asset = CreateActionAsset(Config, out var createdStates);

         foreach (var kvp in createdStates)
         {
            _inputs[kvp.Key] = kvp.Value;
         }

         _profiles = new InputProfileService(_asset, _inputs, GetDefinition);
         _query = new InputBindingQueryService(_profiles, _inputs, GetDefinition);
         _bindingService = new InputBindingService(_query);

         ProfileLoad(_currentProfile);

         foreach (var state in _inputs.Values)
         {
            state.Enable();
         }

         // Enable actions on the generated asset so InputActionReferences work
         EnableGeneratedAsset();
      }

      private void LateUpdate()
      {
         foreach (var s in _inputs.Values)
         {
            s.Tick();
         }
      }

      public IInputState Get(string id)
      {
         if (string.IsNullOrWhiteSpace(id))
         {
            Debug.LogError($"{nameof(IInputController)}.{nameof(Get)} called with null or empty id.");
            return null;
         }

         if (_inputs.TryGetValue(id, out var state))
         {
            return state;
         }

         // Log once per missing id
         if (_missingIdsLogged.Add(id))
         {
            Debug.LogWarning(
            $"{nameof(IInputController)}: Input id '{id}' was requested but does not exist.\n"
            + $"Available ids: {string.Join(", ", _inputs.Keys)}");
         }

         return null;
      }

      public IInputState Get<TEnum>(TEnum id)
         where TEnum : Enum
      {
         return Get(id.ToString());
      }

      public string GetBindingDisplay(string id, InputDevice device, string bindingName = null)
      {
         return _bindingService?.GetBindingDisplay(id, device, bindingName);
      }

      public bool IsAnalogGamepad(string id, bool? value = null)
      {
         if (_profiles == null)
         {
            return false;
         }

         if (value.HasValue)
         {
            _profiles.SetAnalogGamepad(id, value.Value);
         }

         return _profiles.IsAnalogGamepad(id);
      }

      public bool IsAnalogKeyboard(string id, bool? value = null)
      {
         if (_profiles == null)
         {
            return false;
         }

         if (value.HasValue)
         {
            _profiles.SetAnalogKeyboard(id, value.Value);
         }

         return _profiles.IsAnalogKeyboard(id);
      }

      public bool IsInvertedY(string id, bool? value = null)
      {
         if (_profiles == null)
         {
            return false;
         }

         if (value.HasValue)
         {
            _profiles.SetInvertedY(id, value.Value);
         }

         return _profiles.IsInvertedY(id);
      }

      public bool ProfileCanBeSaved(string profileName)
      {
         return _profiles != null && _profiles.WouldSaveChange(profileName);
      }

      public bool ProfileExists(string profileName)
      {
         return !string.IsNullOrWhiteSpace(profileName) && InputProfileUtils.Exists(profileName);
      }

      public void ProfileLoad(string profileName)
      {
         if (string.IsNullOrWhiteSpace(profileName))
         {
            profileName = DEFAULT_PROFILE;
         }

         _currentProfile = profileName;
         _profiles?.Load(profileName);

         SyncOverridesToGeneratedAsset();
      }

      public void ProfileSave(string profileName)
      {
         if (string.IsNullOrWhiteSpace(profileName))
         {
            profileName = DEFAULT_PROFILE;
         }

         _currentProfile = profileName;
         _profiles?.Save(profileName);

         SyncOverridesToGeneratedAsset();
      }

      public void Rebind(
         string id,
         InputDevice device,
         string bindingName = null,
         Action onComplete = null,
         Action onCancel = null)
      {
         if (!_query.CanRebind(id, device) || !_inputs.TryGetValue(id, out var state) || state == null)
         {
            onCancel?.Invoke();
            return;
         }

         var def = GetDefinition(id);
         if (def == null)
         {
            onCancel?.Invoke();
            return;
         }

         var useAnalogKb = IsAnalogKeyboard(id);
         var useAnalogGp = IsAnalogGamepad(id);

         var action = state.GetRebindAction(device, bindingName, useAnalogKb, useAnalogGp);
         if (action == null)
         {
            onCancel?.Invoke();
            return;
         }

         _bindingService.Begin(
         action,
         device,
         bindingName,
         () =>
            {
               SyncOverridesToGeneratedAsset();
               onComplete?.Invoke();
            },
         () => onCancel?.Invoke());
      }

      public void RebindCancel()
      {
         _bindingService?.Cancel();
      }

      public void ResetBindingsToDefault(string profileName = "")
      {
         _profiles?.ResetToDefaults(profileName);

         SyncOverridesToGeneratedAsset();
      }

      private static InputActionAsset CreateActionAsset(
         IInputConfig config,
         out Dictionary<string, IInputState> statesById)
      {
         var asset = ScriptableObject.CreateInstance<InputActionAsset>();
         var map = asset.AddActionMap("Player");

         statesById = new Dictionary<string, IInputState>();

         if (config == null || config.Inputs == null)
         {
            return asset;
         }

         foreach (var def in config.Inputs)
         {
            if (def == null || string.IsNullOrWhiteSpace(def.Id))
            {
               continue;
            }

            if (def.Type == BindingType.Button)
            {
               var button = InputActionUtils.CreateButtonAction(map, def);

               statesById[def.Id] = new InputState(BindingType.Button, button, null, null, null, null);

               continue;
            }

            var kbKeys = InputActionUtils.CreateKeyboardKeysAction(map, def);
            var kbMouse = InputActionUtils.CreateKeyboardMouseAction(map, def);
            var gpButtons = InputActionUtils.CreateGamepadButtonsAction(map, def);
            var gpStick = InputActionUtils.CreateGamepadStickAction(map, def);

            statesById[def.Id] = new InputState(BindingType.Composite2D, null, kbKeys, kbMouse, gpButtons, gpStick);
         }

         return asset;
      }

      private void EnableGeneratedAsset()
      {
         var generatedAsset = _config?.GeneratedActionAsset;
         if (generatedAsset == null)
         {
            return;
         }

         // Enable all action maps on the generated asset so InputActionReferences respond to input
         foreach (var map in generatedAsset.actionMaps)
         {
            map.Enable();
         }
      }

      private IInputDefinition GetDefinition(string id)
      {
         return !_config ? null : _config.Inputs?.FirstOrDefault(d => d.Id == id);
      }

      /// <summary>
      /// Syncs binding overrides from the runtime asset to the generated asset.
      /// This ensures InputActionReferences pointing to the generated asset
      /// respond to rebound keys.
      /// </summary>
      private void SyncOverridesToGeneratedAsset()
      {
         if (!_config)
         {
            return;
         }

         var generatedAsset = _config.GeneratedActionAsset;
         if (generatedAsset == null || _asset == null)
         {
            return;
         }

         // Get binding overrides from the runtime asset as JSON
         var overridesJson = _asset.SaveBindingOverridesAsJson();

         // Clear existing overrides on the generated asset
         foreach (var map in generatedAsset.actionMaps)
         {
            map.RemoveAllBindingOverrides();
         }

         // Apply the same overrides to the generated asset
         if (!string.IsNullOrEmpty(overridesJson))
         {
            generatedAsset.LoadBindingOverridesFromJson(overridesJson);
         }
      }

      private void OnDestroy()
      {
         // Disable the generated asset to prevent lingering input responses
         DisableGeneratedAsset();

         // Disable all runtime states
         foreach (var state in _inputs.Values)
         {
            state.Disable();
         }

         // Clean up runtime asset
         if (_asset != null)
         {
            Destroy(_asset);
            _asset = null;
         }

         _bindingService?.Dispose();
      }

      private void DisableGeneratedAsset()
      {
         var generatedAsset = _config?.GeneratedActionAsset;
         if (generatedAsset == null)
         {
            return;
         }

         // Disable all action maps and clear overrides on the generated asset
         foreach (var map in generatedAsset.actionMaps)
         {
            map.Disable();
            map.RemoveAllBindingOverrides();
         }
      }
   }
}