#region License

// MIT License
// 
// Copyright (c) 2018 
// Marcus Technical Services, Inc.
// http://www.marcusts.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

#define ADD_SHADOW

#region Imports

using SharedAndroid;
using SharedForms.Views.Controls;
using Xamarin.Forms;

#endregion

[assembly: ExportRenderer(typeof(ShapeView), typeof(ShapeRenderer))]

namespace SharedAndroid
{
   #region Imports

   using System.ComponentModel;
   using Android.OS;
   using Android.Support.V4.View;
   using SharedForms.Views.Controls;
   using Xamarin.Forms.Platform.Android;

   #endregion

   public class ShapeRenderer : ViewRenderer<ShapeView, Shape>
   {
      protected override void OnElementChanged(ElementChangedEventArgs<ShapeView> e)
      {
         base.OnElementChanged(e);

         if (e.OldElement != null || Element == null)
         {
            return;
         }

         var shape = new Shape(Resources.DisplayMetrics.Density, Context, Element);
         SetNativeControl(shape);
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         if (Control == null || Element == null)
         {
            return;
         }

         switch (e.PropertyName)
         {
            case nameof(Element.ShapeType):
            case nameof(Element.Color):
            case nameof(Element.BorderColor):
            case nameof(Element.BorderWidth):
            case nameof(Element.RadiusRatio):
            case nameof(Element.NumberOfPoints):
            case nameof(Element.CornerRadius):
            case nameof(Element.Progress):
            case nameof(Element.Points):
               Control.Invalidate();
               break;
         }
      }
   }
}