namespace Application.Core
{
    public class ApplicationException
    {
        private readonly int _statusCode;
        private readonly string _message;
        private readonly string _stackTrace;
        public ApplicationException(int statusCode, string message, string stackTrace = null)
        {
            _stackTrace = stackTrace;
            _message = message;
            _statusCode = statusCode;
        }
    }
}