using System.Threading;
using System.Threading.Tasks;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public interface IStatusProvider
    {
        string DisplayUri { get; }
        string Name { get; }

        Task<ExternalUnit> CheckStatusAsync(CancellationToken cancellationToken);
    }
}