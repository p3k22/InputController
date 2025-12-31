namespace P3k.InputController.Implementations.UI.Bindings
{
   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   internal sealed class InputBindingsView
   {
      internal readonly Dictionary<string, Button> BindingButtons = new();

      internal VisualElement BindingsContainer { get; }

      internal InputBindingsView(VisualElement root)
      {
         BindingsContainer = root.Q<VisualElement>("bindings-container");
      }

      internal static VisualElement CreateSection(string label)
      {
         var section = new VisualElement();
         section.style.marginBottom = 8;
         section.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
         section.style.borderTopLeftRadius = 4;
         section.style.borderTopRightRadius = 4;
         section.style.borderBottomLeftRadius = 4;
         section.style.borderBottomRightRadius = 4;
         section.style.paddingTop = 6;
         section.style.paddingBottom = 6;
         section.style.paddingLeft = 8;
         section.style.paddingRight = 8;

         var header = new Label(label);
         header.style.fontSize = 11;
         header.style.color = Color.white;
         header.style.marginBottom = 4;
         header.style.unityFontStyleAndWeight = FontStyle.Bold;

         section.Add(header);
         return section;
      }

      internal static VisualElement CreateToggleRow(string label, bool value, EventCallback<ChangeEvent<bool>> callback)
      {
         var row = new VisualElement();
         row.style.flexDirection = FlexDirection.Row;
         row.style.alignItems = Align.Center;
         row.style.justifyContent = Justify.FlexStart;
         row.style.height = 22;
         row.style.width = Length.Percent(100);
         row.style.marginTop = 4;
         row.style.marginBottom = 4;

         // LABEL � SAME AS OTHER ROW LABELS
         var lbl = new Label(label);
         lbl.style.width = 70;
         lbl.style.minWidth = 70;
         lbl.style.fontSize = 10;
         lbl.style.color = new Color(0.75f, 0.75f, 0.75f);
         lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
         lbl.style.marginRight = 6;
         lbl.style.whiteSpace = WhiteSpace.NoWrap;

         // TOGGLE � IMMEDIATELY AFTER LABEL
         var toggle = new Toggle();
         toggle.value = value;
         toggle.style.alignSelf = Align.FlexStart;
         toggle.style.marginLeft = 10;

         var checkmark = toggle.Q<VisualElement>("unity-checkmark");
         if (checkmark != null)
         {
            checkmark.style.width = 12;
            checkmark.style.height = 12;
         }

         toggle.RegisterValueChangedCallback(callback);

         row.Add(lbl);
         row.Add(toggle);

         return row;
      }

      internal void Clear()
      {
         BindingButtons.Clear();
         BindingsContainer.Clear();
      }

      internal VisualElement CreateBindingRow(string key, string label, string binding, Action onClick, bool disabled)
      {
         var row = new VisualElement();
         row.style.flexDirection = FlexDirection.Row;
         row.style.alignItems = Align.Center;
         row.style.justifyContent = Justify.FlexStart;
         row.style.height = 24;
         row.style.marginBottom = 8;
         row.style.width = Length.Percent(100);

         // LABEL COLUMN (MATCHES TOGGLE ROW)
         var lbl = new Label(label);
         lbl.style.width = 70;
         lbl.style.minWidth = 70;
         lbl.style.fontSize = 10;
         lbl.style.color = new Color(0.75f, 0.75f, 0.75f);
         lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
         lbl.style.marginRight = 6;

         // CONTROL COLUMN (SHARED GRID)
         var controlColumn = new VisualElement();
         controlColumn.style.flexGrow = 1;
         controlColumn.style.alignItems = Align.FlexStart;
         controlColumn.style.justifyContent = Justify.Center;

         var btn = new Button();
         btn.text = binding;
         btn.style.width = 120;
         btn.style.height = 22;
         btn.style.fontSize = 10;
         btn.style.unityTextAlign = TextAnchor.MiddleCenter;
         btn.style.borderTopLeftRadius = 3;
         btn.style.borderTopRightRadius = 3;
         btn.style.borderBottomLeftRadius = 3;
         btn.style.borderBottomRightRadius = 3;

         if (disabled)
         {
            btn.SetEnabled(false);
            btn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            btn.style.color = new Color(0.5f, 0.5f, 0.5f);
         }
         else
         {
            btn.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            btn.style.color = Color.white;
            if (onClick != null)
            {
               btn.clicked += onClick;
            }
         }

         // REQUIRED: presenter tracks buttons by key
         RegisterBindingButton(key, btn);

         controlColumn.Add(btn);

         row.Add(lbl);
         row.Add(controlColumn);

         return row;
      }

      private void RegisterBindingButton(string key, Button button)
      {
         BindingButtons[key] = button;
      }
   }
}
