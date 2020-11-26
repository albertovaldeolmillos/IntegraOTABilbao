namespace integraMobile.Reports.Operations
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for ParkingReport.
    /// </summary>
    public partial class ParkingReport : Telerik.Reporting.Report
    {
        public ParkingReport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            this.ApplyResources();
            this.ApplyCurrency(System.Configuration.ConfigurationManager.AppSettings["ApplicationCurrencyISOCode"] ?? "EUR");

            
        }

        private void ParkingReport_ItemDataBinding(object sender, EventArgs e)
        {
            this.dsDetail.SelectCommand += string.Format(" WHERE HIS_OPERATIONS.OPE_DATE >= CONVERT(datetime, '{0:yyyy/MM/dd HH:mm:ss}', 120) AND " +
                                                                "HIS_OPERATIONS.OPE_DATE < DATEADD(day, 1, CONVERT(datetime, '{1:yyyy/MM/dd HH:mm:ss}', 120)) AND " +
                                                                "HIS_OPERATIONS.OPE_USR_ID = {2}",
                                                         this.ReportParameters["DateIni"].Value,
                                                         this.ReportParameters["DateEnd"].Value,
                                                         this.ReportParameters["UserId"].Value);
        }
    }
}