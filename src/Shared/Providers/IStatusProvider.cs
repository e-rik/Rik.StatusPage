using System.Threading.Tasks;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public interface IStatusProvider
    {
        Task<ExternalUnit> CheckStatusAsync();
    }
}