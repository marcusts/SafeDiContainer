using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LifecycleAware.Tests
{
   using LifecycleAware;

   [TestClass]
   public class AllTests
   {
      private ISafeDIContainerWithLifecycle _diContainer = null;

      [TestInitialize]
      public void InitializeTests()
      {
         _diContainer = new SafeDIContainerWithLifecycle();
      }

      [TestCleanup]
      public void CleanUpTests()
      {
         _diContainer.Dispose();
      }

      [TestMethod]
      public void TestMethod1()
      {
      }
   }
}
