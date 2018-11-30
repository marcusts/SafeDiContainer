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

namespace SafeDIWithLifecycle.Tests
{
   using System;
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using SafeDI.Lib;

   public interface ISafeDIContainerWithExposedProperties : ISafeDIContainer
   {
      IDictionary<Type, object> ExposedGlobalSingletons { get; }

      IDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>> ExposedRegisteredTypeContracts { get; }

      IDictionary<object, IList<object>> ExposedSharedInstancesWithBoundMembers { get; }
   }
   public class SafeDIContainerWithExposedProperties : SafeDIContainer, ISafeDIContainerWithExposedProperties
   {
      public IDictionary<Type, object> ExposedGlobalSingletons => _globalSingletons;

      public IDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>> ExposedRegisteredTypeContracts => _registeredTypeContracts;

      public IDictionary<object, IList<object>> ExposedSharedInstancesWithBoundMembers => _sharedInstancesWithBoundMembers;
   }
}
