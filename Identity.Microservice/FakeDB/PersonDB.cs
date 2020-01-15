using Common.Contracts;
using Identity.Contracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Microservice.FakeRepository
{
    public class PersonDB : IPersonDB
    {
        public static List<Person> people = new List<Person>
        {
            new Person {Login="admin@gmail.com", Password="12345", Role = "admin" },
            new Person { Login="qwerty@gmail.com", Password="55555", Role = "user" }
        };

        public Person GetPerson(ConsumeContext<ISignInRequest> context)
        {
            var chosenPerson = people.FirstOrDefault(x => x.Login == context.Message.Login && x.Password == context.Message.Password);
            return chosenPerson == null ? null : chosenPerson;
        }
    }
}
