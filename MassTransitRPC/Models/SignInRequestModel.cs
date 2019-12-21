using Identity.Contracts;

namespace MassTransitRPC.Models
{
    public class SignInRequestModel : ISignInRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
