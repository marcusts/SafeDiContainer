#region License

// MIT License
//
// Copyright (c) 2018 Marcus Technical Services, Inc. http://www.marcusts.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion License

namespace SharedForms.Views.Controls
{
   #region Imports

   using Common.Interfaces;
   using Common.Utils;
   using System;
   using System.Diagnostics;
   using Xamarin.Forms;

   #endregion Imports

   public interface ILabelButton : IGenericViewButtonBase<Label>
   {
   }

   public class LabelButton : GenericViewButtonBase<Label>, ILabelButton
   {
      private Style _deselectedLabelButtonStyle;
      private Style _disabledLabelButtonStyle;
      private Style _selectedLabelButtonStyle;

      public LabelButton
      (
         Label label
      )
      {
          if (label == null)
          {
             label = new Label();
          }

         label.InputTransparent = true;

         InternalView = label;

         // The label always has a transparent background
         BackgroundColor = Color.Transparent;

         // Applies to the base control only
         InputTransparent = false;

         // Force-refresh the label styles; this will configure the label properly
         SetStyle();
      }

      public LabelButton()
      : this(null)
      {
      }

      public Style SelectedLabelStyle
      {
         get => _selectedLabelButtonStyle;
         set
         {
            _selectedLabelButtonStyle = value;
            SetStyle();
         }
      }

      public Style DeselectedLabelStyle
      {
         get => _deselectedLabelButtonStyle;
         set
         {
            _deselectedLabelButtonStyle = value;
            SetStyle();
         }
      }

      public Style DisabledLabelStyle
      {
         get => _disabledLabelButtonStyle;
         set
         {
            _disabledLabelButtonStyle = value;
            SetStyle();
         }
      }

      protected override void SetStyle()
      {
         base.SetStyle();

         if (InternalView == null)
         {
            return;
         }

         Style newStyle = null;

         // Set the style based on being enabled/disabled
         if (ButtonState == ButtonStates.Disabled)
         {
            newStyle = DisabledLabelStyle ?? DeselectedLabelStyle;
         }
         else if (ButtonState == ButtonStates.Selected)
         {
            newStyle = SelectedLabelStyle ?? DeselectedLabelStyle;
         }
         else
         {
            newStyle = DeselectedLabelStyle;
         }

         // Can't call Equal comparisons on list-style records
         //if (newStyle != null && (InternalView.Style == null || InternalView.Style.IsNotAnEqualObjectTo(newStyle)))
         //{
#if MERGE_STYLES
         InternalView.Style = InternalView.Style.MergeStyle<LabelButton>(newStyle);
#else
         InternalView.Style = newStyle;
#endif

         // This library is not working well with styles, so forcing all settings manually
         InternalView.ForceStyle(newStyle);
         //}
      }

      public static Style CreateLabelStyle
      (
         Color textColor,
         double fontSize,
         FontAttributes fontAttributes = FontAttributes.None
      )
      {
         return new Style(typeof(Label))
         {
            Setters =
            {
               // The text color is now the background color -- should be white
               new Setter { Property = Label.TextColorProperty, Value = textColor },

               // The label is always transparent
               new Setter { Property = BackgroundColorProperty, Value = Color.Transparent },

               new Setter { Property = Label.FontAttributesProperty, Value = fontAttributes },
               new Setter { Property = Label.FontSizeProperty, Value = fontSize }
            }
         };
      }

      public static BindableProperty CreateLabelButtonBindableProperty<PropertyTypeT>
      (
         string localPropName,
         PropertyTypeT defaultVal = default(PropertyTypeT),
         BindingMode bindingMode = BindingMode.OneWay,
         Action<LabelButton, PropertyTypeT, PropertyTypeT> callbackAction = null
      )
      {
         return BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);
      }
   }
}
