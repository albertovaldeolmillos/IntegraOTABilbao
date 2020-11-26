using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Transactions;
using System.Net;
using System.Configuration;
using System.Globalization;
using integraMobile.Infrastructure.Logging.Tools;
using integraMobile.Domain.Abstract;
using integraMobile.Domain.Helper;

namespace integraMobile.Domain.Concrete
{
    public class SQLInfraestructureRepository : IInfraestructureRepository
    {
        //Log4net Wrapper class
        private static readonly CLogWrapper m_Log = new CLogWrapper(typeof(SQLInfraestructureRepository));
        private const int ctnTransactionTimeout = 30;

        private IQueryable<COUNTRy> countriesTable;
        private IQueryable<CURRENCy> currenciesTable;
        private IQueryable<PARAMETER> parametersTable;

        private const string PARAMETER_VAT_PERC = "VAT_PERC";
        private const string PARAMETER_CHANGE_FEE_PERC = "CHANGE_FEE_PERC";


        public SQLInfraestructureRepository(string connectionString)
        {
            countriesTable = (DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>(null, connectionString)).GetTable<COUNTRy>().OrderBy(coun => coun.COU_DESCRIPTION);
            currenciesTable = (DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>(null, connectionString)).GetTable<CURRENCy>().OrderBy(coun => coun.CUR_ISO_CODE);
            parametersTable = (DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>(null, connectionString)).GetTable<PARAMETER>();
        }

        public IQueryable<COUNTRy> Countries
        {
            get { return countriesTable; }
        }


        public IQueryable<PARAMETER> Parameters
        {
            get { return parametersTable; }
        }

        public IQueryable<CURRENCy> Currencies
        {
            get { return currenciesTable; }
        }

        
        public string GetParameterValue(string strParName)
        {
            string strRes = "";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    var oResultParameters = parametersTable.Where(par => par.PAR_NAME == strParName);
                    if (oResultParameters.Count() > 0)
                    {
                        PARAMETER oParameter = oResultParameters.First();
                        strRes = oParameter.PAR_VALUE;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetParameterValue: ", e);
                strRes = "";
            }

            return strRes;
        }

        public decimal GetVATPerc()
        {
            string strRes = "";
            decimal dRes = 0;
            try
            {
                strRes = GetParameterValue(PARAMETER_VAT_PERC);
                dRes = decimal.Parse(strRes,CultureInfo.InvariantCulture);
               
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVATPerc: ", e);
                strRes = "";
            }

            return dRes;
        }

        public decimal GetChangeFeePerc()
        {
            string strRes = "";
            decimal dRes = 0;
            try
            {
                strRes = GetParameterValue(PARAMETER_CHANGE_FEE_PERC);
                dRes = decimal.Parse(strRes, CultureInfo.InvariantCulture);

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetChangeFeePerc: ", e);
                strRes = "";
            }

            return dRes;
        }

        
        public string GetCountryTelephonePrefix(int iCountryId)
        {
            string strRes = "";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    var oResultCountries = countriesTable.Where(coun => coun.COU_ID == iCountryId);
                    if (oResultCountries.Count() > 0)
                    {
                        COUNTRy oCountry = oResultCountries.First();
                        strRes = oCountry.COU_TEL_PREFIX;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCountryTelephonePrefix: ", e);
                strRes = "";
            }

            return strRes;
        }


        public int GetTelephonePrefixCountry(string strPrefix)
        {
            int iRes = -1;
            string strPrefixNorm = strPrefix.Replace("+", "").Trim() + " ";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    var oResultCountries = countriesTable.Where(coun => coun.COU_TEL_PREFIX == strPrefixNorm);
                    if (oResultCountries.Count() > 0)
                    {
                        COUNTRy oCountry = oResultCountries.First();
                        iRes = Convert.ToInt32(oCountry.COU_ID);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTelephonePrefixCountry: ", e);
                iRes = -1;                
            }

            return iRes;
        }

        public string GetCountryName(int iCountryId)
        {
            string strRes = "";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    var oResultCountries = countriesTable.Where(coun => coun.COU_ID == iCountryId);
                    if (oResultCountries.Count() > 0)
                    {
                        COUNTRy oCountry = oResultCountries.First();
                        strRes = oCountry.COU_DESCRIPTION;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCountryName: ", e);
                strRes = "";
            }

            return strRes;
        }

        public int GetCountryCurrency(int iCountryId)
        {
            int iRes = -1;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    string strApplicationCurrencyISOCode = ConfigurationManager.AppSettings["ApplicationCurrencyISOCode"];

                    if (string.IsNullOrEmpty(strApplicationCurrencyISOCode))
                    {
                        var oResultCountries = countriesTable.Where(coun => coun.COU_ID == iCountryId);
                        if (oResultCountries.Count() > 0)
                        {
                            COUNTRy oCountry = oResultCountries.First();
                            iRes = Convert.ToInt32(oCountry.COU_CUR_ID);
                        }
                    }
                    else
                    {
                        var oResultCurrencies = currenciesTable.Where(curr => curr.CUR_ISO_CODE == strApplicationCurrencyISOCode);
                        if (oResultCurrencies.Count() > 0)
                        {
                            CURRENCy oCurrency = oResultCurrencies.First();
                            iRes = Convert.ToInt32(oCurrency.CUR_ID);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCountryCurrency: ", e);
                iRes = -1;
            }

            return iRes;
        }


        public bool GetCountryPossibleSuscriptionTypes(int iCountryId, out string sSuscriptionType, out RefundBalanceType eRefundBalType)
        {
            bool bRes = false;
            sSuscriptionType = "";
            eRefundBalType = RefundBalanceType.rbtAmount;
                           
            try
            {
                   
                string sGeneralSuscriptionType = ConfigurationManager.AppSettings["SuscriptionType"] ?? "";
                eRefundBalType = (RefundBalanceType)Convert.ToInt32(ConfigurationManager.AppSettings["RefundBalanceType"] ?? "1");

                var oSuscType = countriesTable.Where(coun => coun.COU_ID == iCountryId)
                                    .First()
                                    .COUNTRIES_SUSCRIPTION_TYPEs
                                    .FirstOrDefault();

                if (oSuscType != null)
                {
                    if (oSuscType.COUSUST_SUSCR_TYPE.HasValue)
                    {
                        sSuscriptionType = oSuscType.COUSUST_SUSCR_TYPE.ToString();
                    }
                    eRefundBalType = (RefundBalanceType)oSuscType.COUSUST_REFUND_BALANCE_TYPE;
                }
                else
                {
                    sSuscriptionType = sGeneralSuscriptionType;
                }

                bRes = true;

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCountryPossibleSuscriptionTypes: ", e);
                bRes = false;
            }
         

            return bRes;

        }

        public string GetCurrencyIsoCode(int iCurrencyId)
        {
            string strRes = "";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    var oResultCurrencies = currenciesTable.Where(curr => curr.CUR_ID == iCurrencyId);
                    if (oResultCurrencies.Count() > 0)
                    {
                        CURRENCy oCurrency = oResultCurrencies.First();
                        strRes = oCurrency.CUR_ISO_CODE;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencyIsoCode: ", e);
                strRes = "";
            }

            return strRes;
        }


        public decimal GetCurrencyFromIsoCode(string strISOCode)
        {
            decimal dRes = -1;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    var oResultCurrencies = currenciesTable.Where(curr => curr.CUR_ISO_CODE == strISOCode);
                    if (oResultCurrencies.Count() > 0)
                    {
                        CURRENCy oCurrency = oResultCurrencies.First();
                        dRes = oCurrency.CUR_ID;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencyFromIsoCode: ", e);
            }

            return dRes;
        }


        public string GetCurrencyIsoCodeNumericFromIsoCode(string strISOCode)
        {
            string strRes = "";
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    var oResultCurrencies = currenciesTable.Where(curr => curr.CUR_ISO_CODE == strISOCode);
                    if (oResultCurrencies.Count() > 0)
                    {
                        CURRENCy oCurrency = oResultCurrencies.First();
                        strRes = oCurrency.CUR_ISO_CODE_NUM;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencyIsoCodeNumericFromIsoCode: ", e);
            }

            return strRes;
        }

        public int GetCurrencyDivisorFromIsoCode(string strISOCode)
        {
            int iRes = 1;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    var oResultCurrencies = currenciesTable.Where(curr => curr.CUR_ISO_CODE == strISOCode);
                    if (oResultCurrencies.Count() > 0)
                    {
                        CURRENCy oCurrency = oResultCurrencies.First();
                        iRes = Convert.ToInt32(Math.Pow(10,Convert.ToDouble(oCurrency.CUR_MINOR_UNIT)));
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencyIsoCodeNumericFromIsoCode: ", e);
            }

            return iRes;
        }


        public long SendEmailTo(string strEmailAddress, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal)
        {
            long lRes = -1;
            try
            {
                string strSenderUsername = ConfigurationManager.AppSettings["integraSenderWS_Username"];
                string strSenderPassword = ConfigurationManager.AppSettings["integraSenderWS_Password"];
                integraSenderWS.integraSender oSender = new integraSenderWS.integraSender();
                oSender.Credentials = new NetworkCredential(strSenderUsername, strSenderPassword);

                lRes = oSender.AddEmailToSendWithPriority(strSubject, strMessageBody, strEmailAddress, "", "", "", emailPriority);

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendEmailTo: ", e);
                lRes = -1;
            }

            return lRes;
        }

        public List<long> SendEmailToMultiRecipients(List<string> lstRecipients, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal)
        {
            List<long> lstRes = new List<long>();
            try
            {
                string strSenderUsername = ConfigurationManager.AppSettings["integraSenderWS_Username"];
                string strSenderPassword = ConfigurationManager.AppSettings["integraSenderWS_Password"];
                integraSenderWS.integraSender oSender = new integraSenderWS.integraSender();
                oSender.Credentials = new NetworkCredential(strSenderUsername, strSenderPassword);

                long[] arrRes = oSender.AddEmailToSendMultiRecipients(strSubject, strMessageBody, lstRecipients.ToArray(), "", "", "", emailPriority);
                if (arrRes != null) lstRes = arrRes.ToList();

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendEmailToMultiRecipients: ", e);                
            }

            return lstRes;
        }

        public long SendEmailWithAttachmentsTo(string strEmailAddress, string strSubject, string strMessageBody,List<FileAttachmentInfo> lstAttachments, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal)
        {
            long lRes = -1;
            try
            {
                string strSenderUsername = ConfigurationManager.AppSettings["integraSenderWS_Username"];
                string strSenderPassword = ConfigurationManager.AppSettings["integraSenderWS_Password"];
                integraSenderWS.integraSender oSender = new integraSenderWS.integraSender();
                oSender.Credentials = new NetworkCredential(strSenderUsername, strSenderPassword);

                if (lstAttachments != null)
                {
                    
                    if (lstAttachments.Count() > 0)
                    {
                        int i=0;
                        integraSenderWS.FileAttachmentInfo[] arrFiles = new integraSenderWS.FileAttachmentInfo[lstAttachments.Count()];

                        foreach(FileAttachmentInfo oAttach in lstAttachments)
                        {
                            integraSenderWS.FileAttachmentInfo file = new integraSenderWS.FileAttachmentInfo();
                            file.strName = oAttach.strName;
                            file.strMediaType = oAttach.strMediaType;
                            if ((oAttach.fileContent==null)&&(!string.IsNullOrEmpty(oAttach.filePath)))
                            {
                                file.fileContent = System.IO.File.ReadAllBytes(oAttach.filePath);
                            }
                            else if  ((oAttach.fileContent!=null)&&(oAttach.fileContent.Length>0))
                            {
                                file.fileContent = new byte[oAttach.fileContent.Length];
                                Array.Copy(oAttach.fileContent, file.fileContent, oAttach.fileContent.Length);
                            }
                                                        
                            arrFiles[i++]=file;
                        }


                        lRes = oSender.AddEmailWithAttachementsToSendWithPriority(strSubject, strMessageBody, strEmailAddress, arrFiles, "", "", "", emailPriority);

                    }
                    else
                    {
                        lRes = oSender.AddEmailToSendWithPriority(strSubject, strMessageBody, strEmailAddress, "", "", "", emailPriority);
                    }

                }
                else
                {
                    lRes = oSender.AddEmailToSendWithPriority(strSubject, strMessageBody, strEmailAddress, "", "", "", emailPriority);
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendEmailWithAttachmentsTo: ", e);
                lRes = -1;
            }

            return lRes;
        }

        public List<long> SendEmailWithAttachmentsToMultiRecipients(List<string> lstRecipients, string strSubject, string strMessageBody, List<FileAttachmentInfo> lstAttachments, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal)
        {
            List<long> lstRes = new List<long>();
            try
            {
                string strSenderUsername = ConfigurationManager.AppSettings["integraSenderWS_Username"];
                string strSenderPassword = ConfigurationManager.AppSettings["integraSenderWS_Password"];
                integraSenderWS.integraSender oSender = new integraSenderWS.integraSender();
                oSender.Credentials = new NetworkCredential(strSenderUsername, strSenderPassword);

                int iRecipientsBlockSize = Convert.ToInt32(ConfigurationManager.AppSettings["integraSenderWS_RecipientsBlockSize"] ?? "500");

                if (lstAttachments != null)
                {

                    if (lstAttachments.Count() > 0)
                    {
                        int i = 0;
                        integraSenderWS.FileAttachmentInfo[] arrFiles = new integraSenderWS.FileAttachmentInfo[lstAttachments.Count()];

                        foreach (FileAttachmentInfo oAttach in lstAttachments)
                        {
                            integraSenderWS.FileAttachmentInfo file = new integraSenderWS.FileAttachmentInfo();
                            file.strName = oAttach.strName;
                            file.strMediaType = oAttach.strMediaType;
                            if ((oAttach.fileContent == null) && (!string.IsNullOrEmpty(oAttach.filePath)))
                            {
                                file.fileContent = System.IO.File.ReadAllBytes(oAttach.filePath);
                            }
                            else if ((oAttach.fileContent != null) && (oAttach.fileContent.Length > 0))
                            {
                                file.fileContent = new byte[oAttach.fileContent.Length];
                                Array.Copy(oAttach.fileContent, file.fileContent, oAttach.fileContent.Length);
                            }

                            arrFiles[i++] = file;
                        }
                        
                        if (lstRecipients.Count > iRecipientsBlockSize)
                        {
                            List<string> oRecipientsBlock = null;
                            int iBlocksCount = lstRecipients.Count / iRecipientsBlockSize;
                            if ((lstRecipients.Count % iRecipientsBlockSize) > 0) iBlocksCount += 1;
                            for (int iBlock = 0; iBlock < iBlocksCount; iBlock++)
                            {
                                if (iBlock < (iBlocksCount - 1))
                                    oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, iRecipientsBlockSize);
                                else
                                    oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, lstRecipients.Count - (iBlock * iRecipientsBlockSize));
                                long[] arrRes = oSender.AddEmailWithAttachementsToSendMultiRecipients(strSubject, strMessageBody, oRecipientsBlock.ToArray(), arrFiles, "", "", "", emailPriority);
                                if (arrRes != null) lstRes = arrRes.ToList();
                            }
                        }
                        else
                        {
                            long[] arrRes = oSender.AddEmailWithAttachementsToSendMultiRecipients(strSubject, strMessageBody, lstRecipients.ToArray(), arrFiles, "", "", "", emailPriority);
                            if (arrRes != null) lstRes = arrRes.ToList();
                        }

                    }
                    else
                    {
                        if (lstRecipients.Count > iRecipientsBlockSize)
                        {
                            List<string> oRecipientsBlock = null;
                            int iBlocksCount = lstRecipients.Count / iRecipientsBlockSize;
                            if ((lstRecipients.Count % iRecipientsBlockSize) > 0) iBlocksCount += 1;
                            for (int iBlock = 0; iBlock < iBlocksCount; iBlock++)
                            {
                                if (iBlock < (iBlocksCount - 1))
                                    oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, iRecipientsBlockSize);
                                else
                                    oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, lstRecipients.Count - (iBlock * iRecipientsBlockSize));
                                long[] arrRes = oSender.AddEmailToSendMultiRecipients(strSubject, strMessageBody, oRecipientsBlock.ToArray(), "", "", "", emailPriority);
                                if (arrRes != null) lstRes = arrRes.ToList();
                            }
                        }
                        else
                        {
                            long[] arrRes = oSender.AddEmailToSendMultiRecipients(strSubject, strMessageBody, lstRecipients.ToArray(), "", "", "", emailPriority);
                            if (arrRes != null) lstRes = arrRes.ToList();
                        }
                    }

                }
                else
                {
                    if (lstRecipients.Count > iRecipientsBlockSize)
                    {
                        List<string> oRecipientsBlock = null;
                        int iBlocksCount = lstRecipients.Count / iRecipientsBlockSize;
                        if ((lstRecipients.Count % iRecipientsBlockSize) > 0) iBlocksCount += 1;
                        for (int iBlock = 0; iBlock < iBlocksCount; iBlock++)
                        {
                            if (iBlock < (iBlocksCount - 1))
                                oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, iRecipientsBlockSize);
                            else
                                oRecipientsBlock = lstRecipients.GetRange(iBlock * iRecipientsBlockSize, lstRecipients.Count - (iBlock * iRecipientsBlockSize));
                            long[] arrRes = oSender.AddEmailToSendMultiRecipients(strSubject, strMessageBody, oRecipientsBlock.ToArray(), "", "", "", emailPriority);
                            if (arrRes != null) lstRes = arrRes.ToList();
                        }
                    }
                    else
                    {
                        long[] arrRes = oSender.AddEmailToSendMultiRecipients(strSubject, strMessageBody, lstRecipients.ToArray(), "", "", "", emailPriority);
                        if (arrRes != null) lstRes = arrRes.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendEmailWithAttachmentsToMultiRecipients: ", e);                
            }

            return lstRes;
        }

        public List<long> SendEmailToMultiRecipientsTool(decimal dUniqueId, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal)
        {
            List<long> lstRes = new List<long>();

            System.Data.Common.DbCommand oCommand = null;

            try
            {

                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                oCommand = dbContext.Connection.CreateCommand();
                oCommand.CommandTimeout = 60 * 5;
                oCommand.CommandType = System.Data.CommandType.Text;
                int iCount;

                if (dbContext.Connection.State != System.Data.ConnectionState.Open) dbContext.Connection.Open();

                string sIntegraSenderDB = ConfigurationManager.AppSettings["integraSenderWS_DBName"];

                string sSQL = "INSERT INTO {0}.dbo.EMAIL_MESSAGES (EMSG_SUBJECT, EMSG_BODY, EMSG_RECIPIENT, EMSG_INSERTION_DATE, EMSG_PRIORITY) " +
                              "SELECT '{1}', '{2}', ETR_EMAIL, GETUTCDATE(), {3} " +
                              "FROM EMAILTOOL_RECIPIENTS " +
                              "WHERE ETR_ID = {4} AND ETR_EMAIL NOT LIKE '##Status_%'";

                oCommand.CommandText = string.Format(sSQL, sIntegraSenderDB, strSubject.Replace("'", "''"), strMessageBody.Replace("'", "''"), (int)emailPriority, dUniqueId);
                iCount = oCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendEmailToMultiRecipientsTool: ", e);
            }
            finally
            {
                if (oCommand != null)
                {
                    oCommand.Dispose();
                    oCommand = null;
                }
            }

            return lstRes;
        }

        public long SendSMSTo(int iCountryId, string strTelephone, string strMessage, ref string strCompleteTelephone)
        {
            long lRes = -1;
            try
            {
                string strSenderUsername = ConfigurationManager.AppSettings["integraSenderWS_Username"];
                string strSenderPassword = ConfigurationManager.AppSettings["integraSenderWS_Password"];
                integraSenderWS.integraSender oSender = new integraSenderWS.integraSender();
                oSender.Credentials = new NetworkCredential(strSenderUsername, strSenderPassword);

                strCompleteTelephone = GetCountryTelephonePrefix(iCountryId) + strTelephone;
                strCompleteTelephone = RemoveNotNumericCharacters(strCompleteTelephone);

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["integraSenderWS_SendSMSs"]))
                {
                    lRes = oSender.AddSMSToSend(strMessage, strCompleteTelephone, "", "", "");
                }
                else
                {
                    lRes = 1;
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SendSMSTo: ", e);
                lRes = -1;
            }

            return lRes;
        }


        public bool GetFirstNotGeneratedUserNotification(out USERS_NOTIFICATION notif)
        {

            bool bRes = true;
            notif = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext =new integraMobileDBEntitiesDataContext();

                        var notifs = (from r in dbContext.USERS_NOTIFICATIONs
                                      where r.UNO_STATUS == Convert.ToInt32(UserNotificationStatus.Inserted) && r.USER.USR_ENABLED == 1 &&
                                       (!r.UNO_STARTDATETIME.HasValue || r.UNO_STARTDATETIME <= DateTime.UtcNow)
                                      orderby r.UNO_ID
                                      select r);

                        if (notifs.Count() > 0)
                        {
                            notif = notifs.First();
                        }
                        else
                        {
                            dbContext.Close();
                        }
                       

                    }
                    catch(Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotGeneratedUserNotification: ", e);
                        bRes = false;

                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotGeneratedUserNotification: ", e);
                bRes = false;
            }

            return bRes;


        }



        public bool GenerateUserNotification(ref USERS_NOTIFICATION notif)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    int iNumPushGenerated = 0;
                    try
                    {

                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();
                        decimal notifId = notif.UNO_ID;


                        var oNotifs = (from r in dbContext.USERS_NOTIFICATIONs
                                       where r.UNO_ID == notifId &&
                                         r.UNO_STATUS == Convert.ToInt32(UserNotificationStatus.Inserted)
                                       select r);

                        USERS_NOTIFICATION oNotif = null;
                        if (oNotifs.Count() > 0)
                        {
                            oNotif = oNotifs.First();
                        }

                        if (oNotif != null)
                        {

                            var predicate = PredicateBuilder.True<USERS_PUSH_ID>();

                            if (oNotif.UNO_UPID_ID != null)
                            {
                                predicate = predicate.And(r => r.UPID_ID == oNotif.UNO_UPID_ID.Value);
                            }


                            var oPushIds = (from r in dbContext.USERS_PUSH_IDs
                                            where r.UPID_USR_ID == oNotif.USER.USR_ID
                                                     select r)
                                            .Where(predicate);

                            foreach (USERS_PUSH_ID oPushId in oPushIds)
                            {
                                switch ((MobileOS)oPushId.UPID_OS)
                                {
                                    case MobileOS.WindowsPhone:
                                        {

                                            if ((!string.IsNullOrEmpty(oNotif.UNO_WP_TEXT1)) ||
                                                (!string.IsNullOrEmpty(oNotif.UNO_WP_TEXT2)))
                                            {

                                                oNotif.PUSHID_NOTIFICATIONs.Add(new PUSHID_NOTIFICATION
                                                {
                                                    PNO_OS = oPushId.UPID_OS,
                                                    PNO_PUSHID = oPushId.UPID_PUSHID,
                                                    PNO_LAST_RETRY_DATETIME = null,
                                                    PNO_LIMITDATETIME = oNotif.UNO_LIMITDATETIME,
                                                    PNO_RETRIES = 0,
                                                    PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Inserted),
                                                    PNO_WP_TEXT1 = oNotif.UNO_WP_TEXT1,
                                                    PNO_WP_TEXT2 = oNotif.UNO_WP_TEXT2,
                                                    PNO_WP_PARAM = oNotif.UNO_WP_PARAM,
                                                    PNO_WP_BACKGROUND_IMAGE = "",
                                                    PNO_WP_COUNT = null,
                                                    PNO_WP_TILE_TITLE = "",
                                                    PNO_WP_RAW_DATA = "",
                                                    PNO_ANDROID_RAW_DATA = "",
                                                    PNO_iOS_RAW_DATA = ""
                                                });
                                                iNumPushGenerated++;
                                            }

                                            if ((!string.IsNullOrEmpty(oNotif.UNO_WP_TILE_TITLE)) ||
                                                (oNotif.UNO_WP_COUNT.HasValue) ||
                                                (!string.IsNullOrEmpty(oNotif.UNO_WP_BACKGROUND_IMAGE)))
                                            {

                                                oNotif.PUSHID_NOTIFICATIONs.Add(new PUSHID_NOTIFICATION
                                                {
                                                    PNO_OS = oPushId.UPID_OS,
                                                    PNO_PUSHID = oPushId.UPID_PUSHID,
                                                    PNO_LAST_RETRY_DATETIME = null,
                                                    PNO_LIMITDATETIME = oNotif.UNO_LIMITDATETIME,
                                                    PNO_RETRIES = 0,
                                                    PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Inserted),
                                                    PNO_WP_TEXT1 = "",
                                                    PNO_WP_TEXT2 = "",
                                                    PNO_WP_PARAM = "",
                                                    PNO_WP_BACKGROUND_IMAGE = oNotif.UNO_WP_BACKGROUND_IMAGE,
                                                    PNO_WP_COUNT = oNotif.UNO_WP_COUNT,
                                                    PNO_WP_TILE_TITLE = oNotif.UNO_WP_TILE_TITLE,
                                                    PNO_WP_RAW_DATA = "",
                                                    PNO_ANDROID_RAW_DATA = "",
                                                    PNO_iOS_RAW_DATA = ""
                                                });
                                                iNumPushGenerated++;
                                            }

                                            if (!string.IsNullOrEmpty(oNotif.UNO_WP_RAW_DATA))
                                            {

                                                oNotif.PUSHID_NOTIFICATIONs.Add(new PUSHID_NOTIFICATION
                                                {
                                                    PNO_OS = oPushId.UPID_OS,
                                                    PNO_PUSHID = oPushId.UPID_PUSHID,
                                                    PNO_LAST_RETRY_DATETIME = null,
                                                    PNO_LIMITDATETIME = oNotif.UNO_LIMITDATETIME,
                                                    PNO_RETRIES = 0,
                                                    PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Inserted),
                                                    PNO_WP_TEXT1 = "",
                                                    PNO_WP_TEXT2 = "",
                                                    PNO_WP_PARAM = "",
                                                    PNO_WP_BACKGROUND_IMAGE = "",
                                                    PNO_WP_COUNT = null,
                                                    PNO_WP_TILE_TITLE = "",
                                                    PNO_WP_RAW_DATA = oNotif.UNO_WP_RAW_DATA,
                                                    PNO_ANDROID_RAW_DATA = "",
                                                    PNO_iOS_RAW_DATA = ""
                                                });
                                                iNumPushGenerated++;
                                            }

                                        }
                                        break;


                                    case MobileOS.Android:
                                        {


                                            if (!string.IsNullOrEmpty(oNotif.UNO_ANDROID_RAW_DATA))
                                            {

                                                oNotif.PUSHID_NOTIFICATIONs.Add(new PUSHID_NOTIFICATION
                                                {
                                                    PNO_OS = oPushId.UPID_OS,
                                                    PNO_PUSHID = oPushId.UPID_PUSHID,
                                                    PNO_LAST_RETRY_DATETIME = null,
                                                    PNO_LIMITDATETIME = oNotif.UNO_LIMITDATETIME,
                                                    PNO_RETRIES = 0,
                                                    PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Inserted),
                                                    PNO_WP_TEXT1 = "",
                                                    PNO_WP_TEXT2 = "",
                                                    PNO_WP_PARAM = "",
                                                    PNO_WP_BACKGROUND_IMAGE = "",
                                                    PNO_WP_COUNT = null,
                                                    PNO_WP_TILE_TITLE = "",
                                                    PNO_WP_RAW_DATA = "",
                                                    PNO_ANDROID_RAW_DATA = oNotif.UNO_ANDROID_RAW_DATA,
                                                    PNO_iOS_RAW_DATA = ""
                                                });
                                                iNumPushGenerated++;
                                            }

                                        }
                                        break;
                                    case MobileOS.iOS:
                                        {


                                            if (!string.IsNullOrEmpty(oNotif.UNO_iOS_RAW_DATA))
                                            {

                                                oNotif.PUSHID_NOTIFICATIONs.Add(new PUSHID_NOTIFICATION
                                                {
                                                    PNO_OS = oPushId.UPID_OS,
                                                    PNO_PUSHID = oPushId.UPID_PUSHID,
                                                    PNO_LAST_RETRY_DATETIME = null,
                                                    PNO_LIMITDATETIME = oNotif.UNO_LIMITDATETIME,
                                                    PNO_RETRIES = 0,
                                                    PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Inserted),
                                                    PNO_WP_TEXT1 = "",
                                                    PNO_WP_TEXT2 = "",
                                                    PNO_WP_PARAM = "",
                                                    PNO_WP_BACKGROUND_IMAGE = "",
                                                    PNO_WP_COUNT = null,
                                                    PNO_WP_TILE_TITLE = "",
                                                    PNO_WP_RAW_DATA = "",
                                                    PNO_ANDROID_RAW_DATA = "",
                                                    PNO_iOS_RAW_DATA = oNotif.UNO_iOS_RAW_DATA
                                                });
                                                iNumPushGenerated++;
                                            }

                                        }
                                        break;
                                    default:
                                        break;
                                }


                            }

                            if (iNumPushGenerated > 0)
                            {
                                oNotif.UNO_STATUS = Convert.ToInt32(UserNotificationStatus.Generated);
                            }
                            else
                            {
                                oNotif.UNO_STATUS = Convert.ToInt32(UserNotificationStatus.Finished_Partially);
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();
                            notif = oNotif;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GenerateUserNotification: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GenerateUserNotification: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GenerateUserNotification: ", e);
                bRes = false;
            }


            return bRes;


        }


        public bool GetFirstNotSentNotification(out PUSHID_NOTIFICATION notif, int iResendTime)
        {

            bool bRes = true;
            notif = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                        var notifs = (from r in dbContext.PUSHID_NOTIFICATIONs
                                      where ((r.PNO_STATUS == Convert.ToInt32(PushIdNotificationStatus.Inserted)) ||
                                             ((r.PNO_LAST_RETRY_DATETIME.HasValue) &&
                                              ((DateTime.UtcNow - r.PNO_LAST_RETRY_DATETIME.Value).TotalSeconds >= iResendTime) &&
                                              (r.PNO_STATUS == Convert.ToInt32(PushIdNotificationStatus.Waiting_Next_Retry))))
                                      orderby r.PNO_ID
                                      select r);

                        if (notifs.Count() > 0)
                        {
                            notif = notifs.First();
                        }

                        if (notif != null)
                        {
                            notif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Sending);
                            SetUserNotificationStatus(notif.PNO_UTNO_ID,ref dbContext);

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                               
                                transaction.Complete();
                                dbContext.Close();
                               

                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotSentNotification: ", e);
                                bRes = false;
                            }
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotSentNotification: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotSentNotification: ", e);
                bRes = false;
            }


            return bRes;


        }


        public bool PushIdNotificationSent(decimal dPushNotifID)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                        var oNotifs = (from r in dbContext.PUSHID_NOTIFICATIONs
                                       where r.PNO_ID == dPushNotifID &&
                                         r.PNO_STATUS == Convert.ToInt32(PushIdNotificationStatus.Sending)
                                       select r);


                        PUSHID_NOTIFICATION oNotif = null;
                        if (oNotifs.Count() > 0)
                        {
                            oNotif = oNotifs.First();
                        }


                        if (oNotif != null)
                        {
                            oNotif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Sent);
                            oNotif.PNO_LAST_RETRY_DATETIME = DateTime.UtcNow;

                            var oUserPushIds = (from r in dbContext.USERS_PUSH_IDs
                                                where r.UPID_PUSHID == oNotif.PNO_PUSHID &&
                                                  r.UPID_USR_ID == oNotif.USERS_NOTIFICATION.UNO_USR_ID
                                                select r);

                            if (oUserPushIds.Count() > 0)
                            {
                                oUserPushIds.First().UPID_PUSH_RETRIES = 0;
                                oUserPushIds.First().UPID_LAST_RETRY_DATETIME = oNotif.PNO_LAST_RETRY_DATETIME;
                                oUserPushIds.First().UPID_LAST_SUCESSFUL_PUSH = oNotif.PNO_LAST_RETRY_DATETIME;

                            }

                            SetUserNotificationStatus(oNotif.PNO_UTNO_ID, ref dbContext);


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationSent: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationSent: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationSent: ", e);
                bRes = false;
            }


            return bRes;


        }



        public bool PushIdNotificationFailed(decimal dPushNotifID, int iMaxRetries)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                        var oNotifs = (from r in dbContext.PUSHID_NOTIFICATIONs
                                       where r.PNO_ID == dPushNotifID &&
                                         r.PNO_STATUS == Convert.ToInt32(PushIdNotificationStatus.Sending)
                                       select r);


                        PUSHID_NOTIFICATION oNotif = null;
                        if (oNotifs.Count() > 0)
                        {
                            oNotif = oNotifs.First();
                        }


                        if (oNotif != null)
                        {
                            oNotif.PNO_RETRIES++;
                            oNotif.PNO_LAST_RETRY_DATETIME = DateTime.UtcNow;

                            if (oNotif.PNO_RETRIES >= iMaxRetries)
                            {
                                oNotif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Failed);
                            }
                            else
                            {
                                oNotif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Waiting_Next_Retry);

                            }

                            var oUserPushIds = (from r in dbContext.USERS_PUSH_IDs
                                                where r.UPID_PUSHID == oNotif.PNO_PUSHID &&
                                                  r.UPID_USR_ID == oNotif.USERS_NOTIFICATION.UNO_USR_ID
                                                select r);

                            if (oUserPushIds.Count() > 0)
                            {
                                oUserPushIds.First().UPID_PUSH_RETRIES++;
                                oUserPushIds.First().UPID_LAST_RETRY_DATETIME = oNotif.PNO_LAST_RETRY_DATETIME;
                            }

                            SetUserNotificationStatus(oNotif.PNO_UTNO_ID,ref dbContext);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);                              
                            transaction.Complete();
                            dbContext.Close();


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationFailed: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationFailed: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "PushIdNotificationFailed: ", e);
                bRes = false;
            }


            return bRes;


        }

        public bool PushIdExpired(decimal dPushNotifID, string strNewPushId)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                          new TransactionOptions()
                                                                          {
                                                                              IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                              Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                          }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                        var oNotifs = (from r in dbContext.PUSHID_NOTIFICATIONs
                                       where r.PNO_ID == dPushNotifID &&
                                         r.PNO_STATUS == Convert.ToInt32(PushIdNotificationStatus.Sending)
                                       select r);

                        PUSHID_NOTIFICATION oNotif = null;
                        if (oNotifs.Count() > 0)
                        {
                            oNotif = oNotifs.First();
                        }


                        if (oNotif != null)
                        {
                            oNotif.PNO_RETRIES++;
                            oNotif.PNO_LAST_RETRY_DATETIME = DateTime.UtcNow;

                            var oUserPushIds = (from r in dbContext.USERS_PUSH_IDs
                                                where r.UPID_PUSHID == oNotif.PNO_PUSHID &&
                                                  r.UPID_USR_ID == oNotif.USERS_NOTIFICATION.UNO_USR_ID
                                                select r);

                            if (string.IsNullOrEmpty(strNewPushId))
                            {
                                oNotif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.SubcriptionExpired);
                                if (oUserPushIds.Count() > 0)
                                {
                                    dbContext.USERS_PUSH_IDs.DeleteOnSubmit(oUserPushIds.First());
                                }

                            }
                            else
                            {
                                if (oUserPushIds.Count() > 0)
                                {
                                    oUserPushIds.First().UPID_PUSHID = strNewPushId;
                                    oUserPushIds.First().UPID_LAST_RETRY_DATETIME = null;
                                    oUserPushIds.First().UPID_PUSH_RETRIES = 0;
                                    oUserPushIds.First().UPID_LAST_SUCESSFUL_PUSH = null;

                                }



                                oNotif.PNO_LAST_RETRY_DATETIME = DateTime.UtcNow;
                                oNotif.PNO_STATUS = Convert.ToInt32(PushIdNotificationStatus.Waiting_Next_Retry);
                                oNotif.PNO_RETRIES++;
                                oNotif.PNO_PUSHID = strNewPushId;

                            }

                            SetUserNotificationStatus(oNotif.PNO_UTNO_ID, ref dbContext);


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "PushIdExpired: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "PushIdExpired: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "PushIdExpired: ", e);
                bRes = false;
            }


            return bRes;


        }


        private bool SetUserNotificationStatus(decimal dUserNotificationID,ref integraMobileDBEntitiesDataContext dbContext)
        {
            bool bRes = true;


            try
            {
                var oPushNotifs = (from r in dbContext.PUSHID_NOTIFICATIONs
                                    where r.PNO_UTNO_ID == dUserNotificationID
                                    select r);

                int iNumSending = 0;
                int iNumSent = 0;
                int iNumFailed = 0;


                foreach (PUSHID_NOTIFICATION oPushNotif in oPushNotifs)
                {
                    switch ((PushIdNotificationStatus)oPushNotif.PNO_STATUS)
                    {
                        case PushIdNotificationStatus.Inserted:
                        case PushIdNotificationStatus.Sending:
                        case PushIdNotificationStatus.Waiting_Next_Retry:
                            iNumSending++;
                            break;
                        case PushIdNotificationStatus.Sent:
                            iNumSent++;
                            break;
                        case PushIdNotificationStatus.Failed:
                        case PushIdNotificationStatus.SubcriptionExpired:
                            iNumFailed++;
                            break;
                        default:
                            break;
                    }
                }

                UserNotificationStatus oNewStatus;

                if (iNumSending > 0)
                {
                    oNewStatus = UserNotificationStatus.Sending;
                }
                else
                {
                    if ((iNumFailed == 0) && (iNumSent > 0))
                    {
                        oNewStatus = UserNotificationStatus.Finished_Completely;
                    }
                    else
                    {
                        oNewStatus = UserNotificationStatus.Finished_Partially;
                    }
                }


                var oUserNotif = (from r in dbContext.USERS_NOTIFICATIONs
                                    where r.UNO_ID == dUserNotificationID
                                    select r).First();

                if (oNewStatus != (UserNotificationStatus)oUserNotif.UNO_STATUS)
                {
                    oUserNotif.UNO_STATUS = Convert.ToInt32(oNewStatus);
                   
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserNotificationStatus: ", e);
                bRes = false;
            }
                

            return bRes;


        }


        public bool GeneratePlatesSending()
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oDistinctURLsToSend = (from r in dbContext.INSTALLATIONs
                                                   where r.INS_PLATE_UPDATE_WS_URL != null
                                                   group r by new
                                                   {
                                                       r.INS_PLATE_UPDATE_WS_SIGNATURE_TYPE,
                                                       r.INS_PLATE_UPDATE_WS_URL
                                                   } into grp
                                                   select new
                                                   {
                                                       grp.Key.INS_PLATE_UPDATE_WS_SIGNATURE_TYPE,
                                                       grp.Key.INS_PLATE_UPDATE_WS_URL
                                                   });


                        var oPlatesPendingGeneration = (from r in dbContext.USER_PLATE_MOVs
                                                        where r.USRPM_SEND_INSERTION == 0
                                                        orderby r.USRPM_ID
                                                        select r);


                        foreach (var oURL in oDistinctURLsToSend)
                        {
                            decimal dINSId = (from r in dbContext.INSTALLATIONs
                                              where r.INS_PLATE_UPDATE_WS_URL == oURL.INS_PLATE_UPDATE_WS_URL &&
                                                    r.INS_PLATE_UPDATE_WS_SIGNATURE_TYPE == oURL.INS_PLATE_UPDATE_WS_SIGNATURE_TYPE
                                              orderby r.INS_ID
                                              select r.INS_ID).First();

                            foreach (USER_PLATE_MOV oPlate in oPlatesPendingGeneration)
                            {

                                dbContext.USER_PLATE_MOVS_SENDINGs.InsertOnSubmit(new USER_PLATE_MOVS_SENDING
                                    {
                                        USRPMS_INS_ID = dINSId,
                                        USRPMS_LAST_DATE = DateTime.UtcNow,
                                        USRPMS_USRPMD_ID = oPlate.USRPM_ID,
                                        USRPMS_STATUS = Convert.ToInt32(PlateMovSendingStatus.Inserted)
                                    });

                            }
                        }


                        foreach (USER_PLATE_MOV oPlate in oPlatesPendingGeneration)
                        {
                            oPlate.USRPM_SEND_INSERTION = 1;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GeneratePlatesSending: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GeneratePlatesSending: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GeneratePlatesSending: ", e);
                bRes = false;
            }

            return bRes;


        }

        public IEnumerable<USER_PLATE_MOVS_SENDING> GetPlatesForSending(int iMaxNumPlates)
        {
            IEnumerable<USER_PLATE_MOVS_SENDING> oPlateList=new List<USER_PLATE_MOVS_SENDING>();
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                        var oInstallationsPendingSending = (from r in dbContext.USER_PLATE_MOVS_SENDINGs
                                                            where r.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Inserted) ||
                                                                  r.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry)
                                                            group r by r.USRPMS_INS_ID into grp
                                                            select new { insID = grp.Key, minMovId = grp.Min(m => m.USRPMS_ID) })
                                                            .OrderBy(r => r.minMovId);

                        decimal dInstallation = -1;
                        decimal dMovId = -1;
                        int iStatus = -1;
                        DateTime dtMinDate = DateTime.UtcNow;

                        foreach (var oIns in oInstallationsPendingSending)
                        {
                            var oMov = (from r in dbContext.USER_PLATE_MOVS_SENDINGs
                                        where r.USRPMS_INS_ID == oIns.insID &&
                                                r.USRPMS_ID == oIns.minMovId
                                        select r).First();

                            if (dMovId == -1)
                            {
                                dInstallation = oIns.insID;
                                dMovId = oIns.minMovId;
                                iStatus = oMov.USRPMS_STATUS;
                                dtMinDate = oMov.USRPMS_LAST_DATE;
                            }
                            else if ((oMov.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Inserted)) &&
                                    (iStatus == Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry)))
                            {
                                dInstallation = oIns.insID;
                                dMovId = oIns.minMovId;
                                iStatus = oMov.USRPMS_STATUS;
                                dtMinDate = oMov.USRPMS_LAST_DATE;

                            }
                            else if ((oMov.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry)) &&
                                    (iStatus == Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry)))
                            {
                                if (oMov.USRPMS_LAST_DATE < dtMinDate)
                                {
                                    dInstallation = oIns.insID;
                                    dMovId = oIns.minMovId;
                                    iStatus = oMov.USRPMS_STATUS;
                                    dtMinDate = oMov.USRPMS_LAST_DATE;
                                }
                            }


                            if (iStatus == Convert.ToInt32(PlateMovSendingStatus.Inserted))
                            {
                                break;
                            }

                        }


                        var oMovsPendingSending = (from r in dbContext.USER_PLATE_MOVS_SENDINGs
                                                   where (r.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Inserted) ||
                                                          r.USRPMS_STATUS == Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry)) &&
                                                          r.USRPMS_INS_ID == dInstallation &&
                                                          r.USRPMS_ID >= dMovId
                                                   orderby r.USRPMS_ID
                                                   select r);


                        foreach (USER_PLATE_MOVS_SENDING oMov in oMovsPendingSending)
                        {
                            ((List<USER_PLATE_MOVS_SENDING>)oPlateList).Add(oMov);
                            oMov.USRPMS_STATUS = Convert.ToInt32(PlateMovSendingStatus.Sending);
                            if (oPlateList.Count() == iMaxNumPlates)
                            {
                                break;
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GetPlatesForSending: ", e);
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetPlatesForSending: ", e);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetPlatesForSending: ", e);
            }


            return oPlateList;

        }


        public bool ErrorSedingPlates(IEnumerable<USER_PLATE_MOVS_SENDING> oPlateList)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oPlatesInTheList = (from r in dbContext.USER_PLATE_MOVS_SENDINGs
                                                where oPlateList.Contains(r)
                                                select r);



                        foreach (USER_PLATE_MOVS_SENDING oPlate in oPlatesInTheList)
                        {
                            oPlate.USRPMS_STATUS = Convert.ToInt32(PlateMovSendingStatus.Waiting_Next_Retry);
                            oPlate.USRPMS_LAST_DATE = DateTime.UtcNow;
                        }


                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ErrorSedingPlates: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ErrorSedingPlates: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ErrorSedingPlates: ", e);
                bRes = false;
            }


            return bRes;


        }



        public bool ConfirmSentPlates(IEnumerable<USER_PLATE_MOVS_SENDING> oPlateList)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                        var oPlatesInTheList = (from r in dbContext.USER_PLATE_MOVS_SENDINGs
                                                where oPlateList.Contains(r)
                                                select r);



                        foreach (USER_PLATE_MOVS_SENDING oPlate in oPlatesInTheList)
                        {
                            oPlate.USRPMS_STATUS = Convert.ToInt32(PlateMovSendingStatus.Sent);
                            oPlate.USRPMS_LAST_DATE = DateTime.UtcNow;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ConfirmSentPlates: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ConfirmSentPlates: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConfirmSentPlates: ", e);
                bRes = false;
            }

            return bRes;


        }        



        public bool ExistPlateInSystem(string strPlate)
        {
            bool bRes = false;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    var plates = (from r in dbContext.USER_PLATEs
                                  where r.USRP_ENABLED == 1 && r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USER.USR_ENABLED == 1
                                  select r);

                    if (plates.Count() > 0)
                    {
                        bRes = true;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ExistPlateInSystem: ", e);
                bRes = false;
            }

            return bRes;
        }



        public bool AddExternalPlateFine(decimal dInstallation,
                                  string strPlate,
                                  DateTime dtTicket,
                                  DateTime dtTicketUTC,
                                  string strFineNumber,
                                  int iQuantity,
                                  DateTime dtLimit,
                                  DateTime dtLimitUTC,
                                  string strArticleType,
                                  string strArticleDescription)
        {
            bool bRes = false;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oExternalFines = (from r in dbContext.EXTERNAL_TICKETs
                                              where r.EXTI_INS_ID == dInstallation &&
                                                    r.EXTI_PLATE == strPlate &&
                                                    r.EXTI_DATE == dtTicket
                                              select r);

                        if (oExternalFines.Count() > 0)
                        {
                            bRes = true;
                        }
                        else
                        {
                            dbContext.EXTERNAL_TICKETs.InsertOnSubmit(new EXTERNAL_TICKET
                                {
                                    EXTI_INS_ID = dInstallation,
                                    EXTI_PLATE = strPlate,
                                    EXTI_DATE = dtTicket,
                                    EXTI_DATE_UTC = dtTicketUTC,
                                    EXTI_LIMIT_DATE = dtLimit,
                                    EXTI_LIMIT_DATE_UTC = dtLimitUTC,
                                    EXTI_TICKET_NUMBER = strFineNumber,
                                    EXTI_AMOUNT = iQuantity,
                                    EXTI_ARTICLE_TYPE = strArticleType,
                                    EXTI_ARTICLE_DESCRIPTION = strArticleDescription
                                });


                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();

                                bRes = true;

                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateFine: ", e);
                                bRes = false;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateFine: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateFine: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool AddExternalPlateParking(decimal dInstallation,
                                  string strPlate,
                                  DateTime dtDate,
                                  DateTime dtDateUTC,
                                  DateTime dtEndDate,
                                  DateTime dtEndDateUTC,
                                  decimal? dGroup,
                                  decimal? dTariff,
                                  DateTime? dtIniDate,
                                  DateTime? dtIniDateUTC,
                                  int? iQuantity,
                                  int? iTime,
                                  decimal dExternalProvider,
                                  OperationSourceType operationSourceType,
                                  string strSourceIdent,
                                  ChargeOperationsType chargeType,
                                  string strOperationId1, string strOperationId2,
                                  out decimal dOperationId)
        {
            bool bRes = false;
            dOperationId = 0;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oExternalParkings = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                                                 where r.EPO_INS_ID == dInstallation &&
                                                       r.EPO_PLATE == strPlate &&
                                                       ((r.EPO_DATE != null && r.EPO_DATE == dtDate) || (r.EPO_DATE == null && r.EPO_ENDDATE == dtEndDate))
                                                 select r);

                        if (oExternalParkings.Count() > 0)
                        {
                            bRes = true;
                            dOperationId = oExternalParkings.First().EPO_ID;
                        }
                        else
                        {
                            EXTERNAL_PARKING_OPERATION oExternalOperation = new EXTERNAL_PARKING_OPERATION()
                                {
                                    EPO_INS_ID = dInstallation,
                                    EPO_PLATE = strPlate,
                                    EPO_DATE = dtDate,
                                    EPO_DATE_UTC = dtDateUTC,
                                    EPO_ENDDATE = dtEndDate,
                                    EPO_ENDDATE_UTC = dtEndDateUTC,
                                    EPO_ZONE = dGroup,
                                    EPO_TARIFF = dTariff,
                                    EPO_INIDATE = dtIniDate,
                                    EPO_INIDATE_UTC = dtIniDateUTC,
                                    EPO_AMOUNT = iQuantity,
                                    EPO_TIME = iTime,
                                    EPO_EXP_ID = dExternalProvider,
                                    EPO_SRCTYPE = (int)operationSourceType,
                                    EPO_SRCIDENT = strSourceIdent,
                                    EPO_TYPE = (int)chargeType,
                                    EPO_INSERTION_UTC_DATE = DateTime.UtcNow,                                    
                                    EPO_DATE_UTC_OFFSET = Convert.ToInt32((dtDateUTC - dtDate).TotalMinutes),
                                    EPO_INIDATE_UTC_OFFSET = (dtIniDateUTC.HasValue && dtIniDate.HasValue?Convert.ToInt32((dtIniDateUTC.Value - dtIniDate.Value).TotalMinutes):0),
                                    EPO_ENDDATE_UTC_OFFSET = Convert.ToInt32((dtEndDateUTC - dtEndDate).TotalMinutes),
                                    EPO_OPERATION_ID1 = strOperationId1,
                                    EPO_OPERATION_ID2 = strOperationId2
                                };

                            dbContext.EXTERNAL_PARKING_OPERATIONs.InsertOnSubmit(oExternalOperation);


                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();

                                dOperationId = oExternalOperation.EPO_ID;

                                bRes = true;

                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateParking: ", e);
                                bRes = false;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateParking: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddExternalPlateParking: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool GetInsertionTicketNotificationData(out EXTERNAL_TICKET oTicket)
        {
            bool bRes = true;
            oTicket = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                    var oExternalTickets = (from r in dbContext.EXTERNAL_TICKETs
                                            where r.EXTI_INSERTION_NOTIFIED == 0
                                            orderby r.EXTI_ID
                                            select r);

                    if (oExternalTickets.Count() > 0)
                    {
                        oTicket = oExternalTickets.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInsertionTicketNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }



        public bool GetInsertionUserSecurityDataNotificationData(out USERS_SECURITY_OPERATION oSecurityOperation)
        {
            bool bRes = true;
            oSecurityOperation = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                    var oSecurityOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                            where r.USOP_SEND_BY_PUSH == 1 && r.USOP_UNO_ID == null
                                            orderby r.USOP_ID
                                            select r);

                    if (oSecurityOperations.Count() > 0)
                    {
                        oSecurityOperation = oSecurityOperations.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInsertionUserSecurityDataNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }



        public bool GetInsertionParkingNotificationData(out EXTERNAL_PARKING_OPERATION oParking)
        {
            bool bRes = true;
            oParking = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext =  new integraMobileDBEntitiesDataContext();

                    var oExternalParkings = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                                             where r.EPO_INSERTION_NOTIFIED == 0
                                             orderby r.EPO_ID
                                             select r);

                    if (oExternalParkings.Count() > 0)
                    {
                        oParking = oExternalParkings.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInsertionParkingNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }        
        
        
        public bool GetBeforeEndParkingNotificationData(int iNumMinutesBeforeEndToWarn, out EXTERNAL_PARKING_OPERATION oParking)
        {
            bool bRes = true;
            oParking = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext(); ;
                    TimeSpan ts = new TimeSpan(0, iNumMinutesBeforeEndToWarn, 0);

                    var oExternalParkings = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                                             where r.EPO_ENDING_NOTIFIED == 0 &&
                                                   (r.EPO_ENDDATE_UTC - DateTime.UtcNow) < ts
                                             orderby r.EPO_ENDDATE_UTC
                                             select r);

                    if (oExternalParkings.Count() > 0)
                    {
                        oParking = oExternalParkings.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetBeforeEndParkingNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool GetOffstreetOperationNotificationData(out OPERATIONS_OFFSTREET oOperation)
        {
            bool bRes = true;
            oOperation = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();                    

                    var oOperations = (from r in dbContext.OPERATIONS_OFFSTREETs
                                       where r.OPEOFF_MUST_NOTIFY == 1 && r.OPEOFF_NOTIFIED == 0
                                             orderby r.OPEOFF_EXIT_DATE
                                             select r);

                    if (oOperations.Count() > 0)
                    {
                        oOperation = oOperations.First();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOffstreetOperationNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool MarkAsGeneratedInsertionTicketNotification(EXTERNAL_TICKET oTicket)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext =new integraMobileDBEntitiesDataContext();

                        var oFoundTicket = (from r in dbContext.EXTERNAL_TICKETs
                                            where r.EXTI_ID == oTicket.EXTI_ID
                                            select r).First();


                        oFoundTicket.EXTI_INSERTION_NOTIFIED = 1;
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();
                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionTicketNotification: ", e);
                            bRes = false;
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionTicketNotification: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionTicketNotification: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool MarkAsGeneratedInsertionParkingNotificationData(EXTERNAL_PARKING_OPERATION oParking)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                        var oFoundParking = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                                             where r.EPO_ID == oParking.EPO_ID
                                             select r).First();


                        oFoundParking.EPO_INSERTION_NOTIFIED = 1;
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();

                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionParkingNotificationData: ", e);
                            bRes = false;
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionParkingNotificationData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedInsertionParkingNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool MarkAsGeneratedBeforeEndParkingNotificationData(EXTERNAL_PARKING_OPERATION oParking)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oFoundParking = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                                             where r.EPO_ID == oParking.EPO_ID
                                             select r).First();


                        oFoundParking.EPO_ENDING_NOTIFIED = 1;
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            dbContext.Close();
                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedBeforeEndParkingNotificationData: ", e);
                            bRes = false;
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedBeforeEndParkingNotificationData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedBeforeEndParkingNotificationData: ", e);
                bRes = false;
            }
            
            return bRes;
        }

        public bool MarkAsGeneratedOffstreetOperationNotificationData(OPERATIONS_OFFSTREET oOperation)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                        var oFoundOperation = (from r in dbContext.OPERATIONS_OFFSTREETs
                                               where r.OPEOFF_ID == oOperation.OPEOFF_ID
                                               select r).First();

                        oFoundOperation.OPEOFF_NOTIFIED = 1;
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedOffstreetOperationNotificationData: ", e);
                            bRes = false;
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedOffstreetOperationNotificationData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedOffstreetOperationNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool MarkAsGeneratedUserSecurityDataNotificationData(USERS_SECURITY_OPERATION oSecurityOperation, decimal oNotifID)
        {
            bool bRes = true;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                        var oFoundOperation = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                               where r.USOP_ID == oSecurityOperation.USOP_ID
                                               select r).First();

                        oFoundOperation.USERS_NOTIFICATION = (from r in dbContext.USERS_NOTIFICATIONs
                                                              where r.UNO_ID == oNotifID
                                                              select r).First();
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedUserSecurityDataNotificationData: ", e);
                            bRes = false;
                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedUserSecurityDataNotificationData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "MarkAsGeneratedUserSecurityDataNotificationData: ", e);
                bRes = false;
            }

            return bRes;
        }


       
        
        
        private void SecureSubmitChanges(ref integraMobileDBEntitiesDataContext dbContext)
        {

            try
            {
                dbContext.SubmitChanges(ConflictMode.ContinueOnConflict);
            }

            catch (ChangeConflictException e)
            {
                Console.WriteLine(e.Message);
                // Automerge database values for members that client
                // has not modified.
                foreach (ObjectChangeConflict occ in dbContext.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.KeepChanges);
                }
            }

            // Submit succeeds on second try.
            dbContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
        }


        private static string RemoveNotNumericCharacters(string str)
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= '0' && str[i] <= '9')
                    sb.Append(str[i]);
            }

            return sb.ToString();
        }

        public string GetLiteral(decimal literalId, string langCulture)
        {
            string sRes = "";
            
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();                    

                    var oLiterals = (from r in dbContext.LITERALs
                                     where r.LIT_ID == literalId 
                                     select r);

                    if (oLiterals.Count() > 0)
                    {
                        var oLiteral = oLiterals.First();
                        var oLiteralsLang = oLiteral.LITERAL_LANGUAGEs.Where(l => l.LANGUAGE.LAN_CULTURE == langCulture);
                        if (oLiteralsLang.Count() > 0)
                        {
                            sRes = oLiteralsLang.First().LITL_LITERAL;
                        }
                        else
                        {
                            sRes = oLiteral.LIT_DESCRIPTION;
                        }
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLiteral: ", e);
                sRes = "";
            }

            return sRes;

        }


        public bool getCarrouselVersion(int iVersion, int iLang, out CARROUSEL_SCREEN_VERSION oCarrouselVersion)
        {
            bool bRes = false;
            oCarrouselVersion = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                
                       oCarrouselVersion = (from r in dbContext.CARROUSEL_SCREEN_VERSIONs
                                                    where r.CASCV_VERSION_NUMBER > iVersion &&
                                                        r.CASCV_LANG == iLang
                                                    orderby r.CASCV_VERSION_NUMBER descending
                                                    select r).FirstOrDefault();
                       bRes=true;
                    

                    
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getCarrouselVersion: ", e);
            }

            return bRes;


        }




        public bool GetStripeConfiguration(string Guid, out STRIPE_CONFIGURATION oStripeConfiguration )
        {
            bool bRes = false;
            oStripeConfiguration = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oStripeConfiguration = (from r in dbContext.STRIPE_CONFIGURATIONs
                                                    where r.STRCON_GUID == Guid
                                                    select r).FirstOrDefault();
                    bRes=true;
                    

                    
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetStripeConfiguration: ", e);
            }

            return bRes;

        }

        public bool GetIECISAConfiguration(string Guid, out IECISA_CONFIGURATION oiecisaConfiguration)
        {
            bool bRes = false;
            oiecisaConfiguration = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oiecisaConfiguration = (from r in dbContext.IECISA_CONFIGURATIONs
                                            where r.IECCON_GUID == Guid
                                            select r).FirstOrDefault();
                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetIECISAConfiguration: ", e);
            }

            return bRes;

        }

        public bool GetLanguage(decimal dLanId, out LANGUAGE oLanguage)
        {
            bool bRes = false;
            oLanguage = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    oLanguage = (from r in dbContext.LANGUAGEs
                                 where r.LAN_ID == dLanId
                                 select r)
                                .FirstOrDefault();
                    bRes = (oLanguage != null);
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLanguage: ", e);
                bRes = false;
            }

            return bRes;
        }


       
        public long GetMaxVersionStreets()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREETS_SYNCs
                                     orderby r.STRSY_MOV_VERSION descending
                                     select r.STRSY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreets: ", e);
            }

            return lRes ;

        }






        public long GetMaxVersionStreetSections()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREET_SECTIONS_SYNCs
                                     orderby r.STRSESY_MOV_VERSION descending
                                     select r.STRSESY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreetSections: ", e);
            }

            return lRes ;

        }
         
        public long GetMaxVersionStreetSectionsGeometry()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREET_SECTIONS_GEOMETRY_SYNCs
                                     orderby r.STRSEGESY_MOV_VERSION descending
                                     select r.STRSEGESY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreetSectionsGeometry: ", e);
            }

            return lRes ;

        }

        public long GetMaxVersionStreetSectionsGrid()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREET_SECTIONS_GRID_SYNCs
                                            orderby r.STRSEGSY_MOV_VERSION descending
                                     select r.STRSEGSY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreetSectionsGrid: ", e);
            }

            return lRes ;

        }

        public long GetMaxVersionStreetSectionsGridGeometry()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREET_SECTIONS_GRID_GEOMETRY_SYNCs
                                     orderby r.STRSEGGSY_MOV_VERSION descending
                                     select r.STRSEGGSY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreetSectionsGridGeometry: ", e);
            }

            return lRes ;

        }

        public long GetMaxVersionStreetSectionsStreetSectionsGrid()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.STREET_SECTIONS_STREET_SECTIONS_GRID_SYNCs
                                     orderby r.STRSESSGSY_MOV_VERSION descending
                                     select r.STRSESSGSY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionStreetSectionsStreetSectionsGrid: ", e);
            }

            return lRes ;

        }


        public long GetMaxVersionTariffsInStreetSections()
        {
            long lRes = -1;
          

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dMax = (from r in dbContext.TARIFF_IN_STREETS_SECTIONS_COMPILED_SYNCs
                                     orderby r.TARSTRSECSY_MOV_VERSION descending
                                            select r.TARSTRSECSY_MOV_VERSION).FirstOrDefault();
                    if (dMax.HasValue)
                        lRes = (long)dMax.Value;
                    



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMaxVersionTariffsInStreetSections: ", e);
            }

            return lRes ;

        }

        public bool GetSyncStreets(long lVersionFrom, int iMaxRegistries, out STREETS_SYNC[] oArrSync)
        {
            bool bRes = false;
            oArrSync= null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREETS_SYNCs                                     
                                     orderby r.STRSY_MOV_VERSION
                                     where r.STRSY_MOV_VERSION > lVersionFrom
                                     select r).Take(iMaxRegistries).ToArray();


                    bRes = true;


                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreets: ", e);
            }

            return bRes;

        }

        public bool GetSyncStreetSections(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_SYNC[] oArrSync)
        {
            bool bRes = false;
            oArrSync= null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREET_SECTIONS_SYNCs                                     
                                     orderby r.STRSESY_MOV_VERSION
                                where r.STRSESY_MOV_VERSION > lVersionFrom
                                select r).Take(iMaxRegistries).ToArray();


                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreetSections: ", e);
            }

            return bRes;

        }


        public bool GetSyncStreetSectionsGeometry(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GEOMETRY_SYNC[] oArrSync)
        {
            bool bRes = false;
            oArrSync = null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREET_SECTIONS_GEOMETRY_SYNCs
                                orderby r.STRSEGESY_MOV_VERSION
                                where r.STRSEGESY_MOV_VERSION > lVersionFrom
                                select r).Take(iMaxRegistries).ToArray();


                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreetSectionsGeometry: ", e);
            }

            return bRes;

        }
        public bool GetSyncStreetSectionsGrid(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GRID_SYNC[] oArrSync)
{
            bool bRes = false;
            oArrSync= null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREET_SECTIONS_GRID_SYNCs                                     
                                     orderby r.STRSEGSY_MOV_VERSION
                                     where r.STRSEGSY_MOV_VERSION > lVersionFrom
                                     select r).Take(iMaxRegistries).ToArray();


                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreetSectionsGrid: ", e);
            }

            return bRes;

        }


        public bool GetSyncStreetSectionsGridGeometry(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GRID_GEOMETRY_SYNC[] oArrSync)
{
            bool bRes = false;
            oArrSync= null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREET_SECTIONS_GRID_GEOMETRY_SYNCs                                     
                                     orderby r.STRSEGGSY_MOV_VERSION
                                     where r.STRSEGGSY_MOV_VERSION > lVersionFrom
                                     select r).Take(iMaxRegistries).ToArray();


                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreetSectionsGridGeometry: ", e);
            }

            return bRes;

        }



        public bool GetSyncStreetSectionsStreetSectionsGrid(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_STREET_SECTIONS_GRID_SYNC[] oArrSync)
{
            bool bRes = false;
            oArrSync= null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.STREET_SECTIONS_STREET_SECTIONS_GRID_SYNCs                                     
                                     orderby r.STRSESSGSY_MOV_VERSION
                                where r.STRSESSGSY_MOV_VERSION > lVersionFrom
                                     select r).Take(iMaxRegistries).ToArray();


                    bRes = true;



                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncStreetSectionsStreetSectionsGrid: ", e);
            }

            return bRes;

        }
        public bool GetSyncTariffsInStreetSections(long lVersionFrom, int iMaxRegistries, out TARIFF_IN_STREETS_SECTIONS_COMPILED_SYNC[] oArrSync)
        {
            bool bRes = false;
            oArrSync = null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    oArrSync = (from r in dbContext.TARIFF_IN_STREETS_SECTIONS_COMPILED_SYNCs
                                orderby r.TARSTRSECSY_MOV_VERSION
                                where r.TARSTRSECSY_MOV_VERSION > lVersionFrom
                                select r).Take(iMaxRegistries).ToArray();



                    bRes = true;


                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetSyncTariffsInStreetSections: ", e);
            }

            return bRes;

        }

        public bool AddStreetSectionPackage(decimal dInstallationID, decimal id, byte[] file)
        {
            bool bRes = false;
            
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                  
                  

                    STREET_SECTIONS_PACKAGE_VERSION oVersion = new STREET_SECTIONS_PACKAGE_VERSION()
                    {
                        STSEPV_INS_ID =dInstallationID,
                        STSEPV_ID = id,
                        STSEPV_FILE = file,
                        STSEPV_UTC_DATE = DateTime.UtcNow,
                    };

                    dbContext.STREET_SECTIONS_PACKAGE_VERSIONs.InsertOnSubmit(oVersion);

                    try
                    {
                        SecureSubmitChanges(ref dbContext);                       
                        transaction.Complete();
                        bRes = true;                        
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddStreetSectionPackage: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddStreetSectionPackage: ", e);
            }

            return bRes;

        }



        
        public bool DeleteOlderStreetSectionPackage(decimal dInstallationID, decimal id)
        {
            bool bRes = false;
            
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    dbContext.STREET_SECTIONS_PACKAGE_VERSIONs.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_PACKAGE_VERSIONs.Where(c => c.STSEPV_ID < (id-5)));
                    
                    try
                    {
                        SecureSubmitChanges(ref dbContext);                       
                        transaction.Complete();
                        bRes = true;                        
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteOlderStreetSectionPackage: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteOlderStreetSectionPackage: ", e);
            }

            return bRes;

        }

        public bool GetLastStreetSectionPackageId(decimal dInstallationID, out decimal id)
        {
            bool bRes = false;
            id=-1;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    decimal? dVersion = (from r in dbContext.STREET_SECTIONS_PACKAGE_VERSIONs
                                where r.STSEPV_INS_ID == dInstallationID
                                orderby r.STSEPV_ID descending                              
                                select r.STSEPV_ID).FirstOrDefault();


                    if (dVersion.HasValue)
                    {
                        id=dVersion.Value;

                    }

                    bRes = true;


                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLastStreetSectionPackageId: ", e);
            }

            return bRes;

        }


        public bool GetLastStreetSectionPackage(decimal dInstallationID,out byte[] file)
        {
            bool bRes = false;
            file = null;


            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                    var oFile = (from r in dbContext.STREET_SECTIONS_PACKAGE_VERSIONs
                                         where r.STSEPV_INS_ID == dInstallationID
                                         orderby r.STSEPV_ID descending
                                         select r.STSEPV_FILE).FirstOrDefault();


                    if (oFile != null)
                    {
                        file = oFile.ToArray();
                    }
                    

                    bRes = true;


                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLastStreetSectionPackageId: ", e);
            }

            return bRes;

        }



    }
}
