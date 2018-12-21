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

using SharedUtils.Utils;

namespace SharedForms.Common.Utils
{
   using System.Linq;
   using System.Threading.Tasks;
   using Xamarin.Forms;

   public static class FormsUtils
   {
      public const string FALSE_STR = "false";

      public const string TRUE_STR = "true";

      internal const double BUTTON_RADIUS_FACTOR = 0.15f;

      internal const float DEFAULT_TEXT_SIZE = 20;

      internal static readonly double MAJOR_BUTTON_HEIGHT = 45.0;

      internal static readonly double MAJOR_BUTTON_WIDTH = 120.0;

      public static void AddAutoColumn(this Grid grid) => grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

      public static void AddAutoRow(this Grid grid) => grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      public static void AddFixedColumn(this Grid grid, double width) => grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

      public static void AddFixedRow(this Grid grid, double height) => grid.RowDefinitions.Add(new RowDefinition { Height = height });

      public static void AddStarColumn(this Grid grid, double factor = 1) => grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(factor, GridUnitType.Star) });

      public static void AddStarRow(this Grid grid, double factor = 1) => grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(factor, GridUnitType.Star) });

      public static void CreateRelativeOverlay(this RelativeLayout layout, View viewToAdd) => layout.Children.Add(
           viewToAdd, Constraint.Constant(0), Constraint.Constant(0),
           Constraint.RelativeToParent(parent => parent.Width),
           Constraint.RelativeToParent(parent => parent.Height));

      public static void ForceStyle(this View view, Style style)
      {
         if (style == null || style.Setters.IsEmpty())
         {
            return;
         }

         for (var setterIdx = 0; setterIdx < style.Setters.Count; setterIdx++)
         {
            view.SetValue(style.Setters[setterIdx].Property, style.Setters[setterIdx].Value);
         }
      }

      public static AbsoluteLayout GetExpandingAbsoluteLayout() => new AbsoluteLayout
      {
         HorizontalOptions = LayoutOptions.FillAndExpand,
         VerticalOptions = LayoutOptions.FillAndExpand,
         BackgroundColor = Color.Transparent
      };

      public static Grid GetExpandingGrid() => new Grid
      {
         HorizontalOptions = LayoutOptions.FillAndExpand,
         VerticalOptions = LayoutOptions.FillAndExpand,
         BackgroundColor = Color.Transparent
      };

      public static RelativeLayout GetExpandingRelativeLayout() => new RelativeLayout
      {
         HorizontalOptions = LayoutOptions.FillAndExpand,
         VerticalOptions = LayoutOptions.FillAndExpand,
         BackgroundColor = Color.Transparent
      };

      public static StackLayout GetExpandingStackLayout() => new StackLayout
      {
         VerticalOptions = LayoutOptions.StartAndExpand,
         HorizontalOptions = LayoutOptions.FillAndExpand,
         BackgroundColor = Color.Transparent,
         Orientation = StackOrientation.Vertical
      };

      public static Image GetImage
     (
       string filePath,
       double width = 0,
       double height = 0,
       Aspect aspect = Aspect.AspectFit
     )
      {
         var retImage =
           new Image
           {
              Aspect = aspect,
              VerticalOptions = LayoutOptions.Center,
              HorizontalOptions = LayoutOptions.Center
           };

         if (filePath.IsNotEmpty())
         {
            retImage.Source = ImageSource.FromFile(filePath);
         }

         if (width.IsNotEmpty())
         {
            retImage.WidthRequest = width;
         }

         if (height.IsNotEmpty())
         {
            retImage.HeightRequest = height;
         }

         return retImage;
      }

      public static Label GetSimpleLabel
     (
       string labelText = default(string),
       Color textColor = default(Color),
       TextAlignment textAlignment = TextAlignment.Center,
       NamedSize fontNamedSize = NamedSize.Medium,
       double fontSize = 0.0,
       FontAttributes fontAttributes = FontAttributes.None,
       double width = 0,
       double height = 0,
       string labelBindingPropertyName = default(string),
       object labelBindingSource = null,
       LineBreakMode breakMode = LineBreakMode.WordWrap
     )
      {
         if (textColor.IsAnEqualObjectTo(default(Color)))
         {
            textColor = Color.Black;
         }

         var retLabel =
           new Label
           {
              Text = labelText,
              TextColor = textColor,
              HorizontalTextAlignment = textAlignment,
              VerticalTextAlignment = TextAlignment.Center,
              HorizontalOptions = HorizontalOptionsFromTextAlignment(textAlignment),
              VerticalOptions = LayoutOptions.CenterAndExpand,
              BackgroundColor = Color.Transparent,
              InputTransparent = true,
              FontAttributes = fontAttributes,
              FontSize = fontSize.IsNotEmpty() ? fontSize : Device.GetNamedSize(fontNamedSize, typeof(Label)),
              LineBreakMode = breakMode
           };

         // Set up the label text binding (if provided)
         if (labelBindingPropertyName.IsNotEmpty())
         {
            if (labelBindingSource != null)
            {
               retLabel.SetUpBinding(Label.TextProperty, labelBindingPropertyName, source: labelBindingSource);
            }
            else
            {
               retLabel.SetUpBinding(Label.TextProperty, labelBindingPropertyName);
            }
         }

         if (width.IsNotEmpty())
         {
            retLabel.WidthRequest = width;
         }

         if (height.IsNotEmpty())
         {
            retLabel.HeightRequest = height;
         }

         return retLabel;
      }

      // ------------------------------------------------------------------------------------------
      public static BoxView GetSpacer(double height) => new BoxView
      {
         HeightRequest = height,
         BackgroundColor = Color.Transparent,
         HorizontalOptions = LayoutOptions.FillAndExpand,
         VerticalOptions = LayoutOptions.FillAndExpand
      };

      public static LayoutOptions HorizontalOptionsFromTextAlignment(TextAlignment textAlignment)
      {
         switch (textAlignment)
         {
            case TextAlignment.Center:
               return LayoutOptions.Center;

            case TextAlignment.End:
               return LayoutOptions.End;

            // Covers Start and default
            default:
               return LayoutOptions.Start;
         }
      }

      public static Style MergeStyle<T>(this Style mainStyle, Style newStyle)
      {
         if (newStyle == null || newStyle.Setters.IsEmpty())
         {
            return mainStyle;
         }

         if (mainStyle == null)
         {
            mainStyle = new Style(typeof(T));
         }

         foreach (var setter in newStyle.Setters)
         {
            var foundSetter =
               mainStyle.Setters.FirstOrDefault(s => s.Property.PropertyName.IsSameAs(setter.Property.PropertyName));
            if (foundSetter != null)
            {
               var foundSetterIdx = mainStyle.Setters.IndexOf(foundSetter);

               mainStyle.Setters[foundSetterIdx] = setter;
            }
            else
            {
               mainStyle.Setters.Add(setter);
            }
         }

         // The Style settings must also be considered in the assignment
         mainStyle.ApplyToDerivedTypes = newStyle.ApplyToDerivedTypes || mainStyle.ApplyToDerivedTypes;

         //if (newStyle.BasedOn != default(Style))
         //{
         //   mainStyle.BasedOn = newStyle.BasedOn;
         //}

         // TODO
         //mainStyle.BaseResourceKey = newStyle.BaseResourceKey;
         //mainStyle.CanCascade = newStyle.CanCascade;
         //mainStyle.Class = newStyle.Class;
         //mainStyle.Behaviors = newStyle.Behaviors;
         //mainStyle.Triggers = newStyle.Triggers;
         //mainStyle.TargetType = newStyle.TargetType;

         return mainStyle;
      }

      public static void SetUpBinding
     (
       this BindableObject view,
       BindableProperty bindableProperty,
       string viewModelPropertyName,
       BindingMode bindingMode = BindingMode.OneWay,
       IValueConverter converter = null,
       object converterParameter = null,
       string stringFormat = null,
       object source = null
     ) => view.SetBinding(bindableProperty,
           new Binding(viewModelPropertyName, bindingMode, converter, converterParameter, stringFormat, source));

      public static async Task WithoutChangingContext(this Task task) => await task.ConfigureAwait(false);

      public static async Task<T> WithoutChangingContext<T>(this Task<T> task) => await task.ConfigureAwait(false);

      internal static ScrollView GetExpandingScrollView() => new ScrollView
      {
         VerticalOptions = LayoutOptions.FillAndExpand,
         HorizontalOptions = LayoutOptions.FillAndExpand,
         BackgroundColor = Color.Transparent,
         Orientation = ScrollOrientation.Vertical
      };
   }
}