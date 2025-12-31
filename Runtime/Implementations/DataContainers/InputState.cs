namespace P3k.InputController.Implementations.DataContainers
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Core;

   using System;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   public sealed class InputState : IInputState
   {
      public event Action OnPressed;

      public event Action OnReleased;

      public event Action OnRepeated;

      public event Action OnHeld;

      public BindingType Type { get; }

      public float Value1D { get; private set; }

      public Vector2 Value2D { get; private set; }

      public Vector3 Value3D { get; private set; }

      public bool InvertY { get; set; }

      public bool IsHeld { get; private set; }

      public bool IsPressed { get; private set; }

      public bool IsRepeated { get; private set; }

      public bool IsReleased { get; private set; }

      public InputAction ButtonAction { get; }

      public InputAction KeyboardKeysAction { get; }

      public InputAction KeyboardMouseAction { get; }

      public InputAction GamepadButtonsAction { get; }

      public InputAction GamepadStickAction { get; }

      private bool _useAnalogGamepad;

      private bool _useAnalogKeyboard;

      private float _repeatNextAt;

      private float _repeatStartDelay;

      private float _repeatInterval;

      public InputState(
         BindingType type,
         InputAction buttonAction,
         InputAction keyboardKeysAction,
         InputAction keyboardMouseAction,
         InputAction gamepadButtonsAction,
         InputAction gamepadStickAction)
      {
         Type = type;

         ButtonAction = buttonAction;

         KeyboardKeysAction = keyboardKeysAction;
         KeyboardMouseAction = keyboardMouseAction;

         GamepadButtonsAction = gamepadButtonsAction;
         GamepadStickAction = gamepadStickAction;
      }

      public void SetRepeat(float startDelaySeconds, float intervalSeconds)
      {
         _repeatStartDelay = Mathf.Max(0f, startDelaySeconds);
         _repeatInterval = Mathf.Max(0f, intervalSeconds);
      }

      public void SetModes(bool useAnalogKeyboard, bool useAnalogGamepad)
      {
         _useAnalogKeyboard = useAnalogKeyboard;
         _useAnalogGamepad = useAnalogGamepad;

         ApplyEnabledSet();
      }

      public void Enable()
      {
         if (Type == BindingType.Button)
         {
            ButtonAction?.Enable();
            return;
         }

         KeyboardKeysAction?.Enable();
         KeyboardMouseAction?.Enable();
         GamepadButtonsAction?.Enable();
         GamepadStickAction?.Enable();

         ApplyEnabledSet();
      }

      public void Disable()
      {
         ButtonAction?.Disable();

         KeyboardKeysAction?.Disable();
         KeyboardMouseAction?.Disable();

         GamepadButtonsAction?.Disable();
         GamepadStickAction?.Disable();
      }

      public void Tick()
      {
         if (Type == BindingType.Composite2D)
         {
            SyncEnabledSet();

            var kb = ReadKeyboardVector();
            var gp = ReadGamepadVector();

            Value2D = kb.sqrMagnitude >= gp.sqrMagnitude ? kb : gp;

            if (InvertY)
            {
               Value2D = new Vector2(Value2D.x, -Value2D.y);
            }

            return;
         }

         if (Type == BindingType.Button)
         {
            var pressed = ButtonAction != null && ButtonAction.IsPressed();

            IsPressed = pressed && !IsHeld;
            IsReleased = !pressed && IsHeld;

            IsHeld = pressed;

            IsRepeated = false;
            Value1D = 0;

            if (IsPressed)
            {
               OnPressed?.Invoke();

               if (_repeatInterval > 0f)
               {
                  _repeatNextAt = Time.unscaledTime + _repeatStartDelay;
               }
            }

            if (IsHeld)
            {
               Value1D = 1;
               OnHeld?.Invoke();

               if (_repeatInterval > 0f && Time.unscaledTime >= _repeatNextAt)
               {
                  IsRepeated = true;
                  _repeatNextAt = Time.unscaledTime + _repeatInterval;
                  OnRepeated?.Invoke();
               }
            }

            if (IsReleased)
            {
               OnReleased?.Invoke();
            }
         }
      }

      public InputAction GetRebindAction(InputDevice device, string bindingName, bool useAnalogKb, bool useAnalogGp)
      {
         if (Type == BindingType.Button)
         {
            return ButtonAction;
         }

         if (device == InputDevice.Keyboard)
         {
            // Analog mouse mode has no rebindable parts from the UI.
            if (useAnalogKb)
            {
               return null;
            }

            return KeyboardKeysAction;
         }

         // Gamepad: stick mode rebinds the single stick binding (bindingName == null).
         if (useAnalogGp)
         {
            return bindingName == null ? GamepadStickAction : null;
         }

         return GamepadButtonsAction;
      }

      private Vector2 ReadKeyboardVector()
      {
         if (_useAnalogKeyboard)
         {
            return KeyboardMouseAction != null ? KeyboardMouseAction.ReadValue<Vector2>() : Vector2.zero;
         }

         return KeyboardKeysAction != null ? KeyboardKeysAction.ReadValue<Vector2>() : Vector2.zero;
      }

      private Vector2 ReadGamepadVector()
      {
         if (_useAnalogGamepad)
         {
            return GamepadStickAction != null ? GamepadStickAction.ReadValue<Vector2>() : Vector2.zero;
         }

         return GamepadButtonsAction != null ? GamepadButtonsAction.ReadValue<Vector2>() : Vector2.zero;
      }

      private void SyncEnabledSet()
      {
         if (Type != BindingType.Composite2D)
         {
            return;
         }

         // Keyboard: ensure only one is enabled when both exist
         if (_useAnalogKeyboard)
         {
            if (KeyboardKeysAction != null && KeyboardKeysAction.enabled)
            {
               KeyboardKeysAction.Disable();
            }

            if (KeyboardMouseAction != null && !KeyboardMouseAction.enabled)
            {
               KeyboardMouseAction.Enable();
            }
         }
         else
         {
            if (KeyboardMouseAction != null && KeyboardMouseAction.enabled)
            {
               KeyboardMouseAction.Disable();
            }

            if (KeyboardKeysAction != null && !KeyboardKeysAction.enabled)
            {
               KeyboardKeysAction.Enable();
            }
         }

         // Gamepad: ensure only one is enabled when both exist
         if (_useAnalogGamepad)
         {
            if (GamepadButtonsAction != null && GamepadButtonsAction.enabled)
            {
               GamepadButtonsAction.Disable();
            }

            if (GamepadStickAction != null && !GamepadStickAction.enabled)
            {
               GamepadStickAction.Enable();
            }
         }
         else
         {
            if (GamepadStickAction != null && GamepadStickAction.enabled)
            {
               GamepadStickAction.Disable();
            }

            if (GamepadButtonsAction != null && !GamepadButtonsAction.enabled)
            {
               GamepadButtonsAction.Enable();
            }
         }
      }

      private void ApplyEnabledSet()
      {
         if (Type != BindingType.Composite2D)
         {
            return;
         }

         if (_useAnalogKeyboard)
         {
            KeyboardKeysAction?.Disable();
            KeyboardMouseAction?.Enable();
         }
         else
         {
            KeyboardMouseAction?.Disable();
            KeyboardKeysAction?.Enable();
         }

         if (_useAnalogGamepad)
         {
            GamepadButtonsAction?.Disable();
            GamepadStickAction?.Enable();
         }
         else
         {
            GamepadStickAction?.Disable();
            GamepadButtonsAction?.Enable();
         }
      }
   }
}
