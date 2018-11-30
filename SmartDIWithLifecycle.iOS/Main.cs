using UIKit;

namespace SmartDIWithLifecycle.iOS
{
   public class Application
   {
      #region Private Methods

      // This is the main entry point of the application.
      private static void Main(string[] args) =>
         // if you want to use a different Application Delegate class from "AppDelegate"
         // you can specify it here.
         UIApplication.Main(args, null, "AppDelegate");

      #endregion Private Methods
   }
}