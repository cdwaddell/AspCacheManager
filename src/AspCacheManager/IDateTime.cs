using System;

namespace Titanosoft.AspCacheManager
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}