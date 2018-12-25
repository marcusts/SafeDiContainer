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

namespace SmartDIWithLifecycle.Forms
{
   using System;
   using PropertyChanged;
   using Xamarin.Forms;

   public interface ISecondViewModel
   {
      #region Public Properties

      string Message { get; set; }
      TimeSpan TimeRemaining { get; set; }

      #endregion Public Properties
   }

   [AddINotifyPropertyChangedInterface]
   public class SecondViewModel : ISecondViewModel
   {
      #region Public Constructors

      public SecondViewModel()
      {
      }

      #endregion Public Constructors

      #region Public Properties

      public string FirstViewModelDebugOutput { get; set; }

      public string Message { get; set; }
      public TimeSpan TimeRemaining { get; set; }

      #endregion Public Properties

      public void HandleFirstViewModelDebugOutputChanged(string firstViewModelOutput)
      {
         FirstViewModelDebugOutput = firstViewModelOutput;

         // Remove the first view model broadcast so it does not get stuck on the screen
         //Device.StartTimer(TimeSpan.FromMilliseconds(App.DELAY_BETWEEN_BROADCASTS * 3 / 4), () =>
         //                                                                               {
         //                                                                                  FirstViewModelDebugOutput = "";
         //                                                                                  return false;
         //                                                                               });
      }
   }
}
