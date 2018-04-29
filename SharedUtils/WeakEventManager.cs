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

namespace SharedUtils
{
   using System;

   /// <summary>
   /// Static Class that holds the extension methods to handle events using weak references.
   /// This way we do not need to worry about unregistered the event handler.
   /// </summary>
   /// <remarks>
   /// Thanks, Dennis!
   /// https://forums.xamarin.com/discussion/4931/summary-of-current-best-practices-for-event-handlers-and-disposing
   /// </remarks>
   public static class WeakEventManager
   {
      /// <summary>
      /// This overload handles any type of EventHandler
      /// </summary>
      /// <typeparam name="T">The type of the T.</typeparam>
      /// <typeparam name="TDelegate">The type of the T delegate.</typeparam>
      /// <typeparam name="TArgs">The type of the T args.</typeparam>
      /// <param name="subscriber">The subscriber.</param>
      /// <param name="converter">The converter.</param>
      /// <param name="add">The add.</param>
      /// <param name="remove">The remove.</param>
      /// <param name="action">The action.</param>
      public static void SetAnyHandler<T, TDelegate, TArgs>
         (
            this T subscriber,
            Func<EventHandler<TArgs>, TDelegate> converter,
            Action<TDelegate> add,
            Action<TDelegate> remove,
            Action<T, TArgs> action
         )
         where TArgs : EventArgs
         where TDelegate : class
         where T : class
      {
         if (converter == null)
         {
            throw new ArgumentException("WeakEventManager: SetAnyHandler: Converter cannot be null.");
         }

         var subsWeakRef = new WeakReference(subscriber);
         TDelegate handler = null;

         handler = 
            converter?.Invoke
               (
                  new EventHandler<TArgs>
                     (
                        (s, e) =>
                        {
                           var subsStrongRef = subsWeakRef.Target as T;
                           if (subsStrongRef != null)
                           {
                              action?.Invoke(subsStrongRef, e);
                           }
                           else
                           {
                              if (handler != null)
                              {
                                 remove?.Invoke(handler);
                                 handler = null;
                              }
                           }
                        }
                     )
               );

         add?.Invoke(handler);
      }

      /// <summary>
      /// this overload is simplified for generic EventHandlers
      /// </summary>
      /// <typeparam name="T">The type of the T.</typeparam>
      /// <typeparam name="TArgs">The type of the T args.</typeparam>
      /// <param name="subscriber">The subscriber.</param>
      /// <param name="add">The add.</param>
      /// <param name="remove">The remove.</param>
      /// <param name="action">The action.</param>
      public static void SetAnyHandler<T, TArgs>
      (
         this T subscriber,
         Action<EventHandler<TArgs>> add,
         Action<EventHandler<TArgs>> remove,
         Action<T, TArgs> action
      )
         where TArgs : EventArgs
         where T : class
      {
         SetAnyHandler<T, EventHandler<TArgs>, TArgs>
            (
               subscriber,                     
               h => h,                     
               add,                     
               remove,                     
               action
            );
      }             

      /// <summary>
      /// this overload is simplified for EventHandlers.
      /// </summary>
      /// <typeparam name="T">The type of the T.</typeparam>
      /// <param name="subscriber">The subscriber.</param>
      /// <param name="add">The add.</param>
      /// <param name="remove">The remove.</param>
      /// <param name="action">The action.</param>
      public static void SetAnyHandler<T>
         (
            this T subscriber,
            Action<EventHandler> add, 
            Action<EventHandler> remove,                 
            Action<T, EventArgs> action
         )                 
         where T : class
      {
         SetAnyHandler<T, EventHandler, EventArgs>
            (
               subscriber,                     
               h => (o, e) => h?.Invoke(o, e), //This is a workaround from Rx
               add,                     
               remove,                     
               action
            );
      }
   }
}
