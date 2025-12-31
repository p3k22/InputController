namespace P3k.InputController.Abstractions.Interfaces.State
{
   using System;
   using System.Linq;

   public interface IInputStateEvents
   {
      event Action OnHeld;

      event Action OnPressed;

      event Action OnReleased;

      event Action OnRepeated;
   }
}
