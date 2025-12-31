namespace P3k.InputController.Implementations.Utilities
{
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UIElements;

   /// <summary>
   /// Utility helpers for configuring <see cref="ScrollView"/> controls used by the
   /// input bindings UI. Sets up appearance and behavior for the scroll area and
   /// its vertical scroller so the bindings list matches the project's visual style.
   /// </summary>
   internal static class ScrollViewUtils
   {
      /// <summary>
      /// Apply visual and layout tweaks to the scroll view with name "bindings-scroll"
      /// found under the provided root element. If the scroll view is not present this
      /// method returns silently.
      /// </summary>
      /// <param name="root">Root visual element that contains the scroll view.</param>
      internal static void Apply(VisualElement root)
      {
         // Locate the scroll view that contains binding rows.
         var scroll = root.Q<ScrollView>("bindings-scroll");
         if (scroll == null)
         {
            // Nothing to configure if the expected element is not present.
            return;
         }

         // Disable horizontal scrolling � the layout should wrap horizontally.
         scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

         // Configure content container to take available width and add a small right
         // padding so the custom scroller doesn't overlap content.
         var content = scroll.contentContainer;
         content.style.flexGrow = 1;
         content.style.width = Length.Percent(100);
         content.style.paddingRight = 6;

         // Tweak the vertical scroller visuals: hide the low/high buttons and set
         // a narrower width to match the UI design.
         var scroller = scroll.verticalScroller;
         scroller.lowButton.style.display = DisplayStyle.None;
         scroller.highButton.style.display = DisplayStyle.None;
         scroller.style.width = 10;

         // The slider element controls the visible track area. Make it match the
         // chosen width so the tracker and dragger align correctly.
         var slider = scroller.Q(className: "unity-scroller__slider");
         if (slider != null)
         {
            slider.style.width = 10;
         }

         // Style the tracker (background of the draggable area).
         var tracker = scroller.Q(className: "unity-base-slider__tracker");
         if (tracker != null)
         {
            tracker.style.width = 10;
            tracker.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            tracker.style.borderTopLeftRadius = 4;
            tracker.style.borderTopRightRadius = 4;
            tracker.style.borderBottomLeftRadius = 4;
            tracker.style.borderBottomRightRadius = 4;
         }

         // Style the dragger (the handle the user drags) to be slightly narrower
         // and visually distinct from the tracker background.
         var dragger = scroller.Q(className: "unity-base-slider__dragger");
         if (dragger != null)
         {
            dragger.style.width = 8;
            dragger.style.left = 1;
            dragger.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            dragger.style.borderTopLeftRadius = 4;
            dragger.style.borderTopRightRadius = 4;
            dragger.style.borderBottomLeftRadius = 4;
            dragger.style.borderBottomRightRadius = 4;
         }
      }
   }
}
