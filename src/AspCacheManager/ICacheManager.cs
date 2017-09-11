using System.Threading;
using System.Threading.Tasks;

namespace Titanosoft.AspCacheManager
{
    public interface ICacheManager
    {
        Task CheckRefreshAsync(CancellationToken token);
    }
}