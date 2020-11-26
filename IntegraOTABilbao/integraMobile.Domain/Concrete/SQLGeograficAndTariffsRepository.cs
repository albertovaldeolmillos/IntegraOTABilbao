using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using integraMobile.Domain.Abstract;
using integraMobile.Infrastructure.Logging.Tools;

namespace integraMobile.Domain.Concrete
{

    public class SQLGeograficAndTariffsRepository : IGeograficAndTariffsRepository
    {
        //Log4net Wrapper class
        private static readonly CLogWrapper m_Log = new CLogWrapper(typeof(SQLGeograficAndTariffsRepository));
        private const int ctnTransactionTimeout = 30;


        public SQLGeograficAndTariffsRepository(string connectionString)
        {
        }

        public bool getInstallation(decimal? dInstallationId,
                decimal? dLatitude, decimal? dLongitude, 
                ref INSTALLATION oInstallation, 
                ref DateTime ?dtInsDateTime)
        {
            bool bRes = false;
            oInstallation = null;
            dtInsDateTime = null;

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


                    if (dInstallationId != null)
                    {
                        //var oInstallations_L = (from r in dbContext.INSTALLATIONs where r.INS_ENABLED == 1 select r).ToList();

                        var oInstallations = (from r in dbContext.INSTALLATIONs
                                              where r.INS_ID == dInstallationId &&
                                                    r.INS_ENABLED == 1
                                              select r).ToArray();
                        if (oInstallations.Count() == 1)
                        {
                            oInstallation = oInstallations[0];
                            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                            DateTime dtServerTime = DateTime.Now;
                            dtInsDateTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                            bRes = true;

                        }
                    }
                    else if ((dLatitude != null) && (dLongitude != null))
                    {
                        var oInstallations = (from r in dbContext.INSTALLATIONs
                                              where r.INS_ENABLED == 1
                                              orderby r.INS_ID
                                              select r).ToArray();
                        bool bIsInside = false;

                        foreach (INSTALLATION oInst in oInstallations)
                        {
                            bIsInside = false;
                            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInst.INS_TIMEZONE_ID);
                            DateTime dtServerTime = DateTime.Now;
                            DateTime dtLocalInstTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);

                            var PolygonNumbers = (from r in dbContext.INSTALLATIONS_GEOMETRies
                                                  where ((r.INSGE_INS_ID == oInst.INS_ID) &&
                                                  (r.INSGE_INI_APPLY_DATE <= dtLocalInstTime) &&
                                                  (r.INSGE_END_APPLY_DATE >= dtLocalInstTime))
                                                  group r by new
                                                  {
                                                    r.INSGE_POL_NUMBER
                                                  } into g
                                                  orderby g.Key.INSGE_POL_NUMBER
                                                  select new {iPolNumber = g.Key.INSGE_POL_NUMBER}).ToList();


                            Point p = new Point(Convert.ToDouble(dLongitude), Convert.ToDouble(dLatitude));

                            foreach (var oPolNumber in PolygonNumbers)
                            {
                                var Polygon = (from r in dbContext.INSTALLATIONS_GEOMETRies
                                               where ((r.INSGE_INS_ID == oInst.INS_ID) &&
                                               (r.INSGE_INI_APPLY_DATE <= dtLocalInstTime) &&
                                               (r.INSGE_END_APPLY_DATE >= dtLocalInstTime) &&
                                               (r.INSGE_POL_NUMBER == oPolNumber.iPolNumber))
                                               orderby r.INSGE_ORDER
                                               select new Point(Convert.ToDouble(r.INSGE_LONGITUDE),
                                                                Convert.ToDouble(r.INSGE_LATITUDE))).ToArray();


                                if (Polygon.Count() > 0)
                                {

                                    if (IsPointInsidePolygon(p, Polygon))
                                    {
                                        bIsInside = true;                                       
                                        break;
                                    }

                                }

                            }

                            if (bIsInside)
                            {
                                bRes = true;
                                oInstallation = oInst;
                                dtInsDateTime = dtLocalInstTime;
                                break;
                            }

                        }

                        

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallation: ", e);
            }

            return bRes;


        }


        public IEnumerable<INSTALLATION> getInstallationsList()
        {

            List<INSTALLATION> res = new List<INSTALLATION>();

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
                           where r.INS_ENABLED == 1
                           orderby r.INS_DESCRIPTION
                           select r).ToList();
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallations: ", e);
            }

            return (IEnumerable<INSTALLATION>)res;

        }


        public bool getGroup(decimal? dGroupId,
                ref GROUP oGroup,
                ref DateTime? dtgroupDateTime)
        {
            bool bRes = false;
            oGroup = null;
            dtgroupDateTime = null;

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


                    if (dGroupId != null)
                    {


                        var oGroups = (from r in dbContext.GROUPs
                                       where r.GRP_ID == dGroupId
                                       select r).ToArray();
                        if (oGroups.Count() == 1)
                        {
                            oGroup = oGroups[0];
                            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oGroups[0].INSTALLATION.INS_TIMEZONE_ID);
                            DateTime dtServerTime = DateTime.Now;
                            dtgroupDateTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                            bRes = true;

                        }
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroup: ", e);
            }

            return bRes;


        }




        public DateTime? getInstallationDateTime(decimal dInstallationId)
        {

            DateTime? dtRes = null;

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


                    var oInstallations = (from r in dbContext.INSTALLATIONs
                                          where r.INS_ID == dInstallationId
                                          select r).ToArray();
                    if (oInstallations.Count() == 1)
                    {
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        dtRes = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                    }
                }      
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallationDateTime: ", e);
            }

            return dtRes;

        }

        public DateTime? ConvertInstallationDateTimeToUTC(decimal dInstallationId,DateTime dtInstallation)
        {

            DateTime? dtRes = null;

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


                    var oInstallations = (from r in dbContext.INSTALLATIONs
                                          where r.INS_ID == dInstallationId
                                          select r).ToArray();
                    if (oInstallations.Count() == 1)
                    {
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                        dtRes = TimeZoneInfo.ConvertTime(dtInstallation, tzi, TimeZoneInfo.Utc);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConvertInstallationDateTimeToUTC: ", e);
            }

            return dtRes;

        }


        public DateTime? ConvertUTCToInstallationDateTime(decimal dInstallationId, DateTime dtUTC)
        {

            DateTime? dtRes = null;

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


                    var oInstallations = (from r in dbContext.INSTALLATIONs
                                          where r.INS_ID == dInstallationId
                                          select r).ToArray();
                    if (oInstallations.Count() == 1)
                    {
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                        dtRes = TimeZoneInfo.ConvertTime(dtUTC, TimeZoneInfo.Utc, tzi );
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "ConvertUTCToInstallationDateTime: ", e);
            }

            return dtRes;

        }

        public int? GetInstallationUTCOffSetInMinutes(decimal dInstallationId)
        {
            int? iRes = null;

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


                    var oInstallations = (from r in dbContext.INSTALLATIONs
                                          where r.INS_ID == dInstallationId
                                          select r).ToArray();
                    if (oInstallations.Count() == 1)
                    {
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        DateTime dtInstallation = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                        DateTime dtUTC = TimeZoneInfo.ConvertTime(dtInstallation, tzi, TimeZoneInfo.Utc);

                        iRes = Convert.ToInt32((dtInstallation - dtUTC).TotalMinutes);

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInstallationUTCOffSetInMinutes: ", e);
            }

            return iRes;


        }


        public DateTime? getGroupDateTime(decimal dGroupID)
        {

            DateTime? dtRes = null;

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


                    var oInstallations = (from r in dbContext.INSTALLATIONs
                                          join g in dbContext.GROUPs on r.INS_ID equals g.GRP_INS_ID
                                          where g.GRP_ID == dGroupID
                                          select r).ToArray();
                    if (oInstallations.Count() == 1)
                    {
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oInstallations[0].INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        dtRes = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupDateTime: ", e);
            }

            return dtRes;

        }


        public IEnumerable<stZone> getInstallationGroupHierarchy(decimal dInstallationId, GroupType groupType = GroupType.OnStreet)
        {

            List<stZone> res = new List<stZone>();

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

                    DateTime? dtNow = getInstallationDateTime(dInstallationId);

                    if (dtNow != null)
                    {


                        var firstLevelGrops = (from g in dbContext.GROUPs
                                               join gh in dbContext.GROUPS_HIERARCHies on g.GRP_ID equals gh.GRHI_GPR_ID_CHILD
                                               where g.GRP_INS_ID == dInstallationId &&
                                                     gh.GRHI_GPR_ID_PARENT == null &&
                                                     gh.GRHI_INI_APPLY_DATE <= (DateTime)dtNow &&
                                                     gh.GRHI_END_APPLY_DATE >= (DateTime)dtNow &&
                                                     g.GRP_TYPE == (int) groupType
                                               orderby g.GRP_ID
                                               select g).ToArray();

                        if (firstLevelGrops.Count() > 0)
                        {
                            foreach (GROUP group in firstLevelGrops)
                            {
                                stZone newZone = new stZone
                                                {
                                                    level = 0,
                                                    dID = group.GRP_ID,
                                                    strDescription = group.GRP_DESCRIPTION,
                                                    dLiteralID = group.GRP_LIT_ID,
                                                    strColour = group.GRP_COLOUR,
                                                    strShowId = group.GRP_SHOW_ID,
                                                    subzones = new List<stZone>(),
                                                    GPSpolygons = new List<stGPSPolygon>(),
                                                    GroupType = (GroupType) group.GRP_TYPE,
                                                    Occupancy = (float) (100 - (group.GRP_FREE_SPACES_PERC ?? 0)),
                                                    ParkingType = group.GRP_OFFSTREET_TYPE ?? 0
                                                };

                                res.Add(newZone);

                                getInstallationGroupChilds(dbContext,
                                                           (DateTime)dtNow,
                                                           1,
                                                           groupType,
                                                           ref newZone);


                                foreach (int iPolNumber in group.GROUPS_GEOMETRies
                                        .Where(r => r.GRGE_INI_APPLY_DATE <= (DateTime)dtNow &&
                                                    r.GRGE_END_APPLY_DATE >= (DateTime)dtNow)
                                        .GroupBy(r => r.GRGE_POL_NUMBER)
                                        .OrderBy(r => r.Key)
                                        .Select(r => r.Key))
                                {

                                    stGPSPolygon oGPSPolygon = new stGPSPolygon();
                                    oGPSPolygon.GPSpolygon = new List<stGPSPoint>();
                                    oGPSPolygon.iPolNumber = iPolNumber;

                                    foreach (GROUPS_GEOMETRY oGeometry in group.GROUPS_GEOMETRies
                                            .Where(r => r.GRGE_INI_APPLY_DATE <= (DateTime)dtNow &&
                                                        r.GRGE_END_APPLY_DATE >= (DateTime)dtNow &&
                                                        r.GRGE_POL_NUMBER == iPolNumber)
                                            .OrderBy(r => r.GRGE_ORDER))
                                    {
                                        stGPSPoint gpsPoint = new stGPSPoint
                                                            {
                                                                order = oGeometry.GRGE_ORDER,
                                                                dLatitude = oGeometry.GRGE_LATITUDE,
                                                                dLongitude = oGeometry.GRGE_LONGITUDE
                                                            };
                                        ((List<stGPSPoint>)oGPSPolygon.GPSpolygon).Add(gpsPoint);

                                    }

                                    ((List<stGPSPolygon>)newZone.GPSpolygons).Add(oGPSPolygon);
                                }


                            }

                        }

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallationGroupHierarchy: ", e);
            }

            return (IEnumerable<stZone>)res;

        }



        public IEnumerable<stTariff> getInstallationTariffs(decimal dInstallationId)
        {

            List<stTariff> res = new List<stTariff>();

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

                    DateTime? dtNow = getInstallationDateTime(dInstallationId);

                    if (dtNow != null)
                    {

                        Hashtable tariffHash = new Hashtable();

                        var Tariffs = (from g in dbContext.TARIFFS_IN_GROUPs
                                       where g.TARGR_INI_APPLY_DATE <= (DateTime)dtNow &&
                                             g.TARGR_END_APPLY_DATE >= (DateTime)dtNow
                                       orderby g.TARGR_TAR_ID
                                       select g).ToArray();

                        if (Tariffs.Count() > 0)
                        {
                            foreach (TARIFFS_IN_GROUP tariff in Tariffs)
                            {
                                if (tariff.TARGR_GRP_ID != null)
                                {
                                    if (tariff.GROUP.GRP_INS_ID == dInstallationId)
                                    {
                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                            else
                                            {
                                                if (!((List<decimal>)oldTariff.zones).Exists(element => element == ((decimal)tariff.TARGR_GRP_ID)))
                                                {
                                                    ((List<decimal>)oldTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);
                                                }
                                            }
                                        }

                                    }

                                }
                                else if (tariff.TARGR_GRPT_ID != null)
                                {
                                    if (tariff.GROUPS_TYPE.GRPT_INS_ID == dInstallationId)
                                    {

                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                            {
                                                ((List<decimal>)newTariff.zones).Add((decimal)group_assig.GTA_GRP_ID);
                                            }



                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                                {
                                                    ((List<decimal>)newTariff.zones).Add((decimal)group_assig.GTA_GRP_ID);
                                                }

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                            else
                                            {


                                                foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                                {
                                                    if (!((List<decimal>)oldTariff.zones).Exists(element => element == ((decimal)group_assig.GTA_GRP_ID)))
                                                    {
                                                        ((List<decimal>)oldTariff.zones).Add((decimal)group_assig.GTA_GRP_ID);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallationTariffs: ", e);
            }

            return (IEnumerable<stTariff>)res;

        }



        public IEnumerable<stTariff> getGroupTariffs(decimal dGroupId)
        {

            List<stTariff> res = new List<stTariff>();

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

                    DateTime? dtNow = getGroupDateTime(dGroupId);

                    if (dtNow != null)
                    {

                        Hashtable tariffHash = new Hashtable();

                        var Tariffs = (from g in dbContext.TARIFFS_IN_GROUPs
                                       where g.TARGR_INI_APPLY_DATE <= (DateTime)dtNow &&
                                             g.TARGR_END_APPLY_DATE >= (DateTime)dtNow
                                       orderby g.TARGR_TAR_ID
                                       select g).ToArray();

                        if (Tariffs.Count() > 0)
                        {
                            foreach (TARIFFS_IN_GROUP tariff in Tariffs)
                            {
                                if (tariff.TARGR_GRP_ID != null)
                                {
                                    if (tariff.TARGR_GRP_ID == dGroupId)
                                    {
                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                            else
                                            {
                                                if (!((List<decimal>)oldTariff.zones).Exists(element => element == ((decimal)tariff.TARGR_GRP_ID)))
                                                {
                                                    ((List<decimal>)oldTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);
                                                }
                                            }
                                        }

                                    }

                                }
                                else if (tariff.TARGR_GRPT_ID != null)
                                {
                                    bool bIsGroupInType = false;

                                    foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                    {
                                        bIsGroupInType = (group_assig.GTA_GRP_ID == dGroupId);
                                        if (bIsGroupInType)
                                            break;

                                    }

                                    if (bIsGroupInType)
                                    {

                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            ((List<decimal>)newTariff.zones).Add(dGroupId);

                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                ((List<decimal>)newTariff.zones).Add(dGroupId);

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupTariffs: ", e);
            }

            return (IEnumerable<stTariff>)res;

        }



        public IEnumerable<stTariff> getGroupTariffs(decimal dGroupId, decimal? dLatitude, decimal? dLongitude)
        {

            List<stTariff> res = new List<stTariff>();

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

                    DateTime? dtNow = getGroupDateTime(dGroupId);

                    if (dtNow != null)
                    {

                        Hashtable tariffHash = new Hashtable();

                        var Tariffs = (from g in dbContext.TARIFFS_IN_GROUPs
                                       where g.TARGR_INI_APPLY_DATE <= (DateTime)dtNow &&
                                             g.TARGR_END_APPLY_DATE >= (DateTime)dtNow
                                       orderby g.TARGR_TAR_ID
                                       select g).ToArray();

                        if (Tariffs.Count() > 0)
                        {
                            foreach (TARIFFS_IN_GROUP tariff in Tariffs)
                            {
                                if (tariff.TARGR_GRP_ID != null)
                                {
                                    if (tariff.TARGR_GRP_ID == dGroupId)
                                    {
                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                ((List<decimal>)newTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                            else
                                            {
                                                if (!((List<decimal>)oldTariff.zones).Exists(element => element == ((decimal)tariff.TARGR_GRP_ID)))
                                                {
                                                    ((List<decimal>)oldTariff.zones).Add((decimal)tariff.TARGR_GRP_ID);
                                                }
                                            }
                                        }

                                    }

                                }
                                else if (tariff.TARGR_GRPT_ID != null)
                                {
                                    bool bIsGroupInType = false;

                                    foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                    {
                                        bIsGroupInType = (group_assig.GTA_GRP_ID == dGroupId);
                                        if (bIsGroupInType)
                                            break;

                                    }

                                    if (bIsGroupInType)
                                    {

                                        if (tariffHash[tariff.TARGR_TAR_ID] == null)
                                        {
                                            stTariff newTariff = new stTariff
                                            {
                                                dID = tariff.TARGR_TAR_ID,
                                                dLiteralID = tariff.TARGR_LIT_ID,
                                                strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                zones = new List<decimal>()
                                            };

                                            ((List<decimal>)newTariff.zones).Add(dGroupId);

                                            tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                            res.Add(newTariff);

                                        }
                                        else
                                        {
                                            stTariff oldTariff = (stTariff)tariffHash[tariff.TARGR_TAR_ID];
                                            bool bNewUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1);
                                            if ((oldTariff.bUserSelectable != bNewUserSelectable) ||
                                                (oldTariff.dLiteralID != tariff.TARGR_LIT_ID))
                                            {
                                                stTariff newTariff = new stTariff
                                                {
                                                    dID = tariff.TARGR_TAR_ID,
                                                    dLiteralID = tariff.TARGR_LIT_ID,
                                                    strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                                    bUserSelectable = (tariff.TARGR_USER_SELECTABLE == 1),
                                                    zones = new List<decimal>()
                                                };

                                                ((List<decimal>)newTariff.zones).Add(dGroupId);

                                                tariffHash[tariff.TARGR_TAR_ID] = newTariff;
                                                res.Add(newTariff);

                                            }
                                        }
                                    }
                                }
                            }
                        }


                        if ((res.Count() > 0) && (dLatitude.HasValue) && (dLongitude.HasValue))
                        {
                            List<stTariff> lstTemp = res;                          
                            res = new List<stTariff>();

                            foreach (stTariff tariff in lstTemp)
                            {

                                bool bIsInside = false;
                                var PolygonNumbers = (from r in dbContext.TARIFFS_IN_GROUPS_GEOMETRies
                                                      where ((r.TAGRGE_GRP_ID == dGroupId) &&
                                                      (r.TAGRGE_TAR_ID == tariff.dID) &&
                                                      (r.TAGRGE_INI_APPLY_DATE <= (DateTime)dtNow) &&
                                                      (r.TAGRGE_END_APPLY_DATE >= (DateTime)dtNow))
                                                      group r by new
                                                      {
                                                          r.TAGRGE_POL_NUMBER
                                                      } into g
                                                      orderby g.Key.TAGRGE_POL_NUMBER
                                                      select new { iPolNumber = g.Key.TAGRGE_POL_NUMBER }).ToList();


                                if (PolygonNumbers.Count() > 0)
                                {
                                    Point p = new Point(Convert.ToDouble(dLongitude), Convert.ToDouble(dLatitude));

                                    foreach (var oPolNumber in PolygonNumbers)
                                    {
                                        var Polygon = (from r in dbContext.TARIFFS_IN_GROUPS_GEOMETRies
                                                       where ((r.TAGRGE_GRP_ID == dGroupId) &&
                                                       (r.TAGRGE_TAR_ID == tariff.dID) &&
                                                       (r.TAGRGE_INI_APPLY_DATE <= (DateTime)dtNow) &&
                                                       (r.TAGRGE_END_APPLY_DATE >= (DateTime)dtNow) &&
                                                       (r.TAGRGE_POL_NUMBER == oPolNumber.iPolNumber))
                                                       orderby r.TAGRGE_ORDER
                                                       select new Point(Convert.ToDouble(r.TAGRGE_LONGITUDE),
                                                                        Convert.ToDouble(r.TAGRGE_LATITUDE))).ToArray();


                                        if (Polygon.Count() > 0)
                                        {
                                            if (IsPointInsidePolygon(p, Polygon))
                                            {
                                                bIsInside = true;
                                                break;
                                            }

                                        }

                                    }
                                }
                                else
                                {
                                    //no poligons defined for tariff in group so not filter by gps position
                                    bIsInside = true;
                                }


                                if (bIsInside)
                                {
                                    res.Add(tariff);
                                }
                            }


                            if (res.Count() == 0)
                            {
                                res = lstTemp;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupTariffs: ", e);
            }

            return (IEnumerable<stTariff>)res;

        }


        public IEnumerable<stTariff> getPlateTariffsInGroup(string strPlate, decimal dGroupId, decimal? dLatitude, decimal? dLongitude)
        {

            List<stTariff> res = new List<stTariff>();

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

                    DateTime? dtNow = getGroupDateTime(dGroupId);

                    if (dtNow != null)
                    {

                        var Tariffs = (from g in dbContext.PLATES_TARIFFs
                                       where g.PLTA_INI_APPLY_DATE <= (DateTime)dtNow &&
                                             g.PLTA_END_APPLY_DATE >= (DateTime)dtNow &&
                                             g.PLTA_PLATE.ToUpper().Trim() == strPlate.ToUpper().Trim()
                                       orderby g.PLTA_TAR_ID
                                       select g).ToArray();

                        if (Tariffs.Count() > 0)
                        {
                            foreach (PLATES_TARIFF tariff in Tariffs)
                            {
                                if (tariff.PLTA_GRP_ID != null)
                                {
                                    if (tariff.PLTA_GRP_ID == dGroupId)
                                    {

                                        stTariff newTariff = new stTariff
                                        {
                                            dID = tariff.PLTA_TAR_ID,
                                            dLiteralID = tariff.TARIFF.TAR_LIT_ID,
                                            strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                            bUserSelectable = false,
                                            zones = new List<decimal>()
                                        };

                                        ((List<decimal>)newTariff.zones).Add((decimal)tariff.PLTA_GRP_ID);

                                        res.Add(newTariff);

                                        break;

                                    }

                                }
                                else if (tariff.PLTA_GRPT_ID != null)
                                {
                                    bool bIsGroupInType = false;

                                    foreach (GROUPS_TYPES_ASSIGNATION group_assig in tariff.GROUPS_TYPE.GROUPS_TYPES_ASSIGNATIONs)
                                    {
                                        bIsGroupInType = (group_assig.GTA_GRP_ID == dGroupId);
                                        if (bIsGroupInType)
                                            break;

                                    }

                                    if (bIsGroupInType)
                                    {

                                        stTariff newTariff = new stTariff
                                        {
                                            dID = tariff.PLTA_TAR_ID,
                                            dLiteralID = tariff.TARIFF.TAR_LIT_ID,
                                            strDescription = tariff.TARIFF.TAR_DESCRIPTION,
                                            bUserSelectable = false,
                                            zones = new List<decimal>()
                                        };

                                        ((List<decimal>)newTariff.zones).Add(dGroupId);

                                        res.Add(newTariff);

                                        break;

                                    }
                                }
                            }
                        }

                        if ((res.Count() > 0) && (dLatitude.HasValue) && (dLongitude.HasValue))
                        {
                            List<stTariff> lstTemp = res;
                            res = new List<stTariff>();

                            foreach (stTariff tariff in lstTemp)
                            {

                                bool bIsInside = false;
                                var PolygonNumbers = (from r in dbContext.TARIFFS_IN_GROUPS_GEOMETRies
                                                      where ((r.TAGRGE_GRP_ID == dGroupId) &&
                                                      (r.TAGRGE_TAR_ID == tariff.dID) &&
                                                      (r.TAGRGE_INI_APPLY_DATE <= (DateTime)dtNow) &&
                                                      (r.TAGRGE_END_APPLY_DATE >= (DateTime)dtNow))
                                                      group r by new
                                                      {
                                                          r.TAGRGE_POL_NUMBER
                                                      } into g
                                                      orderby g.Key.TAGRGE_POL_NUMBER
                                                      select new { iPolNumber = g.Key.TAGRGE_POL_NUMBER }).ToList();


                                if (PolygonNumbers.Count() > 0)
                                {
                                    Point p = new Point(Convert.ToDouble(dLongitude), Convert.ToDouble(dLatitude));

                                    foreach (var oPolNumber in PolygonNumbers)
                                    {
                                        var Polygon = (from r in dbContext.TARIFFS_IN_GROUPS_GEOMETRies
                                                       where ((r.TAGRGE_GRP_ID == dGroupId) &&
                                                       (r.TAGRGE_TAR_ID == tariff.dID) &&
                                                       (r.TAGRGE_INI_APPLY_DATE <= (DateTime)dtNow) &&
                                                       (r.TAGRGE_END_APPLY_DATE >= (DateTime)dtNow) &&
                                                       (r.TAGRGE_POL_NUMBER == oPolNumber.iPolNumber))
                                                       orderby r.TAGRGE_ORDER
                                                       select new Point(Convert.ToDouble(r.TAGRGE_LONGITUDE),
                                                                        Convert.ToDouble(r.TAGRGE_LATITUDE))).ToArray();


                                        if (Polygon.Count() > 0)
                                        {
                                            if (IsPointInsidePolygon(p, Polygon))
                                            {
                                                bIsInside = true;
                                                break;
                                            }

                                        }

                                    }
                                }
                                else
                                {
                                    //no poligons defined for tariff in group so not filter by gps position
                                    bIsInside = true;
                                }


                                if (bIsInside)
                                {
                                    res.Add(tariff);
                                }
                            }
                        }


                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getPlateTariffsInGroup: ", e);
            }

            return (IEnumerable<stTariff>)res;

        }





        public bool GetGroupAndTariffExternalTranslation(int iWSNumber,GROUP oGroup, TARIFF oTariff, ref string strExtGroupId, ref string strTarExtId)
        {
            bool bRes = false;
            strExtGroupId = "";
            strTarExtId = "";
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

                    if ((oGroup != null) && (oTariff != null))
                    {

                        if ((iWSNumber == 0) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_QUERY_EXT_ID)) &&
                            (!string.IsNullOrEmpty(oTariff.TAR_QUERY_EXT_ID)))
                        {
                            strExtGroupId = oGroup.GRP_QUERY_EXT_ID;
                            strTarExtId = oTariff.TAR_QUERY_EXT_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 1) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT1_ID)) &&
                            (!string.IsNullOrEmpty(oTariff.TAR_EXT1_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT1_ID;
                            strTarExtId = oTariff.TAR_EXT1_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 2) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT2_ID)) &&
                            (!string.IsNullOrEmpty(oTariff.TAR_EXT2_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT2_ID;
                            strTarExtId = oTariff.TAR_EXT2_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 3) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT3_ID)) &&
                            (!string.IsNullOrEmpty(oTariff.TAR_EXT3_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT3_ID;
                            strTarExtId = oTariff.TAR_EXT3_ID;
                            bRes = true;
                        }
                        else
                        {

                            var oGroupTranslation = (from r in dbContext.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs
                                                     where r.GTET_IN_GRP_ID == oGroup.GRP_ID &&
                                                           r.GTET_IN_TAR_ID == oTariff.TAR_ID &&
                                                           r.GTET_WS_NUMBER == iWSNumber
                                                     select r).ToArray();

                            if (oGroupTranslation.Count() == 1)
                            {
                                strExtGroupId = oGroupTranslation.First().GTET_OUT_GRP_EXT_ID;
                                strTarExtId = oGroupTranslation.First().GTET_OUT_TAR_EXT_ID;
                                bRes = ((!string.IsNullOrEmpty(strExtGroupId)) &&
                                        (!string.IsNullOrEmpty(strTarExtId)));

                            }


                        }
                    }
                    else if ((oGroup != null) && (oTariff == null))
                    {
                        if ((iWSNumber == 0) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_QUERY_EXT_ID)))
                        {
                            strExtGroupId = oGroup.GRP_QUERY_EXT_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 1) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT1_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT1_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 2) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT2_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT2_ID;
                            bRes = true;
                        }
                        else if ((iWSNumber == 3) &&
                            (!string.IsNullOrEmpty(oGroup.GRP_EXT3_ID)))
                        {
                            strExtGroupId = oGroup.GRP_EXT3_ID;
                            bRes = true;
                        }
                        else
                        {

                            var oGroupTranslation = (from r in dbContext.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs
                                                     where r.GTET_IN_GRP_ID == oGroup.GRP_ID &&
                                                           r.GTET_IN_TAR_ID == null &&
                                                           r.GTET_WS_NUMBER == iWSNumber
                                                     select r).ToArray();

                            if (oGroupTranslation.Count() == 1)
                            {
                                strExtGroupId = oGroupTranslation.First().GTET_OUT_GRP_EXT_ID;
                                strTarExtId = oGroupTranslation.First().GTET_OUT_TAR_EXT_ID;
                                bRes = ((!string.IsNullOrEmpty(strExtGroupId)) &&
                                        (!string.IsNullOrEmpty(strTarExtId)));

                            }


                        }

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGroupAndTariffExternalTranslation: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetGroupAndTariffExternalTranslation(int iWSNumber, decimal dGroupId,decimal dTariffId, ref string strExtGroupId, ref string strTarExtId)
        {
            bool bRes = false;
            strExtGroupId = "";
            strTarExtId = "";
            try
            {
                GROUP oGroup = null;
                TARIFF oTariff = null;

                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                                             new TransactionOptions()
                                                                                             {
                                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                                 Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = DataContextFactory.GetScopedDataContext<integraMobileDBEntitiesDataContext>();

                    oGroup = (from g in dbContext.GROUPs
                                    where g.GRP_ID == dGroupId
                                    select g).First();

                    oTariff = (from g in dbContext.TARIFFs
                                    where g.TAR_ID == dTariffId
                                    select g).First();


                }

                return GetGroupAndTariffExternalTranslation(iWSNumber, oGroup, oTariff, ref strExtGroupId, ref strTarExtId);

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGroupAndTariffExternalTranslation: ", e);
                bRes = false;
            }

            return bRes;

        }





        public bool GetGroupAndTariffFromExternalId(int iWSNumber,INSTALLATION oInstallation, string strExtGroupId, string strExtTarId, ref decimal? dGroupId, ref decimal? dTariffId)
        {
            bool bRes = false;
            dGroupId = null;
            dTariffId = null;

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

                    GROUP[] oCandidateGroup = null;

                    switch (iWSNumber)
                    {
                        case 0:
                            oCandidateGroup = oInstallation.GROUPs.Where(r => r.GRP_QUERY_EXT_ID == strExtGroupId && r.GRP_INS_ID == oInstallation.INS_ID).ToArray();
                            break;
                        case 1:
                            oCandidateGroup = oInstallation.GROUPs.Where(r => r.GRP_EXT1_ID == strExtGroupId && r.GRP_INS_ID == oInstallation.INS_ID).ToArray();
                            break;
                        case 2:
                            oCandidateGroup = oInstallation.GROUPs.Where(r => r.GRP_EXT2_ID == strExtGroupId && r.GRP_INS_ID == oInstallation.INS_ID).ToArray();
                            break;
                        case 3:
                            oCandidateGroup = oInstallation.GROUPs.Where(r => r.GRP_EXT3_ID == strExtGroupId && r.GRP_INS_ID == oInstallation.INS_ID).ToArray();
                            break;
                        case 4:
                            oCandidateGroup = oInstallation.GROUPs.Where(r => r.GRP_ID_FOR_EXT_OPS == strExtGroupId && r.GRP_INS_ID == oInstallation.INS_ID).ToArray();
                            break;
                        default:
                            break;

                    }



                    if (oCandidateGroup.Count() == 1)
                    {

                        var oCandidateTariffs = oCandidateGroup.First().TARIFFS_IN_GROUPs.ToArray();

                        foreach (TARIFFS_IN_GROUP oCandidateTariff in oCandidateTariffs)
                        {
                            bool bFound = false;
                            switch (iWSNumber)
                            {
                                case 0:
                                    bFound = (oCandidateTariff.TARIFF.TAR_QUERY_EXT_ID == strExtTarId);
                                    break;
                                case 1:
                                    bFound = (oCandidateTariff.TARIFF.TAR_EXT1_ID == strExtTarId);
                                    break;
                                case 2:
                                    bFound = (oCandidateTariff.TARIFF.TAR_EXT2_ID == strExtTarId);
                                    break;
                                case 3:
                                    bFound = (oCandidateTariff.TARIFF.TAR_EXT3_ID == strExtTarId);
                                    break;
                                case 4:
                                    bFound = (oCandidateTariff.TARIFF.TAR_ID_FOR_EXT_OPS == strExtTarId);
                                    break;
                                default:
                                    break;

                            }


                            if (bFound)
                            {
                                bRes = true;
                                dGroupId = oCandidateGroup.First().GRP_ID;
                                dTariffId = oCandidateTariff.TARIFF.TAR_ID;
                                break;
                            }

                        }


                        if (!bRes)
                        {
                            foreach (GROUPS_TYPES_ASSIGNATION oAssigns in oCandidateGroup.First().GROUPS_TYPES_ASSIGNATIONs)
                            {

                                oCandidateTariffs = oAssigns.GROUPS_TYPE.TARIFFS_IN_GROUPs.ToArray();

                                foreach (TARIFFS_IN_GROUP oCandidateTariff in oCandidateTariffs)
                                {

                                    bool bFound = false;
                                    switch (iWSNumber)
                                    {
                                        case 0:
                                            bFound = (oCandidateTariff.TARIFF.TAR_QUERY_EXT_ID == strExtTarId);
                                            break;
                                        case 1:
                                            bFound = (oCandidateTariff.TARIFF.TAR_EXT1_ID == strExtTarId);
                                            break;
                                        case 2:
                                            bFound = (oCandidateTariff.TARIFF.TAR_EXT2_ID == strExtTarId);
                                            break;
                                        case 3:
                                            bFound = (oCandidateTariff.TARIFF.TAR_EXT3_ID == strExtTarId);
                                            break;
                                        case 4:
                                            bFound = (oCandidateTariff.TARIFF.TAR_ID_FOR_EXT_OPS == strExtTarId);
                                            break;
                                        default:
                                            break;

                                    }

                                    if (bFound)
                                    {
                                        bRes = true;
                                        dGroupId = oCandidateGroup.First().GRP_ID;
                                        dTariffId = oCandidateTariff.TARIFF.TAR_ID;
                                        break;
                                    }

                                }

                                if (bRes)
                                {
                                    break;
                                }
                            }


                        }


                    }



                    if (!bRes)
                    {
                        var oGroupTranslations = (from r in dbContext.GROUPS_TARIFFS_EXTERNAL_TRANSLATIONs
                                                  where r.GTET_OUT_GRP_EXT_ID == strExtGroupId &&
                                                        r.GTET_OUT_TAR_EXT_ID == strExtTarId &&
                                                        r.GTET_WS_NUMBER == iWSNumber
                                                  select r).ToArray();

                        if (oGroupTranslations.Count() == 1)
                        {
                            dGroupId = oGroupTranslations.First().GTET_IN_GRP_ID;
                            dTariffId = oGroupTranslations.First().GTET_IN_TAR_ID;
                            bRes = true;


                        }
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGroupAndTariffFromExternalId: ", e);
                bRes = false;
            }

            return bRes;

        }


        public bool GetGroupAndTariffStepOffsetMinutes(GROUP oGroup, TARIFF oTariff, out int? iOffset)
        {
            bool bReturn = true;
            iOffset = null;

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
                    decimal dInstallationId = oGroup.INSTALLATION.INS_ID;
                    DateTime? dtNow = getInstallationDateTime(dInstallationId);

                    if (dtNow != null)
                    {

                        Hashtable tariffHash = new Hashtable();

                        var Tariffs = (from g in dbContext.TARIFFS_IN_GROUPs
                                       where g.TARGR_INI_APPLY_DATE <= (DateTime)dtNow &&
                                             g.TARGR_END_APPLY_DATE >= (DateTime)dtNow &&
                                             g.TARIFF.TAR_ID == oTariff.TAR_ID
                                       orderby g.TARGR_TAR_ID,
                                               g.TARGR_GRP_ID.HasValue descending, g.TARGR_GRP_ID,
                                               g.TARGR_GRPT_ID
                                       select g).ToArray();

                        if (Tariffs.Count() > 0)
                        {
                            foreach (TARIFFS_IN_GROUP tariff in Tariffs)
                            {
                                if (tariff.TARGR_GRP_ID != null)
                                {
                                    if (tariff.GROUP.GRP_INS_ID == dInstallationId)
                                    {

                                        if (tariff.TARGR_GRP_ID == oGroup.GRP_ID)
                                        {
                                            iOffset = tariff.TARGR_TIME_STEPS_VALUE;
                                            break;
                                        }
                                    }

                                }
                                else if (tariff.TARGR_GRPT_ID != null)
                                {
                                    if (tariff.GROUPS_TYPE.GRPT_INS_ID == dInstallationId)
                                    {

                                        var oGroupTypes = oGroup.GROUPS_TYPES_ASSIGNATIONs.Where(r => r.GTA_GRPT_ID == tariff.GROUPS_TYPE.GRPT_ID);

                                        if (oGroupTypes.Count() > 0)
                                        {
                                            iOffset = tariff.TARGR_TIME_STEPS_VALUE;
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetGroupAndTariffStepOffsetMinutes: ", e);
                bReturn=false;
            }

            return bReturn;

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



        private bool getInstallationGroupChilds(integraMobileDBEntitiesDataContext dbContext,
                                                DateTime dtNow,
                                                int ilevel,
                                                GroupType groupType,
                                                ref stZone zone)
        {
            bool bRes = true;
            try
            {

                decimal zoneid = zone.dID;

                var Groups = (from g in dbContext.GROUPs
                                       join gh in dbContext.GROUPS_HIERARCHies on g.GRP_ID equals gh.GRHI_GPR_ID_CHILD
                                        where gh.GRHI_GPR_ID_PARENT == zoneid &&
                                              gh.GRHI_INI_APPLY_DATE <= (DateTime)dtNow &&
                                              gh.GRHI_END_APPLY_DATE >= (DateTime)dtNow &&
                                              g.GRP_TYPE == (int) groupType
                                        orderby g.GRP_ID
                                       select g).ToArray();

                if (Groups.Count() > 0)
                {
                    foreach (GROUP group in Groups)
                    {
                        stZone newZone=new stZone
                        {
                            level = ilevel,
                            dID = group.GRP_ID,
                            strDescription = group.GRP_DESCRIPTION,
                            dLiteralID = group.GRP_LIT_ID,
                            strColour = group.GRP_COLOUR,
                            strShowId = group.GRP_SHOW_ID,
                            subzones = new List<stZone>(),
                            GPSpolygons = new List<stGPSPolygon>(),
                            GroupType = (GroupType) group.GRP_TYPE,
                            Occupancy = (float)(100 - (group.GRP_FREE_SPACES_PERC ?? 0))
                        };



                        foreach (int iPolNumber in group.GROUPS_GEOMETRies
                                                             .Where(r => r.GRGE_INI_APPLY_DATE <= (DateTime)dtNow &&
                                                                         r.GRGE_END_APPLY_DATE >= (DateTime)dtNow)
                                                             .GroupBy(r => r.GRGE_POL_NUMBER)
                                                             .OrderBy(r => r.Key)
                                                             .Select(r => r.Key))
                        {

                            stGPSPolygon oGPSPolygon = new stGPSPolygon();
                            oGPSPolygon.GPSpolygon = new List<stGPSPoint>();
                            oGPSPolygon.iPolNumber = iPolNumber;


                            foreach (GROUPS_GEOMETRY oGeometry in group.GROUPS_GEOMETRies
                                    .Where(r => r.GRGE_INI_APPLY_DATE <= (DateTime)dtNow &&
                                                r.GRGE_END_APPLY_DATE >= (DateTime)dtNow &&
                                                r.GRGE_POL_NUMBER == iPolNumber)
                                    .OrderBy(r => r.GRGE_ORDER))
                            {
                                stGPSPoint gpsPoint = new stGPSPoint
                                {
                                    order = oGeometry.GRGE_ORDER,
                                    dLatitude = oGeometry.GRGE_LATITUDE,
                                    dLongitude = oGeometry.GRGE_LONGITUDE
                                };
                                ((List<stGPSPoint>)oGPSPolygon.GPSpolygon).Add(gpsPoint);

                            }

                            ((List<stGPSPolygon>)newZone.GPSpolygons).Add(oGPSPolygon);
                        }


                        ((List<stZone>)zone.subzones).Add(newZone);

                        getInstallationGroupChilds(dbContext,
                                                    dtNow,
                                                    ilevel + 1,
                                                    groupType,
                                                    ref newZone);
                    }

                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getInstallationGroupChilds: ", e);
                bRes = false;
            }

            return bRes;

        }




        private bool IsPointInsidePolygon(Point p,Point[] Polygon)
        {
            double dAngle = 0;

            try
            {

                for (int i = 0; i < Polygon.Length; i++)
                {
                    Vector v1 = new Vector(Polygon[i].X - p.X, Polygon[i].Y - p.Y);
                    Vector v2 = new Vector(Polygon[(i + 1) % Polygon.Length].X - p.X,
                                           Polygon[(i + 1) % Polygon.Length].Y - p.Y);

                    dAngle = dAngle + (Vector.AngleBetween(v1, v2) * Math.PI / 180);

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "IsPointInsidePolygon: ", e);
                dAngle = 0;
            }


            return (Math.Abs(dAngle) > Math.PI);

        }

        public bool getExternalProvider(string strName, ref EXTERNAL_PROVIDER oExternalProvider)
        {
            bool bRes = false;
            oExternalProvider = null;            

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


                    if (strName != null)
                    {
                        var oExternalProviders = (from r in dbContext.EXTERNAL_PROVIDERs
                                                  where r.EXP_NAME.ToUpper() == strName.ToUpper()
                                                  select r).ToArray();
                        if (oExternalProviders.Count() == 1)
                        {
                            oExternalProvider = oExternalProviders[0];
                            bRes = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getExternalProvider: ", e);
            }

            return bRes;
        }

        
        public bool getOffStreetConfiguration(decimal? dGroupId, decimal? dLatitude, decimal? dLongitude, ref GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetConfiguration, ref DateTime? dtgroupDateTime)
        {
            bool bRes = false;
            oOffstreetConfiguration = null;            

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

                    if (dGroupId != null)
                    {
                        oOffstreetConfiguration = GetGroupOffstreetWsConfiguration(dGroupId.Value, dbContext);
                    }
                    else if ((dLatitude != null) && (dLongitude != null))
                    {
                        GROUP oGroup = null;
                        var oGroups = (from r in dbContext.GROUPs
                                       where r.GRP_TYPE == (int) GroupType.OffStreet                                       
                                       orderby r.GRP_ID
                                       select r).ToArray();

                        foreach (GROUP oGrp in oGroups)
                        {
                            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oGrp.INSTALLATION.INS_TIMEZONE_ID);
                            DateTime dtServerTime = DateTime.Now;
                            DateTime dtLocalInstTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);

                            var PolygonNumbers = (from r in dbContext.GROUPS_GEOMETRies
                                                  where ((r.GRGE_GRP_ID == oGrp.GRP_ID) &&
                                                  (r.GRGE_INI_APPLY_DATE <= dtLocalInstTime) &&
                                                  (r.GRGE_INI_APPLY_DATE >= dtLocalInstTime))
                                                  group r by new
                                                  {
                                                      r.GRGE_POL_NUMBER
                                                  } into g
                                                  orderby g.Key.GRGE_POL_NUMBER
                                                  select new { iPolNumber = g.Key.GRGE_POL_NUMBER }).ToList();


                            Point p = new Point(Convert.ToDouble(dLongitude), Convert.ToDouble(dLatitude));

                            foreach (var oPolNumber in PolygonNumbers)
                            {


                                var Polygon = (from r in dbContext.GROUPS_GEOMETRies
                                               where ((r.GRGE_GRP_ID == oGrp.GRP_ID) &&
                                               (r.GRGE_INI_APPLY_DATE <= dtLocalInstTime) &&
                                               (r.GRGE_END_APPLY_DATE >= dtLocalInstTime) &&
                                               (r.GRGE_POL_NUMBER == oPolNumber.iPolNumber))
                                               orderby r.GRGE_ORDER
                                               select new Point(Convert.ToDouble(r.GRGE_LONGITUDE),
                                                                Convert.ToDouble(r.GRGE_LATITUDE))).ToArray();

                                if (Polygon.Count() > 0)
                                {
                                    if (IsPointInsidePolygon(p, Polygon))
                                    {
                                        oGroup = oGrp;
                                        break;
                                    }
                                }
                            }

                            if (oGroup != null)
                            {
                                break;
                            }
                        }


                        if (oGroup != null)
                        {
                            oOffstreetConfiguration = GetGroupOffstreetWsConfiguration(oGroup.GRP_ID, dbContext);
                        }

                    }

                    if (oOffstreetConfiguration != null) {
                        bRes = true;
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oOffstreetConfiguration.GROUP.INSTALLATION.INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        dtgroupDateTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getOffStreetConfiguration: ", e);
            }

            return bRes;
        }

        public bool getOffStreetConfigurationByExtOpsId(string sExtGroupId, ref GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetConfiguration, ref DateTime? dtgroupDateTime)
        {
            bool bRes = false;
            oOffstreetConfiguration = null;

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

                    var groups = (from r in dbContext.GROUPs
                                  where r.GRP_TYPE == (int)GroupType.OffStreet &&
                                        r.GRP_ID_FOR_EXT_OPS == sExtGroupId
                                  select r);
                    if (groups.Count() == 1)
                    {
                        oOffstreetConfiguration = GetGroupOffstreetWsConfiguration(groups.First().GRP_ID, dbContext);
                    }
                    
                    if (oOffstreetConfiguration != null)
                    {
                        bRes = true;
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oOffstreetConfiguration.GROUP.INSTALLATION.INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        dtgroupDateTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                    }

                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getOffStreetConfigurationByExtId: ", e);
            }

            return bRes;
        }

        public bool getGroupByExtOpsId(string sExtGroupId,
                                        ref GROUP oGroup,
                                        ref DateTime? dtgroupDateTime)
        {
            bool bRes = false;
            oGroup = null;
            dtgroupDateTime = null;

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

                    var oGroups = (from r in dbContext.GROUPs
                                    where r.GRP_ID_FOR_EXT_OPS == sExtGroupId
                                    select r).ToArray();
                    if (oGroups.Count() == 1)
                    {
                        oGroup = oGroups[0];
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(oGroups[0].INSTALLATION.INS_TIMEZONE_ID);
                        DateTime dtServerTime = DateTime.Now;
                        dtgroupDateTime = TimeZoneInfo.ConvertTime(dtServerTime, TimeZoneInfo.Local, tzi);
                        bRes = true;
                    }
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupByExtOpsId: ", e);
            }

            return bRes;
        }


        public bool GetStreetSectionsUpdateInstallations(out List<INSTALLATION> oInstallations)
        {
            bool bRes = false;
            oInstallations = new List<INSTALLATION>(); 
         
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

                    oInstallations = (from r in dbContext.INSTALLATIONs
                                   where r.INS_ENABLED == 1 && r.INS_STREET_SECTION_UPDATE_WS_SIGNATURE_TYPE != (int)StreetSectionsUpdate.no_call
                                   select r).ToList();

                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupByExtOpsId: ", e);
            }

            return bRes;
        }


        public bool GetInstallationsStreetSections(decimal dInstallationID, out List<StreetSectionData> oStreetSectionsData, out Dictionary<int, GridElement> oGrid)
        {
            bool bRes = false;
            oStreetSectionsData = new List<StreetSectionData>();
            oGrid = new Dictionary<int, GridElement>();

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

                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {

                        foreach (STREET_SECTIONS_GRID oGridElement in oInstallation.STREET_SECTIONS_GRIDs.OrderBy(r=> r.STRSEG_X).OrderBy(r=> r.STRSEG_Y))
                        {
                            GridElement oDataGridElement = new GridElement()
                                    {
                                        id = Convert.ToInt32(oGridElement.STRSEG_ID),
                                        description = oGridElement.STRSEG_DESCRIPTION,
                                        x = oGridElement.STRSEG_X,
                                        y = oGridElement.STRSEG_Y,
                                        maxX = oGridElement.STRSEG_MAX_X,
                                        maxY = oGridElement.STRSEG_MAX_Y,
                                        Polygon = new List<MapPoint>(),
                                        LstStreetSections = new List<StreetSectionData>(),
                                        ReferenceCount=0,
                                    };        
                 
                            foreach (STREET_SECTIONS_GRID_GEOMETRY oGeometry in oGridElement.STREET_SECTIONS_GRID_GEOMETRies.OrderBy(r => r.STRSEGG_ORDER).ToList())
                            {
                                oDataGridElement.Polygon.Add(new MapPoint() { x = oGeometry.STRSEGG_LONGITUDE, y = oGeometry.STRSEGG_LATITUDE });
                            }

                            oGrid[oDataGridElement.id] = oDataGridElement;

                        }


                        foreach (STREET_SECTION oStreetSection in oInstallation.STREET_SECTIONs.OrderBy(r => r.STRSE_ID_EXT))
                        {
                            StreetSectionData oData = new StreetSectionData()
                            {
                                Id = oStreetSection.STRSE_ID_EXT,
                                Colour = oStreetSection.STRSE_COLOUR,
                                Description = oStreetSection.STRSE_DESCRIPTION,
                                Enabled = oStreetSection.STRSE_DELETED==0,
                                idGroup = Convert.ToInt32(oStreetSection.GROUP.GRP_ID),                               
                                Street = oStreetSection.STREET.STR_ID_EXT,
                                StreetFrom = oStreetSection.STREET1.STR_ID_EXT,
                                StreetTo = oStreetSection.STREET2.STR_ID_EXT,
                                Geometry = new List<MapPoint>(),
                                oGridElements = new Dictionary<int,GridElement>(),
                                Tariffs= new List<int>()
                            };


                            foreach(TARIFF_IN_STREETS_SECTIONS_COMPILED oTariff in oStreetSection.TARIFF_IN_STREETS_SECTIONS_COMPILEDs.OrderBy(r=>r.TARSTRSEC_TAR_ID))
                            {
                                oData.Tariffs.Add(Convert.ToInt32(oTariff.TARSTRSEC_TAR_ID));
                            }


                            foreach (STREET_SECTIONS_GEOMETRY oGeometry in oStreetSection.STREET_SECTIONS_GEOMETRies.OrderBy(r => r.STRSEGE_ORDER).ToList())
                            {
                                oData.Geometry.Add(new MapPoint() { x = oGeometry.STRSEGE_LONGITUDE, y = oGeometry.STRSEGE_LATITUDE });
                            }


                            foreach (STREET_SECTIONS_STREET_SECTIONS_GRID oGridAssignatton in oStreetSection.STREET_SECTIONS_STREET_SECTIONS_GRIDs.OrderBy(r => r.STRSESSG_STRSEG_ID).ToList())
                            {
                                oData.oGridElements[Convert.ToInt32(oGridAssignatton.STRSESSG_STRSEG_ID)] = oGrid[Convert.ToInt32(oGridAssignatton.STRSESSG_STRSEG_ID)];
                                oGrid[Convert.ToInt32(oGridAssignatton.STRSESSG_STRSEG_ID)].LstStreetSections.Add(oData);
                                oGrid[Convert.ToInt32(oGridAssignatton.STRSESSG_STRSEG_ID)].ReferenceCount++;
                            }

                            oStreetSectionsData.Add(oData);

                        }


                    }

                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetInstallationsStreetSections: ", e);
            }

            return bRes;


        }




        public bool RecreateStreetSectionsGrid(decimal dInstallationID, ref Dictionary<int, GridElement> oGrid)
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


                    dbContext.STREET_SECTIONS_GRID_GEOMETRies.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_GRID_GEOMETRies.Where(r=>r.STREET_SECTIONS_GRID.INSTALLATION.INS_ID==dInstallationID).AsEnumerable());
                    dbContext.STREET_SECTIONS_STREET_SECTIONS_GRIDs.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_STREET_SECTIONS_GRIDs.Where(r=> r.STREET_SECTIONS_GRID.INSTALLATION.INS_ID==dInstallationID).AsEnumerable());
                    dbContext.STREET_SECTIONS_GRIDs.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_GRIDs.Where(r=>r.INSTALLATION.INS_ID==dInstallationID).AsEnumerable());

                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {

                        foreach (KeyValuePair<int, GridElement> entry in oGrid.OrderBy(r => r.Key))
                        {

                            STREET_SECTIONS_GRID oSectionGrid = new STREET_SECTIONS_GRID()
                            {
                                STRSEG_ID = entry.Value.id,
                                STRSEG_DESCRIPTION = entry.Value.description,
                                STRSEG_X = entry.Value.x,
                                STRSEG_Y = entry.Value.y,
                                STRSEG_MAX_X = entry.Value.maxX,
                                STRSEG_MAX_Y = entry.Value.maxY,
                                STRSEG_INS_ID = dInstallationID,
                            };

                            int i = 1;
                            foreach (MapPoint oPoint in entry.Value.Polygon)
                            {
                                
                                oSectionGrid.STREET_SECTIONS_GRID_GEOMETRies.Add(new STREET_SECTIONS_GRID_GEOMETRY()
                                    {
                                        STRSEGG_LATITUDE = Convert.ToDecimal(oPoint.y),
                                        STRSEGG_LONGITUDE = Convert.ToDecimal(oPoint.x),
                                        STRSEGG_ORDER =i++,
                                        STRSEGG_INI_APPLY_DATE= DateTime.UtcNow.AddDays(-1),
                                        STRSEGG_END_APPLY_DATE= DateTime.UtcNow.AddYears(50),

                                    });
                            }

                            dbContext.STREET_SECTIONS_GRIDs.InsertOnSubmit(oSectionGrid);
                               
                        }

                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
                            bRes = false;
                        }

                    }
                    

                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
            }

            return bRes;


        }


        public bool UpdateStreetSections(decimal dInstallationID, bool bGridRecreated,
                                               ref List<StreetSectionData> oInsertStreetSectionsData, 
                                               ref List<StreetSectionData> oUpdateStreetSectionsData,
                                               ref List<StreetSectionData> oDeleteStreetSectionsData)
        {
            bool bRes = false;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 //Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {
                        STREET oUnknownStreet = oInstallation.STREETs.Where(r => r.STR_DESCRIPTION == "UNKNOWN").FirstOrDefault();

                        STREET_SECTION oLastStreetSection = dbContext.STREET_SECTIONs.OrderByDescending(r => r.STRSE_ID).FirstOrDefault();

                        decimal dNextStreetSectionId = 0;

                        if (oLastStreetSection != null)
                        {
                            dNextStreetSectionId = oLastStreetSection.STRSE_ID;
                        }

                        foreach (StreetSectionData oData in oInsertStreetSectionsData)
                        {
                            dNextStreetSectionId++;

                            STREET_SECTION oStreetSection = new STREET_SECTION()
                            {
                                STRSE_ID = dNextStreetSectionId,
                                STRSE_ID_EXT = oData.Id,
                                STRSE_COLOUR = oData.Colour,
                                STRSE_DELETED = oData.Enabled?0:1,
                                STRSE_DESCRIPTION = oData.Description,
                                STRSE_GRP_ID = oData.idGroup,
                                STRSE_INS_ID = dInstallationID                               
                            };


                            STREET oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.Street).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET = oUnknownStreet;
                            }

                            oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.StreetFrom).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET1 = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET1 = oUnknownStreet;
                            }

                            oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.StreetTo).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET2 = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET2 = oUnknownStreet;
                            }


                            int i = 1;


                            foreach (MapPoint oPoint in oData.Geometry)
                            {

                                oStreetSection.STREET_SECTIONS_GEOMETRies.Add(new STREET_SECTIONS_GEOMETRY()
                                {
                                    STRSEGE_LATITUDE = Convert.ToDecimal(oPoint.y),
                                    STRSEGE_LONGITUDE = Convert.ToDecimal(oPoint.x),
                                    STRSEGE_ORDER = i++,
                                    STRSEGE_INI_APPLY_DATE = DateTime.UtcNow.AddDays(-1),
                                    STRSEGE_END_APPLY_DATE = DateTime.UtcNow.AddYears(50),
                                    STRSEGE_POL_NUMBER = 1,
                                });

                            }
                            if (oData.oGridElements != null)
                            {

                                foreach (KeyValuePair<int, GridElement> entry in oData.oGridElements.OrderBy(r => r.Key))
                                {
                                    oStreetSection.STREET_SECTIONS_STREET_SECTIONS_GRIDs.Add(new STREET_SECTIONS_STREET_SECTIONS_GRID()
                                    {
                                        STRSESSG_STRSEG_ID = entry.Value.id,
                                    });

                                }
                            }

                            foreach (int iTariff in oData.Tariffs)
                            {
                                oStreetSection.TARIFF_IN_STREETS_SECTIONS_COMPILEDs.Add(new TARIFF_IN_STREETS_SECTIONS_COMPILED()
                                {
                                    TARSTRSEC_TAR_ID = Convert.ToDecimal(iTariff),
                                });

                            }


                            oStreetSection.STREET_SECTIONS_OCCUPANCies.Add(new STREET_SECTIONS_OCCUPANCY()
                            {
                                STRSEOC_OCC_NUM_PLACES = 0,
                                STRSEOC_TOTAL_NUM_PLACES = oData.Places,
                                STRSEOC_UTC_DATE = DateTime.UtcNow,
                            });


                            dbContext.STREET_SECTIONs.InsertOnSubmit(oStreetSection);

                            m_Log.LogMessage(LogLevels.logINFO, string.Format("UpdateStreetSections -> Insert Street Section {0}:{1}:{2}",
                                            oInstallation.INS_DESCRIPTION, oData.Id, oData.Description));


                        }


                        foreach (StreetSectionData oData in oUpdateStreetSectionsData)
                        {


                            STREET_SECTION oStreetSection = oInstallation.STREET_SECTIONs.Where(r => r.STRSE_ID_EXT == oData.Id).First();

                            oStreetSection.STRSE_ID_EXT = oData.Id;
                            oStreetSection.STRSE_COLOUR = oData.Colour;
                            oStreetSection.STRSE_DELETED = oData.Enabled ? 0 : 1;
                            oStreetSection.STRSE_DESCRIPTION = oData.Description;
                            oStreetSection.STRSE_GRP_ID = oData.idGroup;
                            oStreetSection.STRSE_INS_ID = dInstallationID;


                            STREET oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.Street).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET = oUnknownStreet;
                            }

                            oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.StreetFrom).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET1 = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET1 = oUnknownStreet;
                            }

                            oStreet = oInstallation.STREETs.Where(r => r.STR_ID_EXT == oData.StreetTo).FirstOrDefault();

                            if (oStreet != null)
                            {
                                oStreetSection.STREET2 = oStreet;
                            }
                            else
                            {
                                oStreetSection.STREET2 = oUnknownStreet;
                            }



                            dbContext.STREET_SECTIONS_GEOMETRies.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_GEOMETRies.Where(r => r.STRSEGE_STRSE_ID==oStreetSection.STRSE_ID).AsEnumerable());

                            int i = 1;                           

                            foreach (MapPoint oPoint in oData.Geometry)
                            {

                                oStreetSection.STREET_SECTIONS_GEOMETRies.Add(new STREET_SECTIONS_GEOMETRY()
                                {
                                    STRSEGE_LATITUDE = Convert.ToDecimal(oPoint.y),
                                    STRSEGE_LONGITUDE = Convert.ToDecimal(oPoint.x),
                                    STRSEGE_ORDER = i++,
                                    STRSEGE_INI_APPLY_DATE = DateTime.UtcNow.AddDays(-1),
                                    STRSEGE_END_APPLY_DATE = DateTime.UtcNow.AddYears(50),
                                    STRSEGE_POL_NUMBER = 1,
                                });

                            }


                            dbContext.STREET_SECTIONS_STREET_SECTIONS_GRIDs.DeleteAllOnSubmit(dbContext.STREET_SECTIONS_STREET_SECTIONS_GRIDs.Where(r => r.STRSESSG_STRSE_ID == oStreetSection.STRSE_ID).AsEnumerable());

                            if (oData.oGridElements != null)
                            {
                                foreach (KeyValuePair<int, GridElement> entry in oData.oGridElements.OrderBy(r => r.Key))
                                {
                                    oStreetSection.STREET_SECTIONS_STREET_SECTIONS_GRIDs.Add(new STREET_SECTIONS_STREET_SECTIONS_GRID()
                                    {
                                        STRSESSG_STRSEG_ID = entry.Value.id,
                                    });

                                }
                            }

                            dbContext.TARIFF_IN_STREETS_SECTIONS_COMPILEDs.DeleteAllOnSubmit(dbContext.TARIFF_IN_STREETS_SECTIONS_COMPILEDs.Where(r => r.TARSTRSEC_STRSE_ID == oStreetSection.STRSE_ID).AsEnumerable());

                            foreach (int iTariff in oData.Tariffs)
                            {
                                oStreetSection.TARIFF_IN_STREETS_SECTIONS_COMPILEDs.Add(new TARIFF_IN_STREETS_SECTIONS_COMPILED()
                                {
                                    TARSTRSEC_TAR_ID = Convert.ToDecimal(iTariff),
                                });

                            }

                            m_Log.LogMessage(LogLevels.logINFO, string.Format("UpdateStreetSections -> Update Street Section {0}:{1}:{2}",
                                                                        oInstallation.INS_DESCRIPTION, oData.Id, oData.Description));


                        }


                        foreach (StreetSectionData oData in oDeleteStreetSectionsData)
                        {
                            STREET_SECTION oStreetSection = oInstallation.STREET_SECTIONs.Where(r => r.STRSE_ID_EXT == oData.Id).First();
                            oStreetSection.STRSE_DELETED = 1;

                            m_Log.LogMessage(LogLevels.logINFO, string.Format("UpdateStreetSections -> Delete Street Section {0}:{1}:{2}",
                                        oInstallation.INS_DESCRIPTION, oData.Id, oData.Description));

                        }

                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
                            bRes = false;
                        }

                    }


                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
            }

            return bRes;


        }


        public bool GetPackageFileData(decimal dInstallationID, out Dictionary<decimal, STREET> oStreets, out List<STREET_SECTION> oStreetSections, 
                                       out List<STREET_SECTIONS_GRID> oGrid, out int iPackageNextVersion)

        {
            bool bRes = false;
            oStreets = new Dictionary<decimal, STREET>();
            oStreetSections = new List<STREET_SECTION>();
            oGrid = new List<STREET_SECTIONS_GRID>();
            iPackageNextVersion = 1;
            Dictionary<string,STREET> oStreetsNames = new Dictionary<string, STREET>();

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

                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {
                        foreach (STREET_SECTION oStreetSection in oInstallation.STREET_SECTIONs.Where(r => r.STRSE_DELETED == 0))
                        {
                            if ((!oStreets.ContainsKey(oStreetSection.STRSE_STR_ID)) &&
                                (!oStreetsNames.ContainsKey(oStreetSection.STREET.STR_DESCRIPTION)))
                            {
                                oStreets[oStreetSection.STRSE_STR_ID] = oStreetSection.STREET;
                                oStreetsNames[oStreetSection.STREET.STR_DESCRIPTION] = oStreetSection.STREET;
                            }
                        }

                        oStreetSections = oInstallation.STREET_SECTIONs.Where(r => r.STRSE_DELETED == 0).ToList();
                        oGrid = oInstallation.STREET_SECTIONS_GRIDs.ToList();
                        STREET_SECTIONS_PACKAGE_VERSION oVersion = oInstallation.STREET_SECTIONS_PACKAGE_VERSIONs
                                                                .OrderByDescending(r => r.STSEPV_ID).FirstOrDefault();

                        if (oVersion != null)
                        {
                            iPackageNextVersion = Convert.ToInt32(oVersion.STSEPV_ID) + 1;
                        }
                    }



                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
            }
            finally
            {
                oStreetsNames = null;
            }

            return bRes;


        }


        public bool GetStreetSectionsExternalIds(decimal dInstallationID, out string[] oStreetSections)
        {
            bool bRes = false;
            oStreetSections = null;

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

                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {

                        oStreetSections = oInstallation.STREET_SECTIONs.Where(r => r.STRSE_DELETED == 0).Select(r => r.STRSE_ID_EXT).ToArray();
                    }



                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "RecreateStreetSectionsGrid: ", e);
            }

            return bRes;


        }



        public bool UpdateStreetSectionsOccupation(decimal dInstallationID, ref List<OccupationData> oStreetSectionsData)
        {
            bool bRes = false;

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                                             new TransactionOptions()
                                                                             {
                                                                                 IsolationLevel = IsolationLevel.ReadUncommitted,
                                                                                 //Timeout = TimeSpan.FromSeconds(ctnTransactionTimeout)
                                                                             }))
                {
                    integraMobileDBEntitiesDataContext dbContext = new integraMobileDBEntitiesDataContext();


                    var oInstallation = (from r in dbContext.INSTALLATIONs
                                         where r.INS_ID == dInstallationID
                                         select r).FirstOrDefault();


                    if (oInstallation != null)
                    {
                        foreach (OccupationData oData in oStreetSectionsData)
                        {

                            STREET_SECTIONS_OCCUPANCY oOccup = dbContext.STREET_SECTIONS_OCCUPANCies.
                                                               Where(r => r.STREET_SECTION.STRSE_ID_EXT == oData.Id).First();

                            if ((oOccup.STRSEOC_OCC_NUM_PLACES!=oData.OcuppiedPlaces)||
                                (oOccup.STRSEOC_TOTAL_NUM_PLACES!=oData.TotalPlaces)||
                                (oOccup.STRSEOC_UTC_DATE!=oData.Date))    

                            {
                                oOccup.STRSEOC_OCC_NUM_PLACES = oData.OcuppiedPlaces;
                                oOccup.STRSEOC_TOTAL_NUM_PLACES = oData.TotalPlaces;
                                oOccup.STRSEOC_UTC_DATE = oData.Date;
                                m_Log.LogMessage(LogLevels.logINFO, string.Format("UpdateStreetSectionsOccupation -> Update Street Section {0}:{1}:{2} -> {3}/{4} {5}", 
                                                                        oInstallation.INS_DESCRIPTION, oData.Id, oData.Description, oData.OcuppiedPlaces, oData.TotalPlaces, oData.Date));

                            }
                           
                        }

                        try
                        {
                            SecureSubmitChanges(ref dbContext);
                            transaction.Complete();
                        }
                        catch (Exception e)
                        {
                            m_Log.LogMessage(LogLevels.logERROR, "UpdateStreetSectionsOccupation: ", e);
                            bRes = false;
                        }

                    }


                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "UpdateStreetSectionsOccupation: ", e);
            }

            return bRes;


        }



        public bool GetStreetSectionsOccupancyUpdateInstallations(out List<INSTALLATION> oInstallations)
        {
            bool bRes = false;
            oInstallations = new List<INSTALLATION>(); 

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

                    oInstallations = (from r in dbContext.INSTALLATIONs
                                      where r.INS_ENABLED == 1 && r.INS_STREET_SECTION_OCUP_WS_SIGNATURE_TYPE != (int)StreetSectionsUpdate.no_call
                                      select r).ToList();

                    bRes = true;
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupByExtOpsId: ", e);
            }

            return bRes;
        }

        public bool GetStreetSectionsOccupancy(decimal dInstallationId, out List<STREET_SECTIONS_OCCUPANCY> oLstStrSeOccupancy)
        {
            bool bRes = false;

            oLstStrSeOccupancy = new List<STREET_SECTIONS_OCCUPANCY>();

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


                    oLstStrSeOccupancy = (from r in dbContext.STREET_SECTIONS_OCCUPANCies
                                          where r.STREET_SECTION.STRSE_INS_ID == dInstallationId && r.STREET_SECTION.STRSE_DELETED == 0
                                          select r).ToList();


                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetStreetSectionsOccupancy: ", e);
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


        public bool GetFinanDistOperator(decimal dFinanDistOperatorId, ref FINAN_DIST_OPERATOR oFinanDistOperator)
        {
            bool bRes = false;
            oFinanDistOperator = null;            

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


                    var oOperators = (from r in dbContext.FINAN_DIST_OPERATORs
                                            where r.FDO_ID == dFinanDistOperatorId
                                            select r).ToArray();
                    if (oOperators.Count() == 1)
                    {
                        oFinanDistOperator = oOperators[0];
                        bRes = true;

                    }
                }
            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "GetFinanDistOperator: ", e);
            }

            return bRes;

        }

        public bool getGroupByExtId(int iExtIndex, string sExtGroupId,
                                    out GROUP oGroup)
        {
            bool bRes = false;
            oGroup = null;            

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
                    
                    if (iExtIndex == 1)
                    {
                        oGroup = (from r in dbContext.GROUPs
                                  where r.GRP_EXT1_ID == sExtGroupId
                                  select r).FirstOrDefault();
                    }
                    else if (iExtIndex == 2)
                    {
                        oGroup = (from r in dbContext.GROUPs
                                  where r.GRP_EXT2_ID == sExtGroupId
                                  select r).FirstOrDefault();
                    }
                    else if (iExtIndex == 3)
                    {
                        oGroup = (from r in dbContext.GROUPs
                                  where r.GRP_EXT3_ID == sExtGroupId
                                  select r).FirstOrDefault();
                    }

                    bRes = (oGroup != null);
                }

            }
            catch (Exception e)
            {
                m_Log.LogMessage(LogLevels.logERROR, "getGroupByExtId: ", e);
            }

            return bRes;
        }

    }
}
