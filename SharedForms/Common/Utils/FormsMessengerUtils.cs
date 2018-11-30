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

   using Interfaces;
   using System;
   using ViewModels;
   using Xamarin.Forms;

   #endregion Imports

   public enum PageLifecycleEvents
   {
     BeforeConstructing,
     AfterConstructing,
     BeforeAppearing,
     AfterAppearing,
     BeforeDisappearing,
     AfterDisappearing
   }

   /// <summary>
   /// A global static utilty library to assist with Xamarin.Forms.MessagingCenter calls.
   /// </summary>
   public static class FormsMessengerUtils
   {
     #region Public Methods

     public static void Send<TMessage>(TMessage message, object sender = null) where TMessage : IMessage
     {
       if (sender == null)
       {
         sender = new object();
       }

       MessagingCenter.Send(sender, typeof(TMessage).FullName, message);
     }

     public static void Subscribe<TMessage>(object subscriber, Action<object, TMessage> callback)
       where TMessage : IMessage
     {
       MessagingCenter.Subscribe(subscriber, typeof(TMessage).FullName, callback);
     }

     public static void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage
     {
       MessagingCenter.Unsubscribe<object, TMessage>(subscriber, typeof(TMessage).FullName);
     }

     #endregion Public Methods
   }

   public class AppStateChangedMessage : GenericMessageWithPayload<AppStateChangeMessageArgs>
   {
     #region Public Constructors

     public AppStateChangedMessage(string oldAppState, bool preventNavStackPush)
     {
       Payload = new AppStateChangeMessageArgs(oldAppState, preventNavStackPush);
     }

     #endregion Public Constructors
   }

   public class AppStateChangeMessageArgs
   {
     #region Public Constructors

     public AppStateChangeMessageArgs(string oldAppState, bool preventNavStackPush)
     {
       OldAppState = oldAppState;
       PreventNavStackPush = preventNavStackPush;
     }

     #endregion Public Constructors

     #region Public Properties

     public string OldAppState { get; set; }

     public bool PreventNavStackPush { get; set; }

     #endregion Public Properties
   }

   /// <summary>
   /// Notifies the app that the device size has changed
   /// </summary>
   public class BroadcastDeviceSizeChangedMessage : GenericMessageWithPayload<DeviceSizeChangeMessageArgs>
   {
     #region Public Constructors

     public BroadcastDeviceSizeChangedMessage(float width, float height)
     {
       Payload = new DeviceSizeChangeMessageArgs(width, height);
     }

     #endregion Public Constructors
   }

   /// <summary>
   /// Notifies the orientation service that a the local device size has changed.
   /// Do *not* use for general broadcast, as it will recur!
   /// </summary>
   public class LocalDeviceSizeChangedMessage : GenericMessageWithPayload<DeviceSizeChangeMessageArgs>
   {
     #region Public Constructors

     public LocalDeviceSizeChangedMessage(float width, float height)
     {
       Payload = new DeviceSizeChangeMessageArgs(width, height);
     }

     #endregion Public Constructors
   }

   /// <summary>
   /// This message is issued as the args whenever a local platform senses a change in its orientation.
   /// </summary>
   public class DeviceSizeChangeMessageArgs : IDeviceSizeChangeMessageArgs
   {
     #region Public Constructors

     public DeviceSizeChangeMessageArgs(float width, float height)
     {
       ScreenWidth = width;
       ScreenHeight = height;
     }

     #endregion Public Constructors

     #region Public Properties

     public float ScreenHeight { get; set; }
     public float ScreenWidth { get; set; }

     #endregion Public Properties
   }

   public abstract class GenericMessageWithPayload<T> : IMessage
   {
     #region Public Properties

     public T Payload { get; set; }

     #endregion Public Properties
   }

   public class MainPageBindingContextChangeRequestMessage : GenericMessageWithPayload<IViewModelBase>
   {
     #region Public Properties

     public bool PreventNavStackPush { get; set; }

     #endregion Public Properties
   }

   public class MainPageChangeRequestMessage : GenericMessageWithPayload<Page>
   {
     #region Public Properties

     public bool PreventNavStackPush { get; set; }

     #endregion Public Properties
   }

   public class MenuLoadedMessage : NoPayloadMessage
   {
   }

   public class NavBarMenuTappedMessage : NoPayloadMessage
   {
   }

   public class NoPayloadMessage : IMessage
   {
   }

   public class ObjectDisappearingMessage : GenericMessageWithPayload<object>
   {
   }

   public class PageLifecycleMessage : GenericMessageWithPayload<IPageLifecycleMessageArgs>
   {
     #region Public Constructors

     public PageLifecycleMessage(IProvidePageEvents sendingPage, PageLifecycleEvents pageEvent)
     {
       Payload = new PageLifecycleMessageArgs(sendingPage, pageEvent);
     }

     #endregion Public Constructors
   }

   public class PageLifecycleMessageArgs : IPageLifecycleMessageArgs
   {
     #region Public Constructors

     public PageLifecycleMessageArgs(IProvidePageEvents sendingPage, PageLifecycleEvents pageEvent)
     {
       SendingPage = sendingPage;
       PageEvent = pageEvent;
     }

     #endregion Public Constructors

     #region Public Properties

     public PageLifecycleEvents PageEvent { get; set; }
     public IProvidePageEvents SendingPage { get; set; }

     #endregion Public Properties
   }

   public interface IDeviceSizeChangeMessageArgs
   {
     #region Public Properties

     float ScreenHeight { get; set; }
     float ScreenWidth { get; set; }

     #endregion Public Properties
   }

   public interface IMessage
   {
   }

   public interface IPageLifecycleMessageArgs
   {
     #region Public Properties

     PageLifecycleEvents PageEvent { get; set; }
     IProvidePageEvents SendingPage { get; set; }

     #endregion Public Properties
   }
}
