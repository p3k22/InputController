namespace P3k.InputController.Abstractions.Interfaces.Configurations
{
   using System.Collections.Generic;
   using System.Linq;

   public interface IInputConfig
   {
      List<IInputDefinition> Inputs { get; }
   }
}
