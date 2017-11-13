using System;
using System.Net;
using System.Reflection.Emit;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class RabbitMqStatusProvider : StatusProvider
    {
        private delegate bool IsAliveDelegate(Uri uri, string user, string password, out string version);

        private static readonly IsAliveDelegate checkStatus = CreateIsAliveDelegate();

        private readonly string connectionString;

        public RabbitMqStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
                throw new ArgumentException("RabbitMQ connection string is required.", nameof(configuration.ConnectionString));

            connectionString = configuration.ConnectionString;
        }

        protected override string GetUri()
        {
            return Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
                ? new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, uri.Fragment).ToString()
                : string.Empty;
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            externalUnit.ServerPlatform = new ServerPlatform { Name = "RabbitMQ" };

            var uri = new Uri(connectionString, UriKind.Absolute);

            var userInfo = uri.UserInfo.Split(new[] { ':' }, 2, StringSplitOptions.None);
            var user = userInfo.Length > 0 ? userInfo[0] : "";
            var password = userInfo.Length > 1 ? userInfo[1] : "";

            if (checkStatus(uri, user, password, out var version))
            {
                externalUnit.ServerPlatform.Version = version;
                externalUnit.SetStatus(UnitStatus.Ok);
            }
            else
                externalUnit.SetStatus(UnitStatus.NotOk, "RabbitMQ ei tööta.");

            return externalUnit;
        }

        private static IsAliveDelegate CreateIsAliveDelegate()
        {
            var clientType = FindType("EasyNetQ.Management.Client.ManagementClient, EasyNetQ.Management.Client");
            if (clientType == null)
                throw new Exception("Cannot find ManagementClient type (is EasyNetQ.Management.Client package not referenced?)");

            var clientConstructor = clientType.GetConstructor(new[]
            {
                typeof(string), typeof(string), typeof(string), typeof(int), typeof(bool), typeof(TimeSpan?),
                typeof(Action<HttpWebRequest>), typeof(bool)
            });

            var vhostType = FindType("EasyNetQ.Management.Client.Model.Vhost, EasyNetQ.Management.Client");
            var overviewType = FindType("EasyNetQ.Management.Client.Model.Overview, EasyNetQ.Management.Client");

            var vhostConstructor = vhostType.GetConstructor(new Type[0]);

            //var isMono = Type.GetType("Mono.Runtime") != null;

            var method = new DynamicMethod("RabbitMqStatusProvider_IsAlive", typeof(bool), new [] { typeof(Uri), typeof(string), typeof(string), typeof(string).MakeByRefType() });

            var il = method.GetILGenerator();

            // version = null;
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stind_Ref);

            // TimeSpan? timeSpan = null;
            var timeSpan = il.DeclareLocal(typeof(TimeSpan?));
            il.Emit(OpCodes.Ldloca, timeSpan);
            il.Emit(OpCodes.Initobj, timeSpan.LocalType);

            // int port = uri.Port;
            var port = il.DeclareLocal(typeof(int));
            var portLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, typeof(Uri).GetProperty("Port").GetGetMethod());
            il.Emit(OpCodes.Stloc, port);
            il.Emit(OpCodes.Ldloc, port);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Bge, portLabel);
            il.Emit(OpCodes.Ldc_I4, 15672);
            il.Emit(OpCodes.Stloc, port);
            il.MarkLabel(portLabel);
            il.Emit(OpCodes.Nop);

            // var management = new ManagementClient(uri.Host, user, password, uri.Port, isMono, timeSpan, null, false);
            var management = il.DeclareLocal(clientType);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, typeof(Uri).GetProperty("Host").GetGetMethod());
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldloc, port);
            //il.Emit(isMono ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldloc, timeSpan);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newobj, clientConstructor);
            il.Emit(OpCodes.Stloc, management);

            // var vhost = new Vhost();
            var vhost = il.DeclareLocal(vhostType);
            il.Emit(OpCodes.Newobj, vhostConstructor);
            il.Emit(OpCodes.Stloc, vhost);

            // vhost.Name = uri.AbsolutePath;
            il.Emit(OpCodes.Ldloc, vhost);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, typeof(Uri).GetProperty("AbsolutePath").GetGetMethod());
            il.Emit(OpCodes.Callvirt, vhostType.GetProperty("Name").GetSetMethod());

            var returnFalseLabel = il.DefineLabel();
            var returnLabel = il.DefineLabel();

            // if (management.IsAlive(vhost))
            il.Emit(OpCodes.Ldloc, management);
            il.Emit(OpCodes.Ldloc, vhost);
            il.Emit(OpCodes.Callvirt, clientType.GetMethod("IsAlive", new[] { vhostType }));
            il.Emit(OpCodes.Brfalse, returnFalseLabel);

            var getLengthsCriteriaType =
                FindType("EasyNetQ.Management.Client.Model.GetLengthsCriteria, EasyNetQ.Management.Client");
            var getRatesCriteriaType =
                FindType("EasyNetQ.Management.Client.Model.GetRatesCriteria, EasyNetQ.Management.Client");

            // version = management.GetOverview().RabbitmqVersion;
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldloc, management);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt, clientType.GetMethod("GetOverview", new[] { getLengthsCriteriaType, getRatesCriteriaType}));
            il.Emit(OpCodes.Callvirt, overviewType.GetProperty("RabbitmqVersion").GetGetMethod());
            il.Emit(OpCodes.Stind_Ref);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Br, returnLabel);

            // return
            il.MarkLabel(returnFalseLabel);
            il.Emit(OpCodes.Ldc_I4_0);

            il.MarkLabel(returnLabel);
            il.Emit(OpCodes.Ret);

            return (IsAliveDelegate)method.CreateDelegate(typeof(IsAliveDelegate));
        }
    }
}