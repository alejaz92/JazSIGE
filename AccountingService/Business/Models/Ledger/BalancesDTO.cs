namespace AccountingService.Business.Models.Ledger
{
    /// Resumen de saldos del cliente en ARS.
    public class BalancesDTO
    {
        /// Open debt from debits (Invoices + Debit Notes) not yet collected.
        public decimal OutstandingArs { get; set; }

        /// Credits at customer side (Credit Notes + unapplied Receipts).
        public decimal CreditsArs { get; set; }

        /// Net = Outstanding - Credits  ( >0 customer owes; <0 we owe the customer )
        public decimal NetBalanceArs { get; set; }
    }
}
