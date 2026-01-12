namespace MedPal.API.Enums
{
    /// <summary>
    /// Estados de factura/invoice
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Factura creada, sin pagos</summary>
        Pending = 0,

        /// <summary>Factura con pagos parciales</summary>
        PartiallyPaid = 1,

        /// <summary>Factura completamente pagada</summary>
        Paid = 2,

        /// <summary>Factura vencida sin pago completo</summary>
        Overdue = 3,

        /// <summary>Factura cancelada (no aplica pago)</summary>
        Cancelled = 4
    }
}
