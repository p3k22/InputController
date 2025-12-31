namespace P3k.InputController.Abstractions.Interfaces.State
{
   using P3k.InputController.Abstractions.Enums;

   using System.Linq;

   using UnityEngine;

   public interface IInputValueState
   {
      BindingType Type { get; }

      bool IsHeld { get; }

      bool IsPressed { get; }

      bool IsReleased { get; }

      bool IsRepeated { get; }

      float Value1D { get; }

      Vector2 Value2D { get; }

      Vector3 Value3D { get; }
   }
}
