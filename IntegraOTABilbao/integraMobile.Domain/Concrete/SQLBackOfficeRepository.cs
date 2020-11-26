using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Data.Linq;
using System.Transactions;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using integraMobile.Domain.Abstract;
using integraMobile.Domain;
using integraMobile.Infrastructure.Logging.Tools;
using integraMobile.Infrastructure;
using integraMobile.Domain.Helper;

namespace integraMobile.Domain.Concrete
{
    public class SQLBackOfficeRepository : IBackOfficeRepository
    {

        //Log4net Wrapper class
        private static readonly CLogWrapper m_Log = new CLogWrapper(typeof(SQLBackOfficeRepository));
        private const int ctnTransactionTimeout = 30;


        public SQLBackOfficeRepository(string connectionString)
        {
        }


        public IQueryable<ALL_OPERATIONS_EXT> GetOperationsExt(Expression<Func<ALL_OPERATIONS_EXT, bool>> predicate, int iTransactionTimeout = 0)
        {
            IQueryable<ALL_OPERATIONS_EXT> res = null;
            try
            {
                if (iTransactionTimeout == 0) iTransactionTimeout = ctnTransactionTimeout;

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(iTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                    dbContext.CommandTimeout = iTransactionTimeout;
                    res = GetOperationsExt(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsExt: ", e);
            }

            return res;
        }
        public IQueryable<ALL_OPERATIONS_EXT> GetOperationsExt(Expression<Func<ALL_OPERATIONS_EXT, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<ALL_OPERATIONS_EXT> res = null;
            try
            {
                res = (from r in dbContext.ALL_OPERATIONS_EXTs
                        select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsExt: ", e);
            }

            return res;
        }

        public IQueryable<ALL_CURR_OPERATIONS_EXT> GetOperationsExt(Expression<Func<ALL_CURR_OPERATIONS_EXT, bool>> predicate, int iTransactionTimeout = 0)
        {
            IQueryable<ALL_CURR_OPERATIONS_EXT> res = null;
            try
            {
                if (iTransactionTimeout == 0) iTransactionTimeout = ctnTransactionTimeout;

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                    dbContext.CommandTimeout = iTransactionTimeout;
                    res = GetOperationsExt(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsExt: ", e);
            }

            return res;
        }
        public IQueryable<ALL_CURR_OPERATIONS_EXT> GetOperationsExt(Expression<Func<ALL_CURR_OPERATIONS_EXT, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<ALL_CURR_OPERATIONS_EXT> res = null;
            try
            {
                res = (from r in dbContext.ALL_CURR_OPERATIONS_EXTs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsExt: ", e);
            }

            return res;
        }

        public IQueryable<USER> GetUsers(Expression<Func<USER, bool>> predicate)
        {
            IQueryable<USER> res = null;
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
                    res = GetUsers(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUsers: ", e);
            }

            return res;
        }
        public IQueryable<USER> GetUsers(Expression<Func<USER, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<USER> res = null;
            try
            {
                res = (from r in dbContext.USERs
                        select r)
                        .Where(predicate)
                        .OrderBy(t => t.USR_USERNAME)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUsers: ", e);
            }

            return res;
        }

        public IQueryable<GROUP> GetGroups(Expression<Func<GROUP, bool>> predicate = null)
        {
            IQueryable<GROUP> res = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    if (predicate == null) predicate = PredicateBuilder.True<GROUP>();
                    predicate = predicate.And(group => group.INSTALLATION.INS_ENABLED != 0);

                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    res = (from r in dbContext.GROUPs
                           select r)
                           .Where(predicate)
                           .AsQueryable();

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGroups: ", e);
            }

            return res;
        }

        public IQueryable<TARIFF> GetTariffs(Expression<Func<TARIFF, bool>> predicate = null)
        {
            IQueryable<TARIFF> res = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    if (predicate == null) predicate = PredicateBuilder.True<TARIFF>();
                    predicate = predicate.And(tariff => tariff.INSTALLATION.INS_ENABLED != 0);

                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    res = (from r in dbContext.TARIFFs
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTariffs: ", e);
            }

            return res;
        }

        public IQueryable<SERVICE_CHARGE_TYPE> GetServiceChargeTypes(Expression<Func<SERVICE_CHARGE_TYPE, bool>> predicate = null)
        {
            IQueryable<SERVICE_CHARGE_TYPE> res = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    if (predicate == null) predicate = PredicateBuilder.True<SERVICE_CHARGE_TYPE>();
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    res = (from r in dbContext.SERVICE_CHARGE_TYPEs
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetServiceChargeTypes: ", e);
            }

            return res;
        }

        public IQueryable<CURRENCy> GetCurrencies(Expression<Func<CURRENCy, bool>> predicate = null)
        {
            IQueryable<CURRENCy> res = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    if (predicate == null) predicate = PredicateBuilder.True<CURRENCy>();

                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();
                    res = GetCurrencies(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencies: ", e);
            }

            return res;
        }
        public IQueryable<CURRENCy> GetCurrencies(Expression<Func<CURRENCy, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<CURRENCy> res = null;
            try
            {
                res = (from r in dbContext.CURRENCies
                       select r)
                       .Where(predicate)
                       .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCurrencies: ", e);
            }

            return res;
        }


        public IQueryable<COUNTRy> GetCountries(Expression<Func<COUNTRy, bool>> predicate = null)
        {
            IQueryable<COUNTRy> res = null;
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

                    if (predicate == null) predicate = PredicateBuilder.True<COUNTRy>();

                    res = (from r in dbContext.COUNTRies
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCountries: ", e);
            }

            return res;
        }


        public IQueryable<CUSTOMER_INVOICE> GetInvoices(Expression<Func<CUSTOMER_INVOICE, bool>> predicate)
        {
            IQueryable<CUSTOMER_INVOICE> res = null;
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

                    res = (from r in dbContext.CUSTOMER_INVOICEs
                           where r.CUSINV_INV_NUMBER != null
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInvoices: ", e);
            }

            return res;
        }

        public IQueryable<EXTERNAL_PARKING_OPERATION> GetExternalOperations(Expression<Func<EXTERNAL_PARKING_OPERATION, bool>> predicate)
        {
            IQueryable<EXTERNAL_PARKING_OPERATION> res = null;
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

                    res = (from r in dbContext.EXTERNAL_PARKING_OPERATIONs
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetExternalOperations: ", e);
            }

            return res;
        }

        public IQueryable<EXTERNAL_PROVIDER> GetExternalProviders(Expression<Func<EXTERNAL_PROVIDER, bool>> predicate = null)
        {
            IQueryable<EXTERNAL_PROVIDER> res = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    if (predicate == null) predicate = PredicateBuilder.True<EXTERNAL_PROVIDER>();

                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    res = (from r in dbContext.EXTERNAL_PROVIDERs
                           select r)
                           .Where(predicate)
                           .AsQueryable();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetExternalProviders: ", e);
            }

            return res;
        }

        public IQueryable<CUSTOMER_INSCRIPTION> GetCustomerInscriptions(Expression<Func<CUSTOMER_INSCRIPTION, bool>> predicate)
        {
            IQueryable<CUSTOMER_INSCRIPTION> res = null;
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

                    res = (from r in dbContext.CUSTOMER_INSCRIPTIONs
                           select r)
                           .Where(predicate)
                           .OrderByDescending(t => t.CUSINS_LAST_SENT_DATE)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCustomerInscriptions: ", e);
            }

            return res;
        }

        public IQueryable<INSTALLATION> GetInstallations(Expression<Func<INSTALLATION, bool>> predicate)
        {
            IQueryable<INSTALLATION> res = null;
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

                    res = (from r in dbContext.INSTALLATIONs
                           select r)
                           .Where(predicate)
                           .OrderBy(t => t.INS_DESCRIPTION)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInstallations: ", e);
            }

            return res;
        }

        public IQueryable<USERS_SECURITY_OPERATION> GetUsersSecurityOperations(Expression<Func<USERS_SECURITY_OPERATION, bool>> predicate)
        {
            IQueryable<USERS_SECURITY_OPERATION> res = null;
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

                    res = (from r in dbContext.USERS_SECURITY_OPERATIONs
                           select r)
                           .Where(predicate)
                           .OrderByDescending(t => t.USOP_LAST_SENT_DATE)
                           .AsQueryable();
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetUsersSecurityOperations: ", e);
            }

            return res;
        }

        public bool SetUserEnabled(decimal dUserId, bool bEnabled, out USER oUser)
        {
            bool bRes = true;
            oUser = null;
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {
                    string sUsername = "";

                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        oUser = (from r in dbContext.USERs
                                 where r.USR_ID == dUserId
                                 select r).First();

                        oUser.USR_ENABLED = (bEnabled ? 1 : 0);                        

                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();                        

                        sUsername = oUser.USR_USERNAME;
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "SetUserEnabled: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "SetUserEnabled: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool UpdateCountry(ref COUNTRy country)
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
                        decimal countryId = country.COU_ID;
                        if (countryId != 0)
                        {
                            var oCountry = (from r in dbContext.COUNTRies
                                            where r.COU_ID == countryId
                                            select r).First();

                            if (oCountry != null)
                            {
                                oCountry.COU_DESCRIPTION = country.COU_DESCRIPTION;
                                oCountry.COU_CODE = country.COU_CODE;
                                oCountry.COU_TEL_PREFIX = country.COU_TEL_PREFIX;
                                oCountry.COU_CUR_ID = country.COU_CUR_ID;
                                country = oCountry;
                                bRes = true;
                            }
                        }
                        else
                        {
                            var oCountry = (from r in dbContext.COUNTRies
                                            orderby r.COU_ID descending
                                            select r).First();
                            if (oCountry != null)
                                country.COU_ID = oCountry.COU_ID + 1;
                            else
                                country.COU_ID = 1;
                            dbContext.COUNTRies.InsertOnSubmit(country);
                            bRes = true;
                        }

                        if (bRes)
                        {
                            SecureSubmitChanges(ref dbContext);

                            transaction.Complete();
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateCountry: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateCountry: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool DeleteCountry(ref COUNTRy country)
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

                        decimal Id = country.COU_ID;
                        var oCountry = (from r in dbContext.COUNTRies
                                        where r.COU_ID == Id
                                        select r).First();

                        if (oCountry != null)
                        {
                            dbContext.COUNTRies.DeleteOnSubmit(oCountry);
                        }

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        country = null;
                        bRes = true;

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteCountry: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteCountry: ", e);
                bRes = false;
            }

            return bRes;

        }

        public IQueryable<OPERATION> GetOperations(Expression<Func<OPERATION, bool>> predicate)
        {
            IQueryable<OPERATION> res = null;
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
                    res = GetOperations(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperations: ", e);
            }

            return res;
        }
        public IQueryable<OPERATION> GetOperations(Expression<Func<OPERATION, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<OPERATION> res = null;
            try
            {
                res = (from r in dbContext.OPERATIONs
                        select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperations: ", e);
            }

            return res;
        }

        public IQueryable<HIS_OPERATION> GetHisOperations(Expression<Func<HIS_OPERATION, bool>> predicate)
        {
            IQueryable<HIS_OPERATION> res = null;
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
                    res = GetHisOperations(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetHisOperations: ", e);
            }

            return res;
        }
        public IQueryable<HIS_OPERATION> GetHisOperations(Expression<Func<HIS_OPERATION, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<HIS_OPERATION> res = null;
            try
            {
                res = (from r in dbContext.HIS_OPERATIONs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetHisOperations: ", e);
            }

            return res;
        }

        public IQueryable<TICKET_PAYMENT> GetTicketPayments(Expression<Func<TICKET_PAYMENT, bool>> predicate)
        {
            IQueryable<TICKET_PAYMENT> res = null;
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
                    res = GetTicketPayments(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTicketPayments: ", e);
            }

            return res;
        }

        public IQueryable<TICKET_PAYMENT> GetTicketPayments(Expression<Func<TICKET_PAYMENT, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<TICKET_PAYMENT> res = null;
            try
            {
                res = (from r in dbContext.TICKET_PAYMENTs
                        select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetTicketPayments: ", e);
            }

            return res;
        }

        public IQueryable<CUSTOMER_PAYMENT_MEANS_RECHARGE> GetCustomerRecharges(Expression<Func<CUSTOMER_PAYMENT_MEANS_RECHARGE, bool>> predicate)
        {
            IQueryable<CUSTOMER_PAYMENT_MEANS_RECHARGE> res = null;
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
                    res = GetCustomerRecharges(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCustomerRecharges: ", e);
            }

            return res;
        }
        public IQueryable<CUSTOMER_PAYMENT_MEANS_RECHARGE> GetCustomerRecharges(Expression<Func<CUSTOMER_PAYMENT_MEANS_RECHARGE, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<CUSTOMER_PAYMENT_MEANS_RECHARGE> res = null;
            try
            {
                res = (from r in dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs
                        select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetCustomerRecharges: ", e);
            }

            return res;
        }

        public IQueryable<SERVICE_CHARGE> GetServiceCharges(Expression<Func<SERVICE_CHARGE, bool>> predicate)
        {
            IQueryable<SERVICE_CHARGE> res = null;
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
                    res = GetServiceCharges(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetServiceCharges: ", e);
            }

            return res;
        }
        public IQueryable<SERVICE_CHARGE> GetServiceCharges(Expression<Func<SERVICE_CHARGE, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<SERVICE_CHARGE> res = null;
            try
            {
                res = (from r in dbContext.SERVICE_CHARGEs
                        select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetServiceCharges: ", e);
            }

            return res;
        }

        public IQueryable<OPERATIONS_DISCOUNT> GetDiscounts(Expression<Func<OPERATIONS_DISCOUNT, bool>> predicate)
        {
            IQueryable<OPERATIONS_DISCOUNT> res = null;
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
                    res = GetDiscounts(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDiscounts: ", e);
            }

            return res;
        }
        public IQueryable<OPERATIONS_DISCOUNT> GetDiscounts(Expression<Func<OPERATIONS_DISCOUNT, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<OPERATIONS_DISCOUNT> res = null;
            try
            {
                res = (from r in dbContext.OPERATIONS_DISCOUNTs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDiscounts: ", e);
            }

            return res;
        }

        public IQueryable<OPERATIONS_OFFSTREET> GetOperationsOffstreet(Expression<Func<OPERATIONS_OFFSTREET, bool>> predicate)
        {
            IQueryable<OPERATIONS_OFFSTREET> res = null;
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
                    res = GetOperationsOffstreet(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsOffstreet: ", e);
            }

            return res;
        }
        public IQueryable<OPERATIONS_OFFSTREET> GetOperationsOffstreet(Expression<Func<OPERATIONS_OFFSTREET, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<OPERATIONS_OFFSTREET> res = null;
            try
            {
                res = (from r in dbContext.OPERATIONS_OFFSTREETs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetOperationsOffstreet: ", e);
            }

            return res;
        }

        public IQueryable<BALANCE_TRANSFER> GetBalanceTransfers(Expression<Func<BALANCE_TRANSFER, bool>> predicate)
        {
            IQueryable<BALANCE_TRANSFER> res = null;
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
                    res = GetBalanceTransfers(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetBalanceTransfers: ", e);
            }

            return res;
        }
        public IQueryable<BALANCE_TRANSFER> GetBalanceTransfers(Expression<Func<BALANCE_TRANSFER, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<BALANCE_TRANSFER> res = null;
            try
            {
                res = (from r in dbContext.BALANCE_TRANSFERs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetBalanceTransfers: ", e);
            }

            return res;
        }

        public bool DeleteOperation(ChargeOperationsType type, decimal operationId, out object oDeleted, out int iBalanceBefore, out USER oUser, out OPERATIONS_DISCOUNT oDiscountDeleted, out bool bIsHisOperation)
        {            
            oDeleted = null;
            iBalanceBefore = 0;
            oUser = null;
            oDiscountDeleted = null;
            bIsHisOperation = false;
            bool bErrorAccess = false;
            return DeleteOperation(type, operationId, out oDeleted, out iBalanceBefore, out oUser, out oDiscountDeleted, out bIsHisOperation, null, out bErrorAccess);
        }
        public bool DeleteOperation(ChargeOperationsType type, decimal operationId, out object oDeleted, out int iBalanceBefore, out USER oUser, out OPERATIONS_DISCOUNT oDiscountDeleted, out bool bIsHisOperation, List<int> oInstallationsAllowed, out bool bErrorAccess)
        {
            return DeleteOperation(type, operationId, out oDeleted, out iBalanceBefore, out oUser, out oDiscountDeleted, out bIsHisOperation, oInstallationsAllowed, out bErrorAccess, PaymentMeanRechargeStatus.Waiting_Refund);
        }
        public bool DeleteOperation(ChargeOperationsType type, decimal operationId, out object oDeleted, out int iBalanceBefore, out USER oUser, out OPERATIONS_DISCOUNT oDiscountDeleted, out bool bIsHisOperation, List<int> oInstallationsAllowed, out bool bErrorAccess, PaymentMeanRechargeStatus oRechargeStatus)
        {
            bool bRes = false;
            oDeleted = null;
            iBalanceBefore = 0;
            oUser = null;
            oDiscountDeleted = null;
            bIsHisOperation = false;
            bErrorAccess = false;
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

                        decimal dUserId = 0;                        
                        int iAmount = 0;
                        DateTime? oUtcDate = null;

                        if (type == ChargeOperationsType.ParkingOperation ||
                            type == ChargeOperationsType.ExtensionOperation ||
                            type == ChargeOperationsType.ParkingRefund)
                        {

                            var predicateOp = PredicateBuilder.True<OPERATION>();
                            predicateOp = predicateOp.And(o => o.OPE_ID == operationId);
                            var operations = GetOperations(predicateOp, dbContext);
                            if (operations.Count() > 0)
                            {
                                var oOperation = operations.First();
                                if (oInstallationsAllowed != null) {
                                    bErrorAccess = !oInstallationsAllowed.Contains(Convert.ToInt32(oOperation.OPE_INS_ID));
                                }
                                if (!bErrorAccess)
                                {
                                    dUserId = oOperation.OPE_USR_ID;
                                    if (oOperation.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPrepay)
                                    {
                                        if (type != ChargeOperationsType.ParkingRefund)
                                            iAmount = -oOperation.OPE_TOTAL_AMOUNT.Value;
                                        else
                                            iAmount = oOperation.OPE_TOTAL_AMOUNT.Value;
                                        if (oOperation.OPERATIONS_DISCOUNT != null)
                                        {
                                            iAmount += oOperation.OPERATIONS_DISCOUNT.OPEDIS_FINAL_AMOUNT;
                                            if (oOperation.OPERATIONS_DISCOUNT.CUSTOMER_INVOICE != null)
                                                oOperation.OPERATIONS_DISCOUNT.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS += oOperation.OPERATIONS_DISCOUNT.OPEDIS_FINAL_AMOUNT;
                                        }

                                    }
                                    else if (oOperation.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPerTransaction && oOperation.CUSTOMER_PAYMENT_MEANS_RECHARGE != null)
                                    {
                                        dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.DeleteOnSubmit(oOperation.CUSTOMER_PAYMENT_MEANS_RECHARGE);
                                    }

                                    if (oOperation.CUSTOMER_INVOICE != null)
                                    {
                                        if (type != ChargeOperationsType.ParkingRefund)
                                            oOperation.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS -= oOperation.OPE_TOTAL_AMOUNT.Value;
                                        else
                                            oOperation.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS += oOperation.OPE_TOTAL_AMOUNT.Value;
                                    }

                                    oUtcDate = oOperation.OPE_UTC_DATE;
                                    oDeleted = oOperation;
                                    if (oOperation.OPERATIONS_DISCOUNT != null)
                                    {
                                        oDiscountDeleted = oOperation.OPERATIONS_DISCOUNT;
                                        dbContext.OPERATIONS_DISCOUNTs.DeleteOnSubmit(oOperation.OPERATIONS_DISCOUNT);
                                    }
                                    dbContext.OPERATIONs.DeleteOnSubmit(oOperation);
                                }
                            }
                            else
                            {
                                var predicateHisOp = PredicateBuilder.True<HIS_OPERATION>();
                                predicateHisOp = predicateHisOp.And(o => o.OPE_ID == operationId);
                                var hisOperations = GetHisOperations(predicateHisOp, dbContext);
                                if (hisOperations.Count() > 0)
                                {
                                    var oHisOperation = hisOperations.First();
                                    if (oInstallationsAllowed != null) {
                                        bErrorAccess = !oInstallationsAllowed.Contains(Convert.ToInt32(oHisOperation.OPE_INS_ID));
                                    }
                                    if (!bErrorAccess)
                                    {
                                        dUserId = oHisOperation.OPE_USR_ID;
                                        if (oHisOperation.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPrepay)
                                        {
                                            if (type != ChargeOperationsType.ParkingRefund)
                                                iAmount = -oHisOperation.OPE_TOTAL_AMOUNT.Value;
                                            else
                                                iAmount = oHisOperation.OPE_TOTAL_AMOUNT.Value;
                                            if (oHisOperation.OPERATIONS_DISCOUNT != null)
                                            {
                                                iAmount += oHisOperation.OPERATIONS_DISCOUNT.OPEDIS_FINAL_AMOUNT;
                                                if (oHisOperation.OPERATIONS_DISCOUNT.CUSTOMER_INVOICE != null)
                                                    oHisOperation.OPERATIONS_DISCOUNT.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS += oHisOperation.OPERATIONS_DISCOUNT.OPEDIS_FINAL_AMOUNT;
                                            }
                                        }
                                        else if (oHisOperation.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPerTransaction && oHisOperation.CUSTOMER_PAYMENT_MEANS_RECHARGE != null)
                                        {
                                            dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.DeleteOnSubmit(oHisOperation.CUSTOMER_PAYMENT_MEANS_RECHARGE);
                                        }

                                        if (oHisOperation.CUSTOMER_INVOICE != null)
                                        {
                                            if (type != ChargeOperationsType.ParkingRefund)
                                                oHisOperation.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS -= oHisOperation.OPE_TOTAL_AMOUNT.Value;
                                            else
                                                oHisOperation.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS += oHisOperation.OPE_TOTAL_AMOUNT.Value;
                                        }

                                        oUtcDate = oHisOperation.OPE_UTC_DATE;
                                        oDeleted = oHisOperation;
                                        bIsHisOperation = true;
                                        if (oHisOperation.OPERATIONS_DISCOUNT != null)
                                        {
                                            oDiscountDeleted = oHisOperation.OPERATIONS_DISCOUNT;
                                            dbContext.OPERATIONS_DISCOUNTs.DeleteOnSubmit(oHisOperation.OPERATIONS_DISCOUNT);
                                        }
                                        dbContext.HIS_OPERATIONs.DeleteOnSubmit(oHisOperation);
                                    }
                                }
                            }

                        }
                        else if (type == ChargeOperationsType.TicketPayment)
                        {

                            var predicateTicket = PredicateBuilder.True<TICKET_PAYMENT>();
                            predicateTicket = predicateTicket.And(t => t.TIPA_ID == operationId);
                            var tickets = GetTicketPayments(predicateTicket, dbContext);
                            if (tickets.Count() > 0)
                            {
                                var oTicket = tickets.First();
                                if (oInstallationsAllowed != null) {
                                    bErrorAccess = !oInstallationsAllowed.Contains(Convert.ToInt32(oTicket.TIPA_INS_ID));
                                }
                                if (!bErrorAccess)
                                {
                                    dUserId = oTicket.TIPA_USR_ID;
                                    iAmount = -oTicket.TIPA_TOTAL_AMOUNT.Value;
                                    oUtcDate = oTicket.TIPA_UTC_DATE;
                                    oDeleted = oTicket;
                                    if (oTicket.CUSTOMER_INVOICE != null)
                                        oTicket.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS -= oTicket.TIPA_TOTAL_AMOUNT.Value;
                                    dbContext.TICKET_PAYMENTs.DeleteOnSubmit(oTicket);
                                }
                            }

                        }
                        else if (type == ChargeOperationsType.BalanceRecharge)
                        {

                            var predicateRecharge = PredicateBuilder.True<CUSTOMER_PAYMENT_MEANS_RECHARGE>();
                            predicateRecharge = predicateRecharge.And(t => t.CUSPMR_ID == operationId);
                            var recharges = GetCustomerRecharges(predicateRecharge, dbContext);
                            if (recharges.Count() > 0)
                            {
                                var oRecharge = recharges.First();                                
                                dUserId = oRecharge.CUSPMR_USR_ID.Value;
                                iAmount = oRecharge.CUSPMR_AMOUNT;
                                oUtcDate = oRecharge.CUSPMR_UTC_DATE;
                                oDeleted = oRecharge;
                                if (oRecharge.CUSTOMER_INVOICE != null)
                                    oRecharge.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT -= Convert.ToInt32(oRecharge.CUSPMR_TOTAL_AMOUNT_CHARGED);
                                oRecharge.CUSPMR_TRANS_STATUS = (int)oRechargeStatus;                                
                            }

                        }
                        else if (type == ChargeOperationsType.ServiceCharge)
                        {

                            var predicateService = PredicateBuilder.True<SERVICE_CHARGE>();
                            predicateService = predicateService.And(t => t.SECH_ID == operationId);
                            var services = GetServiceCharges(predicateService, dbContext);
                            if (services.Count() > 0)
                            {
                                var oService = services.First();                                
                                dUserId = oService.SECH_USR_ID;
                                if (oService.SECH_SUSCRIPTION_TYPE == (int) PaymentSuscryptionType.pstPrepay)
                                {
                                    iAmount = -oService.SECH_FINAL_AMOUNT;
                                    if (oService.CUSTOMER_INVOICE != null)
                                        oService.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS -= oService.SECH_FINAL_AMOUNT;
                                }
                                else if (oService.SECH_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPerTransaction && oService.CUSTOMER_PAYMENT_MEANS_RECHARGE != null)
                                {
                                    dbContext.CUSTOMER_PAYMENT_MEANS_RECHARGEs.DeleteOnSubmit(oService.CUSTOMER_PAYMENT_MEANS_RECHARGE);
                                }
                                oUtcDate = oService.SECH_UTC_DATE;
                                oDeleted = oService;
                                dbContext.SERVICE_CHARGEs.DeleteOnSubmit(oService);
                            }

                        }
                        else if (type == ChargeOperationsType.Discount)
                        {
                            var predicateDiscount = PredicateBuilder.True<OPERATIONS_DISCOUNT>();
                            predicateDiscount = predicateDiscount.And(t => t.OPEDIS_ID == operationId);
                            var discounts = GetDiscounts(predicateDiscount, dbContext);
                            if (discounts.Count() > 0)
                            {
                                var oDiscount = discounts.First();                                
                                dUserId = oDiscount.OPEDIS_USR_ID;
                                iAmount = oDiscount.OPEDIS_FINAL_AMOUNT;
                                oUtcDate = oDiscount.OPEDIS_UTC_DATE;
                                if (oDiscount.CUSTOMER_INVOICE != null)
                                    oDiscount.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS += oDiscount.OPEDIS_FINAL_AMOUNT;

                                if (oDiscount.OPERATIONs != null)
                                {
                                    foreach (OPERATION oOperationRelated in oDiscount.OPERATIONs)
                                    {
                                        oOperationRelated.OPE_OPEDIS_ID = null;
                                    }
                                }
                                oDeleted = oDiscount;
                                dbContext.OPERATIONS_DISCOUNTs.DeleteOnSubmit(oDiscount);
                            }
                        }
                        else if (type == ChargeOperationsType.OffstreetEntry ||
                                 type == ChargeOperationsType.OffstreetExit ||
                                 type == ChargeOperationsType.OffstreetOverduePayment)
                        {
                            var predicateOffstreet = PredicateBuilder.True<OPERATIONS_OFFSTREET>();
                            predicateOffstreet = predicateOffstreet.And(t => t.OPEOFF_ID == operationId);
                            var operations = GetOperationsOffstreet(predicateOffstreet, dbContext);
                            if (operations.Count() > 0)
                            {
                                var oOperationOffstreet = operations.First();
                                if (oInstallationsAllowed != null) {
                                    bErrorAccess = !oInstallationsAllowed.Contains(Convert.ToInt32(oOperationOffstreet.OPEOFF_INS_ID));
                                }
                                if (!bErrorAccess)
                                {
                                    dUserId = oOperationOffstreet.OPEOFF_USR_ID;
                                    iAmount = -oOperationOffstreet.OPEOFF_FINAL_AMOUNT;
                                    if (oOperationOffstreet.CUSTOMER_INVOICE != null)
                                        oOperationOffstreet.CUSTOMER_INVOICE.CUSINV_INV_AMOUNT_OPS -= oOperationOffstreet.OPEOFF_FINAL_AMOUNT;

                                    if (type == ChargeOperationsType.OffstreetEntry)
                                        oUtcDate = oOperationOffstreet.OPEOFF_UTC_NOTIFY_ENTRY_DATE;
                                    else
                                        oUtcDate = oOperationOffstreet.OPEOFF_UTC_PAYMENT_DATE;
                                    oDeleted = oOperationOffstreet;
                                    dbContext.OPERATIONS_OFFSTREETs.DeleteOnSubmit(oOperationOffstreet);
                                }
                            }
                        }
                        else if (type == ChargeOperationsType.BalanceReception ||
                                 type == ChargeOperationsType.BalanceTransfer)
                        {
                            bErrorAccess = true;
                        }

                        if (!bErrorAccess)
                        {
                            if (iAmount != 0)
                            {

                                var predicate = PredicateBuilder.True<ALL_OPERATIONS_EXT>();
                                predicate = predicate.And(o => o.OPE_USR_ID == dUserId && o.OPE_UTC_DATE > oUtcDate);
                                var operationsExt = GetOperationsExt(predicate, dbContext).OrderBy(o => o.OPE_UTC_DATE);

                                foreach (ALL_OPERATIONS_EXT oOperationExt in operationsExt)
                                {
                                    if (oOperationExt.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPrepay)
                                    {
                                        if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ParkingOperation ||
                                            (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ExtensionOperation ||
                                            (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ParkingRefund)
                                        {
                                            var predicateOp = PredicateBuilder.True<OPERATION>();
                                            predicateOp = predicateOp.And(o => o.OPE_ID == oOperationExt.OPE_ID);
                                            var operations = GetOperations(predicateOp, dbContext);
                                            if (operations.Count() > 0)
                                            {
                                                var oOperation = operations.First();
                                                oOperation.OPE_BALANCE_BEFORE -= iAmount;
                                            }
                                            else
                                            {
                                                var predicateHisOp = PredicateBuilder.True<HIS_OPERATION>();
                                                predicateHisOp = predicateHisOp.And(o => o.OPE_ID == oOperationExt.OPE_ID);
                                                var hisOperations = GetHisOperations(predicateHisOp, dbContext);
                                                if (hisOperations.Count() > 0)
                                                {
                                                    var oHisOperation = hisOperations.First();
                                                    oHisOperation.OPE_BALANCE_BEFORE -= iAmount;
                                                }
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.TicketPayment)
                                        {
                                            var predicateTicket = PredicateBuilder.True<TICKET_PAYMENT>();
                                            predicateTicket = predicateTicket.And(t => t.TIPA_ID == oOperationExt.OPE_ID);
                                            var tickets = GetTicketPayments(predicateTicket, dbContext);
                                            if (tickets.Count() > 0)
                                            {
                                                var oTicket = tickets.First();
                                                oTicket.TIPA_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.BalanceRecharge)
                                        {
                                            var predicateRecharge = PredicateBuilder.True<CUSTOMER_PAYMENT_MEANS_RECHARGE>();
                                            predicateRecharge = predicateRecharge.And(t => t.CUSPMR_ID == oOperationExt.OPE_ID);
                                            var recharges = GetCustomerRecharges(predicateRecharge, dbContext);
                                            if (recharges.Count() > 0)
                                            {
                                                var oRecharge = recharges.First();
                                                oRecharge.CUSPMR_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ServiceCharge)
                                        {
                                            var predicateService = PredicateBuilder.True<SERVICE_CHARGE>();
                                            predicateService = predicateService.And(t => t.SECH_ID == oOperationExt.OPE_ID);
                                            var services = GetServiceCharges(predicateService, dbContext);
                                            if (services.Count() > 0)
                                            {
                                                var oService = services.First();
                                                oService.SECH_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.Discount)
                                        {
                                            var predicateDiscount = PredicateBuilder.True<OPERATIONS_DISCOUNT>();
                                            predicateDiscount = predicateDiscount.And(t => t.OPEDIS_ID == oOperationExt.OPE_ID);
                                            var discounts = GetDiscounts(predicateDiscount, dbContext);
                                            if (discounts.Count() > 0)
                                            {
                                                var oDiscount = discounts.First();
                                                oDiscount.OPEDIS_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetEntry ||
                                                 (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetExit ||
                                                 (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetOverduePayment)
                                        {
                                            var predicateOffstreet = PredicateBuilder.True<OPERATIONS_OFFSTREET>();
                                            predicateOffstreet = predicateOffstreet.And(t => t.OPEOFF_ID == oOperationExt.OPE_ID);
                                            var operations = GetOperationsOffstreet(predicateOffstreet, dbContext);
                                            if (operations.Count() > 0)
                                            {
                                                var oOperationOffstreet = operations.First();
                                                oOperationOffstreet.OPEOFF_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.BalanceTransfer)
                                        {
                                            var predicateTransfer = PredicateBuilder.True<BALANCE_TRANSFER>();
                                            predicateTransfer = predicateTransfer.And(t => t.BAT_ID == oOperationExt.OPE_ID);
                                            var transfers = GetBalanceTransfers(predicateTransfer, dbContext);
                                            if (transfers.Count() > 0)
                                            {
                                                var oRecharge = transfers.First();
                                                oRecharge.BAT_SRC_BALANCE_BEFORE -= iAmount;
                                            }
                                        }
                                        else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.BalanceReception)
                                        {
                                            var predicateReception = PredicateBuilder.True<BALANCE_TRANSFER>();
                                            predicateReception = predicateReception.And(t => t.BAT_ID == oOperationExt.OPE_ID);
                                            var transfers = GetBalanceTransfers(predicateReception, dbContext);
                                            if (transfers.Count() > 0)
                                            {
                                                var oRecharge = transfers.First();
                                                oRecharge.BAT_DST_BALANCE_BEFORE -= iAmount;
                                            }
                                        }

                                    }
                                }

                            }

                            // Update user balance
                            var predicateUser = PredicateBuilder.True<USER>();
                            predicateUser = predicateUser.And(t => t.USR_ID == dUserId);
                            oUser = GetUsers(predicateUser, dbContext).First();
                            iBalanceBefore = oUser.USR_BALANCE;
                            if (iAmount != 0)
                                ModifyUserBalance(ref oUser, -iAmount);

                            SecureSubmitChanges(ref dbContext);

                            transaction.Complete();

                            bRes = true;
                        }

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteOperation: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteOperation: ", e);
                bRes = false;
            }

            return bRes;
        }

        private bool ModifyUserBalance(ref USER oUser, int iAmount)
        {
            bool bRes = true;
            try
            {
                oUser.USR_BALANCE += iAmount;

                if (iAmount < 0)
                {
                    oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBalance: ", e);
                bRes = false;
            }

            return bRes;

        }


        private bool SetUserBalance(ref USER oUser, int iUserBalance)
        {
            bool bRes = true;
            try
            {
                if (oUser.USR_BALANCE > iUserBalance)
                {
                    oUser.USR_LAST_BALANCE_USE = DateTime.UtcNow;
                }
                
                oUser.USR_BALANCE = iUserBalance;
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ModifyUserBalance: ", e);
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

        public IQueryable<VW_OPERATIONS_HOUR> GetVwOperationsHour(Expression<Func<VW_OPERATIONS_HOUR, bool>> predicate)
        {
            IQueryable<VW_OPERATIONS_HOUR> res = null;
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
                    res = GetVwOperationsHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsHour: ", e);
            }

            return res;
        }
        public IQueryable<VW_OPERATIONS_HOUR> GetVwOperationsHour(Expression<Func<VW_OPERATIONS_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_OPERATIONS_HOUR> res = null;
            try
            {
                res = (from r in dbContext.VW_OPERATIONS_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsHour: ", e);
            }

            return res;
        }

        public IQueryable<VW_RECHARGES_HOUR> GetVwRechargesHour(Expression<Func<VW_RECHARGES_HOUR, bool>> predicate)
        {
            IQueryable<VW_RECHARGES_HOUR> res = null;
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
                    dbContext.CommandTimeout = 3 * 60;
                    res = GetVwRechargesHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwRechargesHour: ", e);
            }

            return res;
        }
        public IQueryable<VW_RECHARGES_HOUR> GetVwRechargesHour(Expression<Func<VW_RECHARGES_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_RECHARGES_HOUR> res = null;
            try
            {
                res = (from r in dbContext.VW_RECHARGES_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwRechargesHour: ", e);
            }

            return res;
        }

        public IQueryable<VW_INSCRIPTIONS_HOUR> GetVwInscriptionsHour(Expression<Func<VW_INSCRIPTIONS_HOUR, bool>> predicate)
        {
            IQueryable<VW_INSCRIPTIONS_HOUR> res = null;
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
                    res = GetVwInscriptionsHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwInscriptionsHour: ", e);
            }

            return res;
        }
        public IQueryable<VW_INSCRIPTIONS_HOUR> GetVwInscriptionsHour(Expression<Func<VW_INSCRIPTIONS_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_INSCRIPTIONS_HOUR> res = null;
            try
            {
                res = (from r in dbContext.VW_INSCRIPTIONS_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwInscriptionsHour: ", e);
            }

            return res;
        }

        public IQueryable<VW_INSCRIPTIONS_PLATFORM_HOUR> GetVwInscriptionsPlatformHour(Expression<Func<VW_INSCRIPTIONS_PLATFORM_HOUR, bool>> predicate)
        {
            IQueryable<VW_INSCRIPTIONS_PLATFORM_HOUR> res = null;
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
                    res = GetVwInscriptionsPlatformHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwInscriptionsPlatformHour: ", e);
            }

            return res;
        }
        public IQueryable<VW_INSCRIPTIONS_PLATFORM_HOUR> GetVwInscriptionsPlatformHour(Expression<Func<VW_INSCRIPTIONS_PLATFORM_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_INSCRIPTIONS_PLATFORM_HOUR> res = null;
            try
            {
                res = (from r in dbContext.VW_INSCRIPTIONS_PLATFORM_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwInscriptionsPlatformHour: ", e);
            }

            return res;
        }

        public IQueryable<VW_OPERATIONS_USER_HOUR> GetVwOperationsUserHour(Expression<Func<VW_OPERATIONS_USER_HOUR, bool>> predicate)
        {
            IQueryable<VW_OPERATIONS_USER_HOUR> res = null;
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
                    res = GetVwOperationsUserHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsUserHour: ", e);
            }

            return res;
        }
        public IQueryable<VW_OPERATIONS_USER_HOUR> GetVwOperationsUserHour(Expression<Func<VW_OPERATIONS_USER_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_OPERATIONS_USER_HOUR> res = null;
            try
            {
                res = (from r in dbContext.VW_OPERATIONS_USER_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsUserHour: ", e);
            }

            return res;
        }

        public IQueryable<VW_OPERATIONS_MINUTE> GetVwOperationsMinute(Expression<Func<VW_OPERATIONS_MINUTE, bool>> predicate)
        {
            IQueryable<VW_OPERATIONS_MINUTE> res = null;
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
                    dbContext.CommandTimeout = 3 * 60;
                    res = GetVwOperationsMinute(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsMinute: ", e);
            }

            return res;
        }
        public IQueryable<VW_OPERATIONS_MINUTE> GetVwOperationsMinute(Expression<Func<VW_OPERATIONS_MINUTE, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<VW_OPERATIONS_MINUTE> res = null;
            try
            {
                res = (from r in dbContext.VW_OPERATIONS_MINUTEs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetVwOperationsMinute: ", e);
            }

            return res;
        }

        public IQueryable<DB_OPERATIONS_HOUR> GetDbOperationsHour(Expression<Func<DB_OPERATIONS_HOUR, bool>> predicate)
        {
            IQueryable<DB_OPERATIONS_HOUR> res = null;
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
                    res = GetDbOperationsHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsHour: ", e);
            }

            return res;
        }
        public IQueryable<DB_OPERATIONS_HOUR> GetDbOperationsHour(Expression<Func<DB_OPERATIONS_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<DB_OPERATIONS_HOUR> res = null;
            try
            {
                res = (from r in dbContext.DB_OPERATIONS_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsHour: ", e);
            }

            return res;
        }

        public IQueryable<DB_RECHARGES_HOUR> GetDbRechargesHour(Expression<Func<DB_RECHARGES_HOUR, bool>> predicate)
        {
            IQueryable<DB_RECHARGES_HOUR> res = null;
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
                    dbContext.CommandTimeout = 3 * 60;
                    res = GetDbRechargesHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbRechargesHour: ", e);
            }

            return res;
        }
        public IQueryable<DB_RECHARGES_HOUR> GetDbRechargesHour(Expression<Func<DB_RECHARGES_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<DB_RECHARGES_HOUR> res = null;
            try
            {
                res = (from r in dbContext.DB_RECHARGES_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbRechargesHour: ", e);
            }

            return res;
        }

        public IQueryable<DB_OPERATIONS_USERS_HOUR> GetDbOperationsUsersHour(Expression<Func<DB_OPERATIONS_USERS_HOUR, bool>> predicate)
        {
            IQueryable<DB_OPERATIONS_USERS_HOUR> res = null;
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
                    dbContext.CommandTimeout = 3 * 60;
                    res = GetDbOperationsUsersHour(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsUsersHour: ", e);
            }

            return res;
        }
        public IQueryable<DB_OPERATIONS_USERS_HOUR> GetDbOperationsUsersHour(Expression<Func<DB_OPERATIONS_USERS_HOUR, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<DB_OPERATIONS_USERS_HOUR> res = null;
            try
            {
                res = (from r in dbContext.DB_OPERATIONS_USERS_HOURs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsUsersHour: ", e);
            }

            return res;
        }

        public IQueryable<DB_OPERATIONS_MINUTE> GetDbOperationsMinute(Expression<Func<DB_OPERATIONS_MINUTE, bool>> predicate)
        {
            IQueryable<DB_OPERATIONS_MINUTE> res = null;
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
                    dbContext.CommandTimeout = 3 * 60;
                    res = GetDbOperationsMinute(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsMinute: ", e);
            }

            return res;
        }
        public IQueryable<DB_OPERATIONS_MINUTE> GetDbOperationsMinute(Expression<Func<DB_OPERATIONS_MINUTE, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<DB_OPERATIONS_MINUTE> res = null;
            try
            {
                res = (from r in dbContext.DB_OPERATIONS_MINUTEs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbOperationsMinute: ", e);
            }

            return res;
        }

        public IQueryable<Select_DB_INVITATIONS_HOURResult> GetDbInvitationsHour(DateTime dtBegin, DateTime dtEnd, Expression<Func<Select_DB_INVITATIONS_HOURResult, bool>> predicate)
        {
            IQueryable<Select_DB_INVITATIONS_HOURResult> res = null;
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
                    res = GetDbInvitationsHour(dtBegin, dtEnd, predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbInvitationsHour: ", e);
            }

            return res;
        }
        public IQueryable<Select_DB_INVITATIONS_HOURResult> GetDbInvitationsHour(DateTime dtBegin, DateTime dtEnd, Expression<Func<Select_DB_INVITATIONS_HOURResult, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<Select_DB_INVITATIONS_HOURResult> res = null;
            try
            {
                res = dbContext.Select_DB_INVITATIONS_HOUR(dtBegin, dtEnd).AsQueryable().Where(predicate);
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbInvitationsHour: ", e);
            }

            return res;
        }

        public IQueryable<Select_DB_RECHARGE_COUPONS_HOURResult> GetDbRechargeCouponsHour(DateTime dtBegin, DateTime dtEnd, Expression<Func<Select_DB_RECHARGE_COUPONS_HOURResult, bool>> predicate)
        {
            IQueryable<Select_DB_RECHARGE_COUPONS_HOURResult> res = null;
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
                    res = GetDbRechargeCouponsHour(dtBegin, dtEnd, predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbRechargeCouponsHour: ", e);
            }

            return res;
        }
        public IQueryable<Select_DB_RECHARGE_COUPONS_HOURResult> GetDbRechargeCouponsHour(DateTime dtBegin, DateTime dtEnd, Expression<Func<Select_DB_RECHARGE_COUPONS_HOURResult, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<Select_DB_RECHARGE_COUPONS_HOURResult> res = null;
            try
            {
                res = dbContext.Select_DB_RECHARGE_COUPONS_HOUR(dtBegin, dtEnd).AsQueryable().Where(predicate);
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetDbRechargeCouponsHour: ", e);
            }

            return res;
        }

        public bool RecalculateUserBalance(decimal dUserId, out USER oUser, decimal? operationId = null, ChargeOperationsType? operationType = null)
        {
            bool bRes = false;
            oUser = null;
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

                    int iAmount = 0;
                    int iBalance = 0;
                    DateTime? oInsertionUtcDate = null;

                    ALL_OPERATIONS_EXT oFirstOperation = null;

                    if (!operationId.HasValue)
                    {
                        var predicate = PredicateBuilder.True<ALL_OPERATIONS_EXT>();
                        predicate = predicate.And(o => o.OPE_USR_ID == dUserId);
                        var operationsExt = GetOperationsExt(predicate, dbContext).OrderBy(o => o.OPE_INSERTION_UTC_DATE);
                        if (operationsExt.Count() > 0)
                            oFirstOperation = operationsExt.First();
                    }
                    else
                    {
                        var predicate = PredicateBuilder.True<ALL_OPERATIONS_EXT>();
                        predicate = predicate.And(o => o.OPE_USR_ID == dUserId && o.OPE_ID == operationId && o.OPE_TYPE == (int)operationType);
                        var operationsExt = GetOperationsExt(predicate, dbContext);
                        if (operationsExt.Count() == 1)
                            oFirstOperation = operationsExt.First();
                    }

                    if (oFirstOperation != null)
                    {
                        if (oFirstOperation.OPE_TYPE != (int) ChargeOperationsType.BalanceRecharge)
                            iAmount = oFirstOperation.OPE_FINAL_AMOUNT ?? 0;
                        else
                            iAmount = oFirstOperation.OPE_AMOUNT ?? 0;
                        iBalance = oFirstOperation.OPE_BALANCE_BEFORE;
                        oInsertionUtcDate = oFirstOperation.OPE_INSERTION_UTC_DATE;

                        var predicate = PredicateBuilder.True<ALL_OPERATIONS_EXT>();
                        predicate = predicate.And(o => o.OPE_USR_ID == dUserId && o.OPE_INSERTION_UTC_DATE > oInsertionUtcDate);
                        var operationsExt = GetOperationsExt(predicate, dbContext).OrderBy(o => o.OPE_INSERTION_UTC_DATE);

                        foreach (ALL_OPERATIONS_EXT oOperationExt in operationsExt)
                        {
                            if (oOperationExt.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPrepay)
                            {
                                if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ParkingOperation ||
                                    (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ExtensionOperation ||
                                    (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ParkingRefund)
                                {
                                    var predicateOp = PredicateBuilder.True<OPERATION>();
                                    predicateOp = predicateOp.And(o => o.OPE_ID == oOperationExt.OPE_ID);
                                    var operations = GetOperations(predicateOp, dbContext);
                                    if (operations.Count() > 0)
                                    {
                                        var oOperation = operations.First();
                                        if (oOperation.OPE_SUSCRIPTION_TYPE == (int)PaymentSuscryptionType.pstPrepay)
                                        {
                                            oOperation.OPE_BALANCE_BEFORE = iBalance + iAmount;
                                            if ((ChargeOperationsType)oOperationExt.OPE_TYPE != ChargeOperationsType.ParkingRefund)
                                                iAmount = -oOperation.OPE_FINAL_AMOUNT;
                                            else
                                                iAmount = oOperation.OPE_FINAL_AMOUNT;
                                            if (oOperation.OPERATIONS_DISCOUNT != null)
                                                iAmount += oOperation.OPERATIONS_DISCOUNT.OPEDIS_FINAL_AMOUNT;
                                            iBalance = oOperation.OPE_BALANCE_BEFORE;
                                        }
                                    }
                                }
                                else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.TicketPayment)
                                {
                                    var predicateTicket = PredicateBuilder.True<TICKET_PAYMENT>();
                                    predicateTicket = predicateTicket.And(t => t.TIPA_ID == oOperationExt.OPE_ID);
                                    var tickets = GetTicketPayments(predicateTicket, dbContext);
                                    if (tickets.Count() > 0)
                                    {
                                        var oTicket = tickets.First();
                                        oTicket.TIPA_BALANCE_BEFORE = iBalance + iAmount;
                                        iAmount = -oTicket.TIPA_FINAL_AMOUNT;
                                        iBalance = oTicket.TIPA_BALANCE_BEFORE;
                                    }
                                }
                                else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.BalanceRecharge)
                                {
                                    var predicateRecharge = PredicateBuilder.True<CUSTOMER_PAYMENT_MEANS_RECHARGE>();
                                    predicateRecharge = predicateRecharge.And(t => t.CUSPMR_ID == oOperationExt.OPE_ID);
                                    var recharges = GetCustomerRecharges(predicateRecharge, dbContext);
                                    if (recharges.Count() > 0)
                                    {
                                        var oRecharge = recharges.First();
                                        oRecharge.CUSPMR_BALANCE_BEFORE = iBalance + iAmount;
                                        iAmount = oRecharge.CUSPMR_AMOUNT;
                                        iBalance = oRecharge.CUSPMR_BALANCE_BEFORE;
                                    }
                                }
                                else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.ServiceCharge)
                                {
                                    var predicateService = PredicateBuilder.True<SERVICE_CHARGE>();
                                    predicateService = predicateService.And(t => t.SECH_ID == oOperationExt.OPE_ID);
                                    var services = GetServiceCharges(predicateService, dbContext);
                                    if (services.Count() > 0)
                                    {
                                        var oService = services.First();
                                        oService.SECH_BALANCE_BEFORE = iBalance + iAmount;
                                        iAmount = -oService.SECH_FINAL_AMOUNT;
                                        iBalance = oService.SECH_BALANCE_BEFORE;
                                    }
                                }
                                else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.Discount)
                                {
                                    var predicateDiscount = PredicateBuilder.True<OPERATIONS_DISCOUNT>();
                                    predicateDiscount = predicateDiscount.And(t => t.OPEDIS_ID == oOperationExt.OPE_ID);
                                    var discounts = GetDiscounts(predicateDiscount, dbContext);
                                    if (discounts.Count() > 0)
                                    {
                                        var oDiscount = discounts.First();
                                        oDiscount.OPEDIS_BALANCE_BEFORE = iBalance + iAmount;
                                        iAmount = oDiscount.OPEDIS_FINAL_AMOUNT;
                                        iBalance = oDiscount.OPEDIS_BALANCE_BEFORE;
                                    }
                                }
                                else if ((ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetEntry ||
                                         (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetExit ||
                                         (ChargeOperationsType)oOperationExt.OPE_TYPE == ChargeOperationsType.OffstreetOverduePayment)
                                {
                                    var predicateOffstreet = PredicateBuilder.True<OPERATIONS_OFFSTREET>();
                                    predicateOffstreet = predicateOffstreet.And(t => t.OPEOFF_ID == oOperationExt.OPE_ID);
                                    var operations = GetOperationsOffstreet(predicateOffstreet, dbContext);
                                    if (operations.Count() > 0)
                                    {
                                        var oOperationOffstreet = operations.First();
                                        oOperationOffstreet.OPEOFF_BALANCE_BEFORE = iBalance + iAmount;
                                        iAmount = -oOperationOffstreet.OPEOFF_FINAL_AMOUNT;
                                        iBalance = oOperationOffstreet.OPEOFF_BALANCE_BEFORE;
                                    }
                                }

                            }
                        }

                        // Update user balance
                        var predicateUser = PredicateBuilder.True<USER>();
                        predicateUser = predicateUser.And(t => t.USR_ID == dUserId);
                        oUser = GetUsers(predicateUser, dbContext).First();
                        SetUserBalance(ref oUser, iBalance + iAmount);

                        SecureSubmitChanges(ref dbContext);

                        transaction.Complete();

                        bRes = true;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RecalculateUserBalance: ", e);
                bRes = false;
            }

            return bRes;
        }

        public IQueryable<EMAILTOOL_RECIPIENT> GetEmailToolRecipients(Expression<Func<EMAILTOOL_RECIPIENT, bool>> predicate)
        {
            IQueryable<EMAILTOOL_RECIPIENT> res = null;
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
                    res = GetEmailToolRecipients(predicate, dbContext);
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetEmailToolRecipients: ", e);
            }

            return res;
        }
        public IQueryable<EMAILTOOL_RECIPIENT> GetEmailToolRecipients(Expression<Func<EMAILTOOL_RECIPIENT, bool>> predicate, integraMobileDBEntitiesDataContext dbContext)
        {
            IQueryable<EMAILTOOL_RECIPIENT> res = null;
            try
            {
                res = (from r in dbContext.EMAILTOOL_RECIPIENTs
                       select r)
                        .Where(predicate)
                        .AsQueryable();
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetEmailToolRecipients: ", e);
            }

            return res;
        }

        public bool AddEmailToolRecipient(long dId, string sEmail)
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

                        if (dbContext.EMAILTOOL_RECIPIENTs.Where(e => e.ETR_ID == dId && e.ETR_EMAIL == sEmail).FirstOrDefault() == null)
                        {
                            dbContext.EMAILTOOL_RECIPIENTs.InsertOnSubmit(new EMAILTOOL_RECIPIENT()
                            {
                                ETR_ID = dId,
                                ETR_EMAIL = sEmail
                            });

                            // Submit the change to the database.
                            try
                            {
                                SecureSubmitChanges(ref dbContext);
                                transaction.Complete();

                            }
                            catch (Exception e)
                            {
                                m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipient: ", e);
                                bRes = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipient: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipient: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool AddEmailToolRecipients(long dId, string[] oEmails)
        {
            bool bRes = true;
            try
            {
                /*using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {*/
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                        //List<string> oExistingRecipients = dbContext.EMAILTOOL_RECIPIENTs.Where(e => e.ETR_ID == dId && oEmails.Contains(e.ETR_EMAIL)).Select(e => e.ETR_EMAIL).ToList();
                        //dbContext.EMAILTOOL_RECIPIENTs.InsertAllOnSubmit(oEmails.Where(email => !oExistingRecipients.Contains(email)).Select(email => new EMAILTOOL_RECIPIENT() { ETR_ID = dId, ETR_EMAIL = email }));

                        //using (dbContext.Connection)
                        //{

                            System.Data.Common.DbCommand oCommand = dbContext.Connection.CreateCommand();
                            oCommand.CommandType = System.Data.CommandType.Text;
                            int iCount;

                            if (dbContext.Connection.State != System.Data.ConnectionState.Open) dbContext.Connection.Open();


                            var oRecipients = oEmails.ToList();

                            string sSQL = "INSERT INTO EMAILTOOL_RECIPIENTS (ETR_ID, ETR_EMAIL)" +
                                          "SELECT DISTINCT {0}, USR_EMAIL " +
                                          "FROM USERS " +
                                          "WHERE USR_EMAIL IN ({1})";

                            int iBlockSize = 1500;
                            int iBlocks = oRecipients.Count / iBlockSize;
                            string sEmails = "";
                            string sEmailsIn = "";
                            for (int i = 0; i < iBlocks; i++)
                            {
                                sEmails = "";
                                sEmailsIn = "";
                                for (int j = 0; j < iBlockSize; j++)
                                {
                                    sEmails += ";" + oRecipients[(i * iBlockSize) + j];
                                    sEmailsIn += ",'" + oRecipients[(i * iBlockSize) + j] + "'";
                                }
                                if (sEmails.Length > 0) sEmails = sEmails.Substring(1);
                                if (sEmailsIn.Length > 0) sEmailsIn = sEmailsIn.Substring(1);
                                //dbContext.EmailTool_AddRecipient(dId, sEmails);
                                //dbContext.ExecuteCommand(sSQL, dId, sEmailsIn);
                                oCommand.CommandText = string.Format(sSQL, dId, sEmailsIn);
                                iCount = oCommand.ExecuteNonQuery();
                            }
                            if (oRecipients.Count > (iBlocks * iBlockSize))
                            {
                                sEmails = "";
                                sEmailsIn = "";
                                for (int j = 0; j < oRecipients.Count - (iBlocks * iBlockSize); j++)
                                {
                                    sEmails += ";" + oRecipients[(iBlocks * iBlockSize) + j];
                                    sEmailsIn += ",'" + oRecipients[(iBlocks * iBlockSize) + j] + "'";
                                }
                                if (sEmails.Length > 0) sEmails = sEmails.Substring(1);
                                if (sEmailsIn.Length > 0) sEmailsIn = sEmailsIn.Substring(1);
                                //dbContext.EmailTool_AddRecipient(dId, sEmails);
                                //int i = dbContext.ExecuteCommand(sSQL, dId, sEmailsIn);                            
                                oCommand.CommandText = string.Format(sSQL, dId, sEmailsIn);
                                iCount = oCommand.ExecuteNonQuery();

                            }

                        //}

                        //foreach (string sEmail in oEmails) {
                        //    dbContext.EmailTool_AddRecipient(dId, sEmail);
                        //}

                        // Submit the change to the database.
                        /*try
                        {
                            SecureSubmitChanges(ref dbContext);
                            //transaction.Complete();

                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipients: ", e);
                            bRes = false;
                        }*/                        
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipients: ", e);
                        bRes = false;
                    }
                //}
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "AddEmailToolRecipients: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool DeleteEmailToolRecipient(long dId, string sEmail)
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

                        var oRecipient = (from r in dbContext.EMAILTOOL_RECIPIENTs
                                          where r.ETR_ID == dId && r.ETR_EMAIL == sEmail
                                          select r).First();

                        if (oRecipient != null)
                        {

                            dbContext.EMAILTOOL_RECIPIENTs.DeleteOnSubmit(oRecipient);
                        }

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();                            
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipient: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipient: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipient: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool DeleteAllEmailToolRecipients(long dId)
        {
            bool bRes = true;

            try
            {
                /*using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                           new TransactionOptions()
                                                                           {
                                                                               IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                               Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                           }))
                {*/
                    try
                    {
                        integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                       /* dbContext.EMAILTOOL_RECIPIENTs.DeleteAllOnSubmit<EMAILTOOL_RECIPIENT>(dbContext.EMAILTOOL_RECIPIENTs.Where(r => r.ETR_ID == dId));

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteAllEmailToolRecipients: ", e);
                            bRes = false;
                        }*/

                        System.Data.Common.DbCommand oCommand = dbContext.Connection.CreateCommand();
                        oCommand.CommandType = System.Data.CommandType.Text;
                        int iCount;

                        if (dbContext.Connection.State != System.Data.ConnectionState.Open) dbContext.Connection.Open();
                        
                        string sSQL = "DELETE " +                                      
                                      "FROM EMAILTOOL_RECIPIENTS " +
                                      "WHERE ETR_ID = {0}";

                        oCommand.CommandText = string.Format(sSQL, dId);
                        iCount = oCommand.ExecuteNonQuery();

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteAllEmailToolRecipients: ", e);
                        bRes = false;
                    }
                //}
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteAllEmailToolRecipients: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool DeleteEmailToolRecipients(Expression<Func<EMAILTOOL_RECIPIENT, bool>> predicate)
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

                        dbContext.EMAILTOOL_RECIPIENTs.DeleteAllOnSubmit<EMAILTOOL_RECIPIENT>(dbContext.EMAILTOOL_RECIPIENTs.Where(predicate));

                        // Submit the change to the database.
                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipients: ", e);
                            bRes = false;
                        }
                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipients: ", e);
                        bRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "DeleteEmailToolRecipients: ", e);
                bRes = false;
            }
            return bRes;

        }

        public bool UpdateZoneConfiguration(GROUP oDataGroup, GROUPS_HIERARCHY oDataHierarchy, GROUPS_TYPES_ASSIGNATION oDataGroupTypeAssignation, List<GROUPS_TARIFFS_EXTERNAL_TRANSLATION> oDataTranslations, List<string> oExtSubGrpsIds, List<GROUPS_GEOMETRY> oDataGeometry)
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

                        INSTALLATION oInstallation = dbContext.INSTALLATIONs.FirstOrDefault();
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallation.INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        DateTime dtInstallationNow = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);

                        GROUP oGroup = null;

                        var oGroupsByExtIds = dbContext.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs.Where(gt => oExtSubGrpsIds.Contains(gt.GTET_OUT_GRP_EXT_ID))
                                                                    .Select(gt => gt.GROUP)
                                                                    .Distinct();
                        if (oGroupsByExtIds.Count() == 1)
                        {
                            oGroup = oGroupsByExtIds.First();
                            if (oGroup.GRP_DESCRIPTION != oDataGroup.GRP_DESCRIPTION)
                            {
                                oGroup.GRP_DESCRIPTION = oDataGroup.GRP_DESCRIPTION;
                                oGroup.LITERAL.LIT_DESCRIPTION = oDataGroup.GRP_DESCRIPTION;
                            }
                            if (oGroup.GRP_QUERY_EXT_ID != oDataGroup.GRP_QUERY_EXT_ID)
                                oGroup.GRP_QUERY_EXT_ID = oDataGroup.GRP_QUERY_EXT_ID;
                            if (oGroup.GRP_EXT1_ID != oDataGroup.GRP_EXT1_ID)
                                oGroup.GRP_EXT1_ID = oDataGroup.GRP_EXT1_ID;
                            if (oGroup.GRP_EXT2_ID != oDataGroup.GRP_EXT2_ID)
                                oGroup.GRP_EXT2_ID = oDataGroup.GRP_EXT2_ID;
                            if (oGroup.GRP_EXT3_ID != oDataGroup.GRP_EXT3_ID)
                                oGroup.GRP_EXT3_ID = oDataGroup.GRP_EXT3_ID;
                        }
                        else if (!oGroupsByExtIds.Any())
                        {
                            // new group
                            decimal dGrpId = dbContext.GROUPs.Max(g => g.GRP_ID) + 1;
                            int iShowId = dbContext.GROUPs.Where(g => g.GRP_SHOW_ID != null).Max(g => Convert.ToInt32(g.GRP_SHOW_ID)) + 1;
                            decimal dLitId = dbContext.LITERALs.Max(g => g.LIT_ID) + 1;

                            LITERAL oLiteral = new LITERAL()
                            {
                                LIT_ID = dLitId,
                                LIT_DESCRIPTION = oDataGroup.GRP_DESCRIPTION
                            };
                            dbContext.LITERALs.InsertOnSubmit(oLiteral);
                            SecureSubmitChanges(ref dbContext);


                            oGroup = new GROUP()
                            {
                                GRP_ID = dGrpId,
                                GRP_DESCRIPTION = oDataGroup.GRP_DESCRIPTION,
                                GRP_LIT_ID = oLiteral.LIT_ID,
                                GRP_INS_ID = oInstallation.INS_ID,
                                GRP_SHOW_ID = iShowId.ToString(),
                                GRP_QUERY_EXT_ID = oDataGroup.GRP_QUERY_EXT_ID,
                                GRP_EXT1_ID = oDataGroup.GRP_EXT1_ID,
                                GRP_EXT2_ID = oDataGroup.GRP_EXT2_ID,
                                GRP_EXT3_ID = oDataGroup.GRP_EXT3_ID
                            };
                            dbContext.GROUPs.InsertOnSubmit(oGroup);
                            SecureSubmitChanges(ref dbContext);
                        }

                        if (oGroup != null)
                        {
                            if (oGroup.GROUPS_HIERARCHies.Any())
                            {
                                var oHierarchy = oGroup.GROUPS_HIERARCHies.Where(gh => gh.GRHI_INI_APPLY_DATE == oDataHierarchy.GRHI_INI_APPLY_DATE &&
                                                                                       gh.GRHI_END_APPLY_DATE == oDataHierarchy.GRHI_END_APPLY_DATE)
                                                                          .FirstOrDefault();
                                if (oHierarchy == null)
                                {
                                    oHierarchy = oGroup.GROUPS_HIERARCHies.FirstOrDefault();
                                    oHierarchy.GRHI_INI_APPLY_DATE = oDataHierarchy.GRHI_INI_APPLY_DATE;
                                    oHierarchy.GRHI_END_APPLY_DATE = oDataHierarchy.GRHI_END_APPLY_DATE;
                                    SecureSubmitChanges(ref dbContext);
                                }
                                dbContext.GROUPS_HIERARCHies.DeleteAllOnSubmit(oGroup.GROUPS_HIERARCHies.Where(gh => gh.GRHI_INI_APPLY_DATE != oDataHierarchy.GRHI_INI_APPLY_DATE ||
                                                                                                                     gh.GRHI_END_APPLY_DATE != oDataHierarchy.GRHI_END_APPLY_DATE));
                                SecureSubmitChanges(ref dbContext);
                            }
                            else
                            {
                                dbContext.GROUPS_HIERARCHies.InsertOnSubmit(new GROUPS_HIERARCHY()
                                {
                                    GRHI_GPR_ID_PARENT = 300000,
                                    GRHI_GPR_ID_CHILD = oGroup.GRP_ID,
                                    GRHI_INI_APPLY_DATE = oDataHierarchy.GRHI_INI_APPLY_DATE,
                                    GRHI_END_APPLY_DATE = oDataHierarchy.GRHI_END_APPLY_DATE
                                });
                                SecureSubmitChanges(ref dbContext);
                            }

                            /*if (!oGroup.GROUPS_HIERARCHies.Where(gh => gh.GRHI_INI_APPLY_DATE <= dtInstallationNow &&
                                                                       gh.GRHI_END_APPLY_DATE > dtInstallationNow).Any())
                            {
                                dbContext.GROUPS_HIERARCHies.DeleteAllOnSubmit(oGroup.GROUPS_HIERARCHies);
                                SecureSubmitChanges(ref dbContext);
                                dbContext.GROUPS_HIERARCHies.InsertOnSubmit(new GROUPS_HIERARCHY()
                                {
                                    GRHI_GPR_ID_PARENT = 300000,
                                    GRHI_GPR_ID_CHILD = oGroup.GRP_ID,
                                    GRHI_INI_APPLY_DATE = dtInstallationNow,
                                    GRHI_END_APPLY_DATE = new DateTime(2050, 1, 1)
                                });
                                SecureSubmitChanges(ref dbContext);
                            }*/

                            if (oDataGroupTypeAssignation != null &&
                                !oGroup.GROUPS_TYPES_ASSIGNATIONs.Where(gta => gta.GTA_GRPT_ID == oDataGroupTypeAssignation.GTA_GRPT_ID).Any())
                            {
                                dbContext.GROUPS_TYPES_ASSIGNATIONs.DeleteAllOnSubmit(oGroup.GROUPS_TYPES_ASSIGNATIONs);
                                SecureSubmitChanges(ref dbContext);
                                dbContext.GROUPS_TYPES_ASSIGNATIONs.InsertOnSubmit(new GROUPS_TYPES_ASSIGNATION()
                                {
                                    GTA_GRP_ID = oGroup.GRP_ID,
                                    GTA_GRPT_ID = oDataGroupTypeAssignation.GTA_GRPT_ID,
                                    GTA_DESCRIPTION = dbContext.GROUPS_TYPEs.Where(gt => gt.GRPT_ID == oDataGroupTypeAssignation.GTA_GRPT_ID).First().GRPT_DESCRIPTION
                                });
                                SecureSubmitChanges(ref dbContext);
                            }

                            
                            foreach (var oDataTrans in oDataTranslations)
                            {
                                var oTranslation = oGroup.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs.Where(gt => gt.GTET_IN_TAR_ID == oDataTrans.GTET_IN_TAR_ID &&
                                                                                                           gt.GTET_WS_NUMBER == oDataTrans.GTET_WS_NUMBER)
                                                                                              .FirstOrDefault();
                                if (oTranslation == null)
                                {
                                    dbContext.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs.InsertOnSubmit(new GROUPS_TARIFFS_EXTERNAL_TRANSLATION()
                                    {
                                        GTET_IN_GRP_ID = oGroup.GRP_ID,
                                        GTET_IN_TAR_ID = oDataTrans.GTET_IN_TAR_ID,
                                        GTET_WS_NUMBER = oDataTrans.GTET_WS_NUMBER,
                                        GTET_OUT_GRP_EXT_ID = oDataTrans.GTET_OUT_GRP_EXT_ID,
                                        GTET_OUT_TAR_EXT_ID = oDataTrans.GTET_OUT_TAR_EXT_ID
                                    });
                                }
                                else if (oTranslation.GTET_OUT_GRP_EXT_ID != oDataTrans.GTET_OUT_GRP_EXT_ID ||
                                         oTranslation.GTET_OUT_TAR_EXT_ID != oDataTrans.GTET_OUT_TAR_EXT_ID)
                                {
                                    oTranslation.GTET_OUT_GRP_EXT_ID = oDataTrans.GTET_OUT_GRP_EXT_ID;
                                    oTranslation.GTET_OUT_TAR_EXT_ID = oDataTrans.GTET_OUT_TAR_EXT_ID;
                                }

                            }

                            if (oDataGeometry.Any())
                            {
                                dbContext.GROUPS_GEOMETRies.DeleteAllOnSubmit(oGroup.GROUPS_GEOMETRies);
                                SecureSubmitChanges(ref dbContext);

                                foreach (var oDataPoint in oDataGeometry)
                                {
                                    dbContext.GROUPS_GEOMETRies.InsertOnSubmit(new GROUPS_GEOMETRY()
                                    {
                                        GRGE_GRP_ID = oGroup.GRP_ID,
                                        GRGE_POL_NUMBER = oDataPoint.GRGE_POL_NUMBER,
                                        GRGE_ORDER = oDataPoint.GRGE_ORDER,
                                        GRGE_LATITUDE = oDataPoint.GRGE_LATITUDE,
                                        GRGE_LONGITUDE = oDataPoint.GRGE_LONGITUDE,
                                        GRGE_INI_APPLY_DATE = oDataPoint.GRGE_INI_APPLY_DATE,
                                        GRGE_END_APPLY_DATE = oDataPoint.GRGE_END_APPLY_DATE
                                    });
                                }
                            }

                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();

                        }


                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateZoneConfiguration: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateZoneConfiguration: ", e);
                bRes = false;
            }

            return bRes;

        }

        public bool UpdateZonesConfigurationVersion(DateTime dtZonesConfigVersion, string sParameterName)
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

                        var oParameter = dbContext.PARAMETERs.Where(p => p.PAR_NAME == sParameterName).FirstOrDefault();
                        if (oParameter == null)
                        {
                            oParameter = new PARAMETER()
                            {
                                PAR_NAME = sParameterName,
                                PAR_DESCRIPTION = sParameterName
                            };
                            dbContext.PARAMETERs.InsertOnSubmit(oParameter);
                            SecureSubmitChanges(ref dbContext);
                        }

                        oParameter.PAR_VALUE = dtZonesConfigVersion.ToString("yyyyMMddHHmmss");


                        SecureSubmitChanges(ref dbContext);
                        transaction.Complete();

                    }
                    catch (Exception e)
                    {
                        m_Log.LogMessage(LogLevels.logERROR, "UpdateZonesConfigurationVersion: ", e);
                        bRes = false;
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateZonesConfigurationVersion: ", e);
                bRes = false;
            }

            return bRes;
        }

    }
}
