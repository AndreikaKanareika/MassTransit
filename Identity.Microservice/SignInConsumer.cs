using Common.Contracts;
using Identity.Contracts;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Identity.Microservice
{
    public class SignInConsumer : IConsumer<ISignInRequest>
    {
        public async Task Consume(ConsumeContext<ISignInRequest> context)
        {
            // TODO: implement identity
            if (context.Message.Password == null)
            {
                await context.RespondAsync<IFailedResponse>(new
                {
                    ErrorCode = ErrorCode.NotFound,
                    ErrorMessage = "error"
                });
            }
            else
            {
                await context.RespondAsync<ISignInResponse>(new
                {
                    Token = "123",
                    ExpirationDate = DateTime.Today.AddDays(1)
                });
            }             
        }
    }
}
