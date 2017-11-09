using System;
using NUnit.Framework;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Test.Providers
{
    [TestFixture]
    public class FileStorageStatusProviderTest
    {
        private StatusProviderConfigurationElement configurationElement;

        [SetUp]
        public void SetUpFileStorageStatusProviderTest()
        {
            BuildConfigurationElement();
        }

        [Test]
        public void CheckStatusTest()
        {
            var provider = BuildProvider(configurationElement);

            var externalUnit = provider.CheckStatus();

            Assert.AreEqual(configurationElement.Name, externalUnit.Name);
            Assert.AreEqual(configurationElement.StoragePath, externalUnit.Uri);
            Assert.AreEqual(UnitStatus.Ok, externalUnit.Status);
            Assert.IsFalse(externalUnit.StatusMessageSpecified);
            Assert.IsFalse(externalUnit.ServerPlatformSpecified);
        }

        [Test]
        public void StoragePathMissingTest()
        {
            configurationElement.StoragePath = string.Empty;

            var ex = Assert.Throws<ArgumentException>(() => BuildProvider(configurationElement));
            StringAssert.StartsWith("File storage path is required", ex.Message);
        }

        [Test]
        public void CheckStatusStoragePathNotExistsTest()
        {
            configurationElement.StoragePath = @"C:\RandomFolderThatDoesNotExist";
            var provider = BuildProvider(configurationElement);

            var externalUnit = provider.CheckStatus();

            Assert.AreEqual(UnitStatus.NotOk, externalUnit.Status);
            Assert.IsTrue(externalUnit.StatusMessageSpecified);
            StringAssert.StartsWith("File storage path doesn't exist or is not accessible", externalUnit.StatusMessage);
        }

        // TODO: access rights tests

        private void BuildConfigurationElement()
        {
            configurationElement = new StatusProviderConfigurationElement
            {
                Name = "File storage",
                StoragePath = @"C:\Temp\",
                RequireRead = true,
                RequireWrite = true
            };
        }

        private static FileStorageStatusProvider BuildProvider(StatusProviderConfigurationElement element)
        {
            return new FileStorageStatusProvider(element);
        }
    }
}