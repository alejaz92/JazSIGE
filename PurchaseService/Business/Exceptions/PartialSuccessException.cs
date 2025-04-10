namespace PurchaseService.Business.Exceptions
{
    public class PartialSuccessException : Exception
    {
        public PartialSuccessException(string message) : base(message) { }
    }
}
