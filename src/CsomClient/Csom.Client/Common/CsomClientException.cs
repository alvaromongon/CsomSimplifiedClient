using System;
using System.Runtime.Serialization;

namespace Csom.Client.Common
{
    public class CsomClientException : Exception
    {
        public CsomClientException()
        {
        }

        public CsomClientException(string message) : base(message)
        {
        }

        public CsomClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CsomClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
