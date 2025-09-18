namespace AccountingService.Business.Models.Ledger
{
    /// Resumen de saldos del cliente en ARS.
    public class CustomerBalancesDTO
    {
        public decimal OutstandingArs { get; set; } // Deuda abierta (FAC + ND) aún no cobrada
        public decimal CreditsArs { get; set; }     // Créditos a favor (NC + anticipos)
        public decimal NetBalanceArs { get; set; }  // Outstanding - Credits (si >0 debe; si <0 debemos)
    }
}
