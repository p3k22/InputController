namespace P3k.InputController.Implementations.Utilities
{
   using P3k.InputController.Abstractions.Interfaces.Configurations;

   using System;
   using System.Linq;
   using System.Security.Cryptography;
   using System.Text;

   using UnityEngine.InputSystem;

   internal static class InputActionUtils
   {
      internal static InputAction CreateButtonAction(InputActionMap map, IInputDefinition def)
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
            SetBindingId(action, index++, $"{def.Id}__button__gamepad");
         }

         return action;
      }

      internal static InputAction CreateGamepadButtonsAction(InputActionMap map, IInputDefinition def)
      {
         if (def.Gamepad == null || string.IsNullOrEmpty(def.Gamepad.Up))
         {
            return null;
         }

         var name = $"{def.Id}__gp_buttons";
         var action = map.AddAction(name, InputActionType.Value);

         var index = 0;

         action.AddCompositeBinding("2DVector").With("up", def.Gamepad.Up).With("down", def.Gamepad.Down)
            .With("left", def.Gamepad.Left).With("right", def.Gamepad.Right);

         SetBindingId(action, index++, $"{name}__composite");
         SetBindingId(action, index++, $"{name}__up");
         SetBindingId(action, index++, $"{name}__down");
         SetBindingId(action, index++, $"{name}__left");
         SetBindingId(action, index++, $"{name}__right");

         return action;
      }

      internal static InputAction CreateGamepadStickAction(InputActionMap map, IInputDefinition def)
      {
         if (!def.AllowAnalogGamepad || string.IsNullOrEmpty(def.Gamepad?.Primary))
         {
            return null;
         }

         var name = $"{def.Id}__gp_stick";
         var action = map.AddAction(name);

         action.AddBinding(def.Gamepad.Primary);
         SetBindingId(action, 0, $"{name}__stick");

         return action;
      }

      internal static InputAction CreateKeyboardKeysAction(InputActionMap map, IInputDefinition def)
      {
         if (def.Keyboard == null || string.IsNullOrEmpty(def.Keyboard.Up))
         {
            return null;
         }

         var name = $"{def.Id}__kb_keys";
         var action = map.AddAction(name, InputActionType.Value);

         var index = 0;

         action.AddCompositeBinding("2DVector").With("up", def.Keyboard.Up).With("down", def.Keyboard.Down)
            .With("left", def.Keyboard.Left).With("right", def.Keyboard.Right);

         SetBindingId(action, index++, $"{name}__composite");
         SetBindingId(action, index++, $"{name}__up");
         SetBindingId(action, index++, $"{name}__down");
         SetBindingId(action, index++, $"{name}__left");
         SetBindingId(action, index++, $"{name}__right");

         return action;
      }

      internal static InputAction CreateKeyboardMouseAction(InputActionMap map, IInputDefinition def)
      {
         if (!def.AllowAnalogKeyboard)
         {
            return null;
         }

         var name = $"{def.Id}__kb_mouse";
         var action = map.AddAction(name, InputActionType.Value);

         action.AddBinding("<Mouse>/delta");
         SetBindingId(action, 0, $"{name}__mouse_delta");

         return action;
      }

      private static Guid GenerateStableGuid(string key)
      {
         using var md5 = MD5.Create();
         return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));
      }

      private static void SetBindingId(InputAction action, int index, string stableKey)
      {
         if (action == null)
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
}