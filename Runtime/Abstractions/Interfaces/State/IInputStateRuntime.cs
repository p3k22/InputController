namespace P3k.InputController.Abstractions.Interfaces.State
{
   using System.Linq;

   public interface IInputStateRuntime
   {
      void Enable();

      void Disable();

      void Tick();
   }
}
