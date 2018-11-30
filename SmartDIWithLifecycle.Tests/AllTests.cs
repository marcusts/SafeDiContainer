namespace SafeDIWithLifecycle.Tests
{
   using Moq;
   using NUnit.Framework;
   using NUnit.Framework.Interfaces;
   using SafeDI.Lib;
   using TestClasses;

   [TestFixture]
   public class AllTests
   {
      private ISafeDIContainerWithExposedProperties _container = new SafeDIContainerWithExposedProperties();

      [SetUp]
      public void SetUpEachTest()
      {
      }

      [TearDown]
      public void TearDownEachTest()
      {
         _container.ClearContainer();
      }

      [Test]
      public void CanSaveAGlobalSingleton()
      {
         // Try multiple classes and interfaces
         GlobalSingletonTest<IAmSimple, SimpleClass>();
      }

      private void GlobalSingletonTest<InterfaceT, TypeT>()
         where TypeT : class, InterfaceT
         where InterfaceT : class
      {
         // Register a type as an interface -- should not be able to fetch it as the base class
         _container.RegisterType<TypeT>(StorageRules.GlobalSingleton, null, false, typeof(InterfaceT));

         var classAsInterface = _container.Resolve<InterfaceT>();

         Assert.IsNotNull(classAsInterface, "Cannot resolve a properly registered interface.");
         Assert.IsTrue(_container.ExposedGlobalSingletons.Count == 1, "The container did not store a singleton for each type registered.");

         // Try another instance to verify that it is the same memory reference in memory
         var secondClassAsInterface = _container.Resolve<InterfaceT>();
         Assert.IsTrue(ReferenceEquals(classAsInterface, secondClassAsInterface), "Two resolves of the same registered global interface returned different instances.");

         // Try to get the base class, even though we did not register it.
         var classAsBaseClass = _container.Resolve<TypeT>();

         Assert.IsNull(classAsBaseClass, "Illegally acquired a base class without a legal registration.");

         // Register the base class (normally, this would have been done at the original RegisterType above)
         _container.RegisterType<TypeT>(StorageRules.GlobalSingleton, null, true);
         classAsBaseClass = _container.Resolve<TypeT>();

         Assert.IsNotNull(classAsBaseClass, "Failed to acquire a properly registered base class.");
         Assert.IsTrue(_container.ExposedGlobalSingletons.Count == 2, "The container did not store a singleton for each type registered.");
      }
   }
}