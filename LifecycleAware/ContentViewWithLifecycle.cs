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

namespace LifecycleAware
{
   #region Imports

   using System;
   using SafeDI.Lib;
   using SharedForms.Common.Interfaces;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;

   #endregion

   public interface IContentViewWithLifecycle
   {
      IReportPageLifecycle PageLifecycleReporter { get; set; }
   }

   /// <summary>
   /// Use this as the basis of all views if possible.
   /// If not possible in a few cases, copy this code into your other classes as-is and it will work the same way.
   /// </summary>
   /// <remarks>
   /// REMEMBER to supply the originating page if possible, as that is how all of this works.
   /// The event ties are weak and therefore non-binding.
   /// </remarks>
   public class ContentViewWithLifecycle : ContentView, IContentViewWithLifecycle
   {
      private IReportPageLifecycle _pageLifecycleReporter;

      public static BindableProperty CreateContentViewWithLifecycleBindableProperty<PropertyTypeT>
      (
         string localPropName,
         PropertyTypeT defaultVal = default(PropertyTypeT),
         BindingMode bindingMode = BindingMode.OneWay,
         Action<ContentViewWithLifecycle, PropertyTypeT, PropertyTypeT> callbackAction = null
      )
      {
         return BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);
      }

      public static BindableProperty PageLifecycleReporterProperty =
         CreateContentViewWithLifecycleBindableProperty
            (
               nameof(PageLifecycleReporter),
               default(IReportPageLifecycle),
               BindingMode.OneWay,
               (contentView, oldVal, newVal) =>
               {
                  contentView.PageLifecycleReporter = newVal;
               }
            );

      public ContentViewWithLifecycle(IReportPageLifecycle lifeCycleReporter = null)
      {
         PageLifecycleReporter = lifeCycleReporter;
      }

      protected virtual void OnPageAppearing(ContentPage page)
      {
      }

      protected virtual void OnPageDisappearing(ContentPage page)
      {
         this.SendViewOrPageDisappearingMessage();
      }

      public IReportPageLifecycle PageLifecycleReporter
      {
         get => _pageLifecycleReporter;
         set
         {
            _pageLifecycleReporter = value;

            if (_pageLifecycleReporter != null)
            {
               this.SetAnyHandler
               (
                  handler => _pageLifecycleReporter.PageIsDisappearing += OnPageDisappearing,
                  handler => _pageLifecycleReporter.PageIsDisappearing -= OnPageDisappearing,
                  (lifecycle, args) => { }
               );
            }
         }
      }
   }
}
