using Common.Contracts;
using MassTransit;
using MassTransitRPC.Exceptions;
using System;
using System.Threading.Tasks;

namespace MassTransitRPC.Extension
{
    public static class IRequestClientExtensions
    {
        public async static Task<T> GetMessage<T>(this Task<ValueTuple<Task<Response<T>>, Task<Response<IFailedResponse>>>> response) where T: class
        {
            var (succeedResponseTask, failedResponseTask)  = await response;

            if (failedResponseTask.IsCompletedSuccessfully)
            {
                var failedResponse = await failedResponseTask;
                throw new FailedResponseException(failedResponse.Message);
            }

            var result = await succeedResponseTask;
            return result.Message;
        }
    }
}
