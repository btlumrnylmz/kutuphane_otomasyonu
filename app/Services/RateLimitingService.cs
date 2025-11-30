using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace KutuphaneOtomasyonu.Services
{
    /// <summary>
    /// Basit rate limiting servisi (Login endpoint için brute force koruması).
    /// </summary>
    public class RateLimitingService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, List<DateTime>> _loginAttempts = new();

        public RateLimitingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Login denemesi yapılıp yapılamayacağını kontrol eder.
        /// </summary>
        /// <param name="key">Rate limiting key (IP veya username)</param>
        /// <param name="maxAttempts">Maksimum deneme sayısı</param>
        /// <param name="windowMinutes">Zaman penceresi (dakika)</param>
        /// <returns>Eğer deneme yapılabilirse true, aksi halde false</returns>
        public bool IsAllowed(string key, int maxAttempts = 5, int windowMinutes = 5)
        {
            var cacheKey = $"RateLimit:Login:{key}";
            
            if (!_cache.TryGetValue(cacheKey, out List<DateTime>? attempts))
            {
                attempts = new List<DateTime>();
            }
            else
            {
                // Eski denemeleri temizle (window dışında kalanlar)
                var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);
                attempts = attempts.Where(a => a > cutoff).ToList();
            }

            // Maksimum deneme sayısını aştı mı?
            if (attempts.Count >= maxAttempts)
            {
                return false;
            }

            // Yeni denemeyi kaydet
            attempts.Add(DateTime.UtcNow);
            _cache.Set(cacheKey, attempts, TimeSpan.FromMinutes(windowMinutes));
            
            return true;
        }

        /// <summary>
        /// Başarılı login sonrası deneme kayıtlarını temizler.
        /// </summary>
        public void ClearAttempts(string key)
        {
            var cacheKey = $"RateLimit:Login:{key}";
            _cache.Remove(cacheKey);
            _loginAttempts.TryRemove(key, out _);
        }

        /// <summary>
        /// Kalan deneme hakkını döndürür.
        /// </summary>
        public int GetRemainingAttempts(string key, int maxAttempts = 5, int windowMinutes = 5)
        {
            var cacheKey = $"RateLimit:Login:{key}";
            
            if (!_cache.TryGetValue(cacheKey, out List<DateTime>? attempts))
            {
                return maxAttempts;
            }

            var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);
            attempts = attempts.Where(a => a > cutoff).ToList();
            
            return Math.Max(0, maxAttempts - attempts.Count);
        }
    }
}

