using System;
using Common.Contracts;

namespace MassTransitRPC.Exceptions
{
    public class FailedResponseException : Exception
    {
        public IFailedResponse FailedResponse { get; }

        public FailedResponseException(IFailedResponse failedResppnse)
        {
            FailedResponse = failedResppnse;
        }
    }
}
