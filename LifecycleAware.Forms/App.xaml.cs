﻿#region License

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

#define FORCE_CLEANUP

namespace LifecycleAware.Forms
{
   #region Imports

   using System;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using System.Timers;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;

   #endregion

   public partial class App : Application
   {
      #region Public Constructors

      public App()
      {
         InitializeComponent();

         StartFirstTest();
      }

      private void StartFirstTest()
      {
         var secondViewModel = new SecondViewModel
         {
            TimeRemaining = TOTAL_BROADCAST_TIME
         };

         var timer = new Timer(DELAY_BETWEEN_BROADCASTS);
         var timeToStop = DateTime.Now + TOTAL_BROADCAST_TIME;
         timer.Elapsed += (sender, args) =>
         {
            FormsMessengerUtils.Send(new TestPingMessage());

            secondViewModel.TimeRemaining -= TimeSpan.FromMilliseconds(DELAY_BETWEEN_BROADCASTS);

            if (DateTime.Now >= timeToStop)
            {
               secondViewModel.Message = "FINISHED";

               secondViewModel.TimeRemaining = TimeSpan.FromSeconds(0);

               Debug.WriteLine("Starting garbage collection");
               GC.Collect();
               Debug.WriteLine("Finished garbage collection");

               timer.Stop();
               timer.Dispose();

               StartSecondTest();
            }
         };

         timer.Start();

         var firstPage = new FirstPage { BindingContext = new FirstViewModel() };
         Debug.WriteLine("About to assign the main page to the first page.");
         MainPage = firstPage;

         Device.BeginInvokeOnMainThread
         (
            async () =>
            {
               Debug.WriteLine("Finished assigning the main page to the first page.");
               await Task.Delay(5000);
               Debug.WriteLine("About to assign the main page to the second page.");
               MainPage = new SecondPage { BindingContext = secondViewModel };
               secondViewModel.Message = "Working...";
               Debug.WriteLine("Finished assigning the main page to the second page.");
               Debug.WriteLine("The first view model is now OUT OF SCOPE and should not be active.");
            });
      }

      private void StartSecondTest()
      {
         // TODO_IMPLEMENT_ME();
      }

      #endregion Public Constructors

      #region Private Variables

      private static readonly double DELAY_BETWEEN_BROADCASTS = 1000;
      private static readonly TimeSpan TOTAL_BROADCAST_TIME = TimeSpan.FromSeconds(30);

      #endregion Private Variables

      #region Protected Methods

      protected override void OnResume()
      {
         // Handle when your app resumes
      }

      protected override void OnSleep()
      {
         // Handle when your app sleeps
      }

      protected override void OnStart()
      {
         // Handle when your app starts
      }

      #endregion Protected Methods
   }
}