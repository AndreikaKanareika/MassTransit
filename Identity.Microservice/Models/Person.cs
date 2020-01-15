using Identity.Contracts;

namespace Identity.Microservice
{
    public class Person
    {
        public string Login { get ; set ; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}