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
using System.Net;
using System.Globalization;
using integraMobile.Domain;
using integraMobile.Domain.Abstract;
using integraMobile.Infrastructure;
using integraMobile.Infrastructure.Logging.Tools;
using Ninject;
using Newtonsoft.Json;



namespace integraMobile.ExternalWS
{
    public class ThirdPartyBase
    {
        private const long BIG_PRIME_NUMBER = 2147483647;
        protected static string _xmlTagName = "ipark";
        protected const string OUT_SUFIX = "_out";
        protected const int DEFAULT_WS_TIMEOUT = 5000; //ms        

        private IKernel m_kernel = null;

        [Inject]
        public ICustomersRepository customersRepository { get; set; }
        [Inject]
        public IInfraestructureRepository infraestructureRepository { get; set; }
        [Inject]
        public IGeograficAndTariffsRepository geograficAndTariffsRepository { get; set; }
        [Inject]
        public IRetailerRepository retailerRepository { get; set; }
        
        //Log4net Wrapper class
        protected static CLogWrapper m_Log; // = new CLogWrapper(typeof(ThirdPartyOperation));

        protected Notifications m_notifications;

        protected static MadridPlatform.AuthSession m_oMadridPlatformAuthSession = null;
        protected static int m_iMadridPlatformOpenSessions = 0;
        protected static readonly object m_oLocker = new object();

        // Pagatelia 
        protected static string _hMacKey = null;
        protected static byte[] _normKey = null;
        protected static HMACSHA256 _hmacsha256 = null;

        public ThirdPartyBase()
        {
            m_kernel = new StandardKernel(new integraMobileThirdPartyConfirmModule());
            m_kernel.Inject(this);

            m_notifications = NotificationsFactory.CreateNotifications();
        }

        protected int Get3rdPartyWSTimeout()
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

        protected string CalculateGtechnaWSHash(string strMACKey, string strInput)
        {
            string strRes = "";
            int iKeyLength = 64;
            byte[] normMACKey = null;
            HMACSHA256 oMACsha256 = null;

            try
            {

                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(strMACKey);
                normMACKey = new byte[iKeyLength];
                int iSum = 0;

                for (int i = 0; i < iKeyLength; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    normMACKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));

                }

                oMACsha256 = new HMACSHA256(normMACKey);


                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                byte[] hash = oMACsha256.ComputeHash(inputBytes); ;

                if (hash.Length >= 8)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = hash.Length - 8; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    strRes = sb.ToString();
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "CalculateWSHash::Exception", LogLevels.logERROR);
            }

            return strRes;
        }


        protected string CalculateEysaWSHash(string strMACKey, string strInput)
        {
            string strRes = "";
            int iKeyLength = 64;
            byte[] normMACKey = null;
            HMACSHA256 oMACsha256 = null;

            try
            {

                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(strMACKey);
                normMACKey = new byte[iKeyLength];
                int iSum = 0;

                for (int i = 0; i < iKeyLength; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    normMACKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));

                }

                oMACsha256 = new HMACSHA256(normMACKey);


                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                byte[] hash = oMACsha256.ComputeHash(inputBytes); ;

                if (hash.Length >= 8)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = hash.Length - 8; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    strRes = sb.ToString();
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "CalculateEysaWSHash::Exception", LogLevels.logERROR);
            }

            return strRes;
        }


        protected string CalculateStandardWSHash(string strMACKey, string strInput)
        {
            string strRes = "";
            int iKeyLength = 64;
            byte[] normMACKey = null;
            HMACSHA256 oMACsha256 = null;

            try
            {

                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(strMACKey);
                normMACKey = new byte[iKeyLength];
                int iSum = 0;

                for (int i = 0; i < iKeyLength; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    normMACKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));

                }

                oMACsha256 = new HMACSHA256(normMACKey);


                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                byte[] hash = oMACsha256.ComputeHash(inputBytes); ;

                if (hash.Length >= 8)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = hash.Length - 8; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    strRes = sb.ToString();
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "CalculateStandardWSHash::Exception", LogLevels.logERROR);
            }

            return strRes;
        }

        private static void PagateliaInitializeStatic()
        {

            int iKeyLength = 64;

            if (_hMacKey == null)
            {
                _hMacKey = ConfigurationManager.AppSettings["PagateliaWsAuthHashKey"].ToString();
            }



            if (_normKey == null)
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(_hMacKey);
                _normKey = new byte[iKeyLength];
                int iSum = 0;

                for (int i = 0; i < iKeyLength; i++)
                {
                    if (i < keyBytes.Length)
                    {
                        iSum += keyBytes[i];
                    }
                    else
                    {
                        iSum += i;
                    }
                    _normKey[i] = Convert.ToByte((iSum * BIG_PRIME_NUMBER) % (Byte.MaxValue + 1));

                }
            }

            if (_hmacsha256 == null)
            {
                _hmacsha256 = new HMACSHA256(_normKey);
            }

        }



        public string CalculatePagateliaWsHash(string strInput)
        {
            string strRes = "";
            try
            {
                PagateliaInitializeStatic();

                if (_hmacsha256 != null)
                {
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(strInput);
                    byte[] hash = _hmacsha256.ComputeHash(inputBytes);

                    if (hash.Length >= 8)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = hash.Length - 8; i < hash.Length; i++)
                        {
                            sb.Append(hash[i].ToString("X2"));
                        }
                        strRes = sb.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "CalculatePagateliaWsHash::Exception", LogLevels.logERROR);
            }


            return strRes;
        }

        protected ResultType FindOutParameters(string xmlIn, out SortedList parameters)
        {
            ResultType rtRes = ResultType.Result_OK;
            parameters = new SortedList();


            try
            {
                XmlDocument xmlInDoc = new XmlDocument();
                try
                {
                    if (xmlIn.StartsWith("<?xml"))
                    {
                        xmlInDoc.LoadXml(xmlIn);
                    }
                    else
                    {
                        xmlInDoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xmlIn);
                    }

                    XmlNodeList Nodes = xmlInDoc.SelectNodes("//" + _xmlTagName + OUT_SUFIX + "/*");
                    foreach (XmlNode Node in Nodes)
                    {
                        switch (Node.Name)
                        {
                            default:

                                if (Node.HasChildNodes)
                                {
                                    if (Node.ChildNodes[0].HasChildNodes)
                                    {
                                        int i = 0;
                                        foreach (XmlNode ChildNode in Node.ChildNodes)
                                        {
                                            if (!ChildNode.ChildNodes[0].HasChildNodes)
                                            {
                                                if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString()] == null)
                                                {
                                                    parameters[Node.Name + "_" + ChildNode.Name] = ChildNode.InnerText.Trim();
                                                }
                                                else
                                                {
                                                    parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString()] = ChildNode.InnerText.Trim();
                                                }
                                            }
                                            else
                                            {
                                                int j = 0;
                                                foreach (XmlNode ChildNode2 in ChildNode.ChildNodes)
                                                {
                                                    if (!ChildNode2.HasChildNodes)
                                                    {
                                                        if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name] == null)
                                                        {
                                                            parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name] = ChildNode2.InnerText.Trim();
                                                        }
                                                        else
                                                        {
                                                            parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString()] = ChildNode2.InnerText.Trim();
                                                        }
                                                    }
                                                    else
                                                    {

                                                        if (!ChildNode2.ChildNodes[0].HasChildNodes)
                                                        {


                                                            if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name] == null)
                                                            {
                                                                parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name] = ChildNode2.InnerText.Trim();
                                                            }
                                                            else
                                                            {
                                                                parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString()] = ChildNode2.InnerText.Trim();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            int k = 0;
                                                            foreach (XmlNode ChildNode3 in ChildNode2.ChildNodes)
                                                            {
                                                                if (!ChildNode3.HasChildNodes)
                                                                {
                                                                    if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name] == null)
                                                                    {
                                                                        parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name] = ChildNode3.InnerText.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name + "_" + k.ToString()] = ChildNode3.InnerText.Trim();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (!ChildNode3.ChildNodes[0].HasChildNodes)
                                                                    {

                                                                        if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name] == null)
                                                                        {
                                                                            parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name] = ChildNode3.InnerText.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + j.ToString() + "_" + ChildNode3.Name + "_" + k.ToString()] = ChildNode3.InnerText.Trim();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        int l = 0;
                                                                        foreach (XmlNode ChildNode4 in ChildNode3.ChildNodes)
                                                                        {

                                                                            if (parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + ChildNode3.Name + "_" + k.ToString() + "_" + ChildNode4.Name] == null)
                                                                            {
                                                                                parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + ChildNode3.Name + "_" + k.ToString() + "_" + ChildNode4.Name] = ChildNode4.InnerText.Trim();
                                                                            }
                                                                            else
                                                                            {
                                                                                parameters[Node.Name + "_" + ChildNode.Name + "_" + i.ToString() + "_" + ChildNode2.Name + "_" + ChildNode3.Name + "_" + k.ToString() + "_" + ChildNode4.Name + "_" + l.ToString()] = ChildNode4.InnerText.Trim();
                                                                            }

                                                                        }

                                                                    }
                                                                    k++;
                                                                }
                                                            }

                                                        }
                                                        j++;
                                                    }
                                                }
                                            }
                                            i++;
                                            parameters[Node.Name + "_" + ChildNode.Name + "_num"] = i;
                                        }
                                    }
                                    else
                                    {
                                        parameters[Node.Name] = Node.InnerText.Trim();
                                    }
                                }
                                else
                                {
                                    parameters[Node.Name] = null;
                                }

                                break;

                        }

                    }

                    if (Nodes.Count == 0)
                    {
                        Logger_AddLogMessage(string.Format("FindParameters: Bad Input XML: xmlIn={0}", PrettyXml(xmlIn)), LogLevels.logERROR);
                        rtRes = ResultType.Result_Error_Generic;

                    }


                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, string.Format("FindInputParameters: Bad Input XML: xmlIn={0}:Exception", PrettyXml(xmlIn)), LogLevels.logERROR);
                    rtRes = ResultType.Result_Error_Generic;
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "FindInputParameters::Exception", LogLevels.logERROR);

            }


            return rtRes;
        }

        protected ResultType Convert_ResultTypeStandardParkingWS_TO_ResultType(ResultTypeStandardParkingWS oExtResultType)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;

            switch (oExtResultType)
            {
                case ResultTypeStandardParkingWS.ResultSP_OK:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_InvalidAuthenticationHash:
                    rtResultType = ResultType.Result_Error_InvalidAuthenticationHash;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_ParkingMaximumTimeUsed:
                    rtResultType = ResultType.Result_Error_ParkingMaximumTimeUsed;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_NotWaitedReentryTime:
                    rtResultType = ResultType.Result_Error_NotWaitedReentryTime;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_RefundNotPossible:
                    rtResultType = ResultType.Result_Error_RefundNotPossible;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Fine_Number_Not_Found:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Fine_Type_Not_Payable:
                    rtResultType = ResultType.Result_Error_Fine_Type_Not_Payable;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Fine_Payment_Period_Expired:
                    rtResultType = ResultType.Result_Error_Fine_Payment_Period_Expired;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Fine_Number_Already_Paid:
                    rtResultType = ResultType.Result_Error_Fine_Number_Already_Paid;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Generic:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Invalid_Input_Parameter:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Missing_Input_Parameter:
                    rtResultType = ResultType.Result_Error_Missing_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Invalid_City:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Invalid_Group:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Invalid_Tariff:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_Tariff_Not_Available:
                    rtResultType = ResultType.Result_Error_Tariffs_Not_Available;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_InvalidExternalProvider:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_OperationAlreadyExist:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypeStandardParkingWS.ResultSP_Error_CrossSourceExtensionNotPossible:
                    rtResultType = ResultType.Result_Error_CrossSourceExtensionNotPossible;
                    break;
                default:
                    break;
            }


            return rtResultType;
        }


     
        protected ResultType Convert_ResultTypeStandardFineWS_TO_ResultType(ResultTypeStandardFineWS oExtResultType)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;

            switch (oExtResultType)
            {
                case ResultTypeStandardFineWS.Ok:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypeStandardFineWS.InvalidAuthenticationHashExternalService:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.InvalidAuthenticationExternalService:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.InvalidAuthentication:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.TicketNotFound:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;                    
                case ResultTypeStandardFineWS.TicketNumberNotFound:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;
                case ResultTypeStandardFineWS.TicketTypeNotPayable:
                    rtResultType = ResultType.Result_Error_Fine_Type_Not_Payable;
                    break;
                case ResultTypeStandardFineWS.TicketPaymentPeriodExpired:
                    rtResultType = ResultType.Result_Error_Fine_Payment_Period_Expired;
                    break;
                case ResultTypeStandardFineWS.TicketAlreadyCancelled:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;
                case ResultTypeStandardFineWS.TicketAlreadyAnulled:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;
                case ResultTypeStandardFineWS.TicketAlreadyRemitted:
                    rtResultType = ResultType.Result_Error_Fine_Payment_Period_Expired;
                    break;
                case ResultTypeStandardFineWS.TicketAlreadyPaid:
                    rtResultType = ResultType.Result_Error_Fine_Number_Already_Paid;
                    break;
                case ResultTypeStandardFineWS.TicketNotClosed:
                    rtResultType = ResultType.Result_Error_Fine_Number_Not_Found;
                    break;
                case ResultTypeStandardFineWS.InvalidPaymentAmount:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;                
                case ResultTypeStandardFineWS.InstallationNotFound:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.InvalidExternalProvider:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;                                               
                case ResultTypeStandardFineWS.InvalidAuthenticationHash:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.ErrorDetected:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeStandardFineWS.GenericError:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                default:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
            }


            return rtResultType;
        }


        protected ResultType Convert_ResultTypePICBilbaoFineWS_TO_ResultType(ResultTypePICBilbao oExtResultType)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;


            switch (oExtResultType)
            {
                case ResultTypePICBilbao.Result_OK:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypePICBilbao.Result_Error_InvalidAuthenticationHash:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_Generic:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_Invalid_Input_Parameter:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_Invalid_City:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_InvalidExternalProvider:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_Invalid_Unit:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypePICBilbao.Result_Error_TicketPaymentAlreadyExist:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypePICBilbao.Result_Error_TicketNotPayable:
                    rtResultType = ResultType.Result_Error_Fine_Type_Not_Payable;
                    break;               
                default:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
            }


            return rtResultType;
        }



        protected ResultType Convert_ResultTypeMeyparOffstreetWS_TO_ResultType(ResultTypeMeyparOffstreetWS oExtResultType)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;

            switch (oExtResultType)
            {
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_OK:
                    rtResultType = ResultType.Result_OK;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Generic:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Invalid_Id:
                    rtResultType = ResultType.Result_Error_Invalid_Id;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Invalid_Input_Parameter:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Missing_Input_Parameter:
                    rtResultType = ResultType.Result_Error_Missing_Input_Parameter;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_OperationNotFound:
                    rtResultType = ResultType.Result_Error_OperationNotFound;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_OperationAlreadyClosed:
                    rtResultType = ResultType.Result_Error_OperationAlreadyClosed;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Max_Multidiscount_Reached:
                    rtResultType = ResultType.Result_Error_Max_Multidiscount_Reached;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_Discount_NotAllowed:
                    rtResultType = ResultType.Result_Error_Discount_NotAllowed;
                    break;
                case ResultTypeMeyparOffstreetWS.ResultMOffstreet_Error_InvoiceGeneration:
                    rtResultType = ResultType.Result_Error_Offstreet_InvoiceGeneration;
                    break;
                default:
                    break;
            }


            return rtResultType;
        }

        protected ResultType Convert_MadridPlatformAuthResult_TO_ResultType(int iAuthResult)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;

            switch (iAuthResult)
            {
                case 0: rtResultType = ResultType.Result_OK; break;                                     // Autoricado
                case 1: rtResultType = ResultType.Result_Error_Fine_Payment_Period_Expired; break;      // Fuera de periodo
                case 2: rtResultType = ResultType.Result_Error_Fine_Type_Not_Payable; break;            // No anulable
                case 3: rtResultType = ResultType.Result_Error_Fine_Number_Not_Found; break;            // No existe
                case 4: rtResultType = ResultType.Result_Error_Fine_Number_Already_Paid; break;         // Ya abonada
                case 5: rtResultType = ResultType.Result_Error_Fine_Number_Not_Found; break;            // Invalidada
                case 6: rtResultType = ResultType.Result_Error_Generic; break;                          // Error servidor
                case 99: rtResultType = ResultType.Result_Error_Generic; break;                         // Excepción servidor

            }

            return rtResultType;
        }


        protected ResultType Convert_ResultTypePagateliaWS_TO_ResultType(short iExtResultType)
        {
            ResultType rtResultType = ResultType.Result_Error_Generic;

            switch (iExtResultType)
            {
                case 1:
                    rtResultType = ResultType.Result_OK;
                    break;
                case -9:
                    rtResultType = ResultType.Result_Error_Generic;
                    break;
                case -19:
                    rtResultType = ResultType.Result_Error_Invalid_Input_Parameter;
                    break;
                case -20:
                    rtResultType = ResultType.Result_Error_Missing_Input_Parameter;
                    break;
                case -26:
                    rtResultType = ResultType.Result_Error_Invalid_User;
                    break;
                case -27:
                    rtResultType = ResultType.Result_Error_User_Not_Logged;
                    break;
                case -29:
                    rtResultType = ResultType.Result_Error_Invalid_Payment_Mean;
                    break;
                case -30:
                    rtResultType = ResultType.Result_Error_Invalid_Recharge_Code;
                    break;
                case -40:
                    rtResultType = ResultType.Result_Error_UserAccountBlocked;
                    break;
                case -41:
                    rtResultType = ResultType.Result_Error_UserAccountNotAproved;
                    break;
                case -50:
                    rtResultType = ResultType.Result_Error_UserBalanceNotEnough;
                    break;
                case -51:
                    rtResultType = ResultType.Result_Error_UserAmountDailyLimitReached;
                    break;
                case -60:
                    rtResultType = ResultType.Result_Error_AmountNotValid;
                    break;
                default:
                    break;
            }


            return rtResultType;
        }

        protected int ChangeQuantityFromInstallationCurToUserCur(int iQuantity, INSTALLATION oInstallation, USER oUser, out double dChangeApplied, out double dChangeFee)
        {
            int iResult = iQuantity;
            dChangeApplied = 1;
            dChangeFee = 0;


            try
            {

                if (oInstallation.INS_CUR_ID != oUser.USR_CUR_ID)
                {
                    double dConvertedValue = CCurrencyConvertor.ConvertCurrency(Convert.ToDouble(iQuantity),
                                              oInstallation.CURRENCy.CUR_ISO_CODE,
                                              oUser.CURRENCy.CUR_ISO_CODE, out dChangeApplied);
                    if (dConvertedValue < 0)
                    {
                        Logger_AddLogMessage(string.Format("ChangeQuantityFromInstallationCurToUserCur::Error Converting {0} {1} to {2} ", iQuantity, oInstallation.CURRENCy.CUR_ISO_CODE, oUser.CURRENCy.CUR_ISO_CODE), LogLevels.logERROR);
                        return ((int)ResultType.Result_Error_Generic);
                    }

                    dChangeFee = Convert.ToDouble(infraestructureRepository.GetChangeFeePerc()) * dConvertedValue / 100;
                    iResult = Convert.ToInt32(dConvertedValue - dChangeFee + 0.5);
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "ChangeQuantityFromInstallationCurToUserCur::Exception", LogLevels.logERROR);
            }

            return iResult;
        }

        protected int ChangeQuantityFromInstallationCurToUserCur(int iQuantity, INSTALLATION oInstallation, USER oUser)
        {
            int iResult = iQuantity;
            double dChangeApplied = 1;
            double dChangeFee = 0;


            try
            {

                iResult = ChangeQuantityFromInstallationCurToUserCur(iQuantity, oInstallation, oUser, out dChangeApplied, out dChangeFee);


            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "ChangeQuantityFromInstallationCurToUserCur::Exception", LogLevels.logERROR);
            }

            return iResult;
        }




        protected int ChangeQuantityFromInstallationCurToUserCur(int iQuantity, double dChangeToApply, INSTALLATION oInstallation, USER oUser, out double dChangeFee)
        {
            int iResult = iQuantity;
            dChangeFee = 0;

            try
            {

                if (oInstallation.INS_CUR_ID != oUser.USR_CUR_ID)
                {
                    double dConvertedValue = Convert.ToDouble(iQuantity) * dChangeToApply;
                    dConvertedValue = Math.Round(dConvertedValue, 4);

                    dChangeFee = Convert.ToDouble(infraestructureRepository.GetChangeFeePerc()) * dConvertedValue / 100;
                    iResult = Convert.ToInt32(Math.Round(dConvertedValue - dChangeFee, MidpointRounding.AwayFromZero));
                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e, "ChangeQuantityFromInstallationCurToUserCur::Exception", LogLevels.logERROR);
            }

            return iResult;
        }


        protected double GetChangeToApplyFromInstallationCurToUserCur(INSTALLATION oInstallation, USER oUser)
        {
            double dResult = 1.0;


            try
            {

                if (oInstallation.INS_CUR_ID != oUser.USR_CUR_ID)
                {
                    dResult = CCurrencyConvertor.GetChangeToApply(oInstallation.CURRENCy.CUR_ISO_CODE,
                                              oUser.CURRENCy.CUR_ISO_CODE);
                    if (dResult < 0)
                    {
                        Logger_AddLogMessage(string.Format("GetChangeToApplyFromInstallationCurToUserCur::Error getting change from {0} to {1} ", oInstallation.CURRENCy.CUR_ISO_CODE, oUser.CURRENCy.CUR_ISO_CODE), LogLevels.logERROR);
                        return ((int)ResultType.Result_Error_Generic);
                    }
                }

            }
            catch (Exception e)
            {
                dResult = -1.0;
                Logger_AddLogException(e, "GetChangeToApplyFromInstallationCurToUserCur::Exception", LogLevels.logERROR);
            }

            return dResult;
        }

        protected string PrettyXml(string xml)
        {

            try
            {
                var stringBuilder = new StringBuilder();

                var element = XElement.Parse(xml);

                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }

                return "\r\n\t" + stringBuilder.ToString().Replace("\r\n", "\r\n\t") + "\r\n";
            }
            catch
            {
                return "\r\n\t" + xml + "\r\n";
            }
        }

        protected bool MadridPlatfomStartSession(MadridPlatform.PublishServiceV12Client oService, out MadridPlatform.AuthSession oAuthSession)
        {
            bool bRet = false;
            oAuthSession = null;

            try
            {
                lock (m_oLocker)
                {
                    if (m_oMadridPlatformAuthSession == null)
                    {
                        Logger_AddLogMessage("MadridPlatfomStartSession - Starting session ... ", LogLevels.logDEBUG);
                        MadridPlatform.AuthLoginResponse oResponse = null;
                        int iRetry = 0;
                        while ((oResponse == null || oResponse.Status != MadridPlatform.PublisherResponse.PublisherStatus.OK) && iRetry < 3)
                        {
                            if (iRetry > 0) Logger_AddLogMessage(string.Format("MadridPlatfomStartSession - Retrying start session {0} ...", iRetry), LogLevels.logWARN);
                            oResponse = oService.startSession(oService.ClientCredentials.UserName.UserName, oService.ClientCredentials.UserName.Password, "es");
                            iRetry += 1;
                        }
                        if (oResponse.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK)
                        {
                            bRet = true;
                            m_oMadridPlatformAuthSession = oResponse.Result;
                            oAuthSession = oResponse.Result;
                            m_iMadridPlatformOpenSessions += 1;
                            Logger_AddLogMessage(string.Format("MadridPlatfomStartSession - Session started successfully: Status='{0}', sessionId='{1}', userName='{2}'", oResponse.Status.ToString(), m_oMadridPlatformAuthSession.sessionId, m_oMadridPlatformAuthSession.userName), LogLevels.logDEBUG);
                        }
                        else
                        {
                            Logger_AddLogMessage(string.Format("MadridPlatfomStartSession - Error starting session: Status='{0}', errorDetails='{1}'", oResponse.Status.ToString(), oResponse.errorDetails), LogLevels.logERROR);
                        }
                    }
                    else
                    {
                        bRet = true;
                        oAuthSession = m_oMadridPlatformAuthSession;
                        m_iMadridPlatformOpenSessions += 1;
                        Logger_AddLogMessage(string.Format("MadridPlatfomStartSession - Session reused: sessionId='{0}', userName='{1}'", m_oMadridPlatformAuthSession.sessionId, m_oMadridPlatformAuthSession.userName), LogLevels.logDEBUG);
                    }
                    Logger_AddLogMessage(string.Format("MadridPlatfomStartSession - Sessions count: {0}", m_iMadridPlatformOpenSessions), LogLevels.logDEBUG);
                }
            }
            catch (Exception ex)
            {
                Logger_AddLogException(ex, "MadridPlatfomStartSession::Exception", LogLevels.logERROR);
            }

            return bRet;
        }

        protected bool MadridPlatfomEndSession(MadridPlatform.PublishServiceV12Client oService, MadridPlatform.AuthSession oAuthSession)
        {
            bool bRet = false;
          
            lock (m_oLocker)
            {
                bool bClosing = false;          
                try
                {
                    if (m_iMadridPlatformOpenSessions <= 1)
                    {
                        Logger_AddLogMessage("MadridPlatfomEndSession - Ending session ... ", LogLevels.logDEBUG);
                        m_iMadridPlatformOpenSessions = 0;
                        bClosing = true;
                        var oResponse = oService.endSession(oAuthSession);
                        if (oResponse.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK)
                        {
                            Logger_AddLogMessage("MadridPlatfomEndSession - Session ended successfully.", LogLevels.logDEBUG);
                        }
                        else
                        {
                            Logger_AddLogMessage(string.Format("MadridPlatfomEndSession - Error ending session: Status='{0}', errorDetails='{1}'", oResponse.Status.ToString(), oResponse.errorDetails), LogLevels.logERROR);
                        }
                    }
                    else
                    {
                        m_iMadridPlatformOpenSessions -= 1;
                        bRet = true;
                    }
                    Logger_AddLogMessage(string.Format("MadridPlatfomEndSession - Sessions count: {0}", m_iMadridPlatformOpenSessions), LogLevels.logDEBUG);

                }
                catch (Exception ex)
                {
                    Logger_AddLogException(ex, "MadridPlatfomEndSession::Exception", LogLevels.logERROR);
                }
                finally
                {
                    if (bClosing)
                    {
                        bRet = true;
                        m_oMadridPlatformAuthSession = null;
                    }
                }
            }
            return bRet;
        }


        protected static string PrettyJSON(string json)
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

        public static void AddTLS12Support()
        {
            /*if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Tls12) == 0) //Enable TLs 1.2
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Ssl3) != 0) //Disable SSL3
            {
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            }*/
            ServicePointManager.SecurityProtocol = (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
        }

        public static void AddTLS11Support()
        {
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Tls11) == 0) //Enable TLs 1.1
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
            }
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Tls) == 0) //Enable TLs 1.0
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls;
            }
            if (((int)ServicePointManager.SecurityProtocol & (int)SecurityProtocolType.Ssl3) != 0) //Disable SSL3
            {
                ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            }
        }

        protected static void Logger_AddLogMessage(string msg, LogLevels nLevel)
        {
            m_Log.LogMessage(nLevel, msg);
        }


        protected static void Logger_AddLogException(Exception ex, string msg, LogLevels nLevel)
        {
            m_Log.LogMessage(nLevel, msg, ex);
        }

    }
}
