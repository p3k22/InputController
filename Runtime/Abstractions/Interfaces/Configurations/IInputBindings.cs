namespace P3k.InputController.Abstractions.Interfaces.Configurations
{
   using System.Linq;

   public interface IInputBindings
   {
      string Down { get; }

      string Left { get; }

      string Primary { get; }

      string Right { get; }

      string Up { get; }
   }
}
