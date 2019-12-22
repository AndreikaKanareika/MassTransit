using Common.Contracts;

namespace Identity.Contracts
{
    [RequestContract("SignIn")]
    public interface ISignInRequest : IRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
