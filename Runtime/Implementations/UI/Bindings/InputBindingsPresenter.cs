namespace P3k.InputController.Implementations.UI.Bindings
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Configurations;
   using P3k.InputController.Abstractions.Interfaces.Core;

   using System;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   internal sealed class InputBindingsPresenter
   {
      private const float REBIND_TIMEOUT_SECONDS = 5f;

      private readonly IInputController _controller;

      private bool _isRebinding;

      private float _rebindEndTime;

      private bool _rebuildRequested;

      private readonly InputBindingsState _state;

      private bool _suppressRebuildRequests;

      private readonly InputBindingsView _view;

      internal event Action OnRebindFailed;

      internal event Action OnRebindStarted;

      internal event Action OnRebindSuccess;

      internal event Action<float> OnRebindTimerUpdated;

      internal event Action OnRebuildRequested;

      internal InputBindingsPresenter(IInputController controller, InputBindingsView view, InputBindingsState state)
      {
         _controller = controller;
         _view = view;
         _state = state;
      }

      internal void Rebuild()
      {
         _suppressRebuildRequests = true;

         try
         {
            _view.Clear();

            var inputs = _controller.Inputs;
            if (inputs == null)
            {
               return;
            }

            foreach (var def in inputs)
            {
               if (def == null)
               {
                  continue;
               }

               var section = InputBindingsView.CreateSection(def.Id);

               switch (def.Type)
               {
                  case BindingType.Button:
                     BuildButtonSection(section, def.Id);
                     break;

                  case BindingType.Composite2D:
                     BuildComposite2DSection(section, def);
                     break;
               }

               _view.BindingsContainer.Add(section);
            }
         }
         finally
         {
            _suppressRebuildRequests = false;
         }
      }

      internal void SetDevice(InputDevice device)
      {
         _state.CurrentDevice = device;
         Rebuild();
      }

      internal void Tick()
      {
         if (_rebuildRequested)
         {
            _rebuildRequested = false;

            if (!_isRebinding)
            {
               Rebuild();
            }
         }

         if (!_isRebinding)
         {
            return;
         }

         var remaining = _rebindEndTime - Time.unscaledTime;

         if (remaining <= 0f)
         {
            _isRebinding = false;
            _controller.RebindCancel();
            OnRebindFailed?.Invoke();
            return;
         }

         OnRebindTimerUpdated?.Invoke(remaining);
      }

      private void AddCompositePartRow(VisualElement section, string id, string label, string part)
      {
         var key = $"{id}_{part}";
         var binding = _controller.GetBindingDisplay(id, _state.CurrentDevice, part);

         var row = _view.CreateBindingRow(key, label, binding ?? "-", () => BeginRebind(id, part), false);

         section.Add(row);
      }

      private void BeginRebind(string id, string part)
      {
         var key = part == null ? id : $"{id}_{part}";

         if (!_view.BindingButtons.TryGetValue(key, out var btn))
         {
            return;
         }

         btn.text = "...";

         _isRebinding = true;
         _rebindEndTime = Time.unscaledTime + REBIND_TIMEOUT_SECONDS;

         OnRebindStarted?.Invoke();

         _controller.Rebind(
         id,
         _state.CurrentDevice,
         part,
         () =>
            {
               _isRebinding = false;
               btn.text = _controller.GetBindingDisplay(id, _state.CurrentDevice, part) ?? "-";
               OnRebindSuccess?.Invoke();
            },
         () =>
            {
               _isRebinding = false;
               btn.text = _controller.GetBindingDisplay(id, _state.CurrentDevice, part) ?? "-";
               OnRebindFailed?.Invoke();
            });
      }

      private void Build4WayComposite(VisualElement section, string id)
      {
         AddCompositePartRow(section, id, "Up", "up");
         AddCompositePartRow(section, id, "Down", "down");
         AddCompositePartRow(section, id, "Left", "left");
         AddCompositePartRow(section, id, "Right", "right");
      }

      private void BuildButtonSection(VisualElement section, string id)
      {
         var binding = _controller.GetBindingDisplay(id, _state.CurrentDevice);

         var row = _view.CreateBindingRow(id, "Key", binding ?? "-", () => BeginRebind(id, null), false);

         section.Add(row);
      }

      private void BuildComposite2DSection(VisualElement section, IInputDefinition def)
      {
         var id = def.Id;

         if (_state.CurrentDevice == InputDevice.Keyboard)
         {
            // Keyboard: toggle for mouse vs keys
            if (def.AllowAnalogKeyboard)
            {
               var isUsingMouse = _controller.IsAnalogKeyboard(id);

               var toggleRow = InputBindingsView.CreateToggleRow(
               "Use Mouse",
               isUsingMouse,
               evt =>
                  {
                     _controller.IsAnalogKeyboard(id, evt.newValue);
                     _state.MarkBindingsDirty(_state.ProfileDirty);
                     RequestRebuild();
                  });

               section.Add(toggleRow);

               if (isUsingMouse)
               {
                  var row = _view.CreateBindingRow($"{id}_mouse", "Input", "Mouse", null, true);
                  section.Add(row);
               }
               else
               {
                  Build4WayComposite(section, id);
               }
            }
            else
            {
               Build4WayComposite(section, id);
            }
         }
         else
         {
            // Gamepad: toggle for stick vs buttons
            if (def.AllowAnalogGamepad)
            {
               var isUsingStick = _controller.IsAnalogGamepad(id);

               var toggleRow = InputBindingsView.CreateToggleRow(
               "Use Stick",
               isUsingStick,
               evt =>
                  {
                     _controller.IsAnalogGamepad(id, evt.newValue);
                     _state.MarkBindingsDirty(_state.ProfileDirty);
                     RequestRebuild();
                  });

               section.Add(toggleRow);

               if (isUsingStick)
               {
                  var binding = _controller.GetBindingDisplay(id, _state.CurrentDevice);
                  var row = _view.CreateBindingRow(
                  id,
                  "Stick",
                  binding ?? "Left Stick",
                  () => BeginRebind(id, null),
                  false);
                  section.Add(row);
               }
               else
               {
                  Build4WayComposite(section, id);
               }
            }
            else
            {
               Build4WayComposite(section, id);
            }
         }

         // Invert Y toggle
         var invertRow = InputBindingsView.CreateToggleRow(
         "Invert Y",
         _controller.IsInvertedY(id),
         evt =>
            {
               _controller.IsInvertedY(id, evt.newValue);
               _state.MarkBindingsDirty(_state.ProfileDirty);
            });

         section.Add(invertRow);
      }

      private void RequestRebuild()
      {
         if (_suppressRebuildRequests)
         {
            return;
         }

         _rebuildRequested = true;
         OnRebuildRequested?.Invoke();
      }
   }
}