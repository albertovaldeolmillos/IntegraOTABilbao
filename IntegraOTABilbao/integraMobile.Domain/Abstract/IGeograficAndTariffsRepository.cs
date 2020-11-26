using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace integraMobile.Domain.Abstract
{
    public struct stZone
    {
        public int level;
        public decimal dID;
        public string strDescription;
        public decimal dLiteralID;
        public string strShowId;
        public string strColour;
        public IEnumerable<stZone> subzones;
        public IEnumerable<stGPSPolygon> GPSpolygons;
        public GroupType GroupType;
        public float Occupancy;
        public int ParkingType;
    }

    public struct stTariff
    {
        public decimal dID;
        public string strDescription;
        public decimal dLiteralID;
        public bool bUserSelectable;
        public IEnumerable<decimal> zones;
    }

    public struct stGPSPolygon
    {
        public int iPolNumber;
        public IEnumerable<stGPSPoint> GPSpolygon;
    }
    
    public struct stGPSPoint    
    {
        public decimal order;
        public decimal dLatitude;
        public decimal dLongitude;
    }


    public class MapPoint
    {		

        private const decimal MAX_DIFF = 0.000000000001M;
        public decimal x { get; set; }
        public decimal y { get; set; }

        

        public bool IsEqual(MapPoint oCmpPoint)
        {
            bool bRes = false;

            bRes = Math.Abs(x - oCmpPoint.x) < MAX_DIFF;

            if (bRes)
                bRes = Math.Abs(y - oCmpPoint.y) < MAX_DIFF;


            return bRes;           
        }

    };

    public class StreetSectionData
    {


        public string Id { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        public string StreetFrom { get; set; }
        public string StreetTo { get; set; }
        public string idZone { get; set; }
        public int idGroup { get; set; }
        public string SectorType { get; set; }
        public List<int> Tariffs { get; set; }
        public bool Enabled { get; set; }
        public int Places { get; set; }
        public string StringGeometry { get; set; }
        public List<MapPoint> GeometryED50 { get; set; }
        public List<MapPoint> Geometry { get; set; }
        public string Colour { get; set; }
        public Dictionary<int, GridElement> oGridElements { get; set; }


        public bool isEqual(StreetSectionData oCmpSSD)
        {
            bool bRes = false;

            bRes = (Geometry.Count() == oCmpSSD.Geometry.Count() &&
                    oGridElements.Count == oCmpSSD.oGridElements.Count) &&
                    (Tariffs.Count() == Tariffs.Count());

            if (bRes)
            {
                bRes = ((Id == oCmpSSD.Id) &&
                    (Description == oCmpSSD.Description) &&
                    (Street == oCmpSSD.Street) &&
                    (StreetFrom == oCmpSSD.StreetFrom) &&
                    (StreetTo == oCmpSSD.StreetTo) &&
                    (idGroup == oCmpSSD.idGroup) &&                    
                    (Enabled == oCmpSSD.Enabled) &&
                    (Colour == oCmpSSD.Colour));


                int i = 0;

                while (i < Geometry.Count() && bRes)
                {
                    bRes = Geometry[i].IsEqual(oCmpSSD.Geometry[i]);
                    i++;
                }


                if (bRes)
                {
                    i=0;
                    while (i < Tariffs.Count() && bRes)
                    {
                        bRes = (Tariffs[i]==oCmpSSD.Tariffs[i]);
                        i++;
                    }

                }

                if (bRes)
                {
                    foreach (KeyValuePair<int, GridElement> entry in oGridElements.OrderBy(r => r.Key))
                    {

                        bRes = oCmpSSD.oGridElements[entry.Key] != null;
                        if (!bRes)
                        {
                            break;
                        }
                        else
                        {
                            bRes = (oCmpSSD.oGridElements[entry.Key].id == entry.Value.id);
                            if (!bRes)
                                break;
                        }

                    }
                }

            }

            return bRes;

        }

    };

    public class GridElement
    {
        public int id { get; set; }
        public string description { get; set; }
        public List<MapPoint> Polygon { get; set; }
        public int ReferenceCount { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int maxX { get; set; }
        public int maxY { get; set; }
        public List<StreetSectionData> LstStreetSections { get; set; }

        public bool IsEqual(GridElement oCmpElem)
        {
            bool bRes = false;

            bRes = Polygon.Count() == oCmpElem.Polygon.Count();

            if (bRes)
            {
                bRes = ((id == oCmpElem.id) &&
                    (description == oCmpElem.description) &&
                    (x == oCmpElem.x) &&
                    (y == oCmpElem.y) &&
                    (maxX == oCmpElem.maxX) &&
                    (maxY == oCmpElem.maxY));

                if (bRes)
                {
                    int i = 0;

                    while (i < Polygon.Count() && bRes)
                    {
                        bRes = Polygon[i].IsEqual(oCmpElem.Polygon[i]);
                        i++;
                    }

                }

            }

            return bRes;
        }



    }


    public class OccupationData
    {
        public string Id { get; set; }
        public string Description { get; set; }        
        public int TotalPlaces { get; set; }
        public int OcuppiedPlaces { get; set; }
        public DateTime Date;
    }

    public enum FineWSSignatureType
    {
        fst_test = 0,
        fst_internal = 1,
        fst_standard = 2,
        fst_eysa =3,
        fst_gtechna = 4,
        fst_madidplatform = 5,
    }


    public enum ConfirmFineWSSignatureType
    {
        cfst_test = 0,
        cfst_internal = 1,
        cfst_standard = 2,
        cfst_eysa = 3,
        cfst_gtechna = 4,
        cfst_madidplatform = 5,
        cfst_picbilbao = 6,
    }

    public enum ParkWSSignatureType
    {
        pst_test = 0,
        pst_internal = 1,
        pst_standard_time_steps = 2,
        pst_eysa = 3,
        pst_standard_amount_steps = 4,

    }

    public enum UnParkWSSignatureType
    {
        upst_test = 0,
        upst_internal = 1,
        upst_standard = 2,
        upst_eysa = 3,

    }


    public enum ConfirmParkWSSignatureType
    {
        cpst_test = 0,
        cpst_internal = 1,
        cpst_standard = 2,
        cpst_eysa = 3,
        cpst_gtechna = 4,
        cpst_nocall = 5,
        cpst_madridplatform = 6
    }


    public enum UserReplicationWSSignatureType
    {
        urst_Zendesk = 1,
    }

    public enum ConfirmEntryOffstreetWSSignatureType
    {
        test = 0,
        meypar = 1,
        no_call = 2
    }

    public enum QueryExitOffstreetWSSignatureType
    {
        test = 0,
        meypar = 1,
        no_call = 2
    }

    public enum ConfirmExitOffstreetWSSignatureType
    {
        test = 0,
        meypar = 1,
        no_call = 2
    }

    public enum StreetSectionsUpdate
    {
        no_call = 0,
        NGSBilbao = 1,
    }

    public enum StreetSectionsOccupancyUpdate
    {
        no_call = 0,
        ParkXPlorerBilbao = 1,
    }


    public enum RechargeValuesTypes
    {
        rvt_ManualRecharge = 1,
        rvt_AutomaticRecharge = 2,
        rvt_AutomaticRechargeBelow = 3,
        rvt_SignUp = 4,
        rvt_RechargeChangePay = 5,
        rvt_RechargePagatelia = 6,
        rvt_RechargePaypal= 7,
        rvt_BalanceTransfer = 8,
        rvt_OxxoRecharge = 9


    }

    public interface IGeograficAndTariffsRepository
    {
        bool getInstallation(decimal? dInstallationId, decimal? dLatitude, decimal? dLongitude, 
            ref INSTALLATION oInstallation, ref DateTime ?dtInsDateTime);
        IEnumerable<INSTALLATION> getInstallationsList();
        bool getGroup(decimal? dGroupId, ref GROUP oGroup, ref DateTime? dtgroupDateTime);
        DateTime? getInstallationDateTime(decimal dInstallationId);
        DateTime? ConvertInstallationDateTimeToUTC(decimal dInstallationId, DateTime dtInstallation);
        DateTime? ConvertUTCToInstallationDateTime(decimal dInstallationId, DateTime dtUTC);
        int? GetInstallationUTCOffSetInMinutes(decimal dInstallationId);
        DateTime? getGroupDateTime(decimal dGroupID);
        IEnumerable<stZone> getInstallationGroupHierarchy(decimal dInstallationId, GroupType groupType = GroupType.OnStreet);
        IEnumerable<stTariff> getInstallationTariffs(decimal dInstallationId);
        IEnumerable<stTariff> getGroupTariffs(decimal dGroupId);
        IEnumerable<stTariff> getGroupTariffs(decimal dGroupId, decimal? dLatitude, decimal? dLongitude);
        IEnumerable<stTariff> getPlateTariffsInGroup(string strPlate, decimal dGroupId, decimal? dLatitude, decimal? dLongitude);

        bool GetGroupAndTariffExternalTranslation(int iWSNumber, GROUP oGroup, TARIFF oTariff, ref string strExtGroupId, ref string strExtTarId);
        bool GetGroupAndTariffExternalTranslation(int iWSNumber, decimal dGroupId, decimal dTariffId, ref string strExtGroupId, ref string strExtTarId);
        bool GetGroupAndTariffFromExternalId(int iWSNumber, INSTALLATION oInstallation, string strExtGroupId, string strExtTarId, ref decimal? dGroupId, ref decimal? dTariffId);
        bool GetGroupAndTariffStepOffsetMinutes(GROUP oGroup, TARIFF oTariff, out int? iOffset);

        bool getExternalProvider(string strName, ref EXTERNAL_PROVIDER oExternalProvider);

        bool getOffStreetConfiguration(decimal? dGroupId, decimal? dLatitude, decimal? dLongitude, ref GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetConfiguration, ref DateTime? dtgroupDateTime);
        bool getOffStreetConfigurationByExtOpsId(string sExtParkingId, ref GROUPS_OFFSTREET_WS_CONFIGURATION oOffstreetConfiguration, ref DateTime? dtgroupDateTime);
        bool getGroupByExtOpsId(string sExtGroupId, ref GROUP oGroup, ref DateTime? dtgroupDateTime);

        bool GetFinanDistOperator(decimal dFinanDistOperatorId, ref FINAN_DIST_OPERATOR oFinanDistOperator);
        bool GetStreetSectionsUpdateInstallations(out List<INSTALLATION> oInstallations);
        bool GetStreetSectionsOccupancyUpdateInstallations(out List<INSTALLATION> oInstallations);

        bool GetInstallationsStreetSections(decimal dInstallationID, out List<StreetSectionData> oStreetSectionsData, out Dictionary<int, GridElement> oGrid);
        
        bool RecreateStreetSectionsGrid(decimal dInstallationID, ref Dictionary<int, GridElement> oGrid);
        bool UpdateStreetSections(decimal dInstallationID, bool bGridRecreated,
                                  ref List<StreetSectionData> oInsertStreetSectionsData, 
                                  ref List<StreetSectionData> oUpdateStreetSectionsData,
                                  ref List<StreetSectionData> oDeleteStreetSectionsData);

        bool GetPackageFileData(decimal dInstallationID, out Dictionary<decimal, STREET> oStreets, out List<STREET_SECTION> oStreetSections, 
                                out List<STREET_SECTIONS_GRID> oGrid, out int iPackageNextVersion);

        bool GetStreetSectionsExternalIds(decimal dInstallationID, out string [] oStreetSections);
        bool UpdateStreetSectionsOccupation(decimal dInstallationID, ref List<OccupationData> oStreetSectionsData);
        bool GetStreetSectionsOccupancy(decimal dInstallationId, out List<STREET_SECTIONS_OCCUPANCY> oLstStrSeOccupancy);

        bool getGroupByExtId(int iExtIndex, string sExtGroupId, out GROUP oGroup);

    }
}
