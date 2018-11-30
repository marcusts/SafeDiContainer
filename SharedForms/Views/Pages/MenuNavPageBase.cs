#region License

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

// #define STORE_PAGE_MENU_STATICALLY

namespace SharedForms.Views.Pages
{
   #region Imports

   using System.Threading.Tasks;
   using Autofac;
   using Common.DeviceServices;
   using Common.Navigation;
   using Common.Utils;
   using PropertyChanged;
   using SubViews;
   using ViewModels;
   using Xamarin.Forms;

   #endregion

   public interface IMenuNavPageBase : ITypeSafePageBase
   {
     #region Public Methods

     void RemoveMenuFromLayout();

     #endregion Public Methods
   }

   /// <summary>
   ///   A page with a navigation header.
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public abstract class MenuNavPageBase<InterfaceT> : TypeSafePageBase<InterfaceT>, IMenuNavPageBase
     where InterfaceT : class, IPageViewModelBase
   {
     #region Public Constructors

#if STORE_PAGE_MENU_STATICALLY
     static MenuNavPageBase()
     {
       var stateMachine = AppContainer.GlobalVariableContainer.Resolve<IStateMachineBase>();
       PageMenu = new MainMenu(stateMachine);
     }
#endif

     #endregion Public Constructors

     #region Protected Constructors

     protected MenuNavPageBase()
     {
       // Do not use "BeginLifetimeScope" because it does not seem to work. Also, the menu is
       // global for the life of the app.
#if !STORE_PAGE_MENU_STATICALLY
       // PageMenu = AppContainer.GlobalVariableContainer.Resolve<IMainMenu>();
       PageMenu = new MainMenu(null);
#endif

       FormsMessengerUtils.Subscribe<NavBarMenuTappedMessage>(this, OnMainMenuItemSelected);

       BackgroundColor = Color.Transparent;

       PageMenuView.Opacity = 0;

       var controlTemplateNotSet = true;

       BindingContextChanged +=
         (sender, args) =>
         {
            if (controlTemplateNotSet)
            {
              ControlTemplate = Application.Current.Resources[NAV_BAR_CONTROL_TEMPLATE] as ControlTemplate;
              controlTemplateNotSet = false;
            }
         };

       _canvas.InputTransparent = true;
     }

     #endregion Protected Constructors

     #region Public Methods

     public void RemoveMenuFromLayout()
     {
       if (_canvas != null && _canvas.Children.Contains(PageMenuView))
       {
         _canvas.Children.Remove(PageMenuView);
       }
     }

     #endregion Public Methods

     #region Private Variables

     private const int MENU_ANIMATE_MILLISECONDS = 400;

     private const int MENU_FADE_MILLISECONDS = 200;

     private const string NAV_BAR_CONTROL_TEMPLATE = "NavBarControlTemplate";

     private readonly AbsoluteLayout _canvas = FormsUtils.GetExpandingAbsoluteLayout();

     private volatile bool _isPageMenuShowing;

     #endregion Private Variables

     #region Protected Properties

     protected
#if STORE_PAGE_MENU_STATICALLY
       static
#endif
       IMainMenu PageMenu { get; set; }

     protected bool IsPageMenuShowing
     {
       get => _isPageMenuShowing;
       set
       {
         Device.BeginInvokeOnMainThread
         (
            async () =>
            {
              _isPageMenuShowing = value;

              await AnimatePageMenu().WithoutChangingContext();

              // HACK to fix dead UI in the main menu, which sits on top of this canvas
              _canvas.InputTransparent = !_isPageMenuShowing;
            }
         );
       }
     }

     protected View PageMenuView => PageMenu as View;

     #endregion Protected Properties

     #region Protected Methods

     protected override void AfterContentSet(RelativeLayout layout)
     {
       // No need to add it twice
       if (_canvas == null || _canvas.Children.Contains(PageMenuView))
       {
         return;
       }

       PageMenuView.Opacity = 0;

       var targetRect = CreateOfflineRectangle();

       layout.CreateRelativeOverlay(_canvas);

       // A slight cheat; using protected property
       _canvas.Children.Add(PageMenuView, targetRect);
     }

     protected override void OnDisappearing()
     {
       base.OnDisappearing();

       FormsMessengerUtils.Unsubscribe<NavBarMenuTappedMessage>(this);

       RemoveMenuFromLayout();
     }

     #endregion Protected Methods

     #region Private Methods

     private Rectangle CreateOfflineRectangle()
     {
       return new Rectangle(OrientationService.ScreenWidth, 0, 0, PageMenu.MenuHeight);
     }

     /// <summary>
     ///   Animates the panel in our out depending on the state
     /// </summary>
     private async Task AnimatePageMenu()
     {
       if (IsPageMenuShowing)
       {
         // Slide the menu up from the bottom
         var rect =
            new Rectangle
            (
              Width - MainMenu.MENU_GROSS_WIDTH,
              0,
              MainMenu.MENU_GROSS_WIDTH,
              PageMenu.MenuHeight
            );

         await Task.WhenAll(PageMenuView.LayoutTo(rect, MENU_ANIMATE_MILLISECONDS, Easing.CubicIn),
            PageMenuView.FadeTo(1.0, MENU_FADE_MILLISECONDS)).WithoutChangingContext();
       }
       else
       {
         // Retract the menu
         var rect = CreateOfflineRectangle();

         await Task.WhenAll(PageMenuView.LayoutTo(rect, MENU_ANIMATE_MILLISECONDS, Easing.CubicOut),
            PageMenuView.FadeTo(0.0, MENU_FADE_MILLISECONDS)).WithoutChangingContext();
       }
     }

     private void OnMainMenuItemSelected(object sender, NavBarMenuTappedMessage args)
     {
       // Close the menu upon selection
       IsPageMenuShowing = !IsPageMenuShowing;
     }

     #endregion Private Methods
   }
}