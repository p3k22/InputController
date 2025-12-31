namespace P3k.InputController.Abstractions.Interfaces.State
{
   using System.Linq;

   using UnityEngine.InputSystem;

   public interface IInputStateActions
   {
      InputAction ButtonAction { get; }

      InputAction KeyboardKeysAction { get; }

      InputAction KeyboardMouseAction { get; }

      InputAction GamepadButtonsAction { get; }

      InputAction GamepadStickAction { get; }
   }
}
