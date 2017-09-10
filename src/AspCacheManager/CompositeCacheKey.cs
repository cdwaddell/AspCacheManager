using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Titanosoft.AspCacheManager
{
    public class CompositeCacheKey : IEquatable<CompositeCacheKey>
    {
        public readonly string BaseCacheKey;
        public readonly ImmutableDictionary<string, string> SubKeys;

        public CompositeCacheKey(string baseKey, params KeyValuePair<string, string>[] subKeys)
        {
            BaseCacheKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));
            SubKeys = subKeys.ToImmutableDictionary(x => x.Key, x => x.Value);
        }

        public bool Equals(CompositeCacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return !other.SubKeys.Any(kvp => !SubKeys.ContainsKey(kvp.Key) || kvp.Value != SubKeys[kvp.Key]) 
                   && SubKeys.All(kvp => other.SubKeys.ContainsKey(kvp.Key));
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() 
                   && Equals((CompositeCacheKey) obj);
        }

        public override int GetHashCode()
        {
            var hashValue = BaseCacheKey != null? BaseCacheKey.GetHashCode():0;
            unchecked
            {
                foreach (var key in SubKeys.OrderBy(x => x.Key))
                {
                    hashValue = (hashValue * 397) ^ (key.Key != null ? key.Key.GetHashCode() : 0);
                    hashValue = (hashValue * 397) ^ (key.Value != null ? key.Value.GetHashCode() : 0);
                }
            }
            return hashValue;
        }

        public override string ToString()
        {
            return SubKeys.Any() ? $"{BaseCacheKey}_{string.Join(",",SubKeys.Select(x => $"{x.Key}={x.Value}"))}" : BaseCacheKey;
        }
    }
}