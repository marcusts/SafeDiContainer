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

namespace SafeDI.Lib
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Runtime.InteropServices;
   using SharedForms.Common.Utils;

   public static class SafeDIExtensions
   {
      public static void RegisterType
         (
            this ISafeDIContainer diContainer, 
            Type classType,
            StorageRules storageRule = StorageRules.AnyAccessLevel,  
            Func<object> creator = null,
            bool addMainTypeAsDefault = false,
            params Type[] accessibleTypes
         )
      {
         var dictToAdd = new Dictionary<Type, IProvideCreatorAndStorageRule>();

         var accessibleTypesAreNotEmpty = accessibleTypes != null && accessibleTypes.IsNotEmpty();

         if (addMainTypeAsDefault && accessibleTypesAreNotEmpty && accessibleTypes.All(i => i != classType))
         {
            dictToAdd.Add(classType, new ProvideCreatorAndStorageRule(creator, storageRule));
         }

         diContainer.RegisterTypeContracts(classType, dictToAdd);

         if (accessibleTypesAreNotEmpty)
         {
            foreach (var interfaceType in accessibleTypes)
            {
               // We add these with the same constructor and storage rules.
               dictToAdd.Add(interfaceType, new ProvideCreatorAndStorageRule(creator, storageRule));
            }
}
      }

      public static void RegisterType<T>
      (
         this ISafeDIContainer diContainer,
         StorageRules storageRule = StorageRules.AnyAccessLevel,
         Func<object> creator = null,
         bool addMainTypeAsDefault = false,
         params Type[] interfaceTypes
      )
      {
         diContainer.RegisterType(typeof(T), storageRule, creator, addMainTypeAsDefault, interfaceTypes);
      }

      public static object Resolve
      (
         this ISafeDIContainer diContainer,
         Type classOrInterfaceType,
         StorageRules storageRule = StorageRules.AnyAccessLevel,
         object boundInstance = null,
         Func<IEnumerable<IProvideCreatorAndStorageRule>, IProvideCreatorAndStorageRule> multipleContractSelecter = null
      )
      {
         return diContainer.Resolve(classOrInterfaceType, storageRule, boundInstance, multipleContractSelecter);
      }

      public static T Resolve<T>
      (
         this ISafeDIContainer diContainer,
         StorageRules storageRule = StorageRules.AnyAccessLevel,
         object boundInstance = null,
         Func<IEnumerable<IProvideCreatorAndStorageRule>, IProvideCreatorAndStorageRule> multipleContractSelecter = null
      )
         where T : class
      {
         return diContainer.Resolve(typeof(T), storageRule, boundInstance, multipleContractSelecter) as T;
      }
   }
}