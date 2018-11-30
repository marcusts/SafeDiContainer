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

namespace SharedForms.Views.SubViews
{
   using Common.Interfaces;
   using Common.Navigation;
   using Common.Utils;
   using Xamarin.Forms;

   public interface IMainMenu
   {
      bool IsMenuLoaded { get; }

      double MenuHeight { get; }
   }

   public class MainMenu : ContentView, IMainMenu
   {
      public static readonly double MENU_GROSS_WIDTH = MENU_ITEM_WIDTH + 2 * MENU_OUTSIDE_SINGLE_MARGIN;
      public static readonly double MENU_INSIDE_SINGLE_MARGIN = MENU_OUTSIDE_SINGLE_MARGIN / 2;
      public static readonly double MENU_ITEM_WIDTH = 120.0;
      public static readonly Thickness MENU_OUTSIDE_MARGIN = new Thickness(MENU_OUTSIDE_SINGLE_MARGIN);
      public static readonly double MENU_OUTSIDE_SINGLE_MARGIN = 15.0;

      private const bool ALLOW_EVENT_TUNNELING = false;

      private static readonly double MAIN_MENU_OPACITY = 0.95;

      private static readonly double MENU_ITEM_HEIGHT = 40.0;

      private readonly IStateMachineBase _stateMachine;

      private bool _isMenuLoaded;

      public MainMenu(IStateMachineBase stateMachine)
      {
         _stateMachine = stateMachine;

         // Not really used
         BindingContext = this;

         VerticalOptions = LayoutOptions.StartAndExpand;
         HorizontalOptions = LayoutOptions.CenterAndExpand;

         BackgroundColor = ColorUtils.HEADER_AND_TOOLBAR_COLOR;
         Opacity = MAIN_MENU_OPACITY;

         InputTransparent = ALLOW_EVENT_TUNNELING;

         LoadMenuFromStateMachine();
      }

      public bool IsMenuLoaded
      {
         get => _isMenuLoaded;
         private set
         {
            _isMenuLoaded = value;

            FormsMessengerUtils.Send(new MenuLoadedMessage());
         }
      }

      public double MenuHeight { get; private set; }

      private Button CreateMenuItemButton(IMenuNavigationState menuData)
      {
         var retButton =
           new Button
           {
              Text = menuData.MenuTitle,
              WidthRequest = MENU_ITEM_WIDTH,
              HeightRequest = MENU_ITEM_HEIGHT,
              HorizontalOptions = LayoutOptions.Center,
              VerticalOptions = LayoutOptions.Center,
              InputTransparent = ALLOW_EVENT_TUNNELING
           };

         retButton.Clicked += (s, e) =>
         {
            // Ask to close the menu as if the user tapped the hamburger icon.
            FormsMessengerUtils.Send(new NavBarMenuTappedMessage());

            _stateMachine.GoToAppState(menuData.AppState, false);
         };

         return retButton;
      }

      private void LoadMenuFromStateMachine()
      {
         // A grid to handle the entire menu
         var menuStack = FormsUtils.GetExpandingStackLayout();
         menuStack.VerticalOptions = LayoutOptions.StartAndExpand;
         menuStack.HorizontalOptions = LayoutOptions.CenterAndExpand;
         menuStack.Margin = MENU_OUTSIDE_MARGIN;
         menuStack.Spacing = MENU_INSIDE_SINGLE_MARGIN;
         menuStack.InputTransparent = ALLOW_EVENT_TUNNELING;

         var singleMenuItemHeight = MENU_ITEM_HEIGHT + MENU_INSIDE_SINGLE_MARGIN;

         // Allow for the top and bottom margins, etc.
         MenuHeight = 2 * MENU_OUTSIDE_SINGLE_MARGIN;

         foreach (var menuData in _stateMachine.MenuItems)
         {
            menuStack.Children.Add(CreateMenuItemButton(menuData));
            MenuHeight += singleMenuItemHeight;
         }

         HeightRequest = MenuHeight;
         WidthRequest = MENU_GROSS_WIDTH;

         var scroller = FormsUtils.GetExpandingScrollView();
         scroller.InputTransparent = ALLOW_EVENT_TUNNELING;
         scroller.Content = menuStack;

         Content = scroller;

         IsMenuLoaded = true;
      }
   }
}