namespace P3k.InputController.Implementations.DataContainers
{
   using P3k.InputController.Abstractions.Interfaces.Configurations;

   using System;
   using System.Linq;

   using UnityEngine;

   [Serializable]
   public class InputBindings : IInputBindings
   {
      public string Down => _down;

      public string Left => _left;

      public string Primary => _primary;

      public string Right => _right;

      public string Up => _up;

   #region Serialised Fields

      [Tooltip("For Button type, or analog Composite2D (mouse/stick)")]
      [SerializeField]
      [Header("Button")]
      private string _primary;

      [Tooltip("For digital Composite2D")]
      [SerializeField]
      [Header("Composite2D")]
      private string _up;

      [SerializeField]
      private string _down;

      [SerializeField]
      private string _left;

      [SerializeField]
      private string _right;

   #endregion
   }
}