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

namespace SharedForms.Views.Pages
{
   using Common.Interfaces;
   using Common.Utils;
   using Xamarin.Forms;

   /// <remarks>
   /// WARNING: .Net does not provide an IContentView interface, so we cannot reference the view from
   ///        this interface without a hard cast!
   /// </remarks>
   public interface ITypeSafeViewBase : IReceivePageEvents
   {
   }

   /// <summary>
   /// A base class for content views that protects the type safety of the binding context.
   /// </summary>
   /// <remarks>
   /// This code is similar to that at <see cref="TypeSafePageBase{InterfaceT}"/> except it manages a
   /// view rather than a page.
   /// </remarks>
   /// <typeparam name="InterfaceT">The required interface for this view.</typeparam>
   public abstract class TypeSafeViewBase<InterfaceT> : ContentView, ITypeSafeViewBase
     where InterfaceT : class
   {
      #region Private Variables

      private readonly RelativeLayout _contentRelativeLayout = FormsUtils.GetExpandingRelativeLayout();

      private IProvidePageEvents _pageEventProvider;

      #endregion Private Variables

      #region Protected Constructors

      protected TypeSafeViewBase(IProvidePageEvents pageEventProvider = null)
      {
         PageEventProvider = pageEventProvider;
         BackgroundColor = Color.Transparent;

         // Resharper doesn't like the derived methods in the constructor, but there's not much we can do about it.
         // We could move this to the page events and catch the page OnAppearing, but if our page event provider is null, that will not occur.
         // ReSharper disable once VirtualMemberCallInConstructor
#pragma warning disable CC0067 // Virtual Method Called On Constructor
         Content = ConstructView();
#pragma warning restore CC0067 // Virtual Method Called On Constructor
      }

      #endregion Protected Constructors

      #region Public Properties

      /// <summary>
      /// T is normally an interface -- not a class -- but there is no such constraint available.
      /// </summary>
      public new InterfaceT BindingContext
      {
         get => base.BindingContext as InterfaceT;
         set => base.BindingContext = value;
      }

      public IProvidePageEvents PageEventProvider
      {
         get => _pageEventProvider;
         set
         {
            _pageEventProvider = value;

            if (_pageEventProvider == null)
            {
               RemovePageProviderListeners();
            }
            else
            {
               AddPageProviderListeners();
            }
         }
      }

      #endregion Public Properties

      #region Protected Methods

      protected virtual void AfterContentSet(RelativeLayout layout)
      {
      }

      /// <summary>
      /// Requests that the deriver create the physical view.
      /// </summary>
      /// <returns></returns>
      protected virtual View ConstructView() => new ContentView();

      protected virtual void OnPageLifecycleChange(PageLifecycleEvents pageEvent)
      {
      }

      #endregion Protected Methods

      #region Private Methods

      private void AddPageProviderListeners()
      {
         if (PageEventProvider == null)
         {
            return;
         }

         FormsMessengerUtils.Subscribe<PageLifecycleMessage>(this, HandlePageLifeCycleChange);
      }

      private void HandlePageLifeCycleChange(object sender, PageLifecycleMessage args)
      {
         // Must verify that the sender is our page lifecycle broadcaster; it could belong to someone else.
         var ourBroadcaster = PageEventProvider?.GetEventBroadcaster?.Invoke();

         if (args.Payload.SendingPage == null || ourBroadcaster == null ||
            !ReferenceEquals(ourBroadcaster, args.Payload.SendingPage))
         {
            return;
         }

         // Call the protected virtual method so derivers can manage the event
         OnPageLifecycleChange(args.Payload.PageEvent);
      }

      private void RemovePageProviderListeners() => FormsMessengerUtils.Unsubscribe<PageLifecycleMessage>(this);

      #endregion Private Methods
   }
}