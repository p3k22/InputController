namespace P3k.InputController.Abstractions.Interfaces.State
{
   using System.Linq;

   using UnityEngine.InputSystem;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   public interface IInputStateRebinding
   {
      InputAction GetRebindAction(InputDevice device, string bindingName, bool useAnalogKb, bool useAnalogGp);
   }
}
