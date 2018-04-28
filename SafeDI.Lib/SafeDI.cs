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
   using System.IO.Compression;
   using System.Linq;
   using System.Reflection;
   using SharedForms.Common.Utils;

   #endregion

   public interface ISafeDI
   {
   }

   public enum StorageRules
   {
      AnyAccessLevel,
      PerDependentInstance,
      SharedBetweenInstances,
      GlobalSingleton
   }

   public class SafeDI : ISafeDI
   {
      #region Private Variables

      /// <summary>
      /// A dictionary of instances that are shared between one ore more other instances.
      /// When the list of shared instances reaches zero, the main instance is both destroyed and removed.
      /// </summary>
      private readonly IDictionary<IMonitorPageLifecycle, IList<IReportOrMonitorPageLifecycle>>
         _sharedInstances =
            new ConcurrentDictionary<IMonitorPageLifecycle, IList<IReportOrMonitorPageLifecycle>>();

      /// <summary>
      /// A dictionary of global singletons keyed by type. There can only be one each of a given type.
      /// </summary>
      private readonly IDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();

      /// <summary>
      /// Specifies that a type can be resolved as a specific type (can be different as long as related)
      /// Also sets the storage rules.  Defaults to "all".
      /// </summary>
      private readonly IDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>> _registeredTypeContracts =
         new ConcurrentDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>>();

      #endregion Private Variables

      public bool ThrowOnMultipleResolutions { get; set; }

      public bool ThrowOnStorageRuleCoercion { get; set; }

      #region Public Methods

      public object Resolve
         (
            Type typeRequestedT,
            StorageRules ruleRequested = StorageRules.AnyAccessLevel, 
            IReportOrMonitorPageLifecycle boundInstance = null, 
            Func<IEnumerable<IProvideCreatorAndStorageRule>, IProvideCreatorAndStorageRule> multipleContractSelecter = null
         )
      {
         object returnT = null;

         // Verify that we do not have an incompatible rules request and bound object
         if (boundInstance == null && 
             (ruleRequested == StorageRules.PerDependentInstance ||
              ruleRequested == StorageRules.SharedBetweenInstances))
         {
            throw new ArgumentException("SafeDI : Resolve: Cannot resolve a shared or dependent type without a bound type to have it rely on.");
         }

         // Look for a contract for this type.
         // The value we seek is in the IProvideCreatorAndStorageRule.
         var qualifyingTypeContracts = _registeredTypeContracts.Where(rc => rc.Value.ContainsKey(typeRequestedT));

         if (qualifyingTypeContracts.IsEmpty())
         {
            throw new ArgumentException("No registered contracts for the type " + typeRequestedT);
         }

         // If there is more than one master type, we should throw.
         if (qualifyingTypeContracts.Count() > 1)
         {
            throw new ArgumentException("SafeDI : Resolve: Too many contracts (" + qualifyingTypeContracts.Count() + ") for the resolvable type " + typeRequestedT);
         }

         // Get the actual registrations. 
         // Reduce down to the resolvable type -- we might have other resolvable types in this dictionary
         var qualifyingMasterType = qualifyingTypeContracts.First().Key;

         // We know this is not empty because of the previous queries.
         var qualifyingRegistrations = qualifyingTypeContracts.First().Value.Where(qtc => qtc.Key == typeRequestedT).Select(qtc => qtc.Value);

         // Filter against storage rules, if possible
         if (ruleRequested != StorageRules.AnyAccessLevel)
         {
            var rulesTest = qualifyingRegistrations.Where(qr => qr.ProvidedStorageRule == ruleRequested);
            if (rulesTest.IsEmpty())
            {
               if (ThrowOnStorageRuleCoercion)
               {
                  throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " + typeRequestedT +
                                              " using storage rule " + ruleRequested);
               }

               // ELSE proceed with the qualifying registrations
            }

            qualifyingRegistrations = rulesTest;
         }

         IProvideCreatorAndStorageRule resolutionToSeek = null;

         if (!qualifyingRegistrations.Any())
         {
            throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " + typeRequestedT);
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
                     throw new InvalidOperationException("SafeDI : Resolve: Cannot determine which one of " + qualifyingRegistrations.Count() + " registrations to use through the conflict resolver.");
                  }
               }

               // ELSE resolutionToSeek is valid
            }
            else if (ThrowOnMultipleResolutions)
            {
               throw new InvalidOperationException("SafeDI : Resolve: Cannot determine which one of " + qualifyingRegistrations.Count() + " registrations to use.  Please add a conflict resolver or change the registrations.");
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
            throw new ArgumentException("SafeDI : Resolve: Cannot find a registration for the type " + typeRequestedT);
         }

         // Now we have:
         // 1. A master type to create;
         // 2. A TypeRequestedT type to save as -- these might not be the same, but must relate class-design-wise
         // 3. A resolution to seek that might include an instance creator;
         // 4. If no instance creator, then we will use activator create instance

         // Verify that the result will be legal, since we will cast as TypeRequestedT
         if (!qualifyingMasterType.IsMainTypeOrAssignableFromMainType(typeRequestedT))
         {
            throw new InvalidOperationException("SafeDI : Resolve: Cannot save an instance of " + qualifyingMasterType + " as " + typeRequestedT);
         }

         // See if we are asked for a shared instance; if so, that might already exist
         if (ruleRequested == StorageRules.SharedBetweenInstances)
         {
            var foundSharedInstance = _sharedInstances.FirstOrDefault(si => si.Key.GetType() == typeRequestedT);

            // If valid (not empty)
            if (foundSharedInstance.IsNotAnEqualObjectTo(default(KeyValuePair<IMonitorPageLifecycle, IList<IReportOrMonitorPageLifecycle>>)))
            {
               // Add our bound object to the list... maybe make sure it's not there already.. 
               if (!foundSharedInstance.Value.Contains(boundInstance))
               {
                  foundSharedInstance.Value.Add(boundInstance);
               }

               // Return the key object, as that is the one that is being shared between the list of bound instances
               returnT = foundSharedInstance.Key;
               // as TypeRequestedT;
               returnT = Convert.ChangeType(returnT, typeRequestedT);

               if (returnT == null)
               {
                  throw new InvalidOperationException("SafeDI : Resolve: Failed to share a found instance of type " +
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
               throw new InvalidOperationException("SafeDI : Resolve: Could not create an object using the provided constructor.");
            }

            // ELSE the instated object is valid.
            // Proceed as if we had created the object using activator create instance.
         }

         if (instantiatedObject == null)
         {
            // No choice but to use activator create instance
            // Note that we actually *create* the qualifyingMasterType and then *save as* the TypeRequestedT

            // Get the constructors and order by the fewest parameters -- 
            // HACK -- not very smart
            var availableConstructors = qualifyingMasterType.GetConstructors().OrderBy(c => c.GetParameters().Count());

            if (availableConstructors.IsEmpty())
            {
               throw new InvalidOperationException("SafeDI : Resolve: Could not find a valid constructor for type requested: " + qualifyingMasterType);
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
               else
               {
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
                        Debug.WriteLine("SafeDI: Resolve: Exception on attempt to instantiate " + qualifyingMasterType + " using one of its constructors. Will try another constructor...");
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
               }

SKIP_THIS_ONE:;
            }
         }

         if (instantiatedObject == null)
         {
            throw new InvalidOperationException("SafeDI : Resolve: Could not construct an object for base type " + qualifyingMasterType);
         }

         // Now we have an instantiated object.
         // Determine its storage before delivering it.
         returnT = Convert.ChangeType(instantiatedObject, typeRequestedT);

         if (returnT == null)
         {
            throw new InvalidOperationException("SafeDI : Resolve: Failed to type-cast my constructed object " + instantiatedObject.GetType() + " as " + typeRequestedT);
         }

         return returnT;
      }

      /// <summary>
      ///    Adds a key-value pair with a class type and an instance of that class.
      /// </summary>
      /// <typeparam name="TClass">The class type.</typeparam>
      /// <param name="instance">The instance of the type.</param>
      public void CreateSingletonInstance<TClass>(object instance)
      {
         _singletonInstances[typeof(TClass)] = instance;
      }

      /// <summary>
      /// Adds a list of bound instances to a single shared instance.
      /// </summary>
      /// <typeparam name="TSharedInstance">The shared instance.</typeparam>
      /// <param name="sharedInstance">The parent instance.</param>
      /// <param name="instances">The bound instances.</param>
      public void CreateSharedInstances<TSharedInstance>(TSharedInstance sharedInstance,
         params IReportOrMonitorPageLifecycle[] instances)
         where TSharedInstance : IMonitorPageLifecycle
      {
         ManageSharedInstances(sharedInstance, true, instances);
      }

      /// <summary>
      /// Adds a list of types that the parent type can be resolved as.
      /// Includes creators and storage rules.
      /// </summary>
      /// <typeparam name="TParent">The generic parent type</typeparam>
      /// <param name="creatorsAndRules">The list of object creators and rules.  The object creators can be null.</param>
      public void RegisterTypeContracts<TParent>(IDictionary<Type, IProvideCreatorAndStorageRule> creatorsAndRules)
      {
         if (_registeredTypeContracts[typeof(TParent)] == null)
         {
            _registeredTypeContracts[typeof(TParent)] = creatorsAndRules;
         }
         else
         {
            // Augment whatever is there
            foreach (var keyValuePair in creatorsAndRules)
            {
               _registeredTypeContracts[typeof(TParent)].Add(keyValuePair);
            }
         }
      }

      /// <summary>
      ///    Removes the dictionary entry for the instance of a parent class type.
      /// </summary>
      /// <typeparam name="TClass">The generic parent type</typeparam>
      public void RemoveSingletonInstance<TClass>()
      {
         if (_singletonInstances.ContainsKey(typeof(TClass)))
         {
            _singletonInstances.Remove(typeof(TClass));
         }
      }

      /// <summary>
      /// Removes a list of bound instances from a single shared instance.
      /// </summary>
      /// <typeparam name="TSharedInstance">The shared instance.</typeparam>
      /// <param name="sharedInstance">The parent instance.</param>
      /// <param name="instances">The bound instances.</param>
      public void RemoveParentChildInstanceDependencies<TSharedInstance>(TSharedInstance sharedInstance,
         params IReportOrMonitorPageLifecycle[] instances)
         where TSharedInstance : IMonitorPageLifecycle
      {
         ManageSharedInstances(sharedInstance, false, instances);
      }

      /// <summary>
      /// Removes a list of types that the parent type can be resolved as.
      /// Includes creators and storage rules.
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

      #region Private Methods

      /// <summary>
      /// This list is "backwards" --
      /// The key is an object which we instantiate (or which the user's creator function instantiates).
      /// The key type must implement one of the lifecycle interfaces.
      /// Because the key will die whenever the list of dependent types dies, it *must* implement IMonitorPageLifecycle.
      /// It *cannot* be an originating page, so it *cannot* implement IReportPageLifecycle.
      /// The list "value" is a collection of all of the instances that share that single variable.
      /// These types must also implement one of the lifecycle interfaces.
      /// </summary>
      /// <typeparam name="TSharedInstance">The type of the shared instance</typeparam>
      /// <param name="sharedInstance">The actual shared instance</param>
      /// <param name="addThem"></param>
      /// <param name="instances"></param>
      protected void ManageSharedInstances<TSharedInstance>(TSharedInstance sharedInstance, bool addThem, IReportOrMonitorPageLifecycle[] instances)
         where TSharedInstance : IMonitorPageLifecycle
      {
         // Use the same type checker that we apply to the type dependency case. These are instances
         // of *types*
         var instanceTypes = instances.Select(i => i.GetType()).ToArray();

         var illegalTypes = instanceTypes.Where(t => t.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance))).ToList();

         if (illegalTypes.IsNotEmpty())
         {
            // Report the first error.
            throw new ArgumentException("SafeDI: ManageSharedInstances: cannot share " + illegalTypes[0] +
                                        " with itself, even if derived.");
         }

         // Verify that the instance types are *not* directly assignable from the shared instance,
         // as that is circular.
         foreach (var type in instanceTypes)
         {
            if (type.IsMainTypeOrAssignableFromMainType(typeof(TSharedInstance)))
            {
               throw new ArgumentException("SafeDI: ManageSharedInstances: " + type +
                                           " is an illegal circular reference back to the parent " + typeof(TSharedInstance));
            }
         }

         var attachedInstances = _sharedInstances[sharedInstance];

         if (attachedInstances == null)
         {
            if (addThem)
            {
               attachedInstances = new List<IReportOrMonitorPageLifecycle>();
               _sharedInstances.Add(sharedInstance, attachedInstances);
            }
            else
            {
               return;
            }
         }

         foreach (var instance in instances)
         {
            var found = attachedInstances.Contains(instance);

            if (addThem)
            {
               if (!found)
               {
                  attachedInstances.Add(instance);
               }
            }
            else
            {
               if (found)
               {
                  attachedInstances.Remove(instance);
               }
            }
         }
      }

      protected static bool CanManageRegisteredTypeContracts<TParent>(bool addThem, IDictionary<Type, IProvideCreatorAndStorageRule> typesAndRules)
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
                  throw new ArgumentException("SafeDI: CanManageRegisteredTypeContracts: cannot resolve " + typeof(TParent) + " to " +
                                              type + " because they have no class or interface relationship.");
               }
            }
         }

         return true;
      }

      #endregion Private Methods
   }
}

/*
   public static T Resolve<T>()
   {
      return (T)Resolve(typeof(T));
   }

   public static object Resolve(Type contract)
   {
      var implementation = types[contract];
      var constructor = implementation.GetConstructors()[0];
      var constructorParameters = constructor.GetParameters();

      if (constructorParameters.Length == 0)
      {
         return Activator.CreateInstance(implementation);
      }

      var parameters = new List<object>(constructorParameters.Length);

      foreach (ParameterInfo parameterInfo in constructorParameters)
      {
         parameters.Add(Resolve(parameterInfo.ParameterType));
      }

      return constructor.Invoke(parameters.ToArray());
   }

         if (!(typeof(TParent).IsMainTypeOrAssignableFromMainType(typeof(IReportOrMonitorPageLifecycle))))
         {
            throw new ArgumentException(
               "SafeDI: CreateParentChildTypeDependencies: The parent type must implement either " +
               nameof(IReportPageLifecycle) + " or " + nameof(IMonitorPageLifecycle));
         }

         /// <summary>
      /// A dictionary of dependencies based on type alone.
      /// Can be left empty.  If used, these will over-ride attempts to resolve
      /// </summary>
      private readonly IDictionary<Type, IList<Type>> _parentChildTypeDependencies =
         new ConcurrentDictionary<Type, IList<Type>>();


      private void ManageParentChildTypeDependencies<TParent>(bool addThem, Type[] types)
      {
         if (types.IsEmpty())
         {
            return;
         }

         VerifyTypeLegality<TParent>(types);

         var existingDependencies = _parentChildTypeDependencies[typeof(TParent)];

         if (existingDependencies == null)
         {
            if (addThem)
            {
               existingDependencies = new List<Type>();
               _parentChildTypeDependencies.Add(typeof(TParent), existingDependencies);
            }
            else
            {
               return;
            }
         }

         foreach (var type in types)
         {
            var found = existingDependencies.Contains(type);

            if (addThem)
            {
               if (!found)
               {
                  existingDependencies.Add(type);
               }
            }
            else
            {
               if (found)
               {
                  existingDependencies.Remove(type);
               }
            }
         }
      }

      /// <summary>
      ///    Adds a list of parent-child dependencies based on type.
      /// </summary>
      /// <typeparam name="TParent">The generic parent type</typeparam>
      /// <param name="types">The types that are dependent on the parent type.</param>
      public void CreateParentChildTypeDependencies<TParent>(params Type[] types)
         where TParent : IReportOrMonitorPageLifecycle
      {
         ManageParentChildTypeDependencies<TParent>(true, types);
      }

      /// <summary>
      ///    Removes a list of parent-child dependencies based on type.
      /// </summary>
      /// <typeparam name="TParent">The generic parent type</typeparam>
      /// <param name="types">The types that are dependent on the parent type.</param>
      public void RemoveParentChildTypeDependencies<TParent>(params Type[] types)
         where TParent : IReportOrMonitorPageLifecycle
      {
         ManageParentChildTypeDependencies<TParent>(false, types);
      }


*/
