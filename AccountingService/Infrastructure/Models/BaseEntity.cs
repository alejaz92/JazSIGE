namespace AccountingService.Infrastructure.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Optimistic Concurrency
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
