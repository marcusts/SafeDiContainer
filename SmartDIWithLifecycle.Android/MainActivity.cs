using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SmartDIWithLifecycle.Android
{
   using Forms;

   [Activity(Label = nameof(SmartDIWithLifecycle), Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
   public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
   {
      #region Protected Methods

      protected override void OnCreate(Bundle bundle)
      {
         TabLayoutResource = Resource.Layout.Tabbar;
         ToolbarResource = Resource.Layout.Toolbar;

         base.OnCreate(bundle);

         global::Xamarin.Forms.Forms.Init(this, bundle);
         LoadApplication(new App());
      }

      #endregion Protected Methods
   }
}