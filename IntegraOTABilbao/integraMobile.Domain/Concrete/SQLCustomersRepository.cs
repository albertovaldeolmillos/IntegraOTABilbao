using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using integraMobile.Domain.Abstract;
using integraMobile.Domain;
using System.Data.Linq;
using System.Configuration;
using System.Transactions;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Globalization;
using System.Collections;
using integraMobile.Infrastructure.Logging.Tools;
using integraMobile.Domain.Helper;
using integraMobile.Infrastructure;

namespace integraMobile.Domain.Concrete
{   
    public class SQLCustomersRepository: ICustomersRepository
    {
        //Log4net Wrapper class
        private static readonly CLogWrapper m_Log = new CLogWrapper(typeof(SQLCustomersRepository));

        private const int ctnMaxDaysToGoBack = 30;
        private const int ctnNumRegistriesToCommitInInvoicingProcess = 200;
        private const int ctnTransactionTimeout = 30;
        private const int ctnDefaultOperationConfirmationTimeout = 60;
        private const System.String ct_INVOICE_PERIOD_TAG = "InvoiceGenerationPeriod";
        private const System.String ct_ZENDESK_TAGS_TAG = "ZendeskTags";
        private const System.String ct_ZENDESK_ORGANIZATION_ID_TAG = "ZendeskOrganizationId";
        private const int ctnCurrentVersionOfInvoice = 2;
        private DateTime ctnFirstDateOfInvoiceVersion_2 = new DateTime(2015,1,1,0,0,0);

        private const string ct_GATEWAY_LOG_ERROR_MAXRETRIES_TAG = "GatewayErrorLog_MaxRetries";
        private const string ct_GATEWAY_LOG_ERROR_RETRYTIMEOUTMILI_TAG = "GatewayErrorLog_RetryTimeoutMili";
        private const string ct_GATEWAY_LOG_ERROR_LOCKTIMEOUTSEC_TAG = "GatewayErrorLog_LockTimeoutSec";
        private const int ct_GATEWAY_LOG_ERROR_MAXRETRIES_DEFAULT = 5;
        private const int ct_GATEWAY_LOG_ERROR_RETRYTIMEOUTMILI_DEFAULT = 500;
        private const int ct_GATEWAY_LOG_ERROR_LOCKTIMEOUTSEC_DEFAULT = 30;
        
        public SQLCustomersRepository(string connectionString)
        {
        }


        public bool ExistMainTelephone(int iCountry, string strTelephone)
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



                     var oUser = (from r in dbContext.USERs
                                  where r.USR_MAIN_TEL_COUNTRY == iCountry && r.USR_MAIN_TEL == strTelephone && r.USR_ENABLED == 1
                                  select r).ToArray();
                     if (oUser.Count() == 0)
                     {

                         var oCustInsc = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                          where r.CUSINS_MAIN_TEL_COUNTRY == iCountry &&
                                                r.CUSINS_MAIN_TEL == strTelephone
                                          orderby r.CUSINS_LAST_SENT_DATE descending
                                          select r).ToArray();

                         if (oCustInsc.Count() > 0)
                         {
                             bRes = !IsCustomerInscriptionExpired(oCustInsc[0]);
                         }

                         if (!bRes)
                         {
                             var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                                   where r.USOP_NEW_MAIN_TEL_COUNTRY == iCountry &&
                                                         r.USOP_NEW_MAIN_TEL == strTelephone &&
                                                         r.USOP_OP_TYPE == (int)SecurityOperationType.ChangeEmail_Telephone &&
                                                         r.USOP_STATUS == (int)SecurityOperationStatus.Inserted &&
                                                         r.USER.USR_ENABLED == 1
                                                   orderby r.USOP_LAST_SENT_DATE descending
                                                   select r).ToArray();


                             if (oSecOperations.Count() > 0)
                             {
                                 bRes = !IsUserSecurityOperationExpired(oSecOperations[0]);
                             }


                         }


                     }
                     else
                     {
                         bRes = true;
                     }
                     transaction.Complete();
                 }
             }
             catch (Exception e)
             {
                 m_Log.LogMessage(LogLevels.logERROR, "ExistMainTelephone: ", e);
             }

             return bRes;


        }

        public bool ExistEmail(string strEmail)
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

                    var oUser = (from r in dbContext.USERs
                                 where (r.USR_EMAIL == strEmail || r.USR_USERNAME == strEmail) && r.USR_ENABLED == 1
                                 select r).ToArray();

                    if (oUser.Count() == 0)
                    {



                        var oCustInsc = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                         where r.CUSINS_EMAIL == strEmail
                                         orderby r.CUSINS_LAST_SENT_DATE descending
                                         select r).ToArray();

                        if (oCustInsc.Count() > 0)
                        {
                            bRes = !IsCustomerInscriptionExpired(oCustInsc[0]);
                        }

                        if (!bRes)
                        {
                            var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                                  where r.USOP_NEW_EMAIL == strEmail &&
                                                        r.USOP_OP_TYPE == (int)SecurityOperationType.ChangeEmail_Telephone &&
                                                        r.USOP_STATUS == (int)SecurityOperationStatus.Inserted &&
                                                        r.USER.USR_ENABLED == 1
                                                  orderby r.USOP_LAST_SENT_DATE descending
                                                  select r).ToArray();


                            if (oSecOperations.Count() > 0)
                            {
                                bRes = !IsUserSecurityOperationExpired(oSecOperations[0]);
                            }


                        }

                    }
                    else
                    {
                        bRes = true;
                    }
                    transaction.Complete();
                }
                

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ExistEmail: ", e);
            }

            return bRes;           


        }


        public bool ExistUsername(string strUsername)
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

                    var oUsers = (from r in dbContext.USERs
                                  where (r.USR_USERNAME == strUsername || r.USR_EMAIL == strUsername) && r.USR_ENABLED == 1
                                  select r);

                    bRes = (oUsers.Count() > 0);
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ExistUsername: ", e);
            }

            return bRes;


        }

        public bool AddCustomerInscription(ref CUSTOMER_INSCRIPTION custInsc)
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
                        int iNumCharactersActivationSMS = Convert.ToInt32(ConfigurationManager.AppSettings["NumCharactersActivationSMS"]);
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        custInsc.CUSINS_ACTIVATION_CODE = GenerateRandom(iNumCharactersActivationSMS);
                        custInsc.CUSINS_URL_PARAMETER = GenerateId() + GenerateId() + GenerateId();
                        custInsc.CUSINS_ACTIVATION_RETRIES = 0;
                        dbContext.CUSTOMER_INSCRIPTIONs.InsertOnSubmit(custInsc);

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddCustomerInscription: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddCustomerInscription: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddCustomerInscription: ", e);
                bRes = false;
            }
            
            return bRes;

        }


        public bool GetCustomerInscription(ref CUSTOMER_INSCRIPTION custInsc, decimal dCustInscId)
        {
            bool bRes = true;
            custInsc = null;
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

                        var oCustomers = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                          where r.CUSINS_ID == dCustInscId
                                          select r);

                        if (oCustomers.Count() > 0)
                        {
                            custInsc = oCustomers.First();
                        }
                        else
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInscription: Customer inscription not found");
                            bRes = false;
                        }

                        transaction.Complete();
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInscription: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInscription: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool UpdateActivationRetries(ref CUSTOMER_INSCRIPTION custInsc)
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

                        decimal cusId = custInsc.CUSINS_ID;
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oCustomers = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                         where r.CUSINS_ID == cusId
                                         select r);

                        if (oCustomers.Count() > 0)
                        {
                            var oCustomer = oCustomers.First();

                            if (oCustomer != null)
                            {
                                oCustomer.CUSINS_ACTIVATION_RETRIES++;
                                oCustomer.CUSINS_LAST_SENT_DATE = DateTime.UtcNow;

                            }

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();
                                custInsc = oCustomer;


                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "UpdateActivationRetries: ", e);
                                bRes = false;
                            }
                        }
                        else
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateActivationRetries: Customer inscription not found");
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateActivationRetries: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateActivationRetries: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool IsCustomerInscriptionExpired(CUSTOMER_INSCRIPTION custInsc)
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
                    int iNumSecondsTimeoutActivationSMS = Convert.ToInt32(ConfigurationManager.AppSettings["NumSecondsTimeoutActivationSMS"]);


                    if (custInsc != null)
                    {
                        if (custInsc.CUSINS_LAST_SENT_DATE.HasValue)
                        {
                            DateTime dtLastSentDate = (DateTime)custInsc.CUSINS_LAST_SENT_DATE;
                            double dTotalSeconds = (DateTime.UtcNow - dtLastSentDate).TotalSeconds;

                            bRes = (dTotalSeconds >= iNumSecondsTimeoutActivationSMS);

                        }
                    }
                    transaction.Complete();

                }

               

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsCustomerInscriptionExpired: ", e);
            }

            return bRes;


        }


        public bool IsUserSecurityOperationExpired(USERS_SECURITY_OPERATION oSecOperation)
        {

            int iNumSecondsTimeoutActivationSMS = Convert.ToInt32(ConfigurationManager.AppSettings["NumSecondsTimeoutActivationSMS"]);
            return  IsUserSecurityOperationExpired(oSecOperation, iNumSecondsTimeoutActivationSMS);

        }


        public bool IsUserSecurityOperationExpired(USERS_SECURITY_OPERATION oSecOperation, int iMaxSecondsToActivate)
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
                    
                    if (oSecOperation != null)
                    {
                        if (oSecOperation.USOP_LAST_SENT_DATE.HasValue)
                        {
                            DateTime dtLastSentDate = (DateTime)oSecOperation.USOP_LAST_SENT_DATE.Value;
                            double dTotalSeconds = (DateTime.UtcNow - dtLastSentDate).TotalSeconds;

                            bRes = (dTotalSeconds >= iMaxSecondsToActivate);

                        }
                    }
                    transaction.Complete();

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsUserSecurityOperationExpired: ", e);
            }

            return bRes;


        }


        public bool IsCustomerInscriptionAlreadyUsed(CUSTOMER_INSCRIPTION custInsc)
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

                    var oCustomers = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                      where r.CUSINS_ID == custInsc.CUSINS_ID &&
                                            r.CUISINS_CUS_ID != null
                                      select r);

                    bRes = (oCustomers.Count() > 0);
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsCustomerInscriptionAlreadyUsed: ", e);
            }

            return bRes;


        }


        public bool IsUserSecurityOperationAlreadyUsed(USERS_SECURITY_OPERATION oSecOperation)
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

                    var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                          where r.USOP_ID == oSecOperation.USOP_ID &&
                                                r.USOP_STATUS == (int)SecurityOperationStatus.Confirmed
                                          select r);

                    bRes = (oSecOperations.Count() > 0);
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsUserSecurityOperationAlreadyUsed: ", e);
            }

            return bRes;


        }



        public CUSTOMER_INSCRIPTION GetCustomerInscriptionData(string urlParameter)
        {
            CUSTOMER_INSCRIPTION oCustomerRes = null;
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

                    var oCustomers = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                      where r.CUSINS_URL_PARAMETER == urlParameter
                                      orderby r.CUSINS_LAST_SENT_DATE descending
                                      select r);


                    if (oCustomers.Count() > 0)
                    {
                        oCustomerRes = oCustomers.First();
                    }
                    transaction.Complete();

                }
               
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInscriptionData: ", e);
                oCustomerRes = null;
            }

            return oCustomerRes;

        }


        public bool InsertCustomerSMS( CUSTOMER_INSCRIPTION custInsc,string strTelephone, string strMessage, long lSenderId )
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

                        decimal cusId = custInsc.CUSINS_ID;
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        USERS_SMSS oSMS = new USERS_SMSS
                        {
                            USRS_CUSINS_ID = cusId,
                            USRS_DATE = DateTime.UtcNow,
                            USRS_RECIPIENT_TELEPHONE = strTelephone,
                            USRS_SMS = strMessage,
                            USRS_SENDER_ID = lSenderId

                        };

                        // Add the new object to the Orders collection.
                        dbContext.USERS_SMSSes.InsertOnSubmit(oSMS);

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerSMS: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerSMS: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerSMS: ", e);
                bRes = false;
            }
            
            return bRes;

        }

        public bool InsertCustomerEmail(CUSTOMER_INSCRIPTION custInsc, string strEmailAddress, string strSubject, string strMessageBody, long lSenderId)
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

                        decimal cusId = custInsc.CUSINS_ID;
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        USERS_EMAIL oEmail = new USERS_EMAIL
                        {
                            USRE_CUSINS_ID = cusId,
                            USRE_DATE = DateTime.UtcNow,
                            USRE_RECIPIENT_ADDRESS = strEmailAddress,
                            USRE_SUBJECT = strSubject,
                            USRE_BODY = strMessageBody,
                            USRE_SENDER_ID = lSenderId

                        };

                        // Add the new object to the Orders collection.
                        dbContext.USERS_EMAILs.InsertOnSubmit(oEmail);


                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerEmail: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerEmail: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InsertCustomerEmail: ", e);
                bRes = false;
            }
            
            return bRes;

        }


        public bool AddUser(ref USER user, decimal? custInscId)
        {
            bool bRes = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        dbContext.USERs.InsertOnSubmit(user);
                        SecureSubmitChanges(ref dbContext);

                        user.CUSTOMER.CUS_USR_ID = user.USR_ID;
                        //user.USR_CUSPM_ID = user.CUSTOMER.CUSTOMER_PAYMENT_MEANs[0].CUSPM_ID;

                        if (custInscId.HasValue)
                        {
                            var custInsc = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                                            where r.CUSINS_ID == custInscId
                                            select r).First();


                            custInsc.CUISINS_CUS_ID = user.CUSTOMER.CUS_ID;

                            var userSMSs = (from r in dbContext.USERS_SMSSes
                                            where r.USRS_CUSINS_ID == custInscId
                                            select r);

                            foreach (USERS_SMSS userSMS in userSMSs)
                            {
                                userSMS.USRS_CUSINS_ID = null;
                                userSMS.USRS_USR_ID = user.USR_ID;

                            }

                            /*var userEmails = (from r in dbContext.USERS_EMAILs
                                              where r.USRE_CUSINS_ID == custInscId
                                              select r);

                            foreach (USERS_EMAIL userEmail in userEmails)
                            {
                                userEmail.USRE_CUSINS_ID = null;
                                userEmail.USRE_USR_ID = user.USR_ID;
                            }*/
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();
                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddUser: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddUser: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool DeleteNonActivatedUser(string strEmail, int iNumMaxMinutesForActivation, out bool bDeleteMembership)
        {

            bool bRes = false;
            bDeleteMembership = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_EMAIL == strEmail && r.USR_ENABLED == 1 && r.USR_ACTIVATED == 0 &&
                                           (DateTime.UtcNow - r.USR_INSERT_UTC_DATE).TotalMinutes >= iNumMaxMinutesForActivation                                            
                                     select r).First();

                        if (oUser != null)
                        {
                            foreach (USER_PLATE oPlate in oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1))
                            {                               
                                oPlate.USRP_ENABLED = 0;                               
                            }
                          
                            foreach (CUSTOMER_PAYMENT_MEAN oPaymentMean in oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(r => r.CUSPM_ENABLED == 1))
                            {
                                oPaymentMean.CUSPM_ENABLED = 0;
                                if (((PaymentMeanCreditCardProviderType)oPaymentMean.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                                                  PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE)))
                                {
                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oPaymentMean.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oUser.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }
                            }

                            foreach (CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge in oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Where(r => r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Committed))
                            {
                                oRecharge.CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Refund;
                            }

                            foreach (CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge in oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Where(r => r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Commit))
                            {
                                oRecharge.CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Cancellation;
                            }



                            oUser.USR_ENABLED = 0;
                            oUser.USR_DISABLE_UTC_DATE = DateTime.UtcNow;

                            if (oUser.CUSTOMER.CUS_TYPE == (int)CustomerType.Individual)
                            {
                                oUser.CUSTOMER.CUS_ENABLED = 0;
                            }

                            bDeleteMembership = true;

                            bRes = true;
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteNonActivatedUser: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteNonActivatedUser: ", e);
                bRes = false;
            }

            return bRes;



        }


        public bool UpdateUser(ref USER user, IList<string> Plates)
        {
            bool bRes = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser != null)
                        {

                            oUser.USR_USERNAME = user.USR_USERNAME;
                            oUser.CUSTOMER.CUS_NAME = user.CUSTOMER.CUS_NAME;
                            oUser.CUSTOMER.CUS_SURNAME1 = user.CUSTOMER.CUS_SURNAME1;
                            oUser.CUSTOMER.CUS_SURNAME2 = user.CUSTOMER.CUS_SURNAME2;
                            oUser.CUSTOMER.CUS_DOC_ID = user.CUSTOMER.CUS_DOC_ID;
                            oUser.USR_SECUND_TEL_COUNTRY = user.USR_SECUND_TEL_COUNTRY;
                            oUser.USR_SECUND_TEL = user.USR_SECUND_TEL;
                            oUser.CUSTOMER.CUS_STREET =user.CUSTOMER.CUS_STREET;
                            oUser.CUSTOMER.CUS_STREE_NUMBER = user.CUSTOMER.CUS_STREE_NUMBER;
                            oUser.CUSTOMER.CUS_LEVEL_NUM = user.CUSTOMER.CUS_LEVEL_NUM;
                            oUser.CUSTOMER.CUS_DOOR = user.CUSTOMER.CUS_DOOR;
                            oUser.CUSTOMER.CUS_LETTER = user.CUSTOMER.CUS_LETTER;
                            oUser.CUSTOMER.CUS_STAIR = user.CUSTOMER.CUS_STAIR;
                            oUser.CUSTOMER.CUS_COU_ID = user.CUSTOMER.CUS_COU_ID;
                            oUser.CUSTOMER.CUS_STATE = user.CUSTOMER.CUS_STATE;
                            oUser.CUSTOMER.CUS_CITY = user.CUSTOMER.CUS_CITY;
                            oUser.CUSTOMER.CUS_ZIPCODE = user.CUSTOMER.CUS_ZIPCODE;

                            //Delete table plates
                            foreach (USER_PLATE oPlate in oUser.USER_PLATEs.Where(r=> r.USRP_ENABLED==1))
                            {
                                int iCount=Plates.Where(s => s == oPlate.USRP_PLATE).Count();

                                if (iCount == 0)
                                {
                                    oPlate.USRP_ENABLED = 0;
                                }
                            }


                            //Insert table plates

                            foreach (string strPlate in Plates)
                            {
                                USER_PLATE oPlate=null;

                                
                                try
                                {
                                    oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate).First();
                                }
                                catch{}

                                if (oPlate != null)
                                {
                                    if (oPlate.USRP_ENABLED == 0)
                                    {
                                        oPlate.USRP_ENABLED = 1;
                                    }
                                }
                                else
                                {
                                    oUser.USER_PLATEs.Add(new USER_PLATE
                                    {
                                        USRP_PLATE = strPlate,
                                        USRP_ENABLED =1,
                                        USRP_IS_DEFAULT=0,
                                    });

                                }


                            }


                            bRes = true;
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();
                        user = oUser;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateUser: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateUser: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool InsertUserEmail(ref USER user, string strEmailAddress, string strSubject, string strMessageBody, long lSenderId)
        {
            bool bRes = true;
            /*try
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser != null)
                        {
                            oUser.USERS_EMAILs.Add(new USERS_EMAIL()
                            {
                                USRE_USR_ID = userId,
                                USRE_DATE = DateTime.UtcNow,
                                USRE_RECIPIENT_ADDRESS = strEmailAddress,
                                USRE_SUBJECT = strSubject,
                                USRE_BODY = strMessageBody,
                                USRE_SENDER_ID = lSenderId
                            });

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                bRes = false;
            }*/

            return bRes;

        }


        public bool InsertUserSMS(ref USER user, string strTelephone, string strMessage, long lSenderId)
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser != null)
                        {
                            USERS_SMSS oSMS = new USERS_SMSS
                            {
                                USRS_USR_ID = userId,
                                USRS_DATE = DateTime.UtcNow,
                                USRS_RECIPIENT_TELEPHONE = strTelephone,
                                USRS_SMS = strMessage,
                                USRS_SENDER_ID = lSenderId

                            };

                            // Add the new object to the Orders collection.
                            dbContext.USERS_SMSSes.InsertOnSubmit(oSMS);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InsertUserEmail: ", e);
                bRes = false;
            }

            return bRes;

        }



       

        public bool GetUserData(ref USER user,string username)
        {
            bool bRes = false;
            user = null;
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

                    //var users_L = (from r in dbContext.USERs where r.USR_ENABLED == 1 select r.USR_USERNAME).Distinct().ToList();

                    var users = (from r in dbContext.USERs
                                 where r.USR_USERNAME == username && r.USR_ENABLED == 1
                                 select r);

                    if (users.Count() > 0)
                    {
                        user = users.First();
                    }
                    bRes = (user != null);

                    transaction.Complete();
                }
            }
            catch(Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserData: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool RenewUserData(ref USER user)
        {
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
                        decimal userId = user.USR_ID;

                        var users = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r);

                        if (users.Count() > 0)
                        {
                            user = users.First();
                        }
                        bRes = (user != null);

                        transaction.Complete();
                    }

                }
                catch (Exception e)
                {
                    m_Log.LogMessage(LogLevels.logERROR, "GetUserData: ", e);
                    bRes = false;
                }

                return bRes;

            }
        }


        public bool GetUserDataByEmail(ref USER user, string email)
        {
            bool bRes = false;
            user = null;
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

                    var users = (from r in dbContext.USERs
                                 where r.USR_EMAIL == email && r.USR_ENABLED == 1
                                 select r);

                    if (users.Count() > 0)
                    {
                        user = users.First();
                    }

                    bRes = (user != null);

                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserDataByEmail: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool GetUserDataById(ref USER user, decimal dId)
        {
            bool bRes = false;
            user = null;
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

                    var users = (from r in dbContext.USERs
                                 where r.USR_ID == dId && r.USR_ENABLED == 1
                                 select r);

                    if (users.Count() > 0)
                    {
                        user = users.First();
                    }

                    bRes = (user != null);

                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserDataById: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool SetUserCultureLangAndUTCOffest(ref USER user, string strCultureLang,int iUTCOffset)
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

                        decimal dUserId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == dUserId && r.USR_ENABLED == 1 
                                     select r).First();

                        oUser.USR_CULTURE_LANG = strCultureLang;
                        oUser.USR_UTC_OFFSET = iUTCOffset;

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserCultureLang: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserCultureLang: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool SetUserUTCOffest(ref USER user, int iUTCOffset)
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

                        decimal dUserId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == dUserId && r.USR_ENABLED == 1
                                     select r).First();

                        oUser.USR_UTC_OFFSET = iUTCOffset;

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserUTCOffest: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserUTCOffest: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool SetUserCulture(ref USER user, string strCulture)
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

                        decimal dUserId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == dUserId && r.USR_ENABLED == 1
                                     select r).First();

                        oUser.USR_CULTURE_LANG = strCulture;

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserCulture: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserCulture: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool SetUserPagateliaLastCredentials(ref USER user, string sPagateliaUser, string sPagateliaPwd)
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

                        decimal dUserId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == dUserId && r.USR_ENABLED == 1
                                     select r).First();

                        oUser.USR_PAGATELIA_LAST_USER = sPagateliaUser;
                        oUser.USR_PAGATELIA_LAST_PWD = sPagateliaPwd;

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserPagateliaLastCredentials: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserPagateliaLastCredentials: ", e);
                bRes = false;
            }

            return bRes;

        }

/*
        public bool DeleteUser(ref USER user)
        {
            bool bRes = false;
            try
            {
                using (var transaction = new TransactionScope())
                {
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        oUser.CUSTOMER.USER = null;
                        CUSTOMER cust = oUser.CUSTOMER;
                        dbContext.USER_PLATEs.DeleteAllOnSubmit(oUser.USER_PLATEs);
                        dbContext.USERS_SMSSes.DeleteAllOnSubmit(oUser.USERS_SMSSes);
                        dbContext.USERS_EMAILs.DeleteAllOnSubmit(oUser.USERS_EMAILs);

                        foreach (OPERATION reg in oUser.OPERATIONs)
                        {
                            reg.OPE_CUSPMR_ID = null;
                            reg.OPE_OPEDIS_ID = null;
                        }

                        foreach (TICKET_PAYMENT reg in oUser.TICKET_PAYMENTs)
                        {
                            reg.TIPA_CUSPMR_ID = null;
                            reg.TIPA_OPEDIS_ID = null;
                        }


                        foreach (SERVICE_CHARGE reg in oUser.SERVICE_CHARGEs)
                        {
                            reg.SECH_CUSPMR_ID = null;
                        }


                        foreach (CUSTOMER_PAYMENT_MEANS_RECHARGE recharge in oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs)
                        {
                            recharge.CUSPMR_USR_ID = null;
                        }

                        SecureSubmitChanges(ref dbContext);
                        
                        dbContext.OPERATIONs.DeleteAllOnSubmit(oUser.OPERATIONs);
                        dbContext.TICKET_PAYMENTs.DeleteAllOnSubmit(oUser.TICKET_PAYMENTs);
                        dbContext.SERVICE_CHARGEs.DeleteAllOnSubmit(oUser.SERVICE_CHARGEs);
                        dbContext.OPERATIONS_DISCOUNTs.DeleteAllOnSubmit(oUser.OPERATIONS_DISCOUNTs);


                        foreach (MOBILE_SESSION session in oUser.MOBILE_SESSIONs)
                        {
                            dbContext.RECHARGE_COUPONS_USEs.DeleteAllOnSubmit(session.RECHARGE_COUPONS_USEs);
                            dbContext.OPERATIONS_SESSION_INFOs.DeleteAllOnSubmit(session.OPERATIONS_SESSION_INFOs);
                            dbContext.TICKET_PAYMENTS_SESSION_INFOs.DeleteAllOnSubmit(session.TICKET_PAYMENTS_SESSION_INFOs);
                        }


                        dbContext.MOBILE_SESSIONs.DeleteAllOnSubmit(oUser.MOBILE_SESSIONs);
                        dbContext.USERS_PUSH_IDs.DeleteAllOnSubmit(oUser.USERS_PUSH_IDs);
                        foreach (USERS_NOTIFICATION oNotif in oUser.USERS_NOTIFICATIONs)
                        {
                            dbContext.PUSHID_NOTIFICATIONs.DeleteAllOnSubmit(oNotif.PUSHID_NOTIFICATIONs);
                        }
                        dbContext.USERS_NOTIFICATIONs.DeleteAllOnSubmit(oUser.USERS_NOTIFICATIONs);



                        dbContext.USERs.DeleteOnSubmit(oUser);
                        SecureSubmitChanges(ref dbContext);



                        dbContext.CUSTOMER_INSCRIPTIONs.DeleteAllOnSubmit(cust.CUSTOMER_INSCRIPTIONs);
                        dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.DeleteAllOnSubmit(cust.CUSTOMER_PAYMENT_MEANS_RECHARGEs);
                        dbContext.CUSTOMER_PAYMENT_MEANs.DeleteAllOnSubmit(cust.CUSTOMER_PAYMENT_MEANs);
                        dbContext.CUSTOMERs.DeleteOnSubmit(cust);

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        user = null;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteUser: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteUser: ", e);
                bRes = false;
            }

            return bRes;

        }

 */


        public bool DeleteUser(ref USER user)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {
                            foreach (USER_PLATE oPlate in oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1))
                            {
                                oPlate.USRP_ENABLED = 0;
                            }


                            foreach (CUSTOMER_PAYMENT_MEAN oPaymentMean in oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(r => r.CUSPM_ENABLED == 1))
                            {
                                oPaymentMean.CUSPM_ENABLED = 0;
                                if (((PaymentMeanCreditCardProviderType)oPaymentMean.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                                                   PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE)))
                                {                                    
                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oPaymentMean.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oUser.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }


                            }

                            foreach (MOBILE_SESSION oSession in oUser.MOBILE_SESSIONs.Where(r => r.MOSE_STATUS == (int)MobileSessionStatus.Open))
                            {
                                oSession.MOSE_STATUS = (int)MobileSessionStatus.Closed;
                            }
                            
                            oUser.USR_ENABLED = 0;
                            oUser.USR_DISABLE_UTC_DATE = DateTime.UtcNow;

                            if (oUser.CUSTOMER.CUS_TYPE == (int)CustomerType.Individual)
                            {
                                oUser.CUSTOMER.CUS_ENABLED = 0;
                            }
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteUser: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteUser: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool SetUserSuscriptionType(ref USER user, PaymentSuscryptionType pst)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();


                        oUser.USR_SUSCRIPTION_TYPE = (int)pst;
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserSuscriptionType: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserSuscriptionType: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool SetUserRefundBalanceType(ref USER user, RefundBalanceType eRefundBalType)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();


                        oUser.USR_REFUND_BALANCE_TYPE = (int)eRefundBalType;
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserSuscriptionType: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserSuscriptionType: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetUserPossibleSuscriptionTypes(ref USER user, 
                                                    IInfraestructureRepository infrastructureRepository, 
                                                    out string sSuscriptionType,
                                                    out RefundBalanceType eRefundBalType)
        {
            bool bRes = false;
            sSuscriptionType="";
            eRefundBalType = RefundBalanceType.rbtAmount;
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        infrastructureRepository.GetCountryPossibleSuscriptionTypes(Convert.ToInt32(oUser.COUNTRy.COU_ID), out sSuscriptionType, out eRefundBalType);

                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetUserPossibleSuscriptionTypes: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserPossibleSuscriptionTypes: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool SetUserPaymentMean(ref USER user,
                                       IInfraestructureRepository infrastructureRepository, 
                                       CUSTOMER_PAYMENT_MEAN paymentMean)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        foreach (CUSTOMER_PAYMENT_MEAN oCustomerPaymentMeans in oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_ENABLED == 1))
                        {
                            oCustomerPaymentMeans.CUSPM_ENABLED = 0;

                            if (oCustomerPaymentMeans.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {

                                if (((PaymentMeanCreditCardProviderType)oCustomerPaymentMeans.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                    PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oCustomerPaymentMeans.CUSPM_TOKEN_CARD_REFERENCE)))
                                {
                                    oCustomerPaymentMeans.CUSPM_VALID = 0;
                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oCustomerPaymentMeans.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oUser.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oCustomerPaymentMeans.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }
                            }

                        }
                        paymentMean.CUSPM_ENABLED = 1;
                        oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(paymentMean);

                        PaymentMeanTypeStatus eTypeStatus=PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;

                        switch ((PaymentMeanType)paymentMean.CUSPM_PAT_ID)
                        {

                            case PaymentMeanType.pmtDebitCreditCard:
                                eTypeStatus = PaymentMeanTypeStatus.pmsDebitCreditCard;
                                break;
                            case PaymentMeanType.pmtPaypal:
                                eTypeStatus = PaymentMeanTypeStatus.pmsPaypal;
                                break;
                            default:
                                break;
                        }

                        oUser.USR_PAYMETH = (int)eTypeStatus; 
                        if (oUser.USR_SUSCRIPTION_TYPE == null)
                        {

                            string sSuscriptionType = "";
                            RefundBalanceType eRefundBalType = RefundBalanceType.rbtAmount;
                            infrastructureRepository.GetCountryPossibleSuscriptionTypes(Convert.ToInt32(oUser.COUNTRy.COU_ID), out sSuscriptionType,  out eRefundBalType);

                            if (sSuscriptionType != "")
                            {
                                oUser.USR_SUSCRIPTION_TYPE = Convert.ToInt32(sSuscriptionType);
                                oUser.USR_REFUND_BALANCE_TYPE = (int)eRefundBalType;
                            }
                        }
                        SecureSubmitChanges(ref dbContext);
                        oUser.CUSTOMER_PAYMENT_MEAN = oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_ENABLED == 1).First();
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserPaymentMean: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserPaymentMean: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool UpdateUserPaymentMean(ref USER user, int iAutomaticRecharge,
                                        int? iModelAutomaticRechargeQuantity,
                                        int? iModelAutomaticRechargeWhenBelowQuantity,
                                        string strPaypalID)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID == (int)PaymentMeanSubType.pmstUndefined)                                                        
                        {

                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_RECHARGE = iAutomaticRecharge;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AMOUNT_TO_RECHARGE = iModelAutomaticRechargeQuantity;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = iModelAutomaticRechargeWhenBelowQuantity;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_ID = strPaypalID;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_FAILED_RETRIES = 0;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 0;


                            if (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtPaypal)
                            {
                                if (iAutomaticRecharge == 0)
                                {
                                    oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 1;
                                    oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstPaypal;


                                }
                                else if (iAutomaticRecharge == 1)
                                {
                                    oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 0;
                                }

                            }



                        }
                        else
                        {


                            CUSTOMER_PAYMENT_MEAN oNewPaymentMean = new CUSTOMER_PAYMENT_MEAN();

                            oNewPaymentMean.CUSPM_PAT_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID;
                            oNewPaymentMean.CUSPM_PAST_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID;
                            oNewPaymentMean.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER;
                            oNewPaymentMean.CUSPM_AUTOMATIC_RECHARGE = iAutomaticRecharge;
                            oNewPaymentMean.CUSPM_AMOUNT_TO_RECHARGE = iModelAutomaticRechargeQuantity;
                            oNewPaymentMean.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = iModelAutomaticRechargeWhenBelowQuantity;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_ID = strPaypalID;
                            oNewPaymentMean.CUSPM_CUR_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUR_ID;
                            oNewPaymentMean.CUSPM_AUTOMATIC_FAILED_RETRIES = 0;
                            oNewPaymentMean.CUSPM_CUS_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUS_ID;
                            oNewPaymentMean.CUSPM_DESCRIPTION = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_DESCRIPTION;
                            oNewPaymentMean.CUSPM_CPTGC_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CPTGC_ID;
                            oNewPaymentMean.CUSPM_ENABLED = 1;


                            if (oNewPaymentMean.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {
                                oNewPaymentMean.CUSPM_TOKEN_CARD_EXPIRATION_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_EXPIRATION_DATE;
                                oNewPaymentMean.CUSPM_TOKEN_CARD_HASH = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_HASH;
                                oNewPaymentMean.CUSPM_TOKEN_CARD_REFERENCE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE;
                                oNewPaymentMean.CUSPM_TOKEN_MASKED_CARD_NUMBER = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_MASKED_CARD_NUMBER;
                                oNewPaymentMean.CUSPM_TOKEN_CARD_SCHEMA = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_SCHEMA;
                                oNewPaymentMean.CUSPM_VALID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID;


                            }
                            else if (oNewPaymentMean.CUSPM_PAT_ID == (int)PaymentMeanType.pmtPaypal)
                            {
                                if ((strPaypalID == oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_ID) &&
                                    (iAutomaticRecharge == oNewPaymentMean.CUSPM_AUTOMATIC_RECHARGE))
                                {

                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE;
                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY;
                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT;
                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS;
                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT;
                                    oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE;
                                    oNewPaymentMean.CUSPM_VALID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID;
                                }
                                else if (iAutomaticRecharge == 0)
                                {
                                    oNewPaymentMean.CUSPM_VALID = 1;
                                    oNewPaymentMean.CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstPaypal;


                                }
                                else if (iAutomaticRecharge == 1)
                                {
                                    oNewPaymentMean.CUSPM_VALID = 0;
                                }

                            }


                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ENABLED = 0;

                            if (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {

                                if (((PaymentMeanCreditCardProviderType)oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                    PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE)))
                                {
                                    oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 0;
                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oUser.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }
                            }


                            PaymentMeanTypeStatus eTypeStatus = PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;

                            switch ((PaymentMeanType)oNewPaymentMean.CUSPM_PAT_ID)
                            {

                                case PaymentMeanType.pmtDebitCreditCard:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsDebitCreditCard;
                                    break;
                                case PaymentMeanType.pmtPaypal:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsPaypal;
                                    break;
                                default:
                                    break;
                            }

                            oUser.USR_PAYMETH = (int)eTypeStatus; 

                            oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oNewPaymentMean);
                            SecureSubmitChanges(ref dbContext);
                            oUser.CUSTOMER_PAYMENT_MEAN = oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_ENABLED == 1).First();
                        }


                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateUserPaymentMean: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateUserPaymentMean: ", e);
                bRes = false;
            }


            return bRes;

        }



        public bool CopyCurrentUserPaymentMean(ref USER user, int iAutomaticRecharge,
                                        int? iModelAutomaticRechargeQuantity,
                                        int? iModelAutomaticRechargeWhenBelowQuantity,
                                        string strPaypalID)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();


                        if ((oUser.CUSTOMER_PAYMENT_MEAN != null) &&
                          (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID == 1) &&
                          (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ENABLED == 1))
                        {

                            CUSTOMER_PAYMENT_MEAN oNewPaymentMean = new CUSTOMER_PAYMENT_MEAN()
                            {
                                CUSPM_AUTOMATIC_RECHARGE = iAutomaticRecharge,
                                CUSPM_AMOUNT_TO_RECHARGE = iModelAutomaticRechargeQuantity,
                                CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = iModelAutomaticRechargeWhenBelowQuantity,
                                CUSPM_TOKEN_PAYPAL_ID = strPaypalID,
                                CUSPM_AUTOMATIC_FAILED_RETRIES = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_FAILED_RETRIES,
                                CUSPM_CUR_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUR_ID,
                                CUSPM_CUS_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUS_ID,
                                CUSPM_DESCRIPTION = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_DESCRIPTION,
                                CUSPM_ENABLED = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ENABLED,
                                CUSPM_LAST_TIME_USERD = DateTime.UtcNow,
                                CUSPM_PAST_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID,
                                CUSPM_PAT_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID,
                                CUSPM_CREDIT_CARD_PAYMENT_PROVIDER = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER,
                                CUSPM_TOKEN_CARD_EXPIRATION_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_EXPIRATION_DATE,
                                CUSPM_TOKEN_CARD_HASH = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_HASH,
                                CUSPM_TOKEN_CARD_REFERENCE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE,
                                CUSPM_TOKEN_CARD_SCHEMA = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_SCHEMA,
                                CUSPM_TOKEN_MASKED_CARD_NUMBER = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_MASKED_CARD_NUMBER,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT,
                                CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE,
                                CUSPM_VALID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID,
                                CUSPM_CPTGC_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CPTGC_ID

                            };
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ENABLED = 0;
                            oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oNewPaymentMean);
                            SecureSubmitChanges(ref dbContext);
                            oUser.CUSTOMER_PAYMENT_MEAN = oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_ENABLED == 1).First();

                        }

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CopyCurrentUserPaymentMean: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CopyCurrentUserPaymentMean: ", e);
                bRes = false;
            }


            return bRes;

        }



        public IQueryable<CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG> GetCurrenciesPaymentTypeGatewayConfigs()
        {
            IQueryable<CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG> res = null;
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

                    res = (from r in dbContext.CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIGs                           
                           select r)
                           .AsQueryable();
                
                    transaction.Complete();
                }


            }
            catch (Exception e)

            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrenciesPaymentTypeGatewayConfigs: ", e);
            }

            return res;
        }


        public bool UpdateUserPaypalPreapprovalPaymentMean(ref USER user, 
                                                           string strPreapprovalKey,
                                                           DateTime dtPreapprovalStartDate,
                                                           DateTime dtPreapprovalEndDate,
                                                           int? iMaxNumberOfPayments,
                                                           decimal? dMaxAmountPerPayment,
                                                           decimal? dMaxTotalAMount)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();


                        if (string.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY))
                        {
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY = strPreapprovalKey;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE = dtPreapprovalStartDate;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE = dtPreapprovalEndDate;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT = dMaxAmountPerPayment;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS = iMaxNumberOfPayments;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT = dMaxTotalAMount;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 1;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstPaypal;
                        }
                        else
                        {
                            CUSTOMER_PAYMENT_MEAN oNewPaymentMean = new CUSTOMER_PAYMENT_MEAN();

                            oNewPaymentMean.CUSPM_PAT_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID;
                            oNewPaymentMean.CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstPaypal;
                            oNewPaymentMean.CUSPM_AUTOMATIC_RECHARGE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_RECHARGE;
                            oNewPaymentMean.CUSPM_AMOUNT_TO_RECHARGE = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AMOUNT_TO_RECHARGE;
                            oNewPaymentMean.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_ID;
                            oNewPaymentMean.CUSPM_CUR_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUR_ID;
                            oNewPaymentMean.CUSPM_AUTOMATIC_FAILED_RETRIES = 0;
                            oNewPaymentMean.CUSPM_CUS_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CUS_ID;
                            oNewPaymentMean.CUSPM_DESCRIPTION = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_DESCRIPTION;
                            oNewPaymentMean.CUSPM_ENABLED = 1;

                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY = strPreapprovalKey;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE = dtPreapprovalStartDate;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE = dtPreapprovalEndDate;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT = dMaxAmountPerPayment;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS = iMaxNumberOfPayments;
                            oNewPaymentMean.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT = dMaxTotalAMount;
                            oNewPaymentMean.CUSPM_VALID = 1;

                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ENABLED = 0;

                            oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oNewPaymentMean);
                            PaymentMeanTypeStatus eTypeStatus = PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;

                            switch ((PaymentMeanType)oNewPaymentMean.CUSPM_PAT_ID)
                            {

                                case PaymentMeanType.pmtDebitCreditCard:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsDebitCreditCard;
                                    break;
                                case PaymentMeanType.pmtPaypal:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsPaypal;
                                    break;
                                default:
                                    break;
                            }

                            oUser.USR_PAYMETH = (int)eTypeStatus; 

                            SecureSubmitChanges(ref dbContext);

                            oUser.CUSTOMER_PAYMENT_MEAN = oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_ENABLED == 1).First();

                        }


                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        user = oUser;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateUserPaypalPreapprovalPaymentMean: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateUserPaypalPreapprovalPaymentMean: ", e);
                bRes = false;
            }


            return bRes;

        }




        public bool StartRecharge(decimal dGatewayConfig,
                            string strEmail,
                            DateTime dtUTCDate,
                            DateTime dtDate,
                            int iTotalAmaunt,
                            decimal dCurrencyID,
                            string strAuthResult,
                            string strOpReference,
                            string strTransactionID,
                            string strCFTransactionID,
                            string strGatewayDate,
                            string strAuthCode,
                            PaymentMeanRechargeStatus eTransStatus
                            )
        {
            bool bRes = true;
            integraMobileDBEntitiesDataContext dbContext = null;
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



                        dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        PENDING_TRANSACTION_OPERATION oPendingTransaction = new PENDING_TRANSACTION_OPERATION()
                        {
                            PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.Charge,
                            PTROP_CPTGC_ID = dGatewayConfig,
                            PTROP_EMAIL = strEmail,
                            PTROP_UTC_DATE = dtUTCDate,
                            PTROP_DATE = dtDate,
                            PTROP_TOTAL_AMOUNT_CHARGED = iTotalAmaunt,
                            PTROP_CUR_ID = dCurrencyID,
                            PTROP_AUTH_RESULT = strAuthResult,
                            PTROP_OP_REFERENCE = strOpReference,
                            PTROP_TRANSACTION_ID = strTransactionID,
                            PTROP_CF_TRANSACTION_ID = strCFTransactionID,
                            PTROP_GATEWAY_DATE = strGatewayDate,
                            PTROP_AUTH_CODE = strAuthCode,
                            PTROP_TRANS_STATUS = (int)eTransStatus,
                            PTROP_STATUS_DATE = dtUTCDate,
                        };


                        dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(oPendingTransaction);

                        
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            bRes = true;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "StartRecharge: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "StartRecharge: ", e);
                        bRes = false;
                    }
                   
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "StartRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool CompleteStartRecharge(decimal dGatewayConfig,
                          string strEmail,
                          string strTransactionID,
                          string strAuthResult,
                          string strCFTransactionID,
                          string strGatewayDate,
                          string strAuthCode,
                          PaymentMeanRechargeStatus eTransStatus)
        {
            bool bRes = true;
            integraMobileDBEntitiesDataContext dbContext = null;
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



                        dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();




                        var oPendingTransaction = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                               where r.PTROP_OP_TYPE  == (int)PendingTransactionOperationOpType.Charge &&
                                                     r.PTROP_CPTGC_ID == dGatewayConfig &&
                                                     r.PTROP_EMAIL==strEmail &&
                                                     r.PTROP_TRANSACTION_ID == strTransactionID                                                     
                                               select r).FirstOrDefault();


                        if (oPendingTransaction != null)
                        {
                            oPendingTransaction.PTROP_AUTH_RESULT = strAuthResult;
                            oPendingTransaction.PTROP_CF_TRANSACTION_ID = strCFTransactionID;
                            oPendingTransaction.PTROP_GATEWAY_DATE = strGatewayDate;
                            oPendingTransaction.PTROP_AUTH_CODE = strAuthCode;
                            oPendingTransaction.PTROP_TRANS_STATUS = (int)eTransStatus;
                            oPendingTransaction.PTROP_STATUS_DATE = DateTime.UtcNow;
                        }
                                               
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            bRes = true;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "CompleteStartRecharge: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CompleteStartRecharge: ", e);
                        bRes = false;
                    }
                   
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CompleteStartRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool FailedRecharge(decimal dGatewayConfig,
                          string strEmail,
                          string strTransactionID,
                          PaymentMeanRechargeStatus eTransStatus)
        {
            bool bRes = true;
            integraMobileDBEntitiesDataContext dbContext = null;
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



                        dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();




                        var oPendingTransaction = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                                   where r.PTROP_OP_TYPE == (int)PendingTransactionOperationOpType.Charge &&
                                                         r.PTROP_CPTGC_ID == dGatewayConfig &&
                                                         r.PTROP_EMAIL == strEmail &&
                                                         r.PTROP_TRANSACTION_ID == strTransactionID
                                                   select r).FirstOrDefault();


                        if (oPendingTransaction != null)
                        {                            
                            oPendingTransaction.PTROP_TRANS_STATUS = (int)eTransStatus;
                            oPendingTransaction.PTROP_STATUS_DATE = DateTime.UtcNow;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            bRes = true;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "FailedRecharge: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "FailedRecharge: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "FailedRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool RechargeUserBalance(ref USER user,
                                        int iOSType,
                                        bool bAddToBalance,
                                        int iRechargeQuantity,
                                        decimal dPercVAT1, decimal dPercVAT2, int iPartialVAT1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE,
                                        int iQuantityCharged,
                                        decimal dCurrencyID,
                                        PaymentSuscryptionType suscriptionType,
                                        PaymentMeanRechargeStatus rechargeStatus,
                                        PaymentMeanRechargeCreationType rechargeCreationType,
                                        //decimal dVATApplied,
                                        string strOpReference,
                                        string strTransactionId,
                                        string strCFTransactionId,
                                        string strGatewayDate,
                                        string strAuthCode,
                                        string strAuthResult,
                                        string strAuthResultDesc,
                                        string strCardHash,
                                        string strCardReference,
                                        string strCardScheme,
                                        string strMaskedCardNumber,
                                        DateTime? dtCardExpirationDate,
                                        string strPaypalToken,
                                        string strPaypalPayerId,
                                        string strPayPaypalPreapprovedPayKey,
                                        bool bOverwritePaymentTypeData,
                                        decimal? dLatitude, decimal? dLongitude,
                                        string strAppVersion,                                        
                                        out decimal? dRechargeId,
                                        bool bCreateNewContext = false)
        {
            bool bRes = true;
            dRechargeId = null;
            integraMobileDBEntitiesDataContext dbContext = null;
            CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge = null;
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


                        if (bCreateNewContext)
                        {
                            dbContext = new integraMobileDBEntitiesDataContext();
                        }
                        else
                        {
                            dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        }

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iQuantityCharged, 0, null, out dCustomerInvoiceID);



                            if ((oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard) &&
                                ((!String.IsNullOrEmpty(strCardHash)) &&
                                 (!String.IsNullOrEmpty(strCardReference)) &&
                                 (!String.IsNullOrEmpty(strMaskedCardNumber)) &&
                                 (dtCardExpirationDate != null)) &&
                                 ((bOverwritePaymentTypeData) || (String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_HASH)) ||
                                 (String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE)) ||
                                 (String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_MASKED_CARD_NUMBER)) ||
                                 (String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_SCHEMA)) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_EXPIRATION_DATE == null)))
                            {
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_HASH = strCardHash;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE = strCardReference;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_MASKED_CARD_NUMBER = strMaskedCardNumber;
                                if (!String.IsNullOrEmpty(strCardScheme))
                                {
                                    oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_SCHEMA = strCardScheme;
                                }

                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_EXPIRATION_DATE = dtCardExpirationDate;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 1;

                                if (!String.IsNullOrEmpty(strCardScheme))
                                {

                                    var oPaymentSubtypes = (from r in dbContext.PAYMENT_SUBTYPEs
                                                            where r.PAST_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard
                                                            select r).ToArray();
                                    foreach (PAYMENT_SUBTYPE PS in oPaymentSubtypes)
                                    {
                                        if (PS.PAST_DESCRIPTION.ToUpper() == strCardScheme.ToUpper())
                                        {
                                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAST_ID = PS.PAST_ID;
                                            break;
                                        }

                                    }
                                }


                                foreach (CUSTOMER_PAYMENT_MEAN oCustomerPaymentMeans in oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Where(f => f.CUSPM_VALID == 1 && f.CUSPM_ENABLED==0))
                                {

                                    if (oCustomerPaymentMeans.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                                    {

                                        if (((PaymentMeanCreditCardProviderType)oCustomerPaymentMeans.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                            PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oCustomerPaymentMeans.CUSPM_TOKEN_CARD_REFERENCE)))
                                        {
                                            oCustomerPaymentMeans.CUSPM_VALID = 0;
                                            dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                            {
                                                PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                                PTROP_CPTGC_ID = oCustomerPaymentMeans.CUSPM_CPTGC_ID.Value,
                                                PTROP_EMAIL = oUser.USR_EMAIL,
                                                PTROP_UTC_DATE = DateTime.UtcNow,
                                                PTROP_DATE = DateTime.Now,
                                                PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                                PTROP_STATUS_DATE = DateTime.UtcNow,
                                                PTROP_TOKEN = oCustomerPaymentMeans.CUSPM_TOKEN_CARD_REFERENCE,
                                            });
                                        }
                                    }

                                }

                            }
                            else if ((oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_RECHARGE == 0) &&
                                     (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtPaypal) &&
                                 ((!String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_ID)) ||
                                 (!String.IsNullOrEmpty(oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY)) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE != null) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE != null) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT != null) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS != null) ||
                                 (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT != null)))
                            {
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_ID = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_KEY = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_START_DATE = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_AMOUNT_PER_PAYMENT = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_NUMBER_PAYMENTS = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_MAX_TOTAL_AMOUNT = null;
                                oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_VALID = 1;


                            }



                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_FAILED_RETRIES = 0;


                            oRecharge = new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                                                   {
                                                       CUSPMR_MOSE_OS = iOSType,
                                                       CUSPMR_AMOUNT = iRechargeQuantity,
                                                       CUSPMR_CUR_ID = dCurrencyID,
                                                       CUSPMR_TRANS_STATUS = (int)rechargeStatus,
                                                       CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                                       CUSPMR_SUSCRIPTION_TYPE = (int)suscriptionType,
                                                       CUSPMR_CUSPM_ID = oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_ID,
                                                       CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                                       CUSPMR_USR_ID = oUser.USR_ID,
                                                       CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                                       CUSPMR_UTC_DATE = DateTime.UtcNow,
                                                       CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                                       CUSPMR_OP_REFERENCE = strOpReference,
                                                       CUSPMR_TRANSACTION_ID = strTransactionId,
                                                       CUSPMR_CF_TRANSACTION_ID = strCFTransactionId,
                                                       CUSPMR_GATEWAY_DATE = strGatewayDate,
                                                       CUSPMR_AUTH_CODE = strAuthCode,
                                                       CUSPMR_AUTH_RESULT = strAuthResult,
                                                       CUSPMR_CARD_HASH = strCardHash,
                                                       CUSPMR_CARD_REFERENCE = strCardReference,
                                                       CUSPMR_CARD_SCHEME = strCardScheme,
                                                       CUSPMR_MASKED_CARD_NUMBER = strMaskedCardNumber,
                                                       CUSPMR_CARD_EXPIRATION_DATE = dtCardExpirationDate,
                                                       CUSPMR_PAYPAL_3T_TOKEN = strPaypalToken,
                                                       CUSPMR_PAYPAL_3T_PAYER_ID = strPaypalPayerId,
                                                       CUSPMR_PAYPAL_PREAPPROVED_PAY_KEY = strPayPaypalPreapprovedPayKey,
                                                       /*CUSPMR_PERC_VAT = dVATApplied,
                                                       CUSPMR_TOTAL_AMOUNT_CHARGED = iQuantityCharged,
                                                       CUSPMR_PAT_FIXED_FEE = oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_FIXED_FEE.HasValue ?
                                                                (decimal)oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_FIXED_FEE : 0,
                                                       CUSPMR_PAT_PERC_FEE = oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_PERC_FEE.HasValue ?
                                                                (decimal)oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_PERC_FEE : 0,
                                                       CUSPMR_PAST_FIXED_FEE = oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_FIXED_FEE.HasValue ?
                                                                (decimal)oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_FIXED_FEE : 0,
                                                       CUSPMR_PAST_PERC_FEE = oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_PERC_FEE.HasValue ?
                                                                (decimal)oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_PERC_FEE : 0,*/
                                                       CUSPMR_PERC_VAT1 = dPercVAT1,
                                                       CUSPMR_PERC_VAT2 = dPercVAT2,
                                                       CUSPMR_PARTIAL_VAT1 = iPartialVAT1,
                                                       CUSPMR_PERC_FEE = dPercFEE,
                                                       CUSPMR_PERC_FEE_TOPPED = iPercFEETopped,
                                                       CUSPMR_PARTIAL_PERC_FEE = iPartialPercFEE,
                                                       CUSPMR_FIXED_FEE = iFixedFEE,
                                                       CUSPMR_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                                       CUSPMR_TOTAL_AMOUNT_CHARGED = iQuantityCharged,
                                                       CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                                       CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                                       CUSPMR_LATITUDE = dLatitude,
                                                       CUSPMR_LONGITUDE = dLongitude,
                                                       CUSPMR_APP_VERSION = strAppVersion,
                                                       CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                                       CUSPMR_CREATION_TYPE = (int)rechargeCreationType,                                                     
                                                   };



                            if (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {

                                CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG oGatewayConfig = null;
                                oGatewayConfig = oUser.CUSTOMER_PAYMENT_MEAN.
                                                CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG;                                                                       
                                if (oGatewayConfig!=null) 
                                {
                                    oRecharge.CUSPMR_CPTGC_ID = oGatewayConfig.CPTGC_ID;
                                }


                                var oPendingTransaction = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                                           where r.PTROP_OP_TYPE == (int)PendingTransactionOperationOpType.Charge &&
                                                                 r.PTROP_CPTGC_ID == oGatewayConfig.CPTGC_ID &&
                                                                 r.PTROP_EMAIL == oUser.USR_EMAIL &&
                                                                 r.PTROP_TRANSACTION_ID == strTransactionId
                                                           select r).FirstOrDefault();


                                if (oPendingTransaction != null)
                                {
                                    dbContext.PENDING_TRANSACTION_OPERATIONs.DeleteOnSubmit(oPendingTransaction);
                                }

                            }


                            /*if ((oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard) &&
                                (oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER == (int)PaymentMeanCreditCardProviderType.pmccpIECISA))
                            {
                                var oRechargeInfo = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGES_INFOs
                                                     where r.CUSPMRI_OP_REFERENCE == strOpReference
                                                     select r).FirstOrDefault();


                                if (oRechargeInfo != null)
                                {

                                    DateTime dtDate = DateTime.Now;
                                    oRechargeInfo.CUSPMRI_STATUS = (int)PaymentMeanRechargeInfoStatus.Confirmed;
                                    oRechargeInfo.CUSPMRI_STATUS_DATE = dtDate;
                                    oRechargeInfo.CUSPMRI_STATUS_UTCDATE = dtDate + new TimeSpan(0, oUser.USR_UTC_OFFSET, 0);
                                    oRechargeInfo.CUSPMRI_CONFIRM_RESULTCODE = strAuthResult;
                                    oRechargeInfo.CUSPMRI_CONFIRM_RESULTCODE_DESC = strAuthResultDesc;
                                    oRechargeInfo.CUSTOMER_PAYMENT_MEANS_RECHARGE = oRecharge;
                                    oRechargeInfo.CUSPMRI_TRANSACTION_ID = strTransactionId;
                                    oRechargeInfo.CUSPMRI_CARD_REFERENCE = strCardReference;
                                    oRechargeInfo.CUSPMRI_AUTH_CODE = strAuthCode;
                                    oRechargeInfo.CUSPMRI_MASKED_CARD_NUMBER = strMaskedCardNumber;
                                    //;


                                }
                                else
                                {
                                    throw new Exception("Recharge Info is null");

                                }

                            }*/




                            PaymentMeanTypeStatus eTypeStatus = PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;

                            switch ((PaymentMeanType)oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID)
                            {

                                case PaymentMeanType.pmtDebitCreditCard:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsDebitCreditCard;
                                    break;
                                case PaymentMeanType.pmtPaypal:
                                    eTypeStatus = PaymentMeanTypeStatus.pmsPaypal;
                                    break;
                                default:
                                    break;
                            }


                            if ((PaymentMeanTypeStatus) oUser.USR_PAYMETH !=eTypeStatus)
                                oUser.USR_PAYMETH = (int)eTypeStatus; 

                            if (bAddToBalance)
                            {
                                ModifyUserBalance(ref oUser, iRechargeQuantity);
                            }

                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = oRecharge.CUSPMR_UTC_DATE;
                            if (!string.IsNullOrEmpty(oUser.USR_SIGNUP_GUID)) oUser.USR_SIGNUP_GUID=null;

                           
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(oRecharge);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            if (oRecharge != null)
                            {
                                dRechargeId = oRecharge.CUSPMR_ID;
                            }

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalance: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalance: ", e);
                        bRes = false;
                    }
                    finally
                    {
                        if ((dbContext != null)&&(bCreateNewContext))
                        {
                            dbContext.Close();
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalance: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool AutomaticRechargeFailure(ref USER user)
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser != null)
                        {
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_AUTOMATIC_FAILED_RETRIES++;
                            oUser.CUSTOMER_PAYMENT_MEAN.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            dbContext.Close();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AutomaticRechargeFailure: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AutomaticRechargeFailure: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AutomaticRechargeFailure: ", e);
                bRes = false;
            }

            return bRes;
        }




        public bool RechargeUserBalanceWithCoupon(ref USER user,
                                        int iOSType,
                                        int iRechargeQuantity,
                                        decimal dCurrencyID,
                                        string strRechargeId,
                                        ref RECHARGE_COUPON coupon,
                                        decimal? dLatitude, decimal? dLongitude, string strAppVersion)

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
                        decimal userId = user.USR_ID;
                        decimal couponId = coupon.RCOUP_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();


                        var oCoupon = (from r in dbContext.RECHARGE_COUPONs
                                       where r.RCOUP_ID == couponId
                                       select r).First();


                        if ((oUser != null) && (oCoupon != null))
                        {                            


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                            {
                                                CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                                CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,                                                                    
                                                CUSPM_AUTOMATIC_RECHARGE = 0,
                                                CUSPM_AMOUNT_TO_RECHARGE =null,
                                                CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                                CUSPM_TOKEN_PAYPAL_ID = "",
                                                CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                                CUSPM_VALID = 1,
                                                CUSPM_ENABLED = 0
                                            };

                                
                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);


                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(
                                            new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                                            {
                                                CUSPMR_MOSE_OS = iOSType,
                                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                                CUSPMR_AMOUNT = iRechargeQuantity,
                                                CUSPMR_CUR_ID = dCurrencyID,
                                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                                CUSPMR_USR_ID = oUser.USR_ID,
                                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                                CUSPMR_RCOUP_ID = coupon.RCOUP_ID,
                                                CUSPMR_GATEWAY_DATE = (DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0)).ToString("HHmmssddMMyy"),
                                                CUSPMR_TRANSACTION_ID = strRechargeId,
                                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                                CUSPMR_LATITUDE = dLatitude,
                                                CUSPMR_LONGITUDE = dLongitude,
                                                CUSPMR_APP_VERSION = strAppVersion,
                                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                                CUSPMR_TOTAL_AMOUNT_CHARGED = iRechargeQuantity,
                                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Coupon,
                                                CUSPMR_CREATION_TYPE = (int)PaymentMeanRechargeCreationType.pmrctRegularRecharge
                                                
                                            });

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;
                            
                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;

                            oCoupon.RCOUP_COUPS_ID = Convert.ToInt32(RechargeCouponsStatus.Used);


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            coupon = oCoupon;


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCoupon: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCoupon: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCoupon: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RechargeUserBalanceWithPagatelia(ref USER user,
                                        int iOSType,
                                        int iRechargeQuantity,
                                        decimal dCurrencyID,
                                        string strPagateliaSessionId,
                                        int? iPagateliaNewBalance,
                                        decimal? dLatitude, decimal? dLongitude, string strAppVersion)
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
                        decimal userId = user.USR_ID;                        

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                     {
                                         CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                         CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,
                                         CUSPM_AUTOMATIC_RECHARGE = 0,
                                         CUSPM_AMOUNT_TO_RECHARGE = null,
                                         CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                         CUSPM_TOKEN_PAYPAL_ID = "",
                                         CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                         CUSPM_VALID = 1,
                                         CUSPM_ENABLED = 0
                                     };


                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);


                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(
                                            new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                                            {
                                                CUSPMR_MOSE_OS = iOSType,
                                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                                CUSPMR_AMOUNT = iRechargeQuantity,
                                                CUSPMR_CUR_ID = dCurrencyID,
                                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                                CUSPMR_USR_ID = oUser.USR_ID,
                                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,                                                
                                                CUSPMR_GATEWAY_DATE = (DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0)).ToString("HHmmssddMMyy"),
                                                CUSPMR_TRANSACTION_ID = strPagateliaSessionId,
                                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                                CUSPMR_LATITUDE = dLatitude,
                                                CUSPMR_LONGITUDE = dLongitude,
                                                CUSPMR_APP_VERSION = strAppVersion,
                                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                                CUSPMR_TOTAL_AMOUNT_CHARGED = iRechargeQuantity,
                                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Pagatelia,
                                                CUSPMR_PAGATELIA_NEW_BALANCE = iPagateliaNewBalance,
                                                CUSPMR_CREATION_TYPE = (int)PaymentMeanRechargeCreationType.pmrctRegularRecharge
                                            });

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;

                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;
                            

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;                            

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPagatelia: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPagatelia: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPagatelia: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RechargeUserBalanceWithSpotycoins(ref USER user,
                                        int iOSType,
                                        int iRechargeQuantity,
                                        decimal dCurrencyID,
                                        decimal? dLatitude, decimal? dLongitude, string strAppVersion)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                     {
                                         CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                         CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,
                                         CUSPM_AUTOMATIC_RECHARGE = 0,
                                         CUSPM_AMOUNT_TO_RECHARGE = null,
                                         CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                         CUSPM_TOKEN_PAYPAL_ID = "",
                                         CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                         CUSPM_VALID = 1,
                                         CUSPM_ENABLED = 0
                                     };


                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);


                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(
                                            new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                                            {
                                                CUSPMR_MOSE_OS = iOSType,
                                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                                CUSPMR_AMOUNT = iRechargeQuantity,
                                                CUSPMR_CUR_ID = dCurrencyID,
                                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                                CUSPMR_USR_ID = oUser.USR_ID,
                                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                                CUSPMR_GATEWAY_DATE = (DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0)).ToString("HHmmssddMMyy"),                                                
                                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                                CUSPMR_LATITUDE = dLatitude,
                                                CUSPMR_LONGITUDE = dLongitude,
                                                CUSPMR_APP_VERSION = strAppVersion,
                                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                                CUSPMR_TOTAL_AMOUNT_CHARGED = iRechargeQuantity,
                                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Spotycoins,
                                                CUSPMR_TRANSACTION_ID = "SPOTYCOINS",
                                                CUSPMR_CREATION_TYPE = (int)PaymentMeanRechargeCreationType.pmrctRegularRecharge
                                            });

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;

                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithSpotycoins: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithSpotycoins: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithSpotycoins: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RechargeUserBalanceWithCash(ref USER user,
                                                int iOSType,
                                                int iRechargeQuantity,
                                                decimal dPercVAT1, decimal dPercVAT2, int iPartialVAT1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE,
                                                int iQuantityCharged,
                                                decimal dCurrencyID,
                                                decimal? dLatitude, decimal? dLongitude, string strAppVersion,
                                                PaymentMeanRechargeCreationType rechargeCreationType,
                                                decimal? dInstallationId, decimal? dFinanDistOperatorId,
                                                string sBackOfficeUsr,
                                                out decimal? dRechargeId,
                                                bool bCreateNewContext = false)
        {
            bool bRes = true;
            dRechargeId = null;
            CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                     {
                                         CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                         CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,
                                         CUSPM_AUTOMATIC_RECHARGE = 0,
                                         CUSPM_AMOUNT_TO_RECHARGE = null,
                                         CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                         CUSPM_TOKEN_PAYPAL_ID = "",
                                         CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                         CUSPM_VALID = 1,
                                         CUSPM_ENABLED = 0
                                     };


                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);



                            oRecharge = new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                                            {
                                                CUSPMR_MOSE_OS = iOSType,
                                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                                CUSPMR_AMOUNT = iRechargeQuantity,
                                                CUSPMR_CUR_ID = dCurrencyID,
                                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                                CUSPMR_USR_ID = oUser.USR_ID,
                                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                                CUSPMR_GATEWAY_DATE = (DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0)).ToString("HHmmssddMMyy"),
                                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                                CUSPMR_TRANSACTION_ID = "",
                                                CUSPMR_PERC_VAT1 = dPercVAT1,
                                                CUSPMR_PERC_VAT2 = dPercVAT2,
                                                CUSPMR_PARTIAL_VAT1 = iPartialVAT1,
                                                CUSPMR_PERC_FEE = dPercFEE,
                                                CUSPMR_PERC_FEE_TOPPED = iPercFEETopped,
                                                CUSPMR_PARTIAL_PERC_FEE = iPartialPercFEE,
                                                CUSPMR_FIXED_FEE = iFixedFEE,
                                                CUSPMR_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                                CUSPMR_TOTAL_AMOUNT_CHARGED = iQuantityCharged,
                                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                                CUSPMR_LATITUDE = dLatitude,
                                                CUSPMR_LONGITUDE = dLongitude,
                                                CUSPMR_APP_VERSION = strAppVersion,
                                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Cash,
                                                CUSPMR_CREATION_TYPE = (int)rechargeCreationType,
                                                CUSPMR_INS_ID = dInstallationId,
                                                CUSPMR_FDO_ID = dFinanDistOperatorId,
                                                CUSPMR_BACKOFFICE_USR = sBackOfficeUsr
                                            };

                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(oRecharge);

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;

                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            if (oRecharge != null)
                            {
                                dRechargeId = oRecharge.CUSPMR_ID;
                            }

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCash: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCash: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithCash: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool RechargeUserBalanceWithOxxo(ref USER user,
                                                int iOSType,
                                                int iRechargeQuantity,
                                                decimal dPercVAT1, decimal dPercVAT2, int iPartialVAT1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE,
                                                int iQuantityCharged,
                                                decimal dCurrencyID,
                                                decimal? dLatitude, decimal? dLongitude, string strAppVersion,
                                                PaymentMeanRechargeCreationType rechargeCreationType,
                                                string sOxxoToken, int? iOxxoCashMachine, string sOxxoEntryMode, decimal dOxxoTicket, decimal dOxxoFolio, DateTime dtOxxoAdminDate, string sOxxoStore, string sOxxoPartial,
                                                decimal? dSrcCurId, int? iSrcAmount, decimal? dSrcChangeApplied, decimal? dSrcChangeFEEApplied,
                                                out decimal? dRechargeId,
                                                bool bCreateNewContext = false)
        {
            bool bRes = true;
            dRechargeId = null;
            CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                     {
                                         CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                         CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,
                                         CUSPM_AUTOMATIC_RECHARGE = 0,
                                         CUSPM_AMOUNT_TO_RECHARGE = null,
                                         CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                         CUSPM_TOKEN_PAYPAL_ID = "",
                                         CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                         CUSPM_VALID = 1,
                                         CUSPM_ENABLED = 0
                                     };


                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);



                            oRecharge = new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                            {
                                CUSPMR_MOSE_OS = iOSType,
                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                CUSPMR_AMOUNT = iRechargeQuantity,
                                CUSPMR_CUR_ID = dCurrencyID,
                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                CUSPMR_USR_ID = oUser.USR_ID,
                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                CUSPMR_GATEWAY_DATE = (DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0)).ToString("HHmmssddMMyy"),
                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                CUSPMR_TRANSACTION_ID = "",
                                CUSPMR_PERC_VAT1 = dPercVAT1,
                                CUSPMR_PERC_VAT2 = dPercVAT2,
                                CUSPMR_PARTIAL_VAT1 = iPartialVAT1,
                                CUSPMR_PERC_FEE = dPercFEE,
                                CUSPMR_PERC_FEE_TOPPED = iPercFEETopped,
                                CUSPMR_PARTIAL_PERC_FEE = iPartialPercFEE,
                                CUSPMR_FIXED_FEE = iFixedFEE,
                                CUSPMR_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                CUSPMR_TOTAL_AMOUNT_CHARGED = iQuantityCharged,
                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                CUSPMR_LATITUDE = dLatitude,
                                CUSPMR_LONGITUDE = dLongitude,
                                CUSPMR_APP_VERSION = strAppVersion,
                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Oxxo,
                                CUSPMR_CREATION_TYPE = (int)rechargeCreationType,
                                CUSPMR_OXXO_TOKEN = sOxxoToken,
                                CUSPMR_OXXO_CASH_MACHINE = iOxxoCashMachine,
                                CUSPMR_OXXO_ENTRY_MODE = sOxxoEntryMode,
                                CUSPMR_OXXO_TICKET = dOxxoTicket,
                                CUSPMR_OXXO_FOLIO = dOxxoFolio,
                                CUSPMR_OXXO_ADMIN_DATE = dtOxxoAdminDate,
                                CUSPMR_OXXO_STORE = sOxxoStore,
                                CUSPMR_OXXO_PARTIAL = sOxxoPartial,
                                CUSPMR_SRC_CUR_ID = dSrcCurId,
                                CUSPMR_SRC_AMOUNT = iSrcAmount,
                                CUSPMR_SRC_CHANGE_APPLIED = dSrcChangeApplied,
                                CUSPMR_SRC_CHANGE_FEE_APPLIED = dSrcChangeFEEApplied
                            };

                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(oRecharge);

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;

                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            if (oRecharge != null)
                            {
                                dRechargeId = oRecharge.CUSPMR_ID;
                            }

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithOxxo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithOxxo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithOxxo: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool RechargeUserBalanceWithPaypal(ref USER user,
                                                int iOSType,
                                                int iRechargeQuantity,
                                                decimal dPercVAT1, decimal dPercVAT2, int iPartialVAT1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE,
                                                int iQuantityCharged,
                                                decimal dCurrencyID,
                                                decimal? dLatitude, decimal? dLongitude, string strAppVersion,
                                                PaymentMeanRechargeCreationType rechargeCreationType,
                                                string strPaypalId,
                                                string strPaypalAuthorizationId,
                                                string strPaypalCreateTime,
                                                string strPaypalIntent,
                                                string strPaypalState,
                                                out decimal? dRechargeId,
                                                bool bCreateNewContext = false)
        {
            bool bRes = true;
            dRechargeId = null;
            CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            CUSTOMER_PAYMENT_MEAN oPaymentMean = null;
                            if (oUser.CUSTOMER_PAYMENT_MEAN == null)
                            {
                                oPaymentMean =
                                     new CUSTOMER_PAYMENT_MEAN
                                     {
                                         CUSPM_PAT_ID = Convert.ToInt32(PaymentMeanType.pmtUndefined),
                                         CUSPM_PAST_ID = (int)PaymentMeanSubType.pmstUndefined,
                                         CUSPM_AUTOMATIC_RECHARGE = 0,
                                         CUSPM_AMOUNT_TO_RECHARGE = null,
                                         CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS = null,
                                         CUSPM_TOKEN_PAYPAL_ID = "",
                                         CUSPM_CUR_ID = oUser.CURRENCy.CUR_ID,
                                         CUSPM_VALID = 1,
                                         CUSPM_ENABLED = 0
                                     };


                                oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANs.Add(oPaymentMean);
                            }
                            else
                            {
                                oPaymentMean = oUser.CUSTOMER_PAYMENT_MEAN;
                            }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0), oUser.CUSTOMER.CUS_ID, dCurrencyID,
                                               iRechargeQuantity, 0, null, out dCustomerInvoiceID);


                            CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG oPaypalConfig = dbContext.CURRENCies
                                .Where(r=>r.CUR_ID == dCurrencyID).FirstOrDefault()
                                .CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIGs
                                .Where(r => r.CPTGC_ENABLED != 0 && r.CPTGC_PAT_ID == Convert.ToInt32(PaymentMeanType.pmtPaypal))
                                .FirstOrDefault();

                            decimal? dGatewayConfigId = ((oPaypalConfig != null) ? oPaypalConfig.CPTGC_ID : (decimal?)null);

                            oRecharge = new CUSTOMER_PAYMENT_MEANS_RECHARGE()
                            {
                                CUSPMR_MOSE_OS = iOSType,
                                CUSPMR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay,
                                CUSPMR_AMOUNT = iRechargeQuantity,
                                CUSPMR_CUR_ID = dCurrencyID,
                                CUSPMR_CUS_ID = oUser.CUSTOMER.CUS_ID,
                                CUSPMR_USR_ID = oUser.USR_ID,
                                CUSPMR_DATE = DateTime.UtcNow - new TimeSpan(0, oUser.USR_UTC_OFFSET, 0),
                                CUSPMR_UTC_DATE = DateTime.UtcNow,
                                CUSPMR_DATE_UTC_OFFSET = oUser.USR_UTC_OFFSET,
                                CUSPMR_STATUS_DATE = DateTime.UtcNow,
                                CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                CUSPMR_TRANSACTION_ID = strPaypalId,
                                CUSPMR_GATEWAY_DATE = strPaypalCreateTime,
                                CUSPMR_PAYPAL_INTENT = strPaypalIntent,
                                CUSPMR_AUTH_RESULT = strPaypalState,
                                CUSPMR_AUTH_CODE = strPaypalAuthorizationId,
                                CUSPMR_PERC_VAT1 = dPercVAT1,
                                CUSPMR_PERC_VAT2 = dPercVAT2,
                                CUSPMR_PARTIAL_VAT1 = iPartialVAT1,
                                CUSPMR_PERC_FEE = dPercFEE,
                                CUSPMR_PERC_FEE_TOPPED = iPercFEETopped,
                                CUSPMR_PARTIAL_PERC_FEE = iPartialPercFEE,
                                CUSPMR_FIXED_FEE = iFixedFEE,
                                CUSPMR_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                CUSPMR_TOTAL_AMOUNT_CHARGED = iQuantityCharged,
                                CUSPMR_BALANCE_BEFORE = oUser.USR_BALANCE,
                                CUSPMR_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                CUSPMR_LATITUDE = dLatitude,
                                CUSPMR_LONGITUDE = dLongitude,
                                CUSPMR_APP_VERSION = strAppVersion,
                                CUSPMR_CUSINV_ID = dCustomerInvoiceID,
                                CUSPMR_TYPE = (int)PaymentMeanRechargeType.Paypal,
                                CUSPMR_CREATION_TYPE = (int)rechargeCreationType,
                                CUSPMR_CPTGC_ID = dGatewayConfigId,
                            };

                            oPaymentMean.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Add(oRecharge);

                            ModifyUserBalance(ref oUser, iRechargeQuantity);

                            if (!oUser.USR_SUSCRIPTION_TYPE.HasValue) oUser.USR_SUSCRIPTION_TYPE = (int)PaymentSuscryptionType.pstPrepay;
                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = DateTime.UtcNow;


                            oPaymentMean.CUSPM_LAST_TIME_USERD = DateTime.UtcNow;


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            if (oRecharge != null)
                            {
                                dRechargeId = oRecharge.CUSPMR_ID;
                            }

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPaypal: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPaypal: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RechargeUserBalanceWithPaypal: ", e);
                bRes = false;
            }

            return bRes;
        }







        public bool RefundRecharge(ref USER user, decimal dRechargeId, bool bRestoreBalance)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();

                        if (oUser != null)
                        {

                            var oRecharges = oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs.
                                    Where(r => r.CUSPMR_ID == dRechargeId);

                            if (oRecharges.Count() == 1)
                            {
                                if (bRestoreBalance)
                                {
                                    ModifyUserBalance(ref oUser, -oRecharges.First().CUSPMR_AMOUNT);

                                }

                                switch ((PaymentMeanRechargeStatus)oRecharges.First().CUSPMR_TRANS_STATUS)
                                {
                                    case PaymentMeanRechargeStatus.Authorized:
                                        oRecharges.First().CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Cancellation;
                                        oRecharges.First().CUSPMR_STATUS_DATE = DateTime.UtcNow;
                                        break;
                                    case PaymentMeanRechargeStatus.Committed:
                                        oRecharges.First().CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Refund;
                                        oRecharges.First().CUSPMR_STATUS_DATE = DateTime.UtcNow;
                                        break;
                                    default: 
                                        break;

                                }
                            }



                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RefundRecharge: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RefundRecharge: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundRecharge: ", e);
                bRes = false;
            }

            return bRes;



        }


        public bool ConfirmRecharge(ref USER user, decimal dRechargeId)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1 
                                     select r).First();


                        if (oUser != null)
                        {

                            var oRecharges = oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs.
                                    Where(r => r.CUSPMR_ID == dRechargeId);

                            if (oRecharges.Count() == 1)
                            {
                                switch ((PaymentMeanRechargeStatus)oRecharges.First().CUSPMR_TRANS_STATUS)
                                {
                                    case PaymentMeanRechargeStatus.Authorized:
                                        oRecharges.First().CUSPMR_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit;
                                        oRecharges.First().CUSPMR_STATUS_DATE = DateTime.UtcNow;
                                        break;
                                    default:
                                        break;

                                }

                            }


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ConfirmRecharge: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ConfirmRecharge: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConfirmRecharge: ", e);
                bRes = false;
            }

            return bRes;



        }




        public bool SetUserRechargeStatus(ref USER user, decimal dRechargeId,
                                        PaymentMeanRechargeStatus rechargeStatus, int? iCurrRetries)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId  
                                     select r).First();                      


                        if (oUser != null) 
                        {
                            var oRecharges = oUser.CUSTOMER_PAYMENT_MEANS_RECHARGEs.
                                    Where(r => r.CUSPMR_ID == dRechargeId);

                            if (oRecharges.Count() == 1)
                            {
                                if (iCurrRetries.HasValue)
                                {
                                    oRecharges.First().CUSPMR_RETRIES_NUM = iCurrRetries.Value;
                                }

                                oRecharges.First().CUSPMR_TRANS_STATUS = (int)rechargeStatus;
                                oRecharges.First().CUSPMR_STATUS_DATE = DateTime.UtcNow;

                            }

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            dbContext.Close();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "SetUserRechargeStatus: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserRechargeStatus: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserRechargeStatus: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool SetRechargeSecondaryTransactionInfo(decimal dRechargeId,
                                                        string strUserReference,
                                                        string strAuthResult,
                                                        string strGatewayDate,
                                                        string strCommitTransactionId,
                                                        int iTransactionFee,
                                                        string strTransactionFeeCurrencyIsocode,
                                                        string strTransactionURL,
                                                        string strRefundTransactionURL)
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
                        var oRecharges = dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.
                                            Where(r => r.CUSPMR_ID == dRechargeId);

                        if (oRecharges.Count() == 1)
                        {

                            oRecharges.First().CUSPMR_SECOND_TRANSACTION_ID = strCommitTransactionId;
                            oRecharges.First().CUSPMR_SECOND_OP_REFERENCE = strUserReference;
                            oRecharges.First().CUSPMR_SECOND_AUTH_RESULT = strAuthResult;
                            oRecharges.First().CUSPMR_SECOND_GATEWAY_DATE = strGatewayDate;

                            if (oRecharges.First().CUSPMR_TYPE == (int)PaymentMeanRechargeType.Paypal)
                            {
                                oRecharges.First().CUSPMR_PAYPAL_TRANSACTION_FEE_VALUE = iTransactionFee;
                                oRecharges.First().CUSPMR_PAYPAL_TRANSACTION_FEE_CURRENCY_ISOCODE = strTransactionFeeCurrencyIsocode;
                                oRecharges.First().CUSPMR_PAYPAL_TRANSACTION_URL = strTransactionURL;
                                oRecharges.First().CUSPMR_PAYPAL_TRANSACTION_REFUND_URL = strRefundTransactionURL;

                            }

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
                            m_Log.LogMessage(LogLevels.logERROR, "SetRechargeSecondaryTransactionInfo: ", e);
                            bRes = false;
                        }
                    }                       
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetRechargeSecondaryTransactionInfo: ", e);
                        bRes = false;
                    }
                }
                
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserRechargeStatus: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool SetRechargeSecondaryTransactionInfo(decimal dRechargeId,
                                                string strUserReference,
                                                string strAuthCode,
                                                string strAuthResult,
                                                string strAuthResultDesc,
                                                string strGatewayDate,
                                                string strRefundTransactionId)
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
                        var oRecharges = dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.
                                            Where(r => r.CUSPMR_ID == dRechargeId);

                        if (oRecharges.Count() == 1)
                        {

                            oRecharges.First().CUSPMR_SECOND_TRANSACTION_ID = strRefundTransactionId;
                            oRecharges.First().CUSPMR_SECOND_OP_REFERENCE = strUserReference;
                            oRecharges.First().CUSPMR_SECOND_AUTH_RESULT = strAuthResult;
                            oRecharges.First().CUSPMR_SECOND_GATEWAY_DATE = strGatewayDate;


                            /*if ((oRecharges.First().CUSTOMER_PAYMENT_MEAN.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard) &&
                                  (oRecharges.First().CUSTOMER_PAYMENT_MEAN.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER == (int)PaymentMeanCreditCardProviderType.pmccpIECISA))
                            {
                                var oRechargeInfo = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGES_INFOs
                                                     where r.CUSPMRI_OP_REFERENCE == strUserReference
                                                     select r).FirstOrDefault();


                                if (oRechargeInfo != null)
                                {

                                    DateTime dtDate = DateTime.Now;
                                    oRechargeInfo.CUSPMRI_STATUS = (int)PaymentMeanRechargeInfoStatus.Confirmed;
                                    oRechargeInfo.CUSPMRI_STATUS_DATE = dtDate;
                                    oRechargeInfo.CUSPMRI_STATUS_UTCDATE = dtDate + new TimeSpan(0, oRecharges.First().USER.USR_UTC_OFFSET, 0);
                                    oRechargeInfo.CUSPMRI_CONFIRM_RESULTCODE = strAuthResult;
                                    oRechargeInfo.CUSPMRI_CONFIRM_RESULTCODE_DESC = strAuthResultDesc;
                                    oRechargeInfo.CUSTOMER_PAYMENT_MEANS_RECHARGE = oRecharges.First();
                                    oRechargeInfo.CUSPMRI_TRANSACTION_ID = strRefundTransactionId;
                                    oRechargeInfo.CUSPMRI_CARD_REFERENCE = oRecharges.First().CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_CARD_REFERENCE;
                                    oRechargeInfo.CUSPMRI_AUTH_CODE = strAuthCode;
                                    oRechargeInfo.CUSPMRI_MASKED_CARD_NUMBER = oRecharges.First().CUSTOMER_PAYMENT_MEAN.CUSPM_TOKEN_MASKED_CARD_NUMBER;



                                }
                                else
                                {
                                    throw new Exception("Recharge Info is null");

                                }

                            }*/


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
                            m_Log.LogMessage(LogLevels.logERROR, "SetRechargeSecondaryTransactionInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetRechargeSecondaryTransactionInfo: ", e);
                        bRes = false;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserRechargeStatus: ", e);
                bRes = false;
            }

            return bRes;

        }






        public IQueryable<ALL_OPERATION> GetUserOperations(ref USER user,
                                                           Expression<Func<ALL_OPERATION, bool>> predicate,
                                                           string orderbyField, 
                                                           string orderbyDirection,                                               
                                                           int page,
                                                           int pagesize, 
                                                           out int iNumRows)
        {
            IQueryable<ALL_OPERATION> res = null;
            iNumRows = 0;
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

                    decimal userId = user.USR_ID;
                    res = (from r in dbContext.ALL_OPERATIONs
                           where r.OPE_USR_ID == userId
                           select r)
                           .Where(predicate)
                           .OrderBy(orderbyField + " " + orderbyDirection)
                           .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .AsQueryable();


                    iNumRows = (from r in dbContext.ALL_OPERATIONs
                                where r.OPE_USR_ID == userId
                                select r)
                                .Where(predicate).Count();

                    transaction.Complete();
                }
                

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserOperations: ", e);
            }

            return res;
        }

        public IQueryable<ALL_OPERATION> GetUserOperations(ref USER user,
                                                           Expression<Func<ALL_OPERATION, bool>> predicate,
                                                           string orderbyField,
                                                           string orderbyDirection)
        {
            IQueryable<ALL_OPERATION> res = null;            
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

                    decimal userId = user.USR_ID;
                    res = (from r in dbContext.ALL_OPERATIONs
                           where r.OPE_USR_ID == userId
                           select r)
                           .Where(predicate)
                           .OrderBy(orderbyField + " " + orderbyDirection)
                           .AsQueryable();

                    transaction.Complete();
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserOperations: ", e);
            }

            return res;
        }


        public IQueryable<CUSTOMER_INVOICE> GetUserInvoices(ref USER user,
                                                           Expression<Func<CUSTOMER_INVOICE, bool>> predicate,
                                                           string orderbyField,
                                                           string orderbyDirection,
                                                           int page,
                                                           int pagesize,
                                                           out int iNumRows)
        {
            IQueryable<CUSTOMER_INVOICE> res = null;
            iNumRows = 0;
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

                    decimal cusId = user.CUSTOMER.CUS_ID;
                    res = (from r in dbContext.CUSTOMER_INVOICEs
                           where r.CUSINV_CUS_ID == cusId &&
                                 r.CUSINV_INV_NUMBER != null
                           select r)
                           .Where(predicate)
                           .OrderBy(orderbyField + " " + orderbyDirection)
                           .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .AsQueryable();


                    iNumRows = (from r in dbContext.CUSTOMER_INVOICEs
                                where r.CUSINV_CUS_ID == cusId &&
                                      r.CUSINV_INV_NUMBER != null
                                select r)
                                .Where(predicate).Count();

                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserInvoices: ", e);
            }

            return res;
        }


        public IQueryable<ALL_OPERATION> GetUserOperations(ref USER user, int iNumDaysToGoBack)
        {
            IQueryable<ALL_OPERATION> res = null;

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

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();


                    if (oUser != null)
                    {

                        res = (from r in dbContext.ALL_OPERATIONs
                               where r.OPE_USR_ID == userId &&
                                     (dbContext.GetUTCDate() - r.OPE_INSERTION_UTC_DATE.Value).TotalDays < Math.Min(ctnMaxDaysToGoBack, iNumDaysToGoBack)
                               orderby r.OPE_INSERTION_UTC_DATE descending
                               select r).
                               AsQueryable();


                    }

                    transaction.Complete();
                }
             
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserOperations: ", e);
            }

            return res;
        }


        public List<USER_OPERATIONS_HIDDEN> GetUserHiddenOperations(ref USER user)
        {
            List<USER_OPERATIONS_HIDDEN> res = null;

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

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();


                    if (oUser != null)
                    {

                        res = (from r in dbContext.USER_OPERATIONS_HIDDENs
                               where r.UOPHI_USR_ID == userId
                               select r).ToList();
                               

                    }

                    transaction.Complete();
                }


                if (res == null)
                {
                    res=new List<USER_OPERATIONS_HIDDEN>();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserHiddenOperations: ", e);
            }

            return res;
        }


        public bool HideUserOperation(ref USER user, ChargeOperationsType opType, decimal dOPID)
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

                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();


                    if (oUser != null)
                    {

                        int iCount = (from r in dbContext.USER_OPERATIONS_HIDDENs
                                      where r.UOPHI_USR_ID == userId &&
                                            r.UOPHI_OP_TYPE == (int)opType &&
                                            r.UOPHI_OP_ID == dOPID
                                      select r.UOPHI_ID).Count();


                        if (iCount == 0)
                        {

                            oUser.USER_OPERATIONS_HIDDENs.Add(new USER_OPERATIONS_HIDDEN()
                            {
                                UOPHI_OP_TYPE = (int)opType,
                                UOPHI_OP_ID = dOPID,
                            });

                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();
                                user = oUser;
                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "HideUserOperation: ", e);
                                bRes = false;
                            }


                        }                   

                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "HideUserOperation: ", e);
            }

            return bRes;
        }


        public IEnumerable<OPERATION> GetUserPlateLastOperation(ref USER user,
                    out int iNumRows)

        {
            List<OPERATION> res = new List<OPERATION>();
            iNumRows = 0;
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

                    decimal userId = user.USR_ID;


                    foreach (USER_PLATE plate in user.USER_PLATEs.OrderBy(t => t.USRP_PLATE))
                    {
                        if (plate.USRP_ENABLED == 1)
                        {
                            try
                            {
                                var resplate = (from r in dbContext.OPERATIONs
                                                where ((r.OPE_USR_ID == userId) && r.USER.USR_ENABLED == 1 &&
                                                (r.OPE_USRP_ID == plate.USRP_ID))
                                                orderby r.OPE_DATE descending
                                                select r).First();
                                res.Add(resplate);
                                iNumRows++;

                            }
                            catch
                            {

                            }

                        }
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserPlateLastOperation: ", e);
            }

            return (IEnumerable<OPERATION>)res;
        }


        public bool GetOperationsPlatesAndZonesStatistics(ref USER user,
                                                            out string strMostUsedPlate,
                                                            out string strLastUsedPlate,
                                                            out decimal? dMostUsedZone,                                                            
                                                            out decimal? dLastUsedZone,
                                                            out decimal? dMostUsedTariff,
                                                            out decimal? dLastUsedTariff)
        {
            bool bRes = true;

            strMostUsedPlate="";
            strLastUsedPlate="";
            dMostUsedZone=null;
            dLastUsedZone = null;
            dMostUsedTariff = null;
            dLastUsedTariff = null;

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

                    decimal userId = user.USR_ID;


                    try
                    {
                        var resplate = (from r in dbContext.OPERATIONs
                                        where r.OPE_USR_ID == userId && r.USER.USR_ENABLED == 1 && r.USER_PLATE.USRP_ENABLED == 1
                                        group r by r.USER_PLATE.USRP_PLATE into pGroup
                                        let count = pGroup.Count()
                                        orderby count descending
                                        select new { Count = count, pGroup.Key }).First();

                        strMostUsedPlate = resplate.Key;

                    }
                    catch
                    {

                    }


                    try
                    {
                        var resplate = (from r in dbContext.OPERATIONs
                                        where r.OPE_USR_ID == userId && r.USER_PLATE.USRP_ENABLED == 1
                                        orderby r.OPE_DATE descending
                                        select r).First();

                        strLastUsedPlate = resplate.USER_PLATE.USRP_PLATE;

                    }
                    catch
                    {

                    }


                    try
                    {
                        var resGroup = (from r in dbContext.OPERATIONs
                                        where r.OPE_USR_ID == userId
                                        group r by new { groupId = r.OPE_GRP_ID, tariffId = r.OPE_TAR_ID } into pGroup
                                        let count = pGroup.Count()
                                        orderby count descending
                                        select new { Count = count, pGroup.Key.groupId, pGroup.Key.tariffId }).First();

                        dMostUsedZone = resGroup.groupId;
                        dMostUsedTariff = resGroup.tariffId;

                    }
                    catch
                    {

                    }


                    try
                    {
                        var resGroup = (from r in dbContext.OPERATIONs
                                        where r.OPE_USR_ID == userId
                                        orderby r.OPE_DATE descending
                                        select r).First();

                        dLastUsedZone = resGroup.OPE_GRP_ID;
                        dLastUsedTariff = resGroup.OPE_TAR_ID;

                    }
                    catch
                    {

                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsPlatesAndZonesStatistics: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool IsPlateOfUser(ref USER user, string strPlate)
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

                    decimal userId = user.USR_ID;


                    try
                    {
                        var resplate = (from r in dbContext.USER_PLATEs
                                        where r.USRP_USR_ID == userId && r.USER.USR_ENABLED == 1 &&
                                        r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") &&
                                        r.USRP_ENABLED == 1
                                        select r).First();

                        bRes = true;

                    }
                    catch
                    {

                    }
                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsPlateOfUser: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool IsPlateAssignedToAnotherUser(ref USER user, string strPlate)
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

                    decimal userId = user.USR_ID;


                    try
                    {
                        int iNumAssigns = (from r in dbContext.USER_PLATEs
                                           where r.USRP_USR_ID != userId && r.USER.USR_ENABLED == 1 &&
                                           r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") &&
                                           r.USRP_ENABLED == 1
                                           select r).Count();


                        bRes = (iNumAssigns > 0);

                    }
                    catch
                    {

                    }
                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsPlateAssignedToAnotherUser: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool IsPlateAssignedToAnotherUser(string strPlate)
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

                    try
                    {
                        int iNumAssigns = (from r in dbContext.USER_PLATEs
                                           where r.USER.USR_ENABLED == 1 &&
                                           r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") &&
                                           r.USRP_ENABLED == 1
                                           select r).Count();


                        bRes = (iNumAssigns > 0);

                    }
                    catch
                    {

                    }
                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsPlateAssignedToAnotherUser: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool AddPlateToUser(ref USER user, string strPlate)
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED==1
                                     select r).First();

                        if (oUser != null)
                        {
                            oUser.USER_PLATEs.Add(new USER_PLATE()
                            {
                                USRP_ENABLED = 1,
                                USRP_PLATE = strPlate.ToUpper().Trim().Replace(" ", ""),
                                USRP_IS_DEFAULT = 0

                            });

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddPlateToUser: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddPlateToUser: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddPlateToUser: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool StartSession(ref USER user, decimal dInsId, int? iOSID, string strPushID, string strMACWIFI, string strIMEI, string strCellModel,
                                 string strOSVersion, string strPhoneSerialNumber,string strCulture, string strAppVersion, bool bSessionKeepAlive, out string strSessionID)
        {
            bool bRes = false;
            strSessionID = "";
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
                        decimal userId = user.USR_ID;

                        var oSessions = (from r in dbContext.MOBILE_SESSIONs
                                            where r.MOSE_USR_ID == userId && r.USER.USR_ENABLED==1 &&
                                                r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)
                                            select r).AsEnumerable();

                        var oUser = (from r in dbContext.USERs
                                        where r.USR_ID == userId && r.USR_ENABLED == 1
                                        select r).First();

                        int iSessionTimeoutInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["SessionTimeoutInMinutes"]);

                        foreach (MOBILE_SESSION session in oSessions)
                        {
                            TimeSpan ts = DateTime.UtcNow - session.MOSE_LAST_UPDATE_TIME;
                            if (ts.TotalMinutes > iSessionTimeoutInMinutes)
                            {
                                session.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }
                            else if ((!string.IsNullOrEmpty(strMACWIFI)) && (!string.IsNullOrEmpty(session.MOSE_CELL_WIFI_MAC))
                                && (session.MOSE_CELL_WIFI_MAC == strMACWIFI))
                            {
                                session.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);

                            }
                            else if ((!string.IsNullOrEmpty(strIMEI)) && (!string.IsNullOrEmpty(session.MOSE_CELL_IMEI))
                                && (session.MOSE_CELL_IMEI == strIMEI))
                            {
                                session.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);

                            }
                            else if ((string.IsNullOrEmpty(strIMEI)) && (string.IsNullOrEmpty(session.MOSE_CELL_IMEI)) &&
                                        (string.IsNullOrEmpty(strMACWIFI)) && (string.IsNullOrEmpty(session.MOSE_CELL_WIFI_MAC)) &&
                                        (iOSID != null) && (session.MOSE_OS != null) && (iOSID == session.MOSE_OS))
                            {
                                session.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }


                        }

                        MOBILE_SESSION oNewSession = new MOBILE_SESSION
                        {
                            MOSE_CREATION_TIME = DateTime.UtcNow,
                            MOSE_LAST_UPDATE_TIME = DateTime.UtcNow,
                            MOSE_USR_ID = user.USR_ID,
                            MOSE_CELL_WIFI_MAC = strMACWIFI,
                            MOSE_CELL_IMEI = strIMEI,
                            MOSE_OS = iOSID,
                            MOSE_CELL_MODEL = strCellModel,
                            MOSE_OS_VERSION = strOSVersion,
                            MOSE_CELL_SERIALNUMBER = strPhoneSerialNumber,
                            MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Open),
                            MOSE_SESSIONID = GenerateId() + GenerateId() + GenerateId(),
                            MOSE_INS_ID = dInsId,
                            MOSE_CULTURE_LANG = strCulture
                        };

                        strSessionID = oNewSession.MOSE_SESSIONID;

                        if ((!string.IsNullOrEmpty(strPushID)) && (iOSID.HasValue))
                        {
                            bool bFound = false;
                            var oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_PUSHID == strPushID);


                            if (oPushID.Count() != 0)
                            {
                                oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                oPushID.First().UPID_APP_VERSION = strAppVersion;
                                oPushID.First().UPID_APP_SESSION_KEEP_ALIVE = bSessionKeepAlive ? 1 : 0;
                                if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_MODEL))
                                    oPushID.First().UPID_CELL_MODEL = strCellModel;
                                if (string.IsNullOrEmpty(oPushID.First().UPID_OS_VERSION))
                                    oPushID.First().UPID_OS_VERSION = strOSVersion;
                                if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_SERIALNUMBER))
                                    oPushID.First().UPID_CELL_SERIALNUMBER = strPhoneSerialNumber;
                                oNewSession.USERS_PUSH_ID = oPushID.First();
                                bFound = true;
                            }

                            if ((!bFound) && (!string.IsNullOrEmpty(strMACWIFI)))
                            {
                                oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_CELL_WIFI_MAC == strMACWIFI && r.UPID_OS == iOSID);
                                if (oPushID.Count() != 0)
                                {
                                    oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                    oPushID.First().UPID_PUSHID = strPushID;
                                    oPushID.First().UPID_APP_VERSION = strAppVersion;
                                    oPushID.First().UPID_APP_SESSION_KEEP_ALIVE = bSessionKeepAlive ? 1 : 0;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_MODEL))
                                        oPushID.First().UPID_CELL_MODEL = strCellModel;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_OS_VERSION))
                                        oPushID.First().UPID_OS_VERSION = strOSVersion;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_SERIALNUMBER))
                                        oPushID.First().UPID_CELL_SERIALNUMBER = strPhoneSerialNumber;

                                    oNewSession.USERS_PUSH_ID = oPushID.First();


                                    if ((string.IsNullOrEmpty(oPushID.First().UPID_CELL_IMEI)) &&
                                        (!string.IsNullOrEmpty(strIMEI)))
                                    {
                                        oPushID.First().UPID_CELL_IMEI = strIMEI;
                                    }


                                    bFound = true;
                                }
                            }

                            if ((!bFound) && (!string.IsNullOrEmpty(strIMEI)))
                            {
                                oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_CELL_IMEI == strIMEI && r.UPID_OS == iOSID);
                                if (oPushID.Count() != 0)
                                {
                                    oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                    oPushID.First().UPID_PUSHID = strPushID;
                                    oPushID.First().UPID_APP_VERSION = strAppVersion;
                                    oPushID.First().UPID_APP_SESSION_KEEP_ALIVE = bSessionKeepAlive ? 1 : 0;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_MODEL))
                                        oPushID.First().UPID_CELL_MODEL = strCellModel;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_OS_VERSION))
                                        oPushID.First().UPID_OS_VERSION = strOSVersion;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_SERIALNUMBER))
                                        oPushID.First().UPID_CELL_SERIALNUMBER = strPhoneSerialNumber;

                                    oNewSession.USERS_PUSH_ID = oPushID.First();


                                    if ((string.IsNullOrEmpty(oPushID.First().UPID_CELL_WIFI_MAC)) &&
                                        (!string.IsNullOrEmpty(strMACWIFI)))
                                    {
                                        oPushID.First().UPID_CELL_WIFI_MAC = strMACWIFI;
                                    }

                                    bFound = true;
                                }
                            }

                            if ((!bFound) && (string.IsNullOrEmpty(strMACWIFI)) && (string.IsNullOrEmpty(strIMEI)))
                            {
                                oPushID = oUser.USERS_PUSH_IDs.Where(r => string.IsNullOrEmpty(r.UPID_CELL_IMEI) &&
                                                                            string.IsNullOrEmpty(r.UPID_CELL_WIFI_MAC) &&
                                                                            r.UPID_OS == iOSID);
                                if (oPushID.Count() != 0)
                                {
                                    oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                    oPushID.First().UPID_PUSHID = strPushID;
                                    oPushID.First().UPID_APP_VERSION = strAppVersion;
                                    oPushID.First().UPID_APP_SESSION_KEEP_ALIVE = bSessionKeepAlive ? 1 : 0;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_MODEL))
                                        oPushID.First().UPID_CELL_MODEL = strCellModel;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_OS_VERSION))
                                        oPushID.First().UPID_OS_VERSION = strOSVersion;
                                    if (string.IsNullOrEmpty(oPushID.First().UPID_CELL_SERIALNUMBER))
                                        oPushID.First().UPID_CELL_SERIALNUMBER = strPhoneSerialNumber;
                                    
                                    oNewSession.USERS_PUSH_ID = oPushID.First();

                                    bFound = true;
                                }
                            }


                            if (!bFound)
                            {
                                USERS_PUSH_ID oNewPushId= new USERS_PUSH_ID
                                {
                                    UPID_PUSHID = strPushID,
                                    UPID_OS = iOSID.Value,
                                    UPID_APP_VERSION = strAppVersion,
                                    UPID_APP_SESSION_KEEP_ALIVE = bSessionKeepAlive ? 1 : 0,
                                    UPID_CELL_WIFI_MAC = strMACWIFI,
                                    UPID_CELL_IMEI = strIMEI,
                                    UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow,
                                    UPID_PUSH_RETRIES = 0,
                                    UPID_CELL_MODEL = strCellModel,
                                    UPID_OS_VERSION = strOSVersion,
                                    UPID_CELL_SERIALNUMBER = strPhoneSerialNumber

                                };
                                oUser.USERS_PUSH_IDs.Add(oNewPushId);
                                oNewSession.USERS_PUSH_ID = oNewPushId;


                                bFound=true;

                            }
                        }

                        // Add the new object to the Orders collection.
                        dbContext.MOBILE_SESSIONs.InsertOnSubmit(oNewSession);

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                            bRes = true;

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "StartSession: ", e);
                            bRes = false;
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "StartSession: ", e);
                        bRes = false;
                    }
                }
                
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "StartSession: ", e);
                bRes = false;
            }

            return bRes;
        }



        public bool UpdateSession(ref USER user, string strSessionID, string strPushID, string strMACWIFI, string strIMEI, bool bUpdateSessionTime, out decimal? dInsId,
                                  out string strCulture, out string strAppVersion)
        {
            bool bRes = false;
            dInsId = null;
            strCulture = user.USR_CULTURE_LANG;
            strAppVersion = "";
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
                        decimal userId = user.USR_ID;



                        var oSessions = (from r in dbContext.MOBILE_SESSIONs
                                            where r.MOSE_USR_ID == userId && r.USER.USR_ENABLED==1 &&
                                                r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open) &&
                                                r.MOSE_SESSIONID == strSessionID
                                            select r);

                        if (oSessions.Count() > 0)
                        {

                            var oSession = oSessions.First();

                            var oUser = (from r in dbContext.USERs
                                            where r.USR_ID == userId
                                            select r).First();

                            int iSessionTimeoutInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["SessionTimeoutInMinutes"]);

                            TimeSpan ts = DateTime.UtcNow - oSession.MOSE_LAST_UPDATE_TIME;
                            if((ts.TotalMinutes > iSessionTimeoutInMinutes)&&(oSession.USERS_PUSH_ID == null))                                
                            {
                                oSession.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }
                            else if ((ts.TotalMinutes > iSessionTimeoutInMinutes) && (!oSession.USERS_PUSH_ID.UPID_APP_SESSION_KEEP_ALIVE.HasValue))
                            {
                                oSession.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }
                            else if ((ts.TotalMinutes > iSessionTimeoutInMinutes) && (oSession.USERS_PUSH_ID.UPID_APP_SESSION_KEEP_ALIVE == 0))
                            {
                                oSession.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }
                            else if ((ts.TotalMinutes > iSessionTimeoutInMinutes) && (oSession.USERS_PUSH_ID.UPID_APP_SESSION_KEEP_ALIVE == 1) && (!bUpdateSessionTime))
                            {
                                oSession.MOSE_STATUS = Convert.ToInt32(MobileSessionStatus.Closed);
                            }
                            else
                            {
                                if ((!string.IsNullOrEmpty(strMACWIFI)) && (string.IsNullOrEmpty(oSession.MOSE_CELL_WIFI_MAC)))
                                {
                                    oSession.MOSE_CELL_WIFI_MAC = strMACWIFI;
                                    if (oSession.USERS_PUSH_ID != null)
                                    {
                                        oSession.USERS_PUSH_ID.UPID_CELL_WIFI_MAC = strMACWIFI;
                                    }

                                }
                                else if ((!string.IsNullOrEmpty(strIMEI)) && (string.IsNullOrEmpty(oSession.MOSE_CELL_IMEI)))
                                {
                                    oSession.MOSE_CELL_IMEI = strIMEI;
                                    if (oSession.USERS_PUSH_ID != null)
                                    {
                                        oSession.USERS_PUSH_ID.UPID_CELL_IMEI = strIMEI;
                                    }
                                }




                                if ((!string.IsNullOrEmpty(strPushID)) && (oSession.MOSE_OS.HasValue))
                                {
                                    bool bFound = false;
                                    var oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_PUSHID == strPushID);


                                    if (oPushID.Count() != 0)
                                    {
                                        oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                        oSession.USERS_PUSH_ID = oPushID.First();
                                        bFound = true;
                                    }

                                    if ((!bFound) && (!string.IsNullOrEmpty(strMACWIFI)))
                                    {
                                        oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_CELL_WIFI_MAC == strMACWIFI && r.UPID_OS == oSession.MOSE_OS);
                                        if (oPushID.Count() != 0)
                                        {
                                            oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                            oPushID.First().UPID_PUSHID = strPushID;
                                            oSession.USERS_PUSH_ID = oPushID.First();


                                            if ((string.IsNullOrEmpty(oPushID.First().UPID_CELL_IMEI)) &&
                                                                            (!string.IsNullOrEmpty(strIMEI)))
                                            {
                                                oPushID.First().UPID_CELL_IMEI = strIMEI;
                                            }

                                            bFound = true;
                                        }
                                    }

                                    if ((!bFound) && (!string.IsNullOrEmpty(strIMEI)))
                                    {
                                        oPushID = oUser.USERS_PUSH_IDs.Where(r => r.UPID_CELL_IMEI == strIMEI && r.UPID_OS == oSession.MOSE_OS);
                                        if (oPushID.Count() != 0)
                                        {
                                            oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                            oPushID.First().UPID_PUSHID = strPushID;
                                            oSession.USERS_PUSH_ID = oPushID.First();

                                            if ((string.IsNullOrEmpty(oPushID.First().UPID_CELL_WIFI_MAC)) &&
                                                (!string.IsNullOrEmpty(strMACWIFI)))
                                            {
                                                oPushID.First().UPID_CELL_WIFI_MAC = strMACWIFI;
                                            }

                                            bFound = true;
                                        }
                                    }

                                    if ((!bFound) && (string.IsNullOrEmpty(strMACWIFI)) && (string.IsNullOrEmpty(strIMEI)))
                                    {
                                        oPushID = oUser.USERS_PUSH_IDs.Where(r => string.IsNullOrEmpty(r.UPID_CELL_IMEI) &&
                                                                                    string.IsNullOrEmpty(r.UPID_CELL_WIFI_MAC) &&
                                                                                    r.UPID_OS == oSession.MOSE_OS);
                                        if (oPushID.Count() != 0)
                                        {
                                            oPushID.First().UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow;
                                            oPushID.First().UPID_PUSHID = strPushID;
                                            oSession.USERS_PUSH_ID = oPushID.First();
                                            bFound = true;
                                        }
                                    }


                                    if (!bFound)
                                    {
                                        USERS_PUSH_ID  oNewPushId= new USERS_PUSH_ID
                                        {
                                            UPID_PUSHID = strPushID,
                                            UPID_OS = oSession.MOSE_OS.Value,
                                            UPID_APP_SESSION_KEEP_ALIVE = (oSession.USERS_PUSH_ID!=null)? oSession.USERS_PUSH_ID.UPID_APP_SESSION_KEEP_ALIVE: null,
                                            UPID_APP_VERSION = (oSession.USERS_PUSH_ID!=null)? oSession.USERS_PUSH_ID.UPID_APP_VERSION:"",
                                            UPID_CELL_WIFI_MAC = strMACWIFI,
                                            UPID_CELL_IMEI = strIMEI,
                                            UPID_LAST_UPDATE_DATETIME = DateTime.UtcNow,
                                            UPID_PUSH_RETRIES = 0
                                        };

                                        oUser.USERS_PUSH_IDs.Add(oNewPushId);
                                        oSession.USERS_PUSH_ID = oNewPushId;

                                    }

                                    

                                }

                                if (bUpdateSessionTime)
                                    oSession.MOSE_LAST_UPDATE_TIME = DateTime.UtcNow;
                            }


                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                bRes = (oSession.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open));
                                if (bRes)
                                {
                                    dInsId = oSession.MOSE_INS_ID;
                                    strCulture = oSession.MOSE_CULTURE_LANG ?? oUser.USR_CULTURE_LANG;
                                    if (oSession.USERS_PUSH_ID != null)
                                    {
                                        strAppVersion = oSession.USERS_PUSH_ID.UPID_APP_VERSION;
                                    }
                                }
                                user = oUser;
                                transaction.Complete();

                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "UpdateSession: ", e);
                                bRes = false;
                            }
                        }
                        else
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateSession: Session not existing or closed: " + strSessionID);
                            bRes = false;
                        }



                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateSession: ", e);
                        bRes = false;
                    }
                }
                
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateSession: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool GetUserFromOpenSession(string strSessionID, ref Dictionary<string, object> oUserDataDict)
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

                        var oSessions = (from r in dbContext.MOBILE_SESSIONs
                                         where r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open) &&
                                             r.MOSE_SESSIONID == strSessionID
                                         select r);

                        if (oSessions.Count() > 0)
                        {

                            var oSession = oSessions.First();
                            USER oUser = oSession.USER;

                            bRes = GetZendeskUserDataDict(ref oUser, ref oUserDataDict);

                        }



                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetUserFromOpenSession: ", e);
                        bRes = false;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserFromOpenSession: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool GetUserFromOpenSession(string strSessionID, out decimal dInstallationID, ref USER oUser)
        {
            bool bRes = false;
            oUser = null;
            dInstallationID = -1;

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

                        var oSessions = (from r in dbContext.MOBILE_SESSIONs
                                         where r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open) &&
                                             r.MOSE_SESSIONID == strSessionID
                                         select r);

                        if (oSessions.Count() > 0)
                        {

                            var oSession = oSessions.First();
                            dInstallationID = oSession.INSTALLATION.INS_ID;
                            oUser = oSession.USER;
                            bRes = true;
                        }



                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetUserFromOpenSession: ", e);
                        bRes = false;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserFromOpenSession: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool GetWaitingUserReplications(out List<stUserReplicationResult> oReplications, out int iQueueLength, UserReplicationWSSignatureType eSignatureType,
                                                           int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries,
                                                           int iMaxResendTime,
                                                           int iSecondsWait, int iMaxOperationsToReturn)
        {
            bool bRes = true;
            oReplications = null;
            iQueueLength = 0;

            try
            {
                if (iMaxOperationsToReturn > 0)
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                                   new TransactionOptions()
                                                                                                   {
                                                                                                       IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                       Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                                   }))
                    {

                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();
                      



                        var oAllOperations = (from r in dbContext.USERS_REPLICATIONs
                                              where ((r.USRREP_CONFIRMED_IN_WS == (int)UserReplicationStatus.Inserted && r.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_SIGNATURE_TYPE == (int)eSignatureType &&
                                                     (!r.USRREP_CONFIRM_IN_WS_RETRIES_NUM.HasValue || r.USRREP_CONFIRM_IN_WS_RETRIES_NUM == 0)) ||
                                                     (r.USRREP_CONFIRMED_IN_WS == (int)UserReplicationStatus.Error && r.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_SIGNATURE_TYPE == (int)eSignatureType &&
                                                     (r.USRREP_CONFIRM_IN_WS_RETRIES_NUM >0) && 
                                                     (DateTime.UtcNow >= r.USRREP_STATUS_UTC_DATE.AddSeconds(Math.Min(iMaxResendTime, r.USRREP_CONFIRM_IN_WS_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry))))&&                                                       
                                                     (r.USRREP_INSERT_UTC_DATE.AddSeconds(iSecondsWait) <= dbContext.GetUTCDate()))
                                              select r)                                          
                                           .OrderBy(r => r.USRREP_INSERT_UTC_DATE)
                                           .AsQueryable();


                        if (oAllOperations.Count() > 0)
                        {
                            iQueueLength = oAllOperations.Count();
                            var oReps = oAllOperations.Take(iMaxOperationsToReturn).ToList();

                            oReplications = new List<stUserReplicationResult>();
                            foreach (USERS_REPLICATION oRep in oReps)
                            {
                                oReplications.Add(new stUserReplicationResult
                                                    {
                                                        m_dRepId = oRep.USRREP_ID,
                                                        m_dtStatusDate = oRep.USRREP_STATUS_UTC_DATE,
                                                        m_strExternalReplicationId = oRep.USRREP_EXT_REPLICATION_ID,
                                                        m_iInJobOrder = oRep.USRREP_IN_JOB_ORDER,
                                                        m_strJobId = oRep.USRREP_JOB_ID,
                                                        m_strJobURL = oRep.USRREP_JOB_STATUS_URL,
                                                        m_strReplicationError = oRep.USRREP_EXT_REPLICATION_ERROR_TEXT,
                                                        m_eUserReplicationStatus = (UserReplicationStatus)oRep.USRREP_CONFIRMED_IN_WS,
                                                        m_iCurrRetries = oRep.USRREP_CONFIRM_IN_WS_RETRIES_NUM,
                                                        m_iQueueBeforeReplication = oRep.USRREP_QUEUE_LENGTH_BEFORE_CONFIRM_WS,
                                                        m_dReplicationTime = oRep.USRREP_CONFIRMATION_TIME_IN_WS
                                                    });

                            }


                        }

                        dbContext.Close();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingUserReplications: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetWaitingQueuedUserReplications(out List<stUserReplicationResult> oReplications, out int iQueueLength, UserReplicationWSSignatureType eSignatureType,
                                                     int iNumSecondsToWaitInQueuedState,ref string strUsername, ref string strPassword)
        {
            bool bRes = true;
            oReplications = null;
            iQueueLength = 0;

            try
            {
                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                                   new TransactionOptions()
                                                                                                   {
                                                                                                       IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                       Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                                   }))
                    {

                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();




                        var oFirstReplicationToCheck = (from r in dbContext.USERS_REPLICATIONs
                                                          where r.USRREP_CONFIRMED_IN_WS == (int)UserReplicationStatus.Queued && 
                                                                r.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_SIGNATURE_TYPE == (int)eSignatureType &&                                                    
                                                                DateTime.UtcNow >= r.USRREP_STATUS_UTC_DATE.AddSeconds(iNumSecondsToWaitInQueuedState)
                                                          select r)
                                                       .OrderBy(r => r.USRREP_STATUS_UTC_DATE).FirstOrDefault();


                        if (oFirstReplicationToCheck != null)
                        {
                            string strURLJob = oFirstReplicationToCheck.USRREP_JOB_STATUS_URL;
                            strUsername=oFirstReplicationToCheck.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_HTTP_USER;
                            strPassword=oFirstReplicationToCheck.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_HTTP_PASSWORD;

                            var oDbReplications= (from r in dbContext.USERS_REPLICATIONs
                                                            where r.USRREP_CONFIRMED_IN_WS == (int)UserReplicationStatus.Queued &&
                                                                  r.USRREP_JOB_STATUS_URL == strURLJob
                                                            select r)
                                                     .OrderBy(r => r.USRREP_STATUS_UTC_DATE).AsQueryable();

                            oReplications = new List<stUserReplicationResult>();

                            foreach (USERS_REPLICATION oRep in oDbReplications)
                            {
                                oReplications.Add(new stUserReplicationResult
                                {
                                    m_dRepId = oRep.USRREP_ID,
                                    m_dtStatusDate = oRep.USRREP_STATUS_UTC_DATE,
                                    m_strExternalReplicationId = oRep.USRREP_EXT_REPLICATION_ID,
                                    m_iInJobOrder = oRep.USRREP_IN_JOB_ORDER,
                                    m_strJobId = oRep.USRREP_JOB_ID,
                                    m_strJobURL = oRep.USRREP_JOB_STATUS_URL,
                                    m_strReplicationError = oRep.USRREP_EXT_REPLICATION_ERROR_TEXT,
                                    m_eUserReplicationStatus = (UserReplicationStatus)oRep.USRREP_CONFIRMED_IN_WS,
                                    m_iCurrRetries = oRep.USRREP_CONFIRM_IN_WS_RETRIES_NUM,
                                    m_iQueueBeforeReplication = oRep.USRREP_QUEUE_LENGTH_BEFORE_CONFIRM_WS,
                                    m_dReplicationTime = oRep.USRREP_CONFIRMATION_TIME_IN_WS

                                });

                            }


                        }

                        dbContext.Close();
                    }
                

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingUserReplications: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool GetZendeskUserDataDict(ref List<stUserReplicationResult> oUsersReps, ref Dictionary<string, object> oUsersDataDict, 
                                           ref string strURL, ref string strUsername, ref string strPassword)
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

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                    var predicate = PredicateBuilder.False<USERS_REPLICATION>();

                    if ((oUsersReps != null) && (oUsersReps.Count() > 0))
                        {
                            foreach (stUserReplicationResult oUserRep in oUsersReps)
                            {
                                predicate = predicate.Or(a => a.USRREP_ID == oUserRep.m_dRepId);
                            }
                        }


                    var oUserReplications = (from r in dbContext.USERS_REPLICATIONs
                                        select r)
                                        .Where(predicate);


                    System.Collections.ArrayList oArray = new ArrayList();
                    Dictionary<string, object> oUserDataDict = null;
                    int i = 1;

                    foreach (USERS_REPLICATION oRep in oUserReplications)
                    {
                        if (i == 1)
                        {
                            strURL = oRep.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_URL;
                            strUsername = oRep.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_HTTP_USER;
                            strPassword = oRep.USERS_REPLICATION_CONFIG.USRREPC_CONFIRM_WS_HTTP_PASSWORD;
                        }

                        oUserDataDict = null;
                        USER oUser = oRep.USER;
                        stUserReplicationResult oReplication = oUsersReps.Where(r => r.m_dRepId == oRep.USRREP_ID).FirstOrDefault();
                        oUsersReps.Remove(oReplication);
                        if (GetZendeskUserDataDict(ref oUser, ref oUserDataDict))
                        {
                            oReplication.m_iInJobOrder = i;
                            oUsersReps.Add(oReplication);
                            oArray.Add(oUserDataDict);
                            i++;
                        }

                       
                    }

                    if (oArray.Count> 0)
                    {
                        oUsersDataDict = new Dictionary<string, object>();
                        oUsersDataDict.Add("users", oArray);
                        bRes = true;
                    }

                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserReplication: ", e);
                bRes = false;
            }

            return bRes;


        }


        protected bool GetZendeskUserDataDict(ref USER oUser, ref Dictionary<string, object> oUserDataDict)
        {
            bool bRes = false;
            try
            {

                if (oUserDataDict == null)
                    oUserDataDict = new Dictionary<string, object>();

                string strPlates = "";
                int i=0;

                foreach (USER_PLATE oPlate in oUser.USER_PLATEs.Where(r => r.USRP_ENABLED == 1))
                {
                    if (i > 0)
                        strPlates += ",";
                    i++;
                    strPlates += oPlate.USRP_PLATE;
                }

                var oDevices = oUser.USERS_PUSH_IDs;
                string strVersions="";
                string strSOs = "";
                string strDeviceModels="";
                string strSerialNumbers="";

                i = 0;
                foreach (USERS_PUSH_ID oDevice in oDevices)
                {
                    if (i == 0)
                    {
                        strVersions += "|";
                        strSOs += "|";
                        strDeviceModels += "|";
                        strSerialNumbers += "|";
                    }

                    i++;
                    strVersions += string.Format(" {0} |", !string.IsNullOrEmpty(oDevice.UPID_APP_VERSION)?oDevice.UPID_APP_VERSION:"--");
                    strSOs += string.Format(" {0} |", (MobileOS)oDevice.UPID_OS);
                    strDeviceModels += string.Format(" {0} |", !string.IsNullOrEmpty(oDevice.UPID_CELL_MODEL) ? oDevice.UPID_CELL_MODEL : "--");
                    strSerialNumbers += string.Format(" {0} |", !string.IsNullOrEmpty(oDevice.UPID_CELL_SERIALNUMBER) ? oDevice.UPID_CELL_SERIALNUMBER : "--");
                }


                if ((string.IsNullOrEmpty(oUser.CUSTOMER.CUS_NAME)) && (string.IsNullOrEmpty(oUser.CUSTOMER.CUS_SURNAME1)) && (string.IsNullOrEmpty(oUser.CUSTOMER.CUS_SURNAME2)))
                {
                    oUserDataDict.Add("name", oUser.USR_EMAIL);
                }
                else
                {
                    oUserDataDict.Add("name", oUser.CUSTOMER.CUS_NAME + " " + oUser.CUSTOMER.CUS_SURNAME1 + " " + oUser.CUSTOMER.CUS_SURNAME2);
                }
                
                oUserDataDict.Add("email", oUser.USR_EMAIL);                
                oUserDataDict.Add("external_id", (long)oUser.USR_ID);
                oUserDataDict.Add("alias", oUser.USR_EMAIL);
                oUserDataDict.Add("phone", (oUser.COUNTRy1!=null?("+"+oUser.COUNTRy1.COU_TEL_PREFIX+" "):"")+oUser.USR_MAIN_TEL);
                
                string[] strTags = ConfigurationManager.AppSettings[ct_ZENDESK_TAGS_TAG].ToString().Split(new char[] {'|'});
                oUserDataDict.Add("tags", strTags.Select(tag => tag.Trim()).ToArray());
                //oUserDataDict.Add("tags", new string[] { "eysamobile", "usuario", "importado backoffice","proceso-I@" });               
                //oUserDataDict.Add("organization_id", ConfigurationManager.AppSettings[ct_ZENDESK_ORGANIZATION_ID_TAG].ToString());
                //oUserDataDict.Add("organization_id", "459937991");
                oUserDataDict.Add("role", "end-user");

                oUserDataDict.Add("user_fields", new Dictionary<string, string> ()
                    { 
                        {"telfono_fijo", (oUser.COUNTRy2!=null?("+"+oUser.COUNTRy2.COU_TEL_PREFIX+" "):"")+(string.IsNullOrEmpty(oUser.USR_SECUND_TEL)?"":oUser.USR_SECUND_TEL)},
                        {"dni_pasaporte", oUser.CUSTOMER.CUS_DOC_ID},
                        {"provincia", oUser.CUSTOMER.CUS_STATE},
                        {"ciudad", oUser.CUSTOMER.CUS_CITY},
                        {"matriculas", strPlates},
                        {"alta", oUser.USR_INSERT_UTC_DATE.ToString("dd/MM/yyyy HH:mm:ss") + " UTC"},
                        {"tipo_de_suscripcion", (oUser.USR_SUSCRIPTION_TYPE.HasValue?((PaymentSuscryptionType)oUser.USR_SUSCRIPTION_TYPE).ToString():"")},
                        {"saldo_actual", (Convert.ToDouble(oUser.USR_BALANCE)/100).ToString()+" "+oUser.CURRENCy.CUR_ISO_CODE},
                        {"version",strVersions},
                        {"sistema_operativo", strSOs},
                        {"modelo_dispositvo", strDeviceModels},
                        {"numero_serie_dispositivo", strSerialNumbers},
                        {"estado", (oUser.USR_ENABLED==1)?"Active":"Deleted"},
                    });

               


                bRes = true;

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetZendeskUserDataDict: ", e);
                bRes = false;
            }

            return bRes;
        }


        public bool UpdateUserReplications(ref List<stUserReplicationResult> oUsersReps, int iMaxRetries)
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

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                    var predicate = PredicateBuilder.False<USERS_REPLICATION>();

                    if ((oUsersReps != null) && (oUsersReps.Count() > 0))
                    {
                        foreach (stUserReplicationResult oUserRep in oUsersReps)
                        {
                            predicate = predicate.Or(a => a.USRREP_ID == oUserRep.m_dRepId);
                        }
                    }


                    var oUserReplications = (from r in dbContext.USERS_REPLICATIONs
                                             select r)
                                        .Where(predicate);


                    System.Collections.ArrayList oArray = new ArrayList();

                    try
                    {

                        foreach (USERS_REPLICATION oRep in oUserReplications)
                        {

                            stUserReplicationResult oReplication = oUsersReps.Where(r => r.m_dRepId == oRep.USRREP_ID).FirstOrDefault();

                            oRep.USRREP_EXT_REPLICATION_ERROR_TEXT = oReplication.m_strReplicationError;
                            oRep.USRREP_EXT_REPLICATION_ID = oReplication.m_strExternalReplicationId;
                            oRep.USRREP_IN_JOB_ORDER = oReplication.m_iInJobOrder;
                            oRep.USRREP_JOB_ID = oReplication.m_strJobId;
                            oRep.USRREP_JOB_STATUS_URL = oReplication.m_strJobURL;
                            oRep.USRREP_STATUS_UTC_DATE = oReplication.m_dtStatusDate;
                            oRep.USRREP_CONFIRM_IN_WS_RETRIES_NUM = oReplication.m_iCurrRetries;
                            oRep.USRREP_QUEUE_LENGTH_BEFORE_CONFIRM_WS = oReplication.m_iQueueBeforeReplication;
                            oRep.USRREP_CONFIRMATION_TIME_IN_WS = oReplication.m_dReplicationTime;

                            if ((oReplication.m_iCurrRetries>=iMaxRetries)&&(oReplication.m_eUserReplicationStatus==UserReplicationStatus.Error))
                            {
                                oReplication.m_eUserReplicationStatus=UserReplicationStatus.Cancelled;
                            }
                            else if (oReplication.m_eUserReplicationStatus == UserReplicationStatus.Completed)
                            {
                                oRep.USRREP_CONFIRM_IN_WS_DATE = oRep.USRREP_STATUS_UTC_DATE;
                            }
                            
                            oRep.USRREP_CONFIRMED_IN_WS = (int)oReplication.m_eUserReplicationStatus;


                        }

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                       
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponRechargeID: ", e);
                        bRes = false;
                    }                  
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserReplication: ", e);
                bRes = false;
            }

            return bRes;


        }



        public bool GetPaymentMeanFees(ref USER oUser, out decimal dFeeVal, out decimal dFeePerc)
        {
            bool bRes = false;
            dFeeVal=0;
            dFeePerc=0;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    dFeeVal =
                       Convert.ToDecimal(oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_FIXED_FEE.HasValue ?
                       oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_FIXED_FEE : 0) +
                       Convert.ToDecimal(oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_FIXED_FEE.HasValue ?
                       oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_FIXED_FEE : 0);
                    dFeePerc =
                       Convert.ToDecimal(oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_PERC_FEE.HasValue ?
                       oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_SUBTYPE.PAST_PERC_FEE : 0) +
                       Convert.ToDecimal(oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_PERC_FEE.HasValue ?
                       oUser.CUSTOMER_PAYMENT_MEAN.PAYMENT_TYPE.PAT_PERC_FEE : 0);

                    bRes = true;
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetPaymentMeanFees: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool GetRechargeCouponCode(out RECHARGE_COUPON oCoupon, string strCode)
        {

            bool bRes = false;
            oCoupon = null;
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

                    var oCoupons = (from r in dbContext.RECHARGE_COUPONs
                                    where r.RCOUP_CODE == strCode || r.RCOUP_KEYCODE == strCode
                                    select r);

                    if (oCoupons.Count() > 0)
                    {
                        oCoupon = oCoupons.First();
                        bRes = true;
                    }
                    else
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponCode: Code not exist: " + strCode);
                        bRes = false;
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponCode: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetRechargeCouponRechargeID(ref USER oUser, decimal sessionId,decimal couponID, out string strRechargeID)
        {

            bool bRes = false;
            strRechargeID = "";

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

                        RECHARGE_COUPONS_USE oNewRechargeCouponUse = new RECHARGE_COUPONS_USE
                        {
                            RCOUPU_USR_ID = oUser.USR_ID,
                            RCOUPU_RCOUP_ID = couponID,
                            RCOUPU_DATE = DateTime.UtcNow,
                            RCOUPU_CODE = GenerateId() + GenerateId() + GenerateId(),
                            RCOUPU_MOSE_ID = sessionId
                        };


                        dbContext.RECHARGE_COUPONS_USEs.InsertOnSubmit(oNewRechargeCouponUse);
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        strRechargeID = oNewRechargeCouponUse.RCOUPU_CODE;

                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponRechargeID: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponRechargeID: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetRechargeCouponFromRechargeID(ref USER oUser, decimal sessionId, string strRechargeID, out RECHARGE_COUPON oCoupon)
        {
            bool bRes = false;
            oCoupon = null;
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

                    var oCoupons = (from r in dbContext.RECHARGE_COUPONS_USEs
                                    join r2 in dbContext.RECHARGE_COUPONs
                                     on r.RCOUPU_RCOUP_ID equals r2.RCOUP_ID
                                    where r.RCOUPU_CODE == strRechargeID &&
                                         r.RCOUPU_MOSE_ID == sessionId
                                    select r2);

                    if (oCoupons.Count() > 0)
                    {
                        oCoupon = oCoupons.First();
                        bRes = true;
                    }
                    else
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponFromRechargeID: RechargeId not found: " + strRechargeID);
                        bRes = false;
                    }
                    transaction.Complete();
                }



            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetRechargeCouponFromRechargeID: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool ChargeServiceOperation(ref USER user,
                                            int iOSType,
                                            bool bSubstractFromBalance,
                                            ServiceChargeType serviceType,
                                            PaymentSuscryptionType suscriptionType,
                                            DateTime dtPaymentDate,
                                            DateTime dtUTCPaymentDate,
                                            int iQuantity,
                                            decimal dCurID,
                                            decimal dBalanceCurID,
                                            double dChangeApplied,
                                            double dChangeFee,
                                            int iCurrencyChargedQuantity,
                                            decimal? dRechargeId,
                                            out decimal dOperationID)
        {
            bool bRes = true;
            dOperationID = -1;
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

                        SERVICE_CHARGE oCharge = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, iCurrencyChargedQuantity, null, out dCustomerInvoiceID);
                            

                            oCharge = new SERVICE_CHARGE
                            {
                                SECH_MOSE_OS = iOSType,
                                SECH_SECHT_ID = (int)serviceType,
                                SECH_DATE = dtPaymentDate,
                                SECH_UTC_DATE = dtUTCPaymentDate,
                                SECH_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate-dtPaymentDate).TotalMinutes),
                                SECH_AMOUNT = iQuantity,
                                SECH_AMOUNT_CUR_ID = dCurID,
                                SECH_BALANCE_CUR_ID = dBalanceCurID,
                                SECH_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                SECH_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                SECH_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                SECH_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                SECH_CUSPMR_ID = dRechargeId,
                                SECH_BALANCE_BEFORE = oUser.USR_BALANCE,
                                SECH_SUSCRIPTION_TYPE = (int)suscriptionType,
                                SECH_CUSINV_ID = dCustomerInvoiceID
                               
                            };

                            if (bSubstractFromBalance)
                            {
                                ModifyUserBalance(ref oUser, -iCurrencyChargedQuantity);

                            }

                            oUser.SERVICE_CHARGEs.Add(oCharge);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oCharge != null)
                            {
                                dOperationID = oCharge.SECH_ID;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeServiceOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeServiceOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeServiceOperation: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool ChargeFinePayment(ref USER user,
                                    int iOSType,
                                    bool bSubstractFromBalance,
                                    PaymentSuscryptionType suscriptionType,                
                                    decimal dInstallationID,
                                    DateTime dtTicketPayment,
                                    DateTime dtUTCPaymentDate,
                                    string strPlate,
                                    string strTicketNumber,
                                    string strTicketData,
                                    int iQuantity,
                                    decimal dCurID,
                                    decimal dBalanceCurID,                            
                                    double dChangeApplied,
                                    double dChangeFee,
                                    int iCurrencyChargedQuantity,
                                    decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, int iTotalAmount,
                                    decimal? dRechargeId,
                                     bool bConfirmedInWS1, bool bConfirmedInWS2, bool bConfirmedInWS3,
                                    decimal? dLatitude, 
                                    decimal? dLongitude,string strAppVersion,
                                    decimal? dGrpId,
                                    string strSector, 
                                    string strEnforcUser,
                                    out decimal dTicketPaymentID,
                                    out DateTime? dtUTCInsertionDate)
        {
            bool bRes = true;
            dTicketPaymentID = -1;
            dtUTCInsertionDate = null;

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
                        TICKET_PAYMENT oTicketPayment=null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dPlateID = null;
                            try
                            {
                                var oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1).First();
                                if (oPlate != null)
                                {
                                    dPlateID = oPlate.USRP_ID;
                                }
                            }
                            catch
                            { }

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtTicketPayment, oUser.CUSTOMER.CUS_ID, dCurID, 0, iTotalAmount, dInstallationID, out dCustomerInvoiceID);
                            

                            oTicketPayment = new TICKET_PAYMENT
                            {
                                TIPA_MOSE_OS = iOSType,
                                TIPA_INS_ID = dInstallationID,
                                TIPA_DATE = dtTicketPayment,
                                TIPA_UTC_DATE = dtUTCPaymentDate,
                                TIPA_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtTicketPayment).TotalMinutes),
                                TIPA_USRP_ID = dPlateID,
                                TIPA_PLATE_STRING = strPlate,
                                TIPA_TICKET_NUMBER = strTicketNumber,
                                TIPA_TICKET_DATA = string.IsNullOrEmpty(strTicketData) ? "" : strTicketData.Substring(0, Math.Min(strTicketData.Length,300)),
                                TIPA_AMOUNT = iQuantity,
                                TIPA_AMOUNT_CUR_ID = dCurID,
                                TIPA_BALANCE_CUR_ID = dBalanceCurID,
                                TIPA_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                TIPA_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                TIPA_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                TIPA_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                TIPA_CUSPMR_ID = dRechargeId  ,
                                TIPA_BALANCE_BEFORE = oUser.USR_BALANCE,
                                TIPA_SUSCRIPTION_TYPE = (int)suscriptionType,
                                TIPA_CONFIRMED_IN_WS = (bConfirmedInWS1 ? 1 : 0),
                                TIPA_CONFIRMED_IN_WS2 = (bConfirmedInWS2 ? 1 : 0),
                                TIPA_CONFIRMED_IN_WS3 = (bConfirmedInWS3 ? 1 : 0),
                                TIPA_LATITUDE = dLatitude,
                                TIPA_LONGITUDE = dLongitude,
                                TIPA_APP_VERSION = strAppVersion,
                                TIPA_GRP_ID = dGrpId,
                                TIPA_PERC_VAT1 = Convert.ToDecimal(dPercVat1),
                                TIPA_PERC_VAT2 = Convert.ToDecimal(dPercVat2),
                                TIPA_PARTIAL_VAT1 = iPartialVat1,
                                TIPA_PERC_FEE = Convert.ToDecimal(dPercFEE),
                                TIPA_PERC_FEE_TOPPED = iPercFEETopped,
                                TIPA_PARTIAL_PERC_FEE = iPartialPercFEE,
                                TIPA_FIXED_FEE = iFixedFEE,
                                TIPA_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                TIPA_TOTAL_AMOUNT = iTotalAmount,
                                TIPA_CUSINV_ID = dCustomerInvoiceID,
                                TIPA_SECTOR = strSector,
                                TIPA_ENFORCUSER = strEnforcUser,
                            };

                            if (bSubstractFromBalance)
                            {
                                ModifyUserBalance(ref oUser, -iCurrencyChargedQuantity, 0);

                            }

                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = dtUTCPaymentDate;

                            oUser.TICKET_PAYMENTs.Add(oTicketPayment);

                            if (!oUser.USR_FIRST_OPERATION_INS_ID.HasValue)
                            {
                                oUser.USR_FIRST_OPERATION_INS_ID = dInstallationID;
                            }
                            oUser.USR_LAST_OPERATION_INS_ID = dInstallationID;

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oTicketPayment!=null)
                            {
                                dTicketPaymentID= oTicketPayment.TIPA_ID;
                                dtUTCInsertionDate = oTicketPayment.TIPA_INSERTION_UTC_DATE;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeFinePayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeFinePayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeFinePayment: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool UpdateThirdPartyIDInFinePayment(ref USER user,
                                                     int iWSNumber,
                                                     decimal dTicketPaymentID,
                                                     string str3rdPartyOpNum)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            TICKET_PAYMENT oTicketPayment = oUser.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dTicketPaymentID).First();

                            switch (iWSNumber)
                            {
                                case 1:
                                    oTicketPayment.TIPA_EXTERNAL_ID = str3rdPartyOpNum;
                                    break;

                                case 2:
                                    oTicketPayment.TIPA_EXTERNAL_ID2 = str3rdPartyOpNum;
                                    break;

                                case 3:
                                     oTicketPayment.TIPA_EXTERNAL_ID3 = str3rdPartyOpNum;
                                    break;

                                default:

                                    break;
                            }


                           
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInFinePayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInFinePayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInFinePayment: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool UpdateThirdPartyConfirmedInFinePayment(ref USER user,
                                                     decimal dTicketPaymentID,
                                                     bool bConfirmed1, bool bConfirmed2, bool bConfirmed3)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            TICKET_PAYMENT oTicketPayment = oUser.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dTicketPaymentID).First();
                            oTicketPayment.TIPA_CONFIRMED_IN_WS = bConfirmed1 ? 1 : 0;
                            oTicketPayment.TIPA_CONFIRMED_IN_WS2 = bConfirmed2 ? 1 : 0;
                            oTicketPayment.TIPA_CONFIRMED_IN_WS3 = bConfirmed3 ? 1 : 0;

                        }
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFinePayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFinePayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFinePayment: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool AddDiscountToFinePayment(ref USER user,
                                    int iOSType,
                                    PaymentSuscryptionType suscriptionType,
                                    DateTime dtPaymentDate,
                                    DateTime dtUTCPaymentDate,
                                    int iQuantity,
                                    decimal dCurID,
                                    decimal dBalanceCurID,
                                    double dChangeApplied,
                                    double dChangeFee,
                                    int iCurrencyChargedQuantity,
                                    decimal dTicketPaymentID,
                                    decimal? dLatitude, decimal? dLongitude, string strAppVersion)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            TICKET_PAYMENT oTicketPayment = oUser.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dTicketPaymentID).First();

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, -iCurrencyChargedQuantity, oTicketPayment.TIPA_INS_ID, out dCustomerInvoiceID);
                            

                            OPERATIONS_DISCOUNT oDiscount = new OPERATIONS_DISCOUNT
                            {
                                OPEDIS_MOSE_OS = iOSType,
                                OPEDIS_DATE = dtPaymentDate,
                                OPEDIS_UTC_DATE = dtUTCPaymentDate,
                                OPEDIS_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtPaymentDate).TotalMinutes),
                                OPEDIS_AMOUNT = iQuantity,
                                OPEDIS_AMOUNT_CUR_ID = dCurID,
                                OPEDIS_BALANCE_CUR_ID = dBalanceCurID,
                                OPEDIS_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                OPEDIS_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                OPEDIS_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                OPEDIS_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                OPEDIS_USR_ID = oUser.USR_ID,
                                OPEDIS_BALANCE_BEFORE = oUser.USR_BALANCE,
                                OPEDIS_SUSCRIPTION_TYPE = (int)suscriptionType,
                                OPEDIS_LATITUDE = dLatitude,
                                OPEDIS_LONGITUDE = dLongitude,
                                OPEDIS_APP_VERSION = strAppVersion,
                                OPEDIS_CUSINV_ID = dCustomerInvoiceID
                            };

                            oTicketPayment.OPERATIONS_DISCOUNT = oDiscount;

                            ModifyUserBalance(ref oUser, iCurrencyChargedQuantity);


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToFinePayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToFinePayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToFinePayment: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool ChargeParkingOperation( ref USER user,
                                            int iOSType,
                                            bool bSubstractFromBalance,
                                            PaymentSuscryptionType suscriptionType,
                                            ChargeOperationsType chargeType,
                                            string strPlate,
                                            decimal dInstallationID,
                                            decimal dGroupID,
                                            decimal dArticleDef,
                                            decimal? dStreetSectionId,
                                            DateTime dtPaymentDate,
                                            DateTime dtInitialDate,
                                            DateTime dtEndDate,
                                            DateTime dtUTCPaymentDate,
                                            DateTime dtUTCInitialDate,
                                            DateTime dtUTCEndDate,
                                            int iTime,
                                            int iQuantity,
                                            int iRealQuantity,
                                            int iTimeBalUsed,
                                            decimal dCurID,
                                            decimal dBalanceCurID,
                                            double dChangeApplied,
                                            double dChangeFee,
                                            int iCurrencyChargedQuantity,
                                            decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, decimal dPercBonus, int iPartialBonusFEE, int iTotalAmount,
                                            string sBonusId, string sBonusMarca, int? iBonusType,string strPlaceString,int iPostpay,
                                            decimal? dRechargeId,
                                            bool bConfirmedInWS1, bool bConfirmedInWS2, bool bConfirmedInWS3,
                                            decimal dMobileSessionId,
                                            decimal? dLatitude, decimal? dLongitude,string strAppVersion,
                                            string sExternalId1, string sExternalId2, string sExternalId3,                                            
                                            out decimal dOperationID,
                                            out DateTime? dtUTCInsertionDate)
        {
            bool bRes = true;
            dOperationID = -1;
            dtUTCInsertionDate = null;

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

                        OPERATION oOperation = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dPlateID = null;
                            try
                            {
                                var oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1).First();
                                if (oPlate != null)
                                {
                                    dPlateID = oPlate.USRP_ID;
                                }
                            }
                            catch
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "ChargeParkingOperation: Plate is not from user or is not enabled: " + strPlate);
                                bRes = false;
                                return bRes;

                            }


                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, iTotalAmount, dInstallationID, out dCustomerInvoiceID);
                            
                            oOperation = new OPERATION
                            {
                                OPE_MOSE_OS = iOSType,
                                OPE_TYPE = (int)chargeType,
                                OPE_USRP_ID = dPlateID,
                                OPE_INS_ID = dInstallationID,
                                OPE_GRP_ID = dGroupID,
                                OPE_TAR_ID = dArticleDef,
                                OPE_DATE = dtPaymentDate,
                                OPE_INIDATE = dtInitialDate,
                                OPE_ENDDATE = dtEndDate,
                                OPE_UTC_DATE = dtUTCPaymentDate,
                                OPE_UTC_INIDATE = dtUTCInitialDate,
                                OPE_UTC_ENDDATE = dtUTCEndDate,
                                OPE_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtPaymentDate).TotalMinutes),
                                OPE_INIDATE_UTC_OFFSET = Convert.ToInt32((dtUTCInitialDate - dtInitialDate).TotalMinutes),
                                OPE_ENDDATE_UTC_OFFSET = Convert.ToInt32((dtUTCEndDate - dtEndDate).TotalMinutes),
                                OPE_AMOUNT = iQuantity,
                                OPE_TIME = iTime,
                                OPE_AMOUNT_CUR_ID = dCurID,
                                OPE_BALANCE_CUR_ID = dBalanceCurID,
                                OPE_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                OPE_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                OPE_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                OPE_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                OPE_CUSPMR_ID = dRechargeId,
                                OPE_BALANCE_BEFORE = oUser.USR_BALANCE,
                                OPE_TIME_BALANCE_USED = iTimeBalUsed,
                                OPE_TIME_BALANCE_BEFORE = oUser.USR_TIME_BALANCE,
                                OPE_SUSCRIPTION_TYPE = (int)suscriptionType,
                                OPE_CONFIRMED_IN_WS1 = (bConfirmedInWS1 ? 1 : 0),
                                OPE_CONFIRMED_IN_WS2 = (bConfirmedInWS2 ? 1 : 0),
                                OPE_CONFIRMED_IN_WS3 = (bConfirmedInWS3 ? 1 : 0),
                                OPE_MOSE_ID = dMobileSessionId,
                                OPE_LATITUDE = dLatitude,
                                OPE_LONGITUDE = dLongitude,
                                OPE_APP_VERSION = strAppVersion,
                                OPE_EXTERNAL_ID1 = sExternalId1,
                                OPE_EXTERNAL_ID2 = sExternalId2,
                                OPE_EXTERNAL_ID3 = sExternalId3,
                                OPE_PERC_VAT1 = Convert.ToDecimal(dPercVat1),
                                OPE_PERC_VAT2 = Convert.ToDecimal(dPercVat2),
                                OPE_PARTIAL_VAT1 = iPartialVat1,
                                OPE_PERC_FEE = Convert.ToDecimal(dPercFEE),
                                OPE_PERC_FEE_TOPPED = iPercFEETopped,
                                OPE_PARTIAL_PERC_FEE = iPartialPercFEE,
                                OPE_FIXED_FEE = iFixedFEE,
                                OPE_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                OPE_PERC_BONUS = dPercBonus,
                                OPE_PARTIAL_BONUS_FEE = iPartialBonusFEE,
                                OPE_TOTAL_AMOUNT = iTotalAmount,
                                OPE_CUSINV_ID = dCustomerInvoiceID,
                                OPE_BONUS_ID = sBonusId,
                                OPE_BONUS_MARCA = sBonusMarca,
                                OPE_BONUS_TYPE = iBonusType,
                                OPE_SPACE_STRING = strPlaceString,
                                OPE_REAL_AMOUNT = iRealQuantity,
                                OPE_POSTPAY = iPostpay,
                                OPE_STRSE_ID = dStreetSectionId
                            };

                            if (bSubstractFromBalance)
                            {
                                ModifyUserBalanceChargeOperation(ref oUser, -iCurrencyChargedQuantity, -iTimeBalUsed);
                            }

                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = dtUTCPaymentDate;

                            oUser.OPERATIONs.Add(oOperation);

                            if (!oUser.USR_FIRST_OPERATION_INS_ID.HasValue)
                            {
                                oUser.USR_FIRST_OPERATION_INS_ID = dInstallationID;
                            }                           
                            oUser.USR_LAST_OPERATION_INS_ID = dInstallationID;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oOperation!=null)
                            {
                                dOperationID = oOperation.OPE_ID;
                                dtUTCInsertionDate = oOperation.OPE_INSERTION_UTC_DATE;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }


        protected bool GetCustomerInvoice(integraMobileDBEntitiesDataContext dbContext, DateTime dtPaymentDate, decimal dCustomerID, 
                                       decimal dCurID, int iRechargeAmount, int iOpsAmount, decimal? dInstallationID, out decimal? dCustomerInvoiceID)
        {
            bool bRes = true;
            dCustomerInvoiceID = null;
                          
            try
            {

                DateTime dtFirstDateToBeInvoiced = dtPaymentDate;
                DateTime dtLastDateToInvoice = dtPaymentDate;

                InvoicePeriodType eInvoiceGenerationPeriod= InvoicePeriodType.Monthly;

                try
                {
                    eInvoiceGenerationPeriod = (InvoicePeriodType)Convert.ToInt32(ConfigurationManager.AppSettings[ct_INVOICE_PERIOD_TAG].ToString());
                }
                catch
                {
                }



                switch (eInvoiceGenerationPeriod)
                {
                    case InvoicePeriodType.Weekly:
                        int iWeekNumber = DateHelpers.GetIso8601WeekOfYear(dtPaymentDate);
                        dtFirstDateToBeInvoiced = DateHelpers.FirstDateOfWeek(dtPaymentDate.Year, iWeekNumber);
                        dtLastDateToInvoice = dtFirstDateToBeInvoiced.AddDays(7);
                        break;

                    case InvoicePeriodType.Monthly:

                        dtFirstDateToBeInvoiced = new DateTime(dtPaymentDate.Year, dtPaymentDate.Month, 1);
                        dtLastDateToInvoice = dtFirstDateToBeInvoiced.AddMonths(1);
                        break;

                    default:
                        break;

                }


                OPERATOR oOperator = null;

                if (dInstallationID.HasValue)
                {
                    INSTALLATION oInstallation = dbContext.INSTALLATIONs.Where(r => r.INS_ID == dInstallationID).FirstOrDefault();
                    if (oInstallation != default(INSTALLATION))
                    {
                        oOperator = oInstallation.OPERATOR;
                    }
                }

                if (oOperator==null)
                {
                    oOperator= GetDefaultOperator(dbContext);
                }

                var oInvoice = (from r in dbContext.CUSTOMER_INVOICEs
                                 where r.CUSINV_OPR_ID == oOperator.OPR_ID &&
                                       r.CUSINV_CUS_ID == dCustomerID &&
                                       r.CUSINV_CUR_ID == dCurID &&
                                       r.CUSINV_DATEINI == dtFirstDateToBeInvoiced &&
                                       r.CUSINV_DATEEND == dtLastDateToInvoice
                                 select r).FirstOrDefault();


                if (oInvoice != default(CUSTOMER_INVOICE))
                {
                    oInvoice.CUSINV_INV_AMOUNT += iRechargeAmount;
                    oInvoice.CUSINV_INV_AMOUNT_OPS += iOpsAmount;
                }
                else
                {

                    if (oOperator.OPR_CURRENT_INVOICE_NUMBER <= oOperator.OPR_END_INVOICE_NUMBER)
                    {

                        oInvoice = new CUSTOMER_INVOICE
                        {                            
                            CUSINV_CUS_ID = dCustomerID,
                            CUSINV_DATEINI = dtFirstDateToBeInvoiced,
                            CUSINV_DATEEND = dtLastDateToInvoice,
                            CUSINV_GENERATION_DATE = DateTime.UtcNow,
                            CUSINV_OPR_INITIAL_INVOICE_NUMBER = oOperator.OPR_INITIAL_INVOICE_NUMBER,
                            CUSINV_OPR_END_INVOICE_NUMBER = oOperator.OPR_END_INVOICE_NUMBER,
                            CUSINV_CUR_ID = dCurID,
                            CUSINV_OPR_ID = oOperator.OPR_ID,
                            CUSINV_INVOICE_VERSION = ctnCurrentVersionOfInvoice,
                            CUSINV_INV_AMOUNT = iRechargeAmount,
                            CUSINV_INV_AMOUNT_OPS = iOpsAmount
                        };

                        dbContext.CUSTOMER_INVOICEs.InsertOnSubmit(oInvoice);                      

                    }
                }
                
                try
                {
                    SecureSubmitChanges(ref dbContext);
                    dCustomerInvoiceID = oInvoice.CUSINV_ID;
                }
                catch (Exception e)
                {
                    m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInvoice: ", e);
                    bRes = false;
                }

               
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInParkingOperation: ", e);
                bRes = false;
            }
                
  
            return bRes;

        }






        public bool UpdateThirdPartyIDInParkingOperation(ref USER user,
                                                    int iWSNumber,
                                                     decimal dOperationID,
                                                     string str3rdPartyOpNum)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            OPERATION oOperation = oUser.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                            switch (iWSNumber)
                            {
                                case 1:
                                    oOperation.OPE_EXTERNAL_ID1 = str3rdPartyOpNum;
                                    break;

                                case 2:
                                    oOperation.OPE_EXTERNAL_ID2 = str3rdPartyOpNum;
                                    break;

                                case 3:
                                    oOperation.OPE_EXTERNAL_ID3 = str3rdPartyOpNum;
                                    break;

                                default:

                                    break;
                            }

                            m_Log.LogMessage(LogLevels.logDEBUG, string.Format("oOperation.OPE_EXTERNAL_ID1={0}", oOperation.OPE_EXTERNAL_ID1));
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool UpdateThirdPartyConfirmedInParkingOperation(ref USER user,
                                                     decimal dOperationID,
                                                     bool bConfirmed1, bool bConfirmed2, bool bConfirmed3)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            OPERATION oOperation = oUser.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                            oOperation.OPE_CONFIRMED_IN_WS1 = bConfirmed1?1:0;
                            oOperation.OPE_CONFIRMED_IN_WS2 = bConfirmed2?1:0;                                                        
                            oOperation.OPE_CONFIRMED_IN_WS3 = bConfirmed3?1:0;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }
            

        public bool AddDiscountToParkingOperation(ref USER user,
                                    int iOSType,
                                    PaymentSuscryptionType suscriptionType,
                                    DateTime dtPaymentDate,
                                    DateTime dtUTCPaymentDate,
                                    int iQuantity,
                                    decimal dCurID,
                                    decimal dBalanceCurID,
                                    double dChangeApplied,
                                    double dChangeFee,
                                    int iCurrencyChargedQuantity,
                                    decimal dOperationID,
                                    decimal? dLatitude, decimal? dLongitude, string strAppVersion)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            OPERATION oOperation = oUser.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, -iCurrencyChargedQuantity,  oOperation.OPE_INS_ID, out dCustomerInvoiceID);

                            OPERATIONS_DISCOUNT oDiscount = new OPERATIONS_DISCOUNT
                            {
                                OPEDIS_MOSE_OS = iOSType,
                                OPEDIS_DATE = dtPaymentDate,
                                OPEDIS_UTC_DATE = dtUTCPaymentDate,
                                OPEDIS_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtPaymentDate).TotalMinutes),
                                OPEDIS_AMOUNT = iQuantity,
                                OPEDIS_AMOUNT_CUR_ID = dCurID,
                                OPEDIS_BALANCE_CUR_ID = dBalanceCurID,
                                OPEDIS_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                OPEDIS_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                OPEDIS_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                OPEDIS_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                OPEDIS_USR_ID = oUser.USR_ID,        
                                OPEDIS_BALANCE_BEFORE = oUser.USR_BALANCE,
                                OPEDIS_SUSCRIPTION_TYPE = (int)suscriptionType,
                                OPEDIS_LATITUDE = dLatitude,
                                OPEDIS_LONGITUDE = dLongitude,
                                OPEDIS_APP_VERSION = strAppVersion,
                                OPEDIS_CUSINV_ID = dCustomerInvoiceID
                            };

                            oOperation.OPERATIONS_DISCOUNT = oDiscount;

                            ModifyUserBalance(ref oUser, iCurrencyChargedQuantity);


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddDiscountToParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool ChargeUnParkingOperation(ref USER user,
                                            int iOSType,
                                            PaymentSuscryptionType suscriptionType,
                                            string strPlate,
                                            decimal dInstallationID,
                                            decimal? dGroupID,
                                            decimal? dArticleDef,
                                            DateTime dtPaymentDate,
                                            DateTime dtInitialDate,
                                            DateTime dtEndDate,
                                            DateTime dtUTCPaymentDate,
                                            DateTime dtUTCInitialDate,
                                            DateTime dtUTCEndDate,
                                            DateTime? dtPrevEnd,
                                            int iTime,
                                            int iQuantity,
                                            decimal dCurID,
                                            decimal dBalanceCurID,
                                            double dChangeApplied,
                                            double dChangeFee,
                                            int iCurrencyChargedQuantity,
                                            decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, decimal dPercBonus, int iPartialBonusFEE,
                                            int iTotalAmount, string sBonusId,
                                            bool bConfirmedInWS1, bool bConfirmedInWS2, bool bConfirmedInWS3,
                                            decimal dMobileSessionId,
                                            decimal? dLatitude, decimal? dLongitude,string strAppVersion,
                                            out decimal dOperationID,
                                            out DateTime? dtUTCInsertionDate)
        {
            bool bRes = true;
            dOperationID = -1;
            dtUTCInsertionDate = null;
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

                        OPERATION oOperation = null;
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {


                            decimal? dPlateID = null;
                            try
                            {
                                var oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1).First();
                                if (oPlate != null)
                                {
                                    dPlateID = oPlate.USRP_ID;
                                }
                            }
                            catch
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "ChargeUnParkingOperation: Plate is not from user or is not enabled: " + strPlate);
                                bRes = false;
                                return bRes;

                            }


                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, -iTotalAmount, dInstallationID, out dCustomerInvoiceID);


                            oOperation = new OPERATION
                            {
                                OPE_MOSE_OS = iOSType,
                                OPE_TYPE = (int)ChargeOperationsType.ParkingRefund,
                                OPE_USRP_ID = dPlateID,
                                OPE_INS_ID = dInstallationID,
                                OPE_GRP_ID = dGroupID,
                                OPE_TAR_ID = dArticleDef,
                                OPE_DATE = dtPaymentDate,
                                OPE_INIDATE = dtInitialDate,
                                OPE_ENDDATE = dtEndDate,
                                OPE_REFUND_PREVIOUS_ENDDATE = dtPrevEnd,
                                OPE_UTC_DATE = dtUTCPaymentDate,
                                OPE_UTC_INIDATE = dtUTCInitialDate,
                                OPE_UTC_ENDDATE = dtUTCEndDate,
                                OPE_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtPaymentDate).TotalMinutes),
                                OPE_INIDATE_UTC_OFFSET = Convert.ToInt32((dtUTCInitialDate - dtInitialDate).TotalMinutes),
                                OPE_ENDDATE_UTC_OFFSET = Convert.ToInt32((dtUTCEndDate - dtEndDate).TotalMinutes),
                                OPE_AMOUNT = ((RefundBalanceType)oUser.USR_REFUND_BALANCE_TYPE) == RefundBalanceType.rbtAmount ? iQuantity: 0,
                                OPE_TIME = iTime,
                                OPE_TIME_BALANCE_USED = ((RefundBalanceType)oUser.USR_REFUND_BALANCE_TYPE)==RefundBalanceType.rbtAmount?0: iTime,
                                OPE_AMOUNT_CUR_ID = dCurID,
                                OPE_BALANCE_CUR_ID = dBalanceCurID,
                                OPE_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                OPE_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                OPE_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                OPE_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                OPE_CUSPMR_ID = null,
                                OPE_BALANCE_BEFORE = oUser.USR_BALANCE,
                                OPE_TIME_BALANCE_BEFORE = oUser.USR_TIME_BALANCE,
                                OPE_SUSCRIPTION_TYPE = (int)suscriptionType,
                                OPE_CONFIRMED_IN_WS1 = (bConfirmedInWS1 ? 1 : 0),
                                OPE_CONFIRMED_IN_WS2 = (bConfirmedInWS2 ? 1 : 0),
                                OPE_CONFIRMED_IN_WS3 = (bConfirmedInWS3 ? 1 : 0),
                                OPE_MOSE_ID = dMobileSessionId,
                                OPE_LATITUDE = dLatitude,
                                OPE_LONGITUDE = dLongitude,
                                OPE_APP_VERSION = strAppVersion,
                                OPE_PERC_VAT1 = Convert.ToDecimal(dPercVat1),
                                OPE_PERC_VAT2 = Convert.ToDecimal(dPercVat2),
                                OPE_PARTIAL_VAT1 = iPartialVat1,
                                OPE_PERC_FEE = Convert.ToDecimal(dPercFEE),
                                OPE_PERC_FEE_TOPPED = iPercFEETopped,
                                OPE_PARTIAL_PERC_FEE = iPartialPercFEE,
                                OPE_FIXED_FEE = iFixedFEE,
                                OPE_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                OPE_TOTAL_AMOUNT = iTotalAmount,
                                OPE_CUSINV_ID = dCustomerInvoiceID,
                                OPE_PERC_BONUS = dPercBonus,
                                OPE_PARTIAL_BONUS_FEE = iPartialBonusFEE,
                                OPE_BONUS_ID = sBonusId,
                                OPE_REAL_AMOUNT = iQuantity
                            };

                            ModifyUserBalance(ref oUser, iCurrencyChargedQuantity, iTime);


                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = dtUTCPaymentDate;

                            if (!oUser.USR_FIRST_OPERATION_INS_ID.HasValue)
                            {
                                oUser.USR_FIRST_OPERATION_INS_ID = dInstallationID;
                            }
                            oUser.USR_LAST_OPERATION_INS_ID = dInstallationID;

                            oUser.OPERATIONs.Add(oOperation);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oOperation != null)
                            {
                                dOperationID = oOperation.OPE_ID;
                                dtUTCInsertionDate = oOperation.OPE_INSERTION_UTC_DATE;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeUnParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeUnParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeUnParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RefundChargeFinePayment(ref USER user,
                                            bool bAddToBalance,
                                            decimal dTicketPaymentID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oTicket = oUser.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dTicketPaymentID).First();

                            if (oTicket != null)
                            {
                                if (bAddToBalance)
                                {
                                    ModifyUserBalance(ref oUser, oTicket.TIPA_FINAL_AMOUNT);

                                }
                                dbContext.TICKET_PAYMENTs.DeleteOnSubmit(oTicket);
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RefundChargeFinePayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RefundChargeFinePayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundChargeFinePayment: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool RefundChargeParkPayment(ref USER user,
                                            bool bAddToBalance,
                                            decimal dOperationID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oParkOp = oUser.OPERATIONs.Where(r => r.OPE_ID==dOperationID).First();

                            if (oParkOp != null)
                            {
                                if(bAddToBalance)
                                {
                                    ModifyUserBalanceChargeOperation(ref oUser, oParkOp.OPE_FINAL_AMOUNT, oParkOp.OPE_TIME_BALANCE_USED??0);

                                }
                                dbContext.OPERATIONs.DeleteOnSubmit(oParkOp);
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RefundChargeParkPayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RefundChargeParkPayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundChargeParkPayment: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool BackUnParkPayment(ref USER user,
                                      decimal dOperationID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oParkOp = oUser.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                            if (oParkOp != null)
                            {
                                ModifyUserBalance(ref oUser, -oParkOp.OPE_FINAL_AMOUNT, -oParkOp.OPE_TIME_BALANCE_USED??0);
                                dbContext.OPERATIONs.DeleteOnSubmit(oParkOp);
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "BackUnParkPayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "BackUnParkPayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "BackUnParkPayment: ", e);
                bRes = false;
            }
            return bRes;

        }





        public bool AddSessionOperationParkInfo(ref USER user,
                                     string strSessionID,
                                     ChargeOperationsType operationType,
                                     DateTime dtinstDateTime,
                                     DateTime dtUTCDateTime,
                                     string strPlate,
                                     decimal dGroupId,
                                     decimal dTariffId,
                                     double dChangeToApply,
                                     decimal? dAuthId,
                                     decimal dPercVat1, decimal dPercVat2,
                                     decimal dPercFEE, int iPercFEETopped,
                                     int iFixedFEE,
                                     decimal dPercBonus, string sBonusId, string sBonusMarca, int? iBonusType)

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID && 
                                                        r.MOSE_STATUS==Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (OPERATIONS_SESSION_INFO opSessionInfo in dbContext.OPERATIONS_SESSION_INFOs.Where(r => r.OSI_PLATE == strPlate
                              && r.OSI_TAR_ID == dTariffId
                              && r.OSI_GRP_ID == dGroupId
                              && r.MOBILE_SESSION.MOSE_INS_ID == oSession.MOSE_INS_ID))
                            {
                                dbContext.OPERATIONS_SESSION_INFOs.DeleteOnSubmit(opSessionInfo);
                            }
                           

                            oSession.OPERATIONS_SESSION_INFOs.Add( new OPERATIONS_SESSION_INFO
                                {
                                    OSI_OPE_TYPE = (int)operationType,
                                    OSI_INS_DATE  = dtinstDateTime,
                                    OSI_UTC_DATE = dtUTCDateTime,
                                    OSI_PLATE = strPlate,
                                    OSI_GRP_ID = dGroupId,
                                    OSI_TAR_ID = dTariffId,
                                    OSI_CHANGE_APPLIED = Convert.ToDecimal(dChangeToApply),
                                    OSI_AUTH_ID = dAuthId,
                                    OSI_PERC_VAT1 = dPercVat1,
                                    OSI_PERC_VAT2 = dPercVat2,
                                    OSI_PERC_FEE = dPercFEE,
                                    OSI_PERC_FEE_TOPPED = iPercFEETopped,
                                    OSI_FIXED_FEE = iFixedFEE,
                                    OSI_PERC_BONUS = dPercBonus,
                                    OSI_BONUS_ID = sBonusId,
                                    OSI_EYSA_MARCA = sBonusMarca,
                                    OSI_EYSA_TICKET_TYPE = iBonusType
                                });                       
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationParkInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationParkInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationParkInfo: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool AddSessionOperationUnParkInfo(ref USER user,
                                     string strSessionID,
                                     ChargeOperationsType operationType,
                                     DateTime dtinstDateTime,
                                     DateTime dtUTCDateTime,
                                     string strPlate,
                                     int iAmount,
                                     int iTime,
                                     decimal? dGroupId,
                                     decimal? dTariffId,
                                     DateTime dtUTCIniDateTime,
                                     DateTime dtUTCEndDateTime,  
                                     double dChangeToApply,
                                     decimal dPercVat1, decimal dPercVat2,
                                     decimal dPercFEE, int iPercFEETopped,
                                     int iFixedFEE,decimal dPercBonus, string sBonusId)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (OPERATIONS_SESSION_INFO opSessionInfo in dbContext.OPERATIONS_SESSION_INFOs.Where(r => r.OSI_PLATE == strPlate
                                && r.OSI_TAR_ID == dTariffId 
                                && r.OSI_GRP_ID == dGroupId
                                && r.MOBILE_SESSION.MOSE_INS_ID==oSession.MOSE_INS_ID))
                            {
                                dbContext.OPERATIONS_SESSION_INFOs.DeleteOnSubmit(opSessionInfo);
                            }


                            oSession.OPERATIONS_SESSION_INFOs.Add(new OPERATIONS_SESSION_INFO
                            {
                                OSI_OPE_TYPE = (int)operationType,
                                OSI_INS_DATE = dtinstDateTime,
                                OSI_UTC_DATE = dtUTCDateTime,
                                OSI_PLATE = strPlate,
                                OSI_AMOUNT_TO_REFUND = iAmount,                               
                                OSI_TIME_FOR_AMOUNT = iTime,
                                OSI_UTC_INIDATE = dtUTCIniDateTime,
                                OSI_UTC_ENDDATE = dtUTCEndDateTime,
                                OSI_CHANGE_APPLIED = Convert.ToDecimal(dChangeToApply),
                                OSI_GRP_ID = dGroupId,
                                OSI_TAR_ID = dTariffId,
                                OSI_PERC_VAT1 = dPercVat1,
                                OSI_PERC_VAT2 = dPercVat2,
                                OSI_PERC_FEE = dPercFEE,
                                OSI_PERC_FEE_TOPPED = iPercFEETopped,
                                OSI_FIXED_FEE = iFixedFEE,
                                OSI_PERC_BONUS = dPercBonus,
                                OSI_BONUS_ID = sBonusId
                            });
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationUnParkInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationUnParkInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationParkInfo: ", e);
                bRes = false;
            }
            return bRes;

        }



        public bool CheckSessionOperationParkInfo(ref USER user,
                                     string strSessionID,
                                     string strPlate,
                                     decimal dGroupId,
                                     decimal dTariffId,
                                     out DateTime dtinstDateTime,
                                     out ChargeOperationsType operationType,
                                     out double dChangeToApply,
                                     out decimal? dAuthId,
                                     out decimal dPercVat1, out decimal dPercVat2,
                                     out decimal dPercFEE, out int iPercFEETopped,
                                     out int iFixedFEE,
                                     out decimal dPercBonus, out string sBonusId, out string sBonusMarca, out int? iBonusType)
        {
            bool bRes = false;
            
            dtinstDateTime = DateTime.UtcNow;
            operationType = ChargeOperationsType.ParkingOperation;
            dChangeToApply = 1.0;
            dAuthId = null;
            dPercVat1 = 0;
            dPercVat2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;
            dPercBonus = 0;
            sBonusId = null;
            sBonusMarca = null;
            iBonusType = null;

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


                        int iConfirmationTimeout = ctnDefaultOperationConfirmationTimeout;
                        try
                        {
                            iConfirmationTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ConfirmationTimeoutInSeconds"]);
                        }
                        catch { }


                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oSessions = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID && 
                                                        r.MOSE_STATUS==Convert.ToInt32(MobileSessionStatus.Open));


                            if (oSessions.Count() > 0)
                            {
                                OPERATIONS_SESSION_INFO opSessionInfoTemp = oSessions.First().OPERATIONS_SESSION_INFOs.OrderByDescending(r => r.OSI_UTC_DATE).First();

                                OPERATIONS_SESSION_INFO opSessionInfo = oSessions.First().OPERATIONS_SESSION_INFOs
                                            .Where(r=> r.OSI_UTC_DATE==opSessionInfoTemp.OSI_UTC_DATE && r.OSI_TAR_ID == dTariffId && r.OSI_GRP_ID == dGroupId).FirstOrDefault();


                                if (opSessionInfo != null)
                                {
                                    bRes = ((opSessionInfo.OSI_PLATE == strPlate) &&
                                            ((DateTime.UtcNow - opSessionInfo.OSI_UTC_DATE).TotalSeconds <= iConfirmationTimeout) &&
                                            ((opSessionInfo.OSI_OPE_TYPE == (int)ChargeOperationsType.ParkingOperation) ||
                                                (opSessionInfo.OSI_OPE_TYPE == (int)ChargeOperationsType.ExtensionOperation)));

                                    if (bRes)
                                    {
                                        dtinstDateTime = opSessionInfo.OSI_INS_DATE;
                                        operationType = (ChargeOperationsType)opSessionInfo.OSI_OPE_TYPE;
                                        dChangeToApply = Convert.ToDouble(opSessionInfo.OSI_CHANGE_APPLIED);
                                        dAuthId = opSessionInfo.OSI_AUTH_ID;
                                        dPercVat1 = opSessionInfo.OSI_PERC_VAT1 ?? 0;
                                        dPercVat2 = opSessionInfo.OSI_PERC_VAT2 ?? 0;
                                        dPercFEE = opSessionInfo.OSI_PERC_FEE ?? 0;
                                        iPercFEETopped = Convert.ToInt32(opSessionInfo.OSI_PERC_FEE_TOPPED ?? 0);
                                        iFixedFEE = Convert.ToInt32(opSessionInfo.OSI_FIXED_FEE ?? 0);
                                        dPercBonus = opSessionInfo.OSI_PERC_BONUS ?? 0;
                                        sBonusId = opSessionInfo.OSI_BONUS_ID;
                                        sBonusMarca = opSessionInfo.OSI_EYSA_MARCA;
                                        iBonusType = opSessionInfo.OSI_EYSA_TICKET_TYPE;
                                    }
                                }
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationParkInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationParkInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationParkInfo: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool CheckSessionOperationUnParkInfo(ref USER user,
                                     string strSessionID,
                                     string strPlate,
                                     int iAmount,
                                     out DateTime dtinstDateTime,
                                     out int iTime,
                                     out decimal? dGroupId,
                                     out decimal? dTariffId,
                                     out DateTime dtUTCIniDateTime,
                                     out DateTime dtUTCEndDateTime,
                                     out ChargeOperationsType operationType,
                                     out double dChangeToApply,
                                     out decimal dPercVat1, out decimal dPercVat2,
                                     out decimal dPercFEE, out int iPercFEETopped,
                                     out int iFixedFEE,
                                     out decimal dPercBonus, out string sBonusId)
        {
            bool bRes = false;

            dtinstDateTime = DateTime.UtcNow;
            iTime = 0;
            dtUTCIniDateTime = DateTime.UtcNow;
            dtUTCEndDateTime = DateTime.UtcNow;
            dGroupId = null;
            dTariffId = null;

            operationType = ChargeOperationsType.ParkingOperation;
            dChangeToApply = 1.0;

            dPercVat1 = 0;
            dPercVat2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;
            dPercBonus = 0;
            sBonusId = null;


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

                        int iConfirmationTimeout = ctnDefaultOperationConfirmationTimeout;
                        try
                        {
                            iConfirmationTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ConfirmationTimeoutInSeconds"]);
                        }
                        catch { }


                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oSessions = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open));


                            if (oSessions.Count() > 0)
                            {
                                
                                OPERATIONS_SESSION_INFO opSessionInfo = oSessions.First().OPERATIONS_SESSION_INFOs.Where(r=> r.OSI_PLATE==strPlate).OrderByDescending(r => r.OSI_UTC_DATE).First();

                                bRes = ((opSessionInfo.OSI_PLATE == strPlate) && (opSessionInfo.OSI_AMOUNT_TO_REFUND == iAmount) &&
                                        ((DateTime.UtcNow - opSessionInfo.OSI_UTC_DATE).TotalSeconds <= iConfirmationTimeout) &&
                                        (opSessionInfo.OSI_OPE_TYPE == (int)ChargeOperationsType.ParkingRefund));

                                if (bRes)
                                {
                                    dtinstDateTime = opSessionInfo.OSI_INS_DATE;
                                    operationType = (ChargeOperationsType)opSessionInfo.OSI_OPE_TYPE;
                                    dChangeToApply = Convert.ToDouble(opSessionInfo.OSI_CHANGE_APPLIED);
                                    iTime = opSessionInfo.OSI_TIME_FOR_AMOUNT.Value;
                                    dtUTCIniDateTime = opSessionInfo.OSI_UTC_INIDATE.Value;
                                    dtUTCEndDateTime = opSessionInfo.OSI_UTC_ENDDATE.Value;
                                    dGroupId = opSessionInfo.OSI_GRP_ID;
                                    dTariffId = opSessionInfo.OSI_TAR_ID;
                                    dPercVat1 = opSessionInfo.OSI_PERC_VAT1 ?? 0;
                                    dPercVat2 = opSessionInfo.OSI_PERC_VAT2 ?? 0;
                                    dPercFEE = opSessionInfo.OSI_PERC_FEE ?? 0;
                                    iPercFEETopped = Convert.ToInt32(opSessionInfo.OSI_PERC_FEE_TOPPED ?? 0);
                                    iFixedFEE = Convert.ToInt32(opSessionInfo.OSI_FIXED_FEE ?? 0);
                                    dPercBonus = opSessionInfo.OSI_PERC_BONUS ?? 0;
                                    sBonusId = opSessionInfo.OSI_BONUS_ID;
                                }
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationUnParkInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationUnParkInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationUnParkInfo: ", e);
                bRes = false;
            }
            return bRes;

        }



        public bool DeleteSessionOperationInfo(ref USER user,
                                     string strSessionID,
                                     string strPlate)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (OPERATIONS_SESSION_INFO opSessionInfo in oSession.OPERATIONS_SESSION_INFOs.Where(r=> r.OSI_PLATE==strPlate))
                            {
                                dbContext.OPERATIONS_SESSION_INFOs.DeleteOnSubmit(opSessionInfo);
                            }

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationInfo: ", e);
                bRes = false;
            }
            return bRes;

        }
        


        public bool AddSessionTicketPaymentInfo(ref USER user,
                                     string strSessionID,
                                     DateTime dtinstDateTime,
                                     DateTime dtUTCDateTime,
                                     string strFineNumber,
                                     string strPlate,
                                     string strArticleType,
                                     string strArticleDescription,
                                     int iQuantity,
                                     double dChangeToApply, 
                                     decimal? dAuthId,
                                     decimal? dGrpId,
                                     decimal dPercVat1, decimal dPercVat2,
                                     decimal dPercFEE, int iPercFEETopped,
                                     int iFixedFEE,
                                     string strSector, 
                                     string strEnforcUser)
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
                        decimal userId = user.USR_ID;

                        //var oUser_L = (from r in dbContext.USERs where r.USR_ENABLED == 1 select r).ToList();

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (TICKET_PAYMENTS_SESSION_INFO tpSessionInfo in dbContext.TICKET_PAYMENTS_SESSION_INFOs.Where(r => r.TPSI_TICKET_NUMBER == strFineNumber && r.MOBILE_SESSION.MOSE_INS_ID == oSession.MOSE_INS_ID))
                            {
                                dbContext.TICKET_PAYMENTS_SESSION_INFOs.DeleteOnSubmit(tpSessionInfo);
                            }


                            oSession.TICKET_PAYMENTS_SESSION_INFOs.Add(new TICKET_PAYMENTS_SESSION_INFO
                            {
                                TPSI_INS_DATE = dtinstDateTime,
                                TPSI_UTC_DATE = dtUTCDateTime,
                                TPSI_TICKET_NUMBER = strFineNumber,
                                TPSI_PLATE = strPlate,
                                TPSI_ARTICLE_TYPE = strArticleType,
                                TPSI_ARTICLE_DESCRIPTION = strArticleDescription,
                                TPSI_AMOUNT = iQuantity,
                                TPSI_CHANGE_APPLIED = Convert.ToDecimal(dChangeToApply),
                                TPSI_AUTH_ID = dAuthId,
                                TPSI_GRP_ID = dGrpId,
                                TPSI_PERC_VAT1 = dPercVat1,
                                TPSI_PERC_VAT2 = dPercVat2,
                                TPSI_PERC_FEE = dPercFEE,
                                TPSI_PERC_FEE_TOPPED = iPercFEETopped,
                                TPSI_FIXED_FEE = iFixedFEE,
                                TPSI_SECTOR = strSector,
                                TPSI_ENFORCUSER = strEnforcUser,
                            });
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddSessionTicketPaymentInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddSessionTicketPaymentInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddSessionTicketPaymentInfo: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool CheckSessionTicketPaymentInfo(ref USER user,
                                     string strSessionID,
                                     ref string strFineNumber,
                                     int iQuantity,
                                     out string strPlate,
                                     out string strArticleType,
                                     out string strArticleDescription,
                                     out DateTime dtinstDateTime,
                                     out double dChangeToApply,
                                     out decimal? dAuthId,
                                     out decimal? dGrpId,
                                     out decimal dPercVat1, out decimal dPercVat2,
                                     out decimal dPercFEE, out int iPercFEETopped,
                                     out int iFixedFEE,
                                     out string strSector, 
                                     out string strEnforcUser)
        {
            bool bRes = false;

            dtinstDateTime = DateTime.UtcNow;
            dChangeToApply = 1.0;
            strPlate = "";
            strArticleType = "";
            strArticleDescription = "";
            dAuthId = null;
            dGrpId = null;

            dPercVat1 = 0;
            dPercVat2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;
            strSector = "";
            strEnforcUser = "";

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

                        int iConfirmationTimeout = ctnDefaultOperationConfirmationTimeout;
                        try
                        {
                            iConfirmationTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ConfirmationTimeoutInSeconds"]);
                        }
                        catch { }


                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oSessions = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open));


                            if (oSessions.Count() > 0)
                            {

                                TICKET_PAYMENTS_SESSION_INFO tpSessionInfo = oSessions.First().TICKET_PAYMENTS_SESSION_INFOs.OrderByDescending(r => r.TPSI_UTC_DATE).First();

                                bRes = ((tpSessionInfo.TPSI_TICKET_NUMBER.Contains(strFineNumber)) &&
                                       ((DateTime.UtcNow - tpSessionInfo.TPSI_UTC_DATE).TotalSeconds <= iConfirmationTimeout) &&
                                        (tpSessionInfo.TPSI_AMOUNT == iQuantity));

                                if (bRes)
                                {
                                    strFineNumber = tpSessionInfo.TPSI_TICKET_NUMBER;
                                    dtinstDateTime = tpSessionInfo.TPSI_INS_DATE;
                                    dChangeToApply = Convert.ToDouble(tpSessionInfo.TPSI_CHANGE_APPLIED);
                                    strPlate = tpSessionInfo.TPSI_PLATE;
                                    strArticleType = tpSessionInfo.TPSI_ARTICLE_TYPE;
                                    strArticleDescription = tpSessionInfo.TPSI_ARTICLE_DESCRIPTION;
                                    dAuthId = tpSessionInfo.TPSI_AUTH_ID;
                                    dGrpId = tpSessionInfo.TPSI_GRP_ID;
                                    dPercVat1 = tpSessionInfo.TPSI_PERC_VAT1 ?? 0;
                                    dPercVat2 = tpSessionInfo.TPSI_PERC_VAT2 ?? 0;
                                    dPercFEE = tpSessionInfo.TPSI_PERC_FEE ?? 0;
                                    iPercFEETopped = Convert.ToInt32(tpSessionInfo.TPSI_PERC_FEE_TOPPED ?? 0);
                                    iFixedFEE = Convert.ToInt32(tpSessionInfo.TPSI_FIXED_FEE ?? 0);
                                    strSector = tpSessionInfo.TPSI_SECTOR;
                                    strEnforcUser = tpSessionInfo.TPSI_ENFORCUSER;
                                }
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "CheckSessionTicketPaymentInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CheckSessionTicketPaymentInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CheckSessionTicketPaymentInfo: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool DeleteSessionTicketPaymentInfo(ref USER user,
                                                     string strSessionID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (TICKET_PAYMENTS_SESSION_INFO tpSessionInfo in oSession.TICKET_PAYMENTS_SESSION_INFOs)
                            {
                                dbContext.TICKET_PAYMENTS_SESSION_INFOs.DeleteOnSubmit(tpSessionInfo);
                            }


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionTicketPaymentInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionTicketPaymentInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionTicketPaymentInfo: ", e);
                bRes = false;
            }
            return bRes;

        }



        public bool AddWPPushIDNotification(ref USER user, 
                                              string strToastText1,
                                              string strToastText2,
                                              string strToastParam,
                                              string strTileTitle,
                                              int iTileCount,
                                              string strBackgroundImage)       
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            USERS_NOTIFICATION oNotif = new USERS_NOTIFICATION
                             {

                                 UNO_STATUS = Convert.ToInt32(UserNotificationStatus.Inserted),
                                 UNO_WP_TEXT1 = strToastText1,
                                 UNO_WP_TEXT2 = strToastText2,
                                 UNO_WP_PARAM = strToastParam,
                                 UNO_WP_BACKGROUND_IMAGE = strBackgroundImage,
                                 UNO_WP_TILE_TITLE = strTileTitle,
                                 UNO_WP_RAW_DATA = "",
                                 UNO_ANDROID_RAW_DATA = "",
                                 UNO_iOS_RAW_DATA = ""
                             };

                            if (iTileCount <= 0)
                            {
                                oNotif.UNO_WP_COUNT = null;
                            }
                            else
                            {
                                oNotif.UNO_WP_COUNT = iTileCount;
                            }

                            oUser.USERS_NOTIFICATIONs.Add(oNotif);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddWPPushIDNotification: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddWPPushIDNotification: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddWPPushIDNotification: ", e);
                bRes = false;
            }
            return bRes;

        }



        public bool AddAndroidPushIDNotification(ref USER user,
                                                 string strAndroidRawData)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            USERS_NOTIFICATION oNotif = new USERS_NOTIFICATION
                            {

                                UNO_STATUS = Convert.ToInt32(UserNotificationStatus.Inserted),
                                UNO_WP_TEXT1 = "",
                                UNO_WP_TEXT2 = "",
                                UNO_WP_PARAM = "",
                                UNO_WP_COUNT = null,
                                UNO_WP_BACKGROUND_IMAGE = "",
                                UNO_WP_TILE_TITLE = "",
                                UNO_WP_RAW_DATA = "",
                                UNO_ANDROID_RAW_DATA = strAndroidRawData,
                                UNO_iOS_RAW_DATA = ""
                            };

                            oUser.USERS_NOTIFICATIONs.Add(oNotif);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddAndroidPushIDNotification: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddAndroidPushIDNotification: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddAndroidPushIDNotification: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool AddPushIDNotification(ref USER user,
                                            string strToastText1,
                                            string strToastText2,
                                            string strToastParam,
                                            string strTileTitle,
                                            int iTileCount,
                                            string strBackgroundImage,
                                            string strAndroidRawData,
                                            string strIOSRawData,
                                            ref decimal oNotifID,
                                            decimal? dUserPushId = null)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();
                        USERS_NOTIFICATION oNotif = null;

                        if (oUser != null)
                        {

                            oNotif = new USERS_NOTIFICATION
                            {
                                 
                                UNO_STATUS = Convert.ToInt32(UserNotificationStatus.Inserted),
                                UNO_WP_TEXT1 = strToastText1,
                                UNO_WP_TEXT2 = strToastText2,
                                UNO_WP_PARAM = strToastParam,
                                UNO_WP_BACKGROUND_IMAGE = strBackgroundImage,
                                UNO_WP_TILE_TITLE = strTileTitle,
                                UNO_WP_RAW_DATA = "",
                                UNO_ANDROID_RAW_DATA = strAndroidRawData,
                                UNO_iOS_RAW_DATA = strIOSRawData,
                                UNO_UPID_ID = dUserPushId
                            };

                            if (iTileCount <= 0)
                            {
                                oNotif.UNO_WP_COUNT = null;
                            }
                            else
                            {
                                oNotif.UNO_WP_COUNT = iTileCount;
                            }

                            oUser.USERS_NOTIFICATIONs.Add(oNotif);

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            if (oNotif != null)
                                oNotifID = oNotif.UNO_ID;
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddPushIDNotification: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddPushIDNotification: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddPushIDNotification: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetUsersWithPlate(string strPlate, out IEnumerable<USER> oUsersList)
        {
            bool bRes = true;
            oUsersList = new List<USER>();

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

                    oUsersList = (from r in dbContext.USER_PLATEs
                                  where r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1 && r.USER.USR_ENABLED == 1
                                  select r.USER).ToList();
                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUsersWithPlate: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool GetLastOperationWithPlate(string strPlate, decimal dInstallationId, out OPERATION oOperation)
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
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    oOperation = (from r in dbContext.OPERATIONs
                                  where r.USER_PLATE.USRP_PLATE == strPlate && r.OPE_INS_ID == dInstallationId
                                  orderby r.OPE_UTC_DATE descending
                                  select r).First();
                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLastOperationWithPlate: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool QueryUnConfirmedParkingOperations(decimal dCurrId,int? iWSSignatureType, int iMaxRows, out IEnumerable<OPERATION> oOperationList)
        {
            bool bRes = true;
            oOperationList = null;

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


                    if (iWSSignatureType.HasValue)
                    {

                        oOperationList = (from a in dbContext.OPERATIONs
                                          where a.OPE_ID > dCurrId &&
                                                (((a.OPE_CONFIRMED_IN_WS1 == 0) && (a.INSTALLATION.INS_PARK_CONFIRM_WS_SIGNATURE_TYPE == iWSSignatureType)) ||
                                                 ((a.OPE_CONFIRMED_IN_WS2 == 0) && (a.INSTALLATION.INS_PARK_CONFIRM_WS2_SIGNATURE_TYPE == iWSSignatureType)) ||
                                                 ((a.OPE_CONFIRMED_IN_WS3 == 0) && (a.INSTALLATION.INS_PARK_CONFIRM_WS3_SIGNATURE_TYPE == iWSSignatureType)))
                                          orderby a.OPE_ID ascending
                                          select a).Take(iMaxRows).AsEnumerable();
                    }
                    else
                    {
                        oOperationList = (from a in dbContext.OPERATIONs
                                          where a.OPE_ID > dCurrId &&
                                                ((a.OPE_CONFIRMED_IN_WS1 == 0) || (a.OPE_CONFIRMED_IN_WS2 == 0)||(a.OPE_CONFIRMED_IN_WS3 == 0))
                                          orderby a.OPE_ID ascending
                                          select a).Take(iMaxRows).AsEnumerable();

                    }

                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "QueryUnConfirmedParkingOperations: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool ExistUnConfirmedParkingOperationFor(decimal dInsId, string strPlate)
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


                  

                        var oOperationsNotConfirmed = (from a in dbContext.OPERATIONs
                                                       where (((a.OPE_CONFIRMED_IN_WS1 == 0) || (a.OPE_CONFIRMED_IN_WS2 == 0) ||(a.OPE_CONFIRMED_IN_WS3 == 0)) && 
                                                             (a.USER_PLATE.USRP_PLATE==strPlate)&&
                                                             (a.OPE_INS_ID == dInsId))
                                                       orderby a.OPE_ID ascending
                                                       select a).AsQueryable();
                        bRes = (oOperationsNotConfirmed.Count() > 0);

                     transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ExistUnConfirmedParkingOperationFor: ", e);
                bRes = false;
            }

            return bRes;

        }

        
        public bool ConfirmUnConfirmedParkingOperations(decimal dCurrId,int? iWSSignatureType)
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
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();



                    var oOperations = (from a in dbContext.OPERATIONs
                                       where a.OPE_ID <= dCurrId &&
                                             (((a.OPE_CONFIRMED_IN_WS1 == 0) && 
                                                    ((a.INSTALLATION.INS_PARK_CONFIRM_WS_SIGNATURE_TYPE == iWSSignatureType) ||(!iWSSignatureType.HasValue))) ||
                                              ((a.OPE_CONFIRMED_IN_WS2 == 0) && 
                                                    ((a.INSTALLATION.INS_PARK_CONFIRM_WS2_SIGNATURE_TYPE == iWSSignatureType)||(!iWSSignatureType.HasValue))) ||
                                              ((a.OPE_CONFIRMED_IN_WS3 == 0) && 
                                                    ((a.INSTALLATION.INS_PARK_CONFIRM_WS3_SIGNATURE_TYPE == iWSSignatureType)||(!iWSSignatureType.HasValue))))
                                       select a);

                    if (oOperations.Count() > 0)
                    {

                        foreach (OPERATION oOperation in oOperations)
                        {

                            if ((oOperation.OPE_CONFIRMED_IN_WS1 == 0) && 
                                ((oOperation.INSTALLATION.INS_PARK_CONFIRM_WS_SIGNATURE_TYPE == iWSSignatureType)||(!iWSSignatureType.HasValue)))
                            {
                                    oOperation.OPE_CONFIRMED_IN_WS1 = 1;
                                    oOperation.OPE_CONFIRM_IN_WS1_DATE = DateTime.UtcNow;
                            }                            
                            else if ((oOperation.OPE_CONFIRMED_IN_WS2 == 0) && 
                                ((oOperation.INSTALLATION.INS_PARK_CONFIRM_WS2_SIGNATURE_TYPE == iWSSignatureType)||(!iWSSignatureType.HasValue)))
                            {
                                oOperation.OPE_CONFIRMED_IN_WS2 = 1;
                                oOperation.OPE_CONFIRM_IN_WS2_DATE = DateTime.UtcNow;
                            }                            
                            else if ((oOperation.OPE_CONFIRMED_IN_WS3 == 0) && 
                                ((oOperation.INSTALLATION.INS_PARK_CONFIRM_WS3_SIGNATURE_TYPE == iWSSignatureType)||(!iWSSignatureType.HasValue)))
                            {
                                oOperation.OPE_CONFIRMED_IN_WS3 = 1;
                                oOperation.OPE_CONFIRM_IN_WS3_DATE = DateTime.UtcNow;
                            }
                        }

                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ConfirmUnConfirmedParkingOperations: ", e);
                            bRes = false;
                        }
                    }
                    else
                    {
                        transaction.Complete();
                    }
                }
            }               
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "QueryUnConfirmedParkingOperations: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool QueryUnConfirmedFinePayments(decimal dCurrId, int? iWSSignatureType, int iMaxRows, out IEnumerable<TICKET_PAYMENT> oOperationList)
        {
            bool bRes = true;
            oOperationList = null;

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


                    if (iWSSignatureType.HasValue)
                    {

                        oOperationList = (from a in dbContext.TICKET_PAYMENTs
                                          where a.TIPA_ID > dCurrId &&
                                                ((a.TIPA_CONFIRMED_IN_WS == 0) && (a.INSTALLATION.INS_FINE_WS_SIGNATURE_TYPE == iWSSignatureType))
                                          orderby a.TIPA_ID ascending
                                          select a).Take(iMaxRows).AsEnumerable();
                    }
                    else
                    {
                        oOperationList = (from a in dbContext.TICKET_PAYMENTs
                                          where a.TIPA_ID > dCurrId && a.TIPA_CONFIRMED_IN_WS == 0
                                          orderby a.TIPA_ID ascending
                                          select a).Take(iMaxRows).AsEnumerable();

                    }

                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "QueryUnConfirmedFinePayments: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool ConfirmUnConfirmedFinePayments(decimal dCurrId, int? iWSSignatureType)
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
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();



                    var oTicketPayments = (from a in dbContext.TICKET_PAYMENTs
                                       where a.TIPA_ID <= dCurrId &&
                                             ((a.TIPA_CONFIRMED_IN_WS == 0) &&
                                                    ((a.INSTALLATION.INS_FINE_WS_SIGNATURE_TYPE == iWSSignatureType) || (!iWSSignatureType.HasValue)))
                                       select a);

                    if (oTicketPayments.Count() > 0)
                    {

                        foreach (TICKET_PAYMENT oTicketPayment in oTicketPayments)
                        {

                            oTicketPayment.TIPA_CONFIRMED_IN_WS = 1;
                            oTicketPayment.TIPA_CONFIRM_IN_WS_DATE = DateTime.UtcNow;
                        }

                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ConfirmUnConfirmedFinePayments: ", e);
                            bRes = false;
                        }
                    }
                    else
                    {
                        transaction.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "QueryUnConfirmedFinePayments: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool GetWaitingCommitRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iConfirmWaitTime,
                                            int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oRecharge = null;

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


                    var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                                      where (r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Commit) &&
                                            ((r.CUSPMR_TYPE == (int)PaymentMeanRechargeType.Payment)||(r.CUSPMR_TYPE == (int)PaymentMeanRechargeType.Paypal)) &&
                                            (((r.CUSPMR_RETRIES_NUM == 0) && (DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iConfirmWaitTime)))) ||
                                             ((r.CUSPMR_RETRIES_NUM > 0)&&(DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))))
                                      orderby r.CUSPMR_STATUS_DATE
                                      select r).AsQueryable();

                    if (oRecharges.Count() > 0)
                    {
                        oRecharge = oRecharges.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingCommitRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }

        
        public bool CommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,  
                                        string strUserReference,
                                        string strAuthResult,
                                        string strGatewayDate,
                                        string strCommitTransactionId,
                                        int iTransactionFee,
                                        string strTransactionFeeCurrencyIsocode,
                                        string strTransactionURL,
                                        string strRefundTransactionURL)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Committed,null);


                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthResult,
                                                            strGatewayDate,
                                                            strCommitTransactionId,
                                                            iTransactionFee,
                                                            strTransactionFeeCurrencyIsocode,
                                                            strTransactionURL,
                                                            strRefundTransactionURL);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CommitTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool CommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Committed, null);
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CommitTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool RetriesForCommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                                string strUserReference,
                                                string strAuthResult,
                                                string strGatewayDate,
                                                string strCommitTransactionId)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;
                int iCurrRetries=oRecharge.CUSPMR_RETRIES_NUM+1;

                if (iCurrRetries > iMaxRetries)
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Failed_To_Commit, iCurrRetries);
                }
                else
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Waiting_Commit, iCurrRetries);

                }

                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthResult,
                                                            strGatewayDate,
                                                            strCommitTransactionId,
                                                            0,null,null,null);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForCommitTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool GetWaitingCancellationRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,int iConfirmWaitTime,
                                            int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oRecharge = null;

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


                    var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                                      where (r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Cancellation) &&
                                            (((r.CUSPMR_RETRIES_NUM == 0) && (DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iConfirmWaitTime)))) ||
                                             ((r.CUSPMR_RETRIES_NUM > 0) && (DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))))
                                      orderby r.CUSPMR_STATUS_DATE
                                      select r).AsQueryable();

                    if (oRecharges.Count() > 0)
                    {
                        oRecharge = oRecharges.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingCancellationRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool CancelTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,
                                        string strUserReference,
                                        string strAuthResult,
                                        string strGatewayDate,
                                        string strCancellationTransactionId)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Cancelled, null);


                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthResult,
                                                            strGatewayDate,
                                                            strCancellationTransactionId,
                                                            0, null, null, null);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool CancelTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Cancelled, null);

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }



        public bool RetriesForCancellationTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                                string strUserReference,
                                                string strAuthResult,
                                                string strGatewayDate,
                                                string strCancellationTransactionId)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;
                int iCurrRetries = oRecharge.CUSPMR_RETRIES_NUM + 1;

                if (iCurrRetries > iMaxRetries)
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Failed_To_Cancel, iCurrRetries);
                }
                else
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Waiting_Cancellation, iCurrRetries);

                }

                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthResult,
                                                            strGatewayDate,
                                                            strCancellationTransactionId,
                                                            0, null, null, null);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForCancellationTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetWaitingRefundRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,int iConfirmWaitTime,
                                            int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oRecharge = null;

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


                    var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                                      where (r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Refund) &&
                                            (((r.CUSPMR_RETRIES_NUM == 0) && (DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iConfirmWaitTime)))) ||
                                             ((r.CUSPMR_RETRIES_NUM > 0) && (DateTime.UtcNow >= (r.CUSPMR_STATUS_DATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))))
                                      orderby r.CUSPMR_STATUS_DATE
                                      select r).AsQueryable();

                    if (oRecharges.Count() > 0)
                    {
                        oRecharge = oRecharges.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingRefundRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool RefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,
                                        string strUserReference,
                                        string strAuthCode,
                                        string strAuthResult,
                                        string strAuthResultDesc,
                                        string strGatewayDate,
                                        string strRefundTransactionId)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Refunded, null);


                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthCode,
                                                            strAuthResult,
                                                            strAuthResultDesc,
                                                            strGatewayDate,
                                                            strRefundTransactionId);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool RefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;

                bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                            PaymentMeanRechargeStatus.Refunded, null);



            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool RetriesForRefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                                string strUserReference,
                                                string strAuthResult,
                                                string strAuthResultDesc,
                                                string strGatewayDate,
                                                string strRefundTransactionId)
        {
            bool bRes = true;

            try
            {
                USER oUser = oRecharge.USER;
                int iCurrRetries = oRecharge.CUSPMR_RETRIES_NUM + 1;

                if (iCurrRetries > iMaxRetries)
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Failed_To_Refund, iCurrRetries);
                }
                else
                {
                    bRes = SetUserRechargeStatus(ref oUser, oRecharge.CUSPMR_ID,
                                        PaymentMeanRechargeStatus.Waiting_Refund, iCurrRetries);

                }

                if (bRes)
                {

                    bRes = SetRechargeSecondaryTransactionInfo(oRecharge.CUSPMR_ID,
                                                            strUserReference,
                                                            strAuthResult,
                                                            strGatewayDate,
                                                            strRefundTransactionId,
                                                            0, null, null, null);
                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForRefundTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }




        /*
        public bool GetConfirmedRechargesInfo(out CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge,int iConfirmWaitTime,
                                    int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oRecharge = null;

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


                    var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGES_INFOs
                                      where (r.CUSPMRI_STATUS == (int)PaymentMeanRechargeInfoStatus.Confirmed) &&
                                            ((((r.CUSPMRI_RETRIES_NUM == 0) || (!r.CUSPMRI_RETRIES_NUM.HasValue)) && 
                                                (DateTime.UtcNow >= (r.CUSPMRI_STATUS_UTCDATE.AddSeconds(iConfirmWaitTime)))) ||
                                             ((r.CUSPMRI_RETRIES_NUM > 0) && (DateTime.UtcNow >= (r.CUSPMRI_STATUS_UTCDATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))))
                                      orderby r.CUSPMRI_STATUS_UTCDATE
                                      select r).AsQueryable();

                    if (oRecharges.Count() > 0)
                    {
                        oRecharge = oRecharges.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetConfirmedRechargesInfo: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool ReConfirmTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge,
                                        string strConfirmResultCode,
                                        string strConfirmResultCodeDesc)
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
                    //try
                    //{
                            integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();

                
                        
                        decimal dRechargeId = oRecharge.CUSPMRI_ID;

                        
                        var oRecharges = dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGES_INFOs.
                                Where(r => r.CUSPMRI_ID == dRechargeId);

                        if (oRecharges.Count() == 1)
                        {

                            oRecharges.First().CUSPMRI_STATUS = (int)PaymentMeanRechargeInfoStatus.ReConfirmed;
                            oRecharges.First().CUSPMRI_STATUS_UTCDATE = DateTime.UtcNow;
                            oRecharges.First().CUSPMRI_STATUS_DATE = DateTime.Now;
                            oRecharges.First().CUSPMRI_ACK_RESULTCODE = strConfirmResultCode;
                            oRecharges.First().CUSPMRI_ACK_RESULTCODE_DESC = strConfirmResultCodeDesc;


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
                            m_Log.LogMessage(LogLevels.logERROR, "ReConfirmTransaction: ", e);
                            bRes = false;
                        }
                  
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ReConfirmTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool RetriesForReConfirmTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge, int iMaxRetries,
                                                string strConfirmResultCode,
                                                string strConfirmResultCodeDesc)
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
                    //try
                    //{
                            integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                            decimal dRechargeId = oRecharge.CUSPMRI_ID;


                            var oRecharges = dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGES_INFOs.
                                    Where(r => r.CUSPMRI_ID == dRechargeId);

                            if (oRecharges.Count() == 1)
                            {
                                if (oRecharges.First().CUSPMRI_RETRIES_NUM.HasValue)
                                    oRecharges.First().CUSPMRI_RETRIES_NUM++;
                                else
                                    oRecharges.First().CUSPMRI_RETRIES_NUM = 1;

                                if (oRecharges.First().CUSPMRI_RETRIES_NUM > iMaxRetries)
                                {
                                    oRecharges.First().CUSPMRI_STATUS = (int)PaymentMeanRechargeInfoStatus.FailedReconfirmation;
                                }

                                oRecharges.First().CUSPMRI_STATUS_UTCDATE = DateTime.UtcNow;
                                oRecharges.First().CUSPMRI_STATUS_DATE = DateTime.Now;
                                oRecharges.First().CUSPMRI_ACK_RESULTCODE = strConfirmResultCode;
                                oRecharges.First().CUSPMRI_ACK_RESULTCODE_DESC = strConfirmResultCodeDesc;

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
                            m_Log.LogMessage(LogLevels.logERROR, "RetriesForReConfirmTransaction: ", e);
                            bRes = false;
                        }
                   
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForReConfirmTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }

        */


    

        public bool GetCancelableRecharges(out PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                    int iNumMinutestoCancelStartedTransaction,
                                    int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oPendingTransactionOperation = null;

            try
            {

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                new TransactionOptions()
                                                                {
                                                                    IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                    Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                }))
                {
                    //try
                    //{
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();



                    var oPendingOperations = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                      where (((r.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Committed)||(r.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Commit)) &&
                                            (r.PTROP_OP_TYPE == (int)PendingTransactionOperationOpType.Charge) &&
                                            (DateTime.UtcNow >= (r.PTROP_STATUS_DATE.AddMinutes(iNumMinutestoCancelStartedTransaction))))
                                      orderby r.PTROP_STATUS_DATE
                                      select r).AsQueryable();


                    foreach (PENDING_TRANSACTION_OPERATION oPendingOperation in oPendingOperations)
                    {
                        oPendingOperation.PTROP_TRANS_STATUS = oPendingOperation.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Commit? (int)PaymentMeanRechargeStatus.Waiting_Cancellation: (int)PaymentMeanRechargeStatus.Waiting_Refund;
                        oPendingOperation.PTROP_STATUS_DATE = DateTime.UtcNow;
                        oPendingOperation.PTROP_RETRIES_NUM = 0;
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
                        m_Log.LogMessage(LogLevels.logERROR, "GetCancelableRecharges: ", e);
                        bRes = false;
                    }
                   
                }

                if (bRes)
                {

                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                                 new TransactionOptions()
                                                                                                 {
                                                                                                     IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                     Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                                 }))
                    {
                        integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                        var oPendingOperations = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                          where ((r.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Cancellation)||(r.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Refund)) &&
                                           (r.PTROP_OP_TYPE == (int)PendingTransactionOperationOpType.Charge) &&
                                          ((r.PTROP_RETRIES_NUM == 0) ||
                                                 DateTime.UtcNow >= (r.PTROP_STATUS_DATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))
                                          orderby r.PTROP_STATUS_DATE
                                          select r).AsQueryable();

                        if (oPendingOperations.Count() > 0)
                        {
                            oPendingTransactionOperation = oPendingOperations.First();
                        }
                        else
                        {
                            dbContext.Close();
                        }

                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCancelRechargesInfo: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool CancelTransaction(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                          string strUserReference,
                                          string strAuthCode,
                                          string strAuthResult,
                                          string strAuthResultDesc,
                                          string strGatewayDate,
                                          string strRefundTransactionId)
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
                    //try
                    //{
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();



                    decimal dPendingTransactionID = oPendingTransactionOperation.PTROP_ID;


                    var oPendingTransaction = dbContext.PENDING_TRANSACTION_OPERATIONs.
                            Where(r => r.PTROP_ID == dPendingTransactionID);

                    if (oPendingTransaction.Count() == 1)
                    {
                        if (strAuthResult=="cancelled")
                        {
                            oPendingTransaction.First().PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Cancelled;
                        }
                        else
                        {
                            oPendingTransaction.First().PTROP_TRANS_STATUS = oPendingTransaction.First().PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Cancellation ? (int)PaymentMeanRechargeStatus.Cancelled : (int)PaymentMeanRechargeStatus.Refunded; ;
                        }

                        oPendingTransaction.First().PTROP_STATUS_DATE = DateTime.UtcNow;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_RESULT = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_OP_REFERENCE = strUserReference;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_CODE = strAuthResult;
                        oPendingTransaction.First().PTROP_SECOND_TRANSACTION_ID = strRefundTransactionId;
                        oPendingTransaction.First().PTROP_SECOND_GATEWAY_DATE = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_CF_TRANSACTION_ID = strAuthResultDesc;

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
                        m_Log.LogMessage(LogLevels.logERROR, "CancelTransaction: ", e);
                        bRes = false;
                    }
                  
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RetriesForCancelTransaction(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation, int iMaxRetries,
                                                string strAuthResult,
                                                string strAuthResultDesc)
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
                    //try
                    //{
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                    decimal dPendingTransactionID = oPendingTransactionOperation.PTROP_ID;


                    var oPendingTransaction = dbContext.PENDING_TRANSACTION_OPERATIONs.
                            Where(r => r.PTROP_ID == dPendingTransactionID);

                    if (oPendingTransaction.Count() == 1)
                    {
                        oPendingTransaction.First().PTROP_RETRIES_NUM++;
                        if (oPendingTransaction.First().PTROP_RETRIES_NUM > iMaxRetries)
                        {
                            oPendingTransaction.First().PTROP_TRANS_STATUS = oPendingTransaction.First().PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Cancellation ? (int)PaymentMeanRechargeStatus.Failed_To_Cancel : (int)PaymentMeanRechargeStatus.Failed_To_Refund; ;
                        }
                        oPendingTransaction.First().PTROP_STATUS_DATE = DateTime.UtcNow;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_RESULT = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_CODE = strAuthResult;

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
                        m_Log.LogMessage(LogLevels.logERROR, "RetriesForCancelTransaction: ", e);
                        bRes = false;
                    }
                   
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForCancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetTokenDeletions(out PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                   int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries)
        {
            bool bRes = true;
            oPendingTransactionOperation = null;

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


                    var oPendingOperations = (from r in dbContext.PENDING_TRANSACTION_OPERATIONs
                                                where ((r.PTROP_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Waiting_Commit) &&
                                                    (r.PTROP_OP_TYPE == (int)PendingTransactionOperationOpType.TokenDeletion) &&
                                                    ((r.PTROP_RETRIES_NUM == 0) ||
                                                        DateTime.UtcNow >= (r.PTROP_STATUS_DATE.AddSeconds(iNumSecondsToWaitInCaseOfRetry))))
                                                orderby r.PTROP_STATUS_DATE
                                                select r).AsQueryable();

                    if (oPendingOperations.Count() > 0)
                    {
                        oPendingTransactionOperation = oPendingOperations.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                    
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTokenDeletions: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool TokenDeletion(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                          string strUserReference,
                                          string strAuthCode,
                                          string strAuthResult,
                                          string strAuthResultDesc,
                                          string strGatewayDate,
                                          string strRefundTransactionId)
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
                    //try
                    //{
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();



                    decimal dPendingTransactionID = oPendingTransactionOperation.PTROP_ID;


                    var oPendingTransaction = dbContext.PENDING_TRANSACTION_OPERATIONs.
                            Where(r => r.PTROP_ID == dPendingTransactionID);

                    if (oPendingTransaction.Count() == 1)
                    {

                        oPendingTransaction.First().PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Committed;
                        oPendingTransaction.First().PTROP_STATUS_DATE = DateTime.UtcNow;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_RESULT = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_OP_REFERENCE = strUserReference;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_CODE = strAuthResult;
                        oPendingTransaction.First().PTROP_SECOND_TRANSACTION_ID = strRefundTransactionId;
                        oPendingTransaction.First().PTROP_SECOND_GATEWAY_DATE = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_CF_TRANSACTION_ID = strAuthResultDesc;

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
                        m_Log.LogMessage(LogLevels.logERROR, "TokenDeletion: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool RetriesForTokenDeletion(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation, int iMaxRetries,
                                                string strAuthResult,
                                                string strAuthResultDesc)
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
                    //try
                    //{
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                    decimal dPendingTransactionID = oPendingTransactionOperation.PTROP_ID;


                    var oPendingTransaction = dbContext.PENDING_TRANSACTION_OPERATIONs.
                            Where(r => r.PTROP_ID == dPendingTransactionID);

                    if (oPendingTransaction.Count() == 1)
                    {
                        oPendingTransaction.First().PTROP_RETRIES_NUM++;
                        if (oPendingTransaction.First().PTROP_RETRIES_NUM > iMaxRetries)
                        {
                            oPendingTransaction.First().PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Failed_To_Commit;
                        }
                        oPendingTransaction.First().PTROP_STATUS_DATE = DateTime.UtcNow;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_RESULT = strAuthResultDesc;
                        oPendingTransaction.First().PTROP_SECOND_AUTH_CODE = strAuthResult;

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
                        m_Log.LogMessage(LogLevels.logERROR, "RetriesForTokenDeletion: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RetriesForCancelTransaction: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetAutomaticPaymentMeanPendingAutomaticRecharge(out CUSTOMER_PAYMENT_MEAN oPaymentMean, int iNumSecondsToWaitInCaseOfRetry)
        {
            bool bRes = true;
            oPaymentMean = null;

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


                    var oPaymentMeans = (from r in dbContext.CUSTOMER_PAYMENT_MEANs
                                         where r.CUSPM_ENABLED == 1 && r.CUSPM_VALID == 1 &&
                                               r.CUSTOMER.CUS_ENABLED == 1 &&
                                                r.CUSPM_AUTOMATIC_RECHARGE == 1 &&
                                                r.CUSPM_AMOUNT_TO_RECHARGE > 0 &&
                                                r.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS > 0 &&
                                                r.CUSTOMER.USER.USR_BALANCE < r.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS &&
                                                r.CUSTOMER.USER.USR_CUSPM_ID == r.CUSPM_ID &&
                                                r.CUSTOMER.CUS_ENABLED == 1 &&
                                                r.CUSTOMER.USER.USR_ENABLED == 1 &&
                                                (PaymentSuscryptionType)r.CUSTOMER.USER.USR_SUSCRIPTION_TYPE == PaymentSuscryptionType.pstPrepay &&
                                               ((r.CUSPM_AUTOMATIC_FAILED_RETRIES == 0) ||
                                                DateTime.UtcNow >= (r.CUSPM_LAST_TIME_USERD.Value.AddSeconds(iNumSecondsToWaitInCaseOfRetry)))
                                         orderby r.CUSPM_LAST_TIME_USERD
                                         select r);

                    if (oPaymentMeans.Count() > 0)
                    {
                        oPaymentMean = oPaymentMeans.First();
                    }
                    else
                    {
                        dbContext.Close();
                    }

                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetAutomaticPaymentMeanPendingAutomaticRecharge: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool InvalidatePaymentMeans(int iDaysAfterExpiredPaymentToInvalidate,
                                           int iMaxRetriesForAutomaticRecharge)
        {
            bool bRes = true;
            integraMobileDBEntitiesDataContext dbContext = null;
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


                        dbContext = new integraMobileDBEntitiesDataContext();


                        var oExpiredPaymentMeans = (from r in dbContext.CUSTOMER_PAYMENT_MEANs
                                                    where (r.CUSPM_ENABLED == 1 && r.CUSPM_VALID == 1 && r.CUSTOMER.CUS_ENABLED == 1) &&
                                                          ((r.CUSPM_TOKEN_CARD_EXPIRATION_DATE.HasValue &&
                                                            DateTime.UtcNow > r.CUSPM_TOKEN_CARD_EXPIRATION_DATE.Value.AddDays(iDaysAfterExpiredPaymentToInvalidate)) ||
                                                            (r.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE.HasValue &&
                                                            DateTime.UtcNow > r.CUSPM_TOKEN_PAYPAL_PREAPPROVAL_END_DATE.Value.AddDays(iDaysAfterExpiredPaymentToInvalidate)))
                                                    select r).AsQueryable();



                        foreach (CUSTOMER_PAYMENT_MEAN oPaymentMean in oExpiredPaymentMeans)
                        {
                            oPaymentMean.CUSPM_VALID = 0;
                            oPaymentMean.CUSTOMER.USER.USR_PAYMETH = (int)PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;
                            if (oPaymentMean.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {

                                if (((PaymentMeanCreditCardProviderType)oPaymentMean.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                    PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE)))
                                {

                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oPaymentMean.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oPaymentMean.CUSTOMER.USER.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }
                            }                          


                            m_Log.LogMessage(LogLevels.logINFO, string.Format("InvalidatePaymentMeans: Invalidating Expired Payment Mean of user: {0}", oPaymentMean.CUSTOMER.USER.USR_USERNAME));
                        }


                        var oMaxRetriesAutomaticPaymentMeans = (from r in dbContext.CUSTOMER_PAYMENT_MEANs
                                                                where r.CUSPM_ENABLED == 1 && r.CUSPM_VALID == 1 && r.CUSTOMER.CUS_ENABLED == 1 &&
                                                                       r.CUSPM_AUTOMATIC_RECHARGE == 1 &&
                                                                       r.CUSPM_AMOUNT_TO_RECHARGE > 0 &&
                                                                       r.CUSPM_RECHARGE_WHEN_AMOUNT_IS_LESS > 0 &&
                                                                       r.CUSPM_AUTOMATIC_FAILED_RETRIES > iMaxRetriesForAutomaticRecharge
                                                                orderby r.CUSPM_LAST_TIME_USERD
                                                                select r).AsQueryable();

                        foreach (CUSTOMER_PAYMENT_MEAN oPaymentMean in oMaxRetriesAutomaticPaymentMeans)
                        {
                            oPaymentMean.CUSPM_VALID = 0;
                            oPaymentMean.CUSTOMER.USER.USR_PAYMETH = (int)PaymentMeanTypeStatus.pmsWithoutValidPaymentMean;
                            if (oPaymentMean.CUSPM_PAT_ID == (int)PaymentMeanType.pmtDebitCreditCard)
                            {

                                if (((PaymentMeanCreditCardProviderType)oPaymentMean.CUSPM_CREDIT_CARD_PAYMENT_PROVIDER ==
                                    PaymentMeanCreditCardProviderType.pmccpIECISA) && (!string.IsNullOrEmpty(oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE)))
                                {

                                    dbContext.PENDING_TRANSACTION_OPERATIONs.InsertOnSubmit(new PENDING_TRANSACTION_OPERATION()
                                    {
                                        PTROP_OP_TYPE = (int)PendingTransactionOperationOpType.TokenDeletion,
                                        PTROP_CPTGC_ID = oPaymentMean.CUSPM_CPTGC_ID.Value,
                                        PTROP_EMAIL = oPaymentMean.CUSTOMER.USER.USR_EMAIL,
                                        PTROP_UTC_DATE = DateTime.UtcNow,
                                        PTROP_DATE = DateTime.Now,
                                        PTROP_TRANS_STATUS = (int)PaymentMeanRechargeStatus.Waiting_Commit,
                                        PTROP_STATUS_DATE = DateTime.UtcNow,
                                        PTROP_TOKEN = oPaymentMean.CUSPM_TOKEN_CARD_REFERENCE,
                                    });
                                }
                            }
                            m_Log.LogMessage(LogLevels.logINFO, string.Format("InvalidatePaymentMeans: Invalidating For Max Num of Failed Recharges({1}),  Automatic Payment Mean of user: {0}", oPaymentMean.CUSTOMER.USER.USR_USERNAME, iMaxRetriesForAutomaticRecharge));
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InvalidatePaymentMeans: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InvalidatePaymentMeans: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InvalidatePaymentMeans: ", e);
                bRes = false;
            }
            finally
            {
                if (dbContext != null)
                {
                    dbContext.Close();
                }
            }

            return bRes;

        }


        public bool AddSecurityOperation(ref USER user, USERS_SECURITY_OPERATION oSecOperation)
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

                        int iNumCharactersActivationSMS = Convert.ToInt32(ConfigurationManager.AppSettings["NumCharactersActivationSMS"]);
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        oSecOperation.USOP_ACTIVATION_CODE = GenerateRandom(iNumCharactersActivationSMS);
                        oSecOperation.USOP_URL_PARAMETER = GenerateId() + GenerateId() + GenerateId();
                        oSecOperation.USOP_ACTIVATION_RETRIES = 0;
                        dbContext.USERS_SECURITY_OPERATIONs.InsertOnSubmit(oSecOperation);

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddSecurityOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddSecurityOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddSecurityOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool UpdateSecurityOperationRetries(ref USERS_SECURITY_OPERATION secOperation)
        {
            bool bRes = true;
            USERS_SECURITY_OPERATION oSecOperation = null;
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

                        decimal oSecOPId = secOperation.USOP_ID;
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                             where r.USOP_ID == oSecOPId
                                             select r);

                        if (oSecOperations.Count() > 0)
                        {
                            oSecOperation = oSecOperations.First();

                            if (oSecOperation != null)
                            {
                                oSecOperation.USOP_ACTIVATION_RETRIES++;
                                oSecOperation.USOP_LAST_SENT_DATE = DateTime.UtcNow;

                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            secOperation=oSecOperation;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateSecurityOperationRetries: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateSecurityOperationRetries: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateSecurityOperationRetries: ", e);
                bRes = false;
            }

            return bRes;

        }


        public USERS_SECURITY_OPERATION GetUserSecurityOperation(string urlParameter)
        {
            USERS_SECURITY_OPERATION oSecOperation = null;
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

                    var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                          where r.USOP_URL_PARAMETER == urlParameter
                                          orderby r.USOP_LAST_SENT_DATE descending
                                          select r);


                    if (oSecOperations.Count() > 0)
                    {
                        oSecOperation = oSecOperations.First();
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserSecurityOperation: ", e);
                oSecOperation = null;
            }

            return oSecOperation;

        }


        public USERS_SECURITY_OPERATION GetUserSecurityOperation(decimal dSecOpId)
        {
            USERS_SECURITY_OPERATION oSecOperation = null;
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

                    var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                          where r.USOP_ID == dSecOpId                                          
                                          select r);


                    if (oSecOperations.Count() > 0)
                    {
                        oSecOperation = oSecOperations.First();
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserSecurityOperation: ", e);
                oSecOperation = null;
            }

            return oSecOperation;

        }




        public bool ModifyUserEmailOrTelephone(USERS_SECURITY_OPERATION SecOperation, bool bUsernameEqualsEmail)
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

                        decimal userId = SecOperation.USER.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oUser.USR_MAIN_TEL = SecOperation.USOP_NEW_MAIN_TEL;
                            oUser.USR_MAIN_TEL_COUNTRY = SecOperation.USOP_NEW_MAIN_TEL_COUNTRY.Value;
                            oUser.USR_EMAIL = SecOperation.USOP_NEW_EMAIL;
                            if (bUsernameEqualsEmail)
                            {
                                oUser.USR_USERNAME = SecOperation.USOP_NEW_EMAIL; ;
                            }


                            var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                                  where r.USOP_ID == SecOperation.USOP_ID
                                                  select r);

                            if (oSecOperations.Count() > 0)
                            {
                                var oSecOperation = oSecOperations.First();

                                if (oSecOperation != null)
                                {
                                    oSecOperation.USOP_STATUS = (int)SecurityOperationStatus.Confirmed;
                                }
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
                            m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                bRes = false;
            }

            return bRes;


        }

        public bool ModifyUserEmailOrTelephone(ref USER user, decimal dCouId, string sTelephone, string sEmail, bool bUsernameEqualsEmail)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oUser.USR_MAIN_TEL = sTelephone;
                            oUser.USR_MAIN_TEL_COUNTRY = dCouId;
                            oUser.USR_EMAIL = sEmail;
                            if (bUsernameEqualsEmail)
                            {
                                oUser.USR_USERNAME = sEmail;
                            }

                            bRes = true;

                        }
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserEmailOrTelephone: ", e);
                bRes = false;
            }

            return bRes;


        }

        public bool ModifyUserBillingInfo(ref USER user, string sName, string sStreet, int iStreetNum, string sZipCode, string sCity, string sVat)
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

                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oUser.CUSTOMER.CUS_NAME = sName;                            
                            oUser.CUSTOMER.CUS_STREET = sStreet;
                            oUser.CUSTOMER.CUS_STREE_NUMBER = iStreetNum;
                            oUser.CUSTOMER.CUS_ZIPCODE = sZipCode;
                            oUser.CUSTOMER.CUS_CITY = sCity;
                            oUser.CUSTOMER.CUS_DOC_ID = sVat;

                            bRes = true;

                        }
                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBillingInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBillingInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBillingInfo: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool ConfirmSecurityOperation(USERS_SECURITY_OPERATION SecOperation)
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

                        var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                                where r.USOP_ID == SecOperation.USOP_ID
                                                select r);

                        if (oSecOperations.Count() > 0)
                        {
                            var oSecOperation = oSecOperations.First();

                            if (oSecOperation != null)
                            {
                                oSecOperation.USOP_STATUS = (int)SecurityOperationStatus.Confirmed;
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
                            m_Log.LogMessage(LogLevels.logERROR, "ConfirmSecurityOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ConfirmSecurityOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConfirmSecurityOperation: ", e);
                bRes = false;
            }

            return bRes;


        }


        public bool ActivateUser(USERS_SECURITY_OPERATION SecOperation)
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

                        var oSecOperations = (from r in dbContext.USERS_SECURITY_OPERATIONs
                                              where r.USOP_ID == SecOperation.USOP_ID
                                              select r);

                        if (oSecOperations.Count() > 0)
                        {
                            var oSecOperation = oSecOperations.First();

                            if (oSecOperation != null)
                            {
                                oSecOperation.USOP_STATUS = (int)SecurityOperationStatus.Confirmed;
                                oSecOperation.USER.USR_ACTIVATED = 1;
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
                            m_Log.LogMessage(LogLevels.logERROR, "ActivateUser: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ActivateUser: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ActivateUser: ", e);
                bRes = false;
            }

            return bRes;


        }


        
        public bool GetOperationData(ref USER user,
                                     decimal dOperationID,
                                     out OPERATION oParkOp)
        {
            bool bRes = false;
            oParkOp = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oParkOp = oUser.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                            if (oParkOp != null)
                            {
                                bRes = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetOperationData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationData: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool GetRechargeData(ref USER user,
                                     decimal dRechargeID,
                                     out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge)
        {
            bool bRes = false;
            oRecharge = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oRecharge = oUser.CUSTOMER.CUSTOMER_PAYMENT_MEANS_RECHARGEs.Where(r => r.CUSPMR_ID == dRechargeID).First();

                            if (oRecharge != null)
                            {
                                bRes = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetRechargeData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetRechargeData: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool GetTicketPaymentData(ref USER user,
                                     decimal dTicketPaymentID,
                                     out TICKET_PAYMENT oTicketPayment)
        {
            bool bRes = false;
            oTicketPayment = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oTicketPayment = oUser.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dTicketPaymentID).First();

                            if (oTicketPayment != null)
                            {
                                bRes = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetTicketPaymentData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTicketPaymentData: ", e);
                bRes = false;
            }
            return bRes;

        }



        public bool GetFirstNotInvoicedRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge)
        {
            bool bRes = false;
            oRecharge = null;

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

                        var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                                            where r.CUSPMR_CUSINV_ID == null &&
                                                (r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Committed ||
                                                 r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Failed_To_Commit) &&
                                                r.CUSTOMER.CUS_ENABLED == 1
                                            orderby r.CUSPMR_DATE ascending
                                            select r).AsQueryable();

                        if (oRecharges.Count()>0)
                        {
                            oRecharge = oRecharges.First();
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotInvoicedRecharge: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFirstNotInvoicedRecharge: ", e);
                bRes = false;
            }
            return bRes;

        }


        public bool GenerateInvoicesForRecharges(DateTime dt, out int iNumOperations)
        {
            bool bRes = true;
            iNumOperations = 0;

            try
            {
                bool bExit = false;
                while (!bExit)
                {

                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                               new TransactionOptions()
                                                                               {
                                                                                   IsolationLevel = IsolationLevel.ReadUncommitted
                                                                               }))
                    {
                        try
                        {
                            integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                            var oRecharges = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                                              where r.CUSPMR_CUSINV_ID == null &&
                                                    (r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Committed ||
                                                     r.CUSPMR_TRANS_STATUS == (int)PaymentMeanRechargeStatus.Failed_To_Commit || 
                                                     r.CUSPMR_RCOUP_ID != null) &&
                                                    r.CUSPMR_DATE >= ctnFirstDateOfInvoiceVersion_2 &&
                                                    r.CUSPMR_DATE < dt &&
                                                    r.CUSTOMER.CUS_ENABLED == 1
                                              orderby r.CUSPMR_DATE ascending
                                              select r).Take(ctnNumRegistriesToCommitInInvoicingProcess).AsQueryable();

                            if (oRecharges.Count() > 0)
                            {
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForRecharges: Init Process"));


                                foreach (CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge in oRecharges)
                                {

                                    decimal? dCustomerInvoiceID = null;
                                    GetCustomerInvoice(dbContext, DateTime.UtcNow - new TimeSpan(0, oRecharge.USER.USR_UTC_OFFSET, 0), 
                                                       oRecharge.CUSPMR_CUS_ID.Value, oRecharge.CUSPMR_CUR_ID, Convert.ToInt32(oRecharge.CUSPMR_TOTAL_AMOUNT_CHARGED) ,0,
                                                       null, out dCustomerInvoiceID);

                                    if (dCustomerInvoiceID != null)
                                    {
                                        oRecharge.CUSPMR_CUSINV_ID = dCustomerInvoiceID;
                                        try
                                        {
                                            SecureSubmitChanges(ref dbContext);
                                            iNumOperations++;
                                            
                                        }
                                        catch (Exception e)
                                        {
                                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                                            bRes = false;
                                            return bRes;
                                        }


                                    }

                                }

                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForRecharges: End Process"));


                            }
                            else
                                bExit = true;

                            if (!bExit)
                            {
                                
                                // Submit the change to the database.
                                try
                                {

                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForRecharges: Commmitting"));
                                    transaction.Complete();
                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForRecharges: Committed"));

                                }
                                catch (Exception e)
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                                    bRes = false;
                                }
                                
                            }
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                            bRes = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GenerateInvoicesForTicketPayments(DateTime dt, out int iNumOperations)
        {
            bool bRes = true;
            iNumOperations = 0;

            try
            {
                bool bExit = false;
                while (!bExit)
                {

                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                               new TransactionOptions()
                                                                               {
                                                                                   IsolationLevel = IsolationLevel.ReadUncommitted
                                                                               }))
                    {
                        try
                        {
                            integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                            var oTickets = (from r in dbContext.TICKET_PAYMENTs
                                              where r.TIPA_CUSINV_ID == null &&
                                                    r.TIPA_DATE >= ctnFirstDateOfInvoiceVersion_2 &&
                                                    r.TIPA_DATE < dt &&
                                                    r.USER.CUSTOMER.CUS_ENABLED == 1
                                              orderby r.TIPA_DATE ascending
                                              select r).Take(ctnNumRegistriesToCommitInInvoicingProcess).AsQueryable();

                            if (oTickets.Count() > 0)
                            {
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForTicketPayments: Init Process"));


                                foreach (TICKET_PAYMENT oTicket in oTickets)
                                {

                                    decimal? dCustomerInvoiceID = null;
                                    GetCustomerInvoice(dbContext, oTicket.TIPA_DATE,
                                                       oTicket.USER.USR_CUS_ID, oTicket.TIPA_AMOUNT_CUR_ID,0, oTicket.TIPA_TOTAL_AMOUNT.Value,
                                                       null, out dCustomerInvoiceID);

                                    if (dCustomerInvoiceID != null)
                                    {
                                        oTicket.TIPA_CUSINV_ID = dCustomerInvoiceID;
                                        try
                                        {
                                            SecureSubmitChanges(ref dbContext);
                                            iNumOperations++;

                                        }
                                        catch (Exception e)
                                        {
                                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForTicketPayments: ", e);
                                            bRes = false;
                                            return bRes;
                                        }


                                    }
                                 
                                }

                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForTicketPayments: End Process"));


                            }
                            else
                                bExit = true;

                            if (!bExit)
                            {
                                // Submit the change to the database.
                                try
                                {

                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForTicketPayments: Commmitting"));
                                    transaction.Complete();
                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForTicketPayments: Committed"));

                                }
                                catch (Exception e)
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForTicketPayments: ", e);
                                    bRes = false;
                                }                                
                            }
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForTicketPayments: ", e);
                            bRes = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GenerateInvoicesForOperations(DateTime dt, out int iNumOperations)
        {
            bool bRes = true;
            iNumOperations = 0;

            try
            {
                bool bExit = false;
                while (!bExit)
                {

                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                               new TransactionOptions()
                                                                               {
                                                                                   IsolationLevel = IsolationLevel.ReadUncommitted
                                                                               }))
                    {
                        try
                        {
                            integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                            var oOperations = (from r in dbContext.OPERATIONs
                                            where r.OPE_CUSINV_ID == null &&
                                                  r.OPE_DATE >= ctnFirstDateOfInvoiceVersion_2 &&
                                                  r.OPE_DATE < dt &&
                                                  r.USER.CUSTOMER.CUS_ENABLED == 1
                                            orderby r.OPE_DATE ascending
                                            select r).Take(ctnNumRegistriesToCommitInInvoicingProcess).AsQueryable();

                            if (oOperations.Count() > 0)
                            {
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForOperations: Init Process"));


                                foreach (OPERATION oOperation in oOperations)
                                {

                                    decimal? dCustomerInvoiceID = null;

                                    int iTotalAmount = 0;

                                    if ((ChargeOperationsType)oOperation.OPE_TYPE != ChargeOperationsType.ParkingRefund)
                                        iTotalAmount = oOperation.OPE_TOTAL_AMOUNT.Value;
                                    else
                                        iTotalAmount = -oOperation.OPE_TOTAL_AMOUNT.Value;

                                    GetCustomerInvoice(dbContext, oOperation.OPE_DATE,
                                                       oOperation.USER.USR_CUS_ID, oOperation.OPE_AMOUNT_CUR_ID,0, iTotalAmount,
                                                       null, out dCustomerInvoiceID);

                                    if (dCustomerInvoiceID != null)
                                    {
                                        oOperation.OPE_CUSINV_ID = dCustomerInvoiceID;
                                        try
                                        {
                                            SecureSubmitChanges(ref dbContext);
                                            iNumOperations++;

                                        }
                                        catch (Exception e)
                                        {
                                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForOperations: ", e);
                                            bRes = false;
                                            return bRes;
                                        }


                                    }


                                }

                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForOperations: End Process"));
                            }
                            else
                                bExit = true;

                            if (!bExit)
                            {
                                
                                // Submit the change to the database.
                                try
                                {

                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForOperations: Commmitting"));
                                    transaction.Complete();
                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoicesForOperations: Committed"));

                                }
                                catch (Exception e)
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForOperations: ", e);
                                    bRes = false;
                                }
                                
                            }
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForOperations: ", e);
                            bRes = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoicesForRecharges: ", e);
                bRes = false;
            }

            return bRes;

        }




        public bool GenerateInvoices(DateTime endDate, out int iNumInvoices)
        {
            bool bRes = true;
            iNumInvoices = 0;

            try
            {
                bool bExit = false;
                while (!bExit)
                {

                    using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                               new TransactionOptions()
                                                                               {
                                                                                   IsolationLevel = IsolationLevel.ReadUncommitted                                                                                   
                                                                               }))
                    {
                        try
                        {
                            integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();


                            var oInvoices = (from r in dbContext.CUSTOMER_INVOICEs
                                             where r.CUSINV_INV_NUMBER == null &&
                                                   r.CUSINV_DATEINI >= ctnFirstDateOfInvoiceVersion_2 &&
                                                   r.CUSINV_DATEEND <= endDate &&
                                                   r.CUSTOMER.CUS_ENABLED == 1
                                             orderby r.CUSINV_GENERATION_DATE
                                             select r).Take(ctnNumRegistriesToCommitInInvoicingProcess).AsQueryable();

                            if (oInvoices.Count() > 0)
                            {
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoices: Init Process"));


                                foreach (CUSTOMER_INVOICE oInvoice in oInvoices)
                                {


                                    if (oInvoice.OPERATOR != null)
                                    {

                                        if (oInvoice.OPERATOR.OPR_CURRENT_INVOICE_NUMBER <= oInvoice.OPERATOR.OPR_END_INVOICE_NUMBER)
                                        {

                                            oInvoice.CUSINV_INV_NUMBER = oInvoice.OPERATOR.OPR_CURRENT_INVOICE_NUMBER.ToString();
                                            oInvoice.CUSINV_INV_DATE = oInvoice.CUSINV_DATEEND.AddHours(-1).Date;
                                            oInvoice.OPERATOR.OPR_CURRENT_INVOICE_NUMBER++;

                                            try
                                            {
                                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoices: Creating Invoice {0} for {1} ", oInvoice.CUSINV_INV_NUMBER, oInvoice.CUSTOMER.USER.USR_USERNAME));
                                                SecureSubmitChanges(ref dbContext);
                                                iNumInvoices++;

                                            }
                                            catch (Exception e)
                                            {
                                                m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoices: ", e);
                                                bRes = false;
                                                return bRes;
                                            }

                                        }


                                    }
                                }
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoices: End Process"));


                            }
                            else
                                bExit = true;

                            if (!bExit)
                            {

                                // Submit the change to the database.
                                try
                                {

                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoices: Commmitting"));
                                    transaction.Complete();
                                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GenerateInvoices: Committed"));

                                }
                                catch (Exception e)
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoices: ", e);
                                    bRes = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoices: ", e);
                            bRes = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GenerateInvoices: ", e);
                bRes = false;
            }

            return bRes;

        }

       

        public bool GetWaitingConfirmationOperation(out List<OperationConfirmData> oOperations, out int iQueueLength, 
                                                           int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries, 
                                                           int iMaxResendTime, List<decimal> oListRunningOperations, 
                                                           int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList)


        {
            bool bRes = true;
            oOperations = new List<OperationConfirmData>();
            iQueueLength = 0;

            try
            {
               
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                                new TransactionOptions()
                                                                                                {
                                                                                                    IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                    Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                                }))
                {

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();
                    /*var predicate = PredicateBuilder.True<OPERATION>();

                    if ((oListRunningOperations != null) && (oListRunningOperations.Count() > 0))
                    {
                        foreach (decimal OpeId in oListRunningOperations)
                        {
                            predicate = predicate.And(a => a.OPE_ID != OpeId);
                        }
                    }*/



                    var oAllOperations = (from r in dbContext.OPERATIONs
                                        where (r.INSTALLATION.INS_OPT_OPERATIONCONFIRM_MODE ?? 0) == 1 &&
                                            (!oListRunningOperations.Contains(r.OPE_ID))&&
                                                ((ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingOperation ||
                                                (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ExtensionOperation ||
                                                (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingRefund) &&
                                                ((r.OPE_CONFIRMED_IN_WS1 == 0 && (!r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS1_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS1_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS1_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS1_RETRIES_NUM < iMaxRetries))) ||
                                                (r.OPE_CONFIRMED_IN_WS2 == 0 && (!r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS2_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS2_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS2_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS2_RETRIES_NUM < iMaxRetries))) ||
                                                (r.OPE_CONFIRMED_IN_WS3 == 0 && (!r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS3_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS3_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS3_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS3_RETRIES_NUM < iMaxRetries)))) &&
                                                (r.OPE_INSERTION_UTC_DATE.Value.AddSeconds(iSecondsWait)<=dbContext.GetUTCDate())
                                        select r)
                                        .OrderBy(r => r.OPE_INSERTION_UTC_DATE)
                                        .AsQueryable();


                    if (oAllOperations.Count() > 0)
                    {
                        iQueueLength = oAllOperations.Count();
                       
                        /*List<string> oLstConfirmURLs = oConfirmDataList.Select(r => r.URL).ToList();

                        foreach (URLConfirmData oConfirmData in oConfirmDataList.OrderBy(r => r.AssignedElements))
                        {
                            if (oConfirmData.MaxElementsToReturn > 0)
                            {
                                List<OperationConfirmData> oURLOperations = oAllOperations.
                                    Where(r => r.INSTALLATION.INS_PARK_CONFIRM_WS_URL == oConfirmData.URL ||
                                            r.INSTALLATION.INS_PARK_CONFIRM_WS2_URL == oConfirmData.URL ||
                                            r.INSTALLATION.INS_PARK_CONFIRM_WS3_URL == oConfirmData.URL).
                                    Take(oConfirmData.MaxElementsToReturn).
                                    Select(r => new OperationConfirmData
                                    {
                                        OPE_ID = r.OPE_ID,
                                        OPE_TYPE = r.OPE_TYPE,
                                        OPE_CONFIRMED_IN_WS1 = r.OPE_CONFIRMED_IN_WS1,
                                        OPE_CONFIRMED_IN_WS2 = r.OPE_CONFIRMED_IN_WS2,
                                        OPE_CONFIRMED_IN_WS3 = r.OPE_CONFIRMED_IN_WS3,
                                        OPE_CONFIRM_IN_WS1_RETRIES_NUM = r.OPE_CONFIRM_IN_WS1_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS2_RETRIES_NUM = r.OPE_CONFIRM_IN_WS2_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS3_RETRIES_NUM = r.OPE_CONFIRM_IN_WS3_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS1_DATE = r.OPE_CONFIRM_IN_WS1_DATE,
                                        OPE_CONFIRM_IN_WS2_DATE = r.OPE_CONFIRM_IN_WS2_DATE,
                                        OPE_CONFIRM_IN_WS3_DATE = r.OPE_CONFIRM_IN_WS3_DATE,
                                        OPE_GRP_ID = r.OPE_GRP_ID,
                                        OPE_TAR_ID = r.OPE_TAR_ID,
                                        OPE_DATE = r.OPE_DATE,
                                        OPE_INIDATE = r.OPE_INIDATE,
                                        OPE_ENDDATE = r.OPE_ENDDATE,
                                        OPE_AMOUNT = r.OPE_AMOUNT,
                                        OPE_TIME = r.OPE_TIME,
                                        OPE_EXTERNAL_ID1 = r.OPE_EXTERNAL_ID1,
                                        OPE_EXTERNAL_ID2 = r.OPE_EXTERNAL_ID2,
                                        OPE_EXTERNAL_ID3 = r.OPE_EXTERNAL_ID3,
                                        OPE_INSERTION_UTC_DATE = r.OPE_INSERTION_UTC_DATE,
                                        OPE_MOSE_ID = r.OPE_MOSE_ID,
                                        OPE_LATITUDE = r.OPE_LATITUDE,
                                        OPE_LONGITUDE = r.OPE_LONGITUDE,
                                        OPE_PERC_VAT1 = r.OPE_PERC_VAT1,
                                        OPE_PERC_VAT2 = r.OPE_PERC_VAT2,
                                        OPE_PERC_FEE = r.OPE_PERC_FEE,
                                        OPE_PERC_FEE_TOPPED = r.OPE_PERC_FEE_TOPPED,
                                        OPE_FIXED_FEE = r.OPE_FIXED_FEE,
                                        OPE_PERC_BONUS = r.OPE_PERC_BONUS,
                                        OPE_BONUS_MARCA = r.OPE_BONUS_MARCA,
                                        OPE_BONUS_TYPE = r.OPE_BONUS_TYPE,
                                        INSTALLATION = r.INSTALLATION,
                                        USER_PLATE = r.USER_PLATE,
                                        USER = r.USER,
                                    }).ToList();

                                if (oURLOperations.Count() > 0)
                                {
                                    oOperations.AddRange(oURLOperations);
                                }
                            }
                        }*/

                        oOperations.AddRange(oAllOperations.
                                    /*Where(r => !oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS_URL) &&
                                                !oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS2_URL) &&
                                                !oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS3_URL)).*/
                                    Take(iMaxWorkingThreads).
                                        Select(r => new OperationConfirmData
                                        {
                                            OPE_ID = r.OPE_ID,
                                            OPE_TYPE = r.OPE_TYPE,
                                            OPE_CONFIRMED_IN_WS1 = r.OPE_CONFIRMED_IN_WS1,
                                            OPE_CONFIRMED_IN_WS2 = r.OPE_CONFIRMED_IN_WS2,
                                            OPE_CONFIRMED_IN_WS3 = r.OPE_CONFIRMED_IN_WS3,
                                            OPE_CONFIRM_IN_WS1_RETRIES_NUM = r.OPE_CONFIRM_IN_WS1_RETRIES_NUM,
                                            OPE_CONFIRM_IN_WS2_RETRIES_NUM = r.OPE_CONFIRM_IN_WS2_RETRIES_NUM,
                                            OPE_CONFIRM_IN_WS3_RETRIES_NUM = r.OPE_CONFIRM_IN_WS3_RETRIES_NUM,
                                            OPE_CONFIRM_IN_WS1_DATE = r.OPE_CONFIRM_IN_WS1_DATE,
                                            OPE_CONFIRM_IN_WS2_DATE = r.OPE_CONFIRM_IN_WS2_DATE,
                                            OPE_CONFIRM_IN_WS3_DATE = r.OPE_CONFIRM_IN_WS3_DATE,
                                            OPE_GRP_ID = r.OPE_GRP_ID,
                                            OPE_TAR_ID = r.OPE_TAR_ID,
                                            OPE_DATE = r.OPE_DATE,
                                            OPE_INIDATE = r.OPE_INIDATE,
                                            OPE_ENDDATE = r.OPE_ENDDATE,
                                            OPE_AMOUNT = r.OPE_AMOUNT,
                                            OPE_REAL_AMOUNT = r.OPE_REAL_AMOUNT??r.OPE_AMOUNT,
                                            OPE_TIME = r.OPE_TIME,
                                            OPE_POSTPAY = r.OPE_POSTPAY??0,
                                            OPE_EXTERNAL_ID1 = r.OPE_EXTERNAL_ID1,
                                            OPE_EXTERNAL_ID2 = r.OPE_EXTERNAL_ID2,
                                            OPE_EXTERNAL_ID3 = r.OPE_EXTERNAL_ID3,
                                            OPE_INSERTION_UTC_DATE = r.OPE_INSERTION_UTC_DATE,
                                            OPE_MOSE_ID = r.OPE_MOSE_ID,
                                            OPE_LATITUDE = r.OPE_LATITUDE,
                                            OPE_LONGITUDE = r.OPE_LONGITUDE,
                                            OPE_PERC_VAT1 = r.OPE_PERC_VAT1,
                                            OPE_PERC_VAT2 = r.OPE_PERC_VAT2,
                                            OPE_PERC_FEE = r.OPE_PERC_FEE,
                                            OPE_PERC_FEE_TOPPED = r.OPE_PERC_FEE_TOPPED,
                                            OPE_FIXED_FEE = r.OPE_FIXED_FEE,
                                            OPE_PERC_BONUS = r.OPE_PERC_BONUS,
                                            OPE_BONUS_MARCA = r.OPE_BONUS_MARCA,
                                            OPE_BONUS_TYPE = r.OPE_BONUS_TYPE,
                                            OPE_SPACE_STRING = r.OPE_SPACE_STRING,
                                            INSTALLATION = r.INSTALLATION,
                                            USER_PLATE = r.USER_PLATE,
                                            USER = r.USER,
                                        }).ToList());
                       
                    }
                                                   
                    dbContext.Close();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingConfirmationParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }


        /*public bool GetWaitingConfirmationOperation(out List<OperationConfirmData> oOperations, out int iQueueLength,
                                                   int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries,
                                                   int iMaxResendTime, List<decimal> oListRunningOperations,
                                                   int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList)
        {
            bool bRes = true;
            oOperations = new List<OperationConfirmData>();
            iQueueLength = 0;

            try
            {

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                                new TransactionOptions()
                                                                                                {
                                                                                                    IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                    Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                                }))
                {

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                    List<string> oLstConfirmURLs = oConfirmDataList.Select(r => r.URL).ToList();

                    m_Log.LogMessage(LogLevels.logINFO, string.Format("GetWaitingConfirmationOperation --> ConfirmDataList.Count={0}",
                        oConfirmDataList.Count()));


                    foreach (URLConfirmData oConfirmData in oConfirmDataList.OrderBy(r => r.AssignedElements))
                    {
                        m_Log.LogMessage(LogLevels.logINFO, string.Format("GetWaitingConfirmationOperation --> URL={0};  MaxElementsToReturn={1}",
                        oConfirmData.URL, oConfirmData.MaxElementsToReturn));

                        if (oConfirmData.MaxElementsToReturn > 0)
                        {


                            var oAllOperations = (from r in dbContext.OPERATIONs
                                                  where (r.INSTALLATION.INS_OPT_OPERATIONCONFIRM_MODE ?? 0) == 1 &&
                                                      (!oListRunningOperations.Contains(r.OPE_ID)) &&
                                                          ((ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingOperation ||
                                                          (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ExtensionOperation ||
                                                          (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingRefund) &&
                                                          ((r.OPE_CONFIRMED_IN_WS1 == 0 && (!r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS1_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS1_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS1_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS1_RETRIES_NUM < iMaxRetries))) ||
                                                          (r.OPE_CONFIRMED_IN_WS2 == 0 && (!r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS2_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS2_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS2_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS2_RETRIES_NUM < iMaxRetries))) ||
                                                          (r.OPE_CONFIRMED_IN_WS3 == 0 && (!r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS3_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS3_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS3_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS3_RETRIES_NUM < iMaxRetries)))) &&
                                                          (r.OPE_INSERTION_UTC_DATE.Value.AddSeconds(iSecondsWait) <= dbContext.GetUTCDate()) &&
                                                          (r.INSTALLATION.INS_PARK_CONFIRM_WS_URL == oConfirmData.URL ||
                                                          r.INSTALLATION.INS_PARK_CONFIRM_WS2_URL == oConfirmData.URL ||
                                                          r.INSTALLATION.INS_PARK_CONFIRM_WS3_URL == oConfirmData.URL)
                                                  select r)
                                                .OrderBy(r => r.OPE_INSERTION_UTC_DATE)
                                                .AsQueryable();

                            iQueueLength += oAllOperations.Count();
                            if (oAllOperations.Count() > 0)
                            {

                                List<OperationConfirmData> oURLOperations = oAllOperations.
                                    Take(oConfirmData.MaxElementsToReturn).
                                    Select(r => new OperationConfirmData
                                    {
                                        OPE_ID = r.OPE_ID,
                                        OPE_TYPE = r.OPE_TYPE,
                                        OPE_CONFIRMED_IN_WS1 = r.OPE_CONFIRMED_IN_WS1,
                                        OPE_CONFIRMED_IN_WS2 = r.OPE_CONFIRMED_IN_WS2,
                                        OPE_CONFIRMED_IN_WS3 = r.OPE_CONFIRMED_IN_WS3,
                                        OPE_CONFIRM_IN_WS1_RETRIES_NUM = r.OPE_CONFIRM_IN_WS1_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS2_RETRIES_NUM = r.OPE_CONFIRM_IN_WS2_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS3_RETRIES_NUM = r.OPE_CONFIRM_IN_WS3_RETRIES_NUM,
                                        OPE_CONFIRM_IN_WS1_DATE = r.OPE_CONFIRM_IN_WS1_DATE,
                                        OPE_CONFIRM_IN_WS2_DATE = r.OPE_CONFIRM_IN_WS2_DATE,
                                        OPE_CONFIRM_IN_WS3_DATE = r.OPE_CONFIRM_IN_WS3_DATE,
                                        OPE_GRP_ID = r.OPE_GRP_ID,
                                        OPE_TAR_ID = r.OPE_TAR_ID,
                                        OPE_DATE = r.OPE_DATE,
                                        OPE_INIDATE = r.OPE_INIDATE,
                                        OPE_ENDDATE = r.OPE_ENDDATE,
                                        OPE_AMOUNT = r.OPE_AMOUNT,
                                        OPE_TIME = r.OPE_TIME,
                                        OPE_EXTERNAL_ID1 = r.OPE_EXTERNAL_ID1,
                                        OPE_EXTERNAL_ID2 = r.OPE_EXTERNAL_ID2,
                                        OPE_EXTERNAL_ID3 = r.OPE_EXTERNAL_ID3,
                                        OPE_INSERTION_UTC_DATE = r.OPE_INSERTION_UTC_DATE,
                                        OPE_MOSE_ID = r.OPE_MOSE_ID,
                                        OPE_LATITUDE = r.OPE_LATITUDE,
                                        OPE_LONGITUDE = r.OPE_LONGITUDE,
                                        OPE_PERC_VAT1 = r.OPE_PERC_VAT1,
                                        OPE_PERC_VAT2 = r.OPE_PERC_VAT2,
                                        OPE_PERC_FEE = r.OPE_PERC_FEE,
                                        OPE_PERC_FEE_TOPPED = r.OPE_PERC_FEE_TOPPED,
                                        OPE_FIXED_FEE = r.OPE_FIXED_FEE,
                                        OPE_PERC_BONUS = r.OPE_PERC_BONUS,
                                        OPE_BONUS_MARCA = r.OPE_BONUS_MARCA,
                                        OPE_BONUS_TYPE = r.OPE_BONUS_TYPE,
                                        INSTALLATION = r.INSTALLATION,
                                        USER_PLATE = r.USER_PLATE,
                                        USER = r.USER,
                                    }).ToList();

                                if (oURLOperations.Count() > 0)
                                {
                                    oOperations.AddRange(oURLOperations);
                                }

                                m_Log.LogMessage(LogLevels.logINFO, string.Format("GetWaitingConfirmationOperation --> URL={0};  MaxElementsToReturn={1}; Returned={2}",
                                     oConfirmData.URL, oConfirmData.MaxElementsToReturn, oURLOperations.Count()));


                            }
                        }
                    }

                    var oAllOperationsWithoutURL = (from r in dbContext.OPERATIONs
                                                    where (r.INSTALLATION.INS_OPT_OPERATIONCONFIRM_MODE ?? 0) == 1 &&
                                                        (!oListRunningOperations.Contains(r.OPE_ID)) &&
                                                            ((ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingOperation ||
                                                            (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ExtensionOperation ||
                                                            (ChargeOperationsType)r.OPE_TYPE == ChargeOperationsType.ParkingRefund) &&
                                                            ((r.OPE_CONFIRMED_IN_WS1 == 0 && (!r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS1_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS1_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS1_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS1_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS1_RETRIES_NUM < iMaxRetries))) ||
                                                            (r.OPE_CONFIRMED_IN_WS2 == 0 && (!r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS2_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS2_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS2_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS2_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS2_RETRIES_NUM < iMaxRetries))) ||
                                                            (r.OPE_CONFIRMED_IN_WS3 == 0 && (!r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.HasValue || r.OPE_CONFIRM_IN_WS3_RETRIES_NUM == 0 || (r.OPE_CONFIRM_IN_WS3_DATE.HasValue && DateTime.UtcNow >= r.OPE_CONFIRM_IN_WS3_DATE.Value.AddSeconds(Math.Min(iMaxResendTime, r.OPE_CONFIRM_IN_WS3_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPE_CONFIRM_IN_WS3_RETRIES_NUM < iMaxRetries)))) &&
                                                            (r.OPE_INSERTION_UTC_DATE.Value.AddSeconds(iSecondsWait) <= dbContext.GetUTCDate()) &&
                                                            (!oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS_URL ?? "")) &&
                                                            (!oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS2_URL ?? "")) &&
                                                            (!oLstConfirmURLs.Contains(r.INSTALLATION.INS_PARK_CONFIRM_WS3_URL ?? ""))
                                                    select r)
                                               .OrderBy(r => r.OPE_INSERTION_UTC_DATE)
                                               .AsQueryable();

                    iQueueLength += oAllOperationsWithoutURL.Count();
                    if (oAllOperationsWithoutURL.Count() > 0)
                    {

                        oOperations.AddRange(oAllOperationsWithoutURL.
                                        Take(1).
                                            Select(r => new OperationConfirmData
                                            {
                                                OPE_ID = r.OPE_ID,
                                                OPE_TYPE = r.OPE_TYPE,
                                                OPE_CONFIRMED_IN_WS1 = r.OPE_CONFIRMED_IN_WS1,
                                                OPE_CONFIRMED_IN_WS2 = r.OPE_CONFIRMED_IN_WS2,
                                                OPE_CONFIRMED_IN_WS3 = r.OPE_CONFIRMED_IN_WS3,
                                                OPE_CONFIRM_IN_WS1_RETRIES_NUM = r.OPE_CONFIRM_IN_WS1_RETRIES_NUM,
                                                OPE_CONFIRM_IN_WS2_RETRIES_NUM = r.OPE_CONFIRM_IN_WS2_RETRIES_NUM,
                                                OPE_CONFIRM_IN_WS3_RETRIES_NUM = r.OPE_CONFIRM_IN_WS3_RETRIES_NUM,
                                                OPE_CONFIRM_IN_WS1_DATE = r.OPE_CONFIRM_IN_WS1_DATE,
                                                OPE_CONFIRM_IN_WS2_DATE = r.OPE_CONFIRM_IN_WS2_DATE,
                                                OPE_CONFIRM_IN_WS3_DATE = r.OPE_CONFIRM_IN_WS3_DATE,
                                                OPE_GRP_ID = r.OPE_GRP_ID,
                                                OPE_TAR_ID = r.OPE_TAR_ID,
                                                OPE_DATE = r.OPE_DATE,
                                                OPE_INIDATE = r.OPE_INIDATE,
                                                OPE_ENDDATE = r.OPE_ENDDATE,
                                                OPE_AMOUNT = r.OPE_AMOUNT,
                                                OPE_TIME = r.OPE_TIME,
                                                OPE_EXTERNAL_ID1 = r.OPE_EXTERNAL_ID1,
                                                OPE_EXTERNAL_ID2 = r.OPE_EXTERNAL_ID2,
                                                OPE_EXTERNAL_ID3 = r.OPE_EXTERNAL_ID3,
                                                OPE_INSERTION_UTC_DATE = r.OPE_INSERTION_UTC_DATE,
                                                OPE_MOSE_ID = r.OPE_MOSE_ID,
                                                OPE_LATITUDE = r.OPE_LATITUDE,
                                                OPE_LONGITUDE = r.OPE_LONGITUDE,
                                                OPE_PERC_VAT1 = r.OPE_PERC_VAT1,
                                                OPE_PERC_VAT2 = r.OPE_PERC_VAT2,
                                                OPE_PERC_FEE = r.OPE_PERC_FEE,
                                                OPE_PERC_FEE_TOPPED = r.OPE_PERC_FEE_TOPPED,
                                                OPE_FIXED_FEE = r.OPE_FIXED_FEE,
                                                OPE_PERC_BONUS = r.OPE_PERC_BONUS,
                                                OPE_BONUS_MARCA = r.OPE_BONUS_MARCA,
                                                OPE_BONUS_TYPE = r.OPE_BONUS_TYPE,
                                                INSTALLATION = r.INSTALLATION,
                                                USER_PLATE = r.USER_PLATE,
                                                USER = r.USER,
                                            }).ToList());

                    }

                    dbContext.Close();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingConfirmationParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }*/


        /*public OPERATION GetOperation(decimal dOpeId)
        {

            OPERATION oOperation = null;
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


                    oOperation = (from r in dbContext.OPERATIONs
                                      where (r.OPE_ID == dOpeId)
                                      select r).First();
                                     
                }
                

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperation: ", e);
                oOperation = null;
            }

            return oOperation;

        }*/


        public bool UpdateThirdPartyConfirmedInParkingOperation(decimal dOperationID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out OPERATION oOperation)
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
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        oOperation = dbContext.OPERATIONs.Where(r => r.OPE_ID == dOperationID).First();

                        switch (iWSNumber)
                        {
                            case 1: 
                                oOperation.OPE_CONFIRMED_IN_WS1 = bConfirmed ? 1 : 0;
                                oOperation.OPE_CONFIRM_IN_WS1_DATE = DateTime.UtcNow;
                                oOperation.OPE_CONFIRMATION_TIME_IN_WS1 = Math.Truncate(((oOperation.OPE_CONFIRMATION_TIME_IN_WS1 ?? 0)*(oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM ?? 0)+lEllapsedTime)/((oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM ?? 0)+1));
                                oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS1 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS1 ?? 0) * (oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + 1))));

                                if (!bConfirmed)
                                    oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM = (oOperation.OPE_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPE_EXTERNAL_ID1 = str3dPartyOpNum;
                                    }
                                }

                                break;
                            case 2: 
                                oOperation.OPE_CONFIRMED_IN_WS2 = bConfirmed ? 1 : 0;
                                oOperation.OPE_CONFIRM_IN_WS2_DATE = DateTime.UtcNow;
                                oOperation.OPE_CONFIRMATION_TIME_IN_WS2 = Math.Truncate(((oOperation.OPE_CONFIRMATION_TIME_IN_WS2 ?? 0) * (oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1));
                                oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS2 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS2 ?? 0) * (oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1))));
                                if (!bConfirmed)                                    
                                    oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM = (oOperation.OPE_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPE_EXTERNAL_ID2 = str3dPartyOpNum;
                                    }
                                }
                                break;
                            case 3: 
                                oOperation.OPE_CONFIRMED_IN_WS3 = bConfirmed ? 1 : 0;
                                oOperation.OPE_CONFIRM_IN_WS3_DATE = DateTime.UtcNow;
                                oOperation.OPE_CONFIRMATION_TIME_IN_WS3 = Math.Truncate(((oOperation.OPE_CONFIRMATION_TIME_IN_WS3 ?? 0) * (oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1));
                                oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS3 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPE_QUEUE_LENGTH_BEFORE_CONFIRM_WS3 ?? 0) * (oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1))));

                                if (!bConfirmed)                                    
                                    oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM = (oOperation.OPE_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPE_EXTERNAL_ID3 = str3dPartyOpNum;
                                    }
                                }                                                                 
                                break;
                        }


                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();                            
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetWaitingConfirmationFine(out List<TicketPaymentConfirmData> oTicketPayments, out int iQueueLength,int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries,
                                                 int m_iMaxResendTime, List<decimal> oListRunningFines, int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList)
        {
            bool bRes = true;
            oTicketPayments = new List<TicketPaymentConfirmData>();
            iQueueLength = 0;

            try
            {

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                               new TransactionOptions()
                                                                                               {
                                                                                                   IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                   Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                               }))
                {

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();
                    /*var predicate = PredicateBuilder.True<TICKET_PAYMENT>();


                    if ((oListRunningFines != null) && (oListRunningFines.Count() > 0))
                    {
                        foreach (decimal TipaId in oListRunningFines)
                        {
                            predicate=predicate.And(a => a.TIPA_ID != TipaId);
                        }
                    }*/

                    var oAllTicketPayments = (from r in dbContext.TICKET_PAYMENTs
                                       where (r.INSTALLATION.INS_OPT_FINECONFIRM_MODE ?? 0) == 1 &&
                                             (!oListRunningFines.Contains(r.TIPA_ID))&&
                                             ((r.TIPA_CONFIRMED_IN_WS ?? 0) == 0 && (!r.TIPA_CONFIRM_IN_WS_RETRIES_NUM.HasValue || r.TIPA_CONFIRM_IN_WS_RETRIES_NUM == 0 || (r.TIPA_CONFIRM_IN_WS_DATE.HasValue && DateTime.UtcNow >= r.TIPA_CONFIRM_IN_WS_DATE.Value.AddSeconds(Math.Min(m_iMaxResendTime,r.TIPA_CONFIRM_IN_WS_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.TIPA_CONFIRM_IN_WS_RETRIES_NUM < iMaxRetries)))
                                       select r)
                                       .Where(r => r.TIPA_INSERTION_UTC_DATE.Value.AddSeconds(iSecondsWait) <= dbContext.GetUTCDate())
                                       .OrderBy(r => r.TIPA_INSERTION_UTC_DATE)                                      
                                       .AsQueryable();

                    if (oAllTicketPayments.Count() > 0)
                    
                    {
                        //List<string> oLstConfirmURLs = oConfirmDataList.Select(r => r.URL).ToList();
                        iQueueLength = oAllTicketPayments.Count();

                        /*foreach (URLConfirmData oConfirmData in oConfirmDataList.OrderBy(r => r.AssignedElements))
                        {
                            if (oConfirmData.MaxElementsToReturn > 0)
                            {
                                List<TicketPaymentConfirmData> oURLOperations = oAllTicketPayments.
                                    Where(r => r.INSTALLATION.INS_FINE_WS_URL == oConfirmData.URL).
                                    Take(oConfirmData.MaxElementsToReturn).
                                    Select(r => new TicketPaymentConfirmData
                                        {
                                        TIPA_ID = r.TIPA_ID,
                                        TIPA_DATE = r.TIPA_DATE,
                                        TIPA_TICKET_NUMBER = r.TIPA_TICKET_NUMBER,
                                        TIPA_AMOUNT = r.TIPA_AMOUNT,
                                        TIPA_EXTERNAL_ID = r.TIPA_EXTERNAL_ID,
                                        TIPA_INSERTION_UTC_DATE = r.TIPA_INSERTION_UTC_DATE,
                                        TIPA_CONFIRMED_IN_WS = r.TIPA_CONFIRMED_IN_WS,
                                        TIPA_CONFIRM_IN_WS_RETRIES_NUM = r.TIPA_CONFIRM_IN_WS_RETRIES_NUM,
                                        TIPA_CONFIRM_IN_WS_DATE = r.TIPA_CONFIRM_IN_WS_DATE,
                                        TIPA_GRP_ID = r.TIPA_GRP_ID,
                                        INSTALLATION = r.INSTALLATION,
                                        USER = r.USER,
                                        }).ToList();

                                if (oURLOperations.Count() > 0)
                                {
                                    oTicketPayments.AddRange(oURLOperations);
                                }
                            }
                        }*/


                        oTicketPayments.AddRange(oAllTicketPayments.
                                    //Where(r => !oLstConfirmURLs.Contains(r.INSTALLATION.INS_FINE_WS_URL)).
                                    Take(iMaxWorkingThreads).
                                    Select(r => new TicketPaymentConfirmData
                                        {
                                            TIPA_ID = r.TIPA_ID,
                                            TIPA_DATE = r.TIPA_DATE,
                                            TIPA_TICKET_NUMBER = r.TIPA_TICKET_NUMBER,
                                            TIPA_AMOUNT = r.TIPA_AMOUNT,
                                            TIPA_EXTERNAL_ID = r.TIPA_EXTERNAL_ID,
                                            TIPA_EXTERNAL_ID2 = r.TIPA_EXTERNAL_ID2,
                                            TIPA_EXTERNAL_ID3 = r.TIPA_EXTERNAL_ID3,
                                            TIPA_INSERTION_UTC_DATE = r.TIPA_INSERTION_UTC_DATE,
                                            TIPA_CONFIRMED_IN_WS = r.TIPA_CONFIRMED_IN_WS,
                                            TIPA_CONFIRMED_IN_WS2 = r.TIPA_CONFIRMED_IN_WS2,
                                            TIPA_CONFIRMED_IN_WS3 = r.TIPA_CONFIRMED_IN_WS3,
                                            TIPA_CONFIRM_IN_WS_RETRIES_NUM = r.TIPA_CONFIRM_IN_WS_RETRIES_NUM,
                                            TIPA_CONFIRM_IN_WS2_RETRIES_NUM = r.TIPA_CONFIRM_IN_WS2_RETRIES_NUM,
                                            TIPA_CONFIRM_IN_WS3_RETRIES_NUM = r.TIPA_CONFIRM_IN_WS3_RETRIES_NUM,
                                            TIPA_CONFIRM_IN_WS_DATE = r.TIPA_CONFIRM_IN_WS_DATE,
                                            TIPA_PLATE_STRING = r.TIPA_PLATE_STRING,
                                            TIPA_TICKET_DATA = r.TIPA_TICKET_DATA,
                                            TIPA_GRP_ID = r.TIPA_GRP_ID,
                                            INSTALLATION = r.INSTALLATION,
                                            USER = r.USER,
                                        }).ToList());


                    }
                                                   
                    dbContext.Close();
                
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingConfirmationFine: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool UpdateThirdPartyConfirmedInFine(decimal dFineID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out TICKET_PAYMENT oTicketPayment)
        {
            bool bRes = true;
            oTicketPayment = null;

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
                        oTicketPayment = dbContext.TICKET_PAYMENTs.Where(r => r.TIPA_ID == dFineID).First();

                        switch (iWSNumber)
                        {
                            case 1:
                                oTicketPayment.TIPA_CONFIRMED_IN_WS = bConfirmed ? 1 : 0;
                                oTicketPayment.TIPA_CONFIRM_IN_WS_DATE = DateTime.UtcNow;
                                oTicketPayment.TIPA_CONFIRMATION_TIME_IN_WS = Math.Truncate(((oTicketPayment.TIPA_CONFIRMATION_TIME_IN_WS ?? 0) * (oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM ?? 0) + 1));
                                oTicketPayment.TIPA_QUEUE_LENGTH_BEFORE_CONFIRM_WS = Convert.ToInt32(Math.Truncate((decimal)((oTicketPayment.TIPA_QUEUE_LENGTH_BEFORE_CONFIRM_WS ?? 0) * (oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM ?? 0) + iQueueLength) / ((oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM ?? 0) + 1)));

                                if (!bConfirmed)
                                    oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM = (oTicketPayment.TIPA_CONFIRM_IN_WS_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oTicketPayment.TIPA_EXTERNAL_ID = str3dPartyOpNum;
                                    }
                                }

                                break;
                            case 2:
                                // ...
                                break;
                            case 3:
                                // ...
                                break;
                        }


                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFine: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFine: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInFine: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetWaitingConfirmationOffstreetOperation(out List<OperationOffStreetConfirmData> oOperations, out int iQueueLength,
                                                             int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries,
                                                             int m_iMaxResendTime, List<decimal> oListRunningOperations, int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList)
        {
            bool bRes = true;
            oOperations = new List<OperationOffStreetConfirmData>();
            iQueueLength = 0;

            try
            {

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                               new TransactionOptions()
                                                                                               {
                                                                                                   IsolationLevel = IsolationLevel.ReadCommitted,
                                                                                                   Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                               }))
                {

                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();
                    /*var predicate = PredicateBuilder.True<OPERATIONS_OFFSTREET>();

                    if ((oListRunningOperations != null) && (oListRunningOperations.Count() > 0))
                    {
                        foreach (decimal OpeId in oListRunningOperations)
                        {
                            predicate = predicate.And(a => a.OPEOFF_ID != OpeId);
                        }
                    }*/

                    var oConfirmableGroups = new List<decimal>();
                    var oOffstreetGroups = (from r in dbContext.GROUPs
                                              where r.GRP_TYPE == (int)GroupType.OffStreet
                                              select r).ToList();
                    foreach (GROUP oGroup in oOffstreetGroups)
                    {
                        if ((GetGroupOffstreetWsConfiguration(oGroup.GRP_ID, dbContext).GOWC_OPT_OPERATIONCONFIRM_MODE ?? 0) == 1)
                            oConfirmableGroups.Add(oGroup.GRP_ID);
                    }

                    var oAllOperations = (from r in dbContext.OPERATIONS_OFFSTREETs
                                       where oConfirmableGroups.Contains(r.OPEOFF_GRP_ID) &&
                                            (!oListRunningOperations.Contains(r.OPEOFF_ID))&&
                                             ((OffstreetOperationType)r.OPEOFF_TYPE == OffstreetOperationType.Exit ||
                                              (OffstreetOperationType)r.OPEOFF_TYPE == OffstreetOperationType.OverduePayment) &&
                                             ((r.OPEOFF_CONFIRMED_IN_WS1 == 0 && (!r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM.HasValue || r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM == 0 || (r.OPEOFF_CONFIRM_IN_WS1_DATE.HasValue && DateTime.UtcNow >= r.OPEOFF_CONFIRM_IN_WS1_DATE.Value.AddSeconds(Math.Min(m_iMaxResendTime, r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM < iMaxRetries))) ||
                                              (r.OPEOFF_CONFIRMED_IN_WS2 == 0 && (!r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM.HasValue || r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM == 0 || (r.OPEOFF_CONFIRM_IN_WS2_DATE.HasValue && DateTime.UtcNow >= r.OPEOFF_CONFIRM_IN_WS2_DATE.Value.AddSeconds(Math.Min(m_iMaxResendTime, r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM < iMaxRetries))) ||
                                              (r.OPEOFF_CONFIRMED_IN_WS3 == 0 && (!r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM.HasValue || r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM == 0 || (r.OPEOFF_CONFIRM_IN_WS3_DATE.HasValue && DateTime.UtcNow >= r.OPEOFF_CONFIRM_IN_WS3_DATE.Value.AddSeconds(Math.Min(m_iMaxResendTime, r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM.Value * iNumSecondsToWaitInCaseOfRetry)) && r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM < iMaxRetries))))
                                       select r)                                      
                                       .Where(r => r.OPEOFF_INSERTION_UTC_DATE.Value.AddSeconds(iSecondsWait) <= dbContext.GetUTCDate())
                                       .OrderBy(r => r.OPEOFF_INSERTION_UTC_DATE)
                                       .AsQueryable();


                    if (oAllOperations.Count() > 0)
                    {
                        //List<string> oLstConfirmURLs = oConfirmDataList.Select(r => r.URL).ToList();
                        iQueueLength = oAllOperations.Count();


                        /*foreach (URLConfirmData oConfirmData in oConfirmDataList.OrderBy(r => r.AssignedElements))
                        {
                            if (oConfirmData.MaxElementsToReturn > 0)
                            {
                                List<OperationOffStreetConfirmData> oURLOperations = oAllOperations.
                                    Where(r => r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS1_URL == oConfirmData.URL ||
                                               r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS2_URL == oConfirmData.URL ||
                                               r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS2_URL == oConfirmData.URL).
                                    Take(oConfirmData.MaxElementsToReturn).
                                    Select(r => new OperationOffStreetConfirmData
                                        {
                                            OPEOFF_ID = r.OPEOFF_ID,
                                            OPEOFF_TYPE = r.OPEOFF_TYPE,
                                            OPEOFF_GRP_ID = r.OPEOFF_GRP_ID,
                                            OPEOFF_LOGICAL_ID = r.OPEOFF_LOGICAL_ID,
                                            OPEOFF_TARIFF = r.OPEOFF_TARIFF,
                                            OPEOFF_GATE = r.OPEOFF_GATE,
                                            OPEOFF_INSERTION_UTC_DATE = r.OPEOFF_INSERTION_UTC_DATE,
                                            OPEOFF_ENTRY_DATE = r.OPEOFF_ENTRY_DATE,
                                            OPEOFF_END_DATE = r.OPEOFF_END_DATE,
                                            OPEOFF_AMOUNT = r.OPEOFF_AMOUNT,
                                            OPEOFF_TIME = r.OPEOFF_TIME,
                                            OPEOFF_CONFIRMED_IN_WS1 = r.OPEOFF_CONFIRMED_IN_WS1,
                                            OPEOFF_CONFIRMED_IN_WS2 = r.OPEOFF_CONFIRMED_IN_WS2,
                                            OPEOFF_CONFIRMED_IN_WS3 = r.OPEOFF_CONFIRMED_IN_WS3,
                                            OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS1_DATE = r.OPEOFF_CONFIRM_IN_WS1_DATE,
                                            OPEOFF_CONFIRM_IN_WS2_DATE = r.OPEOFF_CONFIRM_IN_WS2_DATE,
                                            OPEOFF_CONFIRM_IN_WS3_DATE = r.OPEOFF_CONFIRM_IN_WS3_DATE,
                                            CURRENCy = r.CURRENCy,
                                            USER_PLATE = r.USER_PLATE,
                                        }).ToList();    

                                if (oURLOperations.Count() > 0)
                                {
                                    oOperations.AddRange(oURLOperations);
                                }
                            }
                        }
                        */
                        oOperations.AddRange(oAllOperations.
                                    /*Where(r => !oLstConfirmURLs.Contains(r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS1_URL) &&
                                              !oLstConfirmURLs.Contains(r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS2_URL) &&
                                              !oLstConfirmURLs.Contains(r.GROUP.GROUPS_OFFSTREET_WS_CONFIGURATIONs.First().GOWC_EXIT_WS3_URL)).*/
                                    Take(iMaxWorkingThreads).
                                    Select(r => new OperationOffStreetConfirmData
                                        {
                                            OPEOFF_ID = r.OPEOFF_ID,
                                            OPEOFF_TYPE = r.OPEOFF_TYPE,
                                            OPEOFF_GRP_ID = r.OPEOFF_GRP_ID,
                                            OPEOFF_LOGICAL_ID = r.OPEOFF_LOGICAL_ID,
                                            OPEOFF_TARIFF = r.OPEOFF_TARIFF,
                                            OPEOFF_GATE = r.OPEOFF_GATE,
                                            OPEOFF_INSERTION_UTC_DATE = r.OPEOFF_INSERTION_UTC_DATE,
                                            OPEOFF_ENTRY_DATE = r.OPEOFF_ENTRY_DATE,
                                            OPEOFF_END_DATE = r.OPEOFF_END_DATE,
                                            OPEOFF_AMOUNT = r.OPEOFF_AMOUNT,
                                            OPEOFF_TIME = r.OPEOFF_TIME,
                                            OPEOFF_CONFIRMED_IN_WS1 = r.OPEOFF_CONFIRMED_IN_WS1,
                                            OPEOFF_CONFIRMED_IN_WS2 = r.OPEOFF_CONFIRMED_IN_WS2,
                                            OPEOFF_CONFIRMED_IN_WS3 = r.OPEOFF_CONFIRMED_IN_WS3,
                                            OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM = r.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM,
                                            OPEOFF_CONFIRM_IN_WS1_DATE = r.OPEOFF_CONFIRM_IN_WS1_DATE,
                                            OPEOFF_CONFIRM_IN_WS2_DATE = r.OPEOFF_CONFIRM_IN_WS2_DATE,
                                            OPEOFF_CONFIRM_IN_WS3_DATE = r.OPEOFF_CONFIRM_IN_WS3_DATE,
                                            CURRENCy = r.CURRENCy,
                                            USER_PLATE = r.USER_PLATE,
                                            USER = r.USER,
                                            CUSTOMER = r.USER.CUSTOMER
                                        }).ToList());

                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetWaitingConfirmationOffstreetOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool UpdateThirdPartyConfirmedInOffstreetOperation(decimal dOperationID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out OPERATIONS_OFFSTREET oOperation)
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
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        oOperation = dbContext.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationID).First();

                        switch (iWSNumber)
                        {
                            case 1:
                                oOperation.OPEOFF_CONFIRMED_IN_WS1 = bConfirmed ? 1 : 0;
                                oOperation.OPEOFF_CONFIRM_IN_WS1_DATE = DateTime.UtcNow;
                                oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS1 = Math.Truncate(((oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS1 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + 1));
                                oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS1 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS1 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + 1))));

                                if (!bConfirmed)
                                    oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM = (oOperation.OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPEOFF_EXTERNAL_ID1 = str3dPartyOpNum;
                                    }
                                }

                                break;
                            case 2:
                                oOperation.OPEOFF_CONFIRMED_IN_WS2 = bConfirmed ? 1 : 0;
                                oOperation.OPEOFF_CONFIRM_IN_WS2_DATE = DateTime.UtcNow;
                                oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS2 = Math.Truncate(((oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS2 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1));
                                oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS2 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS2 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1))));
                                if (!bConfirmed)
                                    oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM = (oOperation.OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPEOFF_EXTERNAL_ID2 = str3dPartyOpNum;
                                    }
                                }
                                break;
                            case 3:
                                oOperation.OPEOFF_CONFIRMED_IN_WS3 = bConfirmed ? 1 : 0;
                                oOperation.OPEOFF_CONFIRM_IN_WS3_DATE = DateTime.UtcNow;
                                oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS3 = Math.Truncate(((oOperation.OPEOFF_CONFIRMATION_TIME_IN_WS3 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + lEllapsedTime) / ((oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1));
                                oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS3 = Convert.ToInt32(Math.Truncate((decimal)(((oOperation.OPEOFF_QUEUE_LENGTH_BEFORE_CONFIRM_WS3 ?? 0) * (oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + iQueueLength) / ((oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1))));

                                if (!bConfirmed)
                                    oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM = (oOperation.OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM ?? 0) + 1;
                                else
                                {
                                    if (!string.IsNullOrEmpty(str3dPartyOpNum))
                                    {
                                        oOperation.OPEOFF_EXTERNAL_ID3 = str3dPartyOpNum;
                                    }
                                }
                                break;
                        }


                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInOffstreetOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInOffstreetOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInParkingOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetMobileSessionById(decimal dMobileSessionId, out MOBILE_SESSION oMobileSession)
        {
            bool bRet = true;
            oMobileSession = null;

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

                    var oSessions = (from r in dbContext.MOBILE_SESSIONs
                                     where (r.MOSE_ID == dMobileSessionId)
                                     select r).AsQueryable();

                    if (oSessions.Count() > 0)
                    {
                        oMobileSession = oSessions.First();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMobileSessionById: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool DeletePlate(ref USER user, string sPlate)
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

                        decimal userId = user.USR_ID;
                        var oPlate = (from r in dbContext.USER_PLATEs
                                      where r.USRP_USR_ID == userId && r.USRP_PLATE == sPlate && r.USRP_ENABLED == 1
                                      select r).First();

                        if (oPlate != null)
                        {
                            oPlate.USRP_ENABLED = 0;
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();
                        
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeletePlate: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeletePlate: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetFavouriteGroupFromUser(ref USER user, decimal? dInstallationId, DateTime xBeginDateUTC, DateTime xEndDateUTC, out decimal? dGroupId)
        {
            bool bRet = true;
            dGroupId = null;

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                    where r.USR_ID == userId && r.USR_ENABLED == 1
                                    select r).First();

                    if (oUser != null)
                    {
                        decimal dInsId = (dInstallationId.HasValue ? dInstallationId.Value : 0);
                        
                        var oParkings = (from t in oUser.OPERATIONs
                                         where t.OPE_TYPE == (int)ChargeOperationsType.ParkingOperation && 
                                               t.OPE_INS_ID == dInsId && 
                                               t.OPE_UTC_DATE >= xBeginDateUTC && t.OPE_UTC_DATE <= xEndDateUTC
                                          group t by t.OPE_GRP_ID into g
                                          select new
                                          {
                                              grpId = g.Key,
                                              countParkings = g.Count()
                                          }).AsQueryable().OrderByDescending(a => a.countParkings);
                        if (oParkings.Count() > 0)
                        {
                            dGroupId = oParkings.First().grpId;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFavouriteGroupFromUser: ", e);
                bRet = false;
            }

            return bRet;

        }

        public bool GetFavouriteAreasFromUser(ref USER user, decimal? dInstallationId, out List<USERS_FAVOURITES_AREA> oFavouriteAreas)
        {
            bool bRet = false;
            oFavouriteAreas = new List<USERS_FAVOURITES_AREA>();

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        oFavouriteAreas = (from t in oUser.USERS_FAVOURITES_AREAs
                                           where !dInstallationId.HasValue || t.USRA_INS_ID == dInstallationId.Value
                                           select t).ToList();
                        bRet = true;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFavouriteAreasFromUser: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool SetFavouriteAreasFromUser(ref USER user, List<USERS_FAVOURITES_AREA> oFavouriteAreas)
        {
            bool bRet = false;

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        dbContext.USERS_FAVOURITES_AREAs.DeleteAllOnSubmit(oUser.USERS_FAVOURITES_AREAs);

                        foreach (var oFavArea in oFavouriteAreas)
                        {
                            oFavArea.USER = oUser;
                        }
                        dbContext.USERS_FAVOURITES_AREAs.InsertAllOnSubmit(oFavouriteAreas);
                                                
                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        user = oUser;

                        bRet = true; 
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetFavouriteAreasFromUser: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool GetPreferredPlatesFromUser(ref USER user, decimal? dInstallationId, out List<USERS_PREFERRED_PLATE> oPreferredPlates)
        {
            bool bRet = true;
            oPreferredPlates = new List<USERS_PREFERRED_PLATE>();

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        oPreferredPlates = (from t in oUser.USERS_PREFERRED_PLATEs
                                           where !dInstallationId.HasValue || t.USRL_INS_ID == dInstallationId.Value
                                           select t).ToList();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetPreferredPlatesFromUser: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool SetPreferredPlatesFromUser(ref USER user, List<USERS_PREFERRED_PLATE> oPreferredPlates)
        {
            bool bRet = false;

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        dbContext.USERS_PREFERRED_PLATEs.DeleteAllOnSubmit(oUser.USERS_PREFERRED_PLATEs);

                        foreach (var oPrefPlate in oPreferredPlates)
                        {
                            oPrefPlate.USER = oUser;
                        }
                        dbContext.USERS_PREFERRED_PLATEs.InsertAllOnSubmit(oPreferredPlates);

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        user = oUser;

                        bRet = true;
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetPreferredPlatesFromUser: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool GetPlateFromUser(ref USER user, string sPlate, out USER_PLATE oUserPlate)
        {
            bool bRet = true;
            oUserPlate = null;

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
                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        oUserPlate = (from t in oUser.USER_PLATEs
                                      where t.USRP_PLATE == sPlate
                                      select t).FirstOrDefault();
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetPlateFromUser: ", e);
                bRet = false;
            }

            return bRet;
        }

        public bool AddSessionOperationOffstreetInfo(ref USER user,
                                                      string strSessionID,
                                                      OffstreetOperationType operationType,
                                                      string sLogicalId,
                                                      string strPlate,
                                                      decimal dGroupId,
                                                      string sTariff,
                                                      int iAmount,
                                                      int iPartialVAT1,
                                                      int iTime,
                                                      DateTime dtUTCDate,
                                                      DateTime dtUTCEntryDate,
                                                      DateTime dtUTCEndDate,
                                                      DateTime dtExitLimitUTCDateTime,
                                                      DateTime dtInstDateTime,
                                                      double dChangeToApply,
                                                      decimal dPercVat1, decimal dPercVat2,
                                                      decimal dPercFEE, int iPercFEETopped,
                                                      int iFixedFEE,
                                                      string sDiscountCodes)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {                            
                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (OPERATIONS_OFFSTREET_SESSION_INFO opSessionInfo in oSession.OPERATIONS_OFFSTREET_SESSION_INFOs)
                            {
                                dbContext.OPERATIONS_OFFSTREET_SESSION_INFOs.DeleteOnSubmit(opSessionInfo);
                            }

                            oSession.OPERATIONS_OFFSTREET_SESSION_INFOs.Add(new OPERATIONS_OFFSTREET_SESSION_INFO
                            {
                                OOSI_UTC_DATE = dtUTCDate,
                                OOSI_LOGICAL_ID = sLogicalId,
                                OOSI_PLATE = strPlate,
                                OOSI_GRP_ID = dGroupId,                                
                                OOSI_TARIFF = sTariff,
                                OOSI_AMOUNT = iAmount,
                                OOSI_PARTIAL_VAT1 = iPartialVAT1,
                                OOSI_TIME = iTime,
                                OOSI_ENTRY_UTC_DATE = dtUTCEntryDate,
                                OOSI_END_UTC_DATE = dtUTCEndDate,
                                OOSI_EXIT_LIMIT_UTC_DATE = dtExitLimitUTCDateTime,
                                OOSI_INS_DATE = dtInstDateTime,
                                OOSI_OPEOFF_TYPE = (int)operationType,
                                OOSI_CHANGE_APPLIED = Convert.ToDecimal(dChangeToApply),
                                OOSI_PERC_VAT1 = dPercVat1,
                                OOSI_PERC_VAT2 = dPercVat2,
                                OOSI_PERC_FEE = dPercFEE,
                                OOSI_PERC_FEE_TOPPED = iPercFEETopped,
                                OOSI_FIXED_FEE = iFixedFEE,
                                OOSI_DISCOUNT_CODES = sDiscountCodes
                            });
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationOffstreetInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationOffstreetInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddSessionOperationOffstreetInfo: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool CheckSessionOperationOffstreetInfo(ref USER user,
                                                string strSessionID,                                                
                                                decimal dGroupId,
                                                string sLogicalId,
                                                out string strPlate,
                                                out DateTime dtInstDateTime,
                                                out OffstreetOperationType operationType,
                                                out int iAmount,
                                                out int iPartialVAT1,
                                                out int iTime,
                                                out string sTariff,
                                                out double dChangeToApply,
                                                out DateTime dtSessionUTCDate,
                                                out DateTime dtUTCEntryDate,
                                                out DateTime dtUTCEndDate,
                                                out DateTime dtUTCExitLimitDate,
                                                out decimal dPercVat1, out decimal dPercVat2,
                                                out decimal dPercFEE, out int iPercFEETopped,
                                                out int iFixedFEE,
                                                out string sDiscounts)
        {
            bool bRes = false;

            strPlate = "";
            dtInstDateTime = DateTime.UtcNow;
            dtSessionUTCDate = DateTime.UtcNow;
            operationType = OffstreetOperationType.Exit;
            iAmount = 0;
            iPartialVAT1 = 0;
            iTime = 0;
            sTariff = "";
            dChangeToApply = 1.0;
            dtUTCEntryDate = DateTime.UtcNow;
            dtUTCEndDate = DateTime.UtcNow;
            dtUTCExitLimitDate = DateTime.UtcNow;
            dPercVat1 = 0;
            dPercVat2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;
            sDiscounts = "";

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


                        int iConfirmationTimeout = ctnDefaultOperationConfirmationTimeout;
                        try
                        {
                            iConfirmationTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ConfirmationTimeoutInSeconds"]);
                        }
                        catch { }


                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oSessions = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open));


                            if (oSessions.Count() > 0)
                            {
                                OPERATIONS_OFFSTREET_SESSION_INFO opSessionInfo = oSessions.First().OPERATIONS_OFFSTREET_SESSION_INFOs.OrderByDescending(r => r.OOSI_UTC_DATE).First();

                                bRes = ((opSessionInfo.OOSI_GRP_ID == dGroupId) && opSessionInfo.OOSI_LOGICAL_ID == sLogicalId &&
                                        ((DateTime.UtcNow - opSessionInfo.OOSI_UTC_DATE).TotalSeconds <= iConfirmationTimeout) &&
                                        ((opSessionInfo.OOSI_OPEOFF_TYPE == (int)OffstreetOperationType.Exit) ||
                                         (opSessionInfo.OOSI_OPEOFF_TYPE == (int)OffstreetOperationType.OverduePayment)));

                                if (bRes)
                                {                                    
                                    strPlate = opSessionInfo.OOSI_PLATE;
                                    dtInstDateTime = opSessionInfo.OOSI_INS_DATE;
                                    dtSessionUTCDate = opSessionInfo.OOSI_UTC_DATE;
                                    operationType = (OffstreetOperationType)opSessionInfo.OOSI_OPEOFF_TYPE;
                                    iAmount = opSessionInfo.OOSI_AMOUNT;
                                    iPartialVAT1 = Convert.ToInt32(opSessionInfo.OOSI_PARTIAL_VAT1 ?? 0);
                                    iTime = opSessionInfo.OOSI_TIME;
                                    sTariff = opSessionInfo.OOSI_TARIFF;
                                    dChangeToApply = Convert.ToDouble(opSessionInfo.OOSI_CHANGE_APPLIED);
                                    dtUTCEntryDate = opSessionInfo.OOSI_ENTRY_UTC_DATE;
                                    dtUTCEndDate = opSessionInfo.OOSI_END_UTC_DATE;
                                    dtUTCExitLimitDate = opSessionInfo.OOSI_EXIT_LIMIT_UTC_DATE;
                                    dPercVat1 = opSessionInfo.OOSI_PERC_VAT1 ?? 0;
                                    dPercVat2 = opSessionInfo.OOSI_PERC_VAT2 ?? 0;
                                    dPercFEE = opSessionInfo.OOSI_PERC_FEE ?? 0;
                                    iPercFEETopped = Convert.ToInt32(opSessionInfo.OOSI_PERC_FEE_TOPPED ?? 0);
                                    iFixedFEE = Convert.ToInt32(opSessionInfo.OOSI_FIXED_FEE ?? 0);
                                    sDiscounts = opSessionInfo.OOSI_DISCOUNT_CODES;
                                }
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationOffstreetInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationOffstreetInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CheckSessionOperationOffstreetInfo: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool DeleteSessionOperationOffstreetInfo(ref USER user,
                                                        string strSessionID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            MOBILE_SESSION oSession = oUser.MOBILE_SESSIONs.Where(r => r.MOSE_SESSIONID == strSessionID &&
                                                        r.MOSE_STATUS == Convert.ToInt32(MobileSessionStatus.Open)).First();


                            foreach (OPERATIONS_OFFSTREET_SESSION_INFO opSessionInfo in oSession.OPERATIONS_OFFSTREET_SESSION_INFOs)
                            {
                                dbContext.OPERATIONS_OFFSTREET_SESSION_INFOs.DeleteOnSubmit(opSessionInfo);
                            }

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationOffstreetInfo: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationOffstreetInfo: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteSessionOperationOffstreetInfo: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool ChargeOffstreetOperation(ref USER user,
                                                int iOSType,
                                                bool bSubstractFromBalance,
                                                PaymentSuscryptionType suscriptionType,
                                                OffstreetOperationType operationType,
                                                string strPlate,
                                                decimal dInstallationID,
                                                decimal dGroupID,
                                                string sLogicalId,
                                                string sTariff,
                                                string sGate,
                                                string sSpaceDescription,
                                                DateTime dtEntryDate,
                                                DateTime dtNotifyEntryDate,
                                                DateTime? dtPaymentDate,
                                                DateTime? dtEndDate,
                                                DateTime? dtExitLimitDate,                                                
                                                DateTime dtUTCEntryDate,
                                                DateTime dtUTCNotifyEntryDate,
                                                DateTime? dtUTCPaymentDate,
                                                DateTime? dtUTCEndDate,
                                                DateTime? dtUTCExitLimitDate,                                                
                                                int iTime,
                                                int iQuantity,
                                                decimal dCurID,
                                                decimal dBalanceCurID,
                                                double dChangeApplied,
                                                double dChangeFee,
                                                int iCurrencyChargedQuantity,
                                                decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE,
                                                int iTotalAmount,
                                                decimal? dRechargeId,
                                                bool bMustNotify,
                                                bool bConfirmedInWS1, bool bConfirmedInWS2, bool bConfirmedInWS3,
                                                decimal? dMobileSessionId,
                                                decimal? dLatitude, decimal? dLongitude, string strAppVersion,
                                                string sDiscountCodes,
                                                out decimal dOperationID)
        {
            bool bRes = true;
            dOperationID = -1;
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
                        OPERATIONS_OFFSTREET oOperation = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dPlateID = null;
                            try
                            {
                                var oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1).First();
                                if (oPlate != null)
                                {
                                    dPlateID = oPlate.USRP_ID;
                                }
                            }
                            catch
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "ChargeOffstreetOperation: Plate is not from user or is not enabled: " + strPlate);
                                bRes = false;
                                return bRes;

                            }

                            decimal? dCustomerInvoiceID = null;
                            DateTime dtInvoicePaymentDate;
                            if (dtPaymentDate.HasValue)
                                dtInvoicePaymentDate = dtPaymentDate.Value;
                            else
                                dtInvoicePaymentDate = dtNotifyEntryDate;
                            GetCustomerInvoice(dbContext, dtInvoicePaymentDate, oUser.CUSTOMER.CUS_ID, dCurID,0,iCurrencyChargedQuantity, dInstallationID, out dCustomerInvoiceID);
                            

                            oOperation = new OPERATIONS_OFFSTREET
                            {
                                OPEOFF_TYPE = (int)operationType,
                                OPEOFF_MOSE_OS = iOSType,
                                OPEOFF_USRP_ID = dPlateID.Value,
                                OPEOFF_INS_ID = dInstallationID,
                                OPEOFF_GRP_ID = dGroupID,
                                OPEOFF_LOGICAL_ID = sLogicalId,
                                OPEOFF_TARIFF = sTariff,
                                OPEOFF_GATE = sGate,
                                OPEOFF_SPACE_DESCRIPTION = sSpaceDescription,
                                OPEOFF_ENTRY_DATE = dtEntryDate,
                                OPEOFF_NOTIFY_ENTRY_DATE = dtNotifyEntryDate,
                                OPEOFF_PAYMENT_DATE = dtPaymentDate,
                                OPEOFF_END_DATE = dtEndDate,
                                OPEOFF_EXIT_LIMIT_DATE = dtExitLimitDate,
                                OPEOFF_UTC_ENTRY_DATE = dtUTCEntryDate,
                                OPEOFF_UTC_NOTIFY_ENTRY_DATE = dtUTCNotifyEntryDate,
                                OPEOFF_UTC_PAYMENT_DATE = dtUTCPaymentDate,
                                OPEOFF_UTC_END_DATE = dtUTCEndDate,
                                OPEOFF_UTC_EXIT_LIMIT_DATE = dtUTCExitLimitDate,
                                OPEOFF_ENTRY_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCEntryDate - dtEntryDate).TotalMinutes),
                                OPEOFF_NOTIFY_ENTRY_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCNotifyEntryDate - dtNotifyEntryDate).TotalMinutes),
                                OPEOFF_PAYMENT_DATE_UTC_OFFSET = (dtPaymentDate.HasValue && dtUTCPaymentDate.HasValue ? Convert.ToInt32((dtUTCPaymentDate.Value - dtPaymentDate.Value).TotalMinutes) : 0),
                                OPEOFF_END_DATE_UTC_OFFSET = (dtEndDate.HasValue && dtUTCEndDate.HasValue ? Convert.ToInt32((dtUTCEndDate.Value - dtEndDate.Value).TotalMinutes) : 0),
                                OPEOFF_EXIT_LIMIT_DATE_UTC_OFFSET = (dtExitLimitDate.HasValue && dtUTCExitLimitDate.HasValue ? Convert.ToInt32((dtUTCExitLimitDate.Value - dtExitLimitDate.Value).TotalMinutes) : 0),
                                OPEOFF_AMOUNT = iQuantity,
                                OPEOFF_TIME = iTime,
                                OPEOFF_AMOUNT_CUR_ID = dCurID,
                                OPEOFF_BALANCE_CUR_ID = dBalanceCurID,
                                OPEOFF_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                OPEOFF_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                OPEOFF_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                OPEOFF_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                OPEOFF_CUSPMR_ID = dRechargeId,
                                OPEOFF_BALANCE_BEFORE = oUser.USR_BALANCE,
                                OPEOFF_SUSCRIPTION_TYPE = (int)suscriptionType,
                                OPEOFF_MUST_NOTIFY = (bMustNotify ? 1 : 0),
                                OPEOFF_CONFIRMED_IN_WS1 = (bConfirmedInWS1 ? 1 : 0),
                                OPEOFF_CONFIRMED_IN_WS2 = (bConfirmedInWS2 ? 1 : 0),
                                OPEOFF_CONFIRMED_IN_WS3 = (bConfirmedInWS3 ? 1 : 0),
                                OPEOFF_MOSE_ID = dMobileSessionId,
                                OPEOFF_LATITUDE = dLatitude,
                                OPEOFF_LONGITUDE = dLongitude,
                                OPEOFF_APP_VERSION = strAppVersion,
                                OPEOFF_PERC_VAT1 = Convert.ToDecimal(dPercVat1),
                                OPEOFF_PERC_VAT2 = Convert.ToDecimal(dPercVat2),
                                OPEOFF_PARTIAL_VAT1 = iPartialVat1,
                                OPEOFF_PERC_FEE = Convert.ToDecimal(dPercFEE),
                                OPEOFF_PERC_FEE_TOPPED = iPercFEETopped,
                                OPEOFF_PARTIAL_PERC_FEE = iPartialPercFEE,
                                OPEOFF_FIXED_FEE = iFixedFEE,
                                OPEOFF_PARTIAL_FIXED_FEE = iPartialFixedFEE,                                
                                OPEOFF_TOTAL_AMOUNT = iTotalAmount,
                                OPEOFF_CUSINV_ID = dCustomerInvoiceID,
                                OPEOFF_DISCOUNT_CODES = sDiscountCodes
                            };

                            if (bSubstractFromBalance)
                            {
                                ModifyUserBalance(ref oUser, -iCurrencyChargedQuantity);

                            }

                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = dtUTCNotifyEntryDate;

                            oUser.OPERATIONS_OFFSTREETs.Add(oOperation);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oOperation != null)
                            {
                                dOperationID = oOperation.OPEOFF_ID;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeOffstreetOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeOffstreetOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeOffstreetOperation: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool RefundChargeOffstreetPayment(ref USER user,
                                                 bool bAddToBalance,
                                                 decimal dOperationID)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            var oParkOp = oUser.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationID).First();

                            if (oParkOp != null)
                            {
                                if (bAddToBalance)
                                {
                                    ModifyUserBalance(ref oUser, oParkOp.OPEOFF_FINAL_AMOUNT);

                                }
                                dbContext.OPERATIONS_OFFSTREETs.DeleteOnSubmit(oParkOp);
                            }
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RefundChargeOffstreetPayment: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "RefundChargeOffstreetPayment: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RefundChargeOffstreetPayment: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool UpdateThirdPartyIDInOffstreetOperation(ref USER user,
                                                           int iWSNumber,
                                                           decimal dOperationID,
                                                           string str3rdPartyOpNum)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            OPERATIONS_OFFSTREET oOperation = oUser.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationID).First();

                            switch (iWSNumber)
                            {
                                case 1:
                                    oOperation.OPEOFF_EXTERNAL_ID1 = str3rdPartyOpNum;
                                    break;

                                case 2:
                                    oOperation.OPEOFF_EXTERNAL_ID2 = str3rdPartyOpNum;
                                    break;

                                case 3:
                                    oOperation.OPEOFF_EXTERNAL_ID3 = str3rdPartyOpNum;
                                    break;

                                default:

                                    break;
                            }


                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInOffstreetOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInOffstreetOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyIDInOffstreetOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool UpdateThirdPartyConfirmedInOffstreetOperation(ref USER user,
                                                                  decimal dOperationID,
                                                                  bool bConfirmed1, bool bConfirmed2, bool bConfirmed3)
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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            OPERATIONS_OFFSTREET oOperation = oUser.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationID).First();

                            oOperation.OPEOFF_CONFIRMED_IN_WS1 = bConfirmed1 ? 1 : 0;
                            oOperation.OPEOFF_CONFIRMED_IN_WS2 = bConfirmed2 ? 1 : 0;
                            oOperation.OPEOFF_CONFIRMED_IN_WS3 = bConfirmed3 ? 1 : 0;
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInOffstreetOperation: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInOffstreetOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateThirdPartyConfirmedInOffstreetOperation: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool GetOperationOffstreetData(ref USER user,
                                              decimal dOperationID,
                                              out OPERATIONS_OFFSTREET oParkOp)
        {
            bool bRes = false;
            oParkOp = null;

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
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oParkOp = oUser.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationID).First();

                            if (oParkOp != null)
                            {
                                bRes = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetOperationOffstreetData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationOffstreetData: ", e);
                bRes = false;
            }
            return bRes;
        }

        public bool GetLastOperationOffstreetData(decimal dGroupId, string sLogicalId, out OPERATIONS_OFFSTREET oParkOp)
        {
            bool bRes = false;
            oParkOp = null;

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

                        var operations = dbContext.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_GRP_ID == dGroupId && r.OPEOFF_LOGICAL_ID == sLogicalId).OrderByDescending(r => (r.OPEOFF_TYPE == (int) OffstreetOperationType.Entry ? r.OPEOFF_ENTRY_DATE : r.OPEOFF_END_DATE.Value));

                        if (operations.Count() > 0)
                        {
                            oParkOp = operations.First();
                        }

                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetLastOperationOffstreetData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetLastOperationOffstreetData: ", e);
                bRes = false;
            }
            return bRes;
        }

        public bool UpdateOperationOffstreetExitData(decimal dOperationId, DateTime dtExitDate, DateTime dtUTCExitDate, bool bMustNotify, out OPERATIONS_OFFSTREET oParkOp)
        {
            bool bRes = false;
            oParkOp = null;

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

                        var operations = dbContext.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationId);
                        if (operations.Count() > 0)
                        {
                            oParkOp = operations.First();

                            oParkOp.OPEOFF_EXIT_DATE = dtExitDate;
                            oParkOp.OPEOFF_UTC_EXIT_DATE = dtUTCExitDate;
                            oParkOp.OPEOFF_EXIT_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCExitDate - dtExitDate).TotalMinutes);
                            oParkOp.OPEOFF_MUST_NOTIFY = (bMustNotify ? 1 : 0);

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();
                                bRes = true;                                     
                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetExitData: ", e);
                                bRes = false;
                            }

                        }
                        else
                            bRes = false;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetExitData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetExitData: ", e);
                bRes = false;
            }
            return bRes;
        }

        public bool UpdateOperationOffstreetSpaceData(decimal dOperationId, string sSpaceDesc, out OPERATIONS_OFFSTREET oParkOp)
        {
            bool bRes = false;
            oParkOp = null;

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

                        var operations = dbContext.OPERATIONS_OFFSTREETs.Where(r => r.OPEOFF_ID == dOperationId);
                        if (operations.Count() > 0)
                        {
                            oParkOp = operations.First();

                            oParkOp.OPEOFF_SPACE_DESCRIPTION = sSpaceDesc;

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();
                                bRes = true;
                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetSpaceData: ", e);
                                bRes = false;
                            }

                        }
                        else
                            bRes = false;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetSpaceData: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateOperationOffstreetSpaceData: ", e);
                bRes = false;
            }
            return bRes;
        }

        private GROUPS_OFFSTREET_WS_CONFIGURATION GetGroupOffstreetWsConfiguration(decimal dGroupId, integraMobileDBEntitiesDataContext dbContext)
        {
            GROUPS_OFFSTREET_WS_CONFIGURATION oGroupWsConfiguration = null;

            var oGroups = (from r in dbContext.GROUPs
                           where r.GRP_ID == dGroupId && r.GRP_TYPE == (int)GroupType.OffStreet
                           select r).ToArray();
            if (oGroups.Count() == 1)
            {
                var oGroup = oGroups[0];
                var oConfigurations = (from r in dbContext.GROUPS_OFFSTREET_WS_CONFIGURATIONs
                                       where r.GOWC_GRP_ID == oGroup.GRP_ID
                                       select r).ToArray();
                if (oConfigurations.Count() >= 1)
                {
                    oGroupWsConfiguration = oConfigurations[0];
                }
                else
                {
                    if (oGroup.GROUPS_HIERARCHies != null && oGroup.GROUPS_HIERARCHies.Count > 0 && oGroup.GROUPS_HIERARCHies[0].GRHI_GPR_ID_PARENT.HasValue)
                        oGroupWsConfiguration = GetGroupOffstreetWsConfiguration(oGroup.GROUPS_HIERARCHies[0].GRHI_GPR_ID_PARENT.Value, dbContext);
                }
            }

            return oGroupWsConfiguration;
        }

        public int HealthCheck()
        {
            int iRes = -1;
            integraMobileDBEntitiesDataContext dbContext = null;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                               new TransactionOptions()
                                                                                               {
                                                                                                   IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                   Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                               }))
                {
                    dbContext = new integraMobileDBEntitiesDataContext();

                    decimal dMinOperator = (from t in dbContext.OPERATORs
                                            select t.OPR_ID).Min();

                    iRes = Convert.ToInt32(dMinOperator);
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetMinOperatorId: ", e);
                iRes = -1;
            }
            finally
            {
                if (dbContext != null)
                {
                    dbContext.Close();
                    dbContext = null;
                }
            }


            return iRes;
        }

        public IEnumerable<OPERATIONS_OFFSTREET> GetUserPlateLastOperationOffstreet(ref USER user, out int iNumRows)
        {
            List<OPERATIONS_OFFSTREET> res = new List<OPERATIONS_OFFSTREET>();
            iNumRows = 0;
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

                    decimal userId = user.USR_ID;


                    foreach (USER_PLATE plate in user.USER_PLATEs.OrderBy(t => t.USRP_PLATE))
                    {
                        if (plate.USRP_ENABLED == 1)
                        {
                            try
                            {
                                var resplate = (from r in dbContext.OPERATIONS_OFFSTREETs
                                                where ((r.OPEOFF_USR_ID == userId) && r.USER.USR_ENABLED == 1 &&
                                                (r.OPEOFF_USRP_ID == plate.USRP_ID))
                                                orderby r.OPEOFF_INSERTION_UTC_DATE /*(r.OPEOFF_TYPE == (int)OffstreetOperationType.Entry ? r.OPEOFF_NOTIFY_ENTRY_DATE : r.OPEOFF_PAYMENT_DATE)*/ descending
                                                select r).First();
                                res.Add(resplate);
                                iNumRows++;
                            }
                            catch
                            {

                            }

                        }
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserPlateLastOperationOffstreet: ", e);
            }

            return (IEnumerable<OPERATIONS_OFFSTREET>)res;
        }

        public OPERATOR GetDefaultOperator()
        {
            OPERATOR oRet = null;
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
                    oRet = GetDefaultOperator(dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDefaultOperator: ", e);
            }

            return oRet;
        }
        public OPERATOR GetDefaultOperator(integraMobileDBEntitiesDataContext dbContext)
        {
            OPERATOR oRet = null;

            try
            {
                var oOperator = (from o in dbContext.OPERATORs
                                 where o.OPR_DEFAULT == 1
                                 select o).FirstOrDefault();
                if (oOperator == default(OPERATOR)) oOperator = dbContext.OPERATORs.FirstOrDefault();
                oRet = oOperator;
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDefaultOperator: ", e);
            }

            return oRet;
        }

        public bool GetFinantialParams(USER user, decimal dInsId, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                    where r.USR_ID == userId && r.USR_ENABLED == 1
                                    select r).First();

                    if (oUser != null)
                    {


                        INSTALLATION oInstallation = dbContext.INSTALLATIONs.Where(i => i.INS_ID == dInsId).FirstOrDefault();
                        if (oInstallation != default(INSTALLATION))
                        {
                            // ERROR *** Incorrect installation id parameter
                        }

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, oInstallation, oInstallation.INS_TIMEZONE_ID, null, null, oSuscriptionType, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, false);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParams(USER user, TOLL oToll, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        INSTALLATION oInstallation = dbContext.INSTALLATIONs.Where(i => i.INS_ID == oToll.TOL_INS_ID).FirstOrDefault();

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, oInstallation, oInstallation.INS_TIMEZONE_ID, oToll, null, oSuscriptionType, iPaymentTypeId, iPaymentSubtypeId, ChargeOperationsType.TollPayment,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, false);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParams(USER user, string sTimezone, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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
                    
                    decimal userId = user.USR_ID;
                    
                    var oUser = (from r in dbContext.USERs
                                where r.USR_ID == userId && r.USR_ENABLED == 1
                                select r).First();

                    if (oUser != null)
                    {

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, null, sTimezone, null, null, null, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, false);
                    }
                }
                
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParams(string sCurrencyIsoCode, string sTimezone, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    bRet = GetFinantialParams(sCurrencyIsoCode, null, sTimezone, null, null, null, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                              out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, false);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParams(USER user, GROUP oGroup, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    decimal userId = user.USR_ID;

                    var oUser = (from r in dbContext.USERs
                                 where r.USR_ID == userId && r.USR_ENABLED == 1
                                 select r).First();

                    if (oUser != null)
                    {
                        INSTALLATION oInstallation = dbContext.INSTALLATIONs.Where(i => i.INS_ID == oGroup.GRP_INS_ID).FirstOrDefault();

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, oInstallation, oInstallation.INS_TIMEZONE_ID, null, oGroup, oSuscriptionType, iPaymentTypeId, iPaymentSubtypeId, ChargeOperationsType.OffstreetExit,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, false);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }


        public bool GetFinantialParamsPaymentType(USER user, decimal dInsId, PaymentSuscryptionType oSuscriptionType, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    decimal userId = user.USR_ID;
                    
                    var oUser = (from r in dbContext.USERs
                                where r.USR_ID == userId && r.USR_ENABLED == 1
                                select r).First();

                    if (oUser != null)
                    {

                        INSTALLATION oInstallation = dbContext.INSTALLATIONs.Where(i => i.INS_ID == dInsId).FirstOrDefault();
                        if (oInstallation != default(INSTALLATION))
                        {
                            // ERROR *** Incorrect installation id parameter
                        }

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, oInstallation, oInstallation.INS_TIMEZONE_ID, null, null, oSuscriptionType, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, true);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParamsPaymentType: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParamsPaymentType(USER user, string sTimezone, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    decimal userId = user.USR_ID;
                    
                    var oUser = (from r in dbContext.USERs
                                where r.USR_ID == userId && r.USR_ENABLED == 1
                                select r).First();

                    if (oUser != null)
                    {

                        bRet = GetFinantialParams(oUser.CURRENCy.CUR_ISO_CODE, null, sTimezone, null, null, null, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                                  out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, true);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParamsPaymentType: ", e);
            }

            return bRet;
        }
        public bool GetFinantialParamsPaymentType(string sCurrencyIsoCode, string sTimezone, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

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

                    bRet = GetFinantialParams(sCurrencyIsoCode, null, sTimezone, null, null, null, iPaymentTypeId, iPaymentSubtypeId, oOpeType,
                                              out dVAT1, out dVAT2, out dPercFEE, out iPercFEETopped, out iFixedFEE, dbContext, true);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParamsPaymentType: ", e);
            }

            return bRet;
        }
        private bool GetFinantialParams(string sCurrencyIsoCode, INSTALLATION oInstallation, string sTimezone, TOLL oToll, GROUP oGroup, PaymentSuscryptionType? oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE, integraMobileDBEntitiesDataContext dbContext, bool bPaymentTypeStrict)
        {
            bool bRet = false;
            dVAT1 = 0;
            dVAT2 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iFixedFEE = 0;

            try
            {

                DateTime dtLocalInstTime = DateTime.Now;
                if (string.IsNullOrEmpty(sTimezone) && oInstallation != null) sTimezone = oInstallation.INS_TIMEZONE_ID;
                if (!string.IsNullOrEmpty(sTimezone))
                {
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(sTimezone);
                    dtLocalInstTime = TimeZoneInfo.ConvertTime(dtLocalInstTime, TimeZoneInfo.Local, tzi);
                }

                if (oOpeType == ChargeOperationsType.ParkingOperation ||
                    oOpeType == ChargeOperationsType.ExtensionOperation ||
                    oOpeType == ChargeOperationsType.ParkingRefund ||
                    oOpeType == ChargeOperationsType.TicketPayment)
                {
                    dVAT1 = oInstallation.INS_PERC_VAT1;
                    dVAT2 = oInstallation.INS_PERC_VAT2;

                    IQueryable<INSTALLATIONS_FINAN_PARS_OPE_TYPE> oFinanPars = null;
                    if (!bPaymentTypeStrict)
                    {
                        oFinanPars = (from p in dbContext.INSTALLATIONS_FINAN_PARS_OPE_TYPEs
                                      where p.INSF_INS_ID == oInstallation.INS_ID && p.INSF_OPE_TYPE == (int)oOpeType &&
                                            p.INSF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            ((!p.INSF_PAT_ID.HasValue && !p.INSF_PAST_ID.HasValue) ||
                                             ((p.INSF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && (p.INSF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0)) /*||
                                             ((p.INSF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && !p.INSF_PAST_ID.HasValue) ||
                                             (!p.INSF_PAT_ID.HasValue && (p.INSF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0))*/) &&
                                            p.INSF_INI_APPLY_DATE <= dtLocalInstTime && p.INSF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.INSF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    else
                    {
                        oFinanPars = (from p in dbContext.INSTALLATIONS_FINAN_PARS_OPE_TYPEs
                                      where p.INSF_INS_ID == oInstallation.INS_ID && p.INSF_OPE_TYPE == (int)oOpeType &&
                                            p.INSF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            (
                                             (p.INSF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && 
                                             ((p.INSF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0) || !iPaymentSubtypeId.HasValue)
                                            ) &&
                                            p.INSF_INI_APPLY_DATE <= dtLocalInstTime && p.INSF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.INSF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    if (oFinanPars.Count() > 0)
                    {
                        foreach (var oParams in oFinanPars)
                        {
                            if (oParams.INSF_IS_TAX == 1) dVAT1 = 0;
                            dPercFEE += (oParams.INSF_PERC_FEE_APPLY == 1 ? (oParams.INSF_PERC_FEE ?? 0) : 0);
                            iPercFEETopped += Convert.ToInt32((oParams.INSF_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.INSF_PERC_FEE_TOPPED ?? 0) : 0));
                            iFixedFEE += Convert.ToInt32((oParams.INSF_FIXED_FEE_APPLY == 1 ? (oParams.INSF_FIXED_FEE ?? 0) : 0));
                        }
                    }
                    else
                    {
                        dVAT1 = 0;
                        dVAT2 = 0;
                    }

                    bRet = true;
                }
                else if (oOpeType == ChargeOperationsType.BalanceRecharge ||
                         oOpeType == ChargeOperationsType.BalanceRechargeRefund ||
                         oOpeType == ChargeOperationsType.CouponCharge)
                {
                    OPERATOR oOperator = GetDefaultOperator(dbContext);
                    if (oOperator != null)
                    {
                        
                        var oFinanPars = (from p in dbContext.OPERATORS_FINAN_PARs
                                          where p.OPRF_OPR_ID == oOperator.OPR_ID &&
                                                p.OPRF_INI_APPLY_DATE <= dtLocalInstTime && p.OPRF_END_APPLY_DATE >= dtLocalInstTime &&
                                                p.CURRENCy.CUR_ISO_CODE == sCurrencyIsoCode
                                          orderby p.OPRF_INI_APPLY_DATE
                                          select p).FirstOrDefault();
                        if (oFinanPars != default(OPERATORS_FINAN_PAR))
                        {
                            dVAT1 = oFinanPars.OPRF_PERC_VAT3;
                        }
                        dVAT2 = dVAT1;

                        int iPercFEEToppedPar = 0;
                        int iFixedFEEPar = 0;

                        IQueryable<OPERATORS_FINAN_PARS_PAT_TYPE> oFinanParsPatTypes = null;
                        if (!bPaymentTypeStrict)
                        {
                            oFinanParsPatTypes = (from p in dbContext.OPERATORS_FINAN_PARS_PAT_TYPEs
                                                  where p.OPRFP_OPR_ID == oOperator.OPR_ID &&
                                                        p.CURRENCy.CUR_ISO_CODE == sCurrencyIsoCode &&
                                                        ((!p.OPRFP_PAT_ID.HasValue && !p.OPRFP_PAST_ID.HasValue) ||
                                                         ((p.OPRFP_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && (p.OPRFP_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0)) /*||
                                                         ((p.OPRFP_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && !p.OPRFP_PAST_ID.HasValue) ||
                                                         (!p.OPRFP_PAT_ID.HasValue && (p.OPRFP_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0))*/) &&
                                                        p.OPRFP_INI_APPLY_DATE <= dtLocalInstTime && p.OPRFP_END_APPLY_DATE >= dtLocalInstTime
                                                  orderby p.OPRFP_INI_APPLY_DATE
                                                  select p).AsQueryable();
                        }
                        else
                        {
                            oFinanParsPatTypes = (from p in dbContext.OPERATORS_FINAN_PARS_PAT_TYPEs
                                                  where p.OPRFP_OPR_ID == oOperator.OPR_ID &&
                                                        p.CURRENCy.CUR_ISO_CODE == sCurrencyIsoCode &&
                                                        (
                                                         (p.OPRFP_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && 
                                                         ((p.OPRFP_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0) || !iPaymentSubtypeId.HasValue)
                                                        ) &&
                                                        p.OPRFP_INI_APPLY_DATE <= dtLocalInstTime && p.OPRFP_END_APPLY_DATE >= dtLocalInstTime
                                                  orderby p.OPRFP_INI_APPLY_DATE
                                                  select p).AsQueryable();

                        }
                        if (oFinanParsPatTypes.Count() > 0)
                        {
                            foreach (var oParams in oFinanParsPatTypes)
                            {
                                switch (oOpeType)
                                {
                                    case ChargeOperationsType.BalanceRecharge:
                                        if (oParams.OPRFP_RECH_IS_TAX == 1) dVAT1 = 0;
                                        dPercFEE += (oParams.OPRFP_RECH_PERC_FEE_APPLY == 1 ? (oParams.OPRFP_RECH_PERC_FEE ?? 0) : 0);
                                        iPercFEEToppedPar = Convert.ToInt32((oParams.OPRFP_RECH_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.OPRFP_RECH_PERC_FEE_TOPPED ?? 0) : 0));
                                        iFixedFEEPar = Convert.ToInt32((oParams.OPRFP_RECH_FIXED_FEE_APPLY == 1 ? (oParams.OPRFP_RECH_FIXED_FEE ?? 0) : 0));
                                        break;

                                    case ChargeOperationsType.BalanceRechargeRefund:
                                        if (oParams.OPRFP_RECH_REFUND_IS_TAX == 1) dVAT1 = 0;
                                        dPercFEE += (oParams.OPRFP_RECH_REFUND_PERC_FEE_APPLY == 1 ? (oParams.OPRFP_RECH_REFUND_PERC_FEE ?? 0) : 0);
                                        iPercFEEToppedPar = Convert.ToInt32((oParams.OPRFP_RECH_REFUND_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.OPRFP_RECH_REFUND_PERC_FEE_TOPPED ?? 0) : 0));
                                        iFixedFEEPar = Convert.ToInt32((oParams.OPRFP_RECH_REFUND_FIXED_FEE_APPLY == 1 ? (oParams.OPRFP_RECH_REFUND_FIXED_FEE ?? 0) : 0));
                                        break;

                                    case ChargeOperationsType.CouponCharge:
                                        if (oParams.OPRFP_RCOUP_IS_TAX == 1) dVAT1 = 0;
                                        dPercFEE += (oParams.OPRFP_RCOUP_PERC_FEE_APPLY == 1 ? (oParams.OPRFP_RCOUP_PERC_FEE ?? 0) : 0);
                                        iPercFEEToppedPar = Convert.ToInt32((oParams.OPRFP_RCOUP_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.OPRFP_RCOUP_PERC_FEE_TOPPED ?? 0) : 0));
                                        iFixedFEEPar = Convert.ToInt32((oParams.OPRFP_RCOUP_FIXED_FEE_APPLY == 1 ? (oParams.OPRFP_RCOUP_FIXED_FEE ?? 0) : 0));
                                        break;
                                }

                                if (sCurrencyIsoCode != oParams.CURRENCy.CUR_ISO_CODE &&
                                    (iPercFEEToppedPar != 0 || iFixedFEEPar != 0))
                                {
                                    double dChangeToApply = integraMobile.Infrastructure.CCurrencyConvertor.GetChangeToApply(oParams.CURRENCy.CUR_ISO_CODE, sCurrencyIsoCode);

                                    double dConvertedValue = Convert.ToDouble(iPercFEEToppedPar) * dChangeToApply;
                                    dConvertedValue = Math.Round(dConvertedValue, 4);
                                    double dChangeFee = Convert.ToDouble(this.GetChangeFeePerc(dbContext)) * dConvertedValue / 100;
                                    iPercFEEToppedPar = Convert.ToInt32(Math.Round(dConvertedValue - dChangeFee, MidpointRounding.AwayFromZero));

                                    dConvertedValue = Convert.ToDouble(iFixedFEEPar) * dChangeToApply;
                                    dConvertedValue = Math.Round(dConvertedValue, 4);
                                    dChangeFee = Convert.ToDouble(this.GetChangeFeePerc(dbContext)) * dConvertedValue / 100;
                                    iFixedFEEPar = Convert.ToInt32(Math.Round(dConvertedValue - dChangeFee, MidpointRounding.AwayFromZero));
                                }

                                iPercFEETopped += iPercFEEToppedPar;
                                iFixedFEE += iFixedFEEPar;
                            }
                        }
                        else
                        {
                            dVAT1 = 0;
                            dVAT2 = 0;
                        }
                        bRet = true;
                    }
                    else
                    {
                        // Invalid operator
                        m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams::Invalid default operator.");
                    }

                }
                else if (oOpeType == ChargeOperationsType.SubscriptionCharge)
                {
                    string sCurIsoCode = "";

                    OPERATOR oOperator = GetDefaultOperator(dbContext);
                    if (oOperator != null)
                    {
                        var oFinanPars = (from p in dbContext.OPERATORS_FINAN_PARs
                                          where p.OPRF_OPR_ID == oOperator.OPR_ID &&
                                                p.CURRENCy.CUR_ISO_CODE == sCurrencyIsoCode &&
                                                p.OPRF_INI_APPLY_DATE <= dtLocalInstTime && p.OPRF_END_APPLY_DATE >= dtLocalInstTime
                                          orderby p.OPRF_INI_APPLY_DATE
                                          select p).FirstOrDefault();
                        if (oFinanPars != default(OPERATORS_FINAN_PAR))
                        {
                            dVAT1 = oFinanPars.OPRF_PERC_VAT3;
                        }
                        dVAT2 = dVAT1;

                        var oMonthSubsPars = (from p in dbContext.MONTH_SUBS_FINAN_PARs
                                          where p.MSF_INI_APPLY_DATE <= dtLocalInstTime && p.MSF_END_APPLY_DATE >= dtLocalInstTime 
                                          orderby p.MSF_INI_APPLY_DATE
                                          select p).FirstOrDefault();
                        if (oMonthSubsPars != default(MONTH_SUBS_FINAN_PAR))
                        {
                            sCurIsoCode = oMonthSubsPars.CURRENCy.CUR_ISO_CODE;
                        }

                        int iPercFEEToppedPar = 0;
                        int iFixedFEEPar = 0;

                        var oMonthSubsFinanParsPatTypes = (from p in dbContext.MONTH_SUBS_FINAN_PARS_PAT_TYPEs
                                                           where p.MSFP_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                                                 ((!p.MSFP_PAT_ID.HasValue && !p.MSFP_PAST_ID.HasValue) ||
                                                                  ((p.MSFP_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && (p.MSFP_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0)) /*||
                                                                  ((p.MSFP_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && !p.MSFP_PAST_ID.HasValue) ||
                                                                  (!p.MSFP_PAT_ID.HasValue && (p.MSFP_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0))*/) &&
                                                                 p.MSFP_INI_APPLY_DATE <= dtLocalInstTime && p.MSFP_END_APPLY_DATE >= dtLocalInstTime
                                                           orderby p.MSFP_INI_APPLY_DATE
                                                           select p).AsQueryable();
                        if (oMonthSubsFinanParsPatTypes.Count() > 0)
                        {

                            foreach (var oParams in oMonthSubsFinanParsPatTypes)
                            {
                                if (oParams.MSFP_IS_TAX == 1) dVAT1 = 0;
                                dPercFEE += (oParams.MSFP_PERC_FEE_APPLY == 1 ? (oParams.MSFP_PERC_FEE ?? 0) : 0);
                                iPercFEEToppedPar = Convert.ToInt32((oParams.MSFP_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.MSFP_PERC_FEE_TOPPED ?? 0) : 0));
                                iFixedFEEPar = Convert.ToInt32((oParams.MSFP_FIXED_FEE_APPLY == 1 ? (oParams.MSFP_FIXED_FEE ?? 0) : 0));

                                if (!string.IsNullOrEmpty(sCurIsoCode) && sCurIsoCode != sCurrencyIsoCode)
                                {
                                    double dChangeToApply = integraMobile.Infrastructure.CCurrencyConvertor.GetChangeToApply(sCurIsoCode, sCurrencyIsoCode);

                                    double dConvertedValue = Convert.ToDouble(iPercFEEToppedPar) * dChangeToApply;
                                    dConvertedValue = Math.Round(dConvertedValue, 4);
                                    double dChangeFee = Convert.ToDouble(this.GetChangeFeePerc(dbContext)) * dConvertedValue / 100;
                                    iPercFEEToppedPar = Convert.ToInt32(Math.Round(dConvertedValue - dChangeFee, MidpointRounding.AwayFromZero));

                                    dConvertedValue = Convert.ToDouble(iFixedFEEPar) * dChangeToApply;
                                    dConvertedValue = Math.Round(dConvertedValue, 4);
                                    dChangeFee = Convert.ToDouble(this.GetChangeFeePerc(dbContext)) * dConvertedValue / 100;
                                    iFixedFEEPar = Convert.ToInt32(Math.Round(dConvertedValue - dChangeFee, MidpointRounding.AwayFromZero));
                                }

                                iPercFEETopped += iPercFEEToppedPar;
                                iFixedFEE += iFixedFEEPar;
                            }
                        }
                        else
                        {
                            dVAT1 = 0;
                            dVAT2 = 0;
                        }
                        bRet = true;
                    }
                    else
                    {
                        // Invalid operator
                        m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams::Invalid default operator.");
                    }
                }
                else if (oOpeType == ChargeOperationsType.TollPayment)
                {
                    dVAT1 = oInstallation.INS_PERC_VAT1;
                    dVAT2 = oInstallation.INS_PERC_VAT2;

                    IQueryable<TOLLS_FINAN_PAR> oFinanPars = null;
                    if (!bPaymentTypeStrict)
                    {
                        oFinanPars = (from p in dbContext.TOLLS_FINAN_PARs
                                      where oToll != null && p.TOLF_TOL_ID == oToll.TOL_ID && 
                                            p.TOLF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            ((!p.TOLF_PAT_ID.HasValue && !p.TOLF_PAST_ID.HasValue) ||
                                             ((p.TOLF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && (p.TOLF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0)) ||
                                             ((p.TOLF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && !p.TOLF_PAST_ID.HasValue) ||
                                             (!p.TOLF_PAT_ID.HasValue && (p.TOLF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0))) &&
                                            p.TOLF_INI_APPLY_DATE <= dtLocalInstTime && p.TOLF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.TOLF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    else
                    {
                        oFinanPars = (from p in dbContext.TOLLS_FINAN_PARs
                                      where oToll != null && p.TOLF_TOL_ID == oToll.TOL_ID && 
                                            p.TOLF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            (
                                             (p.TOLF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) &&
                                             ((p.TOLF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0) || !iPaymentSubtypeId.HasValue)
                                            ) &&
                                            p.TOLF_INI_APPLY_DATE <= dtLocalInstTime && p.TOLF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.TOLF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    if (oFinanPars.Count() > 0)
                    {
                        foreach (var oParams in oFinanPars)
                        {
                            if (oParams.TOLF_IS_TAX == 1) dVAT1 = 0;
                            dPercFEE += (oParams.TOLF_PERC_FEE_APPLY == 1 ? (oParams.TOLF_PERC_FEE ?? 0) : 0);
                            iPercFEETopped += Convert.ToInt32((oParams.TOLF_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.TOLF_PERC_FEE_TOPPED ?? 0) : 0));
                            iFixedFEE += Convert.ToInt32((oParams.TOLF_FIXED_FEE_APPLY == 1 ? (oParams.TOLF_FIXED_FEE ?? 0) : 0));
                        }
                    }
                    else
                    {
                        dVAT1 = 0;
                        dVAT2 = 0;
                    }

                    bRet = true;
                }
                else if (oOpeType == ChargeOperationsType.OffstreetExit)
                {
                    dVAT1 = oInstallation.INS_PERC_VAT1;
                    dVAT2 = oInstallation.INS_PERC_VAT2;

                    IQueryable<GROUPS_OFFSTREET_FINAN_PAR> oFinanPars = null;
                    if (!bPaymentTypeStrict)
                    {
                        oFinanPars = (from p in dbContext.GROUPS_OFFSTREET_FINAN_PARs
                                      where oGroup != null && p.GOF_GRP_ID == oGroup.GRP_ID &&
                                            p.GOF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            ((!p.GOF_PAT_ID.HasValue && !p.GOF_PAST_ID.HasValue) ||
                                             ((p.GOF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && (p.GOF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0)) ||
                                             ((p.GOF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) && !p.GOF_PAST_ID.HasValue) ||
                                             (!p.GOF_PAT_ID.HasValue && (p.GOF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0))) &&
                                            p.GOF_INI_APPLY_DATE <= dtLocalInstTime && p.GOF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.GOF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    else
                    {
                        oFinanPars = (from p in dbContext.GROUPS_OFFSTREET_FINAN_PARs
                                      where oGroup != null && p.GOF_GRP_ID == oGroup.GRP_ID &&
                                            p.GOF_SUSCRIPTION_TYPE == (int)oSuscriptionType.Value &&
                                            (
                                             (p.GOF_PAT_ID ?? 0) == (iPaymentTypeId ?? 0) &&
                                             ((p.GOF_PAST_ID ?? 0) == (iPaymentSubtypeId ?? 0) || !iPaymentSubtypeId.HasValue)
                                            ) &&
                                            p.GOF_INI_APPLY_DATE <= dtLocalInstTime && p.GOF_END_APPLY_DATE >= dtLocalInstTime
                                      orderby p.GOF_INI_APPLY_DATE
                                      select p).AsQueryable();
                    }
                    if (oFinanPars.Count() > 0)
                    {
                        foreach (var oParams in oFinanPars)
                        {
                            if (oParams.GOF_IS_TAX == 1) dVAT1 = 0;
                            dPercFEE += (oParams.GOF_PERC_FEE_APPLY == 1 ? (oParams.GOF_PERC_FEE ?? 0) : 0);
                            iPercFEETopped += Convert.ToInt32((oParams.GOF_PERC_FEE_TOPPED_APPLY == 1 ? (oParams.GOF_PERC_FEE_TOPPED ?? 0) : 0));
                            iFixedFEE += Convert.ToInt32((oParams.GOF_FIXED_FEE_APPLY == 1 ? (oParams.GOF_FIXED_FEE ?? 0) : 0));
                        }
                    }
                    else
                    {
                        dVAT1 = 0;
                        dVAT2 = 0;
                    }

                    bRet = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinantialParams: ", e);
            }

            return bRet;
        }

        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, 
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, 
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT)
        {            
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;            
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;            
            int iPartialBonusFEE = 0;
            int iPartialBonusFEEVAT = 0;

            return CalculateFEE(iAmount, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, 0,
                                out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE,
                                out iPartialPercFEEVAT, out iPartialFixedFEEVAT, out iPartialBonusFEEVAT);
        }
        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT)
        {            
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;
            int iPartialBonusFEE = 0;
            int iPartialBonusFEEVAT = 0;

            return CalculateFEE(iAmount, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, 0,
                                iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE,
                                out iPartialPercFEEVAT, out iPartialFixedFEEVAT, out iPartialBonusFEEVAT);
        }
        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT)
        {
            int iTotalAmount = 0;
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialBonusFEE = 0;
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;
            iPartialBonusFEEVAT = 0;

            try
            {
                iPartialVAT1 = Convert.ToInt32(Math.Round(iAmount * dVAT1, MidpointRounding.AwayFromZero));
                int iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped) iPercFEE = iPercFEETopped;
                iPartialPercFEE = Convert.ToInt32(Math.Round(iPercFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));
                iPartialFixedFEE = Convert.ToInt32(Math.Round(iFixedFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                int iBonusFEE = Convert.ToInt32(Math.Round((iPercFEE + iFixedFEE) * dPercBonus, MidpointRounding.AwayFromZero));
                iPartialBonusFEE = Convert.ToInt32(Math.Round(iBonusFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                iTotalAmount = iAmount + iPartialVAT1 + iPartialPercFEE + iPartialFixedFEE - iPartialBonusFEE;

                iPartialPercFEEVAT = Convert.ToInt32(Math.Round(iPercFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialFixedFEEVAT = Convert.ToInt32(Math.Round(iFixedFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialBonusFEEVAT = Convert.ToInt32(Math.Round(iBonusFEE * dVAT2, MidpointRounding.AwayFromZero));
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEE: ", e);
            }

            return iTotalAmount;
        }
        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT)
        {
            int iTotalAmount = 0;            
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialBonusFEE = 0;
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;
            iPartialBonusFEEVAT = 0;

            try
            {
                //iPartialVAT1 = Convert.ToInt32(Math.Round(iAmount * dVAT1, MidpointRounding.AwayFromZero));
                int iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped) iPercFEE = iPercFEETopped;
                iPartialPercFEE = Convert.ToInt32(Math.Round(iPercFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));
                iPartialFixedFEE = Convert.ToInt32(Math.Round(iFixedFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                int iBonusFEE = Convert.ToInt32(Math.Round((iPercFEE + iFixedFEE) * dPercBonus, MidpointRounding.AwayFromZero));
                iPartialBonusFEE = Convert.ToInt32(Math.Round(iBonusFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                iTotalAmount = iAmount + iPartialVAT1 + iPartialPercFEE + iPartialFixedFEE - iPartialBonusFEE;

                iPartialPercFEEVAT = Convert.ToInt32(Math.Round(iPercFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialFixedFEEVAT = Convert.ToInt32(Math.Round(iFixedFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialBonusFEEVAT = Convert.ToInt32(Math.Round(iBonusFEE * dVAT2, MidpointRounding.AwayFromZero));
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEE: ", e);
            }

            return iTotalAmount;
        }
        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE)
        {            
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            int iPartialBonusFEE = 0;

            return CalculateFEE(iAmount, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, 0,
                                out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE);
        }
        public int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE)
        {
            int iTotalAmount = 0;
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialBonusFEE = 0;

            try
            {
                iPartialVAT1 = Convert.ToInt32(Math.Round(iAmount * dVAT1, MidpointRounding.AwayFromZero));
                int iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped) iPercFEE = iPercFEETopped;
                iPartialPercFEE = Convert.ToInt32(Math.Round(iPercFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));
                iPartialFixedFEE = Convert.ToInt32(Math.Round(iFixedFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                int iBonusFEE = Convert.ToInt32(Math.Round((iPercFEE + iFixedFEE) * dPercBonus, MidpointRounding.AwayFromZero));
                iPartialBonusFEE = Convert.ToInt32(Math.Round(iBonusFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                iTotalAmount = iAmount + iPartialVAT1 + iPartialPercFEE + iPartialFixedFEE - iPartialBonusFEE;

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEE: ", e);
            }

            return iTotalAmount;
        }

        public int CalculateFEE(decimal dInsId, ChargeOperationsType oOpeType, int iAmount,
                                    out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE)
        {
            int iFinalAmount = 0;
            dVAT1 = 0;
            dVAT2 = 0;
            iPartialVAT1 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iPartialPercFEE = 0;
            iFixedFEE = 0;
            iPartialFixedFEE = 0;

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
                    iFinalAmount = CalculateFEE(dInsId, oOpeType, iAmount,
                                                out dVAT1, out dVAT2, out iPartialVAT1, out dPercFEE, out iPercFEETopped, out iPartialPercFEE, out iFixedFEE, out iPartialFixedFEE, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEE: ", e);
            }

            return iFinalAmount;
        }
        public int CalculateFEE(decimal dInsId, ChargeOperationsType oOpeType, int iAmount,
                                    out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE, 
                                    integraMobileDBEntitiesDataContext dbContext)
        {
            int iFinalAmount = 0;
            dVAT1 = 0;
            dVAT2 = 0;
            iPartialVAT1 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iPartialPercFEE = 0;
            iFixedFEE = 0;
            iPartialFixedFEE = 0;

            try
            {
                var oInstallation = (from r in dbContext.INSTALLATIONs
                                     where r.INS_ID == dInsId
                                     select r).FirstOrDefault();
                if (oInstallation != default(INSTALLATION))
                {

                    // ...
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEE: ", e);
            }

            return iFinalAmount;
        }

        public int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT)
        {
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;
            int iPartialBonusFEE = 0;
            int iPartialBonusFEEVAT = 0;

            return CalculateFEEReverse(iTotalAmount, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, 0,
                                out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE,
                                out iPartialPercFEEVAT, out iPartialFixedFEEVAT, out iPartialBonusFEEVAT);
        }
        public int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT)
        {
            int iAmount = 0;
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialBonusFEE = 0;
            iPartialPercFEEVAT = 0;
            iPartialFixedFEEVAT = 0;
            iPartialBonusFEEVAT = 0;

            try
            {
                //iAmount = Convert.ToInt32(Math.Round(iTotalAmount / (1 + dPercFEE + (dPercFEE * dVAT2) + (dPercBonus * dVAT2) + dVAT1), MidpointRounding.AwayFromZero));
                iAmount = Convert.ToInt32(Math.Round((iTotalAmount - (iFixedFEE * (1 + dVAT2))) / (1 + (dPercFEE * (1 + dVAT2)) + (dPercBonus * (1 + dVAT2)) + dVAT1), MidpointRounding.AwayFromZero));

                int iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped)
                {
                    iAmount = Convert.ToInt32(Math.Round((iTotalAmount - (iFixedFEE * (1 + dVAT2)) - (iPercFEETopped * (1 + dVAT2))) / (1 + (dPercBonus * (1 + dVAT2)) + dVAT1), MidpointRounding.AwayFromZero));
                }

                iPartialVAT1 = Convert.ToInt32(Math.Round(iAmount * dVAT1, MidpointRounding.AwayFromZero));
                iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped) iPercFEE = iPercFEETopped;
                iPartialPercFEE = Convert.ToInt32(Math.Round(iPercFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));
                iPartialFixedFEE = Convert.ToInt32(Math.Round(iFixedFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                int iBonusFEE = Convert.ToInt32(Math.Round((iPercFEE + iFixedFEE) * dPercBonus, MidpointRounding.AwayFromZero));
                iPartialBonusFEE = Convert.ToInt32(Math.Round(iBonusFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                //iTotalAmount = iAmount + iPartialVAT1 + iPartialPercFEE + iPartialFixedFEE - iPartialBonusFEE;

                iPartialPercFEEVAT = Convert.ToInt32(Math.Round(iPercFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialFixedFEEVAT = Convert.ToInt32(Math.Round(iFixedFEE * dVAT2, MidpointRounding.AwayFromZero));
                iPartialBonusFEEVAT = Convert.ToInt32(Math.Round(iBonusFEE * dVAT2, MidpointRounding.AwayFromZero));
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEEReverse: ", e);
            }

            return iAmount;
        }
        public int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE)
        {
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            int iPartialBonusFEE = 0;

            return CalculateFEEReverse(iTotalAmount, dVAT1, dVAT2, dPercFEE, iPercFEETopped, iFixedFEE, 0,
                                out iPartialVAT1, out iPartialPercFEE, out iPartialFixedFEE, out iPartialBonusFEE);
        }
        public int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE)
        {
            int iAmount = 0;
            iPartialVAT1 = 0;
            iPartialPercFEE = 0;
            iPartialFixedFEE = 0;
            iPartialBonusFEE = 0;

            try
            {
                iAmount = Convert.ToInt32(Math.Round((iTotalAmount - (iFixedFEE * (1 + dVAT2))) / (1 + (dPercFEE * (1 + dVAT2)) + (dPercBonus * (1 + dVAT2)) + dVAT1), MidpointRounding.AwayFromZero));

                int iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped)
                {
                    iAmount = Convert.ToInt32(Math.Round((iTotalAmount - (iFixedFEE * (1 + dVAT2)) - (iPercFEETopped * (1 + dVAT2))) / (1 + (dPercBonus * (1 + dVAT2)) + dVAT1), MidpointRounding.AwayFromZero));
                }

                iPartialVAT1 = Convert.ToInt32(Math.Round(iAmount * dVAT1, MidpointRounding.AwayFromZero));
                iPercFEE = Convert.ToInt32(Math.Round(iAmount * dPercFEE, MidpointRounding.AwayFromZero));
                if (iPercFEETopped > 0 && iPercFEE > iPercFEETopped) iPercFEE = iPercFEETopped;
                iPartialPercFEE = Convert.ToInt32(Math.Round(iPercFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));
                iPartialFixedFEE = Convert.ToInt32(Math.Round(iFixedFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                int iBonusFEE = Convert.ToInt32(Math.Round((iPercFEE + iFixedFEE) * dPercBonus, MidpointRounding.AwayFromZero));
                iPartialBonusFEE = Convert.ToInt32(Math.Round(iBonusFEE * (1 + dVAT2), MidpointRounding.AwayFromZero));

                //iTotalAmount = iAmount + iPartialVAT1 + iPartialPercFEE + iPartialFixedFEE - iPartialBonusFEE;

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEEReverse: ", e);
            }

            return iAmount;
        }

        public int CalculateFEEReverse(decimal dInsId, ChargeOperationsType oOpeType, int iTotalAmount,
                                       out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE)
        {
            int iAmount = 0;
            dVAT1 = 0;
            dVAT2 = 0;
            iPartialVAT1 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iPartialPercFEE = 0;
            iFixedFEE = 0;
            iPartialFixedFEE = 0;

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
                    iAmount = CalculateFEEReverse(dInsId, oOpeType, iTotalAmount,
                                                  out dVAT1, out dVAT2, out iPartialVAT1, out dPercFEE, out iPercFEETopped, out iPartialPercFEE, out iFixedFEE, out iPartialFixedFEE, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEEReverse: ", e);
            }

            return iAmount;
        }
        public int CalculateFEEReverse(decimal dInsId, ChargeOperationsType oOpeType, int iTotalAmount,
                                       out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE,
                                       integraMobileDBEntitiesDataContext dbContext)
        {
            int iAmount = 0;
            dVAT1 = 0;
            dVAT2 = 0;
            iPartialVAT1 = 0;
            dPercFEE = 0;
            iPercFEETopped = 0;
            iPartialPercFEE = 0;
            iFixedFEE = 0;
            iPartialFixedFEE = 0;

            try
            {
                var oInstallation = (from r in dbContext.INSTALLATIONs
                                     where r.INS_ID == dInsId
                                     select r).FirstOrDefault();
                if (oInstallation != default(INSTALLATION))
                {

                    // ...
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "CalculateFEEReverse: ", e);
            }

            return iAmount;
        }

        public bool NeedDisplayLicenseTerms(USER oUser, string sCulture, out string sVersion, out string sUrl1, out string sUrl2)
        {
            bool bRet = false;
            sVersion = "";
            sUrl1 = "";
            sUrl2 = "";

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

                    var oLastVersion = dbContext.LICENSE_TERMs.OrderByDescending(t => t.LCT_ID).FirstOrDefault();
                    if (oLastVersion != default(LICENSE_TERM))
                    {
                        bRet = (oUser == null || oLastVersion.LCT_ID > (oUser.USR_LCT_ID ?? 0));
                        sVersion = oLastVersion.LCT_VERSION;
                        //if (bRet)
                        //{
                            var oLastVersionParams = (from t in dbContext.LICENSE_TERMS_PARAMs
                                                      where t.LTP_LCT_ID == oLastVersion.LCT_ID &&
                                                            t.LANGUAGE.LAN_CULTURE == sCulture
                                                      select t).AsQueryable().FirstOrDefault();
                            if (oLastVersionParams != default(LICENSE_TERMS_PARAM))
                            {
                                sUrl1 = oLastVersionParams.LTP_URL1;
                                sUrl2 = oLastVersionParams.LTP_URL2;
                            }
                        //}
                    }
                                        
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "NeedDisplayLicenseTerms: ", e);
            }

            return bRet;
        }

        public bool UpdateUserLicenseTerms(USER oUser, string sVersion)
        {
            bool bRes = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    try
                    {
                        decimal userId = oUser.USR_ID;
                        var oUpdateUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();
                        var oLicenseTerms = (from t in dbContext.LICENSE_TERMs
                                             where t.LCT_VERSION == sVersion
                                             select t).FirstOrDefault();

                        if (oUpdateUser != null)
                        {
                            if (oLicenseTerms != null)
                            {
                                oUpdateUser.LICENSE_TERM = oLicenseTerms;                                
                            }
                            bRes = true;
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();
                        
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateUserLicenseTerms: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateUserLicenseTerms: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool GetUserLock(ref USER_LOCK oUserLock, string username)
        {
            bool bRes = false;
            oUserLock = null;
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

                    var users = (from r in dbContext.USER_LOCKs
                                 where r.USRL_USR_USERNAME == username
                                 select r);

                    if (users.Count() > 0)
                    {
                        oUserLock = users.First();
                    }
                    bRes = (oUserLock != null);

                    transaction.Complete();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserLock: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool AddUserLock(string username, DateTime xLockUtcDate)
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

                    USER_LOCK oUserLock = null;

                    var locks = (from r in dbContext.USER_LOCKs
                                 where r.USRL_USR_USERNAME == username
                                 select r);

                    if (locks.Count() > 0)
                    {
                        oUserLock = locks.First();
                        oUserLock.USRL_LOCK_UTC_DATE = xLockUtcDate;
                    }
                    else
                    {
                        oUserLock = new USER_LOCK() {
                            USRL_USR_USERNAME = username,
                            USRL_LOCK_UTC_DATE = xLockUtcDate};
                        dbContext.USER_LOCKs.InsertOnSubmit(oUserLock);
                    }                    

                    // Submit the change to the database.
                    try
                    {
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        bRes = true;                                     
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddUserLock: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddUserLock: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool DeleteUserLock(string username)
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

                    var locks = (from r in dbContext.USER_LOCKs
                                 where r.USRL_USR_USERNAME == username
                                 select r);

                    if (locks.Count() > 0)
                    {
                        dbContext.USER_LOCKs.DeleteOnSubmit(locks.First());                        
                    }

                    // Submit the change to the database.
                    try
                    {
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteUserLock: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteUserLock: ", e);
                bRes = false;
            }

            return bRes;
        }

        public bool TransferBalance(ref USER srcUser, ref USER dstUser,
                                    int iOSType,                                            
                                    int iQuantity,
                                    decimal dCurID,
                                    decimal dDstBalanceCurID,
                                    double dChangeApplied,
                                    double dChangeFee,                     
                                    int iCurrencyDstQuantity,                                            
                                    decimal dMobileSessionId,
                                    string strAppVersion,                                    
                                    out decimal dTransferID,
                                    out DateTime? dtUTCInsertionDate)
        {
            bool bRes = true;
            dTransferID = -1;
            dtUTCInsertionDate = null;

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

                        BALANCE_TRANSFER oBalanceTransfer = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal dSrcUserId = srcUser.USR_ID;
                        decimal dDstUserId = dstUser.USR_ID;

                        var oSrcUser = (from r in dbContext.USERs
                                        where r.USR_ID == dSrcUserId && r.USR_ENABLED == 1
                                        select r).First();
                        var oDstUser = (from r in dbContext.USERs
                                        where r.USR_ID == dDstUserId && r.USR_ENABLED == 1
                                        select r).First();

                        if (oSrcUser != null && oDstUser != null)
                        {

                            DateTime dtTransferUTCDate = DateTime.UtcNow;
                            DateTime dtTransferDate = dtTransferUTCDate - new TimeSpan(0, oSrcUser.USR_UTC_OFFSET, 0);

                            decimal? dSrcCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtTransferDate, oSrcUser.CUSTOMER.CUS_ID, dCurID, 0, iQuantity, null, out dSrcCustomerInvoiceID);

                            decimal? dDstCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtTransferDate, oDstUser.CUSTOMER.CUS_ID, dDstBalanceCurID, iCurrencyDstQuantity, 0, null, out dDstCustomerInvoiceID);                                               

                            oBalanceTransfer = new BALANCE_TRANSFER
                            {
                                BAT_DST_USR_ID = oDstUser.USR_ID,
                                BAT_MOSE_OS = iOSType,
                                BAT_DATE = dtTransferDate,
                                BAT_UTC_DATE = dtTransferUTCDate,
                                BAT_DATE_UTC_OFFSET = oSrcUser.USR_UTC_OFFSET,
                                BAT_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                BAT_AMOUNT = iQuantity,                                
                                BAT_AMOUNT_CUR_ID = dCurID,
                                BAT_DST_BALANCE_CUR_ID = dDstBalanceCurID,
                                BAT_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                BAT_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                BAT_DST_AMOUNT = iCurrencyDstQuantity,                                                                
                                BAT_SRC_BALANCE_BEFORE = oSrcUser.USR_BALANCE,
                                BAT_DST_BALANCE_BEFORE = oDstUser.USR_BALANCE,
                                BAT_MOSE_ID = dMobileSessionId,
                                BAT_APP_VERSION = strAppVersion,
                                BAT_SRC_CUSINV_ID = dSrcCustomerInvoiceID,
                                BAT_DST_CUSINV_ID = dDstCustomerInvoiceID
                            };

                            ModifyUserBalance(ref oSrcUser, -iQuantity);
                            ModifyUserBalance(ref oDstUser, iCurrencyDstQuantity);


                            if (!oDstUser.USR_INSERT_MOSE_OS.HasValue) oDstUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oDstUser.USR_OPERATIVE_UTC_DATE.HasValue) oDstUser.USR_OPERATIVE_UTC_DATE = dtTransferUTCDate;

                            oSrcUser.BALANCE_TRANSFERs.Add(oBalanceTransfer);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oBalanceTransfer != null)
                            {
                                dTransferID = oBalanceTransfer.BAT_ID;
                                dtUTCInsertionDate = oBalanceTransfer.BAT_INSERTION_UTC_DATE;
                            }
                            transaction.Complete();
                            srcUser = oSrcUser;
                            dstUser = oDstUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "TransferBalance: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "TransferBalance: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "TransferBalance: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool QueryCouponAndLock(string sQrCode, bool bKeyQrCode, DateTime dtQueryDateUtc, decimal dExternalProviderId, int iLockTimeoutSeconds, out bool bAvailable, out RECHARGE_COUPON oRechargeCoupon)
        {
            bool bRes = true;
            bAvailable = false;
            oRechargeCoupon = null;            

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

                    var oCoupon = (from r in dbContext.RECHARGE_COUPONs
                                       where (r.RCOUP_CODE == sQrCode && !bKeyQrCode) || (r.RCOUP_KEYCODE == sQrCode && bKeyQrCode)
                                       select r).FirstOrDefault();

                    if (oCoupon != null)
                    {
                        RECHARGE_COUPONS_EXTERNALUSE oCouponExternal = oCoupon.RECHARGE_COUPONS_EXTERNALUSEs.FirstOrDefault();

                        if (oCoupon.RECHARGE_COUPONS_STATUS.RCOUPS_ID == Convert.ToInt32(RechargeCouponsStatus.Actived) ||
                            (oCoupon.RECHARGE_COUPONS_STATUS.RCOUPS_ID == Convert.ToInt32(RechargeCouponsStatus.Locked) && oCouponExternal != null && iLockTimeoutSeconds > 0 && oCouponExternal.RCEU_DATE_UTC.AddSeconds(iLockTimeoutSeconds) <= dtQueryDateUtc))
                        {
                            
                            dbContext.RECHARGE_COUPONS_EXTERNALUSEs.DeleteAllOnSubmit(oCoupon.RECHARGE_COUPONS_EXTERNALUSEs);

                            var oCouponStatusLocked = (from r in dbContext.RECHARGE_COUPONS_STATUS
                                                       where r.RCOUPS_ID == Convert.ToInt32(RechargeCouponsStatus.Locked)
                                                       select r)
                                                      .First();

                            /*var oExternalProvider = (from r in dbContext.EXTERNAL_PROVIDERs
                                                     where r.EXP_ID == dExternalProviderId
                                                     select r)
                                                     .FirstOrDefault();*/

                            var oCouponExternalUse = new RECHARGE_COUPONS_EXTERNALUSE();                            
                            oCouponExternalUse.RECHARGE_COUPON = oCoupon;
                            //oCouponExternalUse.EXTERNAL_PROVIDER = oExternalProvider;
                            oCouponExternalUse.RCEU_EXP_ID = dExternalProviderId;
                            oCouponExternalUse.RCEU_DATE_UTC = dtQueryDateUtc;
                            //oCouponExternalUse.RCEU_RCOUP_ID = Convert.ToInt32(RechargeCouponsStatus.Locked);
                            oCouponExternalUse.RECHARGE_COUPONS_STATUS = oCouponStatusLocked;
                            

                            dbContext.RECHARGE_COUPONS_EXTERNALUSEs.InsertOnSubmit(oCouponExternalUse);

                            oCoupon.RECHARGE_COUPONS_STATUS = oCouponStatusLocked;
                            

                            bAvailable = true;

                        }
                    }

                    // Submit the change to the database.
                    try
                    {
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        oRechargeCoupon = oCoupon;
                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "QueryCouponAndLock: ", e);
                        bRes = false;
                        bAvailable = false;
                    }                    
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "QueryCouponAndLock: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool ConfirmCoupon(string sQrCode, bool bKeyQrCode, DateTime dtQueryDateUtc, decimal dExternalProviderId, RechargeCouponsConfirmType oType, out RECHARGE_COUPON oResCoupon)
        {
            bool bRes = true;
            oResCoupon = null;

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

                    var oCoupon = (from r in dbContext.RECHARGE_COUPONs
                                   where (r.RCOUP_CODE == sQrCode && !bKeyQrCode) || (r.RCOUP_KEYCODE == sQrCode && bKeyQrCode)
                                   select r).FirstOrDefault();

                    if (oCoupon != null)
                    {
                        dbContext.RECHARGE_COUPONS_EXTERNALUSEs.DeleteAllOnSubmit(oCoupon.RECHARGE_COUPONS_EXTERNALUSEs);

                        var oCouponStatus = (from r in dbContext.RECHARGE_COUPONS_STATUS
                                             where r.RCOUPS_ID == Convert.ToInt32((oType == RechargeCouponsConfirmType.Cancel ? RechargeCouponsStatus.Actived : RechargeCouponsStatus.Used))
                                             select r)
                                            .First();

                        /*var oExternalProvider = (from r in dbContext.EXTERNAL_PROVIDERs
                                                    where r.EXP_ID == dExternalProviderId
                                                    select r)
                                                    .FirstOrDefault();*/

                        var oCouponExternalUse = new RECHARGE_COUPONS_EXTERNALUSE();
                        oCouponExternalUse.RECHARGE_COUPON = oCoupon;
                        //oCouponExternalUse.EXTERNAL_PROVIDER = oExternalProvider;
                        oCouponExternalUse.RCEU_EXP_ID = dExternalProviderId;
                        oCouponExternalUse.RCEU_DATE_UTC = dtQueryDateUtc;                        
                        oCouponExternalUse.RECHARGE_COUPONS_STATUS = oCouponStatus;

                        dbContext.RECHARGE_COUPONS_EXTERNALUSEs.InsertOnSubmit(oCouponExternalUse);
                        
                        oCoupon.RECHARGE_COUPONS_STATUS = oCouponStatus;
                    }

                    // Submit the change to the database.
                    try
                    {
                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();
                        oResCoupon = oCoupon;
                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ConfirmCoupon: ", e);
                        bRes = false;                        
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConfirmCoupon: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool InsertUserFriend(ref USER user, string sFriendEmail, long lSenderId)
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {
                            oUser.USERS_FRIENDs1.Add(new USERS_FRIEND()
                            {
                                USRF_DATE = DateTime.UtcNow,
                                USRF_FRIEND_EMAIL = sFriendEmail,
                                USRF_SENDER_ID = lSenderId,
                                USRF_STATUS = Convert.ToInt32(InvitationStatus.isSent),
                                USRF_ACCEPT_USR_ID = null
                               
                            });

                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;


                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "InsertUserFriend: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "InsertUserFriend: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "InsertUserFriend: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool AssignPendingInvitationsToAccept(ref USER user)
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
                        decimal userId = user.USR_ID;
                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();
                        string strEmail = oUser.USR_EMAIL;

                       

                       var oUserInvitation = (from r in dbContext.USERS_FRIENDs
                                     where r.USRF_FRIEND_EMAIL==strEmail
                                     select r).OrderByDescending(r => r.USRF_DATE).FirstOrDefault();

                        if (oUserInvitation != null)
                        {
                            oUserInvitation.USRF_STATUS = (int)InvitationStatus.isAccepted;
                            oUserInvitation.USRF_ACCEPT_USR_ID = userId;
                            oUserInvitation.USRF_ACCEPT_DATE = DateTime.UtcNow;

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();
                                user = oUser;


                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "AssignPendingInvitationsToAccept: ", e);
                                bRes = false;
                            }

                        }

                        
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AssignPendingInvitationsToAccept: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AssignPendingInvitationsToAccept: ", e);
                bRes = false;
            }

            return bRes;

        }


        public IEnumerable<TOLL_MOVEMENT> GetUserPlateLastTollMovement(ref USER user, out int iNumRows)
        {
            List<TOLL_MOVEMENT> res = new List<TOLL_MOVEMENT>();
            iNumRows = 0;
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

                    decimal userId = user.USR_ID;


                    foreach (USER_PLATE plate in user.USER_PLATEs.OrderBy(t => t.USRP_PLATE))
                    {
                        if (plate.USRP_ENABLED == 1)
                        {
                            try
                            {
                                var resplate = (from r in dbContext.TOLL_MOVEMENTs
                                                where ((r.TOLM_USR_ID == userId) && r.USER.USR_ENABLED == 1 &&
                                                       (r.TOLM_USRP_ID == plate.USRP_ID))
                                                orderby r.TOLM_DATE descending
                                                select r).First();
                                res.Add(resplate);
                                iNumRows++;

                            }
                            catch
                            {

                            }

                        }
                    }
                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserPlateLastTollMovement: ", e);
            }

            return (IEnumerable<TOLL_MOVEMENT>)res;
        }

        public bool ChargeTollMovement(ref USER user,
                                        int iOSType,
                                        bool bSubstractFromBalance,
                                        PaymentSuscryptionType suscriptionType,
                                        string strPlate,
                                        decimal dInstallationID,
                                        decimal? dTollID,
                                        string sTollTariff,
                                        DateTime dtPaymentDate,
                                        DateTime dtUTCPaymentDate,
                                        int iQuantity,
                                        decimal dCurID,
                                        decimal dBalanceCurID,
                                        double dChangeApplied,
                                        double dChangeFee,
                                        int iCurrencyChargedQuantity,
                                        decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, int iTotalAmount,
                                        decimal? dRechargeId,                                        
                                        string sExternalId,
                                        ChargeOperationsType eType,
                                        string sQr,
                                        decimal? dLockMovementId,
                                        out decimal dMovementID,
                                        out DateTime? dtUTCInsertionDate)
        {
            bool bRes = true;
            dMovementID = -1;
            dtUTCInsertionDate = null;

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

                        TOLL_MOVEMENT oMovement = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            decimal? dPlateID = null;
                            if (!string.IsNullOrEmpty(strPlate))
                            {
                                try
                                {
                                    var oPlate = oUser.USER_PLATEs.Where(r => r.USRP_PLATE == strPlate.ToUpper().Trim().Replace(" ", "") && r.USRP_ENABLED == 1).First();
                                    if (oPlate != null)
                                    {
                                        dPlateID = oPlate.USRP_ID;
                                    }
                                }
                                catch
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "ChargeTollMovement: Plate is not from user or is not enabled: " + strPlate);
                                    bRes = false;
                                    return bRes;
                                }
                            }


                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, dtPaymentDate, oUser.CUSTOMER.CUS_ID, dCurID, 0, iTotalAmount, dInstallationID, out dCustomerInvoiceID);

                            oMovement = new TOLL_MOVEMENT()
                            {
                                TOLM_MOSE_OS = iOSType,                                
                                TOLM_USRP_ID = dPlateID,
                                TOLM_INS_ID = dInstallationID,
                                TOLM_TOL_ID = dTollID,
                                TOLM_TOL_TARIFF = sTollTariff,
                                TOLM_DATE = dtPaymentDate,
                                TOLM_UTC_DATE = dtUTCPaymentDate,
                                TOLM_DATE_UTC_OFFSET = Convert.ToInt32((dtUTCPaymentDate - dtPaymentDate).TotalMinutes),
                                TOLM_AMOUNT = iQuantity,                                
                                TOLM_AMOUNT_CUR_ID = dCurID,
                                TOLM_BALANCE_CUR_ID = dBalanceCurID,
                                TOLM_CHANGE_APPLIED = Convert.ToDecimal(dChangeApplied),
                                TOLM_CHANGE_FEE_APPLIED = Convert.ToDecimal(dChangeFee),
                                TOLM_FINAL_AMOUNT = iCurrencyChargedQuantity,
                                TOLM_INSERTION_UTC_DATE = dbContext.GetUTCDate(),
                                TOLM_CUSPMR_ID = dRechargeId,
                                TOLM_BALANCE_BEFORE = oUser.USR_BALANCE,
                                TOLM_SUSCRIPTION_TYPE = (int)suscriptionType,
                                //TOLM_APP_VERSION = strAppVersion,
                                TOLM_EXTERNAL_ID = sExternalId,                                
                                TOLM_PERC_VAT1 = Convert.ToDecimal(dPercVat1),
                                TOLM_PERC_VAT2 = Convert.ToDecimal(dPercVat2),
                                TOLM_PARTIAL_VAT1 = iPartialVat1,
                                TOLM_PERC_FEE = Convert.ToDecimal(dPercFEE),
                                TOLM_PERC_FEE_TOPPED = iPercFEETopped,
                                TOLM_PARTIAL_PERC_FEE = iPartialPercFEE,
                                TOLM_FIXED_FEE = iFixedFEE,
                                TOLM_PARTIAL_FIXED_FEE = iPartialFixedFEE,
                                TOLM_TOTAL_AMOUNT = iTotalAmount,
                                TOLM_CUSINV_ID = dCustomerInvoiceID,
                                TOLM_TYPE = (int)eType,                                
                                TOLM_QR_CODE = sQr,
                                TOLM_LOCK_TOLM_ID = dLockMovementId
                            };

                            if (bSubstractFromBalance)
                            {
                                if (eType != ChargeOperationsType.TollUnlock)
                                    ModifyUserBalance(ref oUser, -iCurrencyChargedQuantity);
                                else
                                    ModifyUserBalance(ref oUser, iCurrencyChargedQuantity);

                            }

                            if (!oUser.USR_INSERT_MOSE_OS.HasValue) oUser.USR_INSERT_MOSE_OS = iOSType;
                            if (!oUser.USR_OPERATIVE_UTC_DATE.HasValue) oUser.USR_OPERATIVE_UTC_DATE = dtUTCPaymentDate;
                            if (!oUser.USR_FIRST_OPERATION_INS_ID.HasValue)
                            {
                                oUser.USR_FIRST_OPERATION_INS_ID = dInstallationID;
                            }
                            oUser.USR_LAST_OPERATION_INS_ID = dInstallationID;


                            oUser.TOLL_MOVEMENTs.Add(oMovement);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            if (oMovement != null)
                            {
                                dMovementID = oMovement.TOLM_ID;
                                dtUTCInsertionDate = oMovement.TOLM_INSERTION_UTC_DATE;
                            }
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ChargeTollMovement: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ChargeTollMovement: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ChargeTollMovement: ", e);
                bRes = false;
            }

            return bRes;

        }

        /*public bool ModifyTollMovement(decimal dMovementId,
                                    ref USER user,                                    
                                    bool bSubstractFromBalance,                                    
                                    decimal dTollID,
                                    string sTollTariff,
                                    int iQuantity,
                                    int iCurrencyChargedQuantity,
                                    decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, int iTotalAmount,
                                    decimal? dRechargeId,
                                    string sExternalId,
                                    TollMovementType eType,
                                    string sQr)
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

                        TOLL_MOVEMENT oMovement = null;

                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                        decimal userId = user.USR_ID;

                        var oUser = (from r in dbContext.USERs
                                     where r.USR_ID == userId && r.USR_ENABLED == 1
                                     select r).First();

                        if (oUser != null)
                        {

                            oMovement = (from r in dbContext.TOLL_MOVEMENTs
                                         where r.TOLM_ID == dMovementId
                                         select r).First();


                            decimal? dCustomerInvoiceID = null;
                            GetCustomerInvoice(dbContext, oMovement.TOLM_DATE, oUser.CUSTOMER.CUS_ID, oMovement.TOLM_AMOUNT_CUR_ID, 0, (oMovement.TOLM_TOTAL_AMOUNT ?? 0) - iTotalAmount, oMovement.TOLM_INS_ID, out dCustomerInvoiceID);

                            if (bSubstractFromBalance)
                            {
                                ModifyUserBalance(ref oUser, -(iCurrencyChargedQuantity - oMovement.TOLM_FINAL_AMOUNT));
                            }

                            
                            oMovement.TOLM_TOL_ID = dTollID;
                            oMovement.TOLM_TOL_TARIFF = sTollTariff;
                            oMovement.TOLM_AMOUNT = iQuantity;
                            oMovement.TOLM_FINAL_AMOUNT = iCurrencyChargedQuantity;
                            //oMovement.TOLM_CUSPMR_ID = dRechargeId;
                            //oMovement.TOLM_BALANCE_BEFORE = oUser.USR_BALANCE;
                            oMovement.TOLM_EXTERNAL_ID = sExternalId;
                            oMovement.TOLM_PERC_VAT1 = Convert.ToDecimal(dPercVat1);
                            oMovement.TOLM_PERC_VAT2 = Convert.ToDecimal(dPercVat2);
                            oMovement.TOLM_PARTIAL_VAT1 = iPartialVat1;
                            oMovement.TOLM_PERC_FEE = Convert.ToDecimal(dPercFEE);
                            oMovement.TOLM_PERC_FEE_TOPPED = iPercFEETopped;
                            oMovement.TOLM_PARTIAL_PERC_FEE = iPartialPercFEE;
                            oMovement.TOLM_FIXED_FEE = iFixedFEE;
                            oMovement.TOLM_PARTIAL_FIXED_FEE = iPartialFixedFEE;
                            oMovement.TOLM_TOTAL_AMOUNT = iTotalAmount;
                            oMovement.TOLM_CUSINV_ID = dCustomerInvoiceID;
                            oMovement.TOLM_TYPE = (int)eType;                            
                            oMovement.TOLM_QR_CODE = sQr;                            

                            oUser.TOLL_MOVEMENTs.Add(oMovement);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                            user = oUser;
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "ModifyTollMovement: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "ModifyTollMovement: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyTollMovement: ", e);
                bRes = false;
            }

            return bRes;

        }*/

        public TOLL GetToll(decimal dTollId)
        {

            TOLL oToll = null;
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


                    oToll = (from r in dbContext.TOLLs
                                  where (r.TOL_ID == dTollId)
                                  select r).First();

                }


            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetToll: ", e);
                oToll = null;
            }

            return oToll;

        }

        public bool GetUserAverageBalanceById(ref USER_AVERAGE_BALANCE oUserAverageBal, decimal dUserId)
        {
            bool bRes = false;
            oUserAverageBal = null;
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

                    var avgs = (from r in dbContext.USER_AVERAGE_BALANCEs
                                 where r.USRB_USR_ID == dUserId
                                 orderby r.USRB_UTC_DATE descending
                                 select r);

                    if (avgs.Count() > 0)
                    {
                        oUserAverageBal = avgs.First();
                    }

                    bRes = (oUserAverageBal != null);

                    transaction.Complete();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUserAverageBalanceById: ", e);
                bRes = false;
            }

            return bRes;
        }

        public IQueryable<TOLL_MOVEMENT> GetTollMovementsByQr(string sQr)
        {
            IQueryable<TOLL_MOVEMENT> oRet = null;            

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

                        oRet = (from r in dbContext.TOLL_MOVEMENTs
                                where r.TOLM_QR_CODE == sQr
                                select r).AsQueryable();
                                                        

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementsByQr: ", e);                        
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementsByQr: ", e);                
            }
            return oRet;
        }

        public bool GetTollMovementById(decimal dId, out TOLL_MOVEMENT oTollMovement)
        {
            bool bRes = false;
            oTollMovement = null;

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

                        oTollMovement = (from r in dbContext.TOLL_MOVEMENTs
                                         where r.TOLM_ID == dId
                                         select r).First();

                        bRes = (oTollMovement != null);

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementById: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementById: ", e);
                bRes = false;
            }
            return bRes;
        }

        public bool GetTollMovementByLockId(decimal dLockId, out TOLL_MOVEMENT oTollMovement)
        {
            bool bRes = false;
            oTollMovement = null;

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

                        oTollMovement = (from r in dbContext.TOLL_MOVEMENTs
                                         where r.TOLM_LOCK_TOLM_ID == dLockId
                                         select r).First();

                        bRes = (oTollMovement != null);

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementByLockId: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTollMovementByLockId: ", e);
                bRes = false;
            }
            return bRes;
        }

        public bool GatewayErrorLogUpdate(decimal dGatewayConfigId, bool bExceptionError, bool bTransactionError)
        {
            bool bRes = false;            


            // lock gatewayconfig
            if (this.GatewayErrorLogAcquireLock(dGatewayConfigId))
            {
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



                            var oGatewayConfig = (from t in dbContext.CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIGs
                                                  where t.CPTGC_ID == dGatewayConfigId
                                                  select t)
                                                 .FirstOrDefault();
                            if (oGatewayConfig != null)
                            {


                                var oLastErrorLog = (from t in oGatewayConfig.CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOGs
                                                     where !t.CPTGE_END_DATE.HasValue
                                                     orderby t.CPTGE_INI_DATE descending
                                                     select t)
                                                    .FirstOrDefault();

                                if (bExceptionError || bTransactionError)
                                {
                                    int iErrorType = (bExceptionError ? 0 : 1);

                                    if (oLastErrorLog == null)
                                    {
                                        oLastErrorLog = new CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOG()
                                        {
                                            CPTGE_CPTGC_ID = dGatewayConfigId,
                                            CPTGE_ERROR_TYPE = iErrorType,
                                            CPTGE_INI_DATE = DateTime.UtcNow,
                                            CPTGE_ERROR_COUNT = 0
                                        };
                                        oGatewayConfig.CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOGs.Add(oLastErrorLog);
                                    }
                                    if (oLastErrorLog.CPTGE_ERROR_TYPE != iErrorType)
                                    {
                                        oLastErrorLog.CPTGE_END_DATE = DateTime.UtcNow;
                                        oLastErrorLog = new CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOG()
                                        {
                                            CPTGE_CPTGC_ID = dGatewayConfigId,
                                            CPTGE_ERROR_TYPE = iErrorType,
                                            CPTGE_INI_DATE = DateTime.UtcNow,
                                            CPTGE_ERROR_COUNT = 0
                                        };
                                        oGatewayConfig.CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOGs.Add(oLastErrorLog);                                        
                                    }
                                    oLastErrorLog.CPTGE_ERROR_COUNT += 1;
                                }
                                else
                                {
                                    if (oLastErrorLog != null)
                                    {
                                        oLastErrorLog.CPTGE_END_DATE = DateTime.UtcNow;
                                    }
                                }

                                // Submit the change to the database.
                                try
                                {
                                    SecureSubmitChanges(ref dbContext);
                                    transaction.Complete();
                                    bRes = true;
                                }
                                catch (Exception e)
                                {
                                    m_Log.LogMessage(LogLevels.logERROR, "GatewayErrorLogUpdate: ", e);
                                    bRes = false;
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "GatewayErrorLogUpdate: ", e);
                            bRes = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    m_Log.LogMessage(LogLevels.logERROR, "GatewayErrorLogUpdate: ", e);
                    bRes = false;
                }
                finally
                {
                    this.GatewayErrorLogReleaseLock(dGatewayConfigId);
                }

            }

            return bRes;        
        }

        public bool GetGatewayErrorLogTotalSeconds(decimal dGatewayConfigId, DateTime dtIniUtc, DateTime dtEndUtc, int iMinCommErrors, int iMinTransErrors, out double dTotalSeconds)
        {
            bool bRes = false;
            dTotalSeconds = 0;

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


                        var rows = (from t in dbContext.CURRENCIES_PAYMENT_TYPE_GATEWAY_ERRORLOGs
                                    where //(t.CPTGE_INI_DATE >= dtIniUtc && (t.CPTGE_END_DATE ?? dtEndUtc) <= dtEndUtc) ||
                                          ((t.CPTGE_INI_DATE < dtIniUtc && (t.CPTGE_END_DATE ?? dtEndUtc) > dtIniUtc) ||
                                           (t.CPTGE_INI_DATE >= dtIniUtc && t.CPTGE_INI_DATE <= dtEndUtc)) &&
                                          t.CPTGE_CPTGC_ID == dGatewayConfigId &&
                                          ((t.CPTGE_ERROR_TYPE == 0 && t.CPTGE_ERROR_COUNT >= iMinCommErrors) || (t.CPTGE_ERROR_TYPE == 1 && t.CPTGE_ERROR_COUNT >= iMinTransErrors))
                                    select new { total_seconds = (((t.CPTGE_END_DATE ?? dtEndUtc) > dtEndUtc ? dtEndUtc : (t.CPTGE_END_DATE ?? dtEndUtc)) - (t.CPTGE_INI_DATE < dtIniUtc ? dtIniUtc : t.CPTGE_INI_DATE)).TotalSeconds });
                        if (rows.Any()) 
                            dTotalSeconds = rows.Sum(t => t.total_seconds);

                        bRes = true;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "GetGatewayErrorLogTotalSeconds: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGatewayErrorLogTotalSeconds: ", e);
                bRes = false;
            }

            return bRes;
        }

        private int? m_iGatewayErrorLogLockMaxRetries = null;
        private int? m_iGatewayErrorLogLockTimeoutRetryMili = null;
        private int? m_iGatewayErrorLogLockTimeoutLockSec = null;

        private int GatewayErrorLogLock_MaxRetries
        {
            get
            {
                if (!m_iGatewayErrorLogLockMaxRetries.HasValue)
                {
                    var oValue = ConfigurationManager.AppSettings[ct_GATEWAY_LOG_ERROR_MAXRETRIES_TAG];
                    if (oValue != null && !string.IsNullOrEmpty(oValue.ToString()))
                        m_iGatewayErrorLogLockMaxRetries = Convert.ToInt32(oValue.ToString());
                    else
                        m_iGatewayErrorLogLockMaxRetries = ct_GATEWAY_LOG_ERROR_MAXRETRIES_DEFAULT;
                }
                return m_iGatewayErrorLogLockMaxRetries.Value;
            }
        }
        private int GatewayErrorLogLock_TimeoutRetryMili
        {
            get
            {
                if (!m_iGatewayErrorLogLockTimeoutRetryMili.HasValue)
                {
                    var oValue = ConfigurationManager.AppSettings[ct_GATEWAY_LOG_ERROR_RETRYTIMEOUTMILI_TAG];
                    if (oValue != null && !string.IsNullOrEmpty(oValue.ToString()))
                        m_iGatewayErrorLogLockTimeoutRetryMili = Convert.ToInt32(oValue.ToString());
                    else
                        m_iGatewayErrorLogLockTimeoutRetryMili = ct_GATEWAY_LOG_ERROR_RETRYTIMEOUTMILI_DEFAULT;
                }
                return m_iGatewayErrorLogLockTimeoutRetryMili.Value;
            }
        }
        private int GatewayErrorLogLock_TimeoutLockSec
        {
            get
            {
                if (!m_iGatewayErrorLogLockTimeoutLockSec.HasValue)
                {
                    var oValue = ConfigurationManager.AppSettings[ct_GATEWAY_LOG_ERROR_LOCKTIMEOUTSEC_TAG];
                    if (oValue != null && !string.IsNullOrEmpty(oValue.ToString()))
                        m_iGatewayErrorLogLockTimeoutLockSec = Convert.ToInt32(oValue.ToString());
                    else
                        m_iGatewayErrorLogLockTimeoutLockSec = ct_GATEWAY_LOG_ERROR_LOCKTIMEOUTSEC_DEFAULT;
                }
                return m_iGatewayErrorLogLockTimeoutLockSec.Value;
            }
        }

        private bool GatewayErrorLogAcquireLock(decimal dGatewayConfigId)
        {
            bool bRet = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                int iRetry = 0;
                while (!bRet && iRetry < this.GatewayErrorLogLock_MaxRetries)
                {
                    var oLockResult = dbContext.GatewayErrorLog_AcquireLock(dGatewayConfigId, GatewayErrorLogLock_TimeoutLockSec).FirstOrDefault();
                    if (oLockResult != null)
                        bRet = (oLockResult.Column1 > 0);

                    if (!bRet && iRetry < GatewayErrorLogLock_MaxRetries)                        
                    {
                        System.Threading.Thread.Sleep(GatewayErrorLogLock_TimeoutRetryMili);
                    }
                    iRetry += 1;
                }
                if (!bRet)
                {
                    m_Log.LogMessage(LogLevels.logERROR, string.Format("GatewayErrorLogAcquireLock: Acquire lock failed for {0} gateway config.", dGatewayConfigId));
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GatewayErrorLogAcquireLock: ", e);
            }
            return bRet;
        }
        private bool GatewayErrorLogReleaseLock(decimal dGatewayConfigId)
        {
            bool bRet = false;
            try
            {
                integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                var oLockResult = dbContext.GatewayErrorLog_ReleaseLock(dGatewayConfigId).FirstOrDefault();
                if (oLockResult != null)
                    bRet = (oLockResult.Column1 > 0);

                if (!bRet)
                    m_Log.LogMessage(LogLevels.logERROR, string.Format("GatewayErrorLogReleaseLock: Release lock failed for {0} gateway config.", dGatewayConfigId));
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GatewayErrorLogReleaseLock: ", e);
            }
            return bRet;
        }

        private bool ModifyUserBalance(ref USER oUser, int iAmount)
        {
            bool bRes = true;
            try
            {
                if ((RefundBalanceType)(oUser.USR_REFUND_BALANCE_TYPE) == RefundBalanceType.rbtAmount)
                {
                    oUser.USR_BALANCE += iAmount;

                    if (iAmount < 0)
                    {
                        oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBalance: ", e);
                bRes = false;
            }

            return bRes;

        }


        private bool ModifyUserBalance(ref USER oUser, int iAmount, int iTime)
        {
            bool bRes = true;
            try
            {

                if ((RefundBalanceType)(oUser.USR_REFUND_BALANCE_TYPE) == RefundBalanceType.rbtAmount)
                {
                    oUser.USR_BALANCE += iAmount;
                    if (iAmount < 0)
                    {
                        oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;
                    }
                }
                else
                {
                    oUser.USR_TIME_BALANCE += iTime;
                    if (iTime < 0)
                    {
                        oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBalance: ", e);
                bRes = false;
            }

            return bRes;

        }


        private bool ModifyUserBalanceChargeOperation(ref USER oUser, int iAmount, int iTime)
        {
            bool bRes = true;
            try
            {

                if ((RefundBalanceType)(oUser.USR_REFUND_BALANCE_TYPE) == RefundBalanceType.rbtAmount)
                {
                    oUser.USR_BALANCE += iAmount;

                    if (iAmount < 0)
                    {
                        oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;
                    }
                }
                else
                {
                    oUser.USR_TIME_BALANCE += iTime;

                    if (iTime < 0)
                    {
                        oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBalanceChargeOperation: ", e);
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


        private string GenerateRandom(int iNumCharacters)
        {
            System.Random rand = new System.Random(Convert.ToInt32(DateTime.Now.Ticks%Int32.MaxValue));

            string strRandNumber = rand.Next(Convert.ToInt32(Math.Pow(10, iNumCharacters)) - 1).ToString();

            return strRandNumber.PadLeft(iNumCharacters, '0');
        }

        private string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            System.Random rand = new System.Random(Convert.ToInt32(DateTime.Now.Ticks % Int32.MaxValue));
            return string.Format("{0:x}", i - rand.Next(1, Convert.ToInt32(DateTime.Now.Ticks % Int32.MaxValue)));
        }

        private string GetParameterValue(string strParName, integraMobileDBEntitiesDataContext dbContext)
        {
            string strRes = "";
            try
            {
                var oResultParameters = dbContext.PARAMETERs.Where(par => par.PAR_NAME == strParName);
                if (oResultParameters.Count() > 0)
                {
                    PARAMETER oParameter = oResultParameters.First();
                    strRes = oParameter.PAR_VALUE;
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetParameterValue: ", e);
                strRes = "";
            }

            return strRes;
        }

        private decimal GetChangeFeePerc(integraMobileDBEntitiesDataContext dbContext)
        {
            string strRes = "";
            decimal dRes = 0;
            try
            {
                strRes = GetParameterValue("CHANGE_FEE_PERC", dbContext);
                dRes = decimal.Parse(strRes, CultureInfo.InvariantCulture);

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetChangeFeePerc: ", e);
                strRes = "";
            }

            return dRes;
        }
    }
}
