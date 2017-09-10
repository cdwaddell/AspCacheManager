using System;

namespace Titanosoft.AspCacheManager
{
    public class SystemDateTime: IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}