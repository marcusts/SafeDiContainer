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
   #region Imports

   using System;
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Linq;
   using SharedForms.Common.Utils;

   #endregion

   public enum StorageRules
   {
      AnyAccessLevel, // Default for class management
      IsolatedInstance, // Default for resolving actual variables
      SharedDependencyBetweenInstances,
      GlobalSingleton
   }

   public class SafeDI
   {
      #region Protected Variables

      /// <summary>
      ///    Specifies that a type can be resolved as a specific type (can be different as long as
      ///    related) Also sets the storage rules. Defaults to "all".
      /// </summary>
      protected readonly IDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>> _registeredTypeContracts =
         new ConcurrentDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>>();

      /// <summary>
      ///    A dictionary of instances that are shared between one ore more other instances. When the
      ///    list of shared instances reaches zero, the main instance is removed.
      /// </summary>
      protected readonly IDictionary<object, IList<object>> _sharedInstances =
         new ConcurrentDictionary<object, IList<object>>();

      /// <summary>
      ///    A dictionary of global singletons keyed by type. There can only be one each of a given type.
      /// </summary>
      protected readonly IDictionary<Type, object> _singletonInstances =
         new ConcurrentDictionary<Type, object>();

      #endregion Protected Variables

      #region Public Properties

      public bool ThrowOnMultipleResolutions { get; set; }

      public bool ThrowOnStorageRuleCoercion { get; set; }

      public bool ThrowWhenMoreThanOneMasterContractType { get; set; }

      #endregion Public Properties

      #region Public Methods

      /// <summary>
      ///    Adds a list of bound instances to a single shared instance.
      /// </summary>
      /// <typeparam name="TSharedInstance">The shared instance.</typeparam>
      /// <param name="sharedInstance">The parent instance.</param>
      /// <param name="instances">The bound instances.</param>
      public void CreateSharedInstances<TSharedInstance>(TSharedInstance sharedInstance,
         params object[] instances)
      {
         // Use the same type checker that we apply to the type dependency case. These are instances
         // of *types*
         var instanceTypes = instances.Select(i => i.GetType()).ToArray();

         var illegalTypes = instanceTypes.Where(t => t.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance)))
            .ToList();

         if (illegalTypes.IsNotEmpty())
         {
            // Report the first error.
            throw new ArgumentException("SafeDI: ManageSharedInstances: cannot share " + illegalTypes[0] +
                                        " with itself, even if derived.");
         }

         // Verify that the instance types are *not* directly assignable from the shared instance, as
         // that is circular.
         foreach (var type in instanceTypes)
         {
            if (type.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance)))
            {
               throw new ArgumentException("SafeDI: ManageSharedInstances: " + type +
                                           " is an illegal circular reference back to the parent " +
                                           typeof(TSharedInstance));
            }
         }

         var boundInstances = _sharedInstances[sharedInstance];


         if (boundInstances == null)
         {
            boundInstances = new List<object>();
            _sharedInstances.Add(sharedInstance, boundInstances);
            return;
         }

         foreach (var instance in instances)
         {
            if (!boundInstances.Contains(instance))
            {
               boundInstances.Add(instance);
            }
         }
      }

      /// <summary>
      ///    Adds a key-value pair with a class type and an instance of that class.
      /// </summary>
      /// <typeparam name="ClassT">The class type.</typeparam>
      /// <param name="instance">The instance of the type.</param>
      public void CreateSingletonInstance<ClassT>(ClassT instance)
      {
         if (_singletonInstances[typeof(ClassT)] != null)
         {
            Debug.WriteLine("SafeDI: CreateSingletonInstance: Over-writing an existing singleton of type " +
                            _singletonInstances[typeof(ClassT)].GetType());
         }

         _singletonInstances[typeof(ClassT)] = instance;
      }

      /// <summary>
      ///    Adds a list of types that the type can be resolved as. Includes creators and storage rules.
      /// </summary>
      /// <typeparam name="ClassT">The class type that owns the contracts.</typeparam>
      /// <param name="creatorsAndRules">
      ///    The list of class creators and rules. The creators can be null.
      /// </param>
      public void RegisterTypeContracts<ClassT>(IDictionary<Type, IProvideCreatorAndStorageRule> creatorsAndRules)
      {
         if (_registeredTypeContracts[typeof(ClassT)] == null)
         {
            _registeredTypeContracts[typeof(ClassT)] = creatorsAndRules;
         }
         else
         {
            // Augment whatever is there
            foreach (var keyValuePair in creatorsAndRules)
            {
               _registeredTypeContracts[typeof(ClassT)].Add(keyValuePair);
            }
         }
      }

      /// <summary>
      /// Removes a bound instance from all shared instances.
      /// Also cleans up any orphaned shared instances.
      /// </summary>
      /// <param name="obj"></param>
      public void RemoveBoundSharedDependencies(object obj)
      {
         if (_sharedInstances.IsEmpty())
         {
            return;
         }

         var sharedInstancesToDelete = new List<object>();

         foreach (var sharedInstance in _sharedInstances)
         {
            var foundBoundMembers = sharedInstance.Value.Where(si => ReferenceEquals(si, obj)).ToArray();

            if (!foundBoundMembers.Any())
            {
               continue;
            }

            foreach (var foundBoundMember in foundBoundMembers)
            {
               sharedInstance.Value.Remove(foundBoundMember);
            }

            // If no children, remove the shared instance itself.
            if (!sharedInstance.Value.Any())
            {
               sharedInstancesToDelete.Add(sharedInstance);
            }
         }

         // Remove the shared instances that are now orphaned
         foreach (var sharedInstance in sharedInstancesToDelete)
         {
            _sharedInstances.Remove(sharedInstance);
         }
      }

      /// <summary>
      ///    Removes the dictionary entry for the instance of a parent class type.
      /// </summary>
      /// <param name="classT">The type of singleton to remove.</param>
      public void RemoveSingletonInstance(Type classT)
      {
         if (_singletonInstances.ContainsKey(classT))
         {
            _singletonInstances.Remove(classT);
         }
      }

      /// <summary>
      /// If the type exists as a shared instance, removes it.
      /// Only used if this shared instance is about to go out of view.
      /// </summary>
      /// <param name="type"></param>
      public void RemoveSharedInstance(Type type)
      {
         if (_sharedInstances.ContainsKey(type))
         {
            _sharedInstances.Remove(type);
         }
      }

      public TypeRequestedT Resolve<TypeRequestedT>
      (
         StorageRules ruleRequested = StorageRules.AnyAccessLevel,
         object boundInstance = null,
         Func<IEnumerable<IProvideCreatorAndStorageRule>, IProvideCreatorAndStorageRule> multipleContractSelecter = null
      )
         where TypeRequestedT : class
      {
         return Resolve
         (
            typeof(TypeRequestedT),
            ruleRequested,
            boundInstance,
            multipleContractSelecter = null
         ) as TypeRequestedT;
      }

      public object Resolve
      (
         Type typeRequestedT,
         StorageRules ruleRequested = StorageRules.AnyAccessLevel,
         object boundInstance = null,
         Func<IEnumerable<IProvideCreatorAndStorageRule>, IProvideCreatorAndStorageRule> multipleContractSelecter = null
      )
      {
         object returnT = null;

         // Verify that we do not have an incompatible rules request and bound object
         if (boundInstance == null && ruleRequested == StorageRules.SharedDependencyBetweenInstances)
         {
            throw new ArgumentException(
               "SafeDI : Resolve: Cannot resolve a shared or dependent type without a bound type to have it rely on.");
         }

         // Look for a contract for this type. The value we seek is in the IProvideCreatorAndStorageRule.
         var qualifyingTypeContracts = _registeredTypeContracts.Where(rc => rc.Value.ContainsKey(typeRequestedT));

         if (qualifyingTypeContracts.IsEmpty())
         {
            throw new ArgumentException("No registered contracts for the type " + typeRequestedT);
         }

         // If there is more than one master type, we should consider throwing, as this is extremely
         // confusing for resolution.
         if (qualifyingTypeContracts.Count() > 1)
         {
            if (ThrowWhenMoreThanOneMasterContractType)
            {
               throw new ArgumentException("SafeDI : Resolve: Too many contracts (" +
                                           qualifyingTypeContracts.Count() +
                                           ") for the resolvable type " + typeRequestedT);
            }
         }

         // Get the actual registrations. Reduce down to the resolvable type -- we might have other
         // resolvable types in this dictionary NOTE that the user might have allowed us to make a
         // random decision here: to take the first of several candidates.
         var qualifyingMasterType = qualifyingTypeContracts.First().Key;

         // We know this is not empty because of the previous queries.
         var qualifyingRegistrations = qualifyingTypeContracts.First().Value.Where(qtc => qtc.Key == typeRequestedT)
            .Select(qtc => qtc.Value);

         // Filter against storage rules, if necessary. If "all", all qualifying registrations
         // proceed to the next step.
         if (ruleRequested != StorageRules.AnyAccessLevel)
         {
            var rulesTest = qualifyingRegistrations.Where(qr => qr.ProvidedStorageRule == ruleRequested);
            if (rulesTest.IsEmpty())
            {
               if (ThrowOnStorageRuleCoercion)
               {
                  throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " +
                                              typeRequestedT +
                                              " using storage rule " + ruleRequested);
               }

               // ELSE proceed with the qualifying registrations
            }

            qualifyingRegistrations = rulesTest;
         }

         IProvideCreatorAndStorageRule resolutionToSeek = null;

         if (!qualifyingRegistrations.Any())
         {
            throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " +
                                        typeRequestedT);
         }

         // We have at least one qualifying registration

         if (qualifyingRegistrations.Count() == 1)
         {
            resolutionToSeek = qualifyingRegistrations.First();
         }
         else
         {
            // If more than one registration, see if we can decide with the conflict resolver
            if (multipleContractSelecter != null)
            {
               resolutionToSeek = multipleContractSelecter(qualifyingRegistrations);

               if (resolutionToSeek == null)
               {
                  if (ThrowOnMultipleResolutions)
                  {
                     throw new InvalidOperationException("SafeDI : Resolve: Cannot determine which one of " +
                                                         qualifyingRegistrations.Count() +
                                                         " registrations to use through the conflict resolver.");
                  }
               }

               // ELSE resolutionToSeek is valid
            }
            else if (ThrowOnMultipleResolutions)
            {
               throw new InvalidOperationException("SafeDI : Resolve: Cannot determine which one of " +
                                                   qualifyingRegistrations.Count() +
                                                   " registrations to use.  Please add a conflict resolver or change the registrations.");
            }
            else
            {
               // A user-authorized random choice
               resolutionToSeek = qualifyingRegistrations.First();
            }
         }

         // The same as having no qualifying registrations
         if (resolutionToSeek == null)
         {
            throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " +
                                        typeRequestedT);
         }

         // Verify that the result will be legal, since we will cast as TypeRequestedT
         if (!qualifyingMasterType.IsMainTypeOrAssignableFromMainType(typeRequestedT))
         {
            throw new InvalidOperationException("SafeDI : Resolve: Cannot save an instance of " +
                                                qualifyingMasterType + " as " + typeRequestedT);
         }

         // Now we have:
         // 1. A master type to create;
         // 2. A TypeRequestedT type to save as -- these might not be the same, but must relate class-design-wise
         // 3. A resolution to seek that might include an instance creator;
         // 4. If no instance creator, then we will use activator create instance

         // We switch here; the resolutionToSeek now has authority over our process.
         ruleRequested = resolutionToSeek.ProvidedStorageRule;

         // Check for existing global singletons
         if (ruleRequested == StorageRules.GlobalSingleton)
         {
            var foundGlobalInstance = _singletonInstances.FirstOrDefault(si => si.GetType() == typeRequestedT).Value;

            if (foundGlobalInstance != null)
            {
               var foundGlobalInstanceAsRequestedType = Convert.ChangeType(foundGlobalInstance, typeRequestedT);

               // Return this instance; we are done
               if (foundGlobalInstanceAsRequestedType != null)
               {
                  return foundGlobalInstanceAsRequestedType;
               }
            }

            // ELSE proceed to creating the instance
         }

         // See if we are asked for a shared instance; if so, that might already exist This is "else"
         // because it is completely separate from the global instance case.
         else if (ruleRequested == StorageRules.SharedDependencyBetweenInstances)
         {
            var foundSharedInstance = _sharedInstances.FirstOrDefault(si => si.Key.GetType() == typeRequestedT);

            // If valid (not empty)
            if (foundSharedInstance.IsNotAnEqualObjectTo(default(KeyValuePair<object, IList<object>>)))
            {
               // Add our bound object to the list... maybe make sure it's not there already..
               if (!foundSharedInstance.Value.Contains(boundInstance))
               {
                  foundSharedInstance.Value.Add(boundInstance);
               }

               // Return the key object, as that is the one that is being shared between the list of
               // bound instances
               returnT = foundSharedInstance.Key;
               // as TypeRequestedT;
               returnT = Convert.ChangeType(returnT, typeRequestedT);

               if (returnT == null)
               {
                  throw new InvalidOperationException(
                     "SafeDI : Resolve: Failed to share a found instance of type " +
                     typeRequestedT + ".  UNKNOWN ERROR.");
               }

               // Since returnT is a found shared instance, we are actually done right now
               return returnT;
            }
         }

         // ELSE must instantiate

         // Try the creator
         object instantiatedObject = null;

         if (resolutionToSeek.ProvidedCreator != null)
         {
            instantiatedObject = resolutionToSeek.ProvidedCreator();

            if (instantiatedObject == null)
            {
               throw new InvalidOperationException(
                  "SafeDI : Resolve: Could not create an object using the provided constructor.");
            }

            // ELSE the instated object is valid. Proceed as if we had created the object using
            // activator create instance.
         }

         if (instantiatedObject == null)
         {
            // No choice but to use activator create instance Note that we actually *create* the
            // qualifyingMasterType and then *save as* the TypeRequestedT

            // Get the constructors and order by the fewest parameters -- HACK -- not very smart
            var availableConstructors = qualifyingMasterType.GetConstructors().OrderBy(c => c.GetParameters().Count());

            if (availableConstructors.IsEmpty())
            {
               throw new InvalidOperationException(
                  "SafeDI : Resolve: Could not find a valid constructor for type requested: " +
                  qualifyingMasterType);
            }

            // try each constructor one at a time until we succeed
            foreach (var constructor in availableConstructors)
            {
               var constructorParameters = constructor.GetParameters();

               if (constructorParameters.IsEmpty())
               {
                  instantiatedObject = Activator.CreateInstance(qualifyingMasterType);
                  break;
               }

               var parameters = new List<object>();

               foreach (var parameterInfo in constructorParameters)
               {
                  object variableToInjectAsParameter;

                  try
                  {
                     variableToInjectAsParameter = Resolve(parameterInfo.ParameterType);
                  }
                  catch (Exception)
                  {
                     // If we threw, then this constructor will not work
                     Debug.WriteLine("SafeDI: Resolve: Exception on attempt to instantiate " +
                                     qualifyingMasterType +
                                     " using one of its constructors. Will try another constructor...");
                     goto SKIP_THIS_ONE;
                  }

                  parameters.Add(variableToInjectAsParameter);
               }

               instantiatedObject = constructor.Invoke(parameters.ToArray());

               //var instantiatedType = instantiatedObject.GetType();
               //if ((instantiatedType == qualifyingMasterType))
               //{
               //   // Success
               //   break;
               //}
               //else
               //{
               //   Debug.WriteLine("SafeDI: Resolve: Constructed wrong type: " + instantiatedType + " when I wanted " + qualifyingMasterType);
               //}

               SKIP_THIS_ONE:;
            }
         }

         if (instantiatedObject == null)
         {
            throw new InvalidOperationException("SafeDI : Resolve: Could not construct an object for base type " +
                                                qualifyingMasterType);
         }

         // Now we have an instantiated object. Determine its storage before delivering it.
         returnT = Convert.ChangeType(instantiatedObject, typeRequestedT);

         if (returnT == null)
         {
            throw new InvalidOperationException("SafeDI : Resolve: Failed to type-cast my constructed object " +
                                                instantiatedObject.GetType() + " as " + typeRequestedT);
         }

         // The last step is to determine if we have to save the new object in our container.

         // If global, yes -- this will only occur once
         switch (ruleRequested)
         {
            case StorageRules.GlobalSingleton:
               CreateSingletonInstance(returnT);
               break;

            case StorageRules.SharedDependencyBetweenInstances:
               CreateSharedInstances(returnT, boundInstance);
               break;

            default:
               //case StorageRules.AnyAccessLevel:
               //case StorageRules.IsolatedInstance:
               // DO NOTHING -- the instance manages itself locally
               break;
         }

         return returnT;
      }

      /// <summary>
      ///    Removes a list of types that the parent type can be resolved as. Includes creators and
      ///    storage rules.
      /// </summary>
      /// <param name="typesToUnregister">The types to remove.</param>
      /// <typeparam name="TParent">The generic parent type</typeparam>
      public void UnregisterTypeContracts<TParent>(params Type[] typesToUnregister)
      {
         if (_registeredTypeContracts[typeof(TParent)] == null || typesToUnregister.IsEmpty())
         {
            return;
         }

         // ELSE remove the types individually
         var parentRegistrations = _registeredTypeContracts[typeof(TParent)];

         foreach (var type in typesToUnregister)
         {
            parentRegistrations.Remove(type);
         }

         // If this leaves the collection empty, remove it.
         if (parentRegistrations.IsEmpty())
         {
            _registeredTypeContracts.Remove(typeof(TParent));
         }
      }

      #endregion Public Methods

      #region Protected Methods

      protected virtual bool CanManageRegisteredTypeContracts<TParent>(bool addThem,
         IDictionary<Type, IProvideCreatorAndStorageRule> typesAndRules)
      {
         if (typesAndRules.IsEmpty())
         {
            return false;
         }

         if (addThem)
         {
            foreach (var type in typesAndRules.Keys)
            {
               if (!typeof(TParent).IsMainTypeOrAssignableFromMainType(type))
               {
                  throw new ArgumentException("SafeDI: CanManageRegisteredTypeContracts: cannot resolve " +
                                              typeof(TParent) + " to " +
                                              type + " because they have no class or interface relationship.");
               }
            }
         }

         return true;
      }

      /// <summary>
      ///    This list is "backwards" -- The key is an object which we instantiate (or which the user's
      ///    creator function instantiates).
      /// </summary>
      /// <typeparam name="TSharedInstance">The type of the shared instance</typeparam>
      /// <param name="sharedInstance">The actual shared instance</param>
      /// <param name="addThem"></param>
      /// <param name="instances"></param>
      protected virtual void ManageSharedInstances<TSharedInstance>(TSharedInstance sharedInstance, bool addThem,
         object[] instances)
      {
         if (!addThem && !_sharedInstances.ContainsKey(typeof(TSharedInstance)))
         {
            return;
         }

         // Use the same type checker that we apply to the type dependency case. These are instances
         // of *types*
         var instanceTypes = instances.Select(i => i.GetType()).ToArray();

         if (addThem)
         {
            var illegalTypes = instanceTypes.Where(t => t.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance)))
               .ToList();

            if (illegalTypes.IsNotEmpty())
            {
               // Report the first error.
               throw new ArgumentException("SafeDI: ManageSharedInstances: cannot share " + illegalTypes[0] +
                                           " with itself, even if derived.");
            }

            // Verify that the instance types are *not* directly assignable from the shared instance, as
            // that is circular.
            foreach (var type in instanceTypes)
            {
               if (type.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance)))
               {
                  throw new ArgumentException("SafeDI: ManageSharedInstances: " + type +
                                              " is an illegal circular reference back to the parent " +
                                              typeof(TSharedInstance));
               }
            }
         }

         var boundInstances = _sharedInstances[sharedInstance];

         if (!addThem && boundInstances.IsEmpty())
         {
            return;
         }

         if (boundInstances == null && addThem)
         {
            boundInstances = new List<object>();
            _sharedInstances.Add(sharedInstance, boundInstances);
         }

         foreach (var instance in instances)
         {
            var found = boundInstances.Contains(instance);

            if (addThem)
            {
               if (!found)
               {
                  boundInstances.Add(instance);
               }
            }
            else
            {
               if (found)
               {
                  boundInstances.Remove(instance);
               }
            }
         }
      }

      #endregion Protected Methods

      /// <summary>
      /// Called by the deriver whenever a class is about to disappear from view.
      /// It is better to call this before the finalizer, as that can be extremely late.
      /// An example would be Xamarin.Forms.ContentPage.OnDisapearing.
      /// Other views or view models will have to listen to the original page event and then notify about their own demise.
      /// If this step is skipped, none of the lifecycle protections will occur!
      /// </summary>
      /// <param name="containerClass"></param>
      public virtual void ContainerClassIsDying(object containerClass)
      {
         // Remove the class from the global singletons
         RemoveSingletonInstance(containerClass.GetType());

         // See if the shared instances contain the class/
         // There are two scenarios here:
         // 1. This class is the shared class.  If so, we remove the entire node that contains the container class.

         // This only removes if it exists.
         RemoveSharedInstance(containerClass.GetType());

         // 2. This class is bound to a shared class.  If true, we remove that single part of the node.
         //    However, if this is the last node, then we do as with #1 -- we kill the entire node.
         RemoveBoundSharedDependencies(containerClass);
      }
   }
}