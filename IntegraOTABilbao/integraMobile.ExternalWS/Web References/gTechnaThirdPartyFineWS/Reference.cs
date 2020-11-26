﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.18051.
// 
#pragma warning disable 1591

namespace integraMobile.ExternalWS.gTechnaThirdPartyFineWS {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    // CODEGEN: The optional WSDL extension element 'PolicyReference' from namespace 'http://www.w3.org/ns/ws-policy' was not handled.
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="PayByPhoneOperationPortBinding", Namespace="http://ws.paybyphone.cc.gti.com/")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ticket[]))]
    public partial class PayByPhoneOperationService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback QueryTicketListOperationCompleted;
        
        private System.Threading.SendOrPostCallback QueryTicketStatusOperationCompleted;
        
        private System.Threading.SendOrPostCallback PayTicketOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public PayByPhoneOperationService() {
            this.Url = global::integraMobile.ExternalWS.Properties.Settings.Default.integraMobile_ExternalWS_gTechnaThirdPartyFineWS_PayByPhoneOperationService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event QueryTicketListCompletedEventHandler QueryTicketListCompleted;
        
        /// <remarks/>
        public event QueryTicketStatusCompletedEventHandler QueryTicketStatusCompleted;
        
        /// <remarks/>
        public event PayTicketCompletedEventHandler PayTicketCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.paybyphone.cc.gti.com/", ResponseNamespace="http://ws.paybyphone.cc.gti.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("ticket_list_response", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ticket_list_response QueryTicketList([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] ticket_list_request ticket_list_request) {
            object[] results = this.Invoke("QueryTicketList", new object[] {
                        ticket_list_request});
            return ((ticket_list_response)(results[0]));
        }
        
        /// <remarks/>
        public void QueryTicketListAsync(ticket_list_request ticket_list_request) {
            this.QueryTicketListAsync(ticket_list_request, null);
        }
        
        /// <remarks/>
        public void QueryTicketListAsync(ticket_list_request ticket_list_request, object userState) {
            if ((this.QueryTicketListOperationCompleted == null)) {
                this.QueryTicketListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnQueryTicketListOperationCompleted);
            }
            this.InvokeAsync("QueryTicketList", new object[] {
                        ticket_list_request}, this.QueryTicketListOperationCompleted, userState);
        }
        
        private void OnQueryTicketListOperationCompleted(object arg) {
            if ((this.QueryTicketListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.QueryTicketListCompleted(this, new QueryTicketListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.paybyphone.cc.gti.com/", ResponseNamespace="http://ws.paybyphone.cc.gti.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("ticket_status_response", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ticket_status_response QueryTicketStatus([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] ticket_status_request ticket_status_request) {
            object[] results = this.Invoke("QueryTicketStatus", new object[] {
                        ticket_status_request});
            return ((ticket_status_response)(results[0]));
        }
        
        /// <remarks/>
        public void QueryTicketStatusAsync(ticket_status_request ticket_status_request) {
            this.QueryTicketStatusAsync(ticket_status_request, null);
        }
        
        /// <remarks/>
        public void QueryTicketStatusAsync(ticket_status_request ticket_status_request, object userState) {
            if ((this.QueryTicketStatusOperationCompleted == null)) {
                this.QueryTicketStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnQueryTicketStatusOperationCompleted);
            }
            this.InvokeAsync("QueryTicketStatus", new object[] {
                        ticket_status_request}, this.QueryTicketStatusOperationCompleted, userState);
        }
        
        private void OnQueryTicketStatusOperationCompleted(object arg) {
            if ((this.QueryTicketStatusCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.QueryTicketStatusCompleted(this, new QueryTicketStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://ws.paybyphone.cc.gti.com/", ResponseNamespace="http://ws.paybyphone.cc.gti.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("ticket_payment_response", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ticket_payment_response PayTicket([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] ticket_payment_request ticket_payment_request) {
            object[] results = this.Invoke("PayTicket", new object[] {
                        ticket_payment_request});
            return ((ticket_payment_response)(results[0]));
        }
        
        /// <remarks/>
        public void PayTicketAsync(ticket_payment_request ticket_payment_request) {
            this.PayTicketAsync(ticket_payment_request, null);
        }
        
        /// <remarks/>
        public void PayTicketAsync(ticket_payment_request ticket_payment_request, object userState) {
            if ((this.PayTicketOperationCompleted == null)) {
                this.PayTicketOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPayTicketOperationCompleted);
            }
            this.InvokeAsync("PayTicket", new object[] {
                        ticket_payment_request}, this.PayTicketOperationCompleted, userState);
        }
        
        private void OnPayTicketOperationCompleted(object arg) {
            if ((this.PayTicketCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PayTicketCompleted(this, new PayTicketCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_list_request {
        
        private plate_query[] plate_queryField;
        
        private string dateField;
        
        private string city_codeField;
        
        private string ahField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("plate_query", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public plate_query[] plate_query {
            get {
                return this.plate_queryField;
            }
            set {
                this.plate_queryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string date {
            get {
                return this.dateField;
            }
            set {
                this.dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string city_code {
            get {
                return this.city_codeField;
            }
            set {
                this.city_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ah {
            get {
                return this.ahField;
            }
            set {
                this.ahField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class plate_query {
        
        private string plateField;
        
        private string stateField;
        
        private ticket[] ticketsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string plate {
            get {
                return this.plateField;
            }
            set {
                this.plateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string state {
            get {
                return this.stateField;
            }
            set {
                this.stateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public ticket[] tickets {
            get {
                return this.ticketsField;
            }
            set {
                this.ticketsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket {
        
        private string ticketnoField;
        
        private string articleField;
        
        private string infractionField;
        
        private int fineField;
        
        private string statusField;
        
        private bool payableField;
        
        private string inf_dateField;
        
        private string exp_dateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ticketno {
            get {
                return this.ticketnoField;
            }
            set {
                this.ticketnoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string article {
            get {
                return this.articleField;
            }
            set {
                this.articleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string infraction {
            get {
                return this.infractionField;
            }
            set {
                this.infractionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int fine {
            get {
                return this.fineField;
            }
            set {
                this.fineField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool payable {
            get {
                return this.payableField;
            }
            set {
                this.payableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string inf_date {
            get {
                return this.inf_dateField;
            }
            set {
                this.inf_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string exp_date {
            get {
                return this.exp_dateField;
            }
            set {
                this.exp_dateField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_payment_response {
        
        private int result_codeField;
        
        private string transaction_noField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int result_code {
            get {
                return this.result_codeField;
            }
            set {
                this.result_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string transaction_no {
            get {
                return this.transaction_noField;
            }
            set {
                this.transaction_noField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_payment_request {
        
        private string ticketnoField;
        
        private string dateField;
        
        private int amountField;
        
        private string ahField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ticketno {
            get {
                return this.ticketnoField;
            }
            set {
                this.ticketnoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string date {
            get {
                return this.dateField;
            }
            set {
                this.dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ah {
            get {
                return this.ahField;
            }
            set {
                this.ahField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_status_response {
        
        private int result_codeField;
        
        private string ticketnoField;
        
        private string articleField;
        
        private string infractionField;
        
        private string statusField;
        
        private string plateField;
        
        private string stateField;
        
        private string inf_dateField;
        
        private string exp_dateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int result_code {
            get {
                return this.result_codeField;
            }
            set {
                this.result_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ticketno {
            get {
                return this.ticketnoField;
            }
            set {
                this.ticketnoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string article {
            get {
                return this.articleField;
            }
            set {
                this.articleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string infraction {
            get {
                return this.infractionField;
            }
            set {
                this.infractionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string plate {
            get {
                return this.plateField;
            }
            set {
                this.plateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string state {
            get {
                return this.stateField;
            }
            set {
                this.stateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string inf_date {
            get {
                return this.inf_dateField;
            }
            set {
                this.inf_dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string exp_date {
            get {
                return this.exp_dateField;
            }
            set {
                this.exp_dateField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_status_request {
        
        private string ticketnoField;
        
        private string dateField;
        
        private string ahField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ticketno {
            get {
                return this.ticketnoField;
            }
            set {
                this.ticketnoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string date {
            get {
                return this.dateField;
            }
            set {
                this.dateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ah {
            get {
                return this.ahField;
            }
            set {
                this.ahField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18058")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.paybyphone.cc.gti.com/")]
    public partial class ticket_list_response {
        
        private int result_codeField;
        
        private plate_query[] plate_queryField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int result_code {
            get {
                return this.result_codeField;
            }
            set {
                this.result_codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("plate_query", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public plate_query[] plate_query {
            get {
                return this.plate_queryField;
            }
            set {
                this.plate_queryField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void QueryTicketListCompletedEventHandler(object sender, QueryTicketListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class QueryTicketListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal QueryTicketListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ticket_list_response Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ticket_list_response)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void QueryTicketStatusCompletedEventHandler(object sender, QueryTicketStatusCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class QueryTicketStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal QueryTicketStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ticket_status_response Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ticket_status_response)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void PayTicketCompletedEventHandler(object sender, PayTicketCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PayTicketCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PayTicketCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ticket_payment_response Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ticket_payment_response)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591