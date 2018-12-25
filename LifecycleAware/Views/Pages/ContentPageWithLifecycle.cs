// *********************************************************************************
// Assembly         : Com.MarcusTS.LifecycleAware
// Author           : Stephen Marcus (Marcus Technical Services, Inc.)
// Created          : 12-24-2018
// Last Modified On : 12-24-2018
//
// <copyright file="ContentPageWithLifecycle.cs" company="Marcus Technical Services, Inc.">
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

namespace Com.MarcusTS.LifecycleAware.Views.Pages
{
   using System;
   using Common.Interfaces;
   using Common.Utils;
   using SharedForms.Utils;
   using SharedUtils.Utils;
   using SubViews;
   using ViewModels;
   using Xamarin.Forms;

   /// <summary>
   /// Interface IContentPageWithLifecycle
   /// Implements the <see cref="IHostAppLifecycleReporter" />
   /// Implements the <see cref="IHostAppLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IReportPageLifecycle" />
   /// </summary>
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IReportPageLifecycle" />
   /// <seealso cref="IReportPageLifecycle" />
   /// <seealso cref="IReportPageLifecycle" />
   /// <remarks>WARNING: .Net does not provide IContentPage, so references to this interface type *must* type-cast to
   /// ContentPage_LCA manually.</remarks>
   public interface IContentPageWithLifecycle : IHostAppLifecycleReporter, IReportPageLifecycle
   { }

   /// <summary>
   /// Use this as the basis of all pages if possible. If not possible in a few cases, copy this code into your other
   /// classes as-is and it will work the same way.
   /// Implements the <see cref="Xamarin.Forms.ContentPage" />
   /// Implements the <see cref="IContentPageWithLifecycle" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Views.Pages.IContentPageWithLifecycle" />
   /// Implements the <see cref="ContentPage" />
   /// </summary>
   /// <seealso cref="ContentPage" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Views.Pages.IContentPageWithLifecycle" />
   /// <seealso cref="Xamarin.Forms.ContentPage" />
   /// <seealso cref="IContentPageWithLifecycle" />
   public class ContentPageWithLifecycle : ContentPage, IContentPageWithLifecycle
   {
      #region Public Fields

      /// <summary>
      /// The application lifecycle reporter property
      /// </summary>
      public static BindableProperty AppLifecycleReporterProperty =
         CreateContentPageWithLifecycleBindableProperty
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

      #endregion Public Fields

      #region Private Fields

      /// <summary>
      /// The application lifecycle reporter
      /// </summary>
      private IReportAppLifecycle _appLifecycleReporter;

      #endregion Private Fields

      #region Public Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="ContentPageWithLifecycle"/> class.
      /// </summary>
      public ContentPageWithLifecycle()
      {
         BindingContextChanged += (sender,
                                   args) =>
                                  {
                                     if (BindingContext is IViewModelWithLifecycle bindingContextAsViewModelWithLifecycle)
                                     {
                                        bindingContextAsViewModelWithLifecycle.PageLifecycleReporter = this;
                                     }
                                  };
      }

      #endregion Public Constructors

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

      #endregion Public Properties

      #region Public Events

      /// <summary>
      /// Occurs when [page is appearing].
      /// </summary>
      public event EventUtils.GenericDelegate<object> PageIsAppearing;

      /// <summary>
      /// Occurs when [page is disappearing].
      /// </summary>
      public event EventUtils.GenericDelegate<object> PageIsDisappearing;

      #endregion Public Events

      #region Public Methods

      /// <summary>
      /// Creates the content page with lifecycle bindable property.
      /// </summary>
      /// <typeparam name="PropertyTypeT">The type of the property type t.</typeparam>
      /// <param name="localPropName">Name of the local property.</param>
      /// <param name="defaultVal">The default value.</param>
      /// <param name="bindingMode">The binding mode.</param>
      /// <param name="callbackAction">The callback action.</param>
      /// <returns>BindableProperty.</returns>
      public static BindableProperty CreateContentPageWithLifecycleBindableProperty<PropertyTypeT>(string localPropName,
                                                                                                   PropertyTypeT
                                                                                                      defaultVal =
                                                                                                      default(
                                                                                                         PropertyTypeT),
                                                                                                   BindingMode
                                                                                                      bindingMode =
                                                                                                      BindingMode
                                                                                                        .OneWay,
                                                                                                   Action<
                                                                                                         ContentPageWithLifecycle
                                                                                                       , PropertyTypeT,
                                                                                                         PropertyTypeT>
                                                                                                      callbackAction =
                                                                                                      null)
      {
         return BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);
      }

      /// <summary>
      /// Creates the view for page.
      /// </summary>
      /// <param name="viewCreator">The view creator.</param>
      /// <param name="viewModelCreator">The view model creator.</param>
      /// <returns>View.</returns>
      /// <exception cref="ArgumentException">viewCreator</exception>
      /// <exception cref="System.ArgumentException">viewCreator</exception>
      public View CreateViewForPage(Func<ContentViewWithLifecycle> viewCreator,
                                    Func<ViewModelWithLifecycle>   viewModelCreator = null)
      {
         if (viewCreator == null)
         {
            throw new ArgumentException(nameof(viewCreator) + " must be supplied");
         }

         var returnView = viewCreator();

         if (returnView is IHostAppLifecycleReporter returnViewAsAppLifecycleReporter)
         {
            returnViewAsAppLifecycleReporter.AppLifecycleReporter = AppLifecycleReporter;
         }

         if (viewModelCreator != null)
         {
            var viewModel = viewModelCreator();
            viewModel.PageLifecycleReporter = this;

            if (viewModel is IHostAppLifecycleReporter viewModelAsAppLifecycleReporter)
            {
               viewModelAsAppLifecycleReporter.AppLifecycleReporter = AppLifecycleReporter;
            }
         }

         return returnView;
      }

      #endregion Public Methods

      #region Protected Methods

      /// <summary>
      /// When overridden, allows application developers to customize behavior immediately prior to the
      /// <see cref="T:Xamarin.Forms.Page" /> becoming visible.
      /// </summary>
      /// <remarks>To be added.</remarks>
      protected override void OnAppearing()
      {
         base.OnAppearing();

         PageIsAppearing?.Invoke(this);
      }

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
      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         PageIsDisappearing?.Invoke(this);

         this.SendObjectDisappearingMessage();
      }

      #endregion Protected Methods
   }
}