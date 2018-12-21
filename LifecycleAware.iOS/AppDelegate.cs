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

using SharedUtils.Utils;

namespace LifecycleAware.iOS
{
   using System.Diagnostics;
   using Forms;
   using Foundation;
   using SharedForms.Common.DeviceServices;
   using SharedForms.Common.Utils;
   using UIKit;
   using Xamarin.Forms;
   using Xamarin.Forms.Platform.iOS;

   // The UIApplicationDelegate for the application. This class is responsible for launching the
   // User Interface of the application, as well as listening (and optionally responding) to
   // application events from iOS.
   [Register(nameof(AppDelegate))]
   public class AppDelegate : FormsApplicationDelegate
   {
      #region Private Fields

      private float _oldScreenHeight;
      private float _oldScreenWidth;

      #endregion Private Fields

      #region Public Methods

      //------------------------------------------------------------------------------------------
      public override void DidEnterBackground(UIApplication app)
      {
         // Use this method to release shared resources, save user data, invalidate timers and store
         // the application state. If your application supports background execution this method is
         // called instead of WillTerminate when the user quits.
      }

      // This method is invoked when the application has loaded and is ready to run. In this method
      // you should instantiate the window, load the UI into it and then make the window visible.
      //
      // You have 17 seconds to return from this method, or iOS will terminate your application.
      public override bool FinishedLaunching(UIApplication app, NSDictionary options)
      {
         Forms.Init();

         ShapeRenderer.Init();

         LoadApplication(new App());

         // Assign the OrientationService's event handler to listen to our orientation service's changes
         FormsMessengerUtils.Subscribe<LocalDeviceSizeChangedMessage>(this, OrientationService.HandleDeviceSizeChanged);

         SetScreenSizeAndOrientation();

         // Subscribe to status bar orientation changes -- that's how we determine if orientation has
         // changed. I added a safety check against the status bar frame as well, which will help
         // when the window size changes.
         var notificationCenter = NSNotificationCenter.DefaultCenter;
         notificationCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification,
           ConsiderPossibleSizeOrOrientationChange);
         notificationCenter.AddObserver(UIApplication.DidChangeStatusBarFrameNotification,
           ConsiderPossibleSizeOrOrientationChange);

         UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();

         //// The user is not logged in, but there is an error.
         //if (options.IsNotEmpty())
         //   Device.BeginInvokeOnMainThread
         //   (
         //     async () => { await ProcessNotification(options, false, true); }
         //   );

         return base.FinishedLaunching(app, options);
      }

      //------------------------------------------------------------------------------------------
      //Notifications
      //------------------------------------------------------------------------------------------

      //------------------------------------------------------------------------------------------
      public override void OnActivated(UIApplication app) =>
        // Restart any tasks that were paused (or not yet started) while the application was
        // inactive. If the application was previously in the background, optionally refresh the
        // user interface.
        Debug.WriteLine("IOS: on activated");

      //------------------------------------------------------------------------------------------
      public override void OnResignActivation(UIApplication app)
      {
         // Invoked when the application is about to move from active to inactive state. This can
         // occur for certain types of temporary interruptions (such as an incoming phone call or SMS
         // message) or when the user quits the application and it begins the transition to the
         // background state. Games should use this method to pause the game.
      }

      //------------------------------------------------------------------------------------------
      public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication,
        NSObject annotation) =>
        // Debug.WriteLine( url );
        /* now store the url somewhere in the appâ€™s context. The url is in the url NSUrl object. The data is in url.Host if the link as a scheme as superduperapp://something_interesting */
        false;

      public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
      {
         // Local Notifications are received here
      }

      //------------------------------------------------------------------------------------------
      public override void RegisteredForRemoteNotifications(UIApplication app, NSData deviceToken)
      {
         // var token = deviceToken.Description.Replace("<", "").Replace(">", "").Replace(" ", "");

         // Create the dependency service for the push notification token

         // Application.PUSH_NOTIFICATION_TOKEN = token.IsEmpty() ? string.Empty : token;
      }

      //------------------------------------------------------------------------------------------
      public override void WillEnterForeground(UIApplication app) =>
        // Called as part of the transition from background to active state. Here you can undo many
        // of the changes made on entering the background.
        Debug.WriteLine("IOS: About to enter the foreground");

      #endregion Public Methods

      #region Private Methods

      /// <summary>
      /// Devices the orientation did change.
      /// </summary>
      /// <param name="notification">Notification.</param>
      private void ConsiderPossibleSizeOrOrientationChange(NSNotification notification) => SetScreenSizeAndOrientation();

      private void SetScreenSizeAndOrientation()
      {
         var newScreenWidth = (float)UIScreen.MainScreen.Bounds.Width;
         var newScreenHeight = (float)UIScreen.MainScreen.Bounds.Height;

         if (newScreenHeight.IsDifferentThan(_oldScreenHeight) || newScreenWidth.IsDifferentThan(_oldScreenWidth))
         {
            FormsMessengerUtils.Send(new LocalDeviceSizeChangedMessage(newScreenWidth, newScreenHeight));

            _oldScreenWidth = newScreenWidth;
            _oldScreenHeight = newScreenHeight;
         }
      }

      #endregion Private Methods
   }
}