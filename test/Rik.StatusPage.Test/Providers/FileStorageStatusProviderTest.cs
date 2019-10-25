using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Test.Providers
{
    [TestFixture]
    public class FileStorageStatusProviderTest
    {
        private const string NAME = "File storage";
        private const string STORAGE_PATH = @"C:\Temp\";

        [Test]
        public async Task CheckStatusTest()
        {
            var provider = BuildProvider();

            var externalUnit = await provider.CheckStatusAsync(default);

            Assert.AreEqual(NAME, externalUnit.Name);
            Assert.AreEqual(STORAGE_PATH, externalUnit.Uri);
            Assert.AreEqual(UnitStatus.Ok, externalUnit.Status);
            Assert.IsFalse(externalUnit.StatusMessageSpecified);
            Assert.IsFalse(externalUnit.ServerPlatformSpecified);
        }

        [Test]
        public void StoragePathMissingTest()
        {
            var ex = Assert.Throws<ArgumentException>(() => BuildProvider(options => options.StoragePath = string.Empty));
            StringAssert.StartsWith("File storage path is required", ex.Message);
        }

        [Test]
        public async Task CheckStatusStoragePathNotExistsTest()
        {
            var provider = BuildProvider(options => options.StoragePath = @"C:\RandomFolderThatDoesNotExist");

            var externalUnit = await provider.CheckStatusAsync(default);

            Assert.AreEqual(UnitStatus.NotOk, externalUnit.Status);
            Assert.IsTrue(externalUnit.StatusMessageSpecified);
            StringAssert.StartsWith("File storage path doesn't exist or is not accessible", externalUnit.StatusMessage);
        }

        private static FileStorageStatusProvider BuildProvider(Action<FileStorageStatusProviderOptions> customizer = null)
        {
            var options = new FileStorageStatusProviderOptions(NAME)
            {
                StoragePath = STORAGE_PATH,
                RequireRead = true,
                RequireWrite = true
            };

            customizer?.Invoke(options);

            return new FileStorageStatusProvider(options);
        }

        // TODO: access rights tests
    }
}