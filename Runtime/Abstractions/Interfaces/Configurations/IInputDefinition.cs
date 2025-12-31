namespace P3k.InputController.Abstractions.Interfaces.Configurations
{
   using P3k.InputController.Abstractions.Enums;

   using System.Linq;

   public interface IInputDefinition
   {
      bool AllowAnalogGamepad { get; }

      bool AllowAnalogKeyboard { get; }

      bool DefaultUseMouse { get; }

      IInputBindings Gamepad { get; }

      string Id { get; }

      IInputBindings Keyboard { get; }

      BindingType Type { get; }
   }
}