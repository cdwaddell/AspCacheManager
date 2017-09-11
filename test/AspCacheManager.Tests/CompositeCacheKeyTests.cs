using System.Collections.Generic;
using System.Collections.Immutable;
using Titanosoft.AspCacheManager;
using Xunit;

namespace AspCacheManager.Tests
{
    public class CompositeCacheKeyTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void TestEquality()
        {
            var firstKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));
            var secondKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));

            Assert.Equal(firstKey, secondKey);
            Assert.Equal(firstKey.GetHashCode(), secondKey.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestUnEqualityByBaseKey()
        {
            var firstKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));
            var secondKey = new CompositeCacheKey("Test2", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));

            Assert.NotEqual(firstKey, secondKey);
            Assert.NotEqual(firstKey.GetHashCode(), secondKey.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestUnEqualityByKvpValue()
        {
            var firstKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));
            var secondKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value2"), new KeyValuePair<string, string>("key2", "value2"));

            Assert.NotEqual(firstKey, secondKey);
            Assert.NotEqual(firstKey.GetHashCode(), secondKey.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestUnEqualityByKvpKey()
        {
            var firstKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));
            var secondKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key23", "value1"), new KeyValuePair<string, string>("key2", "value2"));

            Assert.NotEqual(firstKey, secondKey);
            Assert.NotEqual(firstKey.GetHashCode(), secondKey.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void FunctionalDictionary()
        {
            var firstKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key1", "value1"), new KeyValuePair<string, string>("key2", "value2"));
            var secondKey = new CompositeCacheKey("Test1", new KeyValuePair<string, string>("key23", "value1"), new KeyValuePair<string, string>("key2", "value2"));

            var dictionary = new[] {firstKey, secondKey}.ToImmutableDictionary(x => x, x => x.BaseCacheKey);

            Assert.True(dictionary.ContainsKey(firstKey));
            Assert.True(dictionary.ContainsKey(secondKey));

            var firstValue = dictionary[firstKey];
            var secondValue = dictionary[secondKey];

            Assert.Equal(firstValue, firstKey.BaseCacheKey);
            Assert.Equal(secondValue, secondKey.BaseCacheKey);
        }
    }
}
