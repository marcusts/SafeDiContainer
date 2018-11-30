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

namespace SmartDIWithLifeCycle.MSTests.TestClasses
{
   public interface IRecursiveService : IServiceOne
   {
   }

   public interface IServiceOne
   {
   }

   public interface IServiceThree
   {
   }

   public interface IServiceTwo
   {
   }

   public class RecursiveService : ServiceOne, IRecursiveService
   {
      #region Private Fields

      private readonly IServiceOne _serviceOne;

      #endregion Private Fields

      #region Public Constructors

      public RecursiveService(IServiceOne serviceOne) => _serviceOne = serviceOne;

      #endregion Public Constructors
   }

   public class ServiceOne : IServiceOne
   {
   }

   public class ServiceThree : IServiceThree
   {
      #region Private Fields

      private readonly IServiceOne _serviceOne;
      private readonly IServiceTwo _serviceTwo;

      #endregion Private Fields

      #region Public Constructors

      public ServiceThree(IServiceOne serviceOne, IServiceTwo serviceTwo)
      {
         _serviceOne = serviceOne;
         _serviceTwo = serviceTwo;
      }

      #endregion Public Constructors
   }

   public class ServiceTwo : IServiceTwo
   {
      #region Private Fields

      private readonly IServiceOne _serviceOne;

      #endregion Private Fields

      #region Public Constructors

      public ServiceTwo(IServiceOne serviceOne) => _serviceOne = serviceOne;

      #endregion Public Constructors
   }
}