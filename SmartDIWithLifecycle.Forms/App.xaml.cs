namespace SmartDIWithLifecycle.Forms
{
   using System;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using System.Timers;
   using Com.MarcusTS.LifecycleAware.Common.Interfaces;
   using Com.MarcusTS.LifecycleAware.Common.Utils;
   using Com.MarcusTS.SharedForms.Utils;
   using Com.MarcusTS.SharedUtils.Utils;
   using Xamarin.Forms;

   public partial class App : Application, IManagePageChanges, IReportAppLifecycle
   {
      #region Private Fields

      public static readonly double DELAY_BETWEEN_BROADCASTS = 1000;

      private static readonly TimeSpan TOTAL_BROADCAST_TIME = TimeSpan.FromSeconds(30);

      private Timer _timer;

      #endregion Private Fields

      #region Public Constructors

      public App()
      {
         InitializeComponent();

         // Required by IOS
         MainPage = new ContentPage();

         StartFirstTest();
      }

      #endregion Public Constructors

      #region Public Events

      public event EventUtils.NoParamsDelegate AppIsGoingToSleep;

      public event EventUtils.NoParamsDelegate AppIsResuming;

      public event EventUtils.NoParamsDelegate AppIsStarting;

      #endregion Public Events

      #region Private Events

      private event EventUtils.NoParamsDelegate TestCompleted;

      #endregion Private Events

      #region Public Methods

      public void SetMainPage(Page newPage)
      {
         try
         {
            MainPage = newPage;
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }

      #endregion Public Methods

      #region Protected Methods

      protected override void OnResume() =>
         // Handle when your app resumes
         AppIsResuming?.Invoke();

      protected override void OnSleep() =>
         // Handle when your app sleeps
         AppIsGoingToSleep?.Invoke();

      protected override void OnStart() =>
         // Handle when your app starts
         AppIsStarting?.Invoke();

      #endregion Protected Methods

      #region Private Methods

      private SecondViewModel SetUpTest()
      {
         var secondViewModel = new SecondViewModel
         {
            TimeRemaining = TOTAL_BROADCAST_TIME
         };

         _timer = new Timer(DELAY_BETWEEN_BROADCASTS);
         var timeToStop = DateTime.Now + TOTAL_BROADCAST_TIME;
         _timer.Elapsed += (sender, args) =>
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

               _timer.Stop();
               _timer.Dispose();

               TestCompleted?.Invoke();
            }
         };

         _timer.Start();
         return secondViewModel;
      }

      private void StartFirstTest()
      {
         TestCompleted += StartSecondTest;

         Device.BeginInvokeOnMainThread(async () =>
         {
            var firstViewModel = new FirstViewModel();
            var secondViewModel = SetUpTest();
            firstViewModel.DebugOutputChanged += secondViewModel.HandleFirstViewModelDebugOutputChanged;
            var firstPage = new FirstPage { BindingContext = firstViewModel };
            Debug.WriteLine("About to assign the main page to the first page.");
            SetMainPage(firstPage);

            Debug.WriteLine("Finished assigning the main page to the first page.");
            await Task.Delay(5000);
            Debug.WriteLine("About to assign the main page to the second page.");
            SetMainPage(new SecondPage { BindingContext = secondViewModel });

            secondViewModel.Message = "Finished assigning the main page to the second page.";
            Debug.WriteLine(secondViewModel.Message);
            secondViewModel.Message = "The first view model is now OUT OF SCOPE and should not be active.";
            Debug.WriteLine(secondViewModel.Message);
            GC.Collect();
         });
      }

      private void StartSecondTest()
      {
         TestCompleted -= StartSecondTest;

         Device.BeginInvokeOnMainThread(async () =>
         {
            var firstViewModelWithLifecycle = new FirstViewModelWithLifecycle();
            var secondViewModel = SetUpTest();
            firstViewModelWithLifecycle.DebugOutputChanged += secondViewModel.HandleFirstViewModelDebugOutputChanged;
            var firstPageWithLifecycle = new FirstPageWithLifecycle { BindingContext = firstViewModelWithLifecycle };

            // No longer needed
            // firstViewModelWithLifecycle.PageLifecycleReporter = firstPageWithLifecycle;

            Debug.WriteLine("About to assign the main page to the first page with Lifecycle.");
            SetMainPage(firstPageWithLifecycle);

            Debug.WriteLine("Finished assigning the main page to the first page with Lifecycle.");
            await Task.Delay(5000);
            Debug.WriteLine("About to assign the main page to the second page.");
            SetMainPage(new SecondPage { BindingContext = secondViewModel });

            secondViewModel.Message = "Finished assigning the main page to the second page.";
            Debug.WriteLine(secondViewModel.Message);
            secondViewModel.Message = "The first view model is now OUT OF SCOPE and should not be active.";
            Debug.WriteLine(secondViewModel.Message);
         });
      }

      #endregion Private Methods
   }
}