// *********************************************************************************
// Assembly         : Com.MarcusTS.LifecycleAware
// Author           : Stephen Marcus (Marcus Technical Services, Inc.)
// Created          : 12-24-2018
// Last Modified On : 12-24-2018
//
// <copyright file="ViewModelWithLifecycle.cs" company="Marcus Technical Services, Inc.">
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

namespace Com.MarcusTS.LifecycleAware.ViewModels
{
   using System.ComponentModel;
   using System.Runtime.CompilerServices;
   using Common.Interfaces;
   using Common.Utils;
   using SharedUtils.Utils;

   /// <summary>
   /// Interface IViewModelWithLifecycle
   /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
   /// Implements the <see cref="INotifyPropertyChanged" />
   /// Implements the <see cref="System.ComponentModel" />
   /// Implements the <see cref="System" />
   /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostPageLifecycleReporter" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.ICleanUpBeforeFinalization" />
   /// Implements the <see cref="INotifyPropertyChanged" />
   /// </summary>
   /// <seealso cref="INotifyPropertyChanged" />
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostAppLifecycleReporter" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.IHostPageLifecycleReporter" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.Common.Interfaces.ICleanUpBeforeFinalization" />
   /// <seealso cref="ICleanUpBeforeFinalization" />
   /// <seealso cref="IHostAppLifecycleReporter" />
   /// <seealso cref="IHostPageLifecycleReporter" />
   /// <seealso cref="IHostPageLifecycleReporter" />
   public interface IViewModelWithLifecycle : INotifyPropertyChanged, IHostAppLifecycleReporter,
                                              IHostPageLifecycleReporter, ICleanUpBeforeFinalization
   { }

   /// <summary>
   /// Use this as the basis of all view models if possible. If not possible in a few cases, copy this code into your
   /// other classes as-is and it will work the same way.
   /// Implements the <see cref="IViewModelWithLifecycle" />
   /// Implements the <see cref="Com.MarcusTS.LifecycleAware.ViewModels.IViewModelWithLifecycle" />
   /// Implements the <see cref="object" />
   /// </summary>
   /// <seealso cref="object" />
   /// <seealso cref="Com.MarcusTS.LifecycleAware.ViewModels.IViewModelWithLifecycle" />
   /// <seealso cref="IViewModelWithLifecycle" />
   /// <remarks>REMEMBER to set the <see cref="AppLifecycleReporter" /> to the current app and the
   /// <see cref="PageLifecycleReporter" /> to the parent page. The event ties are weak and non-binding.</remarks>
   public class ViewModelWithLifecycle : IViewModelWithLifecycle
   {
      #region Public Events

      /// <summary>
      /// Occurs when [property changed].
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion Public Events

      #region Private Destructors

      /// <summary>
      /// Finalizes an instance of the <see cref="ViewModelWithLifecycle" /> class.
      /// </summary>
      ~ViewModelWithLifecycle()
      {
         if (!IsCleaningUpBeforeFinalization)
         {
            IsCleaningUpBeforeFinalization = true;
         }
      }

      #endregion Private Destructors

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
      /// The page lifecycle reporter
      /// </summary>
      private IReportPageLifecycle _pageLifecycleReporter;

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
                  OnIsCleaningUpBeforeFinalization();

                  // Notifies the safe di container and other concerned foreign members
                  this.SendObjectDisappearingMessage();
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
                   (lifecycle,
                    args) =>
                   { }
                  );

               this.SetAnyHandler
                  (
                   handler => _pageLifecycleReporter.PageIsAppearing += OnPageAppearing,
                   handler => _pageLifecycleReporter.PageIsAppearing -= OnPageAppearing,
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
      /// Called when [is cleaning up before finalization].
      /// </summary>
      protected virtual void OnIsCleaningUpBeforeFinalization()
      { }

      /// <summary>
      /// Called when [page appearing].
      /// </summary>
      /// <param name="val">The value.</param>
      protected virtual void OnPageAppearing(object val)
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
      /// Called when [property changed].
      /// </summary>
      /// <param name="propertyName">Name of the property.</param>
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      #endregion Protected Methods
   }
}