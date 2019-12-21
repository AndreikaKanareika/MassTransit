using System;

namespace Common.Contracts
{
    public class RequestContractAttribute : Attribute
    {
        public string Contract { get; }

        public RequestContractAttribute(string contract)
        {
            Contract = contract;
        }
    }
}
