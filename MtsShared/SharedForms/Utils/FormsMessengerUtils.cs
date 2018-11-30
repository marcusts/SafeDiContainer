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

namespace SharedForms.Common.Utils
{
   using System;
   using Interfaces;
   using ViewModels;
   using Xamarin.Forms;

   public enum PageLifecycleEvents
   {
      BeforeConstructing,
      AfterConstructing,
      BeforeAppearing,
      AfterAppearing,
      BeforeDisappearing,
      AfterDisappearing
   }

   public interface IDeviceSizeChangeMessageArgs
   {
      float ScreenHeight { get; set; }
      float ScreenWidth { get; set; }
   }

   public interface IMessage
   {
   }

   public interface IPageLifecycleMessageArgs
   {
      PageLifecycleEvents PageEvent { get; set; }
      IProvidePageEvents SendingPage { get; set; }
   }

   /// <summary>
   /// A global static utilty library to assist with Xamarin.Forms.MessagingCenter calls.
   /// </summary>
   public static class FormsMessengerUtils
   {
      public static void Send<TMessage>(TMessage message, object sender = null) where TMessage : IMessage
      {
         if (sender == null)
         {
            sender = new object();
         }

         MessagingCenter.Send(sender, typeof(TMessage).FullName, message);
      }

      public static void Subscribe<TMessage>(object subscriber, Action<object, TMessage> callback)
        where TMessage : IMessage => MessagingCenter.Subscribe(subscriber, typeof(TMessage).FullName, callback);

      public static void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage => MessagingCenter.Unsubscribe<object, TMessage>(subscriber, typeof(TMessage).FullName);
   }

   public class AppStateChangedMessage : GenericMessageWithPayload<AppStateChangeMessageArgs>
   {
      public AppStateChangedMessage(string oldAppState, bool preventNavStackPush) => Payload = new AppStateChangeMessageArgs(oldAppState, preventNavStackPush);
   }

   public class AppStateChangeMessageArgs
   {
      public AppStateChangeMessageArgs(string oldAppState, bool preventNavStackPush)
      {
         OldAppState = oldAppState;
         PreventNavStackPush = preventNavStackPush;
      }

      public string OldAppState { get; set; }

      public bool PreventNavStackPush { get; set; }
   }

   /// <summary>
   /// Notifies the app that the device size has changed
   /// </summary>
   public class BroadcastDeviceSizeChangedMessage : GenericMessageWithPayload<DeviceSizeChangeMessageArgs>
   {
      public BroadcastDeviceSizeChangedMessage(float width, float height) => Payload = new DeviceSizeChangeMessageArgs(width, height);
   }

   /// <summary>
   /// This message is issued as the args whenever a local platform senses a change in its orientation.
   /// </summary>
   public class DeviceSizeChangeMessageArgs : IDeviceSizeChangeMessageArgs
   {
      public DeviceSizeChangeMessageArgs(float width, float height)
      {
         ScreenWidth = width;
         ScreenHeight = height;
      }

      public float ScreenHeight { get; set; }
      public float ScreenWidth { get; set; }
   }

   public abstract class GenericMessageWithPayload<T> : IMessage
   {
      public T Payload { get; set; }
   }

   /// <summary>
   /// Notifies the orientation service that a the local device size has changed.
   /// Do *not* use for general broadcast, as it will recur!
   /// </summary>
   public class LocalDeviceSizeChangedMessage : GenericMessageWithPayload<DeviceSizeChangeMessageArgs>
   {
      public LocalDeviceSizeChangedMessage(float width, float height) => Payload = new DeviceSizeChangeMessageArgs(width, height);
   }

   public class MainPageBindingContextChangeRequestMessage : GenericMessageWithPayload<IViewModelBase>
   {
      public bool PreventNavStackPush { get; set; }
   }

   public class MainPageChangeRequestMessage : GenericMessageWithPayload<Page>
   {
      public bool PreventNavStackPush { get; set; }
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
      public PageLifecycleMessage(IProvidePageEvents sendingPage, PageLifecycleEvents pageEvent) => Payload = new PageLifecycleMessageArgs(sendingPage, pageEvent);
   }

   public class PageLifecycleMessageArgs : IPageLifecycleMessageArgs
   {
      public PageLifecycleMessageArgs(IProvidePageEvents sendingPage, PageLifecycleEvents pageEvent)
      {
         SendingPage = sendingPage;
         PageEvent = pageEvent;
      }

      public PageLifecycleEvents PageEvent { get; set; }
      public IProvidePageEvents SendingPage { get; set; }
   }
}