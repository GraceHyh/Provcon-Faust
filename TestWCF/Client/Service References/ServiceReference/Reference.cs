﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestWCF.Client.ServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IMyService")]
    public interface IMyService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMyService/Hello", ReplyAction="http://tempuri.org/IMyService/HelloResponse")]
        string Hello();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMyService/TestException", ReplyAction="http://tempuri.org/IMyService/TestExceptionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestWCF.Model.ArgumentFault), Action="http://tempuri.org/IMyService/TestExceptionArgumentFaultFault", Name="ArgumentFault", Namespace="http://schemas.datacontract.org/2004/07/TestWCF.Model")]
        void TestException();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMyServiceChannel : TestWCF.Client.ServiceReference.IMyService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MyServiceClient : System.ServiceModel.ClientBase<TestWCF.Client.ServiceReference.IMyService>, TestWCF.Client.ServiceReference.IMyService {
        
        public MyServiceClient() {
        }
        
        public MyServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MyServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MyServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MyServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string Hello() {
            return base.Channel.Hello();
        }
        
        public void TestException() {
            base.Channel.TestException();
        }
    }
}
