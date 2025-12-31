namespace P3k.InputController.Implementations.DataContainers
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;

   using System;
   using System.Linq;

   using UnityEngine;

   [Serializable]
   public class InputDefinition : IInputDefinition
   {
      public bool AllowAnalogGamepad => _allowAnalogGamepad;

      public bool AllowAnalogKeyboard => _allowAnalogKeyboard;

      public bool DefaultUseMouse => _defaultUseMouse;

      public IInputBindings Gamepad => _gamepad;

      public string Id => _id;

      public IInputBindings Keyboard => _keyboard;

      public BindingType Type => _type;

   #region Serialised Fields

      [SerializeField]
      private string _id;

      [SerializeField]
      private BindingType _type;

      [Tooltip("For Composite2D - allow toggling to analog input (Stick) at runtime")]
      [SerializeField]
      private bool _allowAnalogGamepad;

      [Tooltip("For Composite2D - allow toggling to analog input (Mouse) at runtime")]
      [SerializeField]
      private bool _allowAnalogKeyboard;

      [Tooltip(
      "For Composite2D - when no saved profile exists, start in mouse mode (only if AllowAnalogKeyboard is enabled)")]
      [SerializeField]
      private bool _defaultUseMouse;

      [SerializeField]
      private InputBindings _gamepad;

      [SerializeField]
      private InputBindings _keyboard;

   #endregion
   }
}