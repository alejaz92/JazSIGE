namespace SalesService.Business.Models.Sale.fiscalDocs
{
    public enum CreditNoteReason
    {
        FullCancellation,      // anulación total
        PartialReturn,         // devolución de mercadería (lleva ítems)
        PriceDownCorrection,   // corrección de precio a la baja
        PostSaleBonus,         // bonificación posterior
        WrongChargeRemoval     // quitar cargos/impuestos/percepciones mal aplicados
    }
    public enum DebitNoteReason
    {
        LatePaymentInterest,       // intereses/recargo por mora
        PriceUpCorrection,         // corrección de precio al alza
        AdditionalServiceCharge,   // flete/servicio omitido
        ExchangeRateAdjustment     // diferencia pactada de TC/actualización
    }
}
