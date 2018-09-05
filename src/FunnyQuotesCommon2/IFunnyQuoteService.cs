using System.ServiceModel;
using System.Threading.Tasks;

namespace FunnyQuotesCommon
{
    [ServiceContract]
    public interface IFunnyQuoteService
    {
        [OperationContract]
        string GetQuote();

        [OperationContract(Name = "GetQuote2")] //wcf doesn't like when methods end with "Async"
        Task<string> GetQuoteAsync();
    }
}