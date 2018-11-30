namespace SmartDIWithLifeCycle.MSTests
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using SharedForms.Common.Utils;
   using SmartDI.Lib;
   using TestClasses;

   [TestClass]
   public class AllTests : IDisposable
   {
      #region Private Fields

      private readonly ISmartDIContainerForUnitTesting _container = new SafeDiContainerForUnitTesting();

      #endregion Private Fields

      #region Public Methods

      public void Dispose()
      {
         _container?.Dispose();
         GC.SuppressFinalize(this);
      }

      [TestInitialize]
      public void SetUpEachTest() => _container.ClearUnitTestExceptions();

      [TestCleanup]
      public void TearDownEachTest() => _container.ResetUnitTestContainer();

      [TestMethod]
      public void TestArguments()
      {
         // Attempt to register an interface as a base type.  Can also use an abstract type, etc.
         _container.RegisterType<IAmSimple>(StorageRules.DoNotStore);
         AssertContainerHasThrownArgumentException("Illegally assigned an interface as a base class type.  Interfaces cannot be instantiated.");

         _container.ResetUnitTestContainer();

         ///////////////////////////////////////////////////
         // ThrowOnMultipleRegisteredTypesForOneResolvedType
         ///////////////////////////////////////////////////

         // 1. Try adding more than one contract; if ThrowOnMultipleRegisteredTypesForOneResolvedType is false (default), this will work.
         _container.ExposedThrowOnMultipleRegisteredTypesForOneResolvedType = false;
         _container.RegisterTypeAsInterface<SimpleClass>(typeof(IAmSimple));
         _container.RegisterTypeAsInterface<AnotherSimpleClass>(typeof(IAmSimple));
         AssertContainerHasRaisedNoExceptions();
         _container.Resolve<IAmSimple>();
         AssertContainerHasRaisedNoExceptions();

         _container.ResetUnitTestContainer();

         // 2. A follow-up to the last case, with a twist: set ThrowOnMultipleRegisteredTypesForOneResolvedType true (non-default) initially so the registration becomes ILLEGAL:
         _container.ExposedThrowOnMultipleRegisteredTypesForOneResolvedType = true;
         _container.RegisterTypeAsInterface<SimpleClass>(typeof(IAmSimple));
         _container.RegisterTypeAsInterface<AnotherSimpleClass>(typeof(IAmSimple));
         AssertContainerHasThrownArgumentException(
           "ThrowOnMultipleRegisteredTypesForOneResolvedType is true but no error was thrown for registering two master classes for the same resolved interface.");

         _container.ResetUnitTestContainer();

         // 3. Finally, a hybrid: we start with legal values and then cause them to become illegal
         // Try adding more than one contract; leave ThrowOnMultipleRegisteredTypesForOneResolvedType at false (default) so this will work.
         _container.ExposedThrowOnMultipleRegisteredTypesForOneResolvedType = false;
         _container.RegisterTypeAsInterface<SimpleClass>(typeof(IAmSimple));
         _container.RegisterTypeAsInterface<AnotherSimpleClass>(typeof(IAmSimple));
         AssertContainerHasRaisedNoExceptions();

         // 4. Now reverse our decision; throw multiple resolutions
         _container.ExposedThrowOnMultipleRegisteredTypesForOneResolvedType = true;
         _container.Resolve<IAmSimple>();
         AssertContainerHasThrownArgumentException("ThrowOnMultipleRegisteredTypesForOneResolvedType was true on registration but false on resolve, yet we were able to resolve without error.");

         _container.ResetUnitTestContainer();

         /////////////////////////////////////////////////
         // ThrowOnAttemptToAssignDuplicateContractSubType
         /////////////////////////////////////////////////

         // This is a registration-only constraint, so will be tested through registrations.

         // 1. SetThrowOnAttemptToAssignDuplicateContractSubType to false (default), and try to replace a resolution contract; this will succeed
         _container.ExposedThrowOnAttemptToAssignDuplicateContractSubType = false;
         _container.RegisterType<SimpleClass>(StorageRules.DoNotStore, null, false, typeof(IAmSimple));
         _container.RegisterType<SimpleClass>(StorageRules.GlobalSingleton, null, false, typeof(IAmSimple));
         AssertContainerHasRaisedNoExceptions();

         // 2. SetThrowOnAttemptToAssignDuplicateContractSubType to true, which should throw an error under the same scenario thatn just succeeded above.
         _container.ExposedThrowOnAttemptToAssignDuplicateContractSubType = true;
         _container.RegisterType<SimpleClass>(StorageRules.DoNotStore, null, false, typeof(IAmSimple));
         _container.RegisterType<SimpleClass>(StorageRules.GlobalSingleton, null, false, typeof(IAmSimple));
         AssertContainerHasThrownArgumentException("With ThrowOnAttemptToAssignDuplicateContractSubType true, was allowed to over-write a sub contract.");
      }

      [TestMethod]
      public void TestComplexConstructorsAndRecursion()
      {
         // Stack up services, etc., and use them to create very complex constructors

         // 1. Try to register a complex constructor before the required parameters shave been registered.
         _container.RegisterType<ServiceThree>();
         var serviceThree = _container.Resolve<ServiceThree>();

         // Also try the interface
         _container.RegisterTypeAsInterface<ServiceThree>(typeof(IServiceThree));
         var serviceThreeInterface = _container.Resolve<IServiceThree>();

         AssertContainerHasThrownArgumentException("Was allowed to resolve a complex class without first registering its parameters.");

         _container.ResetUnitTestContainer();

         // 2. Register the parameters as expected.
         _container.RegisterTypeAsInterface<ServiceOne>(typeof(IServiceOne));
         _container.RegisterTypeAsInterface<ServiceTwo>(typeof(IServiceTwo));
         _container.RegisterTypeAsInterface<ServiceThree>(typeof(IServiceThree));
         serviceThreeInterface = _container.Resolve<IServiceThree>();
         AssertContainerHasRaisedNoExceptions();

         _container.ResetUnitTestContainer();

         // 3. Try to register a subtly recursive service
         _container.RegisterTypeAsInterface<ServiceOne>(typeof(IServiceOne));
         _container.RegisterTypeAsInterface<ServiceTwo>(typeof(IServiceTwo));
         _container.RegisterTypeAsInterface<ServiceThree>(typeof(IServiceThree));

         // Verify that we cannot can send in ourselves as a bound parent to a class that we derive

         _container.RegisterTypeAsInterface<ServiceOne>(typeof(IServiceOne));
         _container.RegisterTypeAsInterface<RecursiveService>(typeof(IRecursiveService));

         // This class derives from ServiceOne
         var recursiveService = _container.Resolve<IRecursiveService>();

         // If  we resolve as a bound dependency using the recursive service, that should be illegal
         var boundService = _container.Resolve<ServiceOne>(StorageRules.SharedDependencyBetweenInstances, recursiveService);
         AssertContainerHasThrownArgumentException("Was allowed to resolve a service that is bound to a class that derives it.");
      }

      [TestMethod]
      public void TestConflictResolution()
      {
         _container.ExposedThrowOnMultipleRegisteredTypesForOneResolvedType = false;

         // NOTE: The registered types are sorted, so DerivedSimpleClass ends up in front of SimpleClass.
         //      Without a conflict resolver, the container will make the first available choice: the DerivedSimpleClass.
         _container.RegisterTypeAsInterface<DerivedSimpleClass>(typeof(IAmSimple));
         _container.RegisterTypeAsInterface<SimpleClass>(typeof(IAmSimple));

         // Ask to derive the interface without using a conflict resolver
         var simple = _container.Resolve<IAmSimple>();
         Assert.IsTrue(simple is DerivedSimpleClass,
           "Was given the second registered type randomly by the container without a conflict resolver.");

         // Now use the conflict resolver to make the choice -- forbid the DerivedSimpleClassss
         simple =
           _container.Resolve<IAmSimple>
              (
                StorageRules.AnyAccessLevel,
                null,
                ForbidDerivedSimpleClass
              );

         Assert.IsFalse(simple is DerivedSimpleClass,
           "Was given the first registered type when the resolver asked for the second one.");
      }

      [TestMethod]
      public void TestGlobalSingletons()
      {
         // Try multiple classes and interfaces
         GlobalSingletonTestByGenericType<IAmSimple, SimpleClass>();

         _container.ResetUnitTestContainer();

         GlobalSingletonTestByGenericType<IAmAParent, ParentClass>();

         _container.ResetUnitTestContainer();

         // Verify that if we ask for a null global singleton, we get a new global singleton back -- not null
         _container.RegisterType<SimpleClass>(StorageRules.GlobalSingleton);
         var globalClass = _container.Resolve<SimpleClass>();
         Assert.IsTrue(_container.ExposedGlobalSingletons.ContainsKey(typeof(SimpleClass)), "Failed to store a global singleton after resolve.");

         // Kill that instance directly
         _container.ExposedGlobalSingletons[typeof(SimpleClass)] = null;

         // Now ask for another one
         globalClass = _container.Resolve<SimpleClass>();
         Assert.IsNotNull(globalClass, "Returned a null global singleton.");

         // Also see if that type got cleared, which is a safety measure inside the container
         var classToCheck = _container.ExposedGlobalSingletons[typeof(SimpleClass)];
         Assert.IsNotNull(classToCheck, "Failed to remove a null global singleton after resolve.");
      }

      [TestMethod]
      public void TestStorageRules()
      {
         // If registered with "any access level" storage rule, can resolve for any storage rule.

         _container.RegisterType<SimpleClass>(StorageRules.AnyAccessLevel);
         var parent = new ParentClass();

         var testClass = _container.Resolve<SimpleClass>(StorageRules.DoNotStore);
         AssertContainerHasRaisedNoExceptions();
         Assert.IsNotNull(testClass, "Could not resolve an 'any' registration as a 'do not store' variable.");

         testClass = _container.Resolve<SimpleClass>(StorageRules.GlobalSingleton);
         AssertContainerHasRaisedNoExceptions();
         Assert.IsNotNull(testClass, "Could not resolve an 'any' registration as a 'global singleton' variable.");

         testClass = _container.Resolve<SimpleClass>(StorageRules.SharedDependencyBetweenInstances, parent);
         AssertContainerHasRaisedNoExceptions();
         Assert.IsNotNull(testClass, "Could not resolve an 'any' registration as a 'shared' variable.");

         _container.ResetUnitTestContainer();

         // If registered with a 'shared' storage level, and resolved with another, will throw (except 'any', which always succeeds)

         _container.RegisterType<SimpleClass>(StorageRules.SharedDependencyBetweenInstances);

         // These tests require that we pass in a bound instance

         testClass = _container.Resolve<SimpleClass>(StorageRules.DoNotStore, parent);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'shared' to 'do not store' illegally.");

         _container.ClearUnitTestExceptions();

         testClass = _container.Resolve<SimpleClass>(StorageRules.GlobalSingleton, parent);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'shared' to 'global singleton' illegally.");

         _container.ClearUnitTestExceptions();

         // Should succeed
         testClass = _container.Resolve<SimpleClass>(StorageRules.AnyAccessLevel, parent);
         AssertContainerHasRaisedNoExceptions();

         _container.ResetUnitTestContainer();

         // If registered with a 'global singleton' storage level, and resolved with another, will throw (except 'any', which always succeeds)

         _container.RegisterType<SimpleClass>(StorageRules.GlobalSingleton);

         testClass = _container.Resolve<SimpleClass>(StorageRules.DoNotStore);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'global singleton' to 'do not store' illegally.");

         _container.ClearUnitTestExceptions();

         testClass = _container.Resolve<SimpleClass>(StorageRules.SharedDependencyBetweenInstances, parent);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'global singleton' to 'shared' illegally.");

         _container.ClearUnitTestExceptions();

         // Should succeed
         testClass = _container.Resolve<SimpleClass>(StorageRules.AnyAccessLevel);
         AssertContainerHasRaisedNoExceptions();

         _container.ResetUnitTestContainer();

         // If registered with a 'do not store' storage level, and resolved with another, will throw (except 'any', which always succeeds)

         _container.RegisterType<SimpleClass>(StorageRules.DoNotStore);

         testClass = _container.Resolve<SimpleClass>(StorageRules.GlobalSingleton);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'do not store' to 'global singleton' illegally.");

         _container.ClearUnitTestExceptions();

         testClass = _container.Resolve<SimpleClass>(StorageRules.SharedDependencyBetweenInstances, parent);
         AssertContainerHasThrownArgumentException("Was allowed to resolve by coercing from 'do not store' to 'shared' illegally.");

         _container.ClearUnitTestExceptions();

         // Should succeed
         testClass = _container.Resolve<SimpleClass>(StorageRules.AnyAccessLevel);
         AssertContainerHasRaisedNoExceptions();

         _container.ResetUnitTestContainer();

         // Try to add duplicate storage rules; this should be ignored.
         _container.RegisterType<SimpleClass>(StorageRules.DoNotStore, () => new SimpleClass());

         // These three statements should now be true
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.ContainsKey(typeof(SimpleClass)));
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts[typeof(SimpleClass)].Keys.Contains(typeof(SimpleClass)));
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts[typeof(SimpleClass)].Values.Count == 1);

         // Now try to register again using a different constructor for the same type.
         _container.RegisterType<SimpleClass>(StorageRules.DoNotStore, SimpleClass_Static.CreateSimpleInstance);

         // Retest; the conditions should not change, because we cannot legally add two constructors for the same type and storage rule.
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.ContainsKey(typeof(SimpleClass)));
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts[typeof(SimpleClass)].Keys.Contains(typeof(SimpleClass)));
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts[typeof(SimpleClass)].Values.Count == 1);
         // NOTE that no error is issued; the new rule over-writes the old one.

         // The bound storage rules are complex, so require specialized testing

         _container.ResetUnitTestContainer();

         // Register as a type for shared storage
         _container.RegisterType<SimpleClass>(StorageRules.SharedDependencyBetweenInstances);

         // Attempt to resolve without a parent
         testClass = _container.Resolve<SimpleClass>(StorageRules.AnyAccessLevel);
         AssertContainerHasThrownArgumentException("Was allowed to resolve a shared instance without a bound parent");

         _container.ClearUnitTestExceptions();

         // Attempt to bind a resolved class to itself as its own parent
         testClass = _container.Resolve<SimpleClass>(StorageRules.AnyAccessLevel, testClass);
         AssertContainerHasThrownArgumentException("Was allowed to resolve a shared instance with the same type as the parent");

         _container.ClearUnitTestExceptions();

         // Normal procedure -- should work
         testClass = _container.Resolve<SimpleClass>(boundInstance: parent);
         AssertContainerHasRaisedNoExceptions();

         // Now add a second parent of the same type, but a different memory reference
         var secondParent = new ParentClass();

         // Should succeed
         var secondTestClass = _container.Resolve<SimpleClass>(boundInstance: secondParent);
         AssertContainerHasRaisedNoExceptions();

         // More importantly, this resolved class should be the same instance as the original test class, since it is shared.
         Assert.IsTrue(ReferenceEquals(testClass, secondTestClass), "Resolved a 'shared' variable as two separate instances rather than sharing one instance.");

         // If we remove both bound instances...
         _container.ContainerObjectIsDisappearing(parent);

         // Last gut check: should still be able to get a shared instance
         Assert.IsTrue(_container.ExposedSharedInstancesWithBoundMembers.Count == 1, "Removed a shared instance that was not actually orphaned.");

         _container.ContainerObjectIsDisappearing(secondParent);

         // The shared instance will have no bound parents, so should be removed
         Assert.IsTrue(_container.ExposedSharedInstancesWithBoundMembers.IsEmpty(), "Left an orphaned shared instance inside the container.");
      }

      [TestMethod]
      public void TestUnregistration()
      {
         // ALL ACCESS
         _container.RegisterType<SimpleClass>(StorageRules.AnyAccessLevel);
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsNotEmpty(), "Failed to register a type for any access level.");
         _container.UnregisterTypeContracts<SimpleClass>();
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsEmpty(), "Failed to unregister a type for any access level.");

         _container.ResetUnitTestContainer();

         _container.RegisterTypeAsInterface<SimpleClass>(typeof(IAmSimple));
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsNotEmpty(), "Failed to register an interface type.");

         // Should fail; the base class is not mentioned, so cannot unregister
         _container.UnregisterTypeContracts<IAmSimple>();
         AssertContainerHasThrownArgumentException("Was allowed to unregister an interface without mentioning the base class.");

         // Naming the base class clears all sub-members as well as the main type
         _container.UnregisterTypeContracts<SimpleClass>();
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsEmpty(), "Failed to unregister a base class type.");

         _container.ResetUnitTestContainer();

         // GLOBAL SINGLETONS
         _container.RegisterType<SimpleClass>(StorageRules.GlobalSingleton);
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsNotEmpty(), "Failed to register a global singleton.");
         _container.UnregisterTypeContracts<SimpleClass>();
         Assert.IsTrue(_container.ExposedRegisteredTypeContracts.IsEmpty(), "Failed to unregister a global singleton.");
      }

      #endregion Public Methods

      #region Private Methods

      private static ConflictResolution ForbidDerivedSimpleClass(IDictionary<Type, IDictionary<Type, IProvideCreatorAndStorageRule>> registrations)
      {
         foreach (var registration in registrations.Where(r => r.Key != typeof(DerivedSimpleClass)))
         {
            return new ConflictResolution()
            {
               MasterType = registration.Key,
               TypeToCastWithStorageRule = registration.Value.First()
            };
         }

         return null;
      }

      private void AssertContainerHasRaisedNoExceptions()
      {
         Assert.IsTrue(_container.IsArgumentException.IsEmpty(), "Container threw an unexpected argument exception ->" + _container.IsArgumentException + "<-");
         Assert.IsTrue(_container.IsOperationException.IsEmpty(), "Container threw an unexpected operation exception ->" + _container.IsOperationException + "<-");
      }

      private void AssertContainerHasThrownArgumentException(string message) => Assert.IsTrue(_container.IsArgumentException.IsNotEmpty(), message);

      private void AssertContainerHasThrownOperationException(string message) => Assert.IsTrue(_container.IsOperationException.IsNotEmpty(), message);

      private void GlobalSingletonTestByGenericType<InterfaceT, TypeT>()
                         where TypeT : class, InterfaceT
       where InterfaceT : class
      {
         // Register a type as an interface -- should not be able to fetch it as the base class
         _container.RegisterType<TypeT>(StorageRules.GlobalSingleton, null, false, typeof(InterfaceT));

         _container.ClearUnitTestExceptions();

         var classAsInterface = _container.Resolve<InterfaceT>();
         AssertContainerHasRaisedNoExceptions();
         Assert.IsNotNull(classAsInterface, "Cannot resolve a properly registered interface.");
         Assert.IsTrue(_container.ExposedGlobalSingletons.Count == 1, "The container did not store a singletons.");

         _container.ClearUnitTestExceptions();

         // Try another instance to verify that it is the same memory reference in memory
         var secondClassAsInterface = _container.Resolve<InterfaceT>();
         AssertContainerHasRaisedNoExceptions();
         Assert.IsTrue(ReferenceEquals(classAsInterface, secondClassAsInterface), "Two resolves of the same registered global interface returned different instances.");

         _container.ClearUnitTestExceptions();

         // Try to get the base class, even though we did not register it.
         var classAsBaseClass = _container.Resolve<TypeT>();
         AssertContainerHasThrownArgumentException("Illegally acquired a base class without a legal registration.");

         _container.ClearUnitTestExceptions();

         // Register the base class (normally, this would have been done at the original RegisterType above)
         _container.RegisterType<TypeT>(StorageRules.GlobalSingleton, null, true);
         classAsBaseClass = _container.Resolve<TypeT>();
         AssertContainerHasRaisedNoExceptions();

         // The container must store an additional global instance because they are keyed by type.
         Assert.IsTrue(_container.ExposedGlobalSingletons.Count == 2, "The container did not store a singleton for both types registered.");

         // Also must be able to fetch the class
         Assert.IsNotNull(classAsBaseClass, "Failed to acquire a properly registered base class.");
      }

      #endregion Private Methods
   }
}