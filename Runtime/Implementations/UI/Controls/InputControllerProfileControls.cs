namespace P3k.InputController.Implementations.UI.Controls
{
   using P3k.InputController.Abstractions.Interfaces.Core;
   using P3k.InputController.Implementations.UI.Bindings;
   using P3k.InputController.Implementations.Utilities;

   using System;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   internal sealed class InputControllerProfileControls : IDisposable
   {
      private readonly Button _btnReset;

      private readonly Button _btnSave;

      private readonly IInputController _inputController;

      private readonly InputBindingsPresenter _presenter;

      private readonly Button _profileDropdownButton;

      private VisualElement _profileDropdownMenu;

      private readonly Button _profileDropdownToggleButton;

      private readonly TextField _profileNameField;

      private readonly VisualElement _root;

      private EventCallback<PointerDownEvent> _rootPointerDownCallback;

      private readonly InputBindingsState _state;

      internal event Action OnBindingsReset;

      internal event Action OnProfileSaved;

      internal InputControllerProfileControls(
         InputControllerUIControls controls,
         IInputController inputController,
         InputBindingsPresenter presenter,
         InputBindingsState state)
      {
         _root = controls.Root;

         _btnSave = controls.BtnSave;
         _btnReset = controls.BtnReset;

         _profileNameField = controls.ProfileNameField;
         _profileDropdownButton = controls.ProfileDropdownButton;
         _profileDropdownToggleButton = controls.ProfileDropdownToggleButton;

         _inputController = inputController;
         _presenter = presenter;
         _state = state;
      }

      public void Dispose()
      {
         _state.DirtyChanged -= RefreshSaveButton;

         _btnSave.clicked -= SaveProfile;
         _btnReset.clicked -= ResetBindings;

         if (_profileDropdownButton != null)
         {
            _profileDropdownButton.clicked -= ToggleProfileDropdown;
         }

         if (_profileDropdownToggleButton != null)
         {
            _profileDropdownToggleButton.clicked -= ToggleProfileDropdown;
         }

         CloseProfileDropdown();
      }

      internal void CloseProfileDropdown()
      {
         if (_profileDropdownMenu == null)
         {
            return;
         }

         if (_rootPointerDownCallback != null)
         {
            _root.UnregisterCallback(_rootPointerDownCallback, TrickleDown.TrickleDown);
         }

         _profileDropdownMenu.RemoveFromHierarchy();
         _profileDropdownMenu = null;
      }

      internal void EvaluateSaveState()
      {
         var name = _profileNameField.value?.Trim();

         if (string.IsNullOrEmpty(name))
         {
            _state.SetDirty(false, false);
            return;
         }

         if (!ProfileExists(name))
         {
            _state.SetDirty(true, true);
            return;
         }

         _state.SetDirty(_inputController.ProfileCanBeSaved(name), false);
      }

      internal void Initialize()
      {
         _rootPointerDownCallback = OnRootPointerDown;

         _state.DirtyChanged += RefreshSaveButton;

         InitializeProfileUI();
         WireButtons();
      }

      internal void RefreshSaveButton()
      {
         if (_state.IsDirty)
         {
            _btnSave.SetEnabled(true);
            _btnSave.style.backgroundColor = new Color(0.2f, 0.5f, 0.2f);
            _btnSave.style.color = Color.white;
         }
         else
         {
            _btnSave.SetEnabled(false);
            _btnSave.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _btnSave.style.color = new Color(0.5f, 0.5f, 0.5f);
         }
      }

      private static bool ProfileExists(string profileName)
      {
         return !string.IsNullOrWhiteSpace(profileName) && InputProfileUtils.Exists(profileName);
      }

      private void InitializeProfileUI()
      {
         _profileNameField.SetEnabled(true);
         _profileNameField.focusable = true;

         _profileNameField.value = _inputController.CurrentProfile;
         _profileDropdownButton.text = _inputController.CurrentProfile;

         _profileNameField.RegisterValueChangedCallback(_ => { EvaluateSaveState(); });

         var input = _profileNameField.Q<VisualElement>("unity-text-input");
         if (input != null)
         {
            input.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            input.style.color = Color.white;
            input.style.fontSize = 10;
            input.style.paddingLeft = 6;
            input.style.paddingRight = 6;
            input.style.paddingTop = 2;
            input.style.paddingBottom = 2;
         }

         var profileRow = _profileNameField.parent;
         if (profileRow != null)
         {
            profileRow.style.marginRight = 10;
            profileRow.style.marginTop = 20;
            profileRow.style.paddingRight = 10;
         }

         _profileNameField.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
         _profileDropdownButton.style.width = new StyleLength(new Length(40, LengthUnit.Percent));
         _profileDropdownButton.style.unityTextAlign = TextAnchor.MiddleLeft;
      }

      private void OnRootPointerDown(PointerDownEvent evt)
      {
         if (_profileDropdownMenu == null)
         {
            return;
         }

         if (evt.target is VisualElement ve)
         {
            if (_profileDropdownMenu.Contains(ve))
            {
               return;
            }

            if (ve == _profileDropdownButton || ve == _profileDropdownToggleButton)
            {
               return;
            }
         }

         CloseProfileDropdown();
      }

      private void ResetBindings()
      {
         _inputController.ResetBindingsToDefault(_profileNameField.value);
         _state.SetDirty(false, false);
         _presenter.Rebuild();
         OnBindingsReset?.Invoke();
      }

      private void SaveProfile()
      {
         var name = _profileNameField.value?.Trim();
         if (string.IsNullOrEmpty(name))
         {
            return;
         }

         _inputController.ProfileSave(name);
         _profileDropdownButton.text = name;
         _state.SetDirty(false, false);

         _presenter.Rebuild();

         OnProfileSaved?.Invoke();
      }

      private void SelectProfile(string profile)
      {
         _profileNameField.value = profile;
         _profileDropdownButton.text = profile;

         _inputController.ProfileLoad(profile);
         _state.SetDirty(false, false);

         _presenter.Rebuild();
      }

      private void ToggleProfileDropdown()
      {
         if (_profileDropdownMenu != null)
         {
            CloseProfileDropdown();
            return;
         }

         if (_profileDropdownButton.worldBound.width <= 0f)
         {
            _root.schedule.Execute(ToggleProfileDropdown).ExecuteLater(0);
            return;
         }

         var profiles = InputProfileUtils.GetProfiles();
         if (profiles == null || profiles.Count == 0)
         {
            return;
         }

         _profileDropdownMenu = new VisualElement();
         _profileDropdownMenu.name = "profile-dropdown-menu";
         _profileDropdownMenu.style.position = Position.Absolute;
         _profileDropdownMenu.style.width = 180;
         _profileDropdownMenu.style.maxHeight = 160;
         _profileDropdownMenu.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

         var b = _profileDropdownButton.worldBound;
         var r = _root.worldBound;
         _profileDropdownMenu.style.left = b.x - r.x;
         _profileDropdownMenu.style.top = (b.y - r.y) + b.height + 2;

         var scroll = new ScrollView();
         scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
         scroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
         scroll.style.maxHeight = 160;

         foreach (var profile in profiles)
         {
            var p = profile;
            var item = new Button {text = p};
            item.style.height = 22;
            item.style.unityTextAlign = TextAnchor.MiddleLeft;
            item.style.paddingLeft = 6;

            item.clicked += () =>
               {
                  SelectProfile(p);
                  CloseProfileDropdown();
               };

            scroll.Add(item);
         }

         _profileDropdownMenu.Add(scroll);
         _root.Add(_profileDropdownMenu);

         if (_rootPointerDownCallback != null)
         {
            _root.RegisterCallback(_rootPointerDownCallback, TrickleDown.TrickleDown);
         }
      }

      private void WireButtons()
      {
         _btnSave.clicked += SaveProfile;
         _btnReset.clicked += ResetBindings;

         if (_profileDropdownButton != null)
         {
            _profileDropdownButton.clicked += ToggleProfileDropdown;
         }

         if (_profileDropdownToggleButton != null)
         {
            _profileDropdownToggleButton.clicked += ToggleProfileDropdown;
         }
      }
   }
}