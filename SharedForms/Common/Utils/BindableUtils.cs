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

namespace SharedForms.Common.Utils
{
   #region Imports

   using System;
   using Xamarin.Forms;

   #endregion Imports

   public static class BindableUtils
   {
     #region Public Methods

     public static BindableProperty CreateBindableProperty<T, U>
     (
       string localPropName,
       U defaultVal = default(U),
       BindingMode bindingMode = BindingMode.OneWay,
       Action<T, U, U> callbackAction = null
     )
       where T : class
     {
       return BindableProperty.Create
       (
         localPropName,
         typeof(U),
         typeof(T),
         defaultVal,
         bindingMode,
         propertyChanged: (bindable, oldVal, newVal) =>
         {
            if (callbackAction != null)
            {
              var bindableAsOverlayButton = bindable as T;
              if (bindableAsOverlayButton != null)
              {
                callbackAction(bindableAsOverlayButton, (U) oldVal, (U) newVal);
              }
            }
         });
     }

     public static BindableProperty CreateReadOnlyBindableProperty<T, U>
     (
       string localPropName,
       U defaultVal = default(U),
       BindingMode bindingMode = BindingMode.OneWay,
       Action<T, U, U> callbackAction = null
     )
       where T : class
     {
       return BindableProperty.CreateReadOnly
       (
         localPropName,
         typeof(U),
         typeof(T),
         defaultVal,
         bindingMode,
         propertyChanged: (bindable, oldVal, newVal) =>
         {
            if (callbackAction != null)
            {
              var bindableAsOverlayButton = bindable as T;
              if (bindableAsOverlayButton != null)
              {
                callbackAction(bindableAsOverlayButton, (U) oldVal, (U) newVal);
              }
            }
         }).BindableProperty;
     }

     #endregion Public Methods
   }
}
