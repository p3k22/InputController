//TODO: Create a default InputConfig layout.

using P3k.InputController.Abstractions.Enums;
using P3k.InputController.Implementations.DataContainers;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputConfig))]
public class InputConfigEditor : Editor
{
   private static readonly string[] BindingTypeNames = {"Button", "Composite2D"};

   private static readonly string[] GamepadButtonPaths =
      {
         "", "<Gamepad>/buttonSouth", "<Gamepad>/buttonEast", "<Gamepad>/buttonWest", "<Gamepad>/buttonNorth",
         "<Gamepad>/leftShoulder", "<Gamepad>/rightShoulder", "<Gamepad>/leftTrigger", "<Gamepad>/rightTrigger",
         "<Gamepad>/select", "<Gamepad>/start", "<Gamepad>/leftStickPress", "<Gamepad>/rightStickPress",
         "<Gamepad>/dpad/up", "<Gamepad>/dpad/down", "<Gamepad>/dpad/left", "<Gamepad>/dpad/right"
      };

   private static readonly string[] GamepadButtons =
      {
         "Unassigned", "A / Cross", "B / Circle", "X / Square", "Y / Triangle", "LB", "RB", "LT", "RT", "Select",
         "Start", "Left Stick Press", "Right Stick Press", "Dpad Up", "Dpad Down", "Dpad Left", "Dpad Right"
      };

   private static readonly string[] GamepadStickDirectionPaths =
      {
         "", "<Gamepad>/leftStick/up", "<Gamepad>/leftStick/down", "<Gamepad>/leftStick/left",
         "<Gamepad>/leftStick/right", "<Gamepad>/rightStick/up", "<Gamepad>/rightStick/down",
         "<Gamepad>/rightStick/left", "<Gamepad>/rightStick/right"
      };

   private static readonly string[] GamepadStickDirections =
      {
         "Unassigned", "Left Stick Up", "Left Stick Down", "Left Stick Left", "Left Stick Right", "Right Stick Up",
         "Right Stick Down", "Right Stick Left", "Right Stick Right"
      };

   private static readonly string[] KeyboardButtonPaths =
      {
         "", "<Keyboard>/space", "<Keyboard>/enter", "<Keyboard>/escape", "<Keyboard>/tab", "<Keyboard>/backquote",
         "<Keyboard>/backspace", "<Keyboard>/delete", "<Keyboard>/leftShift", "<Keyboard>/rightShift",
         "<Keyboard>/leftCtrl", "<Keyboard>/rightCtrl", "<Keyboard>/leftAlt", "<Keyboard>/rightAlt",
         "<Keyboard>/leftMeta", "<Keyboard>/rightMeta", "<Keyboard>/leftArrow", "<Keyboard>/rightArrow",
         "<Keyboard>/upArrow", "<Keyboard>/downArrow", "<Keyboard>/a", "<Keyboard>/b", "<Keyboard>/c", "<Keyboard>/d",
         "<Keyboard>/e", "<Keyboard>/f", "<Keyboard>/g", "<Keyboard>/h", "<Keyboard>/i", "<Keyboard>/j", "<Keyboard>/k",
         "<Keyboard>/l", "<Keyboard>/m", "<Keyboard>/n", "<Keyboard>/o", "<Keyboard>/p", "<Keyboard>/q", "<Keyboard>/r",
         "<Keyboard>/s", "<Keyboard>/t", "<Keyboard>/u", "<Keyboard>/v", "<Keyboard>/w", "<Keyboard>/x", "<Keyboard>/y",
         "<Keyboard>/z", "<Mouse>/leftButton", "<Mouse>/rightButton", "<Mouse>/middleButton", "<Mouse>/forwardButton",
         "<Mouse>/backButton", "<Mouse>/scroll/up", "<Mouse>/scroll/down"
      };

   private static readonly string[] KeyboardButtons =
      {
         "Unassigned", "Space", "Enter", "Escape", "Tab", "Backquote (`)", "Backspace", "Delete", "Left Shift",
         "Right Shift", "Left Ctrl", "Right Ctrl", "Left Alt", "Right Alt", "Left Meta", "Right Meta", "Left Arrow",
         "Right Arrow", "Up Arrow", "Down Arrow", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
         "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Mouse Left", "Mouse Right", "Mouse Middle",
         "Mouse Forward", "Mouse Back", "Mouse Scroll Up", "Mouse Scroll Down"
      };

   private static readonly string[] KeyboardKeyPaths =
      {
         "", "<Keyboard>/w", "<Keyboard>/a", "<Keyboard>/s", "<Keyboard>/d", "<Keyboard>/upArrow",
         "<Keyboard>/leftArrow", "<Keyboard>/downArrow", "<Keyboard>/rightArrow", "<Keyboard>/q", "<Keyboard>/e",
         "<Keyboard>/z", "<Keyboard>/x", "<Keyboard>/c", "<Keyboard>/space", "<Keyboard>/leftShift",
         "<Keyboard>/rightShift", "<Keyboard>/leftCtrl", "<Keyboard>/rightCtrl"
      };

   private static readonly string[] KeyboardKeys =
      {
         "Unassigned", "W", "A", "S", "D", "Up Arrow", "Left Arrow", "Down Arrow", "Right Arrow", "Q", "E", "Z", "X",
         "C", "Space", "Left Shift", "Right Shift", "Left Ctrl", "Right Ctrl"
      };

   private readonly Dictionary<int, bool> _foldouts = new();

   private bool _autoSync = true;
   private SerializedProperty _generatedAssetProp;
   private SerializedProperty _inputsProp;

   private void OnEnable()
   {
      _inputsProp = serializedObject.FindProperty("_inputs");
      _generatedAssetProp = serializedObject.FindProperty("_generatedActionAsset");
   }

   public override void OnInspectorGUI()
   {
      serializedObject.Update();

      DrawMyHeader();

      if (_inputsProp == null || !_inputsProp.isArray)
      {
         EditorGUILayout.HelpBox("Inputs list not found or not an array.", MessageType.Error);
         serializedObject.ApplyModifiedProperties();
         return;
      }

      EditorGUILayout.Space();

      DrawInputActionAssetSection();

      EditorGUILayout.Space();

      DrawInputsList();

      var hasChanges = serializedObject.hasModifiedProperties;

      serializedObject.ApplyModifiedProperties();

      // Auto-sync when changes are applied
      if (hasChanges && _autoSync)
      {
         SyncInputActionAsset();
      }
   }

   private static void DrawBindingTypeDropdown(SerializedProperty typeProp)
   {
      var currentType = (BindingType) typeProp.enumValueIndex;
      var currentIndex = currentType == BindingType.Button ? 0 : 1;

      var newIndex = EditorGUILayout.Popup("Type", currentIndex, BindingTypeNames);

      typeProp.enumValueIndex = newIndex == 0 ? (int) BindingType.Button : (int) BindingType.Composite2D;
   }

   private static void DrawButtonFields(SerializedProperty keyboard, SerializedProperty gamepad)
   {
      EditorGUILayout.LabelField("Keyboard", EditorStyles.miniBoldLabel);
      EditorGUI.indentLevel++;

      var kbPrimary = keyboard.FindPropertyRelative("_primary");
      DrawKeyboardDropdown(kbPrimary, "Keyboard", KeyboardButtons, KeyboardButtonPaths);

      EditorGUI.indentLevel--;

      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Gamepad", EditorStyles.miniBoldLabel);
      EditorGUI.indentLevel++;

      var gpPrimary = gamepad.FindPropertyRelative("_primary");
      DrawGamepadDropdown(gpPrimary, "Gamepad", GamepadButtons, GamepadButtonPaths);

      EditorGUI.indentLevel--;
   }

   private static void DrawComposite2DFields(
      SerializedProperty keyboard,
      SerializedProperty gamepad,
      SerializedProperty allowAnalogKeyboard,
      SerializedProperty defaultUseMouse,
      SerializedProperty allowAnalogGamepad)
   {
      // Keyboard section
      EditorGUILayout.PropertyField(allowAnalogKeyboard, new GUIContent("Allow Mouse (Keyboard)"));

      if (defaultUseMouse != null)
      {
         if (!allowAnalogKeyboard.boolValue)
         {
            defaultUseMouse.boolValue = false;
         }
         else
         {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(defaultUseMouse, new GUIContent("Use Mouse By Default"));
            EditorGUI.indentLevel--;
         }
      }

      EditorGUILayout.LabelField("Keyboard", EditorStyles.miniBoldLabel);
      EditorGUI.indentLevel++;

      DrawKeyboardBindingField(keyboard.FindPropertyRelative("_up"), "Up");
      DrawKeyboardBindingField(keyboard.FindPropertyRelative("_down"), "Down");
      DrawKeyboardBindingField(keyboard.FindPropertyRelative("_left"), "Left");
      DrawKeyboardBindingField(keyboard.FindPropertyRelative("_right"), "Right");

      EditorGUI.indentLevel--;

      // Gamepad section
      EditorGUILayout.Space();
      EditorGUILayout.PropertyField(allowAnalogGamepad, new GUIContent("Allow Stick (Gamepad)"));

      EditorGUILayout.LabelField("Gamepad", EditorStyles.miniBoldLabel);
      EditorGUI.indentLevel++;

      DrawGamepadDropdown(
      gamepad.FindPropertyRelative("_up"),
      "Up",
      GamepadStickDirections,
      GamepadStickDirectionPaths);
      DrawGamepadDropdown(
      gamepad.FindPropertyRelative("_down"),
      "Down",
      GamepadStickDirections,
      GamepadStickDirectionPaths);
      DrawGamepadDropdown(
      gamepad.FindPropertyRelative("_left"),
      "Left",
      GamepadStickDirections,
      GamepadStickDirectionPaths);
      DrawGamepadDropdown(
      gamepad.FindPropertyRelative("_right"),
      "Right",
      GamepadStickDirections,
      GamepadStickDirectionPaths);

      EditorGUI.indentLevel--;
   }

   private static void DrawGamepadDropdown(
      SerializedProperty prop,
      string label,
      IReadOnlyList<string> display,
      IReadOnlyList<string> paths)
   {
      var currentPath = prop.stringValue ?? string.Empty;

      var currentIndex = 0;
      for (var i = 0; i < paths.Count; i++)
      {
         if (string.Equals(paths[i], currentPath, StringComparison.OrdinalIgnoreCase))
         {
            currentIndex = i;
            break;
         }
      }

      var newIndex = EditorGUILayout.Popup(label, currentIndex, display as string[] ?? display.ToArray());

      prop.stringValue = paths[newIndex];
   }

   private static void DrawKeyboardBindingField(SerializedProperty prop, string label)
   {
      DrawKeyboardDropdown(prop, label, KeyboardKeys, KeyboardKeyPaths);
   }

   private static void DrawKeyboardDropdown(
      SerializedProperty prop,
      string label,
      IReadOnlyList<string> display,
      IReadOnlyList<string> paths)
   {
      var currentPath = prop.stringValue ?? string.Empty;

      var currentIndex = 0;
      for (var i = 0; i < paths.Count; i++)
      {
         if (string.Equals(paths[i], currentPath, StringComparison.OrdinalIgnoreCase))
         {
            currentIndex = i;
            break;
         }
      }

      var newIndex = EditorGUILayout.Popup(label, currentIndex, display as string[] ?? display.ToArray());

      prop.stringValue = paths[newIndex];
   }

   private static void DrawMyHeader()
   {
      EditorGUILayout.LabelField("Input Config", EditorStyles.boldLabel);
      EditorGUILayout.HelpBox(
      "Define input IDs and their default bindings.\n" + "Button: single binding.\n"
      + "Composite2D: up/down/left/right bindings.\n\n"
      + "For Composite2D you can optionally allow mouse/stick analog modes (runtime) and set a default mouse mode (editor).\n\n"
      + "Use the 'Sync InputActionAsset' button or enable Auto-Sync to generate an InputActionAsset for use with Action References.",
      MessageType.Info);
   }

   private void DrawInputActionAssetSection()
   {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      EditorGUILayout.LabelField("InputActionAsset Sync", EditorStyles.boldLabel);

      EditorGUI.indentLevel++;

      // Show the generated asset field (read-only display)
      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.PropertyField(_generatedAssetProp, new GUIContent("Generated Asset"));
      EditorGUI.EndDisabledGroup();

      // Auto-sync toggle
      _autoSync = EditorGUILayout.Toggle("Auto-Sync on Change", _autoSync);

      EditorGUILayout.Space();

      // Manual sync button
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Sync InputActionAsset"))
      {
         SyncInputActionAsset();
      }

      var config = (InputConfig) target;
      if (config.GeneratedActionAsset != null)
      {
         if (GUILayout.Button("Select Asset", GUILayout.Width(100)))
         {
            EditorGUIUtility.PingObject(config.GeneratedActionAsset);
            Selection.activeObject = config.GeneratedActionAsset;
         }
      }

      EditorGUILayout.EndHorizontal();

      // Status message
      if (_generatedAssetProp.objectReferenceValue != null)
      {
         var asset = (InputActionAsset) _generatedAssetProp.objectReferenceValue;
         var actionCount = asset.actionMaps.SelectMany(m => m.actions).Count();
         EditorGUILayout.HelpBox($"Asset contains {actionCount} action(s).", MessageType.None);
      }
      else
      {
         EditorGUILayout.HelpBox("No InputActionAsset generated yet. Click 'Sync InputActionAsset' to create one.", MessageType.Warning);
      }

      EditorGUI.indentLevel--;

      EditorGUILayout.EndVertical();
   }

   private void DrawInputDefinition(int index, SerializedProperty property)
   {
      var idProp = property.FindPropertyRelative("_id");
      var typeProp = property.FindPropertyRelative("_type");
      var keyboardProp = property.FindPropertyRelative("_keyboard");
      var gamepadProp = property.FindPropertyRelative("_gamepad");
      var allowAnalogKeyboardProp = property.FindPropertyRelative("_allowAnalogKeyboard");
      var defaultUseMouseProp = property.FindPropertyRelative("_defaultUseMouse");
      var allowAnalogGamepadProp = property.FindPropertyRelative("_allowAnalogGamepad");

      var type = (BindingType) typeProp.enumValueIndex;
      var headerLabel = string.IsNullOrEmpty(idProp.stringValue) ? $"Input {index}" : idProp.stringValue;

      EditorGUILayout.BeginHorizontal();

      _foldouts[index] = EditorGUILayout.Foldout(_foldouts[index], headerLabel, true);

      if (GUILayout.Button("X", GUILayout.Width(22)))
      {
         _inputsProp.DeleteArrayElementAtIndex(index);
         EditorGUILayout.EndHorizontal();
         return;
      }

      EditorGUILayout.EndHorizontal();

      if (!_foldouts[index])
      {
         return;
      }

      EditorGUI.indentLevel++;

      EditorGUILayout.PropertyField(idProp, new GUIContent("Id"));

      DrawBindingTypeDropdown(typeProp);

      EditorGUILayout.Space();

      switch (type)
      {
         case BindingType.Button:
            DrawButtonFields(keyboardProp, gamepadProp);
            break;

         case BindingType.Composite2D:
            DrawComposite2DFields(
            keyboardProp,
            gamepadProp,
            allowAnalogKeyboardProp,
            defaultUseMouseProp,
            allowAnalogGamepadProp);
            break;
      }

      EditorGUI.indentLevel--;
   }

   private void DrawInputsList()
   {
      for (var i = 0; i < _inputsProp.arraySize; i++)
      {
         var element = _inputsProp.GetArrayElementAtIndex(i);
         if (element == null)
         {
            continue;
         }

         if (!_foldouts.ContainsKey(i))
         {
            _foldouts[i] = true;
         }

         EditorGUILayout.BeginVertical(GUI.skin.box);

         DrawInputDefinition(i, element);

         EditorGUILayout.EndVertical();
         EditorGUILayout.Space();
      }

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Add Input"))
      {
         _inputsProp.InsertArrayElementAtIndex(_inputsProp.arraySize);
      }

      if (GUILayout.Button("Remove Last") && _inputsProp.arraySize > 0)
      {
         _inputsProp.DeleteArrayElementAtIndex(_inputsProp.arraySize - 1);
      }

      EditorGUILayout.EndHorizontal();
   }

   private void SyncInputActionAsset()
   {
      var config = (InputConfig) target;
      if (config == null)
      {
         return;
      }

      var generatedAsset = InputActionAssetGenerator.GenerateAsset(config);
      if (generatedAsset != null)
      {
         // Update the reference on the config
         config.GeneratedActionAsset = generatedAsset;
         EditorUtility.SetDirty(config);
         AssetDatabase.SaveAssets();

         // Refresh the serialized object to show the updated reference
         serializedObject.Update();
      }
   }
}