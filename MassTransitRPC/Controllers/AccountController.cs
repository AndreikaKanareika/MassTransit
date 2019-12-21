﻿using Common.Contracts;
using Identity.Contracts;
using MassTransit;
using MassTransitRPC.Exceptions;
using MassTransitRPC.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var (succeedResponseTask, failedResponseTask) = await _signInRequestClient.GetResponse<ISignInResponse, IFailedResponse>(signInRequest);

            if (failedResponseTask.IsCompletedSuccessfully)
            {
                var failedResponse = await failedResponseTask;
                throw new FailedResponseException(failedResponse.Message);
            }

            var respone = await succeedResponseTask;
            return Ok(respone.Message);
        }
    }
}
