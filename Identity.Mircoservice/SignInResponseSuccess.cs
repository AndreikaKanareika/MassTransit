using Identity.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Mircoservice
{
    public class SignInResponseSuccess : ISignInResponse
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
