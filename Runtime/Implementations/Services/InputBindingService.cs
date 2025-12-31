namespace P3k.InputController.Implementations.Services
{
   using System;
   using System.Linq;

   using UnityEngine.InputSystem;

   using InputDevice = P3k.InputController.Abstractions.Enums.InputDevice;

   internal sealed class InputBindingService : IDisposable
   {
      private readonly InputBindingQueryService _query;

      private InputActionRebindingExtensions.RebindingOperation _rebindingOp;

      internal InputBindingService(InputBindingQueryService queryService)
      {
         _query = queryService;
      }

      internal void Begin(
         InputAction action,
         InputDevice device,
         string bindingName,
         Action onComplete,
         Action onCancel)
      {
         if (action == null)
         {
            onCancel?.Invoke();
            return;
         }

         var bindingIndex = FindBindingIndex(action, device, bindingName);
         if (bindingIndex < 0)
         {
            onCancel?.Invoke();
            return;
         }

         action.Disable();

         var excludeDevice = device == InputDevice.Keyboard ? "<Gamepad>" : "<Keyboard>";

         _rebindingOp?.Dispose();

         _rebindingOp = action.PerformInteractiveRebinding(bindingIndex).WithControlsExcluding(excludeDevice)
            .WithControlsExcluding("<Mouse>").OnMatchWaitForAnother(0.1f);

         var binding = action.bindings[bindingIndex];

         // Full stick binding (not part of composite)
         if (binding.path.Contains("Stick") && !binding.isPartOfComposite)
         {
            _rebindingOp.WithExpectedControlType("Stick");
         }
         // Composite parts (keyboard or gamepad) - expect Button
         else if (bindingName != null)
         {
            _rebindingOp.WithExpectedControlType("Button");
         }

         _rebindingOp.OnCancel(op =>
            {
               op.Dispose();
               action.Enable();
               onCancel?.Invoke();
            }).OnComplete(op =>
            {
               op.Dispose();

               if (bindingIndex >= 0 && bindingIndex < action.bindings.Count)
               {
                  var b = action.bindings[bindingIndex];

                  if (!string.IsNullOrEmpty(b.overridePath) && string.Equals(
                      b.overridePath,
                      b.path,
                      StringComparison.OrdinalIgnoreCase))
                  {
                     action.RemoveBindingOverride(bindingIndex);
                  }
               }

               action.Enable();
               onComplete?.Invoke();
            });

         _rebindingOp.Start();
      }

      internal void Cancel()
      {
         _rebindingOp?.Cancel();
      }

      public void Dispose()
      {
         _rebindingOp?.Dispose();
         _rebindingOp = null;
      }

      internal string GetBindingDisplay(string id, InputDevice device, string bindingName = null)
      {
         return _query?.GetBindingDisplay(id, device, bindingName);
      }

      private static int FindBindingIndex(InputAction action, InputDevice device, string bindingName)
      {
         var devicePath = device == InputDevice.Keyboard ? "<Keyboard>" : "<Gamepad>";

         for (var i = 0; i < action.bindings.Count; i++)
         {
            var binding = action.bindings[i];
            if (binding.isComposite)
            {
               continue;
            }

            // For gamepad composite parts, path might be empty - find by name
            if (device == InputDevice.Gamepad && binding.isPartOfComposite && bindingName != null)
            {
               if (string.Equals(binding.name, bindingName, StringComparison.OrdinalIgnoreCase))
               {
                  return i;
               }

               continue;
            }

            if (!binding.path.StartsWith(devicePath))
            {
               continue;
            }

            if (bindingName == null || string.Equals(binding.name, bindingName, StringComparison.OrdinalIgnoreCase))
            {
               return i;
            }
         }

         return -1;
      }
   }
}
