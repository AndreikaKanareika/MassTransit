using Identity.Contracts;
using Identity.Microservice;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Contracts
{
    public interface IPersonDB
    {
        public Person GetPerson(ConsumeContext<ISignInRequest> context);
    }
}
