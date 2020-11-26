using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using integraMobile.Domain;
using integraMobile.Domain.Abstract;
using integraMobile.Infrastructure;
using integraMobile.Infrastructure.Logging.Tools;
using Ninject;
using Newtonsoft.Json;

namespace integraMobile.ExternalWS
{

    public enum ResultTypeStandardFineWS
    {
        Ok = 1,
        InvalidAuthenticationHashExternalService = -1,
        InvalidAuthenticationExternalService = -21,
        InvalidAuthentication = -101,
        TicketNotFound = -211,
        InstallationNotFound = -220,
        TicketNumberNotFound = -224,
        TicketTypeNotPayable = -226,
        TicketPaymentPeriodExpired = -227,
        TicketAlreadyCancelled = -228,
        TicketAlreadyAnulled = -229,
        TicketAlreadyRemitted = -230,
        TicketAlreadyPaid = -231,
        TicketNotClosed = -232,
        InvalidPaymentAmount = -233,
        InvalidExternalProvider = -234,
        InvalidAuthenticationHash = -920,
        ErrorDetected = -990,
        GenericError = -999
    }



    public enum ResultTypePICBilbao
    {
        Result_OK = 1,
        Result_Error_InvalidAuthenticationHash = -1,
        Result_Error_Generic = -9,
        Result_Error_Invalid_Input_Parameter = -10,
        Result_Error_Missing_Input_Parameter = -11,
        Result_Error_Invalid_City = -12,
        Result_Error_InvalidExternalProvider = -14,
        Result_Error_Invalid_Unit = -15,
        Result_Error_TicketPaymentAlreadyExist = -19,
        Result_Error_TicketNotPayable = -20,
    }

    public class ThirdPartyFine : ThirdPartyBase
    {
        internal class GDLGTechnaAPI
        {
            private WebService oWS = null;
            private string strResultString = "";

            public GDLGTechnaAPI(string webserviceEndpoint)
            {
                oWS = new WebService(webserviceEndpoint);
            }

            public int Timeout
            {
                set
                {
                    oWS.Timeout = value;

                }
            }


            public string Username
            {
                set
                {
                    oWS.Username = value;

                }
            }


            public string Password
            {
                set
                {
                    oWS.Password = value;

                }
            }

            public string ResultXML
            {
                get
                {
                    return strResultString;
                }
            }

            public ResultType GetOutstandingTickets(string strTicketNumber, string strType, string strProvider, string strSecurityToken, out SortedList oTicket)
            {
                ResultType rtRes = ResultType.Result_Error_Generic;
                oTicket = null;
                oWS.PreInvoke();

                oWS.AddParameter("TicketNo", strTicketNumber);
                oWS.AddParameter("Type", strType);
                oWS.AddParameter("Source", strProvider);
                oWS.AddParameter("SecurityToken", strSecurityToken);

                try
                {
                    oWS.Invoke("GetOutstandingTicketsRequest", "GetOutstandingTicketsResponse", "http://soap.payment.seci.cc.gti.com/");
                    strResultString = oWS.ResultString;

                    if (oWS.GetOutputElementCount("Success") == 1)
                    {
                        SortedList oList = null;

                        int iCount = oWS.GetOutputElementCount("Success/OutstandingTickets/Ticket");

                        if (iCount > 1)
                        {
                            int i = 0;

                            while (i < iCount)
                            {
                                oList = oWS.GetOutputElement("Success/OutstandingTickets/Ticket/" + i.ToString());
                                if (oList["TicketNo"].ToString() == strTicketNumber)
                                {
                                    break;
                                }
                                else
                                    oList = null;
                                i++;
                            }
                        }
                        else if (iCount == 1)
                        {
                            oList = oWS.GetOutputElement("Success/OutstandingTickets/Ticket");
                            if (oList != null)
                            {
                                if (oList["TicketNo"].ToString() != strTicketNumber)
                                {
                                    oList = null;
                                    rtRes = ResultType.Result_Error_Fine_Number_Already_Paid;
                                }
                            }
                        }
                        else
                        {
                            rtRes = ResultType.Result_Error_Fine_Number_Already_Paid;
                        }


                        if (oList != null)
                        {
                            oTicket = oList;

                            if (oTicket["TicketStatus"].ToString() == "PA")
                                rtRes = ResultType.Result_Error_Fine_Number_Already_Paid;
                            else if (oTicket["PayableStatus"].ToString() == "Y")
                                rtRes = ResultType.Result_OK;
                            else
                                rtRes = ResultType.Result_Error_Fine_Type_Not_Payable;

                        }
                    }
                    else if (oWS.GetOutputElementCount("InvalidParameters/InvalidParameter") == 1)
                    {

                        SortedList oList = oWS.GetOutputElement("InvalidParameters");

                        if (oList["InvalidParameter"].ToString().ToLower().Contains("invalid"))
                        {
                            rtRes = ResultType.Result_Error_Invalid_Input_Parameter;
                        }
                        else if (oList["InvalidParameter"].ToString().ToLower().Contains("found"))
                        {
                            rtRes = ResultType.Result_Error_Fine_Number_Not_Found;
                        }

                    }
                    else if (oWS.GetOutputElementCount("Error/ErrorMessage") == 1)
                    {
                        rtRes = ResultType.Result_Error_Generic;
                    }


                }
                catch (Exception e)
                {
                    strResultString = e.Message;
                    rtRes = ResultType.Result_Error_Generic;
                }
                finally { oWS.PosInvoke(); }

                return rtRes;
            }

            public ResultType PayTicket(string strTicketNumber, double dAmount, string strPaymentType, DateTime dtPayment, string strProvider,
                                        decimal dTicketId, string strSecurityToken, out SortedList oTransaction)
            {
                ResultType rtRes = ResultType.Result_Error_Generic;
                oTransaction = null;
                oWS.PreInvoke();

                oWS.AddParameter("TicketNo", strTicketNumber);
                oWS.AddParameter("Amount", dAmount.ToString(CultureInfo.InvariantCulture));
                oWS.AddParameter("PaymentType", strPaymentType);
                oWS.AddParameter("PaymentDateTime", dtPayment.ToString("yyyy-MM-dd HH:mm:ss ") + dtPayment.ToString("zzz").Replace(":", ""));
                oWS.AddParameter("TransactionBy", strProvider);
                oWS.AddParameter("Source", strProvider);
                oWS.AddParameter("ReferenceNo", dTicketId.ToString());
                oWS.AddParameter("SecurityToken", strSecurityToken);


                try
                {
                    oWS.Invoke("PayTicketsRequest", "PayTicketsResponse", "http://soap.payment.seci.cc.gti.com/");
                    strResultString = oWS.ResultString;

                    if (oWS.GetOutputElementCount("Success") == 1)
                    {

                        oTransaction = oWS.GetOutputElement("Success/TransactionSet/Transaction");

                        if (oTransaction != null)
                            rtRes = ResultType.Result_OK;

                    }
                    else if (oWS.GetOutputElementCount("InvalidParameters/InvalidParameter") == 1)
                    {

                        SortedList oList = oWS.GetOutputElement("InvalidParameters");

                        if (oList["InvalidParameter"].ToString().ToLower().Contains("invalid"))
                        {
                            rtRes = ResultType.Result_Error_Invalid_Input_Parameter;
                        }
                        else if (oList["InvalidParameter"].ToString().ToLower().Contains("found"))
                        {
                            rtRes = ResultType.Result_Error_Fine_Number_Not_Found;
                        }

                    }
                    else if (oWS.GetOutputElementCount("Error/ErrorMessage") == 1)
                    {
                        rtRes = ResultType.Result_Error_Generic;
                    }


                }
                catch (Exception e)
                {
                    strResultString = e.Message;
                    rtRes = ResultType.Result_Error_Generic;
                }
                finally { oWS.PosInvoke(); }

                return rtRes;
            }
        }

        public ThirdPartyFine() : base()
        {
            m_Log = new CLogWrapper(typeof(ThirdPartyFine));
        }

       /* public ResultType EysaQueryFinePayment(string strFineNumber, DateTime dtFineQuery, USER oUser, INSTALLATION oInstallation,
                                                out int iQuantity, out string strPlate,
                                                out string strArticleType, out string strArticleDescription)
        {
            ResultType rtRes = ResultType.Result_OK;
            iQuantity = 0;
            strPlate = "";
            strArticleDescription = "";
            strArticleType = "";

            try
            {
                SortedList parametersIn = new SortedList();
                SortedList parametersOut = new SortedList();

                parametersIn["f"] = strFineNumber;

                rtRes = EysaQueryFinePaymentQuantity(parametersIn, strFineNumber, dtFineQuery, oUser, oInstallation, ref parametersOut);

                if (rtRes == ResultType.Result_OK)
                {
                    iQuantity = Convert.ToInt32(parametersOut["q"].ToString());
                    strPlate = parametersOut["lp"].ToString();
                    strArticleType = parametersOut["ta"].ToString();
                    strArticleDescription = parametersOut["dta"].ToString();
                }



            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogException(e, "EysaQueryFinePaymentQuantity::Exception", LogLevels.logERROR);

            }


            return rtRes;


        }

        */



        public ResultType StandardQueryFinePaymentQuantity(SortedList parametersIn, string strFineNumber, DateTime dtFineQuery, USER oUser, INSTALLATION oInstallation, ref SortedList parametersOut)
        {

            ResultType rtRes = ResultType.Result_Error_Generic;
            Stopwatch watch = null;


            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                watch = Stopwatch.StartNew();
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);


                string strURL = oInstallation.INS_FINE_WS_URL + "/querypaymentinfo";
                WebRequest request = WebRequest.Create(strURL);
                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    request.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_WS_HTTP_USER, oInstallation.INS_FINE_WS_HTTP_PASSWORD);
                }

                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = Get3rdPartyWSTimeout();


                string strFineID = strFineNumber;
                DateTime dtInstallation = dtFineQuery;                
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;
                string strProviderName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();



                Dictionary<string, object> ojsonInObjectDict = new Dictionary<string, object>();
                Dictionary<string, object> oiparkticketInObjectDict = new Dictionary<string, object>();
                Dictionary<string, object> oDataObjectDict = new Dictionary<string, object>();
               
                oDataObjectDict["cityid"] = strCityID;
                oDataObjectDict["ticketnumber"] = strFineID;
                oDataObjectDict["date"] = dtInstallation.ToString("HHmmssddMMyyyy");
                oDataObjectDict["provider"] = strProviderName;
                oDataObjectDict["ah"] = CalculateStandardWSHash(oInstallation.INS_FINE_WS_AUTH_HASH_KEY,
                                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}", strCityID, strFineID, dtInstallation, strProviderName)); ;
                oiparkticketInObjectDict["iparkticket_in"] = oDataObjectDict;

                ojsonInObjectDict["jsonIn"] = JsonConvert.SerializeObject(oiparkticketInObjectDict).ToString();
                var json = JsonConvert.SerializeObject(ojsonInObjectDict);

                Logger_AddLogMessage(string.Format("StandardQueryFinePaymentQuantity request.url={0}, request.json={1}", strURL, json), LogLevels.logINFO);

                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.             

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

                        Logger_AddLogMessage(string.Format("StandardQueryFinePaymentQuantity response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                        // Clean up the streams.

                        dynamic oResponse = JsonConvert.DeserializeObject(responseFromServer);

                        var strResult = oResponse["Result"];

                        long lResult=Convert.ToInt32(strResult);

                        rtRes = Convert_ResultTypeStandardFineWS_TO_ResultType((ResultTypeStandardFineWS)lResult);

                        if (rtRes == ResultType.Result_OK)
                        {

                            dynamic oData = JsonConvert.DeserializeObject(oResponse["Data"].ToString()); ;
                            lResult = Convert.ToInt64(oData["res"]);
                            rtRes = Convert_ResultTypeStandardFineWS_TO_ResultType((ResultTypeStandardFineWS)lResult);

                            if (rtRes == ResultType.Result_OK)
                            {
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                                parametersOut["q"] = oData["amount"];
                                parametersOut["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;
                                parametersOut["lp"] = oData["plate"];

                                DateTime dt = DateTime.ParseExact(oData["ticketdate"].ToString(), "HHmmssddMMyyyy",
                                            CultureInfo.InvariantCulture);
                               
                                parametersOut["d"] = dt.ToString("HHmmssddMMyy");

                                try
                                {
                                    dt = DateTime.ParseExact(oData["maximumPayDate"].ToString(), "HHmmssddMMyyyy",
                                            CultureInfo.InvariantCulture);
                                    parametersOut["df"] = dt.ToString("HHmmssddMMyy");
                                }
                                catch
                                {
                                }
                                parametersOut["ta"] = oData["code"];
                                parametersOut["dta"] = oData ["description"];                                

                            }
                            else
                            {

                                try
                                {
                                    parametersOut["lp"] = oData["plate"].ToString().Trim().Replace(" ", "");
                                    DateTime dt = DateTime.ParseExact(oData["ticketdate"].ToString(), "HHmmssddMMyyyy",
                                                CultureInfo.InvariantCulture);
                                    parametersOut["d"] = dt.ToString("HHmmssddMMyy");
                                    try
                                    {
                                        dt = DateTime.ParseExact(oData["maximumPayDate"].ToString(), "HHmmssddMMyyyy",
                                                CultureInfo.InvariantCulture);
                                        parametersOut["df"] = dt.ToString("HHmmssddMMyy");
                                    }
                                    catch
                                    {
                                    } 
                                    parametersOut["ta"] = oData["code"];
                                    parametersOut["dta"] = oData["description"];
                                }
                                catch
                                {

                                }
                            }

                            var strTicketNumber = oData["ticketnumber"].ToString();
                            if (strFineID != strTicketNumber)
                            {
                                parametersOut["fnumber"] = strTicketNumber;
                            }

                        }

                       
                        reader.Close();
                        dataStream.Close();
                    }

                    response.Close();
                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, "StandardQueryFinePaymentQuantity::Exception", LogLevels.logERROR);
                    rtRes = ResultType.Result_Error_Generic;
                    parametersOut["r"] = Convert.ToInt32(rtRes);
                }

               

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);
                Logger_AddLogException(e, "StandardQueryFinePaymentQuantity::Exception", LogLevels.logERROR);
            }
            finally
            {
                watch.Stop();
                watch = null;

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;          
        }


        public ResultType StandardConfirmFinePayment(int iWSNumber, string strFineNumber, DateTime dtOperationDate, int iQuantity, USER oUser, decimal dTicketPaymentID,
                                                     INSTALLATION oInstallation,ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
        {

            ResultType rtRes = ResultType.Result_Error_Generic;
            Stopwatch watch = null;
            str3dPartyOpNum = "";
            lEllapsedTime = -1;


            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                watch = Stopwatch.StartNew();
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);




                string strHashKey = "";
                WebRequest request = null;
                string strURL = "";

                switch (iWSNumber)
                {
                    case 1:
                       
                        strURL = oInstallation.INS_FINE_CONFIRM_WS_URL + "/pay";
                        request = WebRequest.Create(strURL);
               
                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER))
                        {
                            request.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS_AUTH_HASH_KEY;
                        break;

                    case 2:
                        strURL = oInstallation.INS_FINE_CONFIRM_WS2_URL + "/pay";
                        request = WebRequest.Create(strURL);

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER))
                        {
                            request.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS2HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS2_AUTH_HASH_KEY;
                        break;

                    case 3:
                        strURL = oInstallation.INS_FINE_CONFIRM_WS3_URL + "/pay";
                        request = WebRequest.Create(strURL);

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER))
                        {
                            request.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS3HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS3_AUTH_HASH_KEY;
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("StandardConfirmFinePayment::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }



                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = Get3rdPartyWSTimeout();


                string strFineID = strFineNumber;
                DateTime dtInstallation = dtOperationDate;
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;
                string strProviderName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();


                Dictionary<string, object> ojsonInObjectDict = new Dictionary<string, object>();
                Dictionary<string, object> oiparkticketInObjectDict = new Dictionary<string, object>();
                Dictionary<string, object> oDataObjectDict = new Dictionary<string, object>();

                oDataObjectDict["cityid"] = strCityID;
                oDataObjectDict["ticketnumber"] = strFineID;
                oDataObjectDict["date"] = dtInstallation.ToString("HHmmssddMMyyyy");
                oDataObjectDict["amount"] = iQuantity;
                oDataObjectDict["provider"] = strProviderName;
                oDataObjectDict["op"] = dTicketPaymentID.ToString();
                oDataObjectDict["payinfo"] = oUser.USR_EMAIL;
                oDataObjectDict["ah"] = CalculateStandardWSHash(strHashKey,
                                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}", strCityID, strFineID, dtInstallation, iQuantity, strProviderName, dTicketPaymentID.ToString(), oUser.USR_EMAIL)); ;
                oiparkticketInObjectDict["iparkticket_in"] = oDataObjectDict;

                ojsonInObjectDict["jsonIn"] = JsonConvert.SerializeObject(oiparkticketInObjectDict).ToString();
                var json = JsonConvert.SerializeObject(ojsonInObjectDict);

                Logger_AddLogMessage(string.Format("StandardConfirmFinePaymentQuantity request.url={0}, request.json={1}", strURL, json), LogLevels.logINFO);

                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
              
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
                        lEllapsedTime = watch.ElapsedMilliseconds;
                        Logger_AddLogMessage(string.Format("StandardConfirmFinePaymentQuantity response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                        // Clean up the streams.

                        dynamic oResponse = JsonConvert.DeserializeObject(responseFromServer);

                        var strResult = oResponse["Result"];

                        long lResult = Convert.ToInt32(strResult);

                        rtRes = Convert_ResultTypeStandardFineWS_TO_ResultType((ResultTypeStandardFineWS)lResult);

                        if (rtRes == ResultType.Result_OK)
                        {

                            dynamic oData = JsonConvert.DeserializeObject(oResponse["Data"].ToString()); ;
                            lResult = Convert.ToInt64(oData["res"]);
                            rtRes = Convert_ResultTypeStandardFineWS_TO_ResultType((ResultTypeStandardFineWS)lResult);

                            if (rtRes == ResultType.Result_OK)
                            {
                                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                                if (oData["id"] != null)
                                {
                                    str3dPartyOpNum = oData["id"].ToString();
                                }

                            }
                            else
                            {                                
                                parametersOut["r"] = Convert.ToInt32(rtRes);
                                if (parametersOut.IndexOfKey("autorecharged") >= 0)
                                {
                                    parametersOut.RemoveAt(parametersOut.IndexOfKey("autorecharged"));
                                }
                                if (parametersOut.IndexOfKey("newbal") >= 0)
                                {
                                    parametersOut.RemoveAt(parametersOut.IndexOfKey("newbal"));
                                }
                            }                           
                        }
                       
                        reader.Close();
                        dataStream.Close();
                    }

                    response.Close();
                }
                catch (Exception e)
                {
                    if ((watch != null) && (lEllapsedTime == -1))
                    {
                        lEllapsedTime = watch.ElapsedMilliseconds;
                    }
                    Logger_AddLogException(e, "StandardConfirmFinePaymentQuantity::Exception", LogLevels.logERROR);
                    rtRes = ResultType.Result_Error_Generic;
                    parametersOut["r"] = Convert.ToInt32(rtRes);

                }

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);
                Logger_AddLogException(e, "StandardConfirmFinePaymentQuantity::Exception", LogLevels.logERROR);
            }
            finally
            {
                watch.Stop();
                watch = null;

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;          
 

        }


        public ResultType EysaQueryFinePaymentQuantity(SortedList parametersIn, string strFineNumber, DateTime dtFineQuery,string strCulturePrefix, USER oUser, INSTALLATION oInstallation, ref SortedList parametersOut)
        {

            ResultType rtRes = ResultType.Result_OK;
#if DEBUG
            //HACK: Para poder obtener un resultado valido.
            return rtRes;
#endif

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                AddTLS11Support();
                //ServicePointManager.Expect100Continue = true;


                integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones oFineWS = new integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones();
                oFineWS.Url = oInstallation.INS_FINE_WS_URL;
                oFineWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_WS_HTTP_USER, oInstallation.INS_FINE_WS_HTTP_PASSWORD);
                }

                integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader authentication = new integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oFineWS.ConsolaSoapHeaderValue = authentication;


                string strFineID = strFineNumber;
                DateTime dtInstallation = dtFineQuery;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;

                string strAuthHash = CalculateEysaWSHash(oInstallation.INS_FINE_WS_AUTH_HASH_KEY,
                    string.Format("{0}{1}{2:yyyy-MM-ddTHH:mm:ss.fff}{3}", strFineID, strCityID, dtInstallation, strvers));
                string strMessage = string.Format("<ipark_in><f>{0}</f><city_id>{1}</city_id><d>{2:yyyy-MM-ddTHH:mm:ss.fff}</d><vers>{3}</vers><ah>{4}</ah></ipark_in>",
                    strFineID, strCityID, dtInstallation, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaQueryFinePaymentQuantity xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = oFineWS.rdPQueryFinePaymentQuantity(strMessage);
                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaQueryFinePaymentQuantity xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    if (Convert.ToInt32(wsParameters["r"].ToString()) == (int)ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        parametersOut["q"] = wsParameters["q"];
                        parametersOut["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;
                        parametersOut["lp"] = wsParameters["lp"].ToString().Trim().Replace(" ", "");

                        DateTime dt = DateTime.ParseExact(wsParameters["d"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                     CultureInfo.InvariantCulture);
                        parametersOut["d"] = dt.ToString("HHmmssddMMyy");
                        dt = DateTime.ParseExact(wsParameters["df"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                                             CultureInfo.InvariantCulture);
                        parametersOut["df"] = dt.ToString("HHmmssddMMyy");
                        parametersOut["ta"] = wsParameters["ta"];
                        if (wsParameters.ContainsKey("dta"))
                            parametersOut["dta"] = wsParameters["dta"];
                        else
                        {
                            if (wsParameters.ContainsKey("dta_lang_0_" + strCulturePrefix))
                                parametersOut["dta"] = wsParameters["dta_lang_0_" + strCulturePrefix];
                            else if (wsParameters.ContainsKey("dta_lang_0_es"))
                                parametersOut["dta"] = wsParameters["dta_lang_0_es"];
                            else
                                parametersOut["dta"] = "---------------";

                            if (wsParameters.ContainsKey("dta_sector"))
                                parametersOut["sector"] = wsParameters["dta_sector"];
                            if (wsParameters.ContainsKey("dta_user"))
                                parametersOut["enforcuser"] = wsParameters["dta_user"];
                        }
                        if (wsParameters.ContainsKey("lit"))
                            parametersOut["lit"] = wsParameters["lit"];

                    }
                    else
                    {
                        //denuncia ya remesada = denuncia encontrada pero el plazo de anulación ya ha pasado.
                        parametersOut["r"] = ((Convert.ToInt32(wsParameters["r"])) == -4) ?
                            Convert.ToInt32(ResultType.Result_Error_Fine_Payment_Period_Expired) : Convert.ToInt32(wsParameters["r"]);

                        rtRes = (ResultType)parametersOut["r"];

                        try
                        {
                            parametersOut["lp"] = wsParameters["lp"].ToString().Trim().Replace(" ", "");
                            DateTime dt = DateTime.ParseExact(wsParameters["d"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                         CultureInfo.InvariantCulture);
                            parametersOut["d"] = dt.ToString("HHmmssddMMyy");
                            dt = DateTime.ParseExact(wsParameters["df"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                                                 CultureInfo.InvariantCulture);
                            parametersOut["df"] = dt.ToString("HHmmssddMMyy");
                            parametersOut["ta"] = wsParameters["ta"];
                            if (wsParameters.ContainsKey("dta"))
                                parametersOut["dta"] = wsParameters["dta"];
                            else
                                parametersOut["dta"] = wsParameters["dta_lang_0_es"];
                        }
                        catch
                        {

                        }

                    }

                    if (wsParameters.ContainsKey("fnumber"))
                        parametersOut["fnumber"] = Regex.Replace(wsParameters["fnumber"].ToString(), "[^0-9]", "");
                    else
                    {
                        // *** HB ***
                        //if (oInstallation.INS_ID == 28) parametersOut["fnumber"] = "111111-1";
                        // *** HB ***
                    }

                }



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "EysaQueryFinePaymentQuantity::Exception", LogLevels.logERROR);

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;

        }


        public ResultType EysaConfirmFinePayment(int iWSNumber, string strFineNumber, DateTime dtOperationDate, int iQuantity, USER oUser, INSTALLATION oInstallation,
                                                    ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime )
        {

            ResultType rtRes = ResultType.Result_OK;
            str3dPartyOpNum = "";
            lEllapsedTime = -1;
            Stopwatch watch = null;


            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                AddTLS11Support();
                //ServicePointManager.Expect100Continue = true;

                integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones oFineWS = new integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones();

                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS_AUTH_HASH_KEY;
                        break;

                    case 2:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS2_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS2HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS2_AUTH_HASH_KEY;
                        break;

                    case 3:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS3_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS3HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS3_AUTH_HASH_KEY;
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("EysaConfirmFinePayment::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader authentication = new integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oFineWS.ConsolaSoapHeaderValue = authentication;

                string strFineID = strFineNumber;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;

                string strAuthHash = CalculateEysaWSHash(strHashKey,
                    string.Format("{0}{1}{2:yyyy-MM-ddTHH:mm:ss.fff}{3}{4}", strFineID, strCityID, dtOperationDate, iQuantity, strvers));

                string strMessage = string.Format("<ipark_in><f>{0}</f><city_id>{1}</city_id><d>{2:yyyy-MM-ddTHH:mm:ss.fff}</d><q>{3}</q><vers>{4}</vers><ah>{5}</ah></ipark_in>",
                    strFineID, strCityID, dtOperationDate, iQuantity, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaConfirmFinePayment xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oFineWS.rdPConfirmFinePayment(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaConfirmFinePayment xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    if (Convert.ToInt32(wsParameters["r"].ToString()) > 0)
                    {
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        if (wsParameters["opnum"] != null)
                        {
                            str3dPartyOpNum = wsParameters["opnum"].ToString();
                        }
                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        if (parametersOut.IndexOfKey("autorecharged") >= 0)
                        {
                            parametersOut.RemoveAt(parametersOut.IndexOfKey("autorecharged"));
                        }
                        if (parametersOut.IndexOfKey("newbal")>=0)
                        {
                            parametersOut.RemoveAt(parametersOut.IndexOfKey("newbal"));
                        }

                    }
                }

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "EysaConfirmFinePayment::Exception", LogLevels.logERROR);
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;

        }

        public bool EysaThirdPartyQueryListOfFines(USER oUser, INSTALLATION oInstallation, DateTime dtinstDateTime, ref SortedList parametersOut)
        {

            bool bRes = true;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                AddTLS11Support();
                //ServicePointManager.Expect100Continue = true;


                integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones oFineWS = new integraMobile.ExternalWS.EysaThirdPartyFineWS.Anulaciones();
                oFineWS.Url = oInstallation.INS_FINE_WS_URL;
                oFineWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_WS_HTTP_USER, oInstallation.INS_FINE_WS_HTTP_PASSWORD);
                }

                integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader authentication = new integraMobile.ExternalWS.EysaThirdPartyFineWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oFineWS.ConsolaSoapHeaderValue = authentication;

                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;

                int iNumPlates = oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1).Count();
                string strPlatesList = "";
                string strPlateListForHash = "";
                int iIndex = 0;



                var oUserPlates = oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1).OrderBy(r => r.USRP_PLATE).ToArray();
                foreach (USER_PLATE oPlate in oUserPlates)
                {
                    iIndex++;
                    strPlatesList += string.Format("<lp{0}>{1}</lp{0}>", iIndex, oPlate.USRP_PLATE);
                    strPlateListForHash += oPlate.USRP_PLATE;
                    strPlatesList += string.Format("<st{0}>{1}</st{0}>", iIndex, "1");
                    strPlateListForHash += "1";
                }

                string strAuthHash = CalculateEysaWSHash(oInstallation.INS_FINE_WS_AUTH_HASH_KEY,
                    string.Format("{0}{1}{2}{3:yyyy-MM-ddTHH:mm:ss.fff}{4}", strCityID, iNumPlates, strPlateListForHash, dtinstDateTime, strvers));

                string strMessage = string.Format("<ipark_in><city_id>{0}</city_id><nlp>{1}</nlp>{2}<d>{3:yyyy-MM-ddTHH:mm:ss.fff}</d><vers>{4}</vers><ah>{5}</ah></ipark_in>",
                    strCityID, iNumPlates, strPlatesList, dtinstDateTime, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaThirdPartyQueryListOfFines xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                string strWSOut = oFineWS.rdPQueryListOfFines(strMessage);
                strWSOut = strWSOut.Replace("\r\n  ", "");
                strWSOut = strWSOut.Replace("\r\n ", "");
                strWSOut = strWSOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strWSOut);

                Logger_AddLogMessage(string.Format("EysaThirdPartyQueryListOfFines xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);
                string strXMLOut = "";
                bRes = FindEysaQueryListOfFinesOutParameters(oUserPlates, strWSOut, out strXMLOut);

                parametersOut["userMSG"] = strXMLOut;



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                bRes = false;
                Logger_AddLogException(e, "EysaThirdPartyQueryListOfFines::Exception", LogLevels.logERROR);
            }

            ResultType rtRes = (bRes? ResultType.Result_OK: ResultType.Result_Error_Generic);
            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return bRes;

        }

        private bool FindEysaQueryListOfFinesOutParameters(USER_PLATE[] oUserPlates, string xmlIn, out string strXMLOut)
        {
            bool bRes = true;
            strXMLOut = "";

            try
            {
                XmlDocument xmlInDoc = new XmlDocument();
                try
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        ((sender, certificate, chain, sslPolicyErrors) => true); 

                    xmlInDoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xmlIn);
                    int? iNumInputFines = null;
                    int iNumCountFines = 0;
                    string strCurrentPlate = null;

                    XmlNodeList Nodes = xmlInDoc.SelectNodes("//" + _xmlTagName + OUT_SUFIX + "/*");
                    foreach (XmlNode Node in Nodes)
                    {
                        if (Node.Name == "r")
                        {
                            iNumInputFines = Convert.ToInt32(Node.ChildNodes[0].InnerText.Trim());
                            if (iNumInputFines <= 0)
                                break;
                        }
                        else if (Node.Name.Substring(0, 2) == "lp")
                        {
                            strCurrentPlate = oUserPlates[Convert.ToInt32(Node.Name.Substring(2, Node.Name.Length - 2)) - 1].USRP_PLATE;

                            if (Node.HasChildNodes)
                            {
                                if (Node.ChildNodes[0].HasChildNodes)
                                {
                                    //for each fine fines
                                    foreach (XmlNode xmlFine in Node.ChildNodes)
                                    {
                                        string strXmlFine = string.Format("<usertick><lp>{0}</lp>", strCurrentPlate);
                                        bool bFineOK = true;
                                        int? iFineQuantity = null;

                                        foreach (XmlNode xmlFineData in xmlFine.ChildNodes)
                                        {
                                            /*if ((iFineQuantity.HasValue) &&
                                                (iFineQuantity.Value <= 0))
                                            {
                                                bFineOK = false;
                                                break;
                                            }*/

                                            string strData = xmlFineData.InnerText.Trim();

                                            switch (xmlFineData.Name)
                                            {
                                                case "f":
                                                    strXmlFine += string.Format("<f>{0}</f>", strData);
                                                    break;
                                                case "a":
                                                    iFineQuantity = Convert.ToInt32(strData);
                                                    strXmlFine += string.Format("<q>{0}</q>", strData);
                                                    break;
                                                case "d":
                                                    DateTime dt = DateTime.ParseExact(strData, "yyyy-MM-ddTHH:mm:ss.fff",
                                                                 CultureInfo.InvariantCulture);
                                                    strXmlFine += string.Format("<d>{0}</d>", dt.ToString("HHmmssddMMyy"));
                                                    break;
                                                case "df":
                                                    DateTime dtFinal = DateTime.ParseExact(strData, "yyyy-MM-ddTHH:mm:ss.fff",
                                                                 CultureInfo.InvariantCulture);
                                                    strXmlFine += string.Format("<df>{0}</df>", dtFinal.ToString("HHmmssddMMyy"));
                                                    break;
                                                case "ta":
                                                    strXmlFine += string.Format("<ta>{0}</ta>", strData);
                                                    break;
                                                case "dta":
                                                    strXmlFine += "";//string.Format("<dta>{0}</dta>", strData);
                                                    break;
                                                default:
                                                    break;
                                            }


                                        }


                                        if (bFineOK)
                                        {
                                            strXmlFine += "</usertick>";
                                            strXMLOut += strXmlFine;
                                            iNumCountFines++;
                                        }

                                    }
                                }
                            }
                        }
                    }

                    if (iNumCountFines > 0)
                    {
                        strXMLOut = "<userticks>" + strXMLOut + "</userticks>";
                    }
                    else
                    {
                        strXMLOut = "";

                    }


                    if (Nodes.Count == 0)
                    {
                        Logger_AddLogMessage(string.Format("FindEysaQueryListOfFinesOutParameters: Bad Input XML: xmlIn={0}", PrettyXml(xmlIn)), LogLevels.logERROR);
                        bRes = false;
                        strXMLOut = "";
                    }


                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, string.Format("FindEysaQueryListOfFinesOutParameters: Bad Input XML: xmlIn={0}:Exception", PrettyXml(xmlIn)), LogLevels.logERROR);
                    bRes = false;
                    strXMLOut = "";
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "FindEysaQueryListOfFinesOutParameters::Exception", LogLevels.logERROR);
                bRes = false;
                strXMLOut = "";

            }


            return bRes;
        }

        public bool GtechnaQueryListOfFines(USER oUser, INSTALLATION oInstallation, DateTime dtinstDateTime, ref SortedList parametersOut)
        {

            bool bRes = true;

            string sParamsIn = "";
            string sParamsOut = "";
            Exception oNotificationEx = null;            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.PayByPhoneOperationService oFineWS = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.PayByPhoneOperationService();
                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_list_request request = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_list_request();
                oFineWS.Url = oInstallation.INS_FINE_WS_URL;
                oFineWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_WS_HTTP_USER, oInstallation.INS_FINE_WS_HTTP_PASSWORD);
                }

                int iNumPlates = oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1).Count();
                request.plate_query = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.plate_query[iNumPlates];
                request.city_code = oInstallation.INS_GTECHNA_CITY_CODE;


                string strPlatesListHash = "";
                int iIndex = 0;

                var oUserPlates = oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1).OrderBy(r => r.USRP_PLATE).ToArray();
                foreach (USER_PLATE oPlate in oUserPlates)
                {
                    request.plate_query[iIndex] = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.plate_query();
                    bool bPrefixFound = false;
                    /*foreach (string strProvince in CanadaAndUSAProvinces)
                    {
                        if (oPlate.USRP_PLATE.Substring(0, 2) == strProvince)
                        {
                            bPrefixFound = true;
                            break;
                        }
                    }*/

                    if (bPrefixFound)
                    {
                        request.plate_query[iIndex].plate = oPlate.USRP_PLATE.Substring(2, oPlate.USRP_PLATE.Length - 2);
                        request.plate_query[iIndex].state = oPlate.USRP_PLATE.Substring(0, 2);
                    }
                    else
                    {
                        request.plate_query[iIndex].plate = oPlate.USRP_PLATE;
                        request.plate_query[iIndex].state = "";
                    }

                    strPlatesListHash += request.plate_query[iIndex].plate;
                    strPlatesListHash += request.plate_query[iIndex].state;
                    iIndex++;
                }

                request.date = string.Format("{0:HHmmssddMMyy}", dtinstDateTime);

                string strAuthHash = CalculateGtechnaWSHash(oInstallation.INS_FINE_WS_AUTH_HASH_KEY,
                    string.Format("{0}{1:HHmmssddMMyy}{2}", strPlatesListHash, dtinstDateTime, oInstallation.INS_GTECHNA_CITY_CODE));

                request.ah = strAuthHash;

                sParamsIn = string.Format("GtechnaQueryListOfFines request ={0}", request.ToString());

                Logger_AddLogMessage(sParamsIn, LogLevels.logDEBUG);

                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_list_response response = oFineWS.QueryTicketList(request);

                sParamsOut = string.Format("GtechnaQueryListOfFines response ={0}", response.ToString());

                Logger_AddLogMessage(sParamsOut, LogLevels.logDEBUG);


                string strXMLOut = "";
                foreach (integraMobile.ExternalWS.gTechnaThirdPartyFineWS.plate_query plate_query in response.plate_query)
                {

                    if (plate_query.tickets != null)
                    {

                        foreach (integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket ticket in plate_query.tickets)
                        {


                            if (ticket.payable)
                            {

                                if (strXMLOut.Length == 0)
                                {
                                    strXMLOut = "<userticks>";
                                }
                                strXMLOut += string.Format("<usertick><lp>{0}{1}</lp>", plate_query.state, plate_query.plate);
                                strXMLOut += string.Format("<f>{0}</f>", ticket.ticketno);
                                DateTime dt = DateTime.ParseExact(ticket.inf_date, "HHmmssddMMyy",
                                            CultureInfo.InvariantCulture);
                                strXMLOut += string.Format("<d>{0}</d>", dt.ToString("HHmmssddMMyy"));
                                if (ticket.exp_date != null)
                                {
                                    dt = DateTime.ParseExact(ticket.exp_date, "HHmmssddMMyy",
                                            CultureInfo.InvariantCulture);

                                }
                                else
                                {

                                    dt = dt.AddDays(1825);
                                }

                                strXMLOut += string.Format("<df>{0}</df>", dt.ToString("HHmmssddMMyy"));

                                strXMLOut += string.Format("<q>{0}</q>", ticket.fine);
                                strXMLOut += string.Format("<ta>{0}</ta>", ticket.article);
                                strXMLOut += string.Format("<dta>{0}</dta>", ticket.infraction);
                                strXMLOut += "</usertick>";

                            }

                        }
                    }
                }


                if (strXMLOut.Length > 0)
                {
                    strXMLOut += "</userticks>";
                }


                parametersOut["userMSG"] = strXMLOut;


            }
            catch (Exception e)
            {
                oNotificationEx = e;
                bRes = false;
                Logger_AddLogException(e, "GtechnaQueryListOfFines::Exception", LogLevels.logERROR);
            }

            ResultType rtRes = (bRes ? ResultType.Result_OK : ResultType.Result_Error_Generic);
            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sParamsIn, sParamsOut, false, oNotificationEx);
            }
            catch
            {
            }

            return bRes;

        }



        /*public ResultType GtechnaQueryFinePayment(string strFineNumber, DateTime dtFineQuery, INSTALLATION oInstallation,
                                                out int iQuantity, out string strPlate,
                                                out string strArticleType, out string strArticleDescription)
        {
            ResultType rtRes = ResultType.Result_OK;
            iQuantity = 0;
            strPlate = "";
            strArticleDescription = "";
            strArticleType = "";

            try
            {
                SortedList parametersIn = new SortedList();
                SortedList parametersOut = new SortedList();

                parametersIn["f"] = strFineNumber;

                rtRes = GtechnaQueryFinePaymentQuantity(parametersIn, strFineNumber, dtFineQuery, oInstallation, ref parametersOut);

                if (rtRes == ResultType.Result_OK)
                {
                    iQuantity = Convert.ToInt32(parametersOut["q"].ToString());
                    strPlate = parametersOut["lp"].ToString();
                    strArticleType = parametersOut["ta"].ToString();
                    strArticleDescription = parametersOut["dta"].ToString();
                }



            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogException(e, "GtechnaQueryFinePaymentQuantity::Exception", LogLevels.logERROR);

            }


            return rtRes;


        }
        */
        /*
        public ResultType GtechnaQueryFinePaymentQuantity(SortedList parametersIn, string strFineNumber, DateTime dtFineQuery, INSTALLATION oInstallation, ref SortedList parametersOut)
        {

            ResultType rtRes = ResultType.Result_OK;

            string sParamsIn = "";
            string sParamsOut = "";
            Exception oNotificationEx = null;            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.PayByPhoneOperationService oFineWS = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.PayByPhoneOperationService();
                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_status_request request = new integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_status_request();
                oFineWS.Url = oInstallation.INS_FINE_WS_URL;
                oFineWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_WS_HTTP_USER, oInstallation.INS_FINE_WS_HTTP_PASSWORD);
                }

                string strFineID = strFineNumber;
                DateTime dtInstallation = dtFineQuery;


                string strAuthHash = CalculateGtechnaWSHash(oInstallation.INS_FINE_WS_AUTH_HASH_KEY,
                    string.Format("{0}{1:HHmmssddMMyy}", strFineID, dtInstallation));


                request.ticketno = strFineNumber;
                request.date = string.Format("{0:HHmmssddMMyy}", dtInstallation);
                request.ah = strAuthHash;

                sParamsIn = string.Format("GtechnaQueryFinePaymentQuantity request ={0}", request.ToString());

                Logger_AddLogMessage(sParamsIn, LogLevels.logDEBUG);

                integraMobile.ExternalWS.gTechnaThirdPartyFineWS.ticket_status_response response = oFineWS.QueryTicketStatus(request);

                sParamsOut = string.Format("GtechnaQueryFinePaymentQuantity response ={0}", response.ToString());

                Logger_AddLogMessage(sParamsOut, LogLevels.logDEBUG);


                if (response.result_code > 0)
                {
                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                    parametersOut["q"] = response.result_code;
                    parametersOut["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;
                    parametersOut["lp"] = response.state + response.plate;
                    parametersOut["d"] = DateTime.ParseExact(response.inf_date, "HHmmssddMMyy",
                                                             CultureInfo.InvariantCulture);

                    if (response.exp_date != null)
                    {
                        parametersOut["df"] = DateTime.ParseExact(response.exp_date, "HHmmssddMMyy",
                                                                CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        parametersOut["df"] = DateTime.ParseExact(response.inf_date, "HHmmssddMMyy",
                                                                  CultureInfo.InvariantCulture).AddDays(1825);
                    }

                    parametersOut["ta"] = response.article;
                    parametersOut["dta"] = response.infraction;
                    parametersOut["lit"] = "";


                }
                else
                {
                    rtRes = (ResultType)response.result_code;
                    parametersOut["r"] = response.result_code;
                    parametersOut["lp"] = "";
                    parametersOut["d"] = "";
                    parametersOut["df"] = "";
                    parametersOut["ta"] = "";
                    parametersOut["dta"] = "";
                }


            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                Logger_AddLogException(e, "GtechnaQueryFinePaymentQuantity::Exception", LogLevels.logERROR);
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sParamsIn, sParamsOut, false, oNotificationEx);
            }
            catch
            { }

            return rtRes;

        }
        */

        public ResultType GtechnaQueryFinePaymentQuantity(SortedList parametersIn, string strFineNumber, DateTime dtFineQuery, INSTALLATION oInstallation, ref SortedList parametersOut)
        {

            ResultType rtRes = ResultType.Result_OK;

            string sParamsIn = "";
            string sParamsOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);


                GDLGTechnaAPI oFineWS = new GDLGTechnaAPI(oInstallation.INS_FINE_WS_URL);
                SortedList oTicket = null;
                oFineWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oFineWS.Username = oInstallation.INS_FINE_WS_HTTP_USER;
                    oFineWS.Password = oInstallation.INS_FINE_WS_HTTP_PASSWORD;
                }

                string strFineID = strFineNumber;
                string strCompanyName = ConfigurationManager.AppSettings["GtechnaCompanyName"].ToString();

                sParamsIn = string.Format("GtechnaQueryFinePaymentQuantity TicketID = {0}", strFineID);

                Logger_AddLogMessage(sParamsIn, LogLevels.logDEBUG);

                rtRes= oFineWS.GetOutstandingTickets(strFineID, "P", strCompanyName, oInstallation.INS_FINE_WS_AUTH_HASH_KEY, out oTicket);
                Logger_AddLogMessage(string.Format("GtechnaQueryFinePaymentQuantity ResultXML = {0}", oFineWS.ResultXML), LogLevels.logDEBUG);

                sParamsOut = string.Format("GtechnaQueryFinePaymentQuantity response = {0}\r\n",rtRes);
                if (oTicket != null)
                {
                    IDictionaryEnumerator ide = oTicket.GetEnumerator(); 
	
                    while (ide.MoveNext())
                    {
                        sParamsOut+= string.Format("\t\t{0} = {1}\r\n",ide.Key.ToString(),ide.Value.ToString());
                    }                  

                }

                Logger_AddLogMessage(sParamsOut, LogLevels.logDEBUG);


                if (rtRes == ResultType.Result_OK)
                {
                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                    parametersOut["q"] = Convert.ToInt32(Convert.ToDouble(oTicket["Balance"].ToString(),CultureInfo.InvariantCulture) * Math.Pow(10, Convert.ToDouble(oInstallation.CURRENCy.CUR_MINOR_UNIT)));

                    parametersOut["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;
                    parametersOut["lp"] = oTicket["Plate"].ToString();
                    parametersOut["d"] = DateTime.ParseExact(oTicket["InfractionDate"].ToString(), "yyyy-MM-dd HH:mm:ss zzz",
                                                             CultureInfo.InvariantCulture).ToString("HHmmssddMMyy");

                    

                    parametersOut["ta"] = "";
                    parametersOut["dta"] = "";
                    parametersOut["lit"] = "";


                }
                else
                {
                    parametersOut["r"] = rtRes;
                    parametersOut["lp"] = "";
                    parametersOut["d"] = "";
                    parametersOut["df"] = "";
                    parametersOut["ta"] = "";
                    parametersOut["dta"] = "";
                }

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "GtechnaQueryFinePaymentQuantity::Exception", LogLevels.logERROR);
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sParamsIn, sParamsOut, false, oNotificationEx);
            }
            catch
            { }

            return rtRes;

        }




        public ResultType GtechnaConfirmFinePayment(int iWSNumber, string strFineNumber, DateTime dtFineQuery, int iQuantity, decimal dTicketPaymentID, INSTALLATION oInstallation,
                                                        ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
        {

            ResultType rtRes = ResultType.Result_OK;
            str3dPartyOpNum = "";
            Stopwatch watch = null;
            lEllapsedTime = -1;

            string sParamsIn = "";
            string sParamsOut = "";
            Exception oNotificationEx = null;            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);


                SortedList oTransaction = null;
                GDLGTechnaAPI oFineWS = null;
                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oFineWS = new GDLGTechnaAPI(oInstallation.INS_FINE_CONFIRM_WS_URL);

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER))
                        {
                            oFineWS.Username = oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER;
                            oFineWS.Password = oInstallation.INS_FINE_CONFIRM_WS_HTTP_PASSWORD;
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS_AUTH_HASH_KEY;
                        break;

                    case 2:
                        oFineWS = new GDLGTechnaAPI(oInstallation.INS_FINE_CONFIRM_WS2_URL);

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER))
                        {
                            oFineWS.Username = oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER;
                            oFineWS.Password = oInstallation.INS_FINE_CONFIRM_WS2HTTP_PASSWORD;
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS2_AUTH_HASH_KEY;
                        break;

                    case 3:
                        oFineWS = new GDLGTechnaAPI(oInstallation.INS_FINE_CONFIRM_WS3_URL);

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER))
                        {
                            oFineWS.Username = oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER;
                            oFineWS.Password = oInstallation.INS_FINE_CONFIRM_WS3HTTP_PASSWORD;
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS3_AUTH_HASH_KEY;
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("EysaConfirmFinePayment::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                oFineWS.Timeout = Get3rdPartyWSTimeout();


                string strFineID = strFineNumber;
                string strCompanyName = ConfigurationManager.AppSettings["GtechnaCompanyName"].ToString();
                double dAmount = Math.Round(Convert.ToDouble(iQuantity) / Math.Pow(10, Convert.ToDouble(oInstallation.CURRENCy.CUR_MINOR_UNIT)), oInstallation.CURRENCy.CUR_MINOR_UNIT ?? 0);

                sParamsIn = string.Format("GtechnaConfirmFinePayment TicketID = {0}\r\n", strFineID);
                sParamsIn += string.Format("\t\tAmount = {0}\r\n", dAmount);
                sParamsIn += string.Format("\t\tDate = {0}\r\n", dtFineQuery);
                sParamsIn += string.Format("\t\tTicket Payment ID = {0}\r\n", dTicketPaymentID);

                Logger_AddLogMessage(sParamsIn, LogLevels.logDEBUG);


                rtRes = oFineWS.PayTicket(strFineID, dAmount, "WALLET", dtFineQuery, strCompanyName, dTicketPaymentID, strHashKey, out oTransaction);
                Logger_AddLogMessage(string.Format("GtechnaConfirmFinePayment ResultXML = {0}", oFineWS.ResultXML), LogLevels.logDEBUG);

                sParamsOut = string.Format("GtechnaConfirmFinePayment response = {0}\r\n", rtRes);
                if (oTransaction != null)
                {
                    IDictionaryEnumerator ide = oTransaction.GetEnumerator();

                    while (ide.MoveNext())
                    {
                        sParamsOut += string.Format("\t\t{0} = {1}\r\n", ide.Key.ToString(), ide.Value.ToString());
                    }

                }

                Logger_AddLogMessage(sParamsOut, LogLevels.logDEBUG);


                if (rtRes == ResultType.Result_OK)
                {
                    parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                    str3dPartyOpNum = oTransaction["TransactionID"].ToString();
                }
                else
                {
                    parametersOut["r"] = Convert.ToInt32(rtRes);
                    if (parametersOut.IndexOfKey("autorecharged") >= 0)
                    {
                        parametersOut.RemoveAt(parametersOut.IndexOfKey("autorecharged"));
                    }
                    if (parametersOut.IndexOfKey("newbal") >= 0)
                    {
                        parametersOut.RemoveAt(parametersOut.IndexOfKey("newbal"));
                    }

                }


            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "GtechnaConfirmFinePayment::Exception", LogLevels.logERROR);
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sParamsIn, sParamsOut, false, oNotificationEx);
            }
            catch
            {
            }

            return rtRes;

        }

        public ResultType MadridPlatformQueryFinePaymentQuantity(SortedList parametersIn, string strFineNumber, DateTime dtFineQuery, USER oUser, INSTALLATION oInstallation, ref SortedList parametersOut)
        {

            ResultType rtRes = ResultType.Result_OK;

            string strParamsIn = "";
            string strParamsOut = "";
            Exception oNotificationEx = null;

            MadridPlatform.PublishServiceV12Client oService = null;
            MadridPlatform.AuthSession oAuthSession = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                oService = new MadridPlatform.PublishServiceV12Client();
                // oParkWS.Timeout = Get3rdPartyWSTimeout();

                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        ((sender2, certificate, chain, sslPolicyErrors) => true);

                //string strHashKey = "";

                oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_FINE_WS_URL);
                //strHashKey = oGroup.INSTALLATION.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                if (!string.IsNullOrEmpty(oInstallation.INS_FINE_WS_HTTP_USER))
                {
                    oService.ClientCredentials.UserName.UserName = oInstallation.INS_FINE_WS_HTTP_USER;
                    oService.ClientCredentials.UserName.Password = oInstallation.INS_FINE_WS_HTTP_PASSWORD;
                }


                oService.InnerChannel.OperationTimeout = new TimeSpan(0, 0, 0, 0, Get3rdPartyWSTimeout());

                if (MadridPlatfomStartSession(oService, out oAuthSession))
                {
                    if (strFineNumber.Length >= 10) strFineNumber = strFineNumber.Substring(0, strFineNumber.Length - 1);

                    MadridPlatform.VigTrafficFineFilter oSelector = new MadridPlatform.VigTrafficFineFilter()
                    {
                        ExpedientNumber = strFineNumber,
                        VigFilter = new MadridPlatform.EntityFilterCity()
                        {
                            CodSystem = oInstallation.INS_PHY_ZONE_COD_SYSTEM,
                            CodGeoZone = oInstallation.INS_PHY_ZONE_COD_GEO_ZONE,
                            CodCity = oInstallation.INS_PHY_ZONE_COD_CITY
                        }
                    };

                    MadridPlatform.EntityFilterCity oFilterCity = oSelector.VigFilter as MadridPlatform.EntityFilterCity;

                    strParamsIn = string.Format("sessionId={4};userName={5};" +
                                               "ExpedientNumber={0};" +
                                               "CodSystem={1};CodGeoZone={2};CodCity={3};",
                                                oSelector.ExpedientNumber,
                                                oFilterCity.CodSystem, oFilterCity.CodGeoZone, oFilterCity.CodCity,
                                                oAuthSession.sessionId, oAuthSession.userName);
                    Logger_AddLogMessage(string.Format("MadridPlatformQueryFinePaymentQuantity.GetTfn parametersIn={0}", strParamsIn), LogLevels.logDEBUG);

                    var oResponse1 = oService.GetTfn(oAuthSession, oSelector);

                    string sAnullationCodes = "";
                    if (oResponse1.Result.Length > 0)
                    {
                        foreach (var sCode in oResponse1.Result[0].AnullationCodes)
                        {
                            sAnullationCodes += "," + sCode;
                        }
                        if (!string.IsNullOrEmpty(sAnullationCodes)) sAnullationCodes = sAnullationCodes.Substring(1);

                        strParamsOut = string.Format("Status={0};errorDetails={1};" +
                                                     "AnulStartDtUTC={2:yyyy-MM-ddTHH:mm:ss.fff};AnullationCodes={3};AnulledDtUTC={4};" +
                                                     "CodPhyZone={5};CreatedUTC={6:yyyy-MM-ddTHH:mm:ss.fff};ExpedientNumber={7};Id={8};" +
                                                     "Infraction.AllowCancel={9};Infraction.AllowCancelMinutes={10};Infraction.Article={11};Infraction.ByLaw={12};Infraction.CancelAmount={13};Infraction.Currency={14};" +
                                                     "InvalidatedDtUTC={15:yyyy-MM-ddTHH:mm:ss.fff};Invalidation={16};LastModificationUTC={17:yyyy-MM-ddTHH:mm:ss.fff};State={18};TicketNumber={19};" +
                                                     "Vehicle.LicensePlate={20};Xml={21}",
                                                     oResponse1.Status.ToString(), oResponse1.errorDetails,
                                                     oResponse1.Result[0].AnulStartDtUTC, sAnullationCodes, oResponse1.Result[0].AnulledDtUTC,
                                                     oResponse1.Result[0].CodPhyZone, oResponse1.Result[0].CreatedUTC, oResponse1.Result[0].ExpedientNumber, oResponse1.Result[0].Id,
                                                     oResponse1.Result[0].Infraction.AllowCancel, oResponse1.Result[0].Infraction.AllowCancelMinutes, oResponse1.Result[0].Infraction.Article, oResponse1.Result[0].Infraction.ByLaw, oResponse1.Result[0].Infraction.CancelAmount, oResponse1.Result[0].Infraction.Currency,
                                                     oResponse1.Result[0].InvalidatedDtUTC, oResponse1.Result[0].Invalidation, oResponse1.Result[0].LastModificationUTC, oResponse1.Result[0].State, oResponse1.Result[0].TicketNumber,
                                                     oResponse1.Result[0].Vehicle.LicensePlate, oResponse1.Result[0].Xml);
                    }
                    else
                    {
                        strParamsOut = string.Format("Status={0};errorDetails={1};" +
                                                     "Result.Length={2}",
                                                     oResponse1.Status.ToString(), oResponse1.errorDetails,
                                                     oResponse1.Result.Length);
                    }
                    Logger_AddLogMessage(string.Format("MadridPlatformQueryFinePaymentQuantity.GetTfn response={0}", strParamsOut), LogLevels.logDEBUG);

                    if (oResponse1.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK)
                    {
                        if (oResponse1.Result.Length > 0 && oResponse1.Result[0].AnullationCodes != null && oResponse1.Result[0].AnullationCodes.Length > 0)
                        {
                            rtRes = ResultType.Result_OK;

                            var oRequest = new MadridPlatform.FineAnullationAuthRequest()
                            {
                                AnullationCode = oResponse1.Result[0].AnullationCodes[0],
                                City = new MadridPlatform.EntityFilterCity()
                                {
                                    CodSystem = oInstallation.INS_PHY_ZONE_COD_SYSTEM,
                                    CodGeoZone = oInstallation.INS_PHY_ZONE_COD_GEO_ZONE,
                                    CodCity = oInstallation.INS_PHY_ZONE_COD_CITY
                                },
                                DtRequest = dtFineQuery,
                                IsoLang = "es"
                            };

                            strParamsIn = string.Format("sessionId={5};userName={6};" +
                                                       "AnullationCode={7};" +
                                                       "CodSystem={0};CodGeoZone={1};CodCity={2};" +
                                                       "DtRequest={3:yyyy-MM-ddTHH:mm:ss.fff};" +
                                                       "IsoLang={4}",
                                                        oRequest.City.CodSystem, oRequest.City.CodGeoZone, oRequest.City.CodCity,
                                                        oRequest.DtRequest,
                                                        oRequest.IsoLang,
                                                        oAuthSession.sessionId, oAuthSession.userName,
                                                        oRequest.AnullationCode);

                            Logger_AddLogMessage(string.Format("MadridPlatformQueryFinePaymentQuantity.GetFineAnullationAuthorization parametersIn={0}", strParamsIn), LogLevels.logDEBUG);

                            var oResponse = oService.GetFineAnullationAuthorization(oAuthSession, oRequest);

                            strParamsOut = string.Format("Status={0};errorDetails={1};AuthId={2};AuthResult={3};Expedient={4};TotAmo={5};eAuthResult={6}",
                                                         oResponse.Status.ToString(), oResponse.errorDetails,
                                                         oResponse.AuthId, oResponse.AuthResult, oResponse.Expedient, oResponse.TotAmo, oResponse.eAuthResult);
                            Logger_AddLogMessage(string.Format("MadridPlatformQueryFinePaymentQuantity response={0}", strParamsOut), LogLevels.logDEBUG);

                            if (oResponse.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK)
                            {
                                rtRes = Convert_MadridPlatformAuthResult_TO_ResultType(oResponse.AuthResult);

                                parametersOut["AuthId"] = oResponse.AuthId.ToString();
                                parametersOut["ExtGrpId"] = oResponse1.Result[0].CodPhyZone;
                                parametersOut["q"] = ((int)(oResponse.TotAmo * 100)).ToString();
                                parametersOut["cur"] = oResponse1.Result[0].Infraction.Currency;
                                parametersOut["exp"] = oResponse.Expedient;
                                parametersOut["d"] = dtFineQuery.ToString("HHmmssddMMyy");
                                if (oResponse1.Result[0].AnulStartDtUTC.HasValue)
                                {                                    
                                    DateTime? dtDf = geograficAndTariffsRepository.ConvertUTCToInstallationDateTime(oInstallation.INS_ID, oResponse1.Result[0].AnulStartDtUTC.Value.AddMinutes(oResponse1.Result[0].Infraction.AllowCancelMinutes ?? 0));
                                    if (dtDf.HasValue) parametersOut["df"] = dtDf.Value;
                                }
                                parametersOut["lp"] = oResponse1.Result[0].Vehicle.LicensePlate.Trim().Replace(" ", "");
                                parametersOut["ta"] = oResponse1.Result[0].Infraction.Article;
                                parametersOut["dta"] = oResponse1.Result[0].Infraction.ByLaw;
                                parametersOut["lit"] = "";
                            }
                            else
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                parametersOut["d"] = dtFineQuery.ToString("HHmmssddMMyy");
                                parametersOut["df"] = "";
                                parametersOut["lp"] = "";
                                parametersOut["ta"] = "";
                                parametersOut["dta"] = "";
                                parametersOut["lit"] = "";
                            }
                        }
                        else
                        {
                            rtRes = ResultType.Result_Error_Fine_Type_Not_Payable;
                            parametersOut["d"] = dtFineQuery.ToString("HHmmssddMMyy");
                            parametersOut["df"] = "";
                            parametersOut["lp"] = "";
                            parametersOut["ta"] = "";
                            parametersOut["dta"] = "";
                            parametersOut["lit"] = "";
                            if (oResponse1.Result.Length > 0)
                            {
                                parametersOut["ExtGrpId"] = oResponse1.Result[0].CodPhyZone;
                                if (oResponse1.Result[0].Infraction != null)
                                {
                                    parametersOut["cur"] = oResponse1.Result[0].Infraction.Currency;
                                    if (oResponse1.Result[0].AnulStartDtUTC.HasValue)
                                    {                                        
                                        DateTime? dtDf = geograficAndTariffsRepository.ConvertUTCToInstallationDateTime(oInstallation.INS_ID, oResponse1.Result[0].AnulStartDtUTC.Value.AddMinutes(oResponse1.Result[0].Infraction.AllowCancelMinutes ?? 0));
                                        if (dtDf.HasValue) parametersOut["df"] = dtDf.Value;
                                    }
                                    parametersOut["ta"] = oResponse1.Result[0].Infraction.Article;
                                    parametersOut["dta"] = oResponse1.Result[0].Infraction.ByLaw;
                                }
                                if (oResponse1.Result[0].Vehicle != null)
                                    parametersOut["lp"] = oResponse1.Result[0].Vehicle.LicensePlate.Trim().Replace(" ", "");
                            }
                        }
                    }
                    else
                    {                        
                        if (oResponse1.Status == MadridPlatform.PublisherResponse.PublisherStatus.ZERO_RESULTS)
                            rtRes = ResultType.Result_Error_Fine_Number_Not_Found;
                        else
                            rtRes = ResultType.Result_Error_Generic;
                        parametersOut["d"] = dtFineQuery.ToString("HHmmssddMMyy"); // ***???
                        parametersOut["df"] = "";
                        parametersOut["lp"] = "";
                        parametersOut["ta"] = "";
                        parametersOut["dta"] = "";
                        parametersOut["lit"] = "";
                    }

                    //MadridPlatfomEndSession(oService, oAuthSession);

                }
                else                
                    rtRes = ResultType.Result_Error_Generic;                

                parametersOut["r"] = Convert.ToInt32(rtRes);

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MadridPlatformQueryFinePaymentQuantity::Exception", LogLevels.logERROR);
            }
            finally
            {
                if (oService != null && oAuthSession != null)
                {
                    MadridPlatfomEndSession(oService, oAuthSession);
                }
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, strParamsIn, strParamsOut, true, oNotificationEx);
            }
            catch
            {
            }

            return rtRes;
        }

        public ResultType MadridPlatformConfirmFinePayment(int iWSNumber, string strFineNumber, DateTime dtFineQuery, DateTime dtUTCInsertionDate,  int iQuantity, USER oUser, INSTALLATION oInstallation, decimal dTicketId, decimal dAuthId, GROUP oGroup,
                                                           ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
        {

            ResultType rtRes = ResultType.Result_OK;
            str3dPartyOpNum = "";
            lEllapsedTime = -1;
            Stopwatch watch = null;


            string strParamsIn = "";
            string strParamsOut = "";
            Exception oNotificationEx = null;

            MadridPlatform.PublishServiceV12Client oService = null;
            MadridPlatform.AuthSession oAuthSession = null;


            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                oService = new MadridPlatform.PublishServiceV12Client();
                // oParkWS.Timeout = Get3rdPartyWSTimeout();

                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        ((sender2, certificate, chain, sslPolicyErrors) => true);
               

             

                switch (iWSNumber)
                {
                    case 1:

                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_FINE_CONFIRM_WS_URL);                
                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_FINE_CONFIRM_WS_HTTP_PASSWORD;
                        }

                        break;

                    case 2:
                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_FINE_CONFIRM_WS2_URL);                
                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_FINE_CONFIRM_WS2HTTP_PASSWORD;
                        }
                        break;

                    case 3:
                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_FINE_CONFIRM_WS3_URL);                
                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_FINE_CONFIRM_WS3HTTP_PASSWORD;
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("MadridPlatformConfirmFinePayment::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                string sExtGrpId = "000";
                if (oGroup != null)
                {
                    sExtGrpId = oGroup.GRP_ID_FOR_EXT_OPS;
                }

                oService.InnerChannel.OperationTimeout = new TimeSpan(0, 0, 0, 0, Get3rdPartyWSTimeout());

                if (MadridPlatfomStartSession(oService, out oAuthSession))
                {
                    if (strFineNumber.Length >= 10) strFineNumber = strFineNumber.Substring(0, strFineNumber.Length - 1);

                    DateTime? dtUTCFineQuery = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtFineQuery);

                    var oRequest = new MadridPlatform.PayFineCancellationTransactionRequest()
                    {                        
                        City = new MadridPlatform.EntityFilterCity()
                        {
                            CodSystem = oInstallation.INS_PHY_ZONE_COD_SYSTEM,
                            CodGeoZone = oInstallation.INS_PHY_ZONE_COD_GEO_ZONE,
                            CodCity = oInstallation.INS_PHY_ZONE_COD_CITY
                        },
                        FineTrans = new MadridPlatform.PayTransactionFineCancellation() {
                            AuthId = Convert.ToInt64(dAuthId),
                            //OperationDateUTC = dtUTCFineQuery.Value,
                            OperationDateUTC = dtUTCInsertionDate,
                            TariffId = 2, 
                            TicketNum = string.Format("91{0}00000{1}{2}", sExtGrpId, dtFineQuery.DayOfYear.ToString("000"), dtFineQuery.ToString("HHmm")),
                            TransId = Convert.ToInt64(dTicketId),
                            FineAmount = iQuantity / 100,
                            FineExpedientNumber = strFineNumber
                        }
                    };

                    strParamsIn = string.Format("sessionId={10};userName={11};" +
                                               "CodSystem={0};CodGeoZone={1};CodCity={2};" +
                                               "AuthId={3};OperationDateUTC={4:yyyy-MM-ddTHH:mm:ss.fff};TariffId={5};TicketNum={6};TransId={7};FineAmount={8};FineExpedientNumber={9}",
                                                oRequest.City.CodSystem, oRequest.City.CodGeoZone, oRequest.City.CodCity,
                                                oRequest.FineTrans.AuthId, oRequest.FineTrans.OperationDateUTC, oRequest.FineTrans.TariffId, oRequest.FineTrans.TicketNum, oRequest.FineTrans.TransId,
                                                oRequest.FineTrans.FineAmount, oRequest.FineTrans.FineExpedientNumber,
                                                oAuthSession.sessionId, oAuthSession.userName);

                    Logger_AddLogMessage(string.Format("MadridPlatformConfirmFinePayment parametersIn={0}", strParamsIn), LogLevels.logDEBUG);

                    watch = Stopwatch.StartNew();

                    var oResponse = oService.SetFineAnullationTransaction(oAuthSession, oRequest);

                    lEllapsedTime = watch.ElapsedMilliseconds;

                    strParamsOut = string.Format("Status={0};errorDetails={1}",
                                                 oResponse.Status.ToString(), oResponse.errorDetails);
                    Logger_AddLogMessage(string.Format("MadridPlatformConfirmFinePayment response={0}", strParamsOut), LogLevels.logDEBUG);

                    if (oResponse.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK)
                    {
                        rtRes = ResultType.Result_OK;

                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_Generic;
                    }

                    //MadridPlatfomEndSession(oService, oAuthSession);

                }
                else
                {
                    rtRes = ResultType.Result_Error_Generic;                    
                    if (parametersOut.IndexOfKey("autorecharged") >= 0)
                    {
                        parametersOut.RemoveAt(parametersOut.IndexOfKey("autorecharged"));
                    }
                    if (parametersOut.IndexOfKey("newbal") >= 0)
                    {
                        parametersOut.RemoveAt(parametersOut.IndexOfKey("newbal"));
                    }
                }

                parametersOut["r"] = Convert.ToInt32(rtRes);

                if (rtRes == ResultType.Result_OK)
                {                    
                    str3dPartyOpNum = dAuthId.ToString();
                }


            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MadridPlatformConfirmFinePayment::Exception", LogLevels.logERROR);
            }
            finally
            {
                if (oService != null && oAuthSession != null)
                {
                    MadridPlatfomEndSession(oService, oAuthSession);
                }
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, strParamsIn, strParamsOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;

        }



        public ResultType PICBilbaoConfirmFinePayment(int iWSNumber, string strFineNumber, DateTime dtOperationDate, int iQuantity, decimal dTicketPaymentId,
                                                      string strPlate, string strFineType, USER oUser, INSTALLATION oInstallation,
                                                      ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
        {

            ResultType rtRes = ResultType.Result_OK;
            str3dPartyOpNum = "";
            lEllapsedTime = -1;
            Stopwatch watch = null;


            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                MeyparPICWS.PIC_WS oFineWS = new MeyparPICWS.PIC_WS();               

                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS_AUTH_HASH_KEY;
                        break;

                    case 2:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS2_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS2_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS2HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS2_AUTH_HASH_KEY;
                        break;

                    case 3:
                        oFineWS.Url = oInstallation.INS_FINE_CONFIRM_WS3_URL;
                        oFineWS.Timeout = Get3rdPartyWSTimeout();

                        if (!string.IsNullOrEmpty(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER))
                        {
                            oFineWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_FINE_CONFIRM_WS3_HTTP_USER, oInstallation.INS_FINE_CONFIRM_WS3HTTP_PASSWORD);
                        }
                        strHashKey = oInstallation.INS_FINE_CONFIRM_WS3_AUTH_HASH_KEY;
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("PICBilbaoConfirmFinePayment::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }


             
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();
                string strFineID = strFineNumber;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;

                string strAuthHash = CalculateStandardWSHash(strHashKey,
                    string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}{9}", strFineID, strCityID, dtOperationDate, iQuantity, strPlate, dTicketPaymentId, strFineType, oUser.USR_EMAIL, strCompanyName, strvers));

                string strMessage = string.Format("<ipark_in><ticket_num>{0}</ticket_num><ins_id>{1}</ins_id><date>{2:HHmmssddMMyyyy}</date><amou_payed>{3}</amou_payed><lic_pla>{4}</lic_pla>"+
                                                  "<term_id></term_id><oper_id>{5}</oper_id><ticket_type>{6}</ticket_type><ext_acc>{7}</ext_acc><prov>{8}</prov><vers>{9}</vers><ah>{10}</ah></ipark_in>",
                    strFineID, strCityID, dtOperationDate, iQuantity, strPlate, dTicketPaymentId, strFineType,oUser.USR_EMAIL, strCompanyName, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("PICBilbaoConfirmFinePayment xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oFineWS.ExternalTicketPaymentParkingmeter(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("PICBilbaoConfirmFinePayment xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypePICBilbaoFineWS_TO_ResultType((ResultTypePICBilbao)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        if (wsParameters["oper_id"] != null)
                        {
                            str3dPartyOpNum = wsParameters["oper_id"].ToString();
                        }
                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        if (parametersOut.IndexOfKey("autorecharged") >= 0)
                        {
                            parametersOut.RemoveAt(parametersOut.IndexOfKey("autorecharged"));
                        }
                        if (parametersOut.IndexOfKey("newbal") >= 0)
                        {
                            parametersOut.RemoveAt(parametersOut.IndexOfKey("newbal"));
                        }

                    }
                }

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);
                Logger_AddLogException(e, "PICBilbaoConfirmFinePayment::Exception", LogLevels.logERROR);
            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            {

            }

            return rtRes;

        }


    }
}
