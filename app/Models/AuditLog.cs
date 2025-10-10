using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Audit log modeli. Loans tablosundaki INSERT/UPDATE işlemlerini kaydeder.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int AuditId { get; set; }

        public int LoanId { get; set; }

        [StringLength(20)]
        public string Action { get; set; } = string.Empty; // BORROW / RETURN

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;
    }
}



