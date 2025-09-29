namespace AccountingService.Infrastructure.Models.Common
{
    /// Secuencia por "Scope" (ej.: "Receipt:0001").
    public class NumberingSequence
    {
        public int Id { get; set; }
        public string Scope { get; set; } = null!; // Unique
        public int NextNumber { get; set; } = 1;   // Próximo número a entregar
    }
}
