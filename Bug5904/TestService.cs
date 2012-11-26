//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// This source code was auto-generated by wsdl, Version=4.0.30319.17929.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="TestServiceSoap", Namespace="http://provcon-faust/TestWCF/TestService.asmx/")]
public partial class TestService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    private System.Threading.SendOrPostCallback HelloWorldOperationCompleted;
    
    private System.Threading.SendOrPostCallback TestPostOperationCompleted;
    
    /// <remarks/>
    public TestService() {
        this.Url = "http://provcon-faust/TestWCF/TestService.asmx";
    }
    
    /// <remarks/>
    public event HelloWorldCompletedEventHandler HelloWorldCompleted;
    
    /// <remarks/>
    public event TestPostCompletedEventHandler TestPostCompleted;
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://provcon-faust/TestWCF/TestService.asmx/HelloWorld", RequestNamespace="http://provcon-faust/TestWCF/TestService.asmx/", ResponseNamespace="http://provcon-faust/TestWCF/TestService.asmx/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string HelloWorld() {
        object[] results = this.Invoke("HelloWorld", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginHelloWorld(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("HelloWorld", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndHelloWorld(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public void HelloWorldAsync() {
        this.HelloWorldAsync(null);
    }
    
    /// <remarks/>
    public void HelloWorldAsync(object userState) {
        if ((this.HelloWorldOperationCompleted == null)) {
            this.HelloWorldOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHelloWorldOperationCompleted);
        }
        this.InvokeAsync("HelloWorld", new object[0], this.HelloWorldOperationCompleted, userState);
    }
    
    private void OnHelloWorldOperationCompleted(object arg) {
        if ((this.HelloWorldCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.HelloWorldCompleted(this, new HelloWorldCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://provcon-faust/TestWCF/TestService.asmx/TestPost", RequestNamespace="http://provcon-faust/TestWCF/TestService.asmx/", ResponseNamespace="http://provcon-faust/TestWCF/TestService.asmx/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string TestPost(string body) {
        object[] results = this.Invoke("TestPost", new object[] {
                    body});
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginTestPost(string body, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("TestPost", new object[] {
                    body}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndTestPost(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public void TestPostAsync(string body) {
        this.TestPostAsync(body, null);
    }
    
    /// <remarks/>
    public void TestPostAsync(string body, object userState) {
        if ((this.TestPostOperationCompleted == null)) {
            this.TestPostOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTestPostOperationCompleted);
        }
        this.InvokeAsync("TestPost", new object[] {
                    body}, this.TestPostOperationCompleted, userState);
    }
    
    private void OnTestPostOperationCompleted(object arg) {
        if ((this.TestPostCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.TestPostCompleted(this, new TestPostCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    public new void CancelAsync(object userState) {
        base.CancelAsync(userState);
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
public delegate void HelloWorldCompletedEventHandler(object sender, HelloWorldCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class HelloWorldCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal HelloWorldCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
public delegate void TestPostCompletedEventHandler(object sender, TestPostCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class TestPostCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal TestPostCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
}
