namespace P3k.InputController.Implementations.Services
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;
   using P3k.InputController.Abstractions.Interfaces.Core;
   using P3k.InputController.Implementations.Utilities;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine.InputSystem;

   internal sealed class InputProfileService
   {
      private readonly HashSet<string> _analogGamepads = new();

      private readonly HashSet<string> _analogKeyboards = new();

      private readonly InputActionAsset _asset;

      private readonly Func<string, IInputDefinition> _getDefinition;

      private readonly IReadOnlyDictionary<string, IInputState> _inputs;

      private readonly HashSet<string> _invertedYInputs = new();

      internal InputProfileService(
         InputActionAsset asset,
         IReadOnlyDictionary<string, IInputState> inputs,
         Func<string, IInputDefinition> getDefinition)
      {
         _asset = asset;
         _inputs = inputs;
         _getDefinition = getDefinition;
      }

      internal bool IsAnalogGamepad(string id)
      {
         return _analogGamepads.Contains(id);
      }

      internal bool IsAnalogKeyboard(string id)
      {
         return _analogKeyboards.Contains(id);
      }

      internal bool IsInvertedY(string id)
      {
         return _invertedYInputs.Contains(id);
      }

      internal void Load(string profileName)
      {
         if (_asset != null)
         {
            foreach (var map in _asset.actionMaps)
            {
               map.RemoveAllBindingOverrides();
            }
         }

         _analogKeyboards.Clear();
         _analogGamepads.Clear();
         _invertedYInputs.Clear();

         var loaded = InputProfileUtils.Load(_asset, profileName, out var analogKb, out var analogGp, out var inverted);

         if (loaded)
         {
            foreach (var id in analogKb)
            {
               _analogKeyboards.Add(id);
            }

            foreach (var id in analogGp)
            {
               _analogGamepads.Add(id);
            }

            foreach (var id in inverted)
            {
               _invertedYInputs.Add(id);
            }
         }
         else
         {
            SeedDefaultAnalogKeyboard();
         }

         ApplyAll();
      }

      internal void ResetToDefaults(string profileName = "")
      {
         if (_asset != null)
         {
            foreach (var map in _asset.actionMaps)
            {
               map.RemoveAllBindingOverrides();
            }
         }

         _analogKeyboards.Clear();
         _analogGamepads.Clear();
         _invertedYInputs.Clear();

         SeedDefaultAnalogKeyboard();

         ApplyAll();

         if (InputProfileUtils.Exists(profileName))
         {
            Save(profileName);
         }
      }

      internal void Save(string profileName)
      {
         InputProfileUtils.Save(_asset, profileName, _analogKeyboards, _analogGamepads, _invertedYInputs);
      }

      internal void SetAnalogGamepad(string id, bool enabled)
      {
         if (enabled)
         {
            _analogGamepads.Add(id);
         }
         else
         {
            _analogGamepads.Remove(id);
         }

         Apply(id);
      }

      internal void SetAnalogKeyboard(string id, bool enabled)
      {
         if (enabled)
         {
            _analogKeyboards.Add(id);
         }
         else
         {
            _analogKeyboards.Remove(id);
         }

         Apply(id);
      }

      internal void SetInvertedY(string id, bool inverted)
      {
         if (inverted)
         {
            _invertedYInputs.Add(id);
         }
         else
         {
            _invertedYInputs.Remove(id);
         }

         if (_inputs.TryGetValue(id, out var state) && state != null)
         {
            state.InvertY = inverted;
         }
      }

      internal bool WouldSaveChange(string profileName)
      {
         return InputProfileUtils.WouldSaveChange(
         _asset,
         profileName,
         _analogKeyboards,
         _analogGamepads,
         _invertedYInputs);
      }

      private void Apply(string id)
      {
         if (!_inputs.TryGetValue(id, out var state) || state == null)
         {
            return;
         }

         if (state.Type != BindingType.Composite2D)
         {
            // Button types only care about invert flags (handled elsewhere if needed)
            return;
         }

         var def = _getDefinition?.Invoke(id);
         if (def == null)
         {
            return;
         }

         var useAnalogKb = def.AllowAnalogKeyboard && _analogKeyboards.Contains(id);
         var useAnalogGp = def.AllowAnalogGamepad && _analogGamepads.Contains(id);

         state.SetModes(useAnalogKb, useAnalogGp);
         state.InvertY = _invertedYInputs.Contains(id);
      }

      private void ApplyAll()
      {
         foreach (var id in _inputs.Keys.ToList())
         {
            Apply(id);
         }
      }

      private void SeedDefaultAnalogKeyboard()
      {
         foreach (var id in _inputs.Keys)
         {
            var def = _getDefinition?.Invoke(id);
            if (def == null)
            {
               continue;
            }

            if (def.Type != BindingType.Composite2D)
            {
               continue;
            }

            if (!def.AllowAnalogKeyboard)
            {
               continue;
            }

            if (def.DefaultUseMouse)
            {
               _analogKeyboards.Add(id);
            }
         }
      }
   }
}