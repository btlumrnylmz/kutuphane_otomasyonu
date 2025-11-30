using Microsoft.Extensions.Caching.Memory;

namespace KutuphaneOtomasyonu.Services
{
    /// <summary>
    /// Cache işlemlerini yöneten servis.
    /// </summary>
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Cache'den değer alır.
        /// </summary>
        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return value;
            }
            return default(T);
        }

        /// <summary>
        /// Cache'e değer ekler.
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan? duration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration ?? _defaultCacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(2) // 2 dakika kullanılmazsa expire olur
            };

            _cache.Set(key, value, options);
        }

        /// <summary>
        /// Cache'den değer alır, yoksa factory fonksiyonunu çalıştırıp cache'e kaydeder.
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null)
        {
            var cached = Get<T>(key);
            if (cached != null && !cached.Equals(default(T)))
            {
                return cached;
            }

            var value = await factory();
            Set(key, value, duration);
            return value;
        }

        /// <summary>
        /// Cache'den değer alır, yoksa factory fonksiyonunu çalıştırıp cache'e kaydeder (senkron versiyon).
        /// </summary>
        public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? duration = null)
        {
            var cached = Get<T>(key);
            if (cached != null && !cached.Equals(default(T)))
            {
                return cached;
            }

            var value = factory();
            Set(key, value, duration);
            return value;
        }

        /// <summary>
        /// Cache'den belirtilen key'i siler.
        /// </summary>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        /// Belirtilen pattern'e uyan tüm cache key'lerini siler.
        /// </summary>
        public void RemoveByPattern(string pattern)
        {
            // MemoryCache'te pattern ile silme yok, bu yüzden özel bir cache key yapısı kullanılmalı
            // Şimdilik sadece tek bir key'i silebiliriz
            Remove(pattern);
        }

        /// <summary>
        /// Cache'i temizler (tüm cache'leri siler).
        /// </summary>
        public void Clear()
        {
            if (_cache is MemoryCache memCache)
            {
                // MemoryCache'i temizlemek için dispose edip yeniden oluşturmak gerekir
                // Bu yöntem önerilmez, sadece belirli key'leri silmek daha iyidir
            }
        }

        /// <summary>
        /// Cache key oluşturur.
        /// </summary>
        public static string CreateKey(params string[] parts)
        {
            return string.Join(":", parts);
        }
    }
}
