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

namespace SharedForms.Common.Utils
{
   #region Imports

   using Views.Controls;
   using Xamarin.Forms;

   #endregion Imports

   public static class ViewUtils
   {
      #region Private Variables

      private const double NORMAL_BUTTON_FONT_SIZE = 20;

      private const double SELECTED_BUTTON_FONT_SIZE = NORMAL_BUTTON_FONT_SIZE * 1.1;

      // Xamarin bug -- calculation is pristine, but the pages do not align properly on-screen
      private const double SLOP = 6;

      private static readonly double SELECTED_IMAGE_BUTTON_BORDER_WIDTH = 2;

      #endregion Private Variables

      #region Public Methods

      public static void ClearCompletely(this Grid grid)
      {
         grid.Children.Clear();
         grid.ColumnDefinitions.Clear();
         grid.RowDefinitions.Clear();
      }

      public static double GetAdjustedScreenWidth(int viewIdx, double currentWidth)
      {
         var properX = -(viewIdx * currentWidth);
         var sloppyX = properX - viewIdx * SLOP;

         return sloppyX;
      }

      public static void SetBorderSelectionStyles(this ImageButton retButton)
      {
         // No disabled image treatment as of yet

         retButton.DeselectedButtonStyle = ImageButton.CreateViewButtonStyle(Color.Transparent);
         retButton.SelectedButtonStyle =
            ImageButton.CreateViewButtonStyle(Color.Transparent, SELECTED_IMAGE_BUTTON_BORDER_WIDTH);
         retButton.DisabledButtonStyle = ImageButton.CreateViewButtonStyle(Color.Transparent);
      }

      public static void SetReverseStyles(this LabelButton retButton)
      {
         retButton.DeselectedLabelStyle =
            LabelButton.CreateLabelStyle(Color.Black, NORMAL_BUTTON_FONT_SIZE, FontAttributes.None);
         retButton.SelectedLabelStyle =
            LabelButton.CreateLabelStyle(Color.White, SELECTED_BUTTON_FONT_SIZE, FontAttributes.Bold);
         retButton.DisabledLabelStyle =
            LabelButton.CreateLabelStyle(Color.Gray, NORMAL_BUTTON_FONT_SIZE, FontAttributes.None);

         retButton.DeselectedButtonStyle = LabelButton.CreateViewButtonStyle(Color.Transparent);
         retButton.SelectedButtonStyle = LabelButton.CreateViewButtonStyle(Color.Black);
         retButton.DisabledButtonStyle = LabelButton.CreateViewButtonStyle(Color.Transparent);
      }

      #endregion Public Methods
   }
}
