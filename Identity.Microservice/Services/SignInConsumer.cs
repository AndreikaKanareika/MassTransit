using Common.Contracts;
using Identity.Contracts;
using Identity.Microservice.FakeRepository;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Microservice
{
    public class SignInConsumer : IConsumer<ISignInRequest>
    {
        private readonly IPersonDB _reposiroty;

        public SignInConsumer(IPersonDB repo)
        {
            _reposiroty = repo;
        }

        public async Task Consume(ConsumeContext<ISignInRequest> context)
        {
            var identity = GetIdentity(context);
            if (identity == null)
            {
                await context.RespondAsync<IFailedResponse>(new
                {
                    ErrorCode = ErrorCode.Undefined,
                    ErrorMessage = "Invalid login or password"
                });
            }
            else
            {
                var now = DateTime.Now;

                var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        notBefore: now,
                        claims: identity.Claims,
                        expires: now.Add(TimeSpan.FromSeconds(AuthOptions.LIFETIME)),
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                        SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);


                await context.RespondAsync<ISignInResponse>(new SignInResponseSuccess { Token = encodedJwt });
            }             
        }

        private ClaimsIdentity GetIdentity(ConsumeContext<ISignInRequest> context)
        {
            Person person = _reposiroty.GetPerson(context);

            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType,person.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType,person.Role)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", 
                    ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                
                return claimsIdentity;
            
            }
            
            return null;
        }

    }
}
