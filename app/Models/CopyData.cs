namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Raw SQL sorguları için geçici data class
    /// </summary>
    public class CopyData
    {
        public int CopyId { get; set; }
        public int BookId { get; set; }
        public string? ShelfLocation { get; set; }
        public string BookTitle { get; set; } = string.Empty;
    }
}

