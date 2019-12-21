using System.Net;
using MassTransitRPC.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Common.Contracts;

namespace MassTransitRPC.Filters
{
    public class FailedResponseGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        public FailedResponseGlobalExceptionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is FailedResponseException ex)
            {
                var response = ex.FailedResponse;

                var statusCode = GetStatusCode(response);
                context.Result = GetContextResult(response);
                context.HttpContext.Response.StatusCode = (int)statusCode;
            }
            else
            {
                object obj;

                if (_env.IsDevelopment())
                {
                    obj = new
                    {
                        Message = "Error =(",
                        DeveloperMessage = context.Exception
                    };
                }
                else
                {
                    obj = new { Message = "Error =(" };
                }

                context.Result = new ObjectResult(obj);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }


        private static HttpStatusCode GetStatusCode(IFailedResponse failedResponse)
        {
            switch (failedResponse.ErrorCode)
            {
                case ErrorCode.NotFound:
                    return HttpStatusCode.NotFound;

                case ErrorCode.Forbidden:
                    return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.BadRequest;
        }

        private IActionResult GetContextResult(IFailedResponse response)
        {
            var result = new BadRequestObjectResult(response);

            switch (response.ErrorCode)
            {
                case ErrorCode.NotFound:
                    return new NotFoundObjectResult(response);

                case ErrorCode.Forbidden:
                    result.StatusCode = (int)HttpStatusCode.Forbidden;
                    return result;

                default: return result;
            }
        }
    }
}
