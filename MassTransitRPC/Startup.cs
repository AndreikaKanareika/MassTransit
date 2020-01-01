using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Contracts;
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

            bus.Start();
        }

        private void ConfigureRequest(IServiceCollection services, IBus bus, int rabbitMQTimeoutInSeconds)
        {
            var contractAttributeType = typeof(RequestContractAttribute);
            var factory = bus.CreateClientFactory(RequestTimeout.After(s: rabbitMQTimeoutInSeconds));
            var methodInfo = factory.GetType().GetMethod("CreateRequestClient", new Type[] { typeof(Uri), typeof(RequestTimeout) });

            foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                var contractTypes = assembly.GetTypes().Where(type => type.IsInterface && Attribute.IsDefined(type, contractAttributeType));

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
    }
}
