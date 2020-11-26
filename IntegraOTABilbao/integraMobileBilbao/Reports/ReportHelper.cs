using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Reflection;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;
using integraMobile.Infrastructure.Logging.Tools;
using integraMobile.Web.Resources;

namespace integraMobile.Reports
{
    public static class ReportHelper
    {
        private static readonly CLogWrapper m_oLog = new CLogWrapper(typeof(ReportHelper));

        public static bool ApplyCurrency(this Telerik.Reporting.Report oReport, string sCurrencyISOCode)
        {
            bool bRet = false;
            m_oLog.LogMessage(LogLevels.logDEBUG, "ApplyCurrency::>>");

            try
            {
                var oCultures = ReportHelper.CultureInfoFromCurrencyISO(sCurrencyISOCode);
                if (oCultures.Count > 0)
                {
                    oReport.Culture = oCultures[0];
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                m_oLog.LogMessage(LogLevels.logERROR, "ApplyCurrency::Exception", ex);
            }

            m_oLog.LogMessage(LogLevels.logDEBUG, "ApplyCurrency::<<");

            return bRet;
        }

        public static List<CultureInfo> CultureInfoFromCurrencyISO(string isoCode)
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            List<CultureInfo> Result = new List<CultureInfo>();
            foreach (CultureInfo ci in cultures)
            {
                RegionInfo ri = new RegionInfo(ci.LCID);
                if (ri.ISOCurrencySymbol == isoCode)
                {
                    if (!Result.Contains(ci))
                        Result.Add(ci);
                }
            }
            return Result;
        }

        public static bool ApplyResources(this Telerik.Reporting.Report oReport)
        {
            bool bRet = false;
            m_oLog.LogMessage(LogLevels.logDEBUG, "ApplyResources::>>");

            try
            {

                //this.textBox1.Value = resBundle.GetString("Finantial", this.textBox1.Name + ".Value");
                foreach (FieldInfo oField in oReport.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (oField.FieldType.IsSubclassOf(typeof(ReportItemBase)))
                    {
                        ReportItemBase oItem = (ReportItemBase)oField.GetValue(oReport);
                        if (oItem.GetType().GetProperty("Value") != null && oItem.GetType().GetProperty("Value").PropertyType == typeof(string))
                        {
                            try
                            {
                                string sLiteral = ResourceExtension.GetLiteral(string.Format("Report_{0}_{1}_Value", oReport.Name, oItem.Name));
                                if (string.IsNullOrEmpty(sLiteral))
                                    sLiteral = (string)oItem.GetType().GetProperty("Value").GetValue(oItem, null);
                                oItem.GetType().GetProperty("Value").SetValue(oItem, sLiteral);
                            }
                            catch (Exception ex)
                            {
                                m_oLog.LogMessage(LogLevels.logWARN, string.Format("ApplyResources::Resources value for '{0}.{1}.Value' couldn't be loaded", oReport.Name, oItem.Name), ex);
                            }
                        }
                    }
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                m_oLog.LogMessage(LogLevels.logERROR, "ApplyResources::Exception", ex);
            }

            m_oLog.LogMessage(LogLevels.logDEBUG, "ApplyResources::<<");

            return bRet;
        }

    }
}