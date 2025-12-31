namespace P3k.InputController.Abstractions.Interfaces.Services
{
   using System.Linq;

   public interface IInputFlagsService
   {
      bool IsAnalogGamepad(string id, bool? value = null);

      bool IsAnalogKeyboard(string id, bool? value = null);

      bool IsInvertedY(string id, bool? value = null);
   }
}
