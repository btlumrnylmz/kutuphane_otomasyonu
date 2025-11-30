using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Pagination için view model.
    /// </summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public string? ActionName { get; set; }
        public string? ControllerName { get; set; }
        public Dictionary<string, string>? QueryParameters { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartIndex => (CurrentPage - 1) * PageSize + 1;
        public int EndIndex => Math.Min(CurrentPage * PageSize, TotalItems);

        /// <summary>
        /// Belirtilen sayfa için URL oluşturur.
        /// </summary>
        public string GetPageUrl(int page)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "pageSize", PageSize.ToString() }
            };

            // Mevcut query parametrelerini ekle
            if (QueryParameters != null)
            {
                foreach (var param in QueryParameters)
                {
                    if (param.Key != "page" && param.Key != "pageSize" && !string.IsNullOrEmpty(param.Value))
                    {
                        queryParams[param.Key] = param.Value;
                    }
                }
            }

            var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{ControllerName ?? "Home"}/{ActionName ?? "Index"}?{queryString}";
        }

        /// <summary>
        /// Mevcut HttpContext'ten query parametrelerini alır.
        /// </summary>
        public static Dictionary<string, string> GetQueryParameters(HttpRequest request)
        {
            var parameters = new Dictionary<string, string>();
            
            foreach (var param in request.Query)
            {
                if (!string.IsNullOrEmpty(param.Value.ToString()))
                {
                    parameters[param.Key] = param.Value.ToString();
                }
            }

            return parameters;
        }
    }
}
