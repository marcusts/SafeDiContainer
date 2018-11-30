namespace SmartDIWithLifecycle.Forms
{
   using Xamarin.Forms;

   public partial class App : Application
   {
      #region Public Constructors

      public App()
      {
         InitializeComponent();

         // MainPage = new SmartDIWithLifecycle.MainPage();
         MainPage = new ContentPage();
      }

      #endregion Public Constructors

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