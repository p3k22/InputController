namespace P3k.InputController.Implementations.UI.Controls
{
   using P3k.InputController.Implementations.UI.Bindings;

   using System;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   internal sealed class InputControllerDeviceSelection : IDisposable
   {
      private readonly Button _btnGamepad;

      private readonly Button _btnKeyboard;

      private readonly InputBindingsPresenter _presenter;

      internal InputControllerDeviceSelection(InputControllerUIControls controls, InputBindingsPresenter presenter)
      {
         _btnKeyboard = controls.BtnKeyboard;
         _btnGamepad = controls.BtnGamepad;
         _presenter = presenter;
      }

      public void Dispose()
      {
         _btnKeyboard.clicked -= OnKeyboardClicked;
         _btnGamepad.clicked -= OnGamepadClicked;
      }

      internal void Initialize()
      {
         _btnKeyboard.clicked += OnKeyboardClicked;
         _btnGamepad.clicked += OnGamepadClicked;
      }

      private void OnGamepadClicked()
      {
         _presenter.SetDevice(InputDevice.Gamepad);
         SetDeviceVisual(InputDevice.Gamepad);
      }

      private void OnKeyboardClicked()
      {
         _presenter.SetDevice(InputDevice.Keyboard);
         SetDeviceVisual(InputDevice.Keyboard);
      }

      private void SetDeviceVisual(InputDevice device)
      {
         if (device == InputDevice.Keyboard)
         {
            _btnKeyboard.style.backgroundColor = new Color(0.24f, 0.4f, 0.63f);
            _btnKeyboard.style.color = Color.white;
            _btnGamepad.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f);
            _btnGamepad.style.color = new Color(0.7f, 0.7f, 0.7f);
         }
         else
         {
            _btnGamepad.style.backgroundColor = new Color(0.24f, 0.4f, 0.63f);
            _btnGamepad.style.color = Color.white;
            _btnKeyboard.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f);
            _btnKeyboard.style.color = new Color(0.7f, 0.7f, 0.7f);
         }
      }
   }
}
