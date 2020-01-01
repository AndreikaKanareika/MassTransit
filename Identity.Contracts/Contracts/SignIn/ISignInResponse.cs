using Common.Contracts;
using System;

namespace Identity.Contracts
{
    public interface ISignInResponse : IResponse
    {
        public string Token { get; set; }
    }
}
