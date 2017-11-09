using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class FileStorageStatusProvider : StatusProvider
    {
        private readonly string storagePath;
        private readonly bool requireRead;
        private readonly bool requireWrite;

        public FileStorageStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.StoragePath))
                throw new ArgumentException("File storage path is required.", nameof(configuration.StoragePath));

            storagePath = configuration.StoragePath;
            requireRead = configuration.RequireRead;
            requireWrite = configuration.RequireWrite;
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            externalUnit.Uri = storagePath;

            if (!Directory.Exists(storagePath))
                return externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't exist or is not accessible.");

            var accessControl = Directory.GetAccessControl(storagePath);
            var accessRules = accessControl?.GetAccessRules(true, true, typeof(SecurityIdentifier));
            var applicationIdentity = WindowsIdentity.GetCurrent();

            if (requireRead && !HasRights(accessRules, applicationIdentity, FileSystemRights.Read))
                return externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't have reading rights.");

            if (requireWrite && !HasRights(accessRules, applicationIdentity, FileSystemRights.Write))
                return externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't have writing rights.");

            return externalUnit.SetStatus(UnitStatus.Ok);
        }

        private static bool HasRights(IEnumerable rules, WindowsIdentity identity, FileSystemRights rights)
        {
            if (rules == null)
                return false;

            var allowRights = false;
            var denyRights = false;

            foreach (var rule in rules.Cast<FileSystemAccessRule>().Where(r => r.FileSystemRights.HasFlag(rights) && IsSpecifiedToIdentity(r, identity)))
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Allow:
                        allowRights = true;
                        break;
                    case AccessControlType.Deny:
                        denyRights = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return allowRights && !denyRights;
        }

        private static bool IsSpecifiedToIdentity(AuthorizationRule rule, WindowsIdentity identity)
        {
            if (identity.User != null && identity.User == rule.IdentityReference)
                return true;

            return identity.Groups != null && identity.Groups.Any(g => g == rule.IdentityReference);
        }
    }
}