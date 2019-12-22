using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Contracts;
using Identity.Contracts;
using Identity.Contracts.Implementation.Contracts.SignIn;
using MassTransit;
using MassTransitRPC.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MassTransitRPC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(cfg =>
            {
                cfg.Filters.Add<FailedResponseGlobalExceptionFilter>();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Test API"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            ConfigureBus(services);
        }

     

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API V1");
                c.RoutePrefix = string.Empty;
            });
        }



        private void ConfigureBus(IServiceCollection services)
        {
            var rabbitMQTimeoutInSeconds = Configuration.GetValue<int>("RabbitMQ:TimeoutInSeconds");

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(Configuration["RabbitMQ:Uri:Host"]);
            });

            services.AddSingleton<IPublishEndpoint>(bus);
            services.AddSingleton<ISendEndpointProvider>(bus);
            services.AddSingleton<IBus>(bus);

            ConfigureRequest(services, bus, rabbitMQTimeoutInSeconds);
            //services.AddScoped(x => bus.CreateRequestClient<ISignInRequest>(new Uri("rabbitmq://localhost/signIn")));
            // ConfigureRequestResponseContracts(services, rabbitMQTimeoutInSeconds);

            var x = services.BuildServiceProvider().GetService<IRequestClient<SignInRequest>>();

            bus.Start();
        }


        private void ConfigureRequest(IServiceCollection services, IBus bus, int rabbitMQTimeoutInSeconds)
        {
            var contractAttributeType = typeof(RequestContractAttribute);
            var factory = bus.CreateClientFactory(RequestTimeout.After(s: rabbitMQTimeoutInSeconds));
            var methodInfo = factory.GetType().GetMethod("CreateRequestClient", new Type[] { typeof(Uri), typeof(RequestTimeout) });

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var contractTypes = assembly.GetTypes().Where(type => type.IsInterface && Attribute.IsDefined(type, contractAttributeType)); ;

                foreach (var contractType in contractTypes)
                {
                    var attribute = contractType.GetCustomAttributes(contractAttributeType, true).First() as RequestContractAttribute;
                    var genericMethod = methodInfo.MakeGenericMethod(contractType);
                    var parameters = new object[] {
                        new Uri(Configuration[$"RabbitMQ:Uri:{attribute.Contract}"]),
                        RequestTimeout.After(s: rabbitMQTimeoutInSeconds)
                    };

                    services.AddScoped(
                        typeof(IRequestClient<>).MakeGenericType(contractType),
                        x => methodInfo.MakeGenericMethod(contractType).Invoke(factory, parameters)
                    );
                }
            }
        }


        //#region Request-Response Contracts Configuration

        //private class RequestResponseMatching
        //{
        //    public Type RequestType { get; set; }
        //    public Type ResponseType { get; set; }
        //}

        //private void ConfigureRequestResponseContracts(IServiceCollection services, int rabbitMQTimeoutInSeconds)
        //{
        //    var requestResponseMatchings = new Dictionary<string, RequestResponseMatching>();
        //    var contractAttributeType = typeof(RequestResponseContractAttribute);


        //    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        var contractTypes = assembly.GetTypes().Where(type => type.GetInterface(typeof(IMessage).FullName) != null && Attribute.IsDefined(type, contractAttributeType)); ;

        //        foreach (var contractType in contractTypes)
        //        {
        //            var attribute = contractType.GetCustomAttributes(contractAttributeType, true).First() as RequestResponseContractAttribute;
        //            var contractName = attribute.Contract;

        //            if (contractType.GetInterface(typeof(IRequest).FullName) != null)
        //            {
        //                AddRequest(requestResponseMatchings, contractName, contractType);
        //            }
        //            else if (contractType.GetInterface(typeof(IResponse).FullName) != null)
        //            {
        //                AddResponse(requestResponseMatchings, contractName, contractType);
        //            }
        //            else
        //            {
        //                throw new Exception($"Invalid contract : {contractType}");
        //            }
        //        }
        //    }

        //    foreach (var rrm in requestResponseMatchings)
        //    {
        //        var dependencies = new[] { rrm.Value.RequestType as Type, rrm.Value.ResponseType as Type };
        //        var requestClientType = typeof(IRequestClient<,>).MakeGenericType(dependencies);

        //        services.AddScoped(requestClientType, x =>
        //        {
        //            var messageRequestClientType = typeof(MessageRequestClient<,>).MakeGenericType(dependencies);
        //            var constructor = messageRequestClientType
        //                .GetConstructor(new Type[]
        //                {
        //                    typeof(IBus),
        //                    typeof(Uri),
        //                    typeof(TimeSpan),
        //                    typeof(TimeSpan?),
        //                    typeof(Action<>).MakeGenericType(
        //                        typeof(SendContext<>).MakeGenericType(rrm.Value.RequestType)
        //                    )
        //                });

        //            return constructor.Invoke(new object[] {
        //                x.GetRequiredService<IBus>(),
        //                new Uri(Configuration[$"RabbitMQ:Uri:{rrm.Key}"]),
        //                TimeSpan.FromSeconds(rabbitMQTimeoutInSeconds),
        //                null,
        //                null
        //            });
        //        });
        //    }
        //}

        //private void AddRequest(Dictionary<string, RequestResponseMatching> requestResponseMatchings, string contractName, Type request)
        //{
        //    if (requestResponseMatchings.TryGetValue(contractName, out var dep))
        //    {
        //        dep.RequestType = dep.RequestType == null
        //            ? request
        //            : throw new Exception($"Duplicated request contract : { contractName }");
        //    }
        //    else
        //    {
        //        requestResponseMatchings.Add(contractName, new RequestResponseMatching { RequestType = request });
        //    }
        //}

        //private void AddResponse(Dictionary<string, RequestResponseMatching> requestResponseMatchings, string contractName, Type response)
        //{
        //    if (requestResponseMatchings.TryGetValue(contractName, out var dep))
        //    {
        //        dep.ResponseType = dep.ResponseType == null
        //            ? response
        //            : throw new Exception($"Duplicated response contract : { contractName }");
        //    }
        //    else
        //    {
        //        requestResponseMatchings.Add(contractName, new RequestResponseMatching { ResponseType = response });
        //    }
        //}

        //#endregion

    }
}
