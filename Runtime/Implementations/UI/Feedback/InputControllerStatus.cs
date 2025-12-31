namespace P3k.InputController.Implementations.UI.Feedback
{
   using P3k.InputController.Implementations.UI.Controls;
   using P3k.InputController.Implementations.Utilities;

   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   internal sealed class InputControllerStatus
   {
      private bool _isWaitingForInput;

      private float _statusHideAt;

      private readonly Label _statusLabel;

      internal InputControllerStatus(InputControllerUIControls controls, InputControllerProfileControls profileControls)
      {
         _statusLabel = controls.StatusLabel;

         profileControls.OnBindingsReset += () =>
            {
               Show("Bindings have been reset to default", new Color(0.85f, 0.85f, 0.35f), false);
            };
         profileControls.OnProfileSaved += () =>
            {
               Show(
               $"Profile {controls.ProfileNameField.value}.json saved to {InputProfileUtils.ProfilesDir}",
               new Color(0.3f, 0.8f, 0.3f),
               false);
            };
      }

      internal void Show(string text, Color color, bool sticky)
      {
         if (_statusLabel == null)
         {
            return;
         }

         _statusLabel.text = text;
         _statusLabel.style.color = color;
         _statusLabel.style.visibility = Visibility.Visible;

         _isWaitingForInput = sticky;

         if (!sticky)
         {
            _statusHideAt = Time.unscaledTime + 5f;
         }
      }

      internal void Tick()
      {
         if (_statusLabel == null)
         {
            return;
         }

         if (_isWaitingForInput)
         {
            return;
         }

         if (Time.unscaledTime >= _statusHideAt)
         {
            _statusLabel.style.visibility = Visibility.Hidden;
         }
      }
   }
}
