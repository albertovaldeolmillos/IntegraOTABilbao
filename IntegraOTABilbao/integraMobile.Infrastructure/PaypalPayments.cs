using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.paypal.soap.api;
using com.paypal.sdk.services;
using com.paypal.sdk.profiles;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Net;
using System.IO;
using integraMobile.Infrastructure.Logging.Tools;
using Newtonsoft.Json;
using com.paypal;

namespace integraMobile.Infrastructure
{
    public class PaypalPayments
    {

        private static readonly CLogWrapper m_Log = new CLogWrapper(typeof(PaypalPayments));
        protected const int DEFAULT_WS_TIMEOUT = 5000; //ms        

        static public bool ConfirmAppSDKPaypalPayment(string strPaypalClientID,
                                                      string strPaypalClientSecret,
                                                      string strPaypalURLPrefix,
                                                      string strAuthorizationId,
                                                      string strQuantity,
                                                      string strCurrencyISOCODE,
                                                      out string strSecondPaypalAuthId,
                                                      out int iTransactionFee,
                                                      out string strTransactionFeeCurrencyIsocode,
                                                      out string strTransactionURL,
                                                      out string strRefundTransactionURL)
        {

            bool bRes = false;
            strSecondPaypalAuthId="";
            iTransactionFee=0;
            strTransactionFeeCurrencyIsocode="";
            strTransactionURL="";
            strRefundTransactionURL = "";

            try
            {
                string strAccessToken = "";

                AddTLS12Support();

                if (GetPayPalToken(strPaypalClientID,
                                   strPaypalClientSecret,
                                   strPaypalURLPrefix,
                                   out strAccessToken))
                {
                    string strURL = string.Format("{0}payments/authorization/{1}/capture", strPaypalURLPrefix, strAuthorizationId);
                    WebRequest request = WebRequest.Create(strURL);

                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Timeout = Get3rdPartyWSTimeout();

                    request.Headers["Authorization"] = "Bearer " + strAccessToken;
                   
                    Dictionary<string, object> oDataDict = new Dictionary<string, object>();
                    Dictionary<string, object> oAmountDict = new Dictionary<string, object>();

                    oAmountDict["currency"] = strCurrencyISOCODE;
                    oAmountDict["total"] = strQuantity;
                    oDataDict["amount"] = oAmountDict;
                    oDataDict["is_final_capture"] = true;

                    var json = JsonConvert.SerializeObject(oDataDict);

                    m_Log.LogMessage(LogLevels.logINFO, string.Format("PaypalPayments.ConfirmAppSDKPaypalPayment: request.url={0}, request.json={1}", strURL, PrettyJSON(json)));

                    byte[] byteArray = Encoding.UTF8.GetBytes(json);

                    request.ContentLength = byteArray.Length;
                    // Get the request stream.

                    Stream dataStream = request.GetRequestStream();
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.
                    dataStream.Close();


                    try
                    {

                        WebResponse response = request.GetResponse();
                        // Display the status.
                        HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                        if (oWebResponse.StatusDescription == "OK")
                        {
                            // Get the stream containing content returned by the server.
                            dataStream = response.GetResponseStream();
                            // Open the stream using a StreamReader for easy access.
                            StreamReader reader = new StreamReader(dataStream);
                            // Read the content.
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.

                            m_Log.LogMessage(LogLevels.logINFO, string.Format("PaypalPayments.ConfirmAppSDKPaypalPayment: responseFromServer={0}", PrettyJSON(responseFromServer)));
                            // Clean up the streams.

                            dynamic oResponse = JsonConvert.DeserializeObject(responseFromServer);

                            strSecondPaypalAuthId = oResponse["id"];
                            if (oResponse["transaction_fee"] != null)
                            {
                                strTransactionFeeCurrencyIsocode = oResponse["transaction_fee"]["currency"];
                                NumberFormatInfo numberFormatProvider = new NumberFormatInfo();
                                numberFormatProvider.NumberDecimalSeparator = ".";
                                decimal dQuantity = Convert.ToDecimal(oResponse["transaction_fee"]["value"], numberFormatProvider) * 100;
                                iTransactionFee = Convert.ToInt32(dQuantity, numberFormatProvider);
                            }

                            if (oResponse["links"][0] != null)
                            {
                                strTransactionURL = oResponse["links"][0]["href"];
                            }
                            if (oResponse["links"][1] != null)
                            {
                                strRefundTransactionURL = oResponse["links"][1]["href"];
                            }


                            //strJobId = oJobStatus["id"];
                            reader.Close();
                            dataStream.Close();
                            bRes = true;
                        }

                        response.Close();
                    }
                    catch (WebException e)
                    {
                        bRes = false;
                        m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.ConfirmAppSDKPaypalPayment: ", e);
                    }


                }

            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.ConfirmAppSDKPaypalPayment: ", e);

            }

            return bRes;


        }




        static public bool ExpressCheckoutPassOne(AddressType address,
                                                 string strAmountToPay,
                                                 string strCurISOCode,
                                                 string strCancelURL,
                                                 string strReturnURL,
                                                 string strPaypalOrderDescription,
                                                 out SetExpressCheckoutResponseType PResponse)
        {

            bool bRes = false;
            PResponse = null;

            try
            {


                com.paypal.sdk.services.CallerServices caller = new com.paypal.sdk.services.CallerServices();

                IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();

                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENVIRONMENT"];
                caller.APIProfile = profile;


                // Create the request object.
                SetExpressCheckoutRequestType pp_request = new SetExpressCheckoutRequestType();
                pp_request.Version = ConfigurationManager.AppSettings["PAYPAL_API_VERSION"];

                // Add request-specific fields to the request.
                // Create the request details object.
                pp_request.SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType();
                pp_request.SetExpressCheckoutRequestDetails.PaymentAction = PaymentActionCodeType.Sale;//Enum for PaymentAction is  PaymentActionCodeType.Sale
                pp_request.SetExpressCheckoutRequestDetails.PaymentActionSpecified = true;
                pp_request.SetExpressCheckoutRequestDetails.OrderDescription = strPaypalOrderDescription;
                pp_request.SetExpressCheckoutRequestDetails.NoShipping = "1";
                pp_request.SetExpressCheckoutRequestDetails.ReqConfirmShipping = "1";
                pp_request.SetExpressCheckoutRequestDetails.AddressOverride = "1";
                pp_request.SetExpressCheckoutRequestDetails.PageStyle = "PayPal";
                pp_request.SetExpressCheckoutRequestDetails.AddressOverride = "1";

                pp_request.SetExpressCheckoutRequestDetails.Address = address;

                pp_request.SetExpressCheckoutRequestDetails.OrderTotal = new BasicAmountType();

                pp_request.SetExpressCheckoutRequestDetails.OrderTotal.currencyID = GetCurrencyCodeType(strCurISOCode);
                pp_request.SetExpressCheckoutRequestDetails.OrderTotal.Value = strAmountToPay;

                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails = new PaymentDetailsType[1];
                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails[0] = new PaymentDetailsType();
                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails[0].PaymentDetailsItem = new PaymentDetailsItemType[1];
                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails[0].PaymentDetailsItem[0] = new PaymentDetailsItemType();
                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails[0].PaymentDetailsItem[0].Name = strPaypalOrderDescription;
                pp_request.SetExpressCheckoutRequestDetails.PaymentDetails[0].PaymentDetailsItem[0].Amount = new BasicAmountType()
                {
                    currencyID = GetCurrencyCodeType(strCurISOCode),
                    Value = strAmountToPay
                };

                pp_request.SetExpressCheckoutRequestDetails.CancelURL = strCancelURL;
                pp_request.SetExpressCheckoutRequestDetails.ReturnURL = strReturnURL;

                // Execute the API operation and obtain the response.

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutPassOne.Request: " + System.Environment.NewLine + pp_request.ToString());

                SetExpressCheckoutResponseType pp_response = new SetExpressCheckoutResponseType();
                pp_response = (SetExpressCheckoutResponseType)caller.Call("SetExpressCheckout", pp_request);
                PResponse = pp_response;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutPassOne.Response: " + System.Environment.NewLine + PResponse.ToString());


                bRes = ((PResponse.Ack == AckCodeType.Success) ||
                        (PResponse.Ack == AckCodeType.SuccessWithWarning));


            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.ExpressCheckoutPassOne: ", e);

            }

            return bRes;


        }


        static public bool ExpressCheckoutPassTwo(string strTokenPassTwo,
                                                  out GetExpressCheckoutDetailsResponseType PResponse)
        {

            bool bRes = false;
            PResponse = null;

            try
            {

                com.paypal.sdk.services.CallerServices caller = new com.paypal.sdk.services.CallerServices();

                IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();

                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENVIRONMENT"];
                caller.APIProfile = profile;


                // Create the request object.
                GetExpressCheckoutDetailsRequestType pp_request = new GetExpressCheckoutDetailsRequestType();
                pp_request.Version = ConfigurationManager.AppSettings["PAYPAL_API_VERSION"];

                // Add request-specific fields to the request.
                pp_request.Token = strTokenPassTwo;

                // Execute the API operation and obtain the response.

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutPassTwo.Request: " + System.Environment.NewLine + pp_request.ToString());

                GetExpressCheckoutDetailsResponseType pp_response = new GetExpressCheckoutDetailsResponseType();
                pp_response = (GetExpressCheckoutDetailsResponseType)caller.Call("GetExpressCheckoutDetails", pp_request);
                PResponse = pp_response;
                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutPassTwo.Response: " + System.Environment.NewLine + PResponse.ToString());


                bRes = ((PResponse.Ack == AckCodeType.Success) ||
                    (PResponse.Ack == AckCodeType.SuccessWithWarning));


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.ExpressCheckoutPassTwo: ", e);
                bRes = false;
            }

            return bRes;


        }



        static public bool ExpressCheckoutConfirm(string strPayerID,
                                                 string strTokenPassThree,
                                                 string strAmountToPay,
                                                 string strCurISOCode,
                                                 out DoExpressCheckoutPaymentResponseType PResponse)
        {

            bool bRes = false;
            PResponse = null;

            try
            {


                com.paypal.sdk.services.CallerServices caller = new com.paypal.sdk.services.CallerServices();

                IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();

                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENVIRONMENT"];
                caller.APIProfile = profile;


                // Create the request object.
                DoExpressCheckoutPaymentRequestType pp_request = new DoExpressCheckoutPaymentRequestType();
                pp_request.Version = ConfigurationManager.AppSettings["PAYPAL_API_VERSION"];

                // Add request-specific fields to the request.
                // Create the request details object.
                pp_request.DoExpressCheckoutPaymentRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType();
                pp_request.DoExpressCheckoutPaymentRequestDetails.Token = strTokenPassThree;
                pp_request.DoExpressCheckoutPaymentRequestDetails.PayerID = strPayerID;
                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentAction = PaymentActionCodeType.Sale;

                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails = new PaymentDetailsType[1];

                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0] = new PaymentDetailsType();

                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0].OrderTotal = new BasicAmountType();

                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0].OrderTotal.currencyID = GetCurrencyCodeType(strCurISOCode);
                pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0].OrderTotal.Value = strAmountToPay;

                //pp_request.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0].

                // Execute the API operation and obtain the response.

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutConfirm.Request: " + System.Environment.NewLine + pp_request.ToString());

                DoExpressCheckoutPaymentResponseType pp_response = new DoExpressCheckoutPaymentResponseType();
                pp_response = (DoExpressCheckoutPaymentResponseType)caller.Call("DoExpressCheckoutPayment", pp_request);
                PResponse = pp_response;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.ExpressCheckoutConfirm.Response: " + System.Environment.NewLine + PResponse.ToString());

                bRes = ((PResponse.Ack == AckCodeType.Success) ||
                    (PResponse.Ack == AckCodeType.SuccessWithWarning));


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.ExpressCheckoutConfirm: ", e);
                bRes = false;
            }

            return bRes;


        }



        static public bool PreapprovalRequest(string strPaypalID,
                                  string strCurISOCode,
                                  string strErrorLanguage,
                                  string strCancelURL,
                                  string strReturnURL,
                                  out PayPal.Services.Private.AP.PreapprovalResponse PResponse)
        {

            bool bRes = false;
            PResponse = null;

            try
            {



                PayPal.Services.Private.AP.PreapprovalRequest preapprovalRequest = null;
                PayPal.Platform.SDK.BaseAPIProfile profile = new PayPal.Platform.SDK.BaseAPIProfile();

                profile.APIProfileType = PayPal.Platform.SDK.ProfileType.ThreeToken;
                profile.ApplicationID = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENPOINT"];
                profile.RequestDataformat = ConfigurationManager.AppSettings["PAYPAL_API_REQUESTFORMAT"];
                profile.ResponseDataformat = ConfigurationManager.AppSettings["PAYPAL_API_RESPONSEFORMAT"];
                profile.IsTrustAllCertificates = Convert.ToBoolean(ConfigurationManager.AppSettings["PAYPAL_API_TRUST_ALL"]);


                preapprovalRequest = new PayPal.Services.Private.AP.PreapprovalRequest();
                preapprovalRequest.cancelUrl = strCancelURL;
                preapprovalRequest.returnUrl = strReturnURL;
                preapprovalRequest.senderEmail = strPaypalID;
                preapprovalRequest.requestEnvelope = new PayPal.Services.Private.AP.RequestEnvelope();
                preapprovalRequest.requestEnvelope.errorLanguage = strErrorLanguage;

                if (ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS"] != null)
                {
                    preapprovalRequest.maxNumberOfPayments = Convert.ToInt32(ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS"].ToString());
                    preapprovalRequest.maxNumberOfPaymentsSpecified = true;
                }


                if (ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_AMOUNTS_CURRENCY"] != null)
                {
                    string strDefCurr = ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_AMOUNTS_CURRENCY"].ToString();
                    NumberFormatInfo formatProvider = new NumberFormatInfo();
                    formatProvider.NumberDecimalSeparator = ".";

                    if (ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT"] != null)
                    {
                        double dmaxTotalAmountOfAllPayments = Convert.ToDouble(ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT"].ToString(), formatProvider);
                        double dResult = -1;
                        if (dmaxTotalAmountOfAllPayments > 0)
                        {
                            dResult = CCurrencyConvertor.ConvertCurrency(dmaxTotalAmountOfAllPayments,
                                                                              strDefCurr,
                                                                              strCurISOCode);

                            if (dResult >= 0)
                            {
                                preapprovalRequest.maxTotalAmountOfAllPayments = Convert.ToDecimal(dResult);
                                preapprovalRequest.maxTotalAmountOfAllPaymentsSpecified = true;

                            }

                        }
                    }


                    if (ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT"] != null)
                    {
                        double dmaxAmountPerPayment = Convert.ToDouble(ConfigurationManager.AppSettings["PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT"].ToString(), formatProvider);
                        double dResult = -1;
                        if (dmaxAmountPerPayment > 0)
                        {
                            dResult = CCurrencyConvertor.ConvertCurrency(dmaxAmountPerPayment,
                                                                              strDefCurr,
                                                                              strCurISOCode);

                            if (dResult >= 0)
                            {
                                preapprovalRequest.maxAmountPerPayment = Convert.ToDecimal(dResult);
                                preapprovalRequest.maxAmountPerPaymentSpecified = true;
                            }

                        }
                    }
                }



                preapprovalRequest.currencyCode = strCurISOCode;
                preapprovalRequest.startingDate = DateTime.Now.ToUniversalTime();
                preapprovalRequest.endingDate = DateTime.Now.ToUniversalTime().AddYears(1);
                preapprovalRequest.endingDateSpecified = true;
                preapprovalRequest.clientDetails = new PayPal.Services.Private.AP.ClientDetailsType();
                preapprovalRequest.clientDetails.applicationId = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                preapprovalRequest.clientDetails.deviceId = ConfigurationManager.AppSettings["PAYPAL_API_DEVICE_ID"];
                preapprovalRequest.clientDetails.ipAddress = ConfigurationManager.AppSettings["PAYPAL_API_IP_ADDRESS"];

                PayPal.Platform.SDK.AdapativePayments ap = new PayPal.Platform.SDK.AdapativePayments();
                ap.APIProfile = profile;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalRequest.Request: " + System.Environment.NewLine + preapprovalRequest.ToString());

                PResponse = ap.preapproval(preapprovalRequest);

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalRequest.Response: " + System.Environment.NewLine + PResponse.ToString());


                bRes = (ap.isSuccess.ToUpper() != "FAILURE");

            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.PreapprovalRequest: ", e);

            }

            return bRes;


        }


        static public bool PreapprovalConfirm(string strPaypalIDPreapprovalKey,
                                              string strErrorLanguage,            
                                              out PayPal.Services.Private.AP.PreapprovalDetailsResponse PResponse,
                                              out string strPreapprovalKey)
        {

            bool bRes = false;
            PResponse = null;
            strPreapprovalKey = "";

            try
            {


                PayPal.Services.Private.AP.PreapprovalDetailsRequest preapprovalDetailsRequest = null;
                PayPal.Platform.SDK.BaseAPIProfile profile = new PayPal.Platform.SDK.BaseAPIProfile();

                profile.APIProfileType = PayPal.Platform.SDK.ProfileType.ThreeToken;
                profile.ApplicationID = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENPOINT"];
                profile.RequestDataformat = ConfigurationManager.AppSettings["PAYPAL_API_REQUESTFORMAT"];
                profile.ResponseDataformat = ConfigurationManager.AppSettings["PAYPAL_API_RESPONSEFORMAT"];
                profile.IsTrustAllCertificates = Convert.ToBoolean(ConfigurationManager.AppSettings["PAYPAL_API_TRUST_ALL"]);


                preapprovalDetailsRequest = new PayPal.Services.Private.AP.PreapprovalDetailsRequest();
                preapprovalDetailsRequest.preapprovalKey =strPaypalIDPreapprovalKey;
                preapprovalDetailsRequest.requestEnvelope = new PayPal.Services.Private.AP.RequestEnvelope();
                preapprovalDetailsRequest.requestEnvelope.errorLanguage =strErrorLanguage;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalConfirm.Request: " + System.Environment.NewLine + preapprovalDetailsRequest.ToString());

                PayPal.Platform.SDK.AdapativePayments ap = new PayPal.Platform.SDK.AdapativePayments();
                ap.APIProfile = profile;
                PResponse = ap.preapprovalDetails(preapprovalDetailsRequest);

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalConfirm.Response: " + System.Environment.NewLine + PResponse.ToString());

                bRes = ((ap.isSuccess.ToUpper() != "FAILURE") && (PResponse.status == "ACTIVE"));
                strPreapprovalKey = preapprovalDetailsRequest.preapprovalKey;


            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.PreapprovalConfirm: ", e);

            }

            return bRes;


        }




        static public bool PreapprovalPayRequest(string strPaypalID,
                                          string strPaypalPreapprovalKey,
                                          decimal dAmountToPay,
                                          string strCurISOCode,
                                          string strErrorLanguage,
                                          string strCancelURL,
                                          string strReturnURL,
                                          out PayPal.Services.Private.AP.PayResponse PResponse)
        {

            bool bRes=false;
            PResponse = null;

            try
            {
                

                PayPal.Services.Private.AP.PayRequest payRequest = null;
                PayPal.Platform.SDK.BaseAPIProfile profile = new PayPal.Platform.SDK.BaseAPIProfile();

                profile.APIProfileType = PayPal.Platform.SDK.ProfileType.ThreeToken;
                profile.ApplicationID = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENPOINT"];
                profile.RequestDataformat = ConfigurationManager.AppSettings["PAYPAL_API_REQUESTFORMAT"];
                profile.ResponseDataformat = ConfigurationManager.AppSettings["PAYPAL_API_RESPONSEFORMAT"];
                profile.IsTrustAllCertificates = Convert.ToBoolean(ConfigurationManager.AppSettings["PAYPAL_API_TRUST_ALL"]);

                payRequest = new PayPal.Services.Private.AP.PayRequest();
                payRequest.cancelUrl = strCancelURL;
                payRequest.returnUrl = strReturnURL;
                payRequest.senderEmail = strPaypalID;
                payRequest.clientDetails = new PayPal.Services.Private.AP.ClientDetailsType();
                payRequest.clientDetails.applicationId = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                payRequest.clientDetails.deviceId = ConfigurationManager.AppSettings["PAYPAL_API_DEVICE_ID"];
                payRequest.clientDetails.ipAddress = ConfigurationManager.AppSettings["PAYPAL_API_IP_ADDRESS"];
                payRequest.actionType = "PAY";
                payRequest.currencyCode = strCurISOCode;
                payRequest.requestEnvelope = new PayPal.Services.Private.AP.RequestEnvelope();
                payRequest.requestEnvelope.errorLanguage = strErrorLanguage;

                payRequest.receiverList = new PayPal.Services.Private.AP.Receiver[1];
                payRequest.receiverList[0] = new PayPal.Services.Private.AP.Receiver();
                payRequest.receiverList[0].amount = dAmountToPay;
                payRequest.receiverList[0].email = ConfigurationManager.AppSettings["PAYPAL_API_PAYPAL_ID"];
                payRequest.preapprovalKey = strPaypalPreapprovalKey;

                PayPal.Platform.SDK.AdapativePayments ap = new PayPal.Platform.SDK.AdapativePayments();
                ap.APIProfile = profile;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalPayRequest.Request: " + System.Environment.NewLine + payRequest.ToString());

                PResponse = ap.pay(payRequest);

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalPayRequest.Response: " + System.Environment.NewLine + PResponse.ToString());

                bRes = (ap.isSuccess.ToUpper() != "FAILURE");

            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.PreapprovalPayRequest: ", e);

            }

            return bRes;


        }


        static public bool PreapprovalPayConfirm( string strPaypalPreapprovaPayKey,
                                                  string strErrorLanguage,
                                                  out PayPal.Services.Private.AP.PaymentDetailsResponse PResponse)
        {

            bool bRes = false;
            PResponse = null;

            try
            {

                PayPal.Services.Private.AP.PaymentDetailsRequest pDetailsRequest = null;
                PayPal.Platform.SDK.BaseAPIProfile profile = new PayPal.Platform.SDK.BaseAPIProfile();

                profile.APIProfileType = PayPal.Platform.SDK.ProfileType.ThreeToken;
                profile.ApplicationID = ConfigurationManager.AppSettings["PAYPAL_API_APPLICATION-ID"];
                profile.APIUsername = ConfigurationManager.AppSettings["PAYPAL_API_USERNAME"];
                profile.APIPassword = ConfigurationManager.AppSettings["PAYPAL_API_PASSWORD"];
                profile.APISignature = ConfigurationManager.AppSettings["PAYPAL_API_SIGNATURE"];
                profile.Environment = ConfigurationManager.AppSettings["PAYPAL_API_ENPOINT"];
                profile.RequestDataformat = ConfigurationManager.AppSettings["PAYPAL_API_REQUESTFORMAT"];
                profile.ResponseDataformat = ConfigurationManager.AppSettings["PAYPAL_API_RESPONSEFORMAT"];
                profile.IsTrustAllCertificates = Convert.ToBoolean(ConfigurationManager.AppSettings["PAYPAL_API_TRUST_ALL"]);


                pDetailsRequest = new PayPal.Services.Private.AP.PaymentDetailsRequest();
                pDetailsRequest.payKey = strPaypalPreapprovaPayKey;
                pDetailsRequest.requestEnvelope = new PayPal.Services.Private.AP.RequestEnvelope();
                pDetailsRequest.requestEnvelope.errorLanguage = strErrorLanguage;

                PayPal.Platform.SDK.AdapativePayments ap = new PayPal.Platform.SDK.AdapativePayments();
                ap.APIProfile = profile;

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalPayConfirm.Request: " + System.Environment.NewLine + pDetailsRequest.ToString());

                PResponse = ap.paymentDetails(pDetailsRequest);

                m_Log.LogMessage(LogLevels.logDEBUG, "PaypalPayments.PreapprovalPayConfirm.Response: " + System.Environment.NewLine + PResponse.ToString());


                bRes = ((ap.isSuccess.ToUpper() != "FAILURE") && (PResponse.status == "COMPLETED"));

            }
            catch(Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.PreapprovalPayConfirm: ", e);

            }

            return bRes;


        }

        static public CurrencyCodeType GetCurrencyCodeType(string strIsoCode)
        {

            
            CurrencyCodeType eReturn = CurrencyCodeType.USD;
            switch (strIsoCode)
            {
                case "AUD":
                    eReturn = CurrencyCodeType.AUD;
                    break;
                case "CAD":
                    eReturn = CurrencyCodeType.CAD;
                    break;
                case "CHF":
                    eReturn = CurrencyCodeType.CHF;
                    break;
                case "CZK":
                    eReturn = CurrencyCodeType.CZK;
                    break;
                case "DKK":
                    eReturn = CurrencyCodeType.DKK;
                    break;
                case "EUR":
                    eReturn = CurrencyCodeType.EUR;
                    break;
                case "GBP":
                    eReturn = CurrencyCodeType.GBP;
                    break;
                case "HKD":
                    eReturn = CurrencyCodeType.HKD;
                    break;
                case "HUF":
                    eReturn = CurrencyCodeType.HUF;
                    break;
                case "ILS":
                    eReturn = CurrencyCodeType.ILS;
                    break;
                case "JPY":
                    eReturn = CurrencyCodeType.JPY;
                    break;
                case "MXN":
                    eReturn = CurrencyCodeType.MXN;
                    break;
                case "NOK":
                    eReturn = CurrencyCodeType.NOK;
                    break;
                case "NZD":
                    eReturn = CurrencyCodeType.NZD;
                    break;
                case "PLN":
                    eReturn = CurrencyCodeType.PLN;
                    break;
                case "SEK":
                    eReturn = CurrencyCodeType.SEK;
                    break;
                case "SGD":
                    eReturn = CurrencyCodeType.SGD;
                    break;
                case "USD":
                    eReturn = CurrencyCodeType.USD;
                    break;
                default:
                    eReturn = CurrencyCodeType.USD;
                    break;
            }

            return eReturn;


        }


        static protected bool GetPayPalToken(string strClientId,
                                             string strSecret, 
                                             string strPaypalURLPrefix, 
                                             out string strAccessToken)
        {

            bool bRes = false;
            strAccessToken = "";

            try
            {
                string strURL = string.Format("{0}oauth2/token", strPaypalURLPrefix);

                WebRequest request = WebRequest.Create(strURL);

                request.Method = "POST";
                //request.ContentType = "application/json";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Accept-Language:en_US");   
                request.Timeout = Get3rdPartyWSTimeout();

                string authInfo = strClientId + ":" + strSecret;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                request.Headers["Authorization"] = "Basic " + authInfo;

                using (StreamWriter swt = new StreamWriter(request.GetRequestStream()))
                {
                    swt.Write("grant_type=client_credentials");
                }
               
               
                try
                {

                    WebResponse response = request.GetResponse();
                    // Display the status.
                    HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                    if (oWebResponse.StatusDescription == "OK")
                    {
                        // Get the stream containing content returned by the server.
                        Stream dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.
                        m_Log.LogMessage(LogLevels.logINFO, string.Format("PaypalPayments.GetPayPalToken response.json={0}", PrettyJSON(responseFromServer)));
                        dynamic oResponse = JsonConvert.DeserializeObject(responseFromServer);

                        strAccessToken = (string)oResponse["access_token"];                        
                        bRes=true;
                        reader.Close();
                        dataStream.Close();
                    }

                    response.Close();
                }
                catch (WebException e)
                {
                    m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.GetPayPalToken: ", e);
                    bRes=false;
                }




            }
            catch (Exception e)
            {
                bRes = false;
                m_Log.LogMessage(LogLevels.logERROR, "PaypalPayments.GetPayPalToken: ", e);

            }

            return bRes;


        }

        static string PrettyJSON(string json)
        {

            try
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                string strRes = JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
                return "\r\n\t" + strRes.Replace("\r\n", "\r\n\t") + "\r\n";
            }
            catch
            {
                return "\r\n\t" + json + "\r\n";
            }
        }

        static protected int Get3rdPartyWSTimeout()
        {
            int iRes = DEFAULT_WS_TIMEOUT;
            try
            {
                iRes = Convert.ToInt32(ConfigurationManager.AppSettings["3rdPartyWSTimeout"].ToString());
            }
            catch
            {
                iRes = DEFAULT_WS_TIMEOUT;
            }

            return iRes;

        }


          
        protected static void AddTLS12Support()
        {
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Tls12) == 0) //Enable TLs 1.2
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Ssl3) != 0) //Disable SSL3
            {
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            }
        }
    }
}