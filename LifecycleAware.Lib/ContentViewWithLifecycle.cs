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
   using SharedForms.Common.Utils;
   using SharedUtils;
   using Xamarin.Forms;

   #endregion

   public interface IContentViewWithLifecycle : IDisposable, IReportEndOfLifecycle
   {
      #region Public Properties

      IReportLifecycle LifecycleReporter { get; set; }

      #endregion Public Properties
   }

   /// <summary>
   ///    Use this as the basis of all views if possible. If not possible in a few cases, copy this code
   ///    into your other classes as-is and it will work the same way.
   /// </summary>
   /// <remarks>
   ///    REMEMBER to supply the originating page (PageLifecycleReporterProperty), as that is how all of
   ///    this works. The event ties are weak and therefore non-binding.
   /// </remarks>
   public class ContentViewWithLifecycle : ContentView, IContentViewWithLifecycle
   {
      #region Public Variables

      public static BindableProperty PageLifecycleReporterProperty =
         CreateContentViewWithLifecycleBindableProperty
         (
            nameof(LifecycleReporter),
            default(IReportLifecycle),
            BindingMode.OneWay,
            (contentView, oldVal, newVal) => { contentView.LifecycleReporter = newVal; }
         );

      #endregion Public Variables

      #region Private Variables

      private IReportLifecycle _lifecycleReporter;

      #endregion Private Variables

      #region Public Constructors

      public ContentViewWithLifecycle(IReportLifecycle lifeCycleReporter = null)
      {
         LifecycleReporter = lifeCycleReporter;
      }

      #endregion Public Constructors

      #region Protected Properties

      /// <summary>
      ///    Helpful when a view is about to go out of scope, but the parent page containing it is *not*
      ///    disappearing. The programmer should call Dispose() on this view in that single case. Also,
      ///    if any processes are running, and IsDisposing is true, stop those processes *immediately*.
      ///    The garbage collector is too slow to manage this in a better way for us. Classes are
      ///    surprisingly active until the are finalized unless they implement IDisposable. Finalization
      ///    can take a very long time.
      /// </summary>
      protected bool IsDisposing { get; set; }

      #endregion Protected Properties

      #region Public Events

      public event EventUtils.GenericDelegate<object> IsDisappearing;

      #endregion Public Events

      #region Public Properties

      public IReportLifecycle LifecycleReporter
      {
         get => _lifecycleReporter;
         set
         {
            _lifecycleReporter = value;

            if (_lifecycleReporter != null)
            {
               this.SetAnyHandler
               (
                  handler => _lifecycleReporter.IsDisappearing += OnDisappearing,
                  handler => _lifecycleReporter.IsDisappearing -= OnDisappearing,
                  (lifecycle, args) => { }
               );
            }
         }
      }

      #endregion Public Properties

      #region Private Destructors

      ~ContentViewWithLifecycle()
      {
         Dispose(false);
      }

      #endregion Private Destructors

      #region Public Methods

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

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      #endregion Public Methods

      #region Protected Methods

      protected virtual void Dispose(bool disposing)
      {
         IsDisposing = disposing;

         ReleaseUnmanagedResources();
         if (disposing)
         {
         }
      }

      protected virtual void OnDisappearing(object val)
      {
         Dispose();
      }

      protected virtual void OnPageAppearing(ContentPage page)
      {
      }

      protected virtual void ReleaseUnmanagedResources()
      {
         // Notifies the safe di container and other concerned foreign members
         this.SendViewOrPageDisappearingMessage();

         // Notifies close relatives like view models
         IsDisappearing?.Invoke(this);
      }

      #endregion Protected Methods
   }
}