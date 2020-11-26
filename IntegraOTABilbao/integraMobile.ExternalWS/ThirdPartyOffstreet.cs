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
    public enum ResultTypeMeyparOffstreetWS
    {
        ResultMOffstreet_OK = 1,
        ResultMOffstreet_Error_Generic = -9,
        ResultMOffstreet_Error_Invalid_Id = -15,
        ResultMOffstreet_Error_Invalid_Input_Parameter = -19,
        ResultMOffstreet_Error_Missing_Input_Parameter = -20,
        ResultMOffstreet_Error_OperationNotFound = -38,
        ResultMOffstreet_Error_OperationAlreadyClosed = -39,        
        ResultMOffstreet_Error_Max_Multidiscount_Reached = -54,
        ResultMOffstreet_Error_Discount_NotAllowed = -55,
        ResultMOffstreet_Error_InvoiceGeneration = -57
    }

    public class ThirdPartyOffstreet : ThirdPartyBase
    {
        public ThirdPartyOffstreet()
            : base()
        {
            m_Log = new CLogWrapper(typeof(ThirdPartyOperation));
        }

        public ResultType MeyparQueryCarExitforPayment(GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetParkingConfiguration, string sOpLogicalId, OffstreetOperationIdType oOpeIdType, string sPlate, DateTime dtCurrentDate, 
                                                       ref SortedList parametersOut, out int iOp, out int iAmount, out decimal dVAT, out string sCurIsoCode, out int iTime, out DateTime dtEntryDate, out DateTime dtEndDate,
                                                       out string sTariff, out DateTime dtExitLimitDate, out long lEllapsedTime)
        {
            ResultType rtRes = ResultType.Result_OK;
            iOp = 0;
            iAmount = 0;
            dVAT = 0;
            sCurIsoCode = "";
            iTime = 0;
            dtEntryDate = dtCurrentDate;
            dtEndDate = dtCurrentDate;
            sTariff = "";
            dtExitLimitDate = dtCurrentDate;
            lEllapsedTime = -1;
            Stopwatch watch = null;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                MeyparThirdPartyOffstreetWS.InterfazPublicaWebService oOffstreetWS = new MeyparThirdPartyOffstreetWS.InterfazPublicaWebService();
                oOffstreetWS.Url = oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_URL;
                oOffstreetWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER))
                {
                    oOffstreetWS.Credentials = new System.Net.NetworkCredential(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER, oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_PASSWORD);
                }

                string strvers = "1.0";

                string strMessage = "";
                string strAuthHash = "";

                strAuthHash = CalculateStandardWSHash(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_AUTH_HASH_KEY,
                                                      string.Format("{0}{1}{2}{3}{4:HHmmssddMMyyyy}{5}",
                                                                    oOffstreetParkingConfiguration.GROUP.GRP_QUERY_EXT_ID, sOpLogicalId, (int) oOpeIdType, sPlate, dtCurrentDate, strvers));

                strMessage = string.Format("<ipark_in>" +
                                           "<parking_id>{0}</parking_id>" +
                                           "<ope_id>{1}</ope_id>" +
                                           "<ope_id_type>{2}</ope_id_type>" +
                                           "<p>{3}</p>" +
                                           "<d>{4:HHmmssddMMyyyy}</d>" +
                                           "<vers>{5}</vers>" +
                                           "<ah>{6}</ah>" +
                                           "</ipark_in>",
                                           oOffstreetParkingConfiguration.GROUP.GRP_QUERY_EXT_ID, sOpLogicalId, (int)oOpeIdType, sPlate, dtCurrentDate, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("MeyparQueryCarExitforPayment xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oOffstreetWS.thirdpquerycarexitforpayment(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("MeyparQueryCarExitforPayment xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypeMeyparOffstreetWS_TO_ResultType((ResultTypeMeyparOffstreetWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        // ****
                        //wsParameters["q"] = "20";
                        //wsParameters["vat_perc"] = "0.21";
                        // ****

                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        parametersOut["parking_id"] = wsParameters["parking_id"];
                        parametersOut["ope_id"] = wsParameters["ope_id"];
                        parametersOut["ope_id_type"] = wsParameters["ope_id_type"];
                        parametersOut["plate"] = wsParameters["plate"];
                        parametersOut["op"] = wsParameters["op"];
                        parametersOut["q"] = wsParameters["q"];
                        parametersOut["cur"] = wsParameters["cur"];
                        parametersOut["t"] = wsParameters["t"];
                        parametersOut["bd"] = wsParameters["bd"];
                        parametersOut["ed"] = wsParameters["ed"];
                        parametersOut["tar_id"] = wsParameters["tar_id"];
                        parametersOut["med"] = wsParameters["med"];

                        iOp = Convert.ToInt32(wsParameters["op"]);                        
                        iAmount = Convert.ToInt32(wsParameters["q"]);

                        NumberFormatInfo numberFormatProvider = new NumberFormatInfo();
                        numberFormatProvider.NumberDecimalSeparator = ".";
                        string sVAT = "";
                        try
                        {
                            sVAT = wsParameters["vat_perc"].ToString();
                            if (sVAT.IndexOf(",") > 0) numberFormatProvider.NumberDecimalSeparator = ",";
                            decimal dTryVAT = Convert.ToDecimal(sVAT, numberFormatProvider);
                            dVAT = dTryVAT;
                        }
                        catch
                        {
                            dVAT = 0;
                        }
                        

                        sCurIsoCode = wsParameters["cur"].ToString();
                        iTime = Convert.ToInt32(wsParameters["t"]);
                        dtEntryDate = DateTime.ParseExact(wsParameters["bd"].ToString(), "HHmmssddMMyy",
                                                          CultureInfo.InvariantCulture);
                        dtEndDate = DateTime.ParseExact(wsParameters["ed"].ToString(), "HHmmssddMMyy",
                                                        CultureInfo.InvariantCulture);
                        sTariff = wsParameters["tar_id"].ToString();
                        dtExitLimitDate = DateTime.ParseExact(wsParameters["med"].ToString(), "HHmmssddMMyy",
                                                            CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                    }
                }
                
            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MeyparQueryCarExitforPayment::Exception", LogLevels.logERROR);
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

        public ResultType MeyparQueryCarDiscountforPayment(GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetParkingConfiguration, string sOpLogicalId, OffstreetOperationIdType oOpeIdType, string sDiscountId, DateTime dtCurrentDate,
                                                           ref SortedList parametersOut, out int iOp, out int iInitialAmount, out int iFinalAmount, out decimal dVAT, out string sCurIsoCode/*, out int iTime, out DateTime dtEntryDate, out DateTime dtEndDate,
                                                           out string sTariff, out DateTime dtExitLimitDate*/, out long lEllapsedTime)
        {
            ResultType rtRes = ResultType.Result_OK;
            iOp = 0;
            iInitialAmount = 0;
            iFinalAmount = 0;
            dVAT = 0;
            sCurIsoCode = "";
            /*iTime = 0;
            dtEntryDate = dtCurrentDate;
            dtEndDate = dtCurrentDate;
            sTariff = "";
            dtExitLimitDate = dtCurrentDate;*/
            lEllapsedTime = -1;
            Stopwatch watch = null;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                MeyparThirdPartyOffstreetWS.InterfazPublicaWebService oOffstreetWS = new MeyparThirdPartyOffstreetWS.InterfazPublicaWebService();
                oOffstreetWS.Url = oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_URL;
                oOffstreetWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER))
                {
                    oOffstreetWS.Credentials = new System.Net.NetworkCredential(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER, oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_PASSWORD);
                }

                string strvers = "1.0";

                string strMessage = "";
                string strAuthHash = "";

                strAuthHash = CalculateStandardWSHash(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_AUTH_HASH_KEY,
                                                      string.Format("{0}{1}{2}{3}{4:HHmmssddMMyyyy}{5}",
                                                                    oOffstreetParkingConfiguration.GROUP.GRP_QUERY_EXT_ID, sOpLogicalId, (int)oOpeIdType, sDiscountId, dtCurrentDate, strvers));

                strMessage = string.Format("<ipark_in>" +
                                           "<parking_id>{0}</parking_id>" +
                                           "<ope_id>{1}</ope_id>" +
                                           "<ope_id_type>{2}</ope_id_type>" +
                                           "<dc_id>{3}</dc_id>" +                                           
                                           "<d>{4:HHmmssddMMyyyy}</d>" +
                                           "<vers>{5}</vers>" +
                                           "<ah>{6}</ah>" +
                                           "</ipark_in>",
                                           oOffstreetParkingConfiguration.GROUP.GRP_QUERY_EXT_ID, sOpLogicalId, (int)oOpeIdType, sDiscountId, dtCurrentDate, strvers, strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("MeyparQueryCarDiscountforPayment xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oOffstreetWS.thirdPQueryCarDiscountforPayment(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("MeyparQueryCarDiscountforPayment xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypeMeyparOffstreetWS_TO_ResultType((ResultTypeMeyparOffstreetWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        parametersOut["parking_id"] = wsParameters["parking_id"];
                        parametersOut["ope_id"] = wsParameters["ope_id"];
                        parametersOut["ope_id_type"] = wsParameters["ope_id_type"];
                        //parametersOut["plate"] = wsParameters["p"];
                        parametersOut["op"] = wsParameters["op"];
                        parametersOut["qi"] = wsParameters["qi"];
                        parametersOut["qf"] = wsParameters["qf"];
                        parametersOut["cur"] = wsParameters["cur"];
                        //parametersOut["t"] = wsParameters["t"];
                        //parametersOut["bd"] = wsParameters["bd"];
                        //parametersOut["ed"] = wsParameters["ed"];
                        //parametersOut["tar_id"] = wsParameters["tar_id"];
                        //parametersOut["med"] = wsParameters["med"];

                        iOp = Convert.ToInt32(wsParameters["op"]);
                        iInitialAmount = Convert.ToInt32(wsParameters["qi"]);
                        iFinalAmount = Convert.ToInt32(wsParameters["qf"]);
                        sCurIsoCode = wsParameters["cur"].ToString();

                        NumberFormatInfo numberFormatProvider = new NumberFormatInfo();
                        numberFormatProvider.NumberDecimalSeparator = ".";
                        string sVAT = "";
                        try
                        {
                            sVAT = wsParameters["vat_perc"].ToString();
                            if (sVAT.IndexOf(",") > 0) numberFormatProvider.NumberDecimalSeparator = ",";
                            decimal dTryVAT = Convert.ToDecimal(sVAT, numberFormatProvider);
                            dVAT = dTryVAT;
                        }
                        catch
                        {
                            dVAT = 0;
                        }

                        /*iTime = Convert.ToInt32(wsParameters["t"]);
                        dtEntryDate = DateTime.ParseExact(wsParameters["bd"].ToString(), "HHmmssddMMyy",
                                                          CultureInfo.InvariantCulture);
                        dtEndDate = DateTime.ParseExact(wsParameters["ed"].ToString(), "HHmmssddMMyy",
                                                        CultureInfo.InvariantCulture);
                        sTariff = wsParameters["tar_id"].ToString();
                        dtExitLimitDate = DateTime.ParseExact(wsParameters["med"].ToString(), "HHmmssddMMyy",
                                                            CultureInfo.InvariantCulture);*/
                    }
                    else
                    {
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                    }
                }

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);
                Logger_AddLogException(e, "MeyparQueryCarDiscountforPayment::Exception", LogLevels.logERROR);
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

        public ResultType MeyparNotifyCarPayment(int iWSNumber, GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetParkingConfiguration, string sOpLogicalId, OffstreetOperationIdType oOpeIdType, string sPlate, int iAmount, string sCurIsoCode, int iTime, DateTime dtEntryDate, DateTime dtEndDate,
                                                 string sGate, string sTariff, decimal dOperationId, 
                                                 ref USER oUser,                                                 
                                                 ref SortedList parametersOut, out string s3dPartyOpNum, out long lEllapsedTime)
        {
            ResultType rtRes = ResultType.Result_OK;
            s3dPartyOpNum = "";
            lEllapsedTime = -1;

            Stopwatch watch = null;

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {
                MeyparThirdPartyOffstreetWS.InterfazPublicaWebService oOffstreetWS = new MeyparThirdPartyOffstreetWS.InterfazPublicaWebService();
                oOffstreetWS.Url = oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_URL;
                oOffstreetWS.Timeout = Get3rdPartyWSTimeout();

                if (!string.IsNullOrEmpty(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER))
                {
                    oOffstreetWS.Credentials = new System.Net.NetworkCredential(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_USER, oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_HTTP_PASSWORD);
                }

                string strvers = "1.0";

                string strMessage = "";
                string strAuthHash = "";

                string sUserName = oUser.CUSTOMER.CUS_NAME;
                if (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_SURNAME1)) sUserName += " " + oUser.CUSTOMER.CUS_SURNAME1;
                if (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_SURNAME2)) sUserName += " " + oUser.CUSTOMER.CUS_SURNAME2;

                string sUserAdressStreet = (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_STREET) ? oUser.CUSTOMER.CUS_STREET + " " : "");
                string sUserAddressStreetNum = oUser.CUSTOMER.CUS_STREE_NUMBER.ToString();
                if (oUser.CUSTOMER.CUS_LEVEL_NUM.HasValue) sUserAddressStreetNum += " " + oUser.CUSTOMER.CUS_LEVEL_NUM.Value.ToString();
                if (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_DOOR)) sUserAddressStreetNum += " " + oUser.CUSTOMER.CUS_DOOR;
                if (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_LETTER)) sUserAddressStreetNum += " " + oUser.CUSTOMER.CUS_LETTER;
                if (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_STAIR)) sUserAddressStreetNum += " " + oUser.CUSTOMER.CUS_STAIR;                
                string sUserAdressCity = (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_CITY) ? " " + oUser.CUSTOMER.CUS_CITY : "");
                string sUserAdressState = (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_STATE) ? " " + oUser.CUSTOMER.CUS_STATE : "");
                string sUserAdressZipCode = (!string.IsNullOrEmpty(oUser.CUSTOMER.CUS_ZIPCODE) ? " " + oUser.CUSTOMER.CUS_ZIPCODE : "");

                string sUserAddress = string.Format("{0}{1}{2}{3}{4} {5}", oUser.CUSTOMER.CUS_STREET, sUserAddressStreetNum, sUserAdressCity, sUserAdressState, sUserAdressZipCode, oUser.CUSTOMER.COUNTRy.COU_DESCRIPTION);

                

                strAuthHash = CalculateStandardWSHash(oOffstreetParkingConfiguration.GOWC_QUERY_EXIT_WS_AUTH_HASH_KEY,
                                                      string.Format("{0}{1}{2}{3}{4}{5}{6}{7:HHmmssddMMyyyy}{8:HHmmssddMMyyyy}{9}{10}{11}{12}{13}{14}{15}{16}",
                                                                    oOffstreetParkingConfiguration.GROUP.GRP_EXT1_ID, sOpLogicalId, (int)oOpeIdType, sPlate, iAmount, sCurIsoCode, iTime, dtEntryDate, dtEndDate, sGate, sTariff, dOperationId, strvers,
                                                                    oUser.USR_EMAIL, oUser.CUSTOMER.CUS_DOC_ID, sUserName, sUserAddress));

                strMessage = string.Format("<ipark_in>" +
                                           "<parking_id>{0}</parking_id>" +
                                           "<ope_id>{1}</ope_id>" +
                                           "<ope_id_type>{2}</ope_id_type>" +
                                           "<p>{3}</p>" +
                                           "<q>{4}</q>" +
                                           "<cur>{5}</cur>" +
                                           "<t>{6}</t>" +
                                           "<bd>{7:HHmmssddMMyyyy}</bd>" +
                                           "<ed>{8:HHmmssddMMyyyy}</ed>" +
                                           "<gate_id>{9}</gate_id>" +
                                           "<tar_id>{10}</tar_id>" +
                                           "<opnum>{11}</opnum>" +
                                           "<vers>{12}</vers>" +
                                           "<user_email>{13}</user_email>" +
                                           "<user_identity_card>{14}</user_identity_card>" +
                                           "<user_name>{15}</user_name>" +
                                           "<user_address>{16}</user_address>" +
                                           "<ah>{17}</ah>" +
                                           "</ipark_in>",
                                           oOffstreetParkingConfiguration.GROUP.GRP_EXT1_ID, sOpLogicalId, (int)oOpeIdType, sPlate, iAmount, sCurIsoCode, iTime, dtEntryDate, dtEndDate, sGate, sTariff, dOperationId, strvers, 
                                           oUser.USR_EMAIL, oUser.CUSTOMER.CUS_DOC_ID, sUserName, sUserAddress,
                                           strAuthHash);

                sXmlIn = PrettyXml(strMessage);

                Logger_AddLogMessage(string.Format("MeyparNotifyCarPayment xmlIn ={0}", sXmlIn), LogLevels.logDEBUG);

                watch = Stopwatch.StartNew();
                string strOut = oOffstreetWS.thirdpnotifycarpayment(strMessage);
                lEllapsedTime = watch.ElapsedMilliseconds;

                strOut = strOut.Replace("\r\n  ", "");
                strOut = strOut.Replace("\r\n ", "");
                strOut = strOut.Replace("\r\n", "");

                sXmlOut = PrettyXml(strOut);

                Logger_AddLogMessage(string.Format("MeyparNotifyCarPayment xmlOut ={0}", sXmlOut), LogLevels.logDEBUG);


                SortedList wsParameters = null;

                rtRes = FindOutParameters(strOut, out wsParameters);

                if (rtRes == ResultType.Result_OK)
                {

                    rtRes = Convert_ResultTypeMeyparOffstreetWS_TO_ResultType((ResultTypeMeyparOffstreetWS)Convert.ToInt32(wsParameters["r"].ToString()));

                    if (rtRes == ResultType.Result_OK)
                    {
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                        parametersOut["opnum"] = wsParameters["opnum"];

                        if (wsParameters.Contains("opnum") && wsParameters["opnum"] != null)
                            s3dPartyOpNum = wsParameters["opnum"].ToString();
                    }
                    else
                    {                        
                        parametersOut["r"] = Convert.ToInt32(rtRes);
                    }
                }


            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MeyparNotifyCarPayment::Exception", LogLevels.logERROR);
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

        public ResultType MeyparNotifyCarEntryManual(decimal dGroupId, string sOpLogicalId, OffstreetOperationIdType oOpeIdType, string sPlate, DateTime xEntryDate, 
                                                     string sGate, string sTariff, ref SortedList parametersOut, out DateTime xRealEntryDate, out string sGateOut, out string sTariffOut)
        {
            ResultType rtRes = ResultType.Result_OK;
            xRealEntryDate = xEntryDate;
            sGateOut = "";
            sTariffOut = "";

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {

                // ...

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MeyparNotifyCarEntryManual::Exception", LogLevels.logERROR);
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

        public ResultType MeyparGetParkingListOcuppation(List<decimal> lstGroupsIds, DateTime xEntryDate, ref SortedList parametersOut, out List<OffstreetParkingOccupation> lstParkings)
        {
            ResultType rtRes = ResultType.Result_OK;
            lstParkings = new List<OffstreetParkingOccupation>();

            string sXmlIn = "";
            string sXmlOut = "";
            Exception oNotificationEx = null;

            try
            {

                // ...

            }
            catch (Exception e)
            {
                oNotificationEx = e;
                rtRes = ResultType.Result_Error_Generic;
                parametersOut["r"] = Convert.ToInt32(rtRes);                            
                Logger_AddLogException(e, "MeyparGetParkingListOcuppation::Exception", LogLevels.logERROR);
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
