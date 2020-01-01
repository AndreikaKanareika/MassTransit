using Common.Contracts;
using Identity.Contracts;
using MassTransit;
using MassTransitRPC.Exceptions;
using MassTransitRPC.Extension;
using MassTransitRPC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MassTransitRPC.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IRequestClient<ISignInRequest> _signInRequestClient;

        public AccountController(IRequestClient<ISignInRequest> signInRequestClient)
        {
            _signInRequestClient = signInRequestClient;
        }

        /// <summary>
        /// comments
        /// </summary>
        /// <param name="signInRequest"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(SignInRequestModel signInRequest)
        {
            var response = await _signInRequestClient
                .GetResponse<ISignInResponse, IFailedResponse>(signInRequest)
                .GetMessage();

            return Ok(response);
        }
    }
}
