#region License

// MIT License
//
// Copyright (c) 2018 Marcus Technical Services, Inc. http://www.marcusts.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion License

namespace SharedForms.Views.SubViews
{
   #region Imports

   using Autofac;
   using Common.Navigation;
   using Common.Utils;
   using Controls;
   using Pages;
   using PropertyChanged;
   using System;
   using System.Linq;
   using ViewModels;
   using Xamarin.Forms;

   #endregion Imports

   [AddINotifyPropertyChangedInterface]
   public class NavAndMenuBar : ContentView, INavAndMenuBar
   {
     #region Public Properties

     public Page HostingPage
     {
       get => _hostingPage;
       set
       {
         // Remove the old event handler, if any.
         RemoveHostingPageBindingContextChangedHandler();

         _hostingPage = value;

         _hostingPage.BindingContextChanged += OnHostingPageBindingContextChanged;

         // Add the left-side back and right-side hamburger
         var grid = FormsUtils.GetExpandingGrid();
         grid.HeightRequest = OVERALL_HEIGHT;
         grid.VerticalOptions = LayoutOptions.CenterAndExpand;
         grid.AddStarRow();
         grid.AddFixedColumn(15);
         grid.AddFixedColumn(OVERALL_HEIGHT);
         grid.AddStarColumn();
         grid.AddFixedColumn(OVERALL_HEIGHT);
         grid.AddFixedColumn(15);

         // Wire into navigation, if available
         if (_hostingPage is IMenuNavPageBase)
         {
            _backButton = CreateNavBarButton(BACK_IMAGE, BackButtonTapped);
            _backButton.BindingContext = this;
            _backButton.HorizontalOptions = LayoutOptions.Start;
            grid.Children.Add(_backButton, 1, 0);

            // Initial setting
            SetBackButtonVisiblity();

            // Add the menu if that is allowed.
            _menuButton = CreateNavBarButton(HAMBURGER_IMAGE, MenuButtonTapped);
            _menuButton.HorizontalOptions = LayoutOptions.End;
            _menuButton.SetUpBinding(IsVisibleProperty, nameof(IsMenuLoaded));
            grid.Children.Add(_menuButton, 3, 0);

            // Bind the title, center with margins and overlay Currently depends on the page being navigable
            _titleLabel =
              FormsUtils.GetSimpleLabel
              (
                textColor: Color.White,
                fontNamedSize: NamedSize.Large,
                fontAttributes: FontAttributes.Bold,
                textAlignment: TextAlignment.Start,
                labelBindingPropertyName: nameof(IPageViewModelBase.PageTitle)
              );
            _titleLabel.BackgroundColor = Color.Transparent;
            _titleLabel.HorizontalOptions = LayoutOptions.Center;
            _titleLabel.LineBreakMode = LineBreakMode.WordWrap;

            grid.Children.Add(_titleLabel, 2, 0);
         }

         Content = grid;

         // In case the binding context already exists
         SetUpHostingBindingContexts();
       }
     }

     #endregion Public Properties

     #region Public Methods

     public void Dispose()
     {
       ReleaseUnmanagedResources();
       GC.SuppressFinalize(this);
     }

     #endregion Public Methods

     #region Public Events

     public static event EventUtils.NoParamsDelegate AskToSetBackButtonVisiblity;

     #endregion Public Events

     #region Private Destructors

     ~NavAndMenuBar()
     {
       ReleaseUnmanagedResources();
     }

     #endregion Private Destructors

     #region Public Variables

     public static readonly BindableProperty HostingPageProperty =
       BindableProperty.Create(nameof(HostingPage), typeof(Page), typeof(NavAndMenuBar), default(Page),
         propertyChanged: OnHostingPageChanged);

     public static readonly double OVERALL_HEIGHT = 45.0;

     #endregion Public Variables

     #region Private Variables

     private const string BACK_IMAGE = "left_arrow_with_shadow_512.png";

     private const string HAMBURGER_IMAGE = "hamburger_with_shadow_512.png";

     private static readonly FlexibleStack<string> _appStateBackButtonStack = new FlexibleStack<string>();

     private static readonly double BUTTON_HEIGHT = 30.0;

     private static readonly Thickness IOS_MARGIN = new Thickness(0, 20, 0, 0);

     private static IStateMachineBase _stateMachine;

     private Image _backButton;

     private Page _hostingPage;

     private Image _menuButton;

     private bool _menuButtonEntered;

     private Label _titleLabel;

     #endregion Private Variables

     #region Public Constructors

     /// <remarks>Not used in the run-time app but can be called for unit testing.</remarks>
     public NavAndMenuBar(IStateMachineBase stateMachine)
     {
       _stateMachine = stateMachine;

       BackgroundColor = ColorUtils.HEADER_AND_TOOLBAR_COLOR_DEEP;

       if (Device.RuntimePlatform.IsSameAs(Device.iOS))
       {
         Margin = IOS_MARGIN;
       }

       // Listen for the static page change
       AskToSetBackButtonVisiblity += SetBackButtonVisiblity;

       // These message are subscribed but never unsubscribed. The menu is global static, so
       // persists throughout the life of the app. There is no reason to unsubscribe them under
       // these circumstances.
       FormsMessengerUtils.Subscribe<MenuLoadedMessage>(this, OnMenuLoaded);
       FormsMessengerUtils.Subscribe<AppStateChangedMessage>(this, OnAppStateChanged);
     }

     /// <remarks>
     /// Must be parameterless due to the XAML page control template at app.xaml. This defeats the
     /// flexibility of menu unit testing, as the injected dependency is hard-coded below.
     /// </remarks>
     public NavAndMenuBar() :
       //this(AppContainer.GlobalVariableContainer.Resolve<IStateMachineBase>())
       this(null)
     {
     }

     #endregion Public Constructors

     #region Private Properties

     private bool IsMenuLoaded { get; set; }

     private bool IsNavigationAvailable => _appStateBackButtonStack.IsNotEmpty();

     #endregion Private Properties

     #region Private Methods

     private static void OnAppStateChanged(object sender, AppStateChangedMessage appStateChangedMessage)
     {
       // If no old state, do nothing
       if (appStateChangedMessage.Payload.OldAppState.IsEmpty())
       {
         // Wipe out the stack and restart
         _appStateBackButtonStack.Clear();
         return;
       }

       // Get rid of the old app state -- it might be disorganized in the stack
       _appStateBackButtonStack.RemoveIfPresent
       (
         appStateChangedMessage.Payload.OldAppState,
         appState => appState.IsSameAs(appStateChangedMessage.Payload.OldAppState)
       );

       // Push the old app state to the top of the stack so it so navigation makes sense
       if (!appStateChangedMessage.Payload.PreventNavStackPush)
       {
         _appStateBackButtonStack.Push(appStateChangedMessage.Payload.OldAppState);
       }

       // Reset the back button as needed
       AskToSetBackButtonVisiblity?.Invoke();
     }

     private static void OnHostingPageChanged(BindableObject bindable, object oldvalue, object newvalue)
     {
       if (bindable is NavAndMenuBar bindableAsNavAndMenuBar)
       {
         bindableAsNavAndMenuBar.HostingPage = newvalue as Page;
       }
     }

     private static void RemoveButtonTappedListeners(Image imageButton, EventHandler buttonTapped)
     {
       if (imageButton.GestureRecognizers.IsNotEmpty())
       {
         var tappableGesture = imageButton.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
         if (tappableGesture != null)
         {
            tappableGesture.Tapped -= buttonTapped;
         }
       }
     }

     private void BackButtonTapped(object sender, EventArgs eventArgs)
     {
       // Navigate back if possible --
       // the button will be disabled if we cannot go back
       if (IsNavigationAvailable)
       {
         // Remove the top app state the stack
         var nextAppState = _appStateBackButtonStack.Pop();

         // Get the app state; Do not add to the back stack, since we are going backwards
         _stateMachine.GoToAppState(nextAppState, true);
       }

       SetBackButtonVisiblity();
     }

     private static Image CreateNavBarButton(string imagePath, EventHandler menuButtonTapped)
     {
       var retImage = FormsUtils.GetImage(imagePath, height: BUTTON_HEIGHT);

       var imageTap = new TapGestureRecognizer();
       imageTap.Tapped += menuButtonTapped;
       retImage.GestureRecognizers.Add(imageTap);
       retImage.VerticalOptions = LayoutOptions.Center;

       return retImage;
     }

     private void MenuButtonTapped(object sender, EventArgs e)
     {
       if (_menuButtonEntered)
       {
         return;
       }

       _menuButtonEntered = true;

       // Notify the host page so it can close the menu. Ask to close the menu as if the user
       // tapped the hamburger icon.
       FormsMessengerUtils.Send(new NavBarMenuTappedMessage());

       _menuButtonEntered = false;
     }

     private void OnHostingPageBindingContextChanged(object sender, EventArgs e)
     {
       SetUpHostingBindingContexts();
     }

     private void OnMenuLoaded(object sender, MenuLoadedMessage args)
     {
       IsMenuLoaded = true;
     }

     private void ReleaseUnmanagedResources()
     {
       AskToSetBackButtonVisiblity -= SetBackButtonVisiblity;

       RemoveButtonTappedListeners(_backButton, BackButtonTapped);

       RemoveButtonTappedListeners(_menuButton, MenuButtonTapped);
     }

     private void RemoveHostingPageBindingContextChangedHandler()
     {
       if (_hostingPage != null)
       {
         _hostingPage.BindingContextChanged -= OnHostingPageBindingContextChanged;
       }
     }

     private void SetBackButtonVisiblity()
     {
       if (_backButton == null)
       {
         return;
       }

       _backButton.IsVisible = IsNavigationAvailable;
     }

     private void SetUpHostingBindingContexts()
     {
       BindingContext = _hostingPage.BindingContext;
       _menuButton.BindingContext = this;
       _titleLabel.BindingContext = _hostingPage.BindingContext;
     }

     #endregion Private Methods
   }

   public interface INavAndMenuBar : IDisposable
   {
     #region Public Properties

     Page HostingPage { get; set; }

     #endregion Public Properties
   }
}
