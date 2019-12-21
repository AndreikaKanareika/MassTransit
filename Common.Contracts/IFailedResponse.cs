namespace Common.Contracts
{
    public interface IFailedResponse 
    {
        public ErrorCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
