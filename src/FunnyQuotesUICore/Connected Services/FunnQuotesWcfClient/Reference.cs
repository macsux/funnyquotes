﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FunnQuotesWcfClient
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="FunnQuotesWcfClient.IFunnyQuoteService")]
    public interface IFunnyQuoteService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFunnyQuoteService/GetQuote", ReplyAction="http://tempuri.org/IFunnyQuoteService/GetQuoteResponse")]
        System.Threading.Tasks.Task<string> GetQuoteAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IFunnyQuoteService/GetQuote2", ReplyAction="http://tempuri.org/IFunnyQuoteService/GetQuote2Response")]
        System.Threading.Tasks.Task<string> GetQuote2Async();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    public interface IFunnyQuoteServiceChannel : FunnQuotesWcfClient.IFunnyQuoteService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    public partial class FunnyQuoteServiceClient : System.ServiceModel.ClientBase<FunnQuotesWcfClient.IFunnyQuoteService>, FunnQuotesWcfClient.IFunnyQuoteService
    {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public FunnyQuoteServiceClient() : 
                base(FunnyQuoteServiceClient.GetDefaultBinding(), FunnyQuoteServiceClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public FunnyQuoteServiceClient(EndpointConfiguration endpointConfiguration) : 
                base(FunnyQuoteServiceClient.GetBindingForEndpoint(endpointConfiguration), FunnyQuoteServiceClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public FunnyQuoteServiceClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(FunnyQuoteServiceClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public FunnyQuoteServiceClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(FunnyQuoteServiceClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public FunnyQuoteServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<string> GetQuoteAsync()
        {
            return base.Channel.GetQuoteAsync();
        }
        
        public System.Threading.Tasks.Task<string> GetQuote2Async()
        {
            return base.Channel.GetQuote2Async();
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual new System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService))
            {
                return new System.ServiceModel.EndpointAddress("http://localhost:55483/FunnyQuoteserviceWCF.svc");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return FunnyQuoteServiceClient.GetBindingForEndpoint(EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return FunnyQuoteServiceClient.GetEndpointAddress(EndpointConfiguration.BasicHttpBinding_IFunnyQuoteService);
        }
        
        public enum EndpointConfiguration
        {
            
            BasicHttpBinding_IFunnyQuoteService,
        }
    }
}
