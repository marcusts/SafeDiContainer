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

using SharedUtils.Interfaces;
using SharedUtils.Utils;

namespace SharedForms.Views.Controls
{
   using System;
   using Common.Interfaces;
   using Common.Utils;
   using Xamarin.Forms;

   public interface IImageButton : IGenericViewButtonBase<Image>
   {
      string ImageFileNameRoot { get; set; }

      double ImageHeight { get; set; }
      double ImageWidth { get; set; }
   }

   public class CustomImageButton : GenericViewButtonBase<Image>, IImageButton
   {
      public static readonly BindableProperty ImageFileNameRootProperty =
        CreateImageButtonBindableProperty
        (
          nameof(ImageFileNameRoot),
          default(string),
          BindingMode.OneWay,
          (imageButton, oldVal, newVal) => { imageButton.ImageFileNameRoot = newVal; }
        );

      //---------------------------------------------------------------------------------------------------------------
      // CONSTRUCTOR
      //---------------------------------------------------------------------------------------------------------------
      //---------------------------------------------------------------------------------------------------------------
      // CONSTANTS
      //---------------------------------------------------------------------------------------------------------------
      public static readonly BindableProperty ImageHeightProperty =
        CreateImageButtonBindableProperty
        (
          nameof(ImageHeight),
          default(double),
          BindingMode.OneWay,
          (imageButton, oldVal, newVal) => { imageButton.ImageHeight = newVal; }
        );

      public static readonly BindableProperty ImageWidthProperty =
       CreateImageButtonBindableProperty
       (
         nameof(ImageWidth),
         default(double),
         BindingMode.OneWay,
         (imageButton, oldVal, newVal) => { imageButton.ImageWidth = newVal; }
       );

      private const string DISABLED_SUFFIX = "_disabled";

      private const string PNG_SUFFIX = ".png";

      private const string SELECTED_SUFFIX = "_selected";

      private string _imageFileNameRoot;

      //---------------------------------------------------------------------------------------------------------------
      // PROPERTIES - Public
      //---------------------------------------------------------------------------------------------------------------
      private double _imageHeight;

      private double _imageWidth;

      private string _lastImageFileName;

      private bool _setStyleEntered;

      public CustomImageButton() =>
       // Force-refresh the image styles; this will configure the Image properly
       SetStyle();

      //---------------------------------------------------------------------------------------------------------------
      // VARIABLES
      //---------------------------------------------------------------------------------------------------------------
      public string ImageFileNameRoot
      {
         get => _imageFileNameRoot;
         set
         {
            if (_imageFileNameRoot.IsDifferentThan(value))
            {
               _imageFileNameRoot = value;
               CallRecreateImageSafely();
            }
         }
      }

      //---------------------------------------------------------------------------------------------------------------
      // METHODS - Protected
      //---------------------------------------------------------------------------------------------------------------
      public double ImageHeight
      {
         get => _imageHeight;
         set
         {
            if (_imageHeight.IsDifferentThan(value))
            {
               _imageHeight = value;
               CallRecreateImageSafely();
            }
         }
      }

      public double ImageWidth
      {
         get => _imageWidth;
         set
         {
            if (_imageWidth.IsDifferentThan(value))
            {
               _imageWidth = value;
               CallRecreateImageSafely();
            }
         }
      }

      public static BindableProperty CreateImageButtonBindableProperty<PropertyTypeT>
                       (
       string localPropName,
       PropertyTypeT defaultVal = default(PropertyTypeT),
       BindingMode bindingMode = BindingMode.OneWay,
       Action<CustomImageButton, PropertyTypeT, PropertyTypeT> callbackAction = null
     ) => BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);

      protected override void SetStyle()
      {
         if (_setStyleEntered)
         {
            return;
         }

         _setStyleEntered = true;

         base.SetStyle();

         CallRecreateImageSafely();

         _setStyleEntered = false;
      }

      //---------------------------------------------------------------------------------------------------------------
      // METHODS - Private
      //---------------------------------------------------------------------------------------------------------------

      private void CallRecreateImageSafely()
      {
         if (ThreadHelper.IsOnMainThread)
         {
            RecreateImage();
         }
         else
         {
            Device.BeginInvokeOnMainThread(RecreateImage);
         }
      }

      private void RecreateImage()
      {
         if (ImageWidth.IsEmpty() && ImageHeight.IsEmpty())
         {
            return;
         }

         var imageFileName = _imageFileNameRoot;

         // If no selection, just use the root file name.
         if (CanSelect)
         {
            //Determine the current file name
            switch (ButtonState)
            {
               case ButtonStates.Selected:
                  imageFileName += SELECTED_SUFFIX;
                  break;

               case ButtonStates.Disabled:
                  imageFileName += DISABLED_SUFFIX;
                  break;

               default:
                  // case ButtonStates.Deselected:
                  break;
            }
         }

         if (imageFileName.IsEmpty())
         {
            return;
         }

         // InternalView = null;

         if (!imageFileName.EndsWith(PNG_SUFFIX))
         {
            imageFileName += PNG_SUFFIX;
         }

         if (imageFileName.IsSameAs(_lastImageFileName))
         {
            return;
         }

         InternalView = FormsUtils.GetImage(imageFileName, ImageWidth, ImageHeight);

         // The image always has a transparent background
         InternalView.BackgroundColor = Color.Transparent;

         InternalView.InputTransparent = true;

         _lastImageFileName = imageFileName;
      }

      //---------------------------------------------------------------------------------------------------------------
      // BINDABLE PROPERTIES
      //---------------------------------------------------------------------------------------------------------------
   }
}