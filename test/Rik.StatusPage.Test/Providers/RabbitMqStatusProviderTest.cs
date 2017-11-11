using System;
using NUnit.Framework;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Test.Providers
{
    [TestFixture]
    public class RabbitMqStatusProviderTest
    {
        [Test]
        public void ConnectionStringIsRequired()
        {
            var configuration = new StatusProviderConfigurationElement { Name = "Rabbit MQ Status" };

            var exception = Assert.Throws<ArgumentException>(
                () => { new RabbitMqStatusProvider(configuration); }
            );
            
            Assert.IsNotNull(exception);
            StringAssert.StartsWith("RabbitMQ connection string is required.", exception.Message);
        }
        
        [Test]
        public void CheckStatusTest()
        {
            var configuration = new StatusProviderConfigurationElement
            {
                Name = "Rabbit MQ Status",
                ConnectionString = "amqp://test:test@test.test:15672/"
            };
            
            var statusProvider = new RabbitMqStatusProvider(configuration);

            var externalUnit = statusProvider.CheckStatus();
            
            Assert.IsNotNull(externalUnit);
            Assert.AreEqual(externalUnit.Uri, "amqp://test.test:15672/");
            Assert.AreEqual(externalUnit.Name, "Rabbit MQ Status");
            Assert.IsTrue(externalUnit.ServerPlatformSpecified);
            Assert.IsNotNull(externalUnit.ServerPlatform);
            Assert.AreEqual(externalUnit.ServerPlatform.Name, "RabbitMQ");
            Assert.IsNull(externalUnit.ServerPlatform.Version);
            Assert.AreEqual(externalUnit.Status, UnitStatus.NotOk);
            Assert.IsTrue(externalUnit.StatusMessageSpecified);
            Assert.AreEqual(externalUnit.StatusMessage, "Error: NameResolutionFailure");
        }
    }
}