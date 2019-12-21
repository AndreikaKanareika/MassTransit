using Common.Contracts;

namespace Identity.Contracts
{
    [RequestContract("SignUp")]
    public interface ISignUpRequest : IRequest
    {
    }
}
