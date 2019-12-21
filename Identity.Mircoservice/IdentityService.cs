using Common.Contracts;
using Identity.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Mircoservice
{
    public class IdentityService : BackgroundService
    {
        private readonly IBusControl _bus;

        public IdentityService()
        {
            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://localhost/"), h => { });

                cfg.ReceiveEndpoint("signIn", e =>
                {
                    e.Handler<ISignInRequest>(async context => {

                        //throw new Exception("qqq");
                        if (context.Message.Password == null)
                        {
                            await context.RespondAsync<IFailedResponse>(new
                            {
                                ErrorCode = ErrorCode.NotFound,
                                ErrorMessage = "error"
                            });
                        }
                        else
                        await context.RespondAsync<ISignInResponse>(new
                        {
                            Token = "123",
                            ExpirationDate = DateTime.Today.AddDays(1)
                        });
                    });
                });
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _bus.StartAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(base.StopAsync(cancellationToken), _bus.StopAsync());
        }
    }
}
