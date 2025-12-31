namespace P3k.InputController.Abstractions.Interfaces.State
{
   using System.Linq;

   public interface IInputStateOptions
   {
      bool InvertY { get; set; }

      void SetModes(bool useAnalogKeyboard, bool useAnalogGamepad);

      void SetRepeat(float startDelaySeconds, float intervalSeconds);
   }
}
