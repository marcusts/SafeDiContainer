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
using Bundle = Android.OS.Bundle;
using ConfigChanges = Android.Content.PM.ConfigChanges;
using Configuration = Android.Content.Res.Configuration;

namespace LifecycleAware.Android
{
   using Forms;
   using global::Android.App;
   using SharedForms.Common.DeviceServices;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;
   using Xamarin.Forms.Platform.Android;

   [Activity(Label = nameof(LifecycleAware), Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true,
     ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
   public class MainActivity : FormsAppCompatActivity
   {
      #region Private Fields

      //------------------------------------------------------------------------------------------
      private float _oldScreenHeight;

      private float _oldScreenWidth;

      #endregion Private Fields

      #region Public Methods

      //------------------------------------------------------------------------------------------
      public override void OnConfigurationChanged(Configuration newConfig)
      {
         base.OnConfigurationChanged(newConfig);

         SetScreenSizeAndOrientation();
      }

      #endregion Public Methods

      #region Protected Methods

      //------------------------------------------------------------------------------------------
      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         Forms.Init(this, bundle);

         LoadApplication(new App());

         // Assign the OrientationService's event handler to listen to our orientation service's changes
         FormsMessengerUtils.Subscribe<LocalDeviceSizeChangedMessage>(this, OrientationService.HandleDeviceSizeChanged);

         SetScreenSizeAndOrientation();

         if (IsPlayServicesAvailable())
         {
            // TODO FirebaseIIDService.RefreshToken();
         }
      }

      protected override void OnDestroy()
      {
         base.OnDestroy();

         // Remove the device size changed subscription
         FormsMessengerUtils.Unsubscribe<LocalDeviceSizeChangedMessage>(this);
      }

      #endregion Protected Methods

      #region Private Methods

      //------------------------------------------------------------------------------------------
      private static bool IsPlayServicesAvailable() =>
       /*
var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);

if (resultCode != ConnectionResult.Success)
{
if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
{
DialogFactory.ShowErrorToast(GoogleApiAvailability.Instance.GetErrorString(resultCode),
useTimeout: true);
}
else
{
DialogFactory.ShowErrorToast("This device is not supported", useTimeout: true);
Finish();
}
return false;
}
*/

       true;

      private void SetScreenSizeAndOrientation()
      {
         var newScreenWidth = Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density;
         var newScreenHeight = Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density;

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