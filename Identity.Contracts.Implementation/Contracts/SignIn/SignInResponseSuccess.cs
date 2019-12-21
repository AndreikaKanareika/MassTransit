using System;

namespace Identity.Contracts.Implementation.Contracts.SignIn
{
    public class SignInResponseSuccess : ISignInResponse
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
