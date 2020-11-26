using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace integraMobile.Domain.Abstract
{

    public enum ChargeOperationsType
    {
        ParkingOperation=1,
        ExtensionOperation=2,
        ParkingRefund=3,
        TicketPayment=4,
        BalanceRecharge=5,
        ServiceCharge=6,
        Discount=7,
        OffstreetEntry = 8,
        OffstreetExit = 9,
        OffstreetOverduePayment = 10,
        BalanceRechargeRefund = 11,
        CouponCharge = 12,
        SubscriptionCharge = 13,
        BalanceTransfer = 14,
        BalanceReception = 15,
        TollPayment = 16,
        TollLock = 17,
        TollUnlock = 18
    }


    public enum NotificationEventType
    {
        TicketInsertion=1,
        BeforeEndParking=2,
        ParkingInsertion=3,
        Info = 4,
        OffstreetParkingEntry = 5,
        OffstreetParkingExit = 6,
        Issue = 7,
        PasswordRecovery = 8, 
    }

    public enum UserNotificationStatus
    {
        Inserted = 10,
        Generated = 20,
        Sending = 30,
        Finished_Partially = 40,
        Finished_Completely = 50
    }


    public enum PushIdNotificationStatus
    {
        Inserted = 10,
        Sending = 20,
        Sent = 30,
        Waiting_Next_Retry = 40,
        Failed = 50,
        SubcriptionExpired = 60

    }


    public enum PlateMovSendingStatus
    {
        Inserted = 10,
        Sending = 20,
        Sent = 30,
        Waiting_Next_Retry = 40
    }

    public enum PlateSendingWSSignatureType
    {
        psst_test = 0,
        psst_internal = 1,
        psst_standard = 2,
        psst_eysa = 3
    }


    public enum MobileOS
    {
        None = 0,
        Android = 1,
        WindowsPhone = 2,
        iOS = 3,
        Blackberry = 4,
        Web = 5
    }

    public enum OperationSourceType
    {
        InternMobilePayment = 1,
        ExternalParkingMeter = 2,
        ExternalMobilePayment = 3
    }

    public enum GroupType
    {
        OnStreet = 0,
        OffStreet = 1
    }

    public enum ParkByMapMode
    {
        Zone = 1,
        StreetSection = 2
    }

    [Serializable]
    public struct FileAttachmentInfo
    {

        public string strName;
        public string strMediaType;
        public byte[] fileContent;
        public string filePath;

    }

    public enum OffstreetOperationIdType
    {
        MeyparId = 1,
        QRId = 2
    }

    public enum OffstreetParkingType
    {
        Ticket = 1,
        Barcode = 10,
        Cameras = 100
    }

    public enum OffstreetOperationType
    {
        Entry = 8,
        Exit = 9,
        OverduePayment = 10
    }

    public enum SignupScreenType
    {
        Iparkme = 1,
        BilboPark = 2
    };

    public class OffstreetParkingOccupation
    {
        public decimal GroupId { get; set; }
        public OffstreetParkingType[] ParkingType { get; set; }
        public float OccupationPerc { get; set; }
        public string Colour { get; set; }
        public string ExternalNum { get; set; }
        public string Description { get; set; }
    }

    

    public interface IInfraestructureRepository
    {

        IQueryable<CURRENCy> Currencies { get; }
        IQueryable<COUNTRy> Countries { get; }
        IQueryable<PARAMETER> Parameters { get; }

        string GetParameterValue(string strParName);
        decimal GetVATPerc();
        decimal GetChangeFeePerc();
        string GetCountryTelephonePrefix(int iCountryId);
        int GetTelephonePrefixCountry(string strPrefix);
        string GetCountryName(int iCountryId);
        int GetCountryCurrency(int iCountryId);
        bool GetCountryPossibleSuscriptionTypes(int iCountryId, out string sSuscriptionType, out RefundBalanceType eRefundBalType);
        string GetCurrencyIsoCode(int iCurrencyId);
        decimal GetCurrencyFromIsoCode(string strISOCode);
        string GetCurrencyIsoCodeNumericFromIsoCode(string strISOCode);
        int GetCurrencyDivisorFromIsoCode(string strISOCode);
        long SendEmailTo(string strEmailAddress, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal);
        List<long> SendEmailToMultiRecipients(List<string> lstRecipients, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal);
        long SendEmailWithAttachmentsTo(string strEmailAddress, string strSubject, string strMessageBody, List<FileAttachmentInfo> lstAttachments, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal);
        List<long> SendEmailWithAttachmentsToMultiRecipients(List<string> lstRecipients, string strSubject, string strMessageBody, List<FileAttachmentInfo> lstAttachments, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal);
        List<long> SendEmailToMultiRecipientsTool(decimal dUniqueId, string strSubject, string strMessageBody, integraSenderWS.EmailPriority emailPriority = integraSenderWS.EmailPriority.Normal);
        long SendSMSTo(int iCountryCode, string strTelephone, string strMessage, ref string strCompleteTelephone);

        //STRIPE

        bool GetStripeConfiguration(string Guid, out STRIPE_CONFIGURATION oStripeConfiguration);

        //IECISA

        bool GetIECISAConfiguration(string Guid, out IECISA_CONFIGURATION oStripeConfiguration);


        //NOTIFICATIONS
        bool GetFirstNotGeneratedUserNotification(out USERS_NOTIFICATION notif);
        bool GenerateUserNotification(ref USERS_NOTIFICATION notif);
        bool GetFirstNotSentNotification(out PUSHID_NOTIFICATION notif, int iResendTime);
        bool PushIdNotificationSent(decimal dPushNotifID);
        bool PushIdNotificationFailed(decimal dPushNotifID, int iMaxRetries);
        bool PushIdExpired(decimal dPushNotifID, string strNewPushId);

        //PLATE SYNCRO
        bool GeneratePlatesSending();
        IEnumerable<USER_PLATE_MOVS_SENDING> GetPlatesForSending(int iMaxNumPlates);
        bool ErrorSedingPlates(IEnumerable<USER_PLATE_MOVS_SENDING> oPlateList);
        bool ConfirmSentPlates(IEnumerable<USER_PLATE_MOVS_SENDING> oPlateList);


        //EXTERNAL TICKETS AND PARKS SYNCRO
        bool ExistPlateInSystem(string strPlate);
        bool AddExternalPlateFine(decimal dInstallation,
                                  string strPlate,
                                  DateTime dtTicket,
                                  DateTime dtTicketUTC,
                                  string strFineNumber,
                                  int iQuantity,
                                  DateTime dtLimit,
                                  DateTime dtLimitUTC,
                                  string strArticleType,
                                  string strArticleDescription);

        bool AddExternalPlateParking(decimal dInstallation,
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
                                  out decimal dOperationId);

        bool GetInsertionTicketNotificationData(out EXTERNAL_TICKET oTicket);
        bool GetInsertionUserSecurityDataNotificationData(out USERS_SECURITY_OPERATION oSecurityOperation);
        bool GetInsertionParkingNotificationData(out EXTERNAL_PARKING_OPERATION oParking);
        bool GetBeforeEndParkingNotificationData(int iNumMinutesBeforeEndToWarn, out EXTERNAL_PARKING_OPERATION oParking);
        bool GetOffstreetOperationNotificationData(out OPERATIONS_OFFSTREET oOperation);

        bool MarkAsGeneratedInsertionTicketNotification(EXTERNAL_TICKET oTicket);
        bool MarkAsGeneratedInsertionParkingNotificationData(EXTERNAL_PARKING_OPERATION oParking);
        bool MarkAsGeneratedBeforeEndParkingNotificationData(EXTERNAL_PARKING_OPERATION oParking);
        bool MarkAsGeneratedOffstreetOperationNotificationData(OPERATIONS_OFFSTREET oOperation);
        bool MarkAsGeneratedUserSecurityDataNotificationData(USERS_SECURITY_OPERATION oSecurityOperation, decimal oNotifID);

        bool getCarrouselVersion(int iVersion, int iLang, out CARROUSEL_SCREEN_VERSION oCarrouselVersion);
        
        string GetLiteral(decimal literalId, string langCulture);

        bool GetLanguage(decimal dLanId, out LANGUAGE oLanguage);

        //STREET SECTIONS SYNC

        long GetMaxVersionStreets();
        long GetMaxVersionStreetSections();
        long GetMaxVersionStreetSectionsGeometry();
        long GetMaxVersionStreetSectionsGrid();
        long GetMaxVersionStreetSectionsGridGeometry();
        long GetMaxVersionStreetSectionsStreetSectionsGrid();
        long GetMaxVersionTariffsInStreetSections();

        bool GetSyncStreets(long lVersionFrom, int iMaxRegistries, out STREETS_SYNC[] oArrSync );
        bool GetSyncStreetSections(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_SYNC[] oArrSync);
        bool GetSyncStreetSectionsGeometry(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GEOMETRY_SYNC[] oArrSync);
        bool GetSyncStreetSectionsGrid(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GRID_SYNC[] oArrSync);
        bool GetSyncStreetSectionsGridGeometry(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_GRID_GEOMETRY_SYNC[] oArrSync);
        bool GetSyncStreetSectionsStreetSectionsGrid(long lVersionFrom, int iMaxRegistries, out STREET_SECTIONS_STREET_SECTIONS_GRID_SYNC[] oArrSync);
        bool GetSyncTariffsInStreetSections(long lVersionFrom, int iMaxRegistries, out TARIFF_IN_STREETS_SECTIONS_COMPILED_SYNC[] oArrSync);


        bool AddStreetSectionPackage(decimal dInstallationID, decimal id, byte[] file);
        bool DeleteOlderStreetSectionPackage(decimal dInstallationID, decimal id);
        bool GetLastStreetSectionPackageId(decimal dInstallationID, out decimal id);
        bool GetLastStreetSectionPackage(decimal dInstallationID, out byte[] file);

    }
}
