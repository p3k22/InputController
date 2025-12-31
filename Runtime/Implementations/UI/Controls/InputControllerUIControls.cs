namespace P3k.InputController.Implementations.UI.Controls
{
   using System.Linq;

   using UnityEngine.UIElements;

   internal sealed class InputControllerUIControls
   {
      internal Button BtnGamepad { get; private set; }

      internal Button BtnKeyboard { get; private set; }

      internal Button BtnReset { get; private set; }

      internal Button BtnSave { get; private set; }

      internal VisualElement DragHandle { get; private set; }

      internal Button ProfileDropdownButton { get; private set; }

      internal Button ProfileDropdownToggleButton { get; private set; }

      internal TextField ProfileNameField { get; private set; }

      internal VisualElement Root { get; private set; }

      internal Label StatusLabel { get; private set; }

      internal static bool TryCreate(VisualElement root, out InputControllerUIControls controls)
      {
         var dragHandle = root.Q<VisualElement>("drag-handle");

         var btnKeyboard = root.Q<Button>("btn-keyboard");
         var btnGamepad = root.Q<Button>("btn-gamepad");
         var btnSave = root.Q<Button>("btn-apply");
         var btnReset = root.Q<Button>("btn-reset");

         var profileNameField = root.Q<TextField>("profile-name-field");
         var profileDropdownButton = root.Q<Button>("profile-dropdown-button");
         var profileDropdownToggleButton = root.Q<Button>("profile-dropdown-toggle-button");

         var statusLabel = root.Q<Label>("status-label");

         if (btnKeyboard == null || btnGamepad == null || btnSave == null || btnReset == null
             || profileNameField == null || profileDropdownButton == null)
         {
            controls = null;
            return false;
         }

         controls = new InputControllerUIControls
                       {
                          Root = root,
                          DragHandle = dragHandle,
                          BtnKeyboard = btnKeyboard,
                          BtnGamepad = btnGamepad,
                          BtnSave = btnSave,
                          BtnReset = btnReset,
                          ProfileNameField = profileNameField,
                          ProfileDropdownButton = profileDropdownButton,
                          ProfileDropdownToggleButton = profileDropdownToggleButton,
                          StatusLabel = statusLabel,
                       };

         return true;
      }
   }
}
