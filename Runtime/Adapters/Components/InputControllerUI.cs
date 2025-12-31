// Assets/P3k/InputController/Runtime/Adapters/Components/InputControllerUI.cs (updated)

namespace P3k.InputController.Adapters.Components
{
   using P3k.InputController.Abstractions.Enums;
   using P3k.InputController.Abstractions.Interfaces.Core;
   using P3k.InputController.Implementations.UI;
   using P3k.InputController.Implementations.UI.Bindings;
   using P3k.InputController.Implementations.UI.Controls;
   using P3k.InputController.Implementations.UI.Feedback;
   using P3k.InputController.Implementations.Utilities;

   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.InputSystem;
   using UnityEngine.UIElements;

   [RequireComponent(typeof(UIDocument))]
   public sealed class InputControllerUI : MonoBehaviour
   {
      private InputControllerDeviceSelection _deviceSelection;

      private VisualElement _documentRoot;

      [SerializeField]
      private MonoBehaviour _inputController;

      private InputBindingsPresenter _presenter;

      private InputControllerProfileControls _profileControls;

      private InputControllerRebindFeedback _rebindFeedback;

      private VisualElement _root;

      private int _runtimeStatsInputCount = -1;

      private VisualElement _runtimeStatsOverlay;

      private readonly Dictionary<string, Label> _runtimeValueLabels = new();

      private VisualElement _runtimeValuesOverlayContainer;

      private Toggle _showStatsToggle;

      private InputBindingsState _state;

      private InputControllerStatus _status;

      [SerializeField]
      private Key _toggleKey = Key.F2;

      private InputControllerUIWindow _uiWindow;

      private InputBindingsView _view;

      private IInputController InputController => _inputController as IInputController;

      private void OnEnable()
      {
         if (_inputController == null)
         {
            Debug.LogError("InputController is not assigned.");
            enabled = false;
            return;
         }

         if (InputController == null)
         {
            Debug.LogError($"{nameof(_inputController)} must implement {nameof(IInputController)}.");
            enabled = false;
            return;
         }

         var doc = GetComponent<UIDocument>();
         _documentRoot = doc.rootVisualElement;
         _root = _documentRoot.Q<VisualElement>("root");

         if (_root == null)
         {
            enabled = false;
            return;
         }

         if (!InputControllerUIControls.TryCreate(_root, out var controls))
         {
            enabled = false;
            return;
         }

         _state = new InputBindingsState();
         _view = new InputBindingsView(_root);
         _presenter = new InputBindingsPresenter(InputController, _view, _state);

         _profileControls = new InputControllerProfileControls(controls, InputController, _presenter, _state);

         _status = new InputControllerStatus(controls, _profileControls);

         _deviceSelection = new InputControllerDeviceSelection(controls, _presenter);

         _uiWindow = new InputControllerUIWindow(controls, _toggleKey, _profileControls.CloseProfileDropdown);

         _rebindFeedback = new InputControllerRebindFeedback(_presenter, _status, _profileControls);

         _profileControls.Initialize();
         _deviceSelection.Initialize();
         _uiWindow.Initialize();

         ScrollViewUtils.Apply(_root);

         BindRuntimeStatsUI();

         _root.style.display = DisplayStyle.None;
         RefreshRuntimeStatsVisibilityAndContent();
      }

      private void OnDisable()
      {
         UnbindRuntimeStatsUI();

         _rebindFeedback?.Dispose();
         _rebindFeedback = null;

         _uiWindow?.Dispose();
         _uiWindow = null;

         _deviceSelection?.Dispose();
         _deviceSelection = null;

         _profileControls?.Dispose();
         _profileControls = null;

         _status = null;

         _presenter = null;
         _view = null;
         _state = null;

         _documentRoot = null;
         _root = null;
      }

      private void Start()
      {
         _presenter.Rebuild();
         _profileControls.RefreshSaveButton();
         RefreshRuntimeStatsVisibilityAndContent();
      }

      private void Update()
      {
         if (_uiWindow.TryToggle())
         {
            _presenter.Rebuild();
            _profileControls.EvaluateSaveState();
            RefreshRuntimeStatsVisibilityAndContent();
         }

         _status.Tick();
         _presenter.Tick();

         TickRuntimeStats();
      }

      private void BindRuntimeStatsUI()
      {
         _showStatsToggle = _root.Q<Toggle>("toggle-show-stats");
         _runtimeStatsOverlay = _documentRoot.Q<VisualElement>("runtime-stats-overlay");
         _runtimeValuesOverlayContainer = _documentRoot.Q<VisualElement>("runtime-values-overlay-container");

         if (_showStatsToggle == null || _runtimeStatsOverlay == null || _runtimeValuesOverlayContainer == null)
         {
            return;
         }

         _runtimeStatsOverlay.style.display = DisplayStyle.None;

         _showStatsToggle.RegisterValueChangedCallback(OnShowStatsToggled);
      }

      private void EnsureRuntimeStatsBuilt()
      {
         if (_runtimeValuesOverlayContainer == null)
         {
            return;
         }

         var inputs = InputController?.Inputs;
         if (inputs == null)
         {
            return;
         }

         var count = inputs.Count;
         if (count == _runtimeStatsInputCount && _runtimeValueLabels.Count == count)
         {
            return;
         }

         _runtimeValuesOverlayContainer.Clear();
         _runtimeValueLabels.Clear();

         foreach (var def in inputs)
         {
            if (def == null || string.IsNullOrEmpty(def.Id))
            {
               continue;
            }

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 20;
            row.style.marginBottom = 6;

            var name = new Label(def.Id);
            name.style.width = 140;
            name.style.fontSize = 11;
            name.style.color = new Color(0.78f, 0.78f, 0.78f);
            name.style.whiteSpace = WhiteSpace.NoWrap;

            var value = new Label("-");
            value.style.flexGrow = 1;
            value.style.fontSize = 11;
            value.style.color = Color.white;
            value.style.unityTextAlign = TextAnchor.MiddleRight;
            value.style.whiteSpace = WhiteSpace.NoWrap;

            row.Add(name);
            row.Add(value);

            _runtimeValuesOverlayContainer.Add(row);
            _runtimeValueLabels[def.Id] = value;
         }

         _runtimeStatsInputCount = count;
      }

      private void OnShowStatsToggled(ChangeEvent<bool> evt)
      {
         RefreshRuntimeStatsVisibilityAndContent();
      }

      private void RefreshRuntimeStatsVisibilityAndContent()
      {
         if (_runtimeStatsOverlay == null || _showStatsToggle == null || _root == null)
         {
            return;
         }

         var windowVisible = _root.style.display != DisplayStyle.None;
         var show = windowVisible && _showStatsToggle.value;

         _runtimeStatsOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

         if (show)
         {
            EnsureRuntimeStatsBuilt();
            TickRuntimeStats();
         }
      }

      private void TickRuntimeStats()
      {
         if (_runtimeStatsOverlay == null || _runtimeStatsOverlay.style.display == DisplayStyle.None)
         {
            return;
         }

         if (_showStatsToggle is not {value: true})
         {
            return;
         }

         if (_root == null || _root.style.display == DisplayStyle.None)
         {
            _runtimeStatsOverlay.style.display = DisplayStyle.None;
            return;
         }

         if (InputController == null)
         {
            return;
         }

         EnsureRuntimeStatsBuilt();

         foreach (var pair in _runtimeValueLabels)
         {
            var state = InputController.Get(pair.Key);
            if (state == null)
            {
               continue;
            }

            pair.Value.text = state.Type switch
               {
                  BindingType.Button => state.Value1D.ToString("0", CultureInfo.InvariantCulture),
                  BindingType.Composite2D => $"{state.Value2D.x:0.00}, {state.Value2D.y:0.00}",
                  _ => "-"
               };
         }
      }

      private void UnbindRuntimeStatsUI()
      {
         _showStatsToggle?.UnregisterValueChangedCallback(OnShowStatsToggled);

         if (_runtimeStatsOverlay != null)
         {
            _runtimeStatsOverlay.style.display = DisplayStyle.None;
         }

         _showStatsToggle = null;
         _runtimeStatsOverlay = null;
         _runtimeValuesOverlayContainer = null;

         _runtimeValueLabels.Clear();
         _runtimeStatsInputCount = -1;
      }
   }
}