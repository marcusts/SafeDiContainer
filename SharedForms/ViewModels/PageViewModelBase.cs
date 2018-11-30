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

namespace SharedForms.ViewModels
{
   #region Imports

   using Common.Interfaces;
   using Common.Navigation;
   using Common.Utils;
   using PropertyChanged;

   #endregion Imports

   [AddINotifyPropertyChangedInterface]
   [DoNotNotify]
   public abstract class PageViewModelBase : IPageViewModelBase
   {
     #region Protected Variables

     protected readonly IStateMachineBase Machine;

     #endregion Protected Variables

     #region Protected Constructors

     protected PageViewModelBase(IStateMachineBase stateMachine, IProvidePageEvents pageEventProvider = null)
     {
       // Request the global interface type so the code is more share-able.
       Machine = stateMachine;

       // Also share the page event provider so that derivers know about OnAppearing,
       // OnDisappearing, etc.
       PageEventProvider = pageEventProvider;

       if (PageEventProvider?.GetEventBroadcaster != null)
       {
         FormsMessengerUtils.Subscribe<PageLifecycleMessage>(this, HandlePageLifecycleChanged);
       }
     }

     #endregion Protected Constructors

     #region Protected Methods

     /// <summary>
     /// Make this page lifecycle event visible to derivers
     /// </summary>
     /// <param name="args"></param>
     protected virtual void OnPageLifecycleChanged(IPageLifecycleMessageArgs args)
     {
     }

     #endregion Protected Methods

     #region Private Methods

     private void HandlePageLifecycleChanged(object sender, PageLifecycleMessage args)
     {
       // Make sure the sender is our page
       if (!sender.IsAnEqualReferenceTo(PageEventProvider?.GetEventBroadcaster?.Invoke()))
       {
         return;
       }

       OnPageLifecycleChanged(args.Payload);
     }

     #endregion Private Methods

     #region Public Properties

     public IProvidePageEvents PageEventProvider { get; set; }

     /// <summary>
     /// Copied from the menu item to this page (at least for now)
     /// </summary>
     public string PageTitle { get; set; }

     #endregion Public Properties
   }

   public interface IPageViewModelBase : IViewModelBase, IReceivePageEvents
   {
     #region Public Properties

     /// <summary>
     /// Copied from the menu item to this page (at least for now)
     /// </summary>
     string PageTitle { get; set; }

     #endregion Public Properties
   }
}
