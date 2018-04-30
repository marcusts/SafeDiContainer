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

namespace LifecycleAware.Lib
{
   #region Imports

   using System.ComponentModel;
   using System.Runtime.CompilerServices;
   using Annotations;
   using SharedUtils;
   using Xamarin.Forms;

   #endregion

   public interface IViewModelWithLifecycle : INotifyPropertyChanged, IHostLifecycleReporter, ICanCleanUp
   {
   }

   /// <summary>
   ///    Use this as the basis of all view models if possible. If not possible in a few cases, copy
   ///    this code into your other classes as-is and it will work the same way.
   /// </summary>
   /// <remarks>
   ///    REMEMBER to supply the originating view (PageLifecycleReporterProperty), as that is how all of
   ///    this works. The event ties are weak and therefore non-binding.
   /// </remarks>
   public class ViewModelWithLifecycle : IViewModelWithLifecycle
   {
      #region Public Constructors

      public ViewModelWithLifecycle(IReportLifecycle lifeCycleReporter = null)
      {
         LifecycleReporter = lifeCycleReporter;
      }

      #endregion Public Constructors

      #region Public Events

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion Public Events

      #region Private Destructors

      ~ViewModelWithLifecycle()
      {
         if (!IsCleaningUp)
         {
            IsCleaningUp = true;
         }
      }

      #endregion Private Destructors

      #region Private Variables

      private bool _isCleaningUp;

      private IReportLifecycle _lifecycleReporter;

      #endregion Private Variables

      #region Public Properties

      public bool IsCleaningUp
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
               }
            }
         }
      }

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
                  handler => _lifecycleReporter.IsDisappearing += OnParentDisappearing,
                  handler => _lifecycleReporter.IsDisappearing -= OnParentDisappearing,
                  (lifecycle, args) => { }
               );
            }
         }
      }

      #endregion Public Properties

      #region Protected Methods

      protected virtual void OnPageAppearing(ContentPage page)
      {
      }

      protected virtual void OnParentDisappearing(object val)
      {
         IsCleaningUp = true;
      }

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      #endregion Protected Methods
   }
}