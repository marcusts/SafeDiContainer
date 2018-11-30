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

namespace LifecycleAware.Forms
{
   using System.Diagnostics;
   using Lib.ViewModels;
   using PropertyChanged;
   using SharedForms.Common.Utils;

   public interface IFirstViewModelWithLifecycle : IViewModelWithLifecycle
   {
   }

   [AddINotifyPropertyChangedInterface]
   public class FirstViewModelWithLifecycle : ViewModelWithLifecycle, IFirstViewModelWithLifecycle
   {
      #region Public Constructors

      public FirstViewModelWithLifecycle()
      {
         Debug.WriteLine("The first view model with lifecycle is being created.");

         FormsMessengerUtils.Subscribe<TestPingMessage>(this, OnTestPing);
      }

      #endregion Public Constructors

      #region Private Destructors

      ~FirstViewModelWithLifecycle()
      {
         IsCleaningUpBeforeFinalization = true;
         FormsMessengerUtils.Unsubscribe<TestPingMessage>(this);
         Debug.WriteLine("The first view model with lifecycle is FINALIZED.");
      }

      #endregion Private Destructors

      #region Private Methods

      private void OnTestPing(object sender, TestPingMessage args)
      {
         if (!IsCleaningUpBeforeFinalization)
         {
            Debug.WriteLine("The first view model with lifecycle is still listening to events!");
         }
      }

      #endregion Private Methods
   }
}