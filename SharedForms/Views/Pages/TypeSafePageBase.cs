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

namespace SharedForms.Views.Pages
{
   #region Imports

   using Common.Interfaces;
   using Common.Utils;
   using System;
   using System.Diagnostics;
   using Xamarin.Forms;

   #endregion Imports

   /// <summary>
   /// A base class for content pages that protects the type safety of the binding context.
   /// </summary>
   /// <typeparam name="InterfaceT">The required interface for this view.</typeparam>
   /// <remarks>
   /// This code is similar to that at <see cref="TypeSafeViewBase{InterfaceT}"/> except it manages a
   /// page rather than a view.
   /// </remarks>
   public abstract class TypeSafePageBase<InterfaceT> : ContentPage, ITypeSafePageBase
      where InterfaceT : class
   {
      #region Private Variables

      private readonly RelativeLayout _contentRelativeLayout = FormsUtils.GetExpandingRelativeLayout();

      #endregion Private Variables

      #region Protected Constructors

      protected TypeSafePageBase()
      {
         NavigationPage.SetHasNavigationBar(this, false);
         BackgroundColor = Color.Transparent;

         ConstructTypeSafePageView();
      }

      #endregion Protected Constructors

      #region Private Methods

      /// <summary>
      /// We create an "is busy" view by default so it is always available. We insert the deriver's
      /// content below this is busy view.
      /// </summary>
      private void ConstructTypeSafePageView()
      {
         FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.BeforeConstructing));

         try
         {
            var derivedView = ConstructPageView();

            _contentRelativeLayout.CreateRelativeOverlay(derivedView);

            Content = _contentRelativeLayout;

            // Notify derivers of this final step
            AfterContentSet(_contentRelativeLayout);
         }
         catch (Exception ex)
         {
            Debug.WriteLine("TYPE SAFE PAGE BASE: ConstructTypeSafePageView: ERROR ->" + ex.Message + "<-");
         }
         finally
         {
            FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.AfterConstructing));
         }
      }

      #endregion Private Methods

      #region Public Properties

      /// <summary>
      /// InterfaceT is cast to the base to make it type-safe.
      /// NOTE: This hides the base BindingContext.
      /// </summary>
      public new InterfaceT BindingContext
      {
         get => base.BindingContext as InterfaceT;
         set => base.BindingContext = value;
      }

      public Func<object> GetEventBroadcaster => () => this;

      #endregion Public Properties

      #region Protected Methods

      protected virtual void AfterContentSet(RelativeLayout layout)
      {
      }

      /// <summary>
      /// Requests that the deriver create the physical view.
      /// </summary>
      /// <returns></returns>
      protected abstract View ConstructPageView();

      /// <summary>
      /// Called when the page appears.
      /// </summary>
      protected override void OnAppearing()
      {
         FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.BeforeAppearing));

         base.OnAppearing();

         FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.AfterAppearing));
      }

      protected override void OnDisappearing()
      {
         FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.BeforeDisappearing));

         base.OnDisappearing();

         FormsMessengerUtils.Send(new PageLifecycleMessage(this, PageLifecycleEvents.AfterDisappearing));
      }

      #endregion Protected Methods
   }

   /// <remarks>
   /// WARNING: .Net does not provide an IContentPage interface, so we cannot reference the page from
   ///          this interface without a hard cast!
   /// </remarks>
   public interface ITypeSafePageBase : IProvidePageEvents
   {
   }
}
