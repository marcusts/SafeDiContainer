using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeDI.Tests
{
   using Lib;

   [TestClass]
   public class AllTests
   {
      private ISafeDIContainer _diContainer = null;

      [TestInitialize]
      public void InitializeTests()
      {
         _diContainer = new SafeDIContainer();
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
