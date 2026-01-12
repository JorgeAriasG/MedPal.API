namespace MedPal.API.Enums
{
    /// <summary>
    /// Métodos de pago disponibles
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>Pago en efectivo</summary>
        Cash = 0,

        /// <summary>Pago con tarjeta de crédito</summary>
        CreditCard = 1,

        /// <summary>Pago con tarjeta de débito</summary>
        DebitCard = 2,

        /// <summary>Transferencia bancaria</summary>
        BankTransfer = 3,

        /// <summary>Cobertura de seguro/aseguradora</summary>
        Insurance = 4,

        /// <summary>Cheque</summary>
        Check = 5,

        /// <summary>Billetera digital/e-wallet</summary>
        DigitalWallet = 6
    }
}
