// *********************************************************************************
// Assembly         : Com.MarcusTS.LifecycleAware
// Author           : Stephen Marcus (Marcus Technical Services, Inc.)
// Created          : 12-24-2018
// Last Modified On : 12-24-2018
//
// <copyright file="IRespondToAppStateChanges.cs" company="Marcus Technical Services, Inc.">
//     Copyright @2018 Marcus Technical Services, Inc.
// </copyright>
//
// MIT License
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// *********************************************************************************

namespace Com.MarcusTS.LifecycleAware.Common.Interfaces
{
   using SharedUtils.Utils;

   /// <summary>
   /// Interface IRespondToAppStateChanges
   /// </summary>
   public interface IRespondToAppStateChanges
   {
      #region Public Events

      /// <summary>
      /// Occurs when [application is going to sleep].
      /// </summary>
      event EventUtils.NoParamsDelegate AppIsGoingToSleep;

      /// <summary>
      /// Occurs when [application is resuming].
      /// </summary>
      event EventUtils.NoParamsDelegate AppIsResuming;

      /// <summary>
      /// Occurs when [application is starting].
      /// </summary>
      event EventUtils.NoParamsDelegate AppIsStarting;

      #endregion Public Events
   }
}