// *********************************************************************************
// Assembly         : Com.MarcusTS.LifecycleAware
// Author           : Stephen Marcus (Marcus Technical Services, Inc.)
// Created          : 12-24-2018
// Last Modified On : 12-24-2018
//
// <copyright file="ContentViewWithLifecycle.cs" company="Marcus Technical Services, Inc.">
//     Copyright @2018 Marcus Technical Services, Inc.
// </copyright>
//
// MIT License
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// *********************************************************************************

namespace Com.MarcusTS.LifecycleAware.Views.SubViews
{
   using System;
   using Common.Interfaces;
   using Common.Utils;
   using SharedForms.Utils;
   using SharedUtils.Utils;
   using ViewModels;
   using Xamarin.Forms;

   /// <summary>
   /// Interface IContentViewWithLifecycle
   /// Implements the <see cref="IHostAppLifecycleReporter" />
   /// Implements the <see cref="IHostPageLifecycleReporter" />
   /// Implements the <see cref="ICleanUpBeforeFinalization" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostPageLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.ICleanUpBeforeFinalization" />
   /// </summary>
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostPageLifecycleReporter" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.ICleanUpBeforeFinalization" />
   /// <seealso cref="IHostAppLifecycleReporter" />
   /// <seealso cref="IHostPageLifecycleReporter" />
   /// <seealso cref="ICleanUpBeforeFinalization" />
   public interface IContentViewWithLifecycle : IHostAppLifecycleReporter, IHostPageLifecycleReporter,
                                                ICleanUpBeforeFinalization
   { }

   /// <summary>
   /// Use this as the basis of all views if possible. If not possible in a few cases, copy this code into your other
   /// classes as-is and it will work the same way.
   /// Implements the <see cref="Xamarin.Forms.ContentView" />
   /// Implements the <see cref="IContentViewWithLifecycle" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Views.SubViews.IContentViewWithLifecycle" />
   /// Implements the <see cref="ContentView" />
   /// </summary>
   /// <seealso cref="ContentView" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Views.SubViews.IContentViewWithLifecycle" />
   /// <seealso cref="Xamarin.Forms.ContentView" />
   /// <seealso cref="IContentViewWithLifecycle" />
   /// <remarks>REMEMBER to supply the originating page (PageLifecycleReporterProperty), as that is how all of this works. The
   /// event ties are weak and therefore non-binding.</remarks>
   public class ContentViewWithLifecycle : ContentView, IContentViewWithLifecycle
   {
      #region Public Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="ContentViewWithLifecycle" /> class.
      /// </summary>
      public ContentViewWithLifecycle()
      {
         BindingContextChanged += (sender,
                                   args) =>
                                  {
                                     if (BindingContext is IViewModelWithLifecycle bindingContextAsViewModelWithLifecycle)
                                     {
                                        bindingContextAsViewModelWithLifecycle.PageLifecycleReporter = PageLifecycleReporter;
                                     }
                                  };
      }

      #endregion Public Constructors

      #region Public Events

      /// <summary>
      /// Occurs when [page is disappearing].
      /// </summary>
      public event EventUtils.GenericDelegate<object> PageIsDisappearing;

      #endregion Public Events

      #region Public Methods

      /// <summary>
      /// Creates the content view with lifecycle bindable property.
      /// </summary>
      /// <typeparam name="PropertyTypeT">The type of the property type t.</typeparam>
      /// <param name="localPropName">Name of the local property.</param>
      /// <param name="defaultVal">The default value.</param>
      /// <param name="bindingMode">The binding mode.</param>
      /// <param name="callbackAction">The callback action.</param>
      /// <returns>BindableProperty.</returns>
      public static BindableProperty CreateContentViewWithLifecycleBindableProperty<PropertyTypeT>(string localPropName,
                                                                                                   PropertyTypeT
                                                                                                      defaultVal =
                                                                                                      default(
                                                                                                         PropertyTypeT),
                                                                                                   BindingMode
                                                                                                      bindingMode =
                                                                                                      BindingMode
                                                                                                        .OneWay,
                                                                                                   Action<
                                                                                                         ContentViewWithLifecycle
                                                                                                       , PropertyTypeT,
                                                                                                         PropertyTypeT>
                                                                                                      callbackAction =
                                                                                                      null)
      {
         return BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);
      }

      #endregion Public Methods

      #region Private Destructors

      /// <summary>
      /// Finalizes an instance of the <see cref="ContentViewWithLifecycle" /> class.
      /// </summary>
      ~ContentViewWithLifecycle()
      {
         if (!IsCleaningUpBeforeFinalization)
         {
            IsCleaningUpBeforeFinalization = true;
         }
      }

      #endregion Private Destructors

      #region Public Fields

      /// <summary>
      /// The application lifecycle reporter property
      /// </summary>
      public static BindableProperty AppLifecycleReporterProperty =
         CreateContentViewWithLifecycleBindableProperty
            (
             nameof(AppLifecycleReporter),
             default(IReportAppLifecycle),
             BindingMode.OneWay,
             (contentView,
              oldVal,
              newVal) =>
             {
                contentView.AppLifecycleReporter = newVal;
             }
            );

      /// <summary>
      /// The page lifecycle reporter property
      /// </summary>
      public static BindableProperty PageLifecycleReporterProperty =
         CreateContentViewWithLifecycleBindableProperty
            (
             nameof(PageLifecycleReporter),
             default(IReportPageLifecycle),
             BindingMode.OneWay,
             (contentView,
              oldVal,
              newVal) =>
             {
                contentView.PageLifecycleReporter = newVal;
             }
            );

      #endregion Public Fields

      #region Private Fields

      /// <summary>
      /// The application lifecycle reporter
      /// </summary>
      private IReportAppLifecycle _appLifecycleReporter;

      /// <summary>
      /// The is cleaning up
      /// </summary>
      private bool _isCleaningUp;

      /// <summary>
      /// The lifecycle reporter
      /// </summary>
      private IReportPageLifecycle _lifecycleReporter;

      #endregion Private Fields

      #region Public Properties

      /// <summary>
      /// Gets or sets the application lifecycle reporter.
      /// </summary>
      /// <value>The application lifecycle reporter.</value>
      public IReportAppLifecycle AppLifecycleReporter
      {
         get => _appLifecycleReporter;
         set
         {
            _appLifecycleReporter = value;

            if (_appLifecycleReporter != null)
            {
               this.SetAnyHandler
                  (
                   handler => _appLifecycleReporter.AppIsStarting += OnAppStarting,
                   handler => _appLifecycleReporter.AppIsStarting -= OnAppStarting,
                   (lifecycle,
                    args) =>
                   { }
                  );
               this.SetAnyHandler
                  (
                   handler => _appLifecycleReporter.AppIsGoingToSleep += OnAppGoingToSleep,
                   handler => _appLifecycleReporter.AppIsGoingToSleep -= OnAppGoingToSleep,
                   (lifecycle,
                    args) =>
                   { }
                  );
               this.SetAnyHandler
                  (
                   handler => _appLifecycleReporter.AppIsResuming     += OnAppResuming,
                   handler => _appLifecycleReporter.AppIsGoingToSleep -= OnAppResuming,
                   (lifecycle,
                    args) =>
                   { }
                  );
            }
         }
      }

      /// <summary>
      /// Gets or sets a value indicating whether this instance is cleaning up before finalization.
      /// </summary>
      /// <value><c>true</c> if this instance is cleaning up before finalization; otherwise, <c>false</c>.</value>
      public bool IsCleaningUpBeforeFinalization
      {
         get => _isCleaningUp;
         set
         {
            if (_isCleaningUp != value)
            {
               _isCleaningUp = value;

               if (_isCleaningUp)
               {
                  // Notifies the safe di container and other concerned foreign members
                  this.SendObjectDisappearingMessage();

                  // Notifies close relatives like view models
                  PageIsDisappearing?.Invoke(this);
               }
            }
         }
      }

      /// <summary>
      /// Gets or sets the page lifecycle reporter.
      /// </summary>
      /// <value>The page lifecycle reporter.</value>
      public IReportPageLifecycle PageLifecycleReporter
      {
         get => _lifecycleReporter;
         set
         {
            _lifecycleReporter = value;

            if (_lifecycleReporter != null)
            {
               this.SetAnyHandler
                  (
                   handler => _lifecycleReporter.PageIsDisappearing += OnDisappearing,
                   handler => _lifecycleReporter.PageIsDisappearing -= OnDisappearing,
                   (lifecycle,
                    args) =>
                   { }
                  );
            }
         }
      }

      #endregion Public Properties

      #region Protected Methods

      /// <summary>
      /// Called when [application going to sleep].
      /// </summary>
      protected virtual void OnAppGoingToSleep()
      { }

      /// <summary>
      /// Called when [application resuming].
      /// </summary>
      protected virtual void OnAppResuming()
      { }

      /// <summary>
      /// Called when [application starting].
      /// </summary>
      protected virtual void OnAppStarting()
      { }

      /// <summary>
      /// Called when [disappearing].
      /// </summary>
      /// <param name="val">The value.</param>
      protected virtual void OnDisappearing(object val)
      {
         IsCleaningUpBeforeFinalization = true;
      }

      /// <summary>
      /// Called when [page appearing].
      /// </summary>
      /// <param name="val">The value.</param>
      protected virtual void OnPageAppearing(object val)
      { }

      /// <summary>
      /// Called when [page appearing].
      /// </summary>
      /// <param name="page">The page.</param>
      protected virtual void OnPageAppearing(ContentPage page)
      { }

      /// <summary>
      /// Called when [page disappearing].
      /// </summary>
      /// <param name="val">The value.</param>
      protected virtual void OnPageDisappearing(object val)
      {
         IsCleaningUpBeforeFinalization = true;
      }

      /// <summary>
      /// Releases the unmanaged resources.
      /// </summary>
      protected virtual void ReleaseUnmanagedResources()
      { }

      #endregion Protected Methods
   }
}