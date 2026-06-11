using System.Net;

namespace Amazon.Core.Exceptions
{
    public class BussinesException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BussinesException() : base()
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        public BussinesException(string message) 
            : base(message)
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        // Constructor nuevo — permite especificar el código HTTP
        public BussinesException(string message, HttpStatusCode statusCode) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}