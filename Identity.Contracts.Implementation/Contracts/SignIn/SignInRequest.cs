namespace Identity.Contracts.Implementation.Contracts.SignIn
{
    public class SignInRequest : ISignInRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
