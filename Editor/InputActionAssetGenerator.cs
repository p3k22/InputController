using P3k.InputController.Abstractions.Enums;
using P3k.InputController.Abstractions.Interfaces.Configurations;
using P3k.InputController.Implementations.DataContainers;

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generates and syncs an InputActionAsset from an InputConfig ScriptableObject.
/// This allows Action References to be assigned directly in the inspector.
/// </summary>
public static class InputActionAssetGenerator
{
   private const string ACTION_MAP_NAME = "Player";

   /// <summary>
   /// Generates or updates an InputActionAsset based on the provided InputConfig.
   /// The asset is created alongside the InputConfig if it doesn't exist.
   /// </summary>
   /// <param name="config">The InputConfig to generate actions from.</param>
   /// <returns>The generated or updated InputActionAsset, or null if generation failed.</returns>
   public static InputActionAsset GenerateAsset(InputConfig config)
   {
      if (config == null)
      {
         Debug.LogError("[InputActionAssetGenerator] InputConfig is null.");
         return null;
      }

      var configPath = AssetDatabase.GetAssetPath(config);
      if (string.IsNullOrEmpty(configPath))
      {
         Debug.LogError("[InputActionAssetGenerator] InputConfig has no asset path. Save it first.");
         return null;
      }

      var directory = Path.GetDirectoryName(configPath);
      var configName = Path.GetFileNameWithoutExtension(configPath);
      var assetPath = Path.Combine(directory ?? string.Empty, $"{configName}_Actions.inputactions");

      // Check if asset already exists
      var existingAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);

      // Create or rebuild the asset
      var newAsset = BuildInputActionAsset(config);
      if (newAsset == null)
      {
         Debug.LogError("[InputActionAssetGenerator] Failed to build InputActionAsset.");
         return null;
      }

      // Serialize to JSON
      var json = newAsset.ToJson();

      // Write the file
      File.WriteAllText(assetPath, json, new UTF8Encoding(false));

      AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
      AssetDatabase.Refresh();

      var savedAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);

      if (savedAsset != null)
      {
         Debug.Log($"[InputActionAssetGenerator] Successfully generated InputActionAsset at: {assetPath}");
      }

      return savedAsset;
   }

   /// <summary>
   /// Checks if the InputActionAsset needs to be regenerated based on the InputConfig.
   /// </summary>
   /// <param name="config">The InputConfig to check against.</param>
   /// <param name="existingAsset">The existing InputActionAsset to compare.</param>
   /// <returns>True if regeneration is needed, false otherwise.</returns>
   public static bool NeedsRegeneration(InputConfig config, InputActionAsset existingAsset)
   {
      if (config == null || existingAsset == null)
      {
         return true;
      }

      var inputs = config.Inputs;
      if (inputs == null)
      {
         return true;
      }

      var actionMap = existingAsset.FindActionMap(ACTION_MAP_NAME);
      if (actionMap == null)
      {
         return true;
      }

      // Check if all expected actions exist
      foreach (var def in inputs)
      {
         if (def == null || string.IsNullOrWhiteSpace(def.Id))
         {
            continue;
         }

         if (def.Type == BindingType.Button)
         {
            if (actionMap.FindAction(def.Id) == null)
            {
               return true;
            }
         }
         else
         {
            // Composite2D has multiple sub-actions
            if (actionMap.FindAction($"{def.Id}__kb_keys") == null
                && actionMap.FindAction($"{def.Id}__gp_buttons") == null)
            {
               return true;
            }
         }
      }

      return false;
   }

   private static InputActionAsset BuildInputActionAsset(InputConfig config)
   {
      var asset = ScriptableObject.CreateInstance<InputActionAsset>();
      var map = asset.AddActionMap(ACTION_MAP_NAME);

      var inputs = config.Inputs;
      if (inputs == null)
      {
         return asset;
      }

      foreach (var def in inputs)
      {
         if (def == null || string.IsNullOrWhiteSpace(def.Id))
         {
            continue;
         }

         if (def.Type == BindingType.Button)
         {
            CreateButtonAction(map, def);
         }
         else
         {
            CreateComposite2DActions(map, def);
         }
      }

      return asset;
   }

   private static void CreateButtonAction(InputActionMap map, IInputDefinition def)
   {
      var action = map.AddAction(def.Id, InputActionType.Button);
      var index = 0;

      if (!string.IsNullOrEmpty(def.Keyboard?.Primary))
      {
         action.AddBinding(def.Keyboard.Primary);
         SetBindingId(action, index++, $"{def.Id}__button__keyboard");
      }

      if (!string.IsNullOrEmpty(def.Gamepad?.Primary))
      {
         action.AddBinding(def.Gamepad.Primary);
         SetBindingId(action, index, $"{def.Id}__button__gamepad");
      }
   }

   private static void CreateComposite2DActions(InputActionMap map, IInputDefinition def)
   {
      // Keyboard Keys Action (2D Vector)
      if (def.Keyboard != null && !string.IsNullOrEmpty(def.Keyboard.Up))
      {
         var kbKeysName = $"{def.Id}__kb_keys";
         var kbKeysAction = map.AddAction(kbKeysName, InputActionType.Value);

         kbKeysAction.AddCompositeBinding("2DVector")
            .With("up", def.Keyboard.Up)
            .With("down", def.Keyboard.Down)
            .With("left", def.Keyboard.Left)
            .With("right", def.Keyboard.Right);

         var idx = 0;
         SetBindingId(kbKeysAction, idx++, $"{kbKeysName}__composite");
         SetBindingId(kbKeysAction, idx++, $"{kbKeysName}__up");
         SetBindingId(kbKeysAction, idx++, $"{kbKeysName}__down");
         SetBindingId(kbKeysAction, idx++, $"{kbKeysName}__left");
         SetBindingId(kbKeysAction, idx, $"{kbKeysName}__right");
      }

      // Keyboard Mouse Action (analog)
      if (def.AllowAnalogKeyboard)
      {
         var kbMouseName = $"{def.Id}__kb_mouse";
         var kbMouseAction = map.AddAction(kbMouseName, InputActionType.Value);

         kbMouseAction.AddBinding("<Mouse>/delta");
         SetBindingId(kbMouseAction, 0, $"{kbMouseName}__mouse_delta");
      }

      // Gamepad Buttons Action (2D Vector)
      if (def.Gamepad != null && !string.IsNullOrEmpty(def.Gamepad.Up))
      {
         var gpButtonsName = $"{def.Id}__gp_buttons";
         var gpButtonsAction = map.AddAction(gpButtonsName, InputActionType.Value);

         gpButtonsAction.AddCompositeBinding("2DVector")
            .With("up", def.Gamepad.Up)
            .With("down", def.Gamepad.Down)
            .With("left", def.Gamepad.Left)
            .With("right", def.Gamepad.Right);

         var idx = 0;
         SetBindingId(gpButtonsAction, idx++, $"{gpButtonsName}__composite");
         SetBindingId(gpButtonsAction, idx++, $"{gpButtonsName}__up");
         SetBindingId(gpButtonsAction, idx++, $"{gpButtonsName}__down");
         SetBindingId(gpButtonsAction, idx++, $"{gpButtonsName}__left");
         SetBindingId(gpButtonsAction, idx, $"{gpButtonsName}__right");
      }

      // Gamepad Stick Action (analog)
      if (def.AllowAnalogGamepad && !string.IsNullOrEmpty(def.Gamepad?.Primary))
      {
         var gpStickName = $"{def.Id}__gp_stick";
         var gpStickAction = map.AddAction(gpStickName);

         gpStickAction.AddBinding(def.Gamepad.Primary);
         SetBindingId(gpStickAction, 0, $"{gpStickName}__stick");
      }
   }

   private static Guid GenerateStableGuid(string key)
   {
      using var md5 = MD5.Create();
      return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));
   }

   private static void SetBindingId(InputAction action, int index, string stableKey)
   {
      if (action == null || index < 0 || index >= action.bindings.Count)
      {
         return;
      }

      var guid = GenerateStableGuid(stableKey);
      var b = action.bindings[index];

      action.ChangeBinding(index).To(
         new InputBinding
         {
            id = guid,
            path = b.path,
            interactions = b.interactions,
            processors = b.processors,
            groups = b.groups,
            action = b.action,
            isComposite = b.isComposite,
            isPartOfComposite = b.isPartOfComposite,
            name = b.name
         });
   }
}
