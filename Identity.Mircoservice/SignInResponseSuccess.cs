using Identity.Contracts;
using System;

namespace Identity.Mircoservice
{
    public class SignInResponseSuccess : ISignInResponse
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
