using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;
using System.Threading.Tasks;

namespace Rik.StatusPage.Providers
{
    public class FileStorageStatusProvider : StatusProvider<FileStorageStatusProviderOptions>
    {
        public override string DisplayUri => options.StoragePath;

        public FileStorageStatusProvider(FileStorageStatusProviderOptions options)
            : base(options)
        {
            if (string.IsNullOrWhiteSpace(options.StoragePath))
                throw new ArgumentException("File storage path is required.", nameof(options.StoragePath));
        }

        protected override Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit)
        {
            var directory = new DirectoryInfo(options.StoragePath);
            if (!directory.Exists)
                return Task.FromResult(externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't exist or is not accessible."));

            var accessControl = directory.GetAccessControl();
            var accessRules = accessControl?.GetAccessRules(true, true, typeof(SecurityIdentifier));
            var applicationIdentity = WindowsIdentity.GetCurrent();

            if (options.RequireRead && !HasRights(accessRules, applicationIdentity, FileSystemRights.Read))
                return Task.FromResult(externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't have reading rights."));

            if (options.RequireWrite && !HasRights(accessRules, applicationIdentity, FileSystemRights.Write))
                return Task.FromResult(externalUnit.SetStatus(UnitStatus.NotOk, "File storage path doesn't have writing rights."));

            return Task.FromResult(externalUnit.SetStatus(UnitStatus.Ok));
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