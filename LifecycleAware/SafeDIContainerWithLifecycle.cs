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

namespace LifecycleAware
{
   using SafeDI.Lib;
   using SharedForms.Common.Utils;

   public interface ISafeDIContainerWithLifecycle : ISafeDIContainer
   {
   }

   /// <summary>
   /// This container is currently intended to be used for the full life of an app.
   /// Hence we do not unsubscribe from the messaging center.
   /// </summary>
   public class SafeDIContainerWithLifecycle : SafeDIContainer
   {
      public SafeDIContainerWithLifecycle()
      {
         FormsMessengerUtils.Subscribe<ViewOrPageDisappearingMessage>(this, OnViewOrPageDisappearing);
      }

      private void OnViewOrPageDisappearing(object sender, ViewOrPageDisappearingMessage args)
      {
         // Notify the base container of the change.
         ContainerClassIsDying(args.Payload);
      }
   }
}