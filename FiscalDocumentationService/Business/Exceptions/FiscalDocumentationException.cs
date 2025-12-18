namespace FiscalDocumentationService.Business.Exceptions
{
    public abstract class FiscalDocumentationException : Exception
    {
        protected FiscalDocumentationException(string message) : base(message)
        {
        }

        public sealed class FiscalValidationException : FiscalDocumentationException
        {
            public FiscalValidationException(string message) : base(message)
            {
            }
        }

        public sealed class FiscalConfigurationException : FiscalDocumentationException
        {
            public FiscalConfigurationException(string message) : base(message)
            {
            }
        }

        public sealed class ArcaDependencyException : FiscalDocumentationException
        {
            public int? StatusCode { get; }
            public string? ProviderPayload { get; }

            public ArcaDependencyException(string message, int? statusCode = null, string? providerPayload = null)
                : base(message)
            {
                StatusCode = statusCode;
                ProviderPayload = providerPayload;
            }

        }
    }
}
