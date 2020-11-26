using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace integraMobile.Domain.Abstract
{

   

    public enum CustomerType
    {
        Individual = 1,
        Company=2
    }

    public enum PaymentSuscryptionType
    {
        pstPrepay = 1,
        pstPerTransaction = 2
    }

    public enum RefundBalanceType
    {
        rbtAmount = 1,
        rbtTime = 2,
    }


    public enum PaymentMeanType
    {
        pmtUndefined = -1,
        pmtSkipped = 0,
        pmtDebitCreditCard = 1,
        pmtPaypal = 2,
        pmtCoupon = 3,
        pmtCash = 4,
        pmtOxxo = 5
    }


    public enum PaymentMeanTypeStatus
    {
        pmsDebitCreditCard = 1,
        pmsPaypal = 2,
        pmsWithoutPaymentMean =3,
        pmsWithoutValidPaymentMean =4
    }



    public enum PaymentMeanCreditCardProviderType
    {
        pmccpUndefined = -1,
        pmccpCreditCall = 1,
        pmccpIECISA = 2,
        pmccpPaypal = 3,
        pmccpStripe = 4,
    }

    public enum PaymentMeanSubType
    {
        pmstUndefined = -1,
        pmstVisa = 1,
        pmstMasterCard = 2,
        pmstMaestro = 3,
        pmstAmex = 4,
        pmstPaypal = 10
    }



    public enum RechargeCouponsStatus
    {
        PendingActivation = 1,
        Actived = 2,
        Used=3,
        Cancelled=4,
        Locked = 5
    }
    public enum RechargeCouponsConfirmType
    {
        Cancel = 0,
        Use = 1
    }

    public enum PendingTransactionOperationOpType
    {
        Charge = 1,
        TokenDeletion = 2,
    }

    public enum PaymentMeanRechargeStatus
    {
        Authorized = 1,
        Waiting_Commit = 2,
        Failed_To_Commit = 3,
        Committed  = 4,
        Waiting_Cancellation=5,
        Failed_To_Cancel=6,
        Cancelled=7,
        Waiting_Refund=8,
        Failed_To_Refund=9,
        Refunded=10
    }

    public enum PaymentMeanRechargeInfoType
    {
        Payment_Without_Token = 1,
        Payment_With_Token = 2,
        Refund = 3
    }


    public enum PaymentMeanRechargeInfoStatus
    {
        Started = 1,
        Confirmed = 2,
        Failed = 3,
        CancelledByUser = 4,
        WaitingSystemCancellation = 5,
        CancelledBySystem = 6,
        FailedSystemCancellation = 7,
        ReConfirmed = 8,
        FailedReconfirmation = 9
    }


    public enum PaymentMeanRechargeCreationType
    {
        pmrctRegularRecharge = 0,
        pmrctAutomaticRecharge = 1,
        pmrctUserCreationRecharge = 2,
        pmrctChangePaymentMeanRecharge = 3
    }


    public enum InvitationStatus
    {
        isSent = 0,
        isAccepted = 1
    }


    public enum MobileSessionStatus
    {
        Open = 1,
        Closed = 2
    }


    public enum ServiceChargeType
    {
        NewPaymentMean = 1,
        PeriodicCharge = 2
    }


    public enum SecurityOperationType
    {
        ChangeEmail_Telephone=1,
        RecoverPassword=2,
        ResetPassword=3,
        ActivateAccount=4,
    }


    public enum SecurityOperationStatus
    {
        Inserted = 1,
        Confirmed = 2,
        Expired = 3
    }

    public enum PasswordRecoveryType
    {
        Recover = 1,
        Reset = 2
    }


    public enum InvoicePeriodType
    {
        Weekly = 1,
        Monthly = 2
    }

    public enum PaymentMeanRechargeType
    {
        Payment = 0,
        Coupon = 1,
        BarCode = 2,
        Pagatelia = 3,
        Spotycoins = 4,
        Paypal = 5,
        Cash = 6,
        Oxxo = 7,
    }


    public enum QueryOperationListPaymentMeanRechargeType
    {
        qol_DebitCreditCard=1,
        qol_ExternalParkingMeter=2,
        qol_ExternalMobilePayment=3,
        qol_Pagatelia = 4,
        qol_Oxxo = 5,
        qol_Paypal = 6,
        qol_Cash = 7,
        qol_Coupon = 8,
        qol_Spotycoins = 9,
    }

    public enum TollPaymentMode
    {
        Online = 0,
        Offline = 1,
        OfflineCurrentBalance = 2,
        OfflineAverageBalance = 3
    }

    public enum UserReplicationStatus
    {
        Inserted = 0,
        Queued = 1,
        Completed = 2,
        Error = 3,
        Cancelled = 4,
    };


    public struct stUserReplicationResult
    {
        public decimal m_dRepId;
        public DateTime m_dtStatusDate;
        public string m_strExternalReplicationId;
        public string m_strJobId;
        public string m_strJobURL;
        public int? m_iInJobOrder;
        public string m_strReplicationError;
        public UserReplicationStatus m_eUserReplicationStatus;
        public int? m_iCurrRetries;
        public decimal? m_dReplicationTime;
        public int? m_iQueueBeforeReplication;
    }



    public class OperationConfirmData
    {
        public decimal OPE_ID;
        public int OPE_TYPE;
        public System.Nullable<decimal> OPE_GRP_ID;
        public System.Nullable<decimal> OPE_TAR_ID;
        public System.DateTime OPE_DATE;
        public System.DateTime OPE_INIDATE;
        public System.DateTime OPE_ENDDATE;
        public int OPE_AMOUNT;
        public int OPE_REAL_AMOUNT;
        public int OPE_TIME;
        public int OPE_POSTPAY;
        public string OPE_EXTERNAL_ID1;
        public string OPE_EXTERNAL_ID2;
        public string OPE_EXTERNAL_ID3;
        public System.Nullable<System.DateTime> OPE_INSERTION_UTC_DATE;
        public int OPE_CONFIRMED_IN_WS1;
        public int OPE_CONFIRMED_IN_WS2;
        public int OPE_CONFIRMED_IN_WS3;
        public System.Nullable<int> OPE_CONFIRM_IN_WS1_RETRIES_NUM;
        public System.Nullable<int> OPE_CONFIRM_IN_WS2_RETRIES_NUM;
        public System.Nullable<int> OPE_CONFIRM_IN_WS3_RETRIES_NUM;
        public System.Nullable<System.DateTime> OPE_CONFIRM_IN_WS1_DATE;
        public System.Nullable<System.DateTime> OPE_CONFIRM_IN_WS2_DATE;
        public System.Nullable<System.DateTime> OPE_CONFIRM_IN_WS3_DATE;
        public System.Nullable<decimal> OPE_MOSE_ID;
        public System.Nullable<decimal> OPE_LATITUDE;
        public System.Nullable<decimal> OPE_LONGITUDE;
        public System.Nullable<decimal> OPE_PERC_VAT1;
        public System.Nullable<decimal> OPE_PERC_VAT2;
        public System.Nullable<decimal> OPE_PERC_FEE;
        public System.Nullable<decimal> OPE_PERC_FEE_TOPPED;
        public System.Nullable<decimal> OPE_FIXED_FEE;
        public System.Nullable<decimal> OPE_PERC_BONUS;
        public string OPE_SPACE_STRING;
        public string OPE_BONUS_MARCA;
        public System.Nullable<int> OPE_BONUS_TYPE;
        public INSTALLATION INSTALLATION;
        public USER_PLATE USER_PLATE;
        public USER USER;
    }

    public class TicketPaymentConfirmData
    {
        public decimal TIPA_ID;
        public System.DateTime TIPA_DATE;
        public string TIPA_TICKET_NUMBER;
        public int TIPA_AMOUNT;
        public string TIPA_EXTERNAL_ID;
        public string TIPA_EXTERNAL_ID2;
        public string TIPA_EXTERNAL_ID3;
        public string TIPA_PLATE_STRING;
        public string TIPA_TICKET_DATA;
        public System.Nullable<System.DateTime> TIPA_INSERTION_UTC_DATE;
        public System.Nullable<int> TIPA_CONFIRMED_IN_WS;
        public System.Nullable<int> TIPA_CONFIRMED_IN_WS2;
        public System.Nullable<int> TIPA_CONFIRMED_IN_WS3;
        public System.Nullable<int> TIPA_CONFIRM_IN_WS_RETRIES_NUM;
        public System.Nullable<int> TIPA_CONFIRM_IN_WS2_RETRIES_NUM;
        public System.Nullable<int> TIPA_CONFIRM_IN_WS3_RETRIES_NUM;
        public System.Nullable<System.DateTime> TIPA_CONFIRM_IN_WS_DATE;
        public System.Nullable<System.DateTime> TIPA_CONFIRM_IN_WS2_DATE;
        public System.Nullable<System.DateTime> TIPA_CONFIRM_IN_WS3_DATE;
        public System.Nullable<decimal> TIPA_GRP_ID;
        public INSTALLATION INSTALLATION;
        public USER USER;

    }


    public class OperationOffStreetConfirmData
    {
        public decimal OPEOFF_ID;
        public int OPEOFF_TYPE;
        public decimal OPEOFF_GRP_ID;
        public string OPEOFF_LOGICAL_ID;
        public string OPEOFF_TARIFF;
        public string OPEOFF_GATE;
        public System.Nullable<System.DateTime> OPEOFF_INSERTION_UTC_DATE;
        public System.DateTime OPEOFF_ENTRY_DATE;
        public System.Nullable<System.DateTime> OPEOFF_END_DATE;
        public int OPEOFF_AMOUNT;
        public int OPEOFF_TIME;
        public int OPEOFF_CONFIRMED_IN_WS1;
        public int OPEOFF_CONFIRMED_IN_WS2;
        public int OPEOFF_CONFIRMED_IN_WS3;
        public System.Nullable<int> OPEOFF_CONFIRM_IN_WS1_RETRIES_NUM;
        public System.Nullable<int> OPEOFF_CONFIRM_IN_WS2_RETRIES_NUM;
        public System.Nullable<int> OPEOFF_CONFIRM_IN_WS3_RETRIES_NUM;
        public System.Nullable<System.DateTime> OPEOFF_CONFIRM_IN_WS1_DATE;
        public System.Nullable<System.DateTime> OPEOFF_CONFIRM_IN_WS2_DATE;
        public System.Nullable<System.DateTime> OPEOFF_CONFIRM_IN_WS3_DATE;
        public CURRENCy CURRENCy;
        public USER_PLATE USER_PLATE;
        public USER USER;
        public CUSTOMER CUSTOMER;

    }

    public class URLConfirmData
    {
        public string URL;
        public int AssignedElements;
        public int MaxElementsToReturn;
    }

    public interface ICustomersRepository
    {


        bool ExistMainTelephone(int iCountry, string strTelephone);
        bool ExistEmail(string strEmail);
        bool ExistUsername(string strUsername);
        bool AddCustomerInscription(ref CUSTOMER_INSCRIPTION custInsc);
        bool GetCustomerInscription(ref CUSTOMER_INSCRIPTION custInsc,decimal dCustInscId);
        bool IsCustomerInscriptionExpired(CUSTOMER_INSCRIPTION custInsc);
        bool IsCustomerInscriptionAlreadyUsed(CUSTOMER_INSCRIPTION custInsc);
        bool InsertCustomerSMS(CUSTOMER_INSCRIPTION custInsc, string strTelephone, string strMessage, long lSenderId);
        bool InsertCustomerEmail(CUSTOMER_INSCRIPTION custInsc, string strEmailAddress, string strSubject, string strMessageBody, long lSenderId);
        bool InsertUserEmail(ref USER user, string strEmailAddress, string strSubject, string strMessageBody, long lSenderId);
        bool InsertUserSMS(ref USER user, string strTelephone, string strMessage, long lSenderId);
        bool UpdateActivationRetries(ref CUSTOMER_INSCRIPTION custInsc);
        CUSTOMER_INSCRIPTION GetCustomerInscriptionData(string urlParameter);
        bool AddUser(ref USER user, decimal? custInscId);
        bool DeleteNonActivatedUser(string strEmail, int iNumMaxMinutesForActivation, out bool bDeleteMembership);
        bool UpdateUser(ref USER user, IList<string> Plates);
        bool RenewUserData(ref USER user);
        bool GetUserData(ref USER user, string username);
        bool GetUserDataByEmail(ref USER user, string email);
        bool GetUserDataById(ref USER user, decimal dId);
        bool SetUserCultureLangAndUTCOffest(ref USER user, string strCultureLang,int iUTCOffset);
        bool SetUserUTCOffest(ref USER user, int iUTCOffset);
        bool SetUserCulture(ref USER user, string strCulture);
        bool SetUserPagateliaLastCredentials(ref USER user, string sPagateliaUser, string sPagateliaPwd);
        bool DeleteUser(ref USER user);
        bool SetUserSuscriptionType(ref USER user, PaymentSuscryptionType pst);
        bool SetUserRefundBalanceType(ref USER user, RefundBalanceType eRefundBalType);
        bool GetUserPossibleSuscriptionTypes(ref USER user, IInfraestructureRepository infrastructureRepository, out string sSuscriptionType, out RefundBalanceType eRefundBalType);
        bool SetUserPaymentMean(ref USER user, 
                               IInfraestructureRepository infrastructureRepository,
                               CUSTOMER_PAYMENT_MEAN paymentMean);
        bool UpdateUserPaymentMean(ref USER user, int iAutomaticRecharge,
                                        int? iModelAutomaticRechargeQuantity,
                                        int? iModelAutomaticRechargeWhenBelowQuantity,
                                        string strPaypalID);
        bool CopyCurrentUserPaymentMean(ref USER user, int iAutomaticRecharge,
                                int? iModelAutomaticRechargeQuantity,
                                int? iModelAutomaticRechargeWhenBelowQuantity,
                                string strPaypalID);

        IQueryable<CURRENCIES_PAYMENT_TYPE_GATEWAY_CONFIG> GetCurrenciesPaymentTypeGatewayConfigs();

        bool UpdateUserPaypalPreapprovalPaymentMean(ref USER user,
                                                    string strPreapprovalKey,
                                                    DateTime dtPreapprovalStartDate,
                                                    DateTime dtPreapprovalEndDate,
                                                    int? iMaxNumberOfPayments,
                                                    decimal? dMaxAmountPerPayment,
                                                    decimal? dMaxTotalAMount);


        bool StartRecharge(decimal dGatewayConfig,
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
                            );

        bool CompleteStartRecharge(decimal dGatewayConfig,
                           string strEmail,
                           string strTransactionID,
                           string strAuthResult,
                           string strCFTransactionID,
                           string strGatewayDate,
                           string strAuthCode,
                           PaymentMeanRechargeStatus eTransStatus);


        bool FailedRecharge(decimal dGatewayConfig,
                         string strEmail,
                         string strTransactionID,
                         PaymentMeanRechargeStatus eTransStatus);

        bool RechargeUserBalance(ref USER user,
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
                                decimal? dLatitude, decimal? dLongitude,string strAppVersion,
                                out decimal? dRechargeId,
                                bool bCreateNewContext = false);

        bool RechargeUserBalanceWithCoupon(ref USER user,
                                            int iOSType,
                                            int iRechargeQuantity,
                                            decimal dCurrencyID,
                                            string strRechargeId,
                                            ref RECHARGE_COUPON coupon,
                                            decimal? dLatitude, decimal? dLongitude, string strAppVersion);

        bool RechargeUserBalanceWithPagatelia(ref USER user,
                                            int iOSType,
                                            int iRechargeQuantity,
                                            decimal dCurrencyID,
                                            string strPagateliaSessionId,
                                            int? iPagateliaNewBalance,
                                            decimal? dLatitude, decimal? dLongitude, string strAppVersion);

        bool RechargeUserBalanceWithSpotycoins(ref USER user,
                                            int iOSType,
                                            int iRechargeQuantity,
                                            decimal dCurrencyID,
                                            decimal? dLatitude, decimal? dLongitude, string strAppVersion);

        bool RechargeUserBalanceWithCash(ref USER user,
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
                                        bool bCreateNewContext = false);

        bool RechargeUserBalanceWithOxxo(ref USER user,
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
                                        bool bCreateNewContext = false);

        bool RechargeUserBalanceWithPaypal(ref USER user,
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
                                        bool bCreateNewContext = false);


        bool AutomaticRechargeFailure(ref USER user);


        bool RefundRecharge(ref USER user, decimal dRechargeId, bool bRestoreBalance);
        bool ConfirmRecharge(ref USER user, decimal dRechargeId);


        bool GetWaitingCommitRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iConfirmWaitTime,
                                      int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);

        bool CommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,
                            string strUserReference,
                            string strAuthResult,
                            string strGatewayDate,
                            string strCommitTransactionId,
                            int iTransactionFee,
                            string strTransactionFeeCurrencyIsocode,
                            string strTransactionURL,
                            string strRefundTransactionURL);

        bool CommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge);                                   

        bool RetriesForCommitTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                            string strUserReference,
                                            string strAuthResult,
                                            string strGatewayDate,
                                            string strCommitTransactionId);


        bool GetWaitingCancellationRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,int m_iConfirmWaitTime,
                                      int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);

        bool CancelTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,
                            string strUserReference,
                            string strAuthResult,
                            string strGatewayDate,
                            string strCancellationTransactionId);


        bool CancelTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge);
                           

        bool RetriesForCancellationTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                            string strUserReference,
                                            string strAuthResult,
                                            string strGatewayDate,
                                            string strCancellationTransactionId);


        bool GetWaitingRefundRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iConfirmWaitTime,
                                      int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);

        bool RefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge,
                            string strUserReference,
                            string strAuthCode,
                            string strAuthResult,
                            string strAuthResultDesc,
                            string strGatewayDate,
                            string strRefundTransactionId);

        bool RefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge);


        bool RetriesForRefundTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge, int iMaxRetries,
                                            string strUserReference,
                                            string strAuthResult,
                                            string strAuthResultDesc,
                                            string strGatewayDate,
                                            string strRefundTransactionId);
        /*bool GetConfirmedRechargesInfo(out CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge, int iConfirmWaitTime,
                                        int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);
        bool ReConfirmTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge,
                                        string strConfirmResultCode,
                                        string strConfirmResultCodeDesc);
        bool RetriesForReConfirmTransaction(CUSTOMER_PAYMENT_MEANS_RECHARGES_INFO oRecharge, int iMaxRetries,
                                                string strConfirmResultCode,
                                                string strConfirmResultCodeDesc);
        */
        bool GetCancelableRecharges(out PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                    int iNumMinutestoCancelStartedTransaction,
                                    int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);
        bool CancelTransaction(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                       string strUserReference,
                                       string strAuthCode,
                                       string strAuthResult,
                                       string strAuthResultDesc,
                                       string strGatewayDate,
                                       string strRefundTransactionId);
        bool RetriesForCancelTransaction(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation, int iMaxRetries,
                                         string strAuthResult,
                                         string strAuthResultDesc);

        bool GetTokenDeletions(out PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                   int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries);
        bool TokenDeletion(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation,
                                       string strUserReference,
                                       string strAuthCode,
                                       string strAuthResult,
                                       string strAuthResultDesc,
                                       string strGatewayDate,
                                       string strRefundTransactionId);
        bool RetriesForTokenDeletion(PENDING_TRANSACTION_OPERATION oPendingTransactionOperation, int iMaxRetries,
                                         string strAuthResult,
                                         string strAuthResultDesc);


        bool GetAutomaticPaymentMeanPendingAutomaticRecharge(out CUSTOMER_PAYMENT_MEAN oPaymentMean,
                                                            int iNumSecondsToWaitInCaseOfRetry);


        bool InvalidatePaymentMeans(int iDaysAfterExpiredPaymentToInvalidate,
                                    int iMaxRetriesForAutomaticRecharge);



        bool SetUserRechargeStatus(ref USER user, decimal dRechargeId,PaymentMeanRechargeStatus rechargeStatus, int? iCurrRetries);



        IQueryable<ALL_OPERATION> GetUserOperations(ref USER user,
            Expression<Func<ALL_OPERATION, bool>> predicate,
            string orderbyField, 
            string orderbyDirection,
            int page,
            int pagesize,
            out int iNumRows);

        IQueryable<ALL_OPERATION> GetUserOperations(ref USER user,
                                                    Expression<Func<ALL_OPERATION, bool>> predicate,
                                                    string orderbyField,
                                                    string orderbyDirection);


        IQueryable<CUSTOMER_INVOICE> GetUserInvoices(ref USER user,
            Expression<Func<CUSTOMER_INVOICE, bool>> predicate,
            string orderbyField,
            string orderbyDirection,
            int page,
            int pagesize,
            out int iNumRows);


        
        IQueryable<ALL_OPERATION> GetUserOperations(ref USER user, int iNumDaysToGoBack);


        bool HideUserOperation(ref USER user, ChargeOperationsType opType, decimal dOPID);

        List<USER_OPERATIONS_HIDDEN> GetUserHiddenOperations(ref USER user);

        IEnumerable<OPERATION> GetUserPlateLastOperation(ref USER user,
                    out int iNumRows);

        bool GetOperationsPlatesAndZonesStatistics(ref USER user,
                                                    out string strMostUsedPlate,
                                                    out string strLastUsedPlate,
                                                    out decimal? dMostUsedZone,
                                                    out decimal? dLastUsedZone,
                                                    out decimal? dMostUsedTariff,
                                                    out decimal? dLastUsedTariff);


        bool IsPlateOfUser(ref USER user, string strPlate);
        bool IsPlateAssignedToAnotherUser(ref USER user, string strPlate);
        bool IsPlateAssignedToAnotherUser(string strPlate);
        bool AddPlateToUser(ref USER user, string strPlate);


        bool StartSession(ref USER user, decimal dInsId, int? iOSID, string strPushID, string strMACWIFI, string strIMEI, string strCellModel, string strOSVersion,string strPhoneSerialNumber,
                          string strCulture, string strAppVersion, bool bSessionKeepAlive, out string strSessionID);
        bool UpdateSession(ref USER user, string strSessionID, string strPushID, string strMACWIFI, string strIMEI, bool bUpdateSessionTime, out decimal? dInsId, out string strCulture, out string strAppVersion);
        bool GetUserFromOpenSession(string strSessionID, ref Dictionary<string, object> oUserDataDict);
        bool GetUserFromOpenSession(string strSessionID, out decimal dInstallationID, ref USER oUser);
        bool GetWaitingUserReplications(out List<stUserReplicationResult> oReplications, out int iQueueLength, UserReplicationWSSignatureType eSignatureType,
                                                           int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries,
                                                           int iMaxResendTime,
                                                           int iSecondsWait, int iMaxOperationsToReturn);
        bool GetWaitingQueuedUserReplications(out List<stUserReplicationResult> oReplications, out int iQueueLength, UserReplicationWSSignatureType eSignatureType,
                                              int iNumSecondsToWaitInQueuedState,ref string strUsername, ref string strPassword);

        bool GetZendeskUserDataDict(ref List<stUserReplicationResult> oUsersReps, ref Dictionary<string, object> oUsersDataDict,
                                    ref string strURL, ref string strUsername, ref string strPassword);
        
        bool UpdateUserReplications(ref List<stUserReplicationResult> oUsersReps, int iMaxRetries);

        bool GetPaymentMeanFees(ref USER oUser, out decimal dFeeVal, out decimal dFeePerc);
        bool GetRechargeCouponCode(out RECHARGE_COUPON oCoupon, string strCode);
        bool GetRechargeCouponRechargeID(ref USER oUser, decimal sessionId, decimal couponID, out string strRechargeID);
        bool GetRechargeCouponFromRechargeID(ref USER oUser, decimal sessionId, string strRechargeID,out RECHARGE_COUPON oCoupon);

        bool ChargeServiceOperation(ref USER user,
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
                                    out decimal dOperationID);



        bool ChargeFinePayment(ref USER user,
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
                                bool bConfirmedInWS1,bool bConfirmedInWS2,bool bConfirmedInWS3,
                                decimal? dLatitude,
                                decimal? dLongitude,string strAppVersion,
                                decimal? dGrpId,
                                string strSector, 
                                string strEnforcUser,
                                out decimal dTicketPaymentID,
                                out DateTime? dtUTCInsertionDate);

        bool UpdateThirdPartyIDInFinePayment(ref USER user,
                                             int iWSNumber,
                                             decimal dTicketPaymentID,
                                             string str3rdPartyOpNum);

        bool UpdateThirdPartyConfirmedInFinePayment(ref USER user,
                                                     decimal dTicketPaymentID,
                                                     bool bConfirmed1, bool bConfirmed2, bool bConfirmed3);


        bool AddDiscountToFinePayment(ref USER user,
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
                                    decimal? dLatitude, decimal? dLongitude, string strAppVersion);

        bool ChargeParkingOperation(ref USER user,
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
                                    decimal dPercVat1, decimal dPercVat2, int iPartialVat1, decimal dPercFEE, int iPercFEETopped, int iPartialPercFEE, int iFixedFEE, int iPartialFixedFEE, decimal dPercBonus, int iPartialBonusFEE, 
                                    int iTotalAmount,
                                    string sBonusId, string sBonusMarca, int? iBonusType,string strPlaceString,int iPostpay,
                                    decimal? dRechargeId,
                                    bool bConfirmedInWS1, bool bConfirmedInWS2, bool bConfirmedInWS3,
                                    decimal dMobileSessionId,
                                    decimal? dLatitude, decimal? dLongitude,string strAppVersion,
                                    string sExternalId1, string sExternalId2, string sExternalId3,
                                    out decimal dOperationID,
                                    out DateTime? dtUTCInsertionDate);


        bool UpdateThirdPartyIDInParkingOperation(ref USER user,
                                             int iWSNumber,
                                             decimal dOperationID,
                                             string str3rdPartyOpNum);

        bool UpdateThirdPartyConfirmedInParkingOperation(ref USER user,
                                             decimal dOperationID,
                                             bool bConfirmed1, bool bConfirmed2, bool bConfirmed3);

        bool ChargeUnParkingOperation(ref USER user,
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
                                    out DateTime? dtUTCInsertionDate);

        bool AddDiscountToParkingOperation(ref USER user,
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
                                    decimal? dLatitude, decimal? dLongitude, string strAppVersion);


        bool RefundChargeFinePayment(ref USER user,
                                    bool bAddToBalance,
                                    decimal dTicketPaymentID);

        bool RefundChargeParkPayment(ref USER user,
                                    bool bAddToBalance,
                                    decimal dOperationID);
        bool BackUnParkPayment(ref USER user,
                               decimal dOperationID);


        bool AddSessionOperationParkInfo(ref USER user,
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
                                     decimal dPercBonus, string sBonusId, string sBonusMarca, int? iBonusType);


        bool AddSessionOperationUnParkInfo(ref USER user,
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
                                     int iFixedFEE,
                                     decimal dPercBonus, string sBonusId);



        bool CheckSessionOperationParkInfo(ref USER user,
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
                                     out decimal dPercBonus, out string sBonusId, out string sBonusMarca, out int? iBonusType);


        bool CheckSessionOperationUnParkInfo(ref USER user,
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
                                     out decimal dPercBonus, out string sBonusId);

        bool DeleteSessionOperationInfo(ref USER user,
                                     string strSessionID,
                                     string strPlate);


        bool AddSessionTicketPaymentInfo(ref USER user,
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
                                     string strEnforcUser);


        bool CheckSessionTicketPaymentInfo(ref USER user,
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
                                     out string strEnforcUser);
           

        bool DeleteSessionTicketPaymentInfo(ref USER user,
                                     string strSessionID);


        bool AddWPPushIDNotification(ref USER user,
                                    string strToastText1,
                                    string strToastText2,
                                    string strToastParam,
                                    string strTileTitle,
                                    int iTileCount,
                                    string strBackgroundImage);

        bool AddAndroidPushIDNotification(ref USER user,
                                          string strAndroidRawData);


        bool AddPushIDNotification( ref USER user,
                                    string strToastText1,
                                    string strToastText2,
                                    string strToastParam,
                                    string strTileTitle,
                                    int iTileCount,
                                    string strBackgroundImage,
                                    string strAndroidRawData,
                                    string strIOSRawData,
                                    ref decimal oNotifID,
                                    decimal? dUserPushId=null);


        bool GetUsersWithPlate(string strPlate, out IEnumerable<USER> oUsersList);
        bool GetLastOperationWithPlate(string strPlate, decimal dInstallationId, out OPERATION oOperation);
        bool QueryUnConfirmedParkingOperations(decimal dCurrId,int? iWSSignatureType, int iMaxRows, out IEnumerable<OPERATION> oOperationList);
        bool ExistUnConfirmedParkingOperationFor(decimal dInsId, string strPlate);
        bool ConfirmUnConfirmedParkingOperations(decimal dCurrId, int? iWSSignatureType);
        bool QueryUnConfirmedFinePayments(decimal dCurrId, int? iWSSignatureType, int iMaxRows, out IEnumerable<TICKET_PAYMENT> oFinePaymentList);
        bool ConfirmUnConfirmedFinePayments(decimal dCurrId, int? iWSSignatureType);


        bool AddSecurityOperation(ref USER user, USERS_SECURITY_OPERATION oSecOperation);
        bool UpdateSecurityOperationRetries(ref USERS_SECURITY_OPERATION secOperation);
        bool IsUserSecurityOperationExpired(USERS_SECURITY_OPERATION oSecOperation);
        bool IsUserSecurityOperationExpired(USERS_SECURITY_OPERATION oSecOperation, int iMaxSecondsToActivate);
        bool IsUserSecurityOperationAlreadyUsed(USERS_SECURITY_OPERATION oSecOperation);
        bool ModifyUserEmailOrTelephone(USERS_SECURITY_OPERATION oSecOperation, bool bUsernameEqualsEmail);
        bool ModifyUserEmailOrTelephone(ref USER user, decimal dCouId, string sTelephone, string sEmail, bool bUsernameEqualsEmail);
        bool ModifyUserBillingInfo(ref USER user, string sName, string sAddr, int iAddrNum, string sZipCode, string sCity, string sVat);
        bool ConfirmSecurityOperation(USERS_SECURITY_OPERATION oSecOperation);
        bool ActivateUser(USERS_SECURITY_OPERATION oSecOperation);
        USERS_SECURITY_OPERATION GetUserSecurityOperation(string urlParameter);
        USERS_SECURITY_OPERATION GetUserSecurityOperation(decimal dSecOpId);

        bool GetOperationData(ref USER user,
                            decimal dOperationID,
                            out OPERATION oParkOp);

        bool GetRechargeData(ref USER user,
                             decimal dRechargeID,
                             out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge);

        bool GetTicketPaymentData(ref USER user,
                                decimal dTicketPaymentID,
                                out TICKET_PAYMENT oTicketPayment);


        bool GetFirstNotInvoicedRecharge(out CUSTOMER_PAYMENT_MEANS_RECHARGE oRecharge);


        bool GenerateInvoicesForRecharges(DateTime dt, out int iNumOperations);
        bool GenerateInvoicesForTicketPayments(DateTime dt, out int iNumOperations);
        bool GenerateInvoicesForOperations(DateTime dt, out int iNumOperations);
        bool GenerateInvoices(DateTime endDate, out int iNumInvoices);

        //OPERATION GetOperation(decimal dOpeId);
        bool GetWaitingConfirmationOperation(out List<OperationConfirmData> oOperations, out int iQueueLength, int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries, int m_iMaxResendTime, List<decimal> oListRunningOperations, int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList);
        bool UpdateThirdPartyConfirmedInParkingOperation(decimal dOperationID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out OPERATION oOperation);
        bool GetWaitingConfirmationFine(out List<TicketPaymentConfirmData> oTicketPayments, out int iQueueLength, int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries, int m_iMaxResendTime, List<decimal> oListRunningFines, int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList);
        bool UpdateThirdPartyConfirmedInFine(decimal dFineID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out TICKET_PAYMENT oTicketPayment);
        bool GetWaitingConfirmationOffstreetOperation(out List<OperationOffStreetConfirmData> oOperations, out int iQueueLength, int iNumSecondsToWaitInCaseOfRetry, int iMaxRetries, int m_iMaxResendTime, List<decimal> oListRunningOperations, int iSecondsWait, int iMaxWorkingThreads, ref List<URLConfirmData> oConfirmDataList);
        bool UpdateThirdPartyConfirmedInOffstreetOperation(decimal dOperationID, int iWSNumber, bool bConfirmed, string str3dPartyOpNum, long lEllapsedTime, int iQueueLength, out OPERATIONS_OFFSTREET oOperation);

        bool GetMobileSessionById(decimal dMobileSessionId, out MOBILE_SESSION oMobileSession);

        bool DeletePlate(ref USER user, string sPlate);

        bool GetFavouriteGroupFromUser(ref USER user, decimal? dInstallationId, DateTime xBeginDateUTC, DateTime xEndDateUTC, out decimal? dGroupId);
        bool GetFavouriteAreasFromUser(ref USER user, decimal? dInstallationId, out List<USERS_FAVOURITES_AREA> oFavouriteAreas);
        bool SetFavouriteAreasFromUser(ref USER user, List<USERS_FAVOURITES_AREA> oFavouriteAreas);
        bool GetPreferredPlatesFromUser(ref USER user, decimal? dInstallationId, out List<USERS_PREFERRED_PLATE> oPreferredPlates);
        bool SetPreferredPlatesFromUser(ref USER user, List<USERS_PREFERRED_PLATE> oPreferredPlates);
        bool GetPlateFromUser(ref USER user, string sPlate, out USER_PLATE oUserPlate);

        bool AddSessionOperationOffstreetInfo(ref USER user,
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
                                              DateTime dtExitLimitUTCDate,
                                              DateTime dtInstDate,
                                              double dChangeToApply,
                                              decimal dPercVat1, decimal dPercVat2,                                            
                                              decimal dPercFEE, int iPercFEETopped,
                                              int iFixedFEE, 
                                              string sDiscountCodes);

        bool CheckSessionOperationOffstreetInfo(ref USER user,
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
                                                out string sDiscounts);

        bool DeleteSessionOperationOffstreetInfo(ref USER user,
                                                 string strSessionID);

        bool ChargeOffstreetOperation(ref USER user,
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
                                    out decimal dOperationID);

        bool RefundChargeOffstreetPayment(ref USER user,
                                          bool bAddToBalance,
                                          decimal dOperationID);

        bool UpdateThirdPartyIDInOffstreetOperation(ref USER user,
                                                    int iWSNumber,
                                                    decimal dOperationID,
                                                    string str3rdPartyOpNum);

        bool UpdateThirdPartyConfirmedInOffstreetOperation(ref USER user,
                                                           decimal dOperationID,
                                                           bool bConfirmed1, bool bConfirmed2, bool bConfirmed3);

        bool GetOperationOffstreetData(ref USER user,
                                       decimal dOperationID,
                                       out OPERATIONS_OFFSTREET oParkOp);

        bool GetLastOperationOffstreetData(decimal dGroupId, string sLogicalId, out OPERATIONS_OFFSTREET oParkOp);        

        bool UpdateOperationOffstreetExitData(decimal dOperationId, DateTime dtExitDate, DateTime dtUTCExitDate, bool bMustNotify, out OPERATIONS_OFFSTREET oParkOp);

        bool UpdateOperationOffstreetSpaceData(decimal dOperationId, string sSpaceDesc, out OPERATIONS_OFFSTREET oParkOp);

        int HealthCheck();

        IEnumerable<OPERATIONS_OFFSTREET> GetUserPlateLastOperationOffstreet(ref USER user, out int iNumRows);

        OPERATOR GetDefaultOperator();
        OPERATOR GetDefaultOperator(integraMobileDBEntitiesDataContext dbContext);
        
        bool GetFinantialParams(USER oUser, decimal dInsId, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParams(USER oUser, TOLL oToll, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParams(USER oUser, string sTimezone, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParams(string sCurrencyIsoCode, string sTimezone, int? iPaymentTypeId, int? iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParams(USER user, GROUP oGroup, PaymentSuscryptionType oSuscriptionType, int? iPaymentTypeId, int? iPaymentSubtypeId, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);

        bool GetFinantialParamsPaymentType(USER oUser, decimal dInsId, PaymentSuscryptionType oSuscriptionType, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParamsPaymentType(USER oUser, string sTimezone, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);
        bool GetFinantialParamsPaymentType(string sCurrencyIsoCode, string sTimezone, int iPaymentTypeId, int iPaymentSubtypeId, ChargeOperationsType oOpeType, out decimal dVAT1, out decimal dVAT2, out decimal dPercFEE, out int iPercFEETopped, out int iFixedFEE);

        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                         out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE,
                         out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT);
        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                         int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE,
                         out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT);
        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT);
        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                         int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                         out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT);
        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                         out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE);
        int CalculateFEE(int iAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                         out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE);

        int CalculateFEE(decimal dInsId, ChargeOperationsType oOpeType, int iAmount,
                         out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE);
        int CalculateFEE(decimal dInsId, ChargeOperationsType oOpeType, int dAmount,
                         out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE, integraMobileDBEntitiesDataContext dbContext);

        int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT);
        int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE,
                                out int iPartialPercFEEVAT, out int iPartialFixedFEEVAT, out int iPartialBonusFEEVAT);
        int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE);
        int CalculateFEEReverse(int iTotalAmount, decimal dVAT1, decimal dVAT2, decimal dPercFEE, int iPercFEETopped, int iFixedFEE, decimal dPercBonus,
                                out int iPartialVAT1, out int iPartialPercFEE, out int iPartialFixedFEE, out int iPartialBonusFEE);

        int CalculateFEEReverse(decimal dInsId, ChargeOperationsType oOpeType, int iTotalAmount,
                                out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE);
        int CalculateFEEReverse(decimal dInsId, ChargeOperationsType oOpeType, int iTotalAmount,
                                out decimal dVAT1, out decimal dVAT2, out int iPartialVAT1, out decimal dPercFEE, out int iPercFEETopped, out int iPartialPercFEE, out int iFixedFEE, out int iPartialFixedFEE, integraMobileDBEntitiesDataContext dbContext);

        bool NeedDisplayLicenseTerms(USER oUser, string sCulture, out string sVersion, out string sUrl1, out string sUrl2);
        bool UpdateUserLicenseTerms(USER oUser, string sVersion);

        bool GetUserLock(ref USER_LOCK oUserLock, string username);
        bool AddUserLock(string username, DateTime xLockUtcDate);
        bool DeleteUserLock(string username);

        bool TransferBalance(ref USER srcUser, ref USER dstUser,
                                    int iOSType,
                                    int iQuantity,
                                    decimal dCurID,
                                    decimal dDstBalanceCurID,
                                    double dChangeApplied, double dChangeFee,
                                    int iCurrencyDstQuantity,
                                    decimal dMobileSessionId,
                                    string strAppVersion,
                                    out decimal dTransferID,
                                    out DateTime? dtUTCInsertionDate);

        bool QueryCouponAndLock(string sQrCode, bool bKeyQrCode, DateTime dtQueryDateUtc, decimal dExternalProviderId, int iLockTimeoutSeconds, out bool bAvailable, out RECHARGE_COUPON oRechargeCoupon);
        bool ConfirmCoupon(string sQrCode, bool bKeyQrCode, DateTime dtQueryDateUtc, decimal dExternalProviderId, RechargeCouponsConfirmType oType, out RECHARGE_COUPON oResCoupon);

        bool InsertUserFriend(ref USER user, string sFriendEmail, long lSenderId);
        bool AssignPendingInvitationsToAccept(ref USER user);

        IEnumerable<TOLL_MOVEMENT> GetUserPlateLastTollMovement(ref USER user, out int iNumRows);
        bool ChargeTollMovement(ref USER user,
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
                                out DateTime? dtUTCInsertionDate);
        /*bool ModifyTollMovement(decimal dMovementId,
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
                                string sQr);*/
        TOLL GetToll(decimal dTollId);
        bool GetUserAverageBalanceById(ref USER_AVERAGE_BALANCE user, decimal dUserId);
        IQueryable<TOLL_MOVEMENT> GetTollMovementsByQr(string sQr);
        bool GetTollMovementById(decimal dId, out TOLL_MOVEMENT oTollMovement);
        bool GetTollMovementByLockId(decimal dLockId, out TOLL_MOVEMENT oTollMovement);

        bool GatewayErrorLogUpdate(decimal dGatewayConfigId, bool bExceptionError, bool bTransactionError);
        bool GetGatewayErrorLogTotalSeconds(decimal dGatewayConfigId, DateTime dtIniUtc, DateTime dtEndUtc, int iMinCommErrors, int iMinTransErrors, out double dTotalSeconds);

    }
}
