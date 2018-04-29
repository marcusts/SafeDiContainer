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
      /// <summary>
      /// Registers a base type so that it can be resolved in the future.
      /// Most of the parameters are optional. If omitted, we make the exact class type
      /// available for a call to Resolve(), but do not allow other forms of access.
      /// To Resolve() and convert to an interface, add those to accessibleTypes.
      /// You can also call the base method, which is wide open but must be
      /// managed carefully.
      /// </summary>
      /// <param name="diContainer">The DI container -- omitted when you call this method, as it is an extension.</param>
      /// <param name="classType">The base class type that will be constructed. *Must* be a concrete class.</param>
      /// <param name="storageRule">
      /// Determines if a strict rule will be enforced about how the new instance of the class will be stored upon Resolve():
      /// 
      /// * AnyAccessLevel: The default; allows the caller to Resolve to determine the way the instance will be stored.
      /// 
      /// * IsolatedInstance: A new instance of the variable will be issued, but it will *not* be stored.
      ///   This is typical where you just need a view model for a view, and there is no reason to maintain it globally.
      ///   If the view model contains data, however, and if it might be required elsewhere in the app, then this will
      ///   cause you to have isolated instances that have separate states. So in that case, it is not recommended.
      ///   You should use the SharedDependencyBetweenInstances in that case.
      /// 
      /// * SharedDependencyBetweenInstances: The container will issue an instance and also store it.
      ///   It will be shared with any requester. This *requires* that you supply your host ("bound") class,
      ///   as there is no other way to manage the relationship between that host and this new instance.
      ///   For example, if you bind a view model to a view, the host is the view and the resolved instance is the view model.
      ///   In the same scenario, the view might belong to a page, so the host would be the page
      ///   and the resolved instance will be the view.
      ///   Because it is a shared instance, it cannot be considered "private".  If that is required, use an IsolatedInstance.
      ///   As soon as all of the bound hosts are disposed, this instance will also be automatically removed from the container.
      ///
      /// * GlobalSingleton: Creates only one instance of the requested type and stores it globally *forever* as long as the container is alive.
      ///   Can be used for service injection. Almost never used for any other purpose.
      /// </param>
      /// <param name="creator">
      ///   A function for creating the class type, if any.
      ///   You do not need to cast as the final type.  The container handles that for you.
      /// </param>
      /// <param name="addMainTypeAsDefault">
      /// Optional, and defaulted to false.  Often, DI containers are asked to create these sorts of instances:
      /// * Cat as IAnimal
      /// * Dog as IAnimal
      /// * Bird as IAnimal
      /// In all three cases, you might pass this as the creator: "() => new Cat()" or dog or bird, etc.
      /// We would then typecast the resulting instance as IAnimal for you.
      /// However, you might do something entirely different:
      /// * MyClass as ImplementedInterface
      /// In which case, your creator might be: "() => new MyClass()".  We would resolve this as ImplementedInterface.
      /// But what if you also wanted to resolve like this: "Resolve{BaseClass}();" ???
      /// You would turn this boolean parameter to True.
      /// Your registration would be:  RegisterType(BaseClass, creator: () > new BaseClass, adddMainAsDefault = true,accessibleTypes = typeof(ImplementedInterface).
      /// After that, you can resolve as either BaseClass or ImplementedInterface.
      /// </param>
      /// <param name="accessibleTypes">
      /// The list of types to type-cast the constructed base class as.
      /// It can be any number. Remember to use "typeof(your type)" for each type, separated by a comma.
      /// </param>
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

      /// <summary>
      /// The same as <see cref="RegisterType"/>, except with a more Generic way to state the base class type.
      /// </summary>
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

      /// <summary>
      /// Creates an instance of a class and stores it according to the requested rules.
      /// Only works if you have registered each base class first along with any interface they should be available (type-cast) as.
      /// See <see cref="RegisterType"/> for details about this.
      /// </summary>
      /// <param name="diContainer">The DI container -- omitted when you call this method, as it is an extension.</param>
      /// <param name="classOrInterfaceType">
      /// This is the type that you wish to receive. It is not necessarily the "base" class type.
      /// It is more commonly an interface implemented by your base class type.
      /// </param>
      /// <param name="storageRule">
      /// This is a *request* for a storage rule, but is subject to strict guidelines:
      /// * If you ask for "AnyAccessLevel", and there is no other value set in the registration, we will give you an IsolatedInstance.
      /// * If, when you registered, you set the access level to something like "SharedDependencyBetweenInstances",
      ///   and then here on Resolve ask for "GlobalSingleton", we default to throw with an illegal request.
      ///   You can defeat this limitation by setting the property <see cref="ISafeDIContainer.ThrowOnStorageRuleCoercion"/> to false.
      ///   If you like to keep centralized control of types and instances, you should probably just register more carefully
      ///   and never ask for what you did not register as.
      /// * Generally, when registrations are neat and tight, you can ignore this setting, as it is often defeated as we just described.
      /// </param>
      /// <param name="boundInstance">
      /// The "host" or "bound" class that is attached to this instance.  Only required if you need a "SharedDependencyBetweenInstances".
      /// </param>
      /// <param name="multipleContractSelecter">
      /// This is an advanced parameter where you can include a function to "break the tie" when this container tries to Resolve,
      /// but comes up with a bunch of competing candidates.  If you leave this at null, and we cannot see a single legal choice, we will throw an error.
      /// You can defeat this error by setting the <see cref="ISafeDIContainer.ThrowOnMultipleResolutions"/> to false.
      /// If you do that -- which is the default -- we will just pick the first legal candidate we can find.
      /// THIS IS A VERY SLOPPY PRACTICE that we are attempting to discourage!
      /// </param>
      /// <returns></returns>
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

      /// <summary>
      /// The same as <see cref="Resolve"/>, except with a more Generic way to state the base class type.
      /// </summary>
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
