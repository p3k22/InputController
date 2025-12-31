namespace P3k.InputController.Implementations.UI.Bindings
{
   using P3k.InputController.Abstractions.Enums;

   using System;
   using System.Linq;

   internal sealed class InputBindingsState
   {
      private bool _bindingsDirty;

      internal InputDevice CurrentDevice { get; set; } = InputDevice.Keyboard;

      internal bool IsDirty => _bindingsDirty || ProfileDirty;

      internal bool ProfileDirty { get; private set; }

      /// <summary>
      ///    Fired whenever IsDirty may have changed.
      /// </summary>
      internal event Action DirtyChanged;

      internal void MarkBindingsDirty(bool profileDirty)
      {
         var before = IsDirty;

         _bindingsDirty = true;
         ProfileDirty = profileDirty;

         NotifyIfDirtyChanged(before);
      }

      internal void SetDirty(bool bindings, bool profile)
      {
         var before = IsDirty;

         _bindingsDirty = bindings;
         ProfileDirty = profile;

         NotifyIfDirtyChanged(before);
      }

      private void NotifyIfDirtyChanged(bool before)
      {
         var after = IsDirty;
         if (before != after)
         {
            DirtyChanged?.Invoke();
         }
      }
   }
}
