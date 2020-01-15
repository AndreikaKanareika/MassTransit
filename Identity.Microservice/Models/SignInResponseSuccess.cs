using Identity.Contracts;

namespace Identity.Microservice
{
    public class SignInResponseSuccess : ISignInResponse
    {
        public string Token { get; set; }
    }
}
