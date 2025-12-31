namespace P3k.InputController.Implementations.UI.Feedback
{
   using P3k.InputController.Implementations.UI.Bindings;
   using P3k.InputController.Implementations.UI.Controls;

   using System;
   using System.Linq;

   using UnityEngine;

   internal sealed class InputControllerRebindFeedback : IDisposable
   {
      private readonly InputBindingsPresenter _presenter;

      private readonly InputControllerProfileControls _profiles;

      private readonly InputControllerStatus _status;

      internal InputControllerRebindFeedback(
         InputBindingsPresenter presenter,
         InputControllerStatus status,
         InputControllerProfileControls profiles)
      {
         _presenter = presenter;
         _status = status;
         _profiles = profiles;

         _presenter.OnRebindStarted += HandleRebindStarted;
         _presenter.OnRebindTimerUpdated += HandleRebindTimerUpdated;
         _presenter.OnRebindSuccess += HandleRebindSuccess;
         _presenter.OnRebindFailed += HandleRebindFailed;
      }

      public void Dispose()
      {
         _presenter.OnRebindStarted -= HandleRebindStarted;
         _presenter.OnRebindTimerUpdated -= HandleRebindTimerUpdated;
         _presenter.OnRebindSuccess -= HandleRebindSuccess;
         _presenter.OnRebindFailed -= HandleRebindFailed;
      }

      private void HandleRebindFailed()
      {
         _status.Show("Rebind failed", new Color(0.85f, 0.3f, 0.3f), false);
      }

      private void HandleRebindStarted()
      {
         _status.Show("Waiting for input…", new Color(0.85f, 0.85f, 0.35f), true);
      }

      private void HandleRebindSuccess()
      {
         _profiles.EvaluateSaveState();
         _status.Show("Rebind successful", new Color(0.3f, 0.8f, 0.3f), false);
      }

      private void HandleRebindTimerUpdated(float secondsRemaining)
      {
         _status.Show($"Waiting for input… {Mathf.CeilToInt(secondsRemaining)}", new Color(0.85f, 0.85f, 0.35f), true);
      }
   }
}