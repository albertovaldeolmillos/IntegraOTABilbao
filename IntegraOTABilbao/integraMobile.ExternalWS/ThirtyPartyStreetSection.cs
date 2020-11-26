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
using System.IO;
using System.Globalization;
using System.Diagnostics;
using integraMobile.Domain;
using integraMobile.Domain.Abstract;
using integraMobile.Infrastructure;
using integraMobile.Infrastructure.Logging.Tools;
using Ninject;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace integraMobile.ExternalWS
{


    public class ThirdPartyStreetSection : ThirdPartyBase
    {

       
        private const double EARTH_RADIUS_KM = 6378.1370;
        private const decimal GRID_SIZE_KM = 0.2M;

        const string wkt4326 = "GEOGCS[\"WGS 84\","+
                                "DATUM[\"WGS_1984\","+
                                    "SPHEROID[\"WGS 84\",6378137,298.257223563,"+
                                        "AUTHORITY[\"EPSG\",\"7030\"]],"+
                                    "AUTHORITY[\"EPSG\",\"6326\"]],"+
                                "PRIMEM[\"Greenwich\",0,"+
                                    "AUTHORITY[\"EPSG\",\"8901\"]],"+
                                "UNIT[\"degree\",0.0174532925199433,"+
                                    "AUTHORITY[\"EPSG\",\"9122\"]],"+
                                "AUTHORITY[\"EPSG\",\"4326\"]]";

        const string wkt23030 = "PROJCS[\"ED50 / UTM zone 30N\"," +
                                "GEOGCS[\"ED50\"," +
                                    "DATUM[\"European_Datum_1950\"," +
                                        "SPHEROID[\"International 1924\",6378388,297," +
                                            "AUTHORITY[\"EPSG\",\"7022\"]]," +
                                        "TOWGS84[-87,-98,-121,0,0,0,0]," +
                                        "AUTHORITY[\"EPSG\",\"6230\"]]," +
                                    "PRIMEM[\"Greenwich\",0," +
                                        "AUTHORITY[\"EPSG\",\"8901\"]]," +
                                    "UNIT[\"degree\",0.0174532925199433," +
                                        "AUTHORITY[\"EPSG\",\"9122\"]]," +
                                    "AUTHORITY[\"EPSG\",\"4230\"]]," +
                                "PROJECTION[\"Transverse_Mercator\"]," +
                                "PARAMETER[\"latitude_of_origin\",0]," +
                                "PARAMETER[\"central_meridian\",-3]," +
                                "PARAMETER[\"scale_factor\",0.9996]," +
                                "PARAMETER[\"false_easting\",500000]," +
                                "PARAMETER[\"false_northing\",0]," +
                                "UNIT[\"metre\",1," +
                                    "AUTHORITY[\"EPSG\",\"9001\"]]," +
                                "AXIS[\"Easting\",EAST]," +
                                "AXIS[\"Northing\",NORTH]," +
                                "AUTHORITY[\"EPSG\",\"23030\"]]";

        public class LoginRequest
        {
            /// <summary>
            /// Usuario autorizado de parkXplorer
            /// </summary>
            public string user { get; set; }
            /// <summary>
            /// Contraseña
            /// </summary>
            public string pass { get; set; }
        }

        public class OcupacionRequest
        {
            /// <summary>
            /// Código de la contrata según parkXplorer (en Bilbao, EYSABILB)
            /// </summary>
            public string contrata { get; set; }
            /// <summary>
            /// Conjunto de identificadores del área (en Bilbao, se utiliza el idperm del tramo)
            /// </summary>
            public string[] tramos { get; set; }
        }


      

        public class ErrorRespuesta 
        {
            public string mensaje; //Mensaje descriptivo del error
            public string stackTrace; //Pila
            public string tipo; //Tipo de excepción
            public string codigo; //Código de error
            public DateTime fecha;
        }

        public class RLogin 
        {
            public string sesion { get; set; } //Token de sesión a indicar en la cabecera (en los métodos que lo requieran)
            public string alias { get; set; }
            public string nombre { get; set; }
            public string apellidos { get; set; }
            public long Id { get; set; }
            public long id_organizacion { get; set; }
        }


        public class RSetOcupacion 
        {
            public DateTime fechaConsulta { get; set; }
            public RAreaOcupacion[] tramos { get; set; }
        }

        public class RAreaOcupacion 
        {
            public long Id { get; set; }
            public string identificador { get; set; } //Identificador del área (idperm en Bilbao)
            public string nombre { get; set; }
            public int plazas { get; set; } //Plazas totales ofertadas
            public int ocupadas_estacionamiento { get; set; }
            public int ocupadas_obra { get; set; }
            public DateTime ultimoCalculo { get; set; }
        }

        public class BaseResponse 
        {
            public bool ok;
            public string info; //Campo de información complementaria para algunas firmas.
            public ErrorRespuesta error; //Si ok=true, estará seteado a null
            public long ms;
            private DateTime inicio;         
        }


        public class LoginResponse: BaseResponse
        {
            public RLogin contenido;
        }


        public class OcupacionResponse : BaseResponse
        {
            public RSetOcupacion contenido;
        }


        public ThirdPartyStreetSection()
            : base()
        {
            m_Log = new CLogWrapper(typeof(ThirdPartyStreetSection));
        }

        public bool NGSBilbaoStreetSectionsUpdate(INSTALLATION oInstallation, DateTime dtInstDateTime, out List<StreetSectionData> oStreetSectionsData, out Dictionary<int, GridElement> oGrid)
        {

            bool bRes = false;
            long lEllapsedTime = -1;
            Stopwatch watch = null;
            oStreetSectionsData = new List<StreetSectionData>();
            oGrid = new Dictionary<int, GridElement>();

            try
            {
             
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                  ((sender, certificate, chain, sslPolicyErrors) => true);

                AddTLS12Support();

                string strURL = oInstallation.INS_STREET_SECTION_UPDATE_WS_URL;
                WebRequest request = WebRequest.Create(strURL);

                DateTime dtUtc = DateTime.UtcNow.AddHours(-2);
                DateTime dtUTC1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan tIat = (dtUtc - dtUTC1970);
                TimeSpan tExp = (dtUtc.AddHours(8) - dtUTC1970);
                int iIat = (int)tIat.TotalSeconds;
                int iExp = (int)tExp.TotalSeconds;

                Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate iat={0:yyyyMMdd HHmmss} exp={1:yyyyMMdd HHmmss}", dtUtc, dtUtc.AddHours(8)), LogLevels.logINFO);
                Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate iat={0} exp={1}", iIat, iExp), LogLevels.logINFO);


                var oPayload = new Dictionary<string, object>()
                {
                    { "iss", "CentroControlEYSA" },
                    { "sub", "IntegracionGIS" },
                    { "aud", "SIGMICT" },
                    { "exp", iExp },
                    { "iat", iIat}
                };

                Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate Payload={0}", PrettyJSON(JsonConvert.SerializeObject( oPayload ))), LogLevels.logINFO);

                string sSecretKey = oInstallation.INS_STREET_SECTION_UPDATE_WS_AUTH_HASH_KEY;
                var bySecretKey = Encoding.ASCII.GetBytes(sSecretKey);
                string sJwtToken = Jose.JWT.Encode(oPayload, bySecretKey, Jose.JwsAlgorithm.HS256);

                request.Method = "GET";
                request.ContentType = "application/json";
                request.Timeout = Get3rdPartyWSTimeout();
                request.Headers.Add("Authorization", string.Format("Bearer {0}", sJwtToken));

                Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate request.url={0} Authorization={1}", strURL, string.Format("Bearer {0}", sJwtToken)), LogLevels.logINFO);

                watch = Stopwatch.StartNew();


                try
                {

                    WebResponse response = request.GetResponse();
                    // Display the status.
                    HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                    if (oWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        // Get the stream containing content returned by the server.
                        Stream dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.

                        Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                        // Clean up the streams.


                        reader.Close();
                        dataStream.Close();

                        dynamic oResponse = JsonConvert.DeserializeObject(responseFromServer);
                        bool bSuccess = false;

                        try
                        {
                            bSuccess = Convert.ToBoolean(oResponse["success"].ToString());
                        }
                        catch
                        {

                        }


                        if (!bSuccess)
                        {
                            bRes = bSuccess;
                        }
                        else
                        {
                           

                            decimal xmin = decimal.MaxValue;
                            decimal xmax = decimal.MinValue;
                            decimal ymin = decimal.MaxValue;
                            decimal ymax = decimal.MinValue;
                            var oArrayData = oResponse["data"];
                            NumberFormatInfo numberFormatProvider = new NumberFormatInfo();
                            numberFormatProvider.NumberDecimalSeparator = ".";
                            int i = 0;

                            foreach (var oData in oArrayData)
                            {
                                
                                DateTime dDateFrom =  DateTime.MinValue;
                                if (oData["fecha"]["inicio"]!=null)
                                    dDateFrom = Conversions.UnixTimeStampToDateTime(Convert.ToInt64(oData["fecha"]["inicio"].ToString()));

                                DateTime dDateTo=DateTime.MaxValue;
                                if (oData["fecha"]["fin"]!=null)
                                    dDateTo = Conversions.UnixTimeStampToDateTime(Convert.ToInt64(oData["fecha"]["fin"].ToString()));

                                if ((dtInstDateTime >= dDateFrom) && (dtInstDateTime < dDateTo))
                                {
                                    StreetSectionData oSSData = new StreetSectionData();

                                    oSSData.Id = oData["idperm"].ToString();
                                    oSSData.Description = oData["descripcion"].ToString();
                                    if (oData["zona"]["idperm"] != null)
                                        oSSData.idZone = oData["zona"]["idperm"].ToString();
                                    oSSData.SectorType = oData["tipoSector"].ToString();
                                    if (oData["calle"] != null)
                                    {
                                        oSSData.Street = oData["calle"]["id"].ToString();
                                        if (oData["calle"]["codigoDesde"] != null)
                                            oSSData.StreetFrom = oData["calle"]["codigoDesde"].ToString();
                                        else
                                            oSSData.StreetFrom = oSSData.Street;
                                        if (oData["calle"]["codigoHasta"] != null) 
                                            oSSData.StreetTo = oData["calle"]["codigoHasta"].ToString();
                                        else
                                            oSSData.StreetTo = oSSData.Street;
                                    }
                                    oSSData.Enabled = Convert.ToBoolean(oData["activo"].ToString());
                                    oSSData.Places = Convert.ToInt32(oData["plazas"]["ofrecidas"].ToString());
                                    oSSData.StringGeometry = oData["geometry"]["WKT"].ToString();


                                    oSSData.StringGeometry = oSSData.StringGeometry.Replace("MULTIPOLYGON", "").Replace("(", "").Replace(")", "");
                                    oSSData.GeometryED50 = new List<MapPoint>();
                                    oSSData.oGridElements = new Dictionary<int, GridElement>();


                                    string[] strPairs = oSSData.StringGeometry.Split(new char[] { ',' });
                                    i = 0;

                                    foreach (string strPair in strPairs)
                                    {
                                        string[] strComponents = strPair.Split(new char[] { ' ' });

                                        if (i < strPairs.Count() - 1)
                                        {
                                            oSSData.GeometryED50.Add(new MapPoint()
                                            {
                                                x = Convert.ToDecimal(strComponents[0], numberFormatProvider),
                                                y = Convert.ToDecimal(strComponents[1], numberFormatProvider),
                                            });
                                        }
                                        else
                                            break;

                                        i++;
                                    }

                                    oSSData.Geometry = new List<MapPoint>();
                                    foreach (MapPoint oED50Point in oSSData.GeometryED50)
                                    {
                                        MapPoint oGPSPoint;
                                        ED50Z30ToGPS(oED50Point, out oGPSPoint);
                                        oSSData.Geometry.Add(oGPSPoint);

                                        if (oSSData.Enabled)
                                        {
                                            if (oGPSPoint.x < xmin)
                                            {
                                                xmin = oGPSPoint.x;
                                            }

                                            if (oGPSPoint.x > xmax)
                                            {
                                                xmax = oGPSPoint.x;
                                            }

                                            if (oGPSPoint.y < ymin)
                                            {
                                                ymin = oGPSPoint.y;
                                            }

                                            if (oGPSPoint.y > ymax)
                                            {
                                                ymax = oGPSPoint.y;
                                            }
                                        }
                                    }


                                    int iGroup = 300001;

                                    var oGroup = oInstallation.GROUPs.Where(g => g.GRP_QUERY_EXT_ID == oSSData.idZone).FirstOrDefault();
                                    if (oGroup != null)
                                        iGroup = Convert.ToInt32(oGroup.GRP_ID);
                                    
                                    /*switch (oSSData.idZone)
                                    {
                                        case "50001":
                                            iGroup = 300001;
                                            break;
                                        case "50002":
                                            iGroup = 300002;
                                            break;
                                        case "50003":
                                            iGroup = 300003;
                                            break;
                                        case "50004":
                                            iGroup = 300004;
                                            break;
                                        case "50005":
                                            iGroup = 300005;
                                            break;
                                        case "50006":
                                            iGroup = 300006;
                                            break;
                                        case "50007":
                                            iGroup = 300007;
                                            break;
                                        case "50008":
                                            iGroup = 300008;
                                            break;
                                        case "50009":
                                            iGroup = 300009;
                                            break;
                                        case "50010":
                                            iGroup = 300010;
                                            break;
                                        case "50011":
                                            iGroup = 300013;
                                            break;
                                        case "50012":
                                            iGroup = 300011;
                                            break;
                                        case "50013":
                                            iGroup = 300012;
                                            break;


                                    }*/
                                    oSSData.idGroup = iGroup;
                                    oSSData.Colour = ConfigurationManager.AppSettings["NGSBilbaoSSColourBlue"].ToString();
                                    int iTariff = 300001;

                                    if (!string.IsNullOrEmpty(oSSData.SectorType))
                                    {
                                        if (oSSData.SectorType.ToUpper().Contains("VERDES L"))
                                        {
                                            oSSData.Colour = ConfigurationManager.AppSettings["NGSBilbaoSSColourGreenL"].ToString();
                                            iTariff = 300010;
                                        }
                                        else if (oSSData.SectorType.ToUpper().Contains("VERDE"))
                                        {
                                            oSSData.Colour = ConfigurationManager.AppSettings["NGSBilbaoSSColourGreen"].ToString();
                                            iTariff = 300002;
                                        }

                                    }

                                    oSSData.Tariffs = new List<int>();
                                    oSSData.Tariffs.Add(iTariff);

                                    oStreetSectionsData.Add(oSSData);
                                }

                            }


                            oStreetSectionsData = oStreetSectionsData.OrderBy(r => r.Id).ToList();

                            MapPoint[] oLstContainerPolygon = new MapPoint[4];

                            oLstContainerPolygon[0] = new MapPoint() { x = xmin, y = ymax };
                            oLstContainerPolygon[1] = new MapPoint() { x = xmax, y = ymax };
                            oLstContainerPolygon[2] = new MapPoint() { x = xmax, y = ymin };
                            oLstContainerPolygon[3] = new MapPoint() { x = xmin, y = ymin };

                            List<MapPoint> oXGridPoints = new List<MapPoint>();
                            List<MapPoint> oYGridPoints = new List<MapPoint>();

                            decimal dXDistance = GetDistanceKM(oLstContainerPolygon[0], oLstContainerPolygon[1]);
                            decimal dYDistance = GetDistanceKM(oLstContainerPolygon[1], oLstContainerPolygon[2]);
                            decimal dGridSize = GRID_SIZE_KM;
                            decimal dXDistanceProp = dGridSize / dXDistance;
                            decimal dYDistanceProp = dGridSize / dYDistance;

                            oLstContainerPolygon[0].x -= (oLstContainerPolygon[1].x - oLstContainerPolygon[0].x) * dXDistanceProp;
                            oLstContainerPolygon[0].y += (oLstContainerPolygon[0].y - oLstContainerPolygon[3].y) * dYDistanceProp;
                            oLstContainerPolygon[1].x += (oLstContainerPolygon[1].x - oLstContainerPolygon[0].x) * dXDistanceProp;
                            oLstContainerPolygon[1].y += (oLstContainerPolygon[0].y - oLstContainerPolygon[3].y) * dYDistanceProp;
                            oLstContainerPolygon[2].x += (oLstContainerPolygon[1].x - oLstContainerPolygon[0].x) * dXDistanceProp;
                            oLstContainerPolygon[2].y -= (oLstContainerPolygon[0].y - oLstContainerPolygon[3].y) * dYDistanceProp;
                            oLstContainerPolygon[3].x -= (oLstContainerPolygon[1].x - oLstContainerPolygon[0].x) * dXDistanceProp;
                            oLstContainerPolygon[3].y -= (oLstContainerPolygon[0].y - oLstContainerPolygon[3].y) * dYDistanceProp;

                            dXDistance = GetDistanceKM(oLstContainerPolygon[0], oLstContainerPolygon[1]);
                            dYDistance = GetDistanceKM(oLstContainerPolygon[1], oLstContainerPolygon[2]);
                            dXDistanceProp = dGridSize / dXDistance;
                            dYDistanceProp = dGridSize / dYDistance;



                            oXGridPoints.Add(oLstContainerPolygon[0]);

                            i = 1;
                            decimal dTempDistance = 0;
                            while (dTempDistance < dXDistance)
                            {
                                decimal dNextX = oLstContainerPolygon[0].x + (oLstContainerPolygon[1].x - oLstContainerPolygon[0].x) * i * dXDistanceProp;
                                oXGridPoints.Add(new MapPoint() { x = dNextX, y = oLstContainerPolygon[0].y });
                                dTempDistance = GetDistanceKM(oXGridPoints[0], oXGridPoints[i]);
                                i++;
                            }


                            oYGridPoints.Add(oLstContainerPolygon[0]);
                            i = 1;
                            dTempDistance = 0;
                            while (dTempDistance < dYDistance)
                            {
                                decimal dNextY = oLstContainerPolygon[0].y - (oLstContainerPolygon[0].y - oLstContainerPolygon[3].y) * i * dYDistanceProp;
                                oYGridPoints.Add(new MapPoint() { x = oLstContainerPolygon[0].x, y = dNextY });
                                dTempDistance = GetDistanceKM(oXGridPoints[0], oXGridPoints[i]);
                                i++;
                            }

                            
                            int id = 1;

                            i = 0;
                            int j;
                            while (i < oXGridPoints.Count() - 1)
                            {
                                j = 0;
                                while (j < oYGridPoints.Count() - 1)
                                {

                                    List<MapPoint> oPolygon = new List<MapPoint>();
                                    oPolygon.Add(new MapPoint() { x = oXGridPoints[i].x, y = oYGridPoints[j].y });
                                    oPolygon.Add(new MapPoint() { x = oXGridPoints[i + 1].x, y = oYGridPoints[j].y });
                                    oPolygon.Add(new MapPoint() { x = oXGridPoints[i + 1].x, y = oYGridPoints[j + 1].y });
                                    oPolygon.Add(new MapPoint() { x = oXGridPoints[i].x, y = oYGridPoints[j + 1].y });
                                    oGrid.Add(id, new GridElement()
                                    {
                                        id = id,
                                        description = string.
                                            Format("Grid({0},{1})", i, j),
                                        Polygon = oPolygon,
                                        ReferenceCount = 0,
                                        LstStreetSections = new List<StreetSectionData>(),
                                        x = i,
                                        y = j,
                                        maxX = oXGridPoints.Count() - 2,
                                        maxY = oYGridPoints.Count() - 2
                                    });
                                    id++;
                                    j++;
                                }
                                i++;
                            }


                            foreach (StreetSectionData oData in oStreetSectionsData.Where(r=>r.Enabled))
                            {
                                
                                foreach (MapPoint oPoint in oData.Geometry)
                                {
                                    bool bInside = false;
                                    int iId = -1;

                                    foreach (KeyValuePair<int, GridElement> entry in oGrid)
                                    {
                                        bInside = IsPointInsidePolygon(oPoint, entry.Value.Polygon);
                                        if (bInside)
                                        {
                                            iId = entry.Key;
                                            break;
                                        }
                                    }

                                    if (!bInside)
                                    {
                                        Console.Write("Error");
                                    }
                                    else
                                    {
                                        if (!oData.oGridElements.ContainsKey(iId))
                                        {
                                            oGrid[iId].ReferenceCount++;
                                            oGrid[iId].LstStreetSections.Add(oData);
                                            oData.oGridElements[iId] = oGrid[iId];
                                        }
                                    }


                                }
                            }
                            bRes = true;
                        }
                    }

                    response.Close();

                }
                catch (WebException e)
                {
                    if (e.Response != null)
                        Logger_AddLogMessage(string.Format("NGSStreetSectionsUpdate Web Exception HTTP Status={0}", ((HttpWebResponse)e.Response).StatusCode), LogLevels.logERROR);
                    Logger_AddLogException(e, "NGSStreetSectionsUpdate::Exception", LogLevels.logERROR);
                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, "NGSStreetSectionsUpdate::Exception", LogLevels.logERROR);
                }


                lEllapsedTime = watch.ElapsedMilliseconds;

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                Logger_AddLogException(e, "NGSStreetSectionsUpdate::Exception", LogLevels.logERROR);

            }

            return bRes;

        }




        public bool ParkXPlorerBilbaoStreetSectionsOccup(INSTALLATION oInstallation, string[] oStreetSections, ref string SessionID, out List<OccupationData> oOcupationData)
        {

            bool bRes = false;
            long lEllapsedTime = -1;
            Stopwatch watch = null;
            oOcupationData = new List<OccupationData>();
            

            try
            {
                bool bExistSession=false;
                if (!string.IsNullOrEmpty(SessionID))
                {
                    bExistSession = ParkXPlorerBilbaoStreetSectionsOccupCheckSession(oInstallation, SessionID);
                }
                                
                if (!bExistSession)
                {
                    bExistSession = ParkXPlorerBilbaoStreetSectionsOccupLogin(oInstallation, out SessionID);
                }

                if (!bExistSession)
                {
                    SessionID = "";
                }
                else
                {
                    string strURL = oInstallation.INS_STREET_SECTION_OCUP_WS_URL+"/ocupacion";
                    WebRequest request = WebRequest.Create(strURL);

                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Timeout = Get3rdPartyWSTimeout();
                    request.Headers.Add("PX-OCUP-SESSION" ,SessionID);

                    OcupacionRequest oRequest = new OcupacionRequest()
                    {
                        tramos = oStreetSections,
                        contrata = ConfigurationManager.AppSettings["ParkXplorerBilbaoCompanyName"].ToString(),
                    };

                    var json = JsonConvert.SerializeObject(oRequest);


                    Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccup request.url={0}, request.json={1}", strURL, PrettyJSON(json)), LogLevels.logINFO);

                    byte[] byteArray = Encoding.UTF8.GetBytes(json);

                    request.ContentLength = byteArray.Length;
                    // Get the request stream.
                    Stream dataStream = request.GetRequestStream();
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.
                    dataStream.Close();

                    watch = Stopwatch.StartNew();


                    try
                    {

                        WebResponse response = request.GetResponse();
                        // Display the status.
                        HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                        if (oWebResponse.StatusCode == HttpStatusCode.OK)
                        {
                            // Get the stream containing content returned by the server.
                            dataStream = response.GetResponseStream();
                            // Open the stream using a StreamReader for easy access.
                            StreamReader reader = new StreamReader(dataStream);
                            // Read the content.
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.

                            Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccup response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                            // Clean up the streams.


                            reader.Close();
                            dataStream.Close();

                            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

                            OcupacionResponse oResp = JsonConvert.DeserializeObject<OcupacionResponse>(responseFromServer, settings);

                            if (oResp.ok)
                            {

                                foreach (RAreaOcupacion oStreetSectionData in oResp.contenido.tramos)
                                {
                                    OccupationData oData = new OccupationData()
                                    {
                                        Id = oStreetSectionData.identificador,
                                        Description = oStreetSectionData.nombre,
                                        TotalPlaces = oStreetSectionData.plazas - oStreetSectionData.ocupadas_obra,
                                        OcuppiedPlaces = oStreetSectionData.ocupadas_estacionamiento,
                                        Date = oStreetSectionData.ultimoCalculo,
                                    };

                                    oOcupationData.Add(oData);
                                    
                                }

                                bRes = true;
                            }                
                                                        
                            
                        }

                        response.Close();

                    }
                    catch (WebException e)
                    {
                        Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccup Web Exception HTTP Status={0}", ((HttpWebResponse)e.Response).StatusCode), LogLevels.logERROR);
                        Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccup::Exception", LogLevels.logERROR);
                    }
                    catch (Exception e)
                    {
                        Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccup::Exception", LogLevels.logERROR);
                    }

                }
                lEllapsedTime = watch.ElapsedMilliseconds;

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccup::Exception", LogLevels.logERROR);

            }

            return bRes;

        }


        public bool ParkXPlorerBilbaoStreetSectionsOccupLogin(INSTALLATION oInstallation, out string SessionID)
        {

            bool bRes = false;
            long lEllapsedTime = -1;
            Stopwatch watch = null;
            SessionID = "";


            try
            {
                string strURL = oInstallation.INS_STREET_SECTION_OCUP_WS_URL+"/login";
                WebRequest request = WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = Get3rdPartyWSTimeout();


                LoginRequest login = new LoginRequest()
                {
                    user = oInstallation.INS_STREET_SECTION_OCUP_WS_HTTP_USER,
                    pass = oInstallation.INS_STREET_SECTION_OCUP_WS_HTTP_PASSWORD
                };


                //var json = new JavaScriptSerializer().Serialize(oUsersDataDict);
                var json = JsonConvert.SerializeObject(login);


                Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupLogin request.url={0}, request.json={1}", strURL, PrettyJSON(json)), LogLevels.logINFO);

                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                watch = Stopwatch.StartNew();


                try
                {

                    WebResponse response = request.GetResponse();
                    // Display the status.
                    HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                    if (oWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        // Get the stream containing content returned by the server.
                        dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.

                        Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupLogin response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                        // Clean up the streams.


                        reader.Close();
                        dataStream.Close();
                        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

                        LoginResponse oResp = JsonConvert.DeserializeObject<LoginResponse>(responseFromServer, settings);

                        if (oResp.ok)
                        {
                            SessionID = oResp.contenido.sesion;
                            bRes = true;
                        }                                              
                    }

                    response.Close();

                }
                catch (WebException e)
                {
                    Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupLogin Web Exception HTTP Status={0}", ((HttpWebResponse)e.Response).StatusCode), LogLevels.logERROR);
                    Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupLogin::Exception", LogLevels.logERROR);
                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupLogin::Exception", LogLevels.logERROR);
                }


                lEllapsedTime = watch.ElapsedMilliseconds;

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupLogin::Exception", LogLevels.logERROR);

            }

            return bRes;

        }


        protected bool ParkXPlorerBilbaoStreetSectionsOccupCheckSession(INSTALLATION oInstallation,  string SessionID)
        {

            bool bRes = false;
            long lEllapsedTime = -1;
            Stopwatch watch = null;

            try
            {
                string strURL = oInstallation.INS_STREET_SECTION_OCUP_WS_URL+"/check";
                WebRequest request = WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = Get3rdPartyWSTimeout();
                request.Headers.Add("PX-OCUP-SESSION" ,SessionID);
                request.ContentLength = 0;


                Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupCheckSession request.url={0}, request.json=", strURL), LogLevels.logINFO);

                watch = Stopwatch.StartNew();


                try
                {

                    WebResponse response = request.GetResponse();
                    // Display the status.
                    HttpWebResponse oWebResponse = ((HttpWebResponse)response);


                    if (oWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        // Get the stream containing content returned by the server.
                        Stream dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.

                        Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupCheckSession response.json={0}", PrettyJSON(responseFromServer)), LogLevels.logINFO);
                        // Clean up the streams.


                        reader.Close();
                        dataStream.Close();
                        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

                        BaseResponse oResp = JsonConvert.DeserializeObject<BaseResponse>(responseFromServer, settings);
                        
                        bRes=oResp.ok;
                    }

                    response.Close();

                }
                catch (WebException e)
                {
                    Logger_AddLogMessage(string.Format("ParkXPlorerBilbaoStreetSectionsOccupCheckSession Web Exception HTTP Status={0}", ((HttpWebResponse)e.Response).StatusCode), LogLevels.logERROR);
                    Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupCheckSession::Exception", LogLevels.logERROR);
                }
                catch (Exception e)
                {
                    Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupCheckSession::Exception", LogLevels.logERROR);
                }


                lEllapsedTime = watch.ElapsedMilliseconds;

            }
            catch (Exception e)
            {
                if ((watch != null) && (lEllapsedTime == -1))
                {
                    lEllapsedTime = watch.ElapsedMilliseconds;
                }
                Logger_AddLogException(e, "ParkXPlorerBilbaoStreetSectionsOccupCheckSession::Exception", LogLevels.logERROR);

            }

            return bRes;

        }

        private void ED50Z30ToGPS(MapPoint inPoint, out MapPoint outPoint)
        {
            double[] fromPoint = new double[2];
            fromPoint[0] = Convert.ToDouble(inPoint.x);
            fromPoint[1] = Convert.ToDouble(inPoint.y);
            outPoint = new MapPoint();

            CoordinateSystemFactory cf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            ICoordinateSystem sys4326 = cf.CreateFromWkt ( wkt4326 );
            ICoordinateSystem sys23030 = cf.CreateFromWkt ( wkt23030 );

            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(sys23030, sys4326);
            double[] toPoint = trans.MathTransform.Transform(fromPoint);
          
            outPoint.x = Convert.ToDecimal(toPoint[0]);
            outPoint.y = Convert.ToDecimal(toPoint[1]);
        }

        private bool IsPointInsidePolygon(MapPoint p, List<MapPoint> Polygon)
        {
            decimal dAngle = 0;

            try
            {

                for (int i = 0; i < Polygon.Count; i++)
                {
                    System.Windows.Vector v1 = new System.Windows.Vector(Convert.ToDouble(Polygon[i].x - p.x), Convert.ToDouble(Polygon[i].y - p.y));
                    System.Windows.Vector v2 = new System.Windows.Vector(Convert.ToDouble(Polygon[(i + 1) % Polygon.Count].x - p.x),
                                           Convert.ToDouble(Polygon[(i + 1) % Polygon.Count].y - p.y));

                    dAngle = dAngle + Convert.ToDecimal((System.Windows.Vector.AngleBetween(v1, v2) * Math.PI / 180));

                }
            }
            catch (Exception e)
            {

                dAngle = 0;
            }


            return (Math.Abs(Convert.ToDouble(dAngle)) > Math.PI);
        }

        private decimal GetDistanceKM(MapPoint A, MapPoint B)
        {
            double aStartLat = ConvertToRadians(A.y);
            double aStartLong = ConvertToRadians(A.x);
            double aEndLat = ConvertToRadians(B.y);
            double aEndLong = ConvertToRadians(B.x);

            double distance = Math.Acos(Math.Sin(aStartLat) * Math.Sin(aEndLat)
                    + Math.Cos(aStartLat) * Math.Cos(aEndLat)
                    * Math.Cos(aEndLong - aStartLong));

            return Convert.ToDecimal(EARTH_RADIUS_KM * distance);
        }

        private double ConvertToRadians(decimal angle)
        {
            return (Math.PI / 180) * Convert.ToDouble(angle);
        }


    }
}
