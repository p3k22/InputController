namespace P3k.InputController.Abstractions.Interfaces.Core
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   public interface IInputStateProvider
   {
      IReadOnlyList<IInputDefinition> Inputs { get; }

      IInputState Get(string id);

      IInputState Get<TEnum>(TEnum id)
         where TEnum : Enum;

      string GetBindingDisplay(string id, InputDevice device, string bindingName = null);
   }
}
