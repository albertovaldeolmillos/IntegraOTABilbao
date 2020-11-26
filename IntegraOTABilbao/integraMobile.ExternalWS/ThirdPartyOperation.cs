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
using integraMobile.Domain;
using integraMobile.Domain.Abstract;
using integraMobile.Infrastructure;
using integraMobile.Infrastructure.Logging.Tools;
using Ninject;

namespace integraMobile.ExternalWS
{
    public enum ResultType
    {
        Result_OK = 1,
        Result_Error_InvalidAuthenticationHash = -1,
        Result_Error_ParkingMaximumTimeUsed = -2,
        Result_Error_NotWaitedReentryTime = -3,
        Result_Error_RefundNotPossible = -4,
        Result_Error_Fine_Number_Not_Found = -5,
        Result_Error_Fine_Type_Not_Payable = -6,
        Result_Error_Fine_Payment_Period_Expired = -7,
        Result_Error_Fine_Number_Already_Paid = -8,
        Result_Error_Generic = -9,
        Result_Error_InvalidAuthentication = -11,
        Result_Error_LoginMaximumNumberOfTrialsReached = -12,
        Result_Error_Invalid_First_Name = -13,
        Result_Error_Invalid_Last_Name = -14,
        Result_Error_Invalid_Id = -15,
        Result_Error_Invalid_Country_Code = -16,
        Result_Error_Invalid_Cell_Number = -17,
        Result_Error_Invalid_Email_Number = -18,
        Result_Error_Invalid_Input_Parameter = -19,
        Result_Error_Missing_Input_Parameter = -20,
        Result_Error_Mobile_Phone_Already_Exist = -21,
        Result_Error_Email_Already_Exist = -22,
        Result_Error_Recharge_Failed = -23,
        Result_Error_Recharge_Not_Possible = -24,
        Result_Error_Invalid_City = -25,
        Result_Error_Invalid_User = -26,
        Result_Error_User_Not_Logged = -27,
        Result_Error_Tariffs_Not_Available = -28,
        Result_Error_Invalid_Payment_Mean = -29,
        Result_Error_Invalid_Recharge_Code = -30,
        Result_Error_Expired_Recharge_Code = -31,
        Result_Error_AlreadyUsed_Recharge_Code = -32,
        Result_Error_Not_Enough_Balance = -33,
        Result_Error_ResidentParkingExhausted = -34,
        Result_Error_OperationExpired = -35,
        Result_Error_InvalidTicketId = -36,
        Result_Error_ExpiredTicketId = -37,
        Result_Error_OperationNotFound = -38,
        Result_Error_OperationAlreadyClosed = -39,
        Result_Error_OperationEntryAlreadyExists = -40,
        Result_Error_ConfirmOperationAlreadyExecuting = -41,
        Result_Error_InvalidAppVersion_UpdateMandatory = -42,
        Result_Error_InvalidAppVersion_UpdateNotMandatory = -43,
        Result_Error_Madrid_Council_Platform_Is_Not_Available = -44,
        Result_Error_TransferingBalance = 0,
        Result_Error_InvalidUserReceiverEmail = -45,
        Result_Error_UserReceiverDisabled = -46,
        Result_Error_UserAccountBlocked = -47,
        Result_Error_UserAccountNotAproved = -48,
        Result_Error_UserBalanceNotEnough = -49,
        Result_Error_AmountNotValid = -50,
        Result_Error_UserAmountDailyLimitReached = -51,
        Result_Toll_is_Not_from_That_installation = -52,
        Result_Parking_Not_Allowed= -53,
        Result_Error_Max_Multidiscount_Reached = -54,
        Result_Error_Discount_NotAllowed = -55,
        Result_Error_Invalid_Plate = -56,
        Result_Error_Offstreet_InvoiceGeneration = -57,
        Result_Error_Getting_Transaction_Parameters = -58,
        Result_Error_Duplicate_Recharge = -59,
        Result_Error_Offstreet_OperationInFreePass = -60,
        Result_Error_Invalid_Payment_Gateway = -61,
        Result_Error_User_Has_no_Suscription_Type = -62,
        Result_Error_User_Is_Not_Activated = -63,
        Result_Error_Plate_Is_Assigned_To_Another_User = -64,
        Result_Error_CrossSourceExtensionNotPossible = -65
    }

    public enum ResultTypeStandardParkingWS
    {
        ResultSP_OK = 1,
        ResultSP_Error_InvalidAuthenticationHash = -1,
        ResultSP_Error_ParkingMaximumTimeUsed = -2,
        ResultSP_Error_NotWaitedReentryTime = -3,
        ResultSP_Error_RefundNotPossible = -4,
        ResultSP_Error_Fine_Number_Not_Found = -5,
        ResultSP_Error_Fine_Type_Not_Payable = -6,
        ResultSP_Error_Fine_Payment_Period_Expired = -7,
        ResultSP_Error_Fine_Number_Already_Paid = -8,
        ResultSP_Error_Generic = -9,
        ResultSP_Error_Invalid_Input_Parameter = -10,
        ResultSP_Error_Missing_Input_Parameter = -11,
        ResultSP_Error_Invalid_City = -12,
        ResultSP_Error_Invalid_Group = -13,
        ResultSP_Error_Invalid_Tariff = -14,
        ResultSP_Error_Tariff_Not_Available = -15,
        ResultSP_Error_InvalidExternalProvider = -16,
        ResultSP_Error_OperationAlreadyExist = -17,
        ResultSP_Error_CrossSourceExtensionNotPossible = -24,
    }

    public class ThirdPartyOperation :ThirdPartyBase
    {
        private const int DEFAULT_TIME_STEP = 5; //minutes
        private const int DEFAULT_AMOUNT_STEP = 5; //CENTS      

        public ThirdPartyOperation() : base()
        {
            m_Log = new CLogWrapper(typeof(ThirdPartyOperation));
        }

        public ResultType EysaConfirmParking(int iWSNumber, string strPlate, DateTime dtUTCInsertionDate, USER oUser, INSTALLATION oInstallation, decimal? dGroupId, decimal? dTariffId,
                                             int iQuantity, int iTime,DateTime dtIni, DateTime dtEnd, int iQFEE, int iQBonus, int iQFEEVAT, string sBonusMarca, int? iBonusType,  decimal? dLatitude, 
                                             decimal? dLongitude, ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
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


                EysaThirdPartyConfirmParkWS.Ticket oParkWS = new EysaThirdPartyConfirmParkWS.Ticket();
                oParkWS.Timeout = Get3rdPartyWSTimeout();
                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        break;

                    case 2:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS2_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS2_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD);
                        }
                        break;

                    case 3:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS3_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS3_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD);
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("EysaConfirmParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                EysaThirdPartyConfirmParkWS.ConsolaSoapHeader authentication = new EysaThirdPartyConfirmParkWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oParkWS.ConsolaSoapHeaderValue = authentication;

                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;
                string strCompanyName = ConfigurationManager.AppSettings["EYSACompanyName"].ToString();



                string strMessage = "";
                string strAuthHash = "";


                string strExtTariffId = "";
                string strExtGroupId = "";


                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId.Value, dTariffId.Value, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("EysaConfirmParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                strAuthHash = CalculateEysaWSHash(strHashKey,
                    string.Format("1{0}{1}{2:yyyy-MM-ddTHH:mm:ss.fff}{3}{4}{5}{6}{7:yyyy-MM-ddTHH:mm:ss.fff}{8:yyyy-MM-ddTHH:mm:ss.fff}{9}{10}{11}{12}{13}{14}",
                    strCityID, strPlate, dtUTCInsertionDate, strExtGroupId, strExtTariffId, iQuantity, iTime, dtIni, dtEnd, strvers, iQFEE, iQBonus, iQFEEVAT, iBonusType, sBonusMarca));

                strMessage = string.Format("<ipark_in><u>1</u><city_id>{0}</city_id><p>{1}</p><d>{2:yyyy-MM-ddTHH:mm:ss.fff}</d><g>{3}</g><tar_id>{4}</tar_id><q>{5}</q><t>{6}</t>" +
                                           "<bd>{7:yyyy-MM-ddTHH:mm:ss.fff}</bd><ed>{8:yyyy-MM-ddTHH:mm:ss.fff}</ed><vers>{9}</vers><ah>{10}</ah><em>{11}</em><qt>{12}</qt><qc>{13}</qc>"+
                                           "<iva>{14}</iva><o>{15}</o><marca>{16}</marca><lt_ticket>{17}</lt_ticket><lg_ticket>{18}</lg_ticket></ipark_in>",
                    strCityID, strPlate, dtUTCInsertionDate, strExtGroupId, strExtTariffId, iQuantity, iTime, dtIni, dtEnd, strvers, strAuthHash, strCompanyName, iQFEE, iQBonus, 
                    iQFEEVAT, iBonusType, sBonusMarca, 
                    dLatitude.HasValue? dLatitude.Value.ToString(CultureInfo.InvariantCulture): "-999",
                    dLongitude.HasValue? dLongitude.Value.ToString(CultureInfo.InvariantCulture): "-999");

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaConfirmParking xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oParkWS.rdPConfirmParkingOperation(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaConfirmParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    if (Convert.ToInt32(wsParameters["r"].ToString()) > 0)
                    {
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        str3dPartyOpNum = wsParameters["opnum"].ToString();
                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        parametersOut["r"] = Convert.ToInt32(rtRes);

                    }
                }

            }
            catch (Exception e)
            {
                if ((watch!=null)&&(lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }

                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "EysaConfirmParking::Exception", LogLevels.logERROR);
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

        public ResultType StandardConfirmParking(int iWSNumber, string strPlate, DateTime dtParkQuery, USER oUser, INSTALLATION oInstallation, decimal? dGroupId, decimal? dTariffId, int iQuantity, int iPaidQuantity, int iTime,
                                                          DateTime dtIni, DateTime dtEnd, decimal dOperationId, string strPlaceString, int iPostpay, ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
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


                StandardParkingWS.TariffComputerWS oParkWS = new StandardParkingWS.TariffComputerWS();
                oParkWS.Timeout = Get3rdPartyWSTimeout();
                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        break;

                    case 2:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS2_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS2_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD);
                        }
                        break;

                    case 3:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS3_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS3_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD);
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("StandardConfirmParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }


                DateTime dtInstallation = dtParkQuery;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();



                string strMessage = "";
                string strAuthHash = "";


                string strExtTariffId = "";
                string strExtGroupId = "";

                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId.Value, dTariffId.Value, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("StandardConfirmParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                strAuthHash = CalculateStandardWSHash(strHashKey,
                    string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7:HHmmssddMMyyyy}{8:HHmmssddMMyyyy}{9}{10}{11}{12}{13}{14}{15}",
                    strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iQuantity, iTime, dtIni, dtEnd, strvers,
                    dOperationId, strCompanyName, oUser.USR_USERNAME, strPlaceString, iPostpay, iPaidQuantity));

                strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><pur_date>{2:HHmmssddMMyyyy}</pur_date>" +
                                           "<grp_id>{3}</grp_id><tar_id>{4}</tar_id>" +
                                           "<amou_payed>{5}</amou_payed><time_payed>{6}</time_payed>" +
                                           "<ini_date>{7:HHmmssddMMyyyy}</ini_date>" +
                                           "<end_date>{8:HHmmssddMMyyyy}</end_date>" +
                                           "<ver>{9}</ver><oper_id>{10}</oper_id><prov>{11}</prov><ext_acc>{12}</ext_acc><space>{13}</space><postpay>{14}</postpay><real_amou_payed>{15}</real_amou_payed>" +
                                           "<ah>{16}</ah></ipark_in>",
                    strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iQuantity, iTime, dtIni, dtEnd, strvers, dOperationId,
                    strCompanyName, oUser.USR_USERNAME, strPlaceString, iPostpay, iPaidQuantity, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("StandardConfirmParking xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oParkWS.InsertExternalParkingOperationInstallationTime(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("StandardConfirmParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypeStandardParkingWS_TO_ResultType((ResultTypeStandardParkingWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        str3dPartyOpNum = wsParameters["oper_id"].ToString();
                    }
                    else
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        parametersOut["r"] = Convert.ToInt32(rtRes);

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
                Logger_AddLogException(e, "StandardConfirmParking::Exception", LogLevels.logERROR);

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
        
        public ResultType StandardConfirmUnParking(int iWSNumber, string strPlate, DateTime dtUnParkQuery, USER oUser, INSTALLATION oInstallation,
                                                   int iQuantity, int iTime, decimal dGroupId,decimal dTariffId, DateTime dtIni, DateTime dtEnd,
                                                   decimal dOperationId, ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
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


                StandardParkingWS.TariffComputerWS oUnParkWS = new StandardParkingWS.TariffComputerWS();
                oUnParkWS.Timeout = Get3rdPartyWSTimeout();
                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oUnParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oUnParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        break;

                    case 2:
                        oUnParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS2_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS2_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oUnParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD);
                        }
                        break;

                    case 3:
                        oUnParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS3_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS3_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oUnParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD);
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("StandardConfirmUnParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }


                DateTime dtInstallation = dtUnParkQuery;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();

                string strExtTariffId = "";
                string strExtGroupId = "";


                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId, dTariffId, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("StandardConfirmUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                string strMessage = "";
                string strAuthHash = "";


                strAuthHash = CalculateStandardWSHash(strHashKey,
                    string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}", strCityID, strPlate, dtInstallation, oUser.USR_USERNAME, strExtGroupId, strExtTariffId, dOperationId, strCompanyName, strvers));

                strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><ext_acc>{3}</ext_acc><grp_id>{4}</grp_id><tar_id>{5}</tar_id><oper_id>{6}</oper_id><prov>{7}</prov><vers>{8}</vers><ah>{9}</ah></ipark_in>",
                    strCityID, strPlate, dtInstallation, oUser.USR_USERNAME, strExtGroupId, strExtTariffId, dOperationId, strCompanyName, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("StandardConfirmUnParking xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oUnParkWS.InsertExternalUnParkingOperationInstallationTime(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");


                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("StandardConfirmUnParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    if (Convert.ToInt32(wsParameters["r"].ToString()) > 0)
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
                Logger_AddLogException(e, "StandardConfirmUnParking::Exception", LogLevels.logERROR);

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






        public ResultType GtechnaConfirmParking(int iWSNumber, decimal? dMobileSessionId, string strPlate, DateTime dtParkQuery, INSTALLATION oInstallation,decimal? dGroupId,decimal? dTariffId, 
                                                int iQuantity, int iTime,DateTime dtIni, DateTime dtEnd, decimal dOperationId, ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
        {
            ResultType rtRes = ResultType.Result_OK;
            str3dPartyOpNum = "";
            lEllapsedTime = -1;
            Stopwatch watch = null;


            string sParamsIn = "";
            string sParamsOut = "";
            Exception oNotificationEx = null;            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                gTechnaThirdPartyParkingConfirmWS.MESParkingRightsSOAPFacadeService oParkWS = new gTechnaThirdPartyParkingConfirmWS.MESParkingRightsSOAPFacadeService();
                oParkWS.Timeout = Get3rdPartyWSTimeout();

                switch (iWSNumber)
                {
                    case 1:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS_URL;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        break;

                    case 2:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS2_URL;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD);
                        }
                        break;

                    case 3:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS3_URL;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD);
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("GtechnaConfirmParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                DateTime dtInstallation = dtParkQuery;
                string strCompanyName = ConfigurationManager.AppSettings["GtechnaCompanyName"].ToString();

                string strUsername = "";
                string strWIFIMAC = "";
                string strIMEI = "";

                MOBILE_SESSION oMobileSession = null;
                if (dMobileSessionId.HasValue)
                    customersRepository.GetMobileSessionById(dMobileSessionId.Value, out oMobileSession);
                if (oMobileSession != null)
                {
                    strUsername = oMobileSession.USER.USR_USERNAME;
                    strWIFIMAC = oMobileSession.MOSE_CELL_WIFI_MAC ?? "";
                    strIMEI = oMobileSession.MOSE_CELL_IMEI ?? "";
                }

                string strTerminalNumber = strIMEI + "/" + strWIFIMAC;

                string strExtTariffId = "";
                string strExtGroupId = "";

                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId.Value, dTariffId.Value, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("GtechnaConfirmParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                bool bPrefixFound = false;
                /*foreach (string strProvince in CanadaAndUSAProvinces)
                {
                    if (strPlate.Substring(0, 2) == strProvince)
                    {
                        bPrefixFound = true;
                        break;
                    }
                }*/


                string strParameterPlate = "";
                string strParameterState = "";
                if (bPrefixFound)
                {
                    strParameterPlate = strPlate.Substring(2, strPlate.Length - 2);
                    strParameterState = strPlate.Substring(0, 2);
                }
                else
                {
                    strParameterPlate = strPlate;
                    strParameterState = "";
                }

                DateTime? dtInstallationUTC = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtInstallation);
                DateTime? dtIniUTC = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtIni);
                DateTime? dtEndUTC = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtEnd);

                sParamsIn = string.Format("updateParkingRights({0},null,{1},{2},{3:HH:mm:ss dd/MM/yyyy},{4:HH:mm:ss dd/MM/yyyy},{5:HH:mm:ss dd/MM/yyyy},{6},{7},0,{8},{9},null,null,false,{10},null,null,null,null,false)",
                                           strParameterPlate,
                                           strCompanyName,
                                           dOperationId.ToString(),
                                           dtInstallationUTC,
                                           dtIniUTC,
                                           dtEndUTC,
                                           Convert.ToDecimal(iQuantity) / 100,
                                           infraestructureRepository.GetCurrencyIsoCode( Convert.ToInt32(oInstallation.INS_CUR_ID)),
                                           strTerminalNumber,
                                           strExtGroupId,
                                           strParameterState);

                Logger_AddLogMessage(sParamsIn, LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();

                string strRes = oParkWS.updateParkingRights(strParameterPlate,
                                            null,
                                            strCompanyName,
                                            dOperationId.ToString(),
                                            dtInstallationUTC.Value,
                                            dtIniUTC.Value,
                                            dtEndUTC.Value,
                                            Convert.ToDecimal(iQuantity) / 100,
                                            infraestructureRepository.GetCurrencyIsoCode(Convert.ToInt32(oInstallation.INS_CUR_ID)),
                                            0,
                                            strTerminalNumber,
                                            strExtGroupId,
                                            null,
                                            null,
                                            false,
                                            strParameterState,
                                            null,
                                            null,
                                            null,
                                            null,
                                            false);

                lEllapsedTime = watch.ElapsedMilliseconds;

                Logger_AddLogMessage(string.Format("GtechnaConfirmParking:updateParkingRights()={0}", strRes), LogLevels.logDEBUG);

                parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                str3dPartyOpNum = strRes;

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
                Logger_AddLogException(e, "GtechnaConfirmParking::Exception", LogLevels.logERROR);

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sParamsIn, sParamsOut, false, oNotificationEx);
            }
            catch
            { }


            return rtRes;
        }
        
        public ResultType EysaConfirmUnParking(int iWSNumber, string strPlate, DateTime dtInstallation, USER oUser, INSTALLATION oInstallation, 
                                               int iQuantity, int iTime, decimal? dGroupId,decimal? dTariffId,DateTime dtIni, DateTime dtEnd,
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

                EysaThirdPartyConfirmParkWS.Ticket oParkWS = new EysaThirdPartyConfirmParkWS.Ticket();
                oParkWS.Timeout = Get3rdPartyWSTimeout();
                string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD);
                        }
                        break;

                    case 2:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS2_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS2_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD);
                        }
                        break;

                    case 3:
                        oParkWS.Url = oInstallation.INS_PARK_CONFIRM_WS3_URL;
                        strHashKey = oInstallation.INS_PARK_CONFIRM_WS3_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER, oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD);
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("EysaConfirmUnParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }



                EysaThirdPartyConfirmParkWS.ConsolaSoapHeader authentication = new EysaThirdPartyConfirmParkWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oParkWS.ConsolaSoapHeaderValue = authentication;

                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;
                string strCompanyName = ConfigurationManager.AppSettings["EYSACompanyName"].ToString();


                string strExtTariffId = "";
                string strExtGroupId = "";


                if ((dGroupId.HasValue)&&(dTariffId.HasValue)&&
                    (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId.Value, dTariffId.Value, ref strExtGroupId, ref strExtTariffId)))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("EysaConfirmUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                string strMessage = "";
                string strAuthHash = "";


                strAuthHash = CalculateEysaWSHash(strHashKey,
                    string.Format("1{0}{1:yyyy-MM-ddTHH:mm:ss.fff}{2}",
                    strPlate, dtInstallation, iQuantity, strvers));

                strMessage = string.Format("<ipark_in><u>1</u><m>{0}</m><d>{1:yyyy-MM-ddTHH:mm:ss.fff}</d><q>{2}</q>" +
                                           "<vers>{3}</vers><ah>{4}</ah><em>{5}</em><g>{6}</g><tar_id>{7}</tar_id></ipark_in>",
                    strPlate, dtInstallation, iQuantity, strvers, strAuthHash, strCompanyName, strExtGroupId, strExtTariffId);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaConfirmUnParking xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oParkWS.rdPConfirmUnParkingOperation(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaConfirmUnParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


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
                Logger_AddLogException(e, "EysaConfirmUnParking::Exception", LogLevels.logERROR);

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

        public ResultType EysaQueryParking(int iWSNumber, USER oUser, string strPlate, DateTime dtParkQuery, GROUP oGroup, TARIFF oTariff, bool bWithSteps, int? iMaxAmountAllowedToPay,
                                           double dChangeToApply, ulong ulAppVersion, ref SortedList parametersOut, ref string strAuthId)
        {

            ResultType rtRes = ResultType.Result_OK;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 

                EysaThirdPartyParkWS.Tarifas oParkWS = new EysaThirdPartyParkWS.Tarifas();
                oParkWS.Url = oGroup.INSTALLATION.INS_PARK_WS_URL;
                oParkWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER))
                {
                    oParkWS.Credentials = new System.Net.NetworkCredential(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER, oGroup.INSTALLATION.INS_PARK_WS_HTTP_PASSWORD);
                }

                EysaThirdPartyParkWS.ConsolaSoapHeader authentication = new EysaThirdPartyParkWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oGroup.INSTALLATION.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oParkWS.ConsolaSoapHeaderValue = authentication;

                DateTime dtInstallation = dtParkQuery;
                string strvers = "1.0";
                string strCityID = oGroup.INSTALLATION.INS_EYSA_CONTRATA_ID;
                string strCompanyName = ConfigurationManager.AppSettings["EYSACompanyName"].ToString();


                string strMessage = "";
                string strAuthHash = "";

                string strExtTariffId = "";
                string strExtGroupId = "";

                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, oGroup, oTariff, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("EysaQueryParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }



                if (strExtTariffId.Length == 0)
                {
                    strAuthHash = CalculateEysaWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("1{0}{1}{2:yyyy-MM-ddTHH:mm:ss.fff}{3}{4}", strCityID, strPlate, dtInstallation, strExtGroupId, strvers));

                    strMessage = string.Format("<ipark_in><u>1</u><city_id>{0}</city_id><p>{1}</p><d>{2:yyyy-MM-ddTHH:mm:ss.fff}</d><g>{3}</g><vers>{4}</vers><ah>{5}</ah><em>{6}</em></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strvers, strAuthHash, strCompanyName);
                }
                else
                {

                    strAuthHash = CalculateEysaWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("1{0}{1}{2:yyyy-MM-ddTHH:mm:ss.fff}{3}{4}{5}", strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strvers));

                    strMessage = string.Format("<ipark_in><u>1</u><city_id>{0}</city_id><p>{1}</p><d>{2:yyyy-MM-ddTHH:mm:ss.fff}</d><g>{3}</g><tar_id>{4}</tar_id><vers>{5}</vers><ah>{6}</ah><em>{7}</em></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strvers, strAuthHash, strCompanyName);


                }

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaThirdPartyQueryParkingOperationWithTimeSteps xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = oParkWS.rdPQueryParkingOperationWithTimeSteps(strMessage);
                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaThirdPartyQueryParkingOperationWithTimeSteps xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    if (Convert.ToInt32(wsParameters["r"].ToString()) == (int)ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        parametersOut["ad"] = oTariff.TAR_ID.ToString();
                        parametersOut["q1"] = wsParameters["q1"];
                        parametersOut["q2"] = wsParameters["q2"];
                        parametersOut["t1"] = wsParameters["t1"];
                        parametersOut["t2"] = wsParameters["t2"];
                        parametersOut["o"] = wsParameters["o"];
                        parametersOut["at"] = wsParameters["at"];
                        parametersOut["aq"] = wsParameters["aq"];
                        parametersOut["cur"] = oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE;

                        DateTime dt = DateTime.ParseExact(wsParameters["di"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                    CultureInfo.InvariantCulture);
                        parametersOut["di"] = dt.ToString("HHmmssddMMyy");

                        parametersOut["bonusper"] = wsParameters["bonusper"];
                        parametersOut["bonusid"] = wsParameters["bonusid"];
                        parametersOut["bonusmarca"] = wsParameters["marca"];
                        parametersOut["bonustype"] = wsParameters["oeysa"];

                        strAuthId = "";
                        if (wsParameters.ContainsKey("idP"))
                        {
                            strAuthId = wsParameters["idP"].ToString();
                        }

                       
                        parametersOut["idP"] = strAuthId;
                        parametersOut["coe"] = wsParameters["coe"];
                        parametersOut["carcat_desc"] = wsParameters["ca"];
                        parametersOut["ocu_desc"] = wsParameters["ocu"];
                        if (wsParameters["coe"] != null && wsParameters["ca"] != null && wsParameters["ocu"] != null)
                            parametersOut["forcedisp"] = "1";
                        else
                            parametersOut["forcedisp"] = "0";

                        Logger_AddLogMessage(string.Format("EysaThirdPartyQueryParkingOperationWithTimeSteps Coe={0}, Ca={1}, Ocu={2}, ForceDisp={4}, Plate={3}", wsParameters["coe"], wsParameters["ca"], wsParameters["ocu"], strPlate, parametersOut["forcedisp"]), LogLevels.logDEBUG);
                       
                        double dChangeFee = 0;
                        int iQChange = 0;

                        if (oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE != oUser.CURRENCy.CUR_ISO_CODE)
                        {
                            iQChange = ChangeQuantityFromInstallationCurToUserCur(Convert.ToInt32(parametersOut["q1"]),
                                            dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                            parametersOut["qch1"] = iQChange.ToString();
                            iQChange = ChangeQuantityFromInstallationCurToUserCur(Convert.ToInt32(parametersOut["q2"]),
                                            dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                            parametersOut["qch2"] = iQChange.ToString();

                        }


                        if (bWithSteps)
                        {
                            int iNumSteps = Convert.ToInt32(wsParameters["steps_step_num"]);
                            string strSteps = "";

                            ChargeOperationsType oOperationType = (Convert.ToInt32(parametersOut["o"]) == 1 ? ChargeOperationsType.ParkingOperation : ChargeOperationsType.ExtensionOperation);

                            int iQ = 0;
                            int iQFEE = 0;
                            int iQFEEChange = 0;
                            int iQBonus = 0;
                            int iQBonusChange = 0;
                            int iQVAT = 0;
                            int iQSubTotal = 0;
                            int iQTotal = 0;
                            int iQTotalChange = 0;
                            int iQSubTotalChange = 0;

                            decimal dVAT1;
                            decimal dVAT2;
                            int iPartialVAT1;
                            decimal dPercFEE;
                            int iPercFEETopped;
                            int iPartialPercFEE;
                            int iFixedFEE;
                            int iPartialFixedFEE;
                            int iPartialPercFEEVAT;
                            int iPartialFixedFEEVAT;
                            decimal dPercBonus = 0;
                            int iPartialBonusFEE;
                            int iPartialBonusFEEVAT;

                            int? iPaymentTypeId = null;
                            int? iPaymentSubtypeId = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN != null)
                            {
                                iPaymentTypeId = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID;
                                iPaymentSubtypeId = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID;
                            }
                            if (!customersRepository.GetFinantialParams(oUser, oGroup.INSTALLATION.INS_ID, (PaymentSuscryptionType)oUser.USR_SUSCRIPTION_TYPE, iPaymentTypeId, iPaymentSubtypeId, oOperationType,
                                                                        out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE))
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("EysaQueryParking::Error getting finantial parameters", LogLevels.logERROR);
                            }

                            if (rtRes == ResultType.Result_OK)
                            {
                                if (parametersOut["bonusper"] != null)
                                {
                                    dPercBonus = Convert.ToDecimal(parametersOut["bonusper"]) / Convert.ToDecimal(100);
                                }

                                for (int i = 0; i < iNumSteps; i++)
                                {
                                    dt = DateTime.ParseExact(wsParameters[string.Format("steps_step_{0}_d", i)].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

                                    iQ = Convert.ToInt32(wsParameters[string.Format("steps_step_{0}_q", i)].ToString());

                                    iQTotal = customersRepository.CalculateFEE(iQ, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, dPercBonus,
                                                                               out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE,
                                                                               out iPartialPercFEEVAT, out iPartialFixedFEEVAT, out iPartialBonusFEEVAT);
                                    iQFEE = Convert.ToInt32(Math.Round(iQ * dPercFEE, MidpointRounding.AwayFromZero));
                                    if (iPercFEETopped > 0 && iQFEE > iPercFEETopped) iQFEE = iPercFEETopped;
                                    iQFEE += iFixedFEE;
                                    iQBonus = Convert.ToInt32(Math.Round(iQFEE * dPercBonus, MidpointRounding.AwayFromZero));
                                    iQVAT = iPartialVAT1 + iPartialPercFEEVAT + iPartialFixedFEEVAT - iPartialBonusFEEVAT;
                                    iQSubTotal = iQ + iQFEE - iQBonus;

                                    if (oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE != oUser.CURRENCy.CUR_ISO_CODE)
                                    {
                                        iQChange = ChangeQuantityFromInstallationCurToUserCur(iQ, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                                        iQFEEChange = ChangeQuantityFromInstallationCurToUserCur(iQFEE, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);
                                        iQBonusChange = ChangeQuantityFromInstallationCurToUserCur(iQBonus, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);
                                        iQSubTotalChange = ChangeQuantityFromInstallationCurToUserCur(iQSubTotal, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);
                                        iQTotalChange = ChangeQuantityFromInstallationCurToUserCur(iQTotal, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);


                                        if (iMaxAmountAllowedToPay.HasValue)
                                        {
                                            if (iQTotalChange > iMaxAmountAllowedToPay)
                                            {
                                                if (i == 0)
                                                {
                                                    rtRes = ResultType.Result_Error_Not_Enough_Balance; ;
                                                    parametersOut["r"] = Convert.ToInt32(rtRes);
                                                }
                                                break;
                                            }
                                        }

                                        strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><qch>{2}</qch><d>{3:HHmmssddMMyy}</d><q_fee>{4}</q_fee><qbonusam>{11}</qbonusam><q_vat>{5}</q_vat><q_subtotal>{6}</q_subtotal><q_total>{7}</q_total><qch_fee>{8}</qch_fee><qchbonusam>{12}</qchbonusam><qch_subtotal>{9}</qch_subtotal><qch_total>{10}</qch_total></step>",
                                                                wsParameters[string.Format("steps_step_{0}_t", i)].ToString(),
                                                                wsParameters[string.Format("steps_step_{0}_q", i)].ToString(),
                                                                iQChange,
                                                                dt,
                                                                iQFEE, iQVAT, iQSubTotal, iQTotal,
                                                                iQFEEChange, iQSubTotalChange, iQTotalChange,
                                                                -iQBonus, -iQBonusChange);

                                        parametersOut["q2"] = wsParameters[string.Format("steps_step_{0}_q", i)];
                                        parametersOut["t2"] = wsParameters[string.Format("steps_step_{0}_t", i)];


                                    }
                                    else
                                    {
                                        if (iMaxAmountAllowedToPay.HasValue)
                                        {
                                            if (iQTotal > iMaxAmountAllowedToPay)
                                            {
                                                if (i == 0)
                                                {
                                                    rtRes = ResultType.Result_Error_Not_Enough_Balance; ;
                                                    parametersOut["r"] = Convert.ToInt32(rtRes);
                                                }
                                                break;
                                            }
                                        }

                                        strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><d>{2:HHmmssddMMyy}</d><q_fee>{3}</q_fee><qbonusam>{7}</qbonusam><q_vat>{4}</q_vat><q_subtotal>{5}</q_subtotal><q_total>{6}</q_total></step>",
                                                                wsParameters[string.Format("steps_step_{0}_t", i)].ToString(),
                                                                wsParameters[string.Format("steps_step_{0}_q", i)].ToString(),
                                                                dt,
                                                                iQFEE, iQVAT, 
                                                                iQSubTotal, iQTotal,
                                                                -iQBonus);

                                        parametersOut["q2"] = wsParameters[string.Format("steps_step_{0}_q", i)];
                                        parametersOut["t2"] = wsParameters[string.Format("steps_step_{0}_t", i)];


                                    }

                                }
                            }

                            parametersOut["steps"] = strSteps;
                        }

                    }
                    else
                    {
                       

                        if (wsParameters["r"].ToString() == "-100")
                        {
                            rtRes = ResultType.Result_Parking_Not_Allowed;
                            parametersOut["r"] = Convert.ToInt32(rtRes);                            
                            parametersOut["rsub"] = wsParameters["rsub"]; ;
                            parametersOut["rsubcesc"] = wsParameters["rsubcesc"]; ;


                        }
                        else
                        {
                            parametersOut["r"] = Convert.ToInt32(wsParameters["r"]);
                            rtRes = (ResultType)parametersOut["r"];
                        }
                    }

                }



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "EysaQueryParking::Exception", LogLevels.logERROR);

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

        public ResultType EysaQueryUnParking(int iWSNumber,USER oUser, string strPlate, DateTime dtUnParkQuery, INSTALLATION oInstallation, ulong ulAppVersion, ref SortedList parametersOut,
                                             ref List<SortedList> lstRefunds)
        {

            ResultType rtRes = ResultType.Result_OK;            
            decimal? dGroupId = null;
            decimal? dTariffId = null;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 


                EysaThirdPartyConfirmParkWS.Ticket oUnParkWS = new EysaThirdPartyConfirmParkWS.Ticket();
                oUnParkWS.Url = oInstallation.INS_UNPARK_WS_URL;
                oUnParkWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_UNPARK_WS_HTTP_USER))
                {
                    oUnParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_UNPARK_WS_HTTP_USER, oInstallation.INS_UNPARK_WS_HTTP_PASSWORD);
                }

                EysaThirdPartyConfirmParkWS.ConsolaSoapHeader authentication = new EysaThirdPartyConfirmParkWS.ConsolaSoapHeader();
                authentication.IdContrata = Convert.ToInt32(oInstallation.INS_EYSA_CONTRATA_ID);
                authentication.IdUsuario = oUser.USR_ID.ToString();
                oUnParkWS.ConsolaSoapHeaderValue = authentication;

                DateTime dtInstallation = dtUnParkQuery;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_EYSA_CONTRATA_ID;
                string strCompanyName = ConfigurationManager.AppSettings["EYSACompanyName"].ToString();


                string strMessage = "";
                string strAuthHash = "";


                strAuthHash = CalculateEysaWSHash(oInstallation.INS_UNPARK_WS_AUTH_HASH_KEY,
                    string.Format("1{0}{1:yyyy-MM-ddTHH:mm:ss.fff}{2}", strPlate, dtInstallation, strvers));

                strMessage = string.Format("<ipark_in><u>1</u><p>{0}</p><d>{1:yyyy-MM-ddTHH:mm:ss.fff}</d><vers>{2}</vers><ah>{3}</ah><em>{4}</em><g>{5}</g></ipark_in>",
                    strPlate, dtInstallation, strvers, strAuthHash, strCompanyName, strCityID);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("EysaQueryUnParking xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = oUnParkWS.rdPQueryUnParkingOperation(strMessage);
                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("EysaQueryUnParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    if (Convert.ToInt32(wsParameters["r"].ToString()) == (int)ResultType.Result_OK)
                    {
                        /*
                         	  <r>1</r>
	                          <q>165</q>
	                          <d1>04/12/2013 11:15:19</d1>
	                          <d2>04/12/2013 11:35:05</d2>
	                          <t>99</t>
	                        </ipark_out>                         
                         */
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);

                        SortedList oRefund = new SortedList();
                        oRefund["p"] = strPlate;
                        oRefund["q"] = wsParameters["q"];
                        oRefund["t"] = wsParameters["t"];
                        oRefund["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;

                        DateTime dt1 = DateTime.ParseExact(wsParameters["d1"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                    CultureInfo.InvariantCulture);
                        DateTime dt2 = DateTime.ParseExact(wsParameters["d2"].ToString(), "yyyy-MM-ddTHH:mm:ss.fff",
                                    CultureInfo.InvariantCulture);
                        oRefund["d1"] = dt1.ToString("HHmmssddMMyy");
                        oRefund["d2"] = dt2.ToString("HHmmssddMMyy");
                        oRefund["exp"] = (Conversions.RoundSeconds(dtInstallation) < Conversions.RoundSeconds(dt2)) ? "0" : "1";

                       
                        oRefund["bonusper"] = wsParameters["bonusper"];
                        oRefund["bonusid"] = wsParameters["bonusid"];

                    

                        string strExtTariffId="";
                        string strExtGroupId ="";

                        if ((wsParameters["tar_id"] != null) && (wsParameters["g"] != null))
                        {

                            strExtTariffId = wsParameters["tar_id"].ToString();
                            strExtGroupId = wsParameters["g"].ToString();

                            if (!geograficAndTariffsRepository.GetGroupAndTariffFromExternalId(iWSNumber, oInstallation, strExtGroupId, 
                                    strExtTariffId, ref dGroupId, ref dTariffId))
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("EysaQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }

                            if (!dTariffId.HasValue)
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("EysaQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }

                            if (!dGroupId.HasValue)
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("EysaQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }


                            oRefund["ad"] = dTariffId.Value;
                            oRefund["g"] = dGroupId.Value;
                        }
                        
                        lstRefunds.Add(oRefund);
                    }
                    else
                    {
                        parametersOut["r"] = Convert.ToInt32(wsParameters["r"]);
                        rtRes = (ResultType)parametersOut["r"];
                    }                  

                }



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "EysaQueryUnParking::Exception", LogLevels.logERROR);

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            { }


            return rtRes;

        }

        public ResultType StandardQueryParkingTimeSteps(int iWSNumber, USER oUser, string strPlate, DateTime dtParkQuery, GROUP oGroup, TARIFF oTariff, bool bWithSteps, int? iMaxAmountAllowedToPay, double dChangeToApply, ref SortedList parametersOut, ref List<SortedList> oAdditionals)
        {

            ResultType rtRes = ResultType.Result_OK;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 


                StandardParkingWS.TariffComputerWS oParkWS = new StandardParkingWS.TariffComputerWS();
                oParkWS.Url = oGroup.INSTALLATION.INS_PARK_WS_URL;
                oParkWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER))
                {
                    oParkWS.Credentials = new System.Net.NetworkCredential(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER, oGroup.INSTALLATION.INS_PARK_WS_HTTP_PASSWORD);
                }

                DateTime dtInstallation = dtParkQuery;
                string strvers = "1.0";
                string strCityID = oGroup.INSTALLATION.INS_STANDARD_CITY_ID;
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();


                string strMessage = "";
                string strAuthHash = "";

                string strExtTariffId = "";
                string strExtGroupId = "";

                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, oGroup, oTariff, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("StandardQueryParkingTimeSteps::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                if (!bWithSteps)
                {

                    strAuthHash = CalculateStandardWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}", strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers));

                    strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><grp_id>{3}</grp_id><tar_id>{4}</tar_id><prov>{5}</prov><ext_acc>{6}</ext_acc><free_time>{7}</free_time><vers>{8}</vers><ah>{9}</ah></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers, strAuthHash);
                }
                else
                {
                    int? iTimeOffSet = DEFAULT_TIME_STEP; //minutes
                    geograficAndTariffsRepository.GetGroupAndTariffStepOffsetMinutes(oGroup, oTariff, out iTimeOffSet);


                    strAuthHash = CalculateStandardWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}{9}", strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iTimeOffSet, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers));

                    strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><grp_id>{3}</grp_id><tar_id>{4}</tar_id><time_off>{5}</time_off><prov>{6}</prov><ext_acc>{7}</ext_acc><free_time>{8}</free_time><vers>{9}</vers><ah>{10}</ah></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iTimeOffSet, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers, strAuthHash);

                }

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("StandardQueryParkingTimeSteps xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = "";
                if (!bWithSteps)
                {
                    strOut = oParkWS.QueryParkingOperation(strMessage);
                }
                else
                {
                    strOut = oParkWS.QueryParkingOperationWithTimeSteps(strMessage);
                }

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("StandardQueryParkingTimeSteps xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypeStandardParkingWS_TO_ResultType((ResultTypeStandardParkingWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        rtRes = StandardQueryParkingComputeOutput("", iWSNumber, oUser, strPlate, dtParkQuery, oGroup, oTariff, bWithSteps, iMaxAmountAllowedToPay, dChangeToApply, strExtGroupId, strExtTariffId, ref wsParameters, ref parametersOut);

                        parametersOut["numadditionals"] = 0;

                        if (wsParameters["numadditionals"] != null && Convert.ToInt32(wsParameters["numadditionals"]) > 0)
                        {
                            int iNumAdditionals = 0;
                            int i = 0;
                            bool bExit = false;
                            ResultType rtResInt;

                            do
                            {
                                bExit = (wsParameters[string.Format("additionals_parkingdata_{0}_r", i)] == null);

                                if (!bExit)
                                {
                                    rtResInt = Convert_ResultTypeStandardParkingWS_TO_ResultType((ResultTypeStandardParkingWS)Convert.ToInt32(wsParameters[string.Format("additionals_parkingdata_{0}_r", i)].ToString()));

                                    if (rtResInt == ResultType.Result_OK)
                                    {

                                        if (wsParameters[string.Format("additionals_parkingdata_{0}_is_remote_extension", i)] != null &&
                                            Convert.ToInt32(wsParameters[string.Format("additionals_parkingdata_{0}_is_remote_extension", i)]) == 0)
                                        {
                                            SortedList parametersOutTemp = new SortedList();

                                            rtResInt = StandardQueryParkingComputeOutput(string.Format("additionals_parkingdata_{0}_", i),
                                                iWSNumber, oUser, strPlate, dtParkQuery, oGroup, oTariff, bWithSteps, iMaxAmountAllowedToPay, dChangeToApply, strExtGroupId, strExtTariffId, ref wsParameters, ref parametersOutTemp);


                                            if (rtResInt == ResultType.Result_OK)
                                            {
                                                oAdditionals.Add(parametersOutTemp);
                                                iNumAdditionals++;
                                            }

                                        }
                                    }
                                }
                                i++;

                            }
                            while (!bExit);

                            if (iNumAdditionals > 0)
                            {
                                parametersOut["numadditionals"] = iNumAdditionals;
                                parametersOut["additionals"] = "";


                            }
                        }
                    }
                }



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "StandardQueryParkingTimeSteps::Exception", LogLevels.logERROR);

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



        public ResultType StandardQueryParkingAmountSteps(int iWSNumber, USER oUser, string strPlate, DateTime dtParkQuery, GROUP oGroup, TARIFF oTariff, bool bWithSteps, int? iMaxAmountAllowedToPay, double dChangeToApply, ref SortedList parametersOut, ref List<SortedList> oAdditionals)
        {

            ResultType rtRes = ResultType.Result_OK;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);


                StandardParkingWS.TariffComputerWS oParkWS = new StandardParkingWS.TariffComputerWS();
                oParkWS.Url = oGroup.INSTALLATION.INS_PARK_WS_URL;
                oParkWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER))
                {
                    oParkWS.Credentials = new System.Net.NetworkCredential(oGroup.INSTALLATION.INS_PARK_WS_HTTP_USER, oGroup.INSTALLATION.INS_PARK_WS_HTTP_PASSWORD);
                }

                DateTime dtInstallation = dtParkQuery;
                string strvers = "1.0";
                string strCityID = oGroup.INSTALLATION.INS_STANDARD_CITY_ID;
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();


                string strMessage = "";
                string strAuthHash = "";

                string strExtTariffId = "";
                string strExtGroupId = "";

                if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, oGroup, oTariff, ref strExtGroupId, ref strExtTariffId))
                {
                    rtRes = ResultType.Result_Error_Generic;
                    Logger_AddLogMessage("StandardQueryParkingAmountSteps::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                }


                if (!bWithSteps)
                {

                    strAuthHash = CalculateStandardWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}", strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers));

                    strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><grp_id>{3}</grp_id><tar_id>{4}</tar_id><prov>{5}</prov><ext_acc>{6}</ext_acc><free_time>{7}</free_time><vers>{8}</vers><ah>{9}</ah></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers, strAuthHash);
                }
                else
                {
                    int? iAmountOffSet = DEFAULT_AMOUNT_STEP; //minutes
                    geograficAndTariffsRepository.GetGroupAndTariffStepOffsetMinutes(oGroup, oTariff, out iAmountOffSet);


                    strAuthHash = CalculateStandardWSHash(oGroup.INSTALLATION.INS_PARK_WS_AUTH_HASH_KEY,
                        string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}{6}{7}{8}{9}", strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iAmountOffSet, strCompanyName, oUser.USR_USERNAME,oUser.USR_TIME_BALANCE, strvers));

                    strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><grp_id>{3}</grp_id><tar_id>{4}</tar_id><amou_off>{5}</amou_off><prov>{6}</prov><ext_acc>{7}</ext_acc><free_time>{8}</free_time><vers>{9}</vers><ah>{10}</ah></ipark_in>",
                        strCityID, strPlate, dtInstallation, strExtGroupId, strExtTariffId, iAmountOffSet, strCompanyName, oUser.USR_USERNAME, oUser.USR_TIME_BALANCE, strvers, strAuthHash);

                }

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("StandardQueryParkingAmountSteps xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = "";
                if (!bWithSteps)
                {
                    strOut = oParkWS.QueryParkingOperation(strMessage);
                }
                else
                {
                    strOut = oParkWS.QueryParkingOperationWithAmountSteps(strMessage);
                }

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("StandardQueryParkingAmountSteps xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    rtRes = Convert_ResultTypeStandardParkingWS_TO_ResultType((ResultTypeStandardParkingWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        rtRes = StandardQueryParkingComputeOutput("", iWSNumber, oUser, strPlate, dtParkQuery, oGroup, oTariff, bWithSteps, iMaxAmountAllowedToPay, dChangeToApply, strExtGroupId, strExtTariffId, ref wsParameters, ref parametersOut);

                        parametersOut["numadditionals"] = 0;

                        if (wsParameters["numadditionals"]!=null && Convert.ToInt32(wsParameters["numadditionals"])>0)
                        { 
                            int iNumAdditionals=0;
                            int i=0;
                            bool bExit=false;
                            ResultType rtResInt;
                          
                            do
                            {
                                bExit = (wsParameters[string.Format("additionals_parkingdata_{0}_r", i)] == null);

                                if (!bExit)
                                {
                                    rtResInt = Convert_ResultTypeStandardParkingWS_TO_ResultType((ResultTypeStandardParkingWS)Convert.ToInt32(wsParameters[string.Format("additionals_parkingdata_{0}_r", i)].ToString()));

                                    if (rtResInt == ResultType.Result_OK)
                                    {

                                        if (wsParameters[string.Format("additionals_parkingdata_{0}_is_remote_extension", i)] != null &&
                                            Convert.ToInt32(wsParameters[string.Format("additionals_parkingdata_{0}_is_remote_extension", i)]) == 0)
                                        {
                                            SortedList parametersOutTemp = new SortedList();

                                            rtResInt = StandardQueryParkingComputeOutput(string.Format("additionals_parkingdata_{0}_", i),
                                                iWSNumber, oUser, strPlate, dtParkQuery, oGroup, oTariff, bWithSteps, iMaxAmountAllowedToPay, dChangeToApply, strExtGroupId, strExtTariffId, ref wsParameters, ref parametersOutTemp);


                                            if (rtResInt == ResultType.Result_OK)
                                            {                                               
                                                oAdditionals.Add(parametersOutTemp);
                                                iNumAdditionals++;                                                
                                            }

                                        }
                                    }
                                }
                                i++;

                            }
                            while (!bExit);

                            if (iNumAdditionals > 0)
                            {
                                parametersOut["numadditionals"] = iNumAdditionals;
                                parametersOut["additionals"] = "";

                               
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "StandardQueryParkingAmountSteps::Exception", LogLevels.logERROR);

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






        private ResultType StandardQueryParkingComputeOutput(string strXMLPrefix, int iWSNumber, USER oUser, string strPlate, DateTime dtParkQuery, GROUP oGroup, 
                                                    TARIFF oTariff, bool bWithSteps, int? iMaxAmountAllowedToPay, double dChangeToApply, 
                                                    string strExtGroupId, string strExtTariffId,
                                                    ref SortedList wsParameters,  ref SortedList parametersOut)
        {

             ResultType rtRes = ResultType.Result_OK;

            string sXmlIn = "";
            string sXmlOut = "";            

            try
            {
                parametersOut["r"] = Convert.ToInt32(rtRes);

                if (wsParameters[strXMLPrefix+"tar_id"].ToString() != strExtTariffId)
                {

                    decimal? dGroupId = null;
                    decimal? dTariffId = null;

                    if (!geograficAndTariffsRepository.GetGroupAndTariffFromExternalId(iWSNumber, oGroup.INSTALLATION, strExtGroupId, wsParameters[strXMLPrefix+"tar_id"].ToString(), ref dGroupId, ref dTariffId))
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        Logger_AddLogMessage("StandardQueryParkingComputeOutput::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                        return rtRes;
                    }

                    if (!dTariffId.HasValue)
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        Logger_AddLogMessage("StandardQueryParkingComputeOutput::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                        return rtRes;
                    }

                    parametersOut["ad"] = dTariffId.Value;

                }
                else
                {
                    parametersOut["ad"] = oTariff.TAR_ID;
                }
                parametersOut["q1"] = wsParameters[strXMLPrefix+"a_min"];
                parametersOut["q2"] = wsParameters[strXMLPrefix+"a_max"];
                parametersOut["t1"] = wsParameters[strXMLPrefix+"t_min"];
                parametersOut["t2"] = wsParameters[strXMLPrefix+"t_max"];
                parametersOut["o"] = wsParameters[strXMLPrefix+"op_type"];
                parametersOut["at"] = wsParameters[strXMLPrefix+"a_acum"];
                parametersOut["aq"] = wsParameters[strXMLPrefix+"t_acum"];
                      
                parametersOut["cur"] = oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE;
                parametersOut["postpay"] = "0";
                parametersOut["notrefundwarning"] = "0";
                if (wsParameters.ContainsKey("postpay"))
                {
                    parametersOut["postpay"] = wsParameters[strXMLPrefix+"postpay"];
                }

                if (wsParameters.ContainsKey("notrefundwarning"))
                {
                    parametersOut["notrefundwarning"] = wsParameters[strXMLPrefix+"notrefundwarning"];
                }

                DateTime dt = DateTime.ParseExact(wsParameters[strXMLPrefix+"d_init"].ToString(), "HHmmssddMMyyyy",
                            CultureInfo.InvariantCulture);
                parametersOut["di"] = dt.ToString("HHmmssddMMyy");


                double dChangeFee = 0;
                int iQChange = 0;

                if (oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE != oUser.CURRENCy.CUR_ISO_CODE)
                {
                    iQChange = ChangeQuantityFromInstallationCurToUserCur(Convert.ToInt32(parametersOut["q1"]),
                                    dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                    parametersOut["qch1"] = iQChange.ToString();
                    iQChange = ChangeQuantityFromInstallationCurToUserCur(Convert.ToInt32(parametersOut["q2"]),
                                    dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                    parametersOut["qch2"] = iQChange.ToString();

                }


                if (bWithSteps)
                {
                    int iNumSteps = Convert.ToInt32(wsParameters[strXMLPrefix+"num_steps"]);
                    string strSteps = "";

                    ChargeOperationsType oOperationType = (Convert.ToInt32(parametersOut["o"]) == 1 ? ChargeOperationsType.ParkingOperation : ChargeOperationsType.ExtensionOperation);

                    int iQ = 0;
                    int iQFEE = 0;
                    int iQFEEChange = 0;
                    decimal dQVAT = 0;
                    int iQTotal = 0;
                    int iQTotalChange = 0;
                    int iQSubTotal = 0;
                    int iQSubTotalChange = 0;

                    decimal dVAT1;
                    decimal dVAT2;
                    int iPartialVAT1;
                    decimal dPercFEE;
                    int iPercFEETopped;
                    int iPartialPercFEE;
                    int iFixedFEE;
                    int iPartialFixedFEE;
                    int iPartialPercFEEVAT;
                    int iPartialFixedFEEVAT;

                    int? iPaymentTypeId = null;
                    int? iPaymentSubtypeId = null;
                    if (oUser.CUSTOMER_PAYMENT_MEAN != null)
                    {
                        iPaymentTypeId = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID;
                        iPaymentSubtypeId = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID;
                    }
                    if (!customersRepository.GetFinantialParams(oUser, oGroup.INSTALLATION.INS_ID, (PaymentSuscryptionType)oUser.USR_SUSCRIPTION_TYPE, iPaymentTypeId, iPaymentSubtypeId, oOperationType,
                                                                out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE))
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        Logger_AddLogMessage("StandardQueryParkingTimeSteps::Error getting finantial parameters", LogLevels.logERROR);
                    }

                    if (rtRes == ResultType.Result_OK)
                    {
                        for (int i = 0; i < iNumSteps; i++)
                        {
                            dt = DateTime.ParseExact(wsParameters[strXMLPrefix+string.Format("steps_step_{0}_d", i)].ToString(), "HHmmssddMMyyyy",
                                    CultureInfo.InvariantCulture);

                            iQ = Convert.ToInt32(wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)].ToString());

                            int iFreeTimeUsed = 0;
                            int iRealQ = iQ;

                            if (wsParameters.ContainsKey(string.Format("steps_step_{0}_free_time_used", i)))
                            {
                                iFreeTimeUsed = Convert.ToInt32(wsParameters[strXMLPrefix+string.Format("steps_step_{0}_free_time_used", i)].ToString());
                            }

                            if (wsParameters.ContainsKey(string.Format("steps_step_{0}_real_a", i)))
                            {
                                if (string.IsNullOrEmpty(wsParameters[strXMLPrefix+string.Format("steps_step_{0}_real_a", i)].ToString()))
                                {
                                    iRealQ=iQ;
                                }
                                else
                                {
                                    iRealQ = Convert.ToInt32(wsParameters[strXMLPrefix+string.Format("steps_step_{0}_real_a", i)].ToString());
                                }
                                        
                            }


                            iQTotal = customersRepository.CalculateFEE(iQ, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE,
                                                                        out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE,
                                                                        out iPartialPercFEEVAT, out iPartialFixedFEEVAT);
                            iQFEE = Convert.ToInt32(Math.Round(iQ * dPercFEE, MidpointRounding.AwayFromZero));
                            if (iPercFEETopped > 0 && iQFEE > iPercFEETopped) iQFEE = iPercFEETopped;
                            iQFEE += iFixedFEE;
                            dQVAT = iPartialVAT1 + iPartialPercFEEVAT + iPartialFixedFEEVAT;
                            iQSubTotal = iQ + iQFEE;

                            if (oGroup.INSTALLATION.CURRENCy.CUR_ISO_CODE != oUser.CURRENCy.CUR_ISO_CODE)
                            {
                                iQChange = ChangeQuantityFromInstallationCurToUserCur(iQ, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                                iQFEEChange = ChangeQuantityFromInstallationCurToUserCur(iQFEE, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);
                                iQSubTotalChange = ChangeQuantityFromInstallationCurToUserCur(iQSubTotal, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);
                                iQTotalChange = ChangeQuantityFromInstallationCurToUserCur(iQTotal, dChangeToApply, oGroup.INSTALLATION, oUser, out dChangeFee);

                                if (iMaxAmountAllowedToPay.HasValue)
                                {
                                    if (iQTotalChange > iMaxAmountAllowedToPay)
                                    {
                                        if (i == 0)
                                        {
                                            rtRes = ResultType.Result_Error_Not_Enough_Balance; ;
                                            parametersOut["r"] = Convert.ToInt32(rtRes);
                                        }
                                        break;
                                    }
                                }

                                //if (oGroup.INSTALLATION.INS_SHORTDESC != "SLP")
                                //{
                                strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><qch>{2}</qch><d>{3:HHmmssddMMyy}</d><q_fee>{4}</q_fee><q_vat>{5}</q_vat><q_subtotal>{6}</q_subtotal><q_total>{7}</q_total><qch_fee>{8}</qch_fee><qch_subtotal>{9}</qch_subtotal><qch_total>{10}</qch_total><time_bal_used>{11}</time_bal_used><real_q>{12}</real_q></step>",
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)].ToString(),
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)].ToString(),
                                                            iQChange,
                                                            dt,
                                                            iQFEE, dQVAT, iQSubTotal, iQTotal,
                                                            iQFEEChange, iQSubTotalChange, iQTotalChange, iFreeTimeUsed, iRealQ);

                                    parametersOut["q2"] = wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)];
                                    parametersOut["t2"] = wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)];

                                /*}
                                else
                                {
                                    // *** New tags for loyout 3 ***
                                    strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><qch>{2}</qch><d>{3:HHmmssddMMyy}</d><q_fee>{4}</q_fee><q_vat>{5}</q_vat><q_subtotal>{6}</q_subtotal><q_total>{7}</q_total><qch_fee>{8}</qch_fee><qch_subtotal>{9}</qch_subtotal><qch_total>{10}</qch_total><qbonusam>{11}</qbonusam><qbonusamch>{12}</qbonusamch></step>",
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)].ToString(),
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)].ToString(),
                                                            iQChange,
                                                            dt,
                                                            iQFEE, dQVAT, iQSubTotal, iQTotal,
                                                            iQFEEChange, iQSubTotalChange, iQTotalChange,
                                                            Convert.ToInt32(Math.Round(iQ * 0.1, MidpointRounding.AwayFromZero)), 
                                                            Convert.ToInt32(Math.Round(iQChange * 0.1, MidpointRounding.AwayFromZero)));
                                }*/
                            }
                            else
                            {

                                if (iMaxAmountAllowedToPay.HasValue)
                                {
                                    if (iQTotalChange > iMaxAmountAllowedToPay)
                                    {
                                        if (i == 0)
                                        {
                                            rtRes = ResultType.Result_Error_Not_Enough_Balance; ;
                                            parametersOut["r"] = Convert.ToInt32(rtRes);
                                        }
                                        break;
                                    }
                                }
                                //if (oGroup.INSTALLATION.INS_SHORTDESC != "SLP")
                                //{

                                if (iMaxAmountAllowedToPay.HasValue)
                                {
                                    if (iQTotal > iMaxAmountAllowedToPay)
                                    {
                                        if (i == 0)
                                        {
                                            rtRes = ResultType.Result_Error_Not_Enough_Balance; ;
                                            parametersOut["r"] = Convert.ToInt32(rtRes);
                                        }
                                        break;
                                    }
                                }
                                strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><d>{2:HHmmssddMMyy}</d><q_fee>{3}</q_fee><q_vat>{4}</q_vat><q_subtotal>{5}</q_subtotal><q_total>{6}</q_total><time_bal_used>{7}</time_bal_used><real_q>{8}</real_q></step>",
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)].ToString(),
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)].ToString(),
                                                            dt,
                                                            iQFEE, dQVAT, iQSubTotal, iQTotal, iFreeTimeUsed, iRealQ);

                                    parametersOut["q2"] = wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)];
                                    parametersOut["t2"] = wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)];

                                /*}
                                else
                                {
                                    // *** New tags for loyout 3 ***
                                    strSteps += string.Format("<step json:Array='true'><t>{0}</t><q>{1}</q><d>{2:HHmmssddMMyy}</d><q_fee>{3}</q_fee><q_vat>{4}</q_vat><q_subtotal>{5}</q_subtotal><q_total>{6}</q_total><qbonusam>{7}</qbonusam></step>",
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_t", i)].ToString(),
                                                            wsParameters[strXMLPrefix+string.Format("steps_step_{0}_a", i)].ToString(),
                                                            dt,
                                                            iQFEE, dQVAT, iQSubTotal, iQTotal,
                                                            Convert.ToInt32(Math.Round(iQ * 0.1, MidpointRounding.AwayFromZero)));
                                }*/
                            }

                        }
                    }

                    parametersOut["steps"] = strSteps;
                }

                    
                   



            }
            catch (Exception e)
            {
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);
                Logger_AddLogException(e, "StandardQueryParkingComputeOutput::Exception", LogLevels.logERROR);

            }


            return rtRes;

        }


        public ResultType StandardQueryUnParking(int iWSNumber,USER oUser, string strPlate, DateTime dtUnParkQuery, INSTALLATION oInstallation, ulong ulAppVersion, ref SortedList parametersOut, ref List<SortedList> lstRefunds)
        {

            ResultType rtRes = ResultType.Result_OK;
            decimal? dGroupId = null;
            decimal? dTariffId = null;
            

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true); 


                StandardParkingWS.TariffComputerWS oUnParkWS = new StandardParkingWS.TariffComputerWS();

                oUnParkWS.Url = oInstallation.INS_UNPARK_WS_URL;
                oUnParkWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oInstallation.INS_UNPARK_WS_HTTP_USER))
                {
                    oUnParkWS.Credentials = new System.Net.NetworkCredential(oInstallation.INS_UNPARK_WS_HTTP_USER, oInstallation.INS_UNPARK_WS_HTTP_PASSWORD);
                }

                DateTime dtInstallation = dtUnParkQuery;
                string strvers = "1.0";
                string strCityID = oInstallation.INS_STANDARD_CITY_ID;
                string strCompanyName = ConfigurationManager.AppSettings["STDCompanyName"].ToString();


                string strMessage = "";
                string strAuthHash = "";


                strAuthHash = CalculateStandardWSHash(oInstallation.INS_UNPARK_WS_AUTH_HASH_KEY,
                    string.Format("{0}{1}{2:HHmmssddMMyyyy}{3}{4}{5}", strCityID, strPlate, dtInstallation, oUser.USR_USERNAME, strCompanyName, strvers));

                strMessage = string.Format("<ipark_in><ins_id>{0}</ins_id><lic_pla>{1}</lic_pla><date>{2:HHmmssddMMyyyy}</date><ext_acc>{3}</ext_acc><prov>{4}</prov><vers>{5}</vers><ah>{6}</ah></ipark_in>",
                    strCityID, strPlate, dtInstallation, oUser.USR_USERNAME, strCompanyName, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("StandardQueryUnParking xmlIn={0}", sXmlIn), LogLevels.logDEBUG);

                string strOut = oUnParkWS.QueryUnParkingOperation(strMessage);
                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("StandardQueryUnParking xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);

                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {
                    if (Convert.ToInt32(wsParameters["r"].ToString()) == (int)ResultType.Result_OK)
                    {
                     
                        parametersOut["r"] = Convert.ToInt32(ResultType.Result_OK);
                        SortedList oRefund = new SortedList();

                        oRefund["p"] = strPlate;
                        oRefund["q"] = wsParameters["refunds_refund_0_ref_amount"];
                        oRefund["t"] = wsParameters["refunds_refund_0_ref_time"];
                        oRefund["cur"] = oInstallation.CURRENCy.CUR_ISO_CODE;

                        DateTime dt1 = DateTime.ParseExact(wsParameters["refunds_refund_0_d_ini"].ToString(), "HHmmssddMMyyyy",
                                    CultureInfo.InvariantCulture);
                        DateTime dt2 = DateTime.ParseExact(wsParameters["refunds_refund_0_d_end"].ToString(), "HHmmssddMMyyyy",
                                    CultureInfo.InvariantCulture);
                        DateTime dt_prev_end = DateTime.ParseExact(wsParameters["refunds_refund_0_d_prev_end"].ToString(), "HHmmssddMMyyyy",
                                    CultureInfo.InvariantCulture);

                        oRefund["d1"] = dt1.ToString("HHmmssddMMyy");
                        oRefund["d2"] = dt2.ToString("HHmmssddMMyy");
                        oRefund["d_prev_end"] = dt_prev_end.ToString("HHmmssddMMyy");
                        oRefund["exp"] = (Conversions.RoundSeconds(dtInstallation) < Conversions.RoundSeconds(dt2)) ? "0" : "1";

                        string strExtTariffId = "";
                        string strExtGroupId = "";

                        if ((wsParameters["refunds_refund_0_tar_id"] != null) && (wsParameters["refunds_refund_0_grp_id"] != null))
                        {

                            strExtTariffId = wsParameters["refunds_refund_0_tar_id"].ToString();
                            strExtGroupId = wsParameters["refunds_refund_0_grp_id"].ToString();

                            if (!geograficAndTariffsRepository.GetGroupAndTariffFromExternalId(iWSNumber, oInstallation, strExtGroupId,
                                    strExtTariffId, ref dGroupId, ref dTariffId))
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("StandardQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }

                            if (!dTariffId.HasValue)
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("StandardQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }

                            if (!dGroupId.HasValue)
                            {
                                rtRes = ResultType.Result_Error_Generic;
                                Logger_AddLogMessage("StandardQueryUnParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                                return rtRes;
                            }


                            oRefund["ad"] = dTariffId.Value;
                            oRefund["g"] = dGroupId.Value;
                        }

                        lstRefunds.Add(oRefund);
                       
                    }
                    else
                    {
                        parametersOut["r"] = Convert.ToInt32(wsParameters["r"]);
                        rtRes = (ResultType)parametersOut["r"];
                    }

                }



            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "StandardQueryUnParking::Exception", LogLevels.logERROR);

            }

            try
            {
                m_notifications.Notificate(this.GetType().GetMethod(System.Reflection.MethodBase.GetCurrentMethod().Name), rtRes, sXmlIn, sXmlOut, true, oNotificationEx);
            }
            catch
            { }


            return rtRes;

        }

        public ResultType MadridPlatformConfirmParking(int iWSNumber, string strPlate, DateTime dtParkQuery, DateTime dtUTCInsertionDateUTCDate, USER oUser, INSTALLATION oInstallation, decimal? dGroupId, decimal? dTariffId, int iQuantity, int iTime,
                                                       DateTime dtIni, DateTime dtEnd, decimal dOperationId, decimal dAuthId, ref SortedList parametersOut, out string str3dPartyOpNum, out long lEllapsedTime)
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

                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        ((sender2, certificate, chain, sslPolicyErrors) => true);

                //string strHashKey = "";

                switch (iWSNumber)
                {
                    case 1:
                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_PARK_CONFIRM_WS_URL);
                        //strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_PARK_CONFIRM_WS_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_PARK_CONFIRM_WS_HTTP_PASSWORD;
                        }
                        break;

                    case 2:
                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_PARK_CONFIRM_WS2_URL);
                        //strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_PARK_CONFIRM_WS2_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_PARK_CONFIRM_WS2_HTTP_PASSWORD;
                        }
                        break;

                    case 3:
                        oService.Endpoint.Address = new System.ServiceModel.EndpointAddress(oInstallation.INS_PARK_CONFIRM_WS3_URL);
                        //strHashKey = oInstallation.INS_PARK_CONFIRM_WS_AUTH_HASH_KEY;
                        if (!string.IsNullOrEmpty(oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER))
                        {
                            oService.ClientCredentials.UserName.UserName = oInstallation.INS_PARK_CONFIRM_WS3_HTTP_USER;
                            oService.ClientCredentials.UserName.Password = oInstallation.INS_PARK_CONFIRM_WS3_HTTP_PASSWORD;
                        }
                        break;

                    default:
                        {
                            rtRes = ResultType.Result_Error_Generic;
                            Logger_AddLogMessage("MadridPlatformConfirmParking::Error: Bad WS Number", LogLevels.logERROR);
                            return rtRes;
                        }

                }

                oService.InnerChannel.OperationTimeout = new TimeSpan(0, 0, 0, 0, Get3rdPartyWSTimeout());

                if (MadridPlatfomStartSession(oService, out oAuthSession))
                {
                    DateTime? dtUTCInstallation = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtParkQuery);
                    DateTime? dtUTCIni = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtIni);
                    DateTime? dtUTCEnd = geograficAndTariffsRepository.ConvertInstallationDateTimeToUTC(oInstallation.INS_ID, dtEnd);

                    string strExtTariffId = "";
                    string strExtGroupId = "";

                    if (!geograficAndTariffsRepository.GetGroupAndTariffExternalTranslation(iWSNumber, dGroupId.Value, dTariffId.Value, ref strExtGroupId, ref strExtTariffId))
                    {
                        rtRes = ResultType.Result_Error_Generic;
                        Logger_AddLogMessage("MadridPlatformConfirmParking::GetGroupAndTariffExternalTranslation Error", LogLevels.logERROR);
                    }

                    string sCodPhyZone = strExtGroupId.Split('~')[0];
                    string sSubBarrio = (strExtGroupId.Split('~').Length > 1 ? strExtGroupId.Split('~')[1] : "");

                    var oRequest = new MadridPlatform.PayParkingTransactionRequest()
                    {
                        PhyZone = new MadridPlatform.EntityFilterPhyZone()
                        {
                            CodSystem = oInstallation.INS_PHY_ZONE_COD_SYSTEM,
                            CodGeoZone = oInstallation.INS_PHY_ZONE_COD_GEO_ZONE,
                            CodCity = oInstallation.INS_PHY_ZONE_COD_CITY,
                            CodPhyZone = sCodPhyZone
                        },
                        PrkTrans = new MadridPlatform.PayTransactionParking()
                        {
                            AuthId = Convert.ToInt64(dAuthId),
                            //OperationDateUTC = dtUTCInstallation.Value,
                            OperationDateUTC = dtUTCInsertionDateUTCDate,
                            TariffId = Convert.ToInt32(strExtTariffId),
                            TicketNum = string.Format("91{0}00000{1}{2}{3}", sCodPhyZone, dtParkQuery.DayOfYear.ToString("000"), dtParkQuery.ToString("HHmm"), dtEnd.ToString("HHmm")),
                            TransId = Convert.ToInt64(dOperationId),
                            ParkingOper = new MadridPlatform.PayParking()
                            {
                                PrkBgnUtc = dtUTCIni.Value,
                                PrkEndUtc = dtUTCEnd.Value,
                                TotAmo = ((decimal)iQuantity / (decimal)100),
                                TotTim = new TimeSpan(0, iTime, 0)
                            },
                            UserPlate = strPlate                            
                        },
                        SubBarrio = sSubBarrio
                    };

                    strParamsIn = string.Format("sessionId={15};userName={16};" +
                                               "CodSystem={0};CodGeoZone={1};CodCity={2};CodPhyZone={3};" +
                                               "AuthId={4};OperationDateUTC={5:yyyy-MM-ddTHH:mm:ss.fff};TariffId={6};TicketNum={7};TransId={8};" +
                                               "PrkBgnUtc={9:yyyy-MM-ddTHH:mm:ss.fff};PrkEndUtc={10:yyyy-MM-ddTHH:mm:ss.fff};TotAmo={11};TotTim={12};UserPlate={13};" + 
                                               "SubBarrio={14}",
                                                oRequest.PhyZone.CodSystem, oRequest.PhyZone.CodGeoZone, oRequest.PhyZone.CodCity, oRequest.PhyZone.CodPhyZone,
                                                oRequest.PrkTrans.AuthId, oRequest.PrkTrans.OperationDateUTC, oRequest.PrkTrans.TariffId, oRequest.PrkTrans.TicketNum, oRequest.PrkTrans.TransId,
                                                oRequest.PrkTrans.ParkingOper.PrkBgnUtc, oRequest.PrkTrans.ParkingOper.PrkEndUtc, oRequest.PrkTrans.ParkingOper.TotAmo, oRequest.PrkTrans.ParkingOper.TotTim, oRequest.PrkTrans.UserPlate,
                                                oRequest.SubBarrio,
                                                oAuthSession.sessionId, oAuthSession.userName);

                    Logger_AddLogMessage(string.Format("MadridPlatformConfirmParking parametersIn={0}", strParamsIn), LogLevels.logDEBUG);

                    watch = Stopwatch.StartNew();
                    var oParkingResp = oService.SetParkingTransaction(oAuthSession, oRequest);
                    lEllapsedTime = watch.ElapsedMilliseconds;

                    strParamsOut = string.Format("Status={0};errorDetails={1}", oParkingResp.Status.ToString(), oParkingResp.errorDetails);
                    Logger_AddLogMessage(string.Format("MadridPlatformConfirmParking response={0}", strParamsOut), LogLevels.logDEBUG);

                    rtRes = (oParkingResp.Status == MadridPlatform.PublisherResponse.PublisherStatus.OK ? ResultType.Result_OK : ResultType.Result_Error_Generic);


                }
                else
                {
                    rtRes = ResultType.Result_Error_Generic;
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
                Logger_AddLogException(e, "MadridPlatformConfirmParking::Exception", LogLevels.logERROR);
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



    }
}
