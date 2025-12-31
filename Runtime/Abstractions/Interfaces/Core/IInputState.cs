namespace P3k.InputController.Abstractions.Interfaces.Core
{
   using P3k.InputController.Abstractions.Interfaces.State;

   using System.Linq;

   public interface IInputState : IInputStateOptions,
                                  IInputStateRuntime,
                                  IInputStateRebinding,
                                  IInputStateEvents,
                                  IInputValueState,
                                  IInputStateActions
   {
   }
}
