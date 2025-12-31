namespace P3k.InputController.Abstractions.Interfaces.Services
{
   using P3k.InputController.Abstractions.Enums;

   using System;
   using System.Linq;

   public interface IInputRebindingService
   {
      void Rebind(
         string id,
         InputDevice device,
         string bindingName = null,
         Action onComplete = null,
         Action onCancel = null);

      void RebindCancel();
   }
}
