namespace P3k.InputController.Implementations.UI
{
   using P3k.InputController.Implementations.UI.Controls;

   using System;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;
   using UnityEngine.UIElements;

   internal sealed class InputControllerUIWindow : IDisposable
   {
      private readonly VisualElement _dragHandle;

      private Vector2 _dragStart;

      private bool _isDragging;

      private readonly Action _onHidden;

      private bool _positionInitialized;

      private readonly VisualElement _root;

      private readonly Key _toggleKey;

      private Vector2 _windowStart;

      internal InputControllerUIWindow(InputControllerUIControls controls, Key toggleKey, Action onHidden)
      {
         _root = controls.Root;
         _dragHandle = controls.DragHandle;
         _toggleKey = toggleKey;
         _onHidden = onHidden;
      }

      public void Dispose()
      {
         if (_dragHandle == null)
         {
            return;
         }

         _dragHandle.UnregisterCallback<PointerDownEvent>(OnPointerDown);
         _dragHandle.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
         _dragHandle.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      }

      internal void Initialize()
      {
         if (_dragHandle == null)
         {
            return;
         }

         _dragHandle.RegisterCallback<PointerDownEvent>(OnPointerDown);
         _dragHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
         _dragHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
      }

      internal bool TryToggle()
      {
         if (Keyboard.current == null)
         {
            return false;
         }

         if (!Keyboard.current[_toggleKey].wasPressedThisFrame)
         {
            return false;
         }

         var show = _root.style.display != DisplayStyle.Flex;
         _root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

         if (!show)
         {
            _onHidden?.Invoke();
         }

         return show;
      }

      private void OnPointerDown(PointerDownEvent evt)
      {
         if (!_positionInitialized)
         {
            var b = _root.worldBound;
            _root.style.left = b.x;
            _root.style.top = b.y;
            _positionInitialized = true;
         }

         _isDragging = true;
         _dragStart = evt.position;
         _windowStart = new Vector2(_root.resolvedStyle.left, _root.resolvedStyle.top);
         _dragHandle.CapturePointer(evt.pointerId);
         evt.StopPropagation();
      }

      private void OnPointerMove(PointerMoveEvent evt)
      {
         if (!_isDragging)
         {
            return;
         }

         var delta = new Vector2(evt.position.x, evt.position.y) - _dragStart;
         _root.style.left = _windowStart.x + delta.x;
         _root.style.top = _windowStart.y + delta.y;
         evt.StopPropagation();
      }

      private void OnPointerUp(PointerUpEvent evt)
      {
         _isDragging = false;
         _dragHandle.ReleasePointer(evt.pointerId);
         evt.StopPropagation();
      }
   }
}