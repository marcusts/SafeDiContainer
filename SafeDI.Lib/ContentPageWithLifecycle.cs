﻿#region License
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

namespace SafeDI.Lib
{
   using System;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;

   /// <remarks>
   /// WARNING: .Net does not provide IContentpage, so references to this interface type *must* type-cast to ContentPage_LCA manually.
   /// </remarks>
   public interface IContentPageWithLifecycle : IReportPageLifecycle
   {
   }

   public class ContentPageWithLifecycle : ContentPage, IContentPageWithLifecycle
   {
      protected override void OnAppearing()
      {
         base.OnAppearing();

         PageIsAppearing?.Invoke(this);
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         PageIsDisappearing?.Invoke(this);
      }

      public event EventUtils.GenericDelegate<ContentPage> PageIsAppearing;
      public event EventUtils.GenericDelegate<ContentPage> PageIsDisappearing;
   }
}
