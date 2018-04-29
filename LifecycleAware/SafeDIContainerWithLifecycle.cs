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
   #region Imports

   using SafeDI.Lib;
   using SharedForms.Common.Utils;

   #endregion

   public interface ISafeDIContainerWithLifecycle : ISafeDIContainer
   {
   }

   public class SafeDIContainerWithLifecycle : SafeDIContainer, ISafeDIContainerWithLifecycle
   {
      #region Public Constructors

      public SafeDIContainerWithLifecycle()
      {
         FormsMessengerUtils.Subscribe<ObjectDisappearingMessage>(this, OnViewOrPageDisappearing);
      }

      #endregion Public Constructors

      #region Protected Methods

      protected override void ReleaseUnmanagedResources()
      {
         base.ReleaseUnmanagedResources();

         FormsMessengerUtils.Unsubscribe<ObjectDisappearingMessage>(this);
      }

      #endregion Protected Methods

      #region Private Methods

      private void OnViewOrPageDisappearing(object sender, ObjectDisappearingMessage args)
      {
         // Notify the base container of the change.
         ContainerClassIsDying(args.Payload);
      }

      #endregion Private Methods
   }
}