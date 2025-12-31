namespace P3k.InputController.Implementations.Services
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;
   using P3k.InputController.Abstractions.Interfaces.Core;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine.InputSystem;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   internal sealed class InputBindingQueryService
   {
      private readonly Func<string, IInputDefinition> _getDefinition;

      private readonly IReadOnlyDictionary<string, IInputState> _inputs;

      private readonly InputProfileService _profiles;

      internal InputBindingQueryService(
         InputProfileService profiles,
         IReadOnlyDictionary<string, IInputState> inputs,
         Func<string, IInputDefinition> getDefinition)
      {
         _profiles = profiles;
         _inputs = inputs;
         _getDefinition = getDefinition;
      }

      internal bool CanRebind(string id, InputDevice device, string bindingName = null)
      {
         var def = _getDefinition?.Invoke(id);
         if (def == null)
         {
            return false;
         }

         if (def.Type != BindingType.Composite2D)
         {
            return true;
         }

         // Keyboard: analog mouse active ? cannot rebind composite keys
         if (device == InputDevice.Keyboard && def.AllowAnalogKeyboard && _profiles.IsAnalogKeyboard(id))
         {
            return false;
         }

         // Gamepad: stick active ? allow only full-stick rebind (no parts)
         if (device == InputDevice.Gamepad && def.AllowAnalogGamepad && _profiles.IsAnalogGamepad(id)
             && bindingName != null)
         {
            return false;
         }

         return true;
      }

      internal string GetBindingDisplay(string id, InputDevice device, string bindingName)
      {
         if (!_inputs.TryGetValue(id, out var state) || state == null)
         {
            return null;
         }

         var def = _getDefinition?.Invoke(id);
         if (def == null)
         {
            return null;
         }

         // Button
         if (def.Type == BindingType.Button)
         {
            return GetFirstMatchingDisplay(state.ButtonAction, device, bindingName);
         }

         // Composite2D � Keyboard
         if (device == InputDevice.Keyboard)
         {
            if (def.AllowAnalogKeyboard && _profiles.IsAnalogKeyboard(id))
            {
               return "Mouse";
            }

            return GetFirstMatchingDisplay(state.KeyboardKeysAction, device, bindingName);
         }

         // Composite2D � Gamepad
         if (def.AllowAnalogGamepad && _profiles.IsAnalogGamepad(id))
         {
            return GetFirstMatchingDisplay(state.GamepadStickAction, device, bindingName);
         }

         return GetFirstMatchingDisplay(state.GamepadButtonsAction, device, bindingName);
      }

      private static string GetFirstMatchingDisplay(InputAction action, InputDevice device, string bindingName)
      {
         if (action == null)
         {
            return null;
         }

         var devicePath = device == InputDevice.Keyboard ? "<Keyboard>" : "<Gamepad>";

         for (var i = 0; i < action.bindings.Count; i++)
         {
            var binding = action.bindings[i];

            if (binding.isComposite)
            {
               continue;
            }

            if (bindingName != null && !string.Equals(binding.name, bindingName, StringComparison.OrdinalIgnoreCase))
            {
               continue;
            }

            if (!string.IsNullOrEmpty(binding.effectivePath)
                && !binding.effectivePath.StartsWith(devicePath, StringComparison.Ordinal))
            {
               continue;
            }

            return action.GetBindingDisplayString(i);
         }

         return null;
      }
   }
}
