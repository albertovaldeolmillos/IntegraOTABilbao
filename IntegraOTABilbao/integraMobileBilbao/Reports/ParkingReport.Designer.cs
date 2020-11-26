namespace integraMobile.Reports.Operations
{
    partial class ParkingReport
    {
        #region Component Designer generated code
        /// <summary>
        /// Required method for telerik Reporting designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParkingReport));
            Telerik.Reporting.ReportParameter reportParameter1 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter2 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.ReportParameter reportParameter3 = new Telerik.Reporting.ReportParameter();
            Telerik.Reporting.Drawing.StyleRule styleRule1 = new Telerik.Reporting.Drawing.StyleRule();
            this.pageHeaderSection1 = new Telerik.Reporting.PageHeaderSection();
            this.lblTitle = new Telerik.Reporting.TextBox();
            this.lblColID = new Telerik.Reporting.TextBox();
            this.lblColPlate = new Telerik.Reporting.TextBox();
            this.lblColType = new Telerik.Reporting.TextBox();
            this.lblColGroup = new Telerik.Reporting.TextBox();
            this.lblColTariff = new Telerik.Reporting.TextBox();
            this.lblColDate = new Telerik.Reporting.TextBox();
            this.lblColIniDate = new Telerik.Reporting.TextBox();
            this.lblColEndDate = new Telerik.Reporting.TextBox();
            this.lblColAmount = new Telerik.Reporting.TextBox();
            this.lblColTime = new Telerik.Reporting.TextBox();
            this.imgLogo = new Telerik.Reporting.PictureBox();
            this.lblParamIniDate = new Telerik.Reporting.TextBox();
            this.lblParamEndDate = new Telerik.Reporting.TextBox();
            this.txtParamIniDate = new Telerik.Reporting.TextBox();
            this.txtParamEndDate = new Telerik.Reporting.TextBox();
            this.detail = new Telerik.Reporting.DetailSection();
            this.txtColID = new Telerik.Reporting.TextBox();
            this.txtColPlate = new Telerik.Reporting.TextBox();
            this.txtColType = new Telerik.Reporting.TextBox();
            this.txtColGroup = new Telerik.Reporting.TextBox();
            this.txtColTariff = new Telerik.Reporting.TextBox();
            this.txtColDate = new Telerik.Reporting.TextBox();
            this.txtColIniDate = new Telerik.Reporting.TextBox();
            this.txtColEndDate = new Telerik.Reporting.TextBox();
            this.txtColAmount = new Telerik.Reporting.TextBox();
            this.txtColTime = new Telerik.Reporting.TextBox();
            this.pageFooterSection1 = new Telerik.Reporting.PageFooterSection();
            this.textBox1 = new Telerik.Reporting.TextBox();
            this.dsDetail = new Telerik.Reporting.SqlDataSource();
            this.reportFooterSection1 = new Telerik.Reporting.ReportFooterSection();
            this.lblTotal = new Telerik.Reporting.TextBox();
            this.txtTotal = new Telerik.Reporting.TextBox();
            this.lblTotal2 = new Telerik.Reporting.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // pageHeaderSection1
            // 
            this.pageHeaderSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(2.5D);
            this.pageHeaderSection1.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.lblTitle,
            this.lblColID,
            this.lblColPlate,
            this.lblColType,
            this.lblColGroup,
            this.lblColTariff,
            this.lblColDate,
            this.lblColIniDate,
            this.lblColEndDate,
            this.lblColAmount,
            this.lblColTime,
            this.imgLogo,
            this.lblParamIniDate,
            this.lblParamEndDate,
            this.txtParamIniDate,
            this.txtParamEndDate});
            this.pageHeaderSection1.Name = "pageHeaderSection1";
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.00010012308484874666D), Telerik.Reporting.Drawing.Unit.Cm(0.30000004172325134D));
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(9.7996988296508789D), Telerik.Reporting.Drawing.Unit.Cm(0.89999997615814209D));
            this.lblTitle.Style.Color = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblTitle.Style.Font.Bold = true;
            this.lblTitle.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(12D);
            this.lblTitle.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblTitle.Value = "JUSTIFICANTE DE APARCAMIENTOS";
            // 
            // lblColID
            // 
            this.lblColID.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.00010012308484874666D), Telerik.Reporting.Drawing.Unit.Cm(1.8999998569488525D));
            this.lblColID.Name = "lblColID";
            this.lblColID.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.5998998880386353D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColID.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColID.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColID.Style.Color = System.Drawing.Color.White;
            this.lblColID.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColID.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColID.Value = "ID";
            // 
            // lblColPlate
            // 
            this.lblColPlate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(1.600199818611145D), Telerik.Reporting.Drawing.Unit.Cm(1.8999998569488525D));
            this.lblColPlate.Name = "lblColPlate";
            this.lblColPlate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.8997998237609863D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColPlate.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColPlate.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColPlate.Style.Color = System.Drawing.Color.White;
            this.lblColPlate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColPlate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColPlate.Value = "Matrícula";
            // 
            // lblColType
            // 
            this.lblColType.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.5001997947692871D), Telerik.Reporting.Drawing.Unit.Cm(1.8999998569488525D));
            this.lblColType.Name = "lblColType";
            this.lblColType.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.2997996807098389D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColType.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColType.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColType.Style.Color = System.Drawing.Color.White;
            this.lblColType.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColType.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColType.Value = "Tipo";
            // 
            // lblColGroup
            // 
            this.lblColGroup.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(5.8001995086669922D), Telerik.Reporting.Drawing.Unit.Cm(1.8999998569488525D));
            this.lblColGroup.Name = "lblColGroup";
            this.lblColGroup.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.2998001575469971D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColGroup.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColGroup.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColGroup.Style.Color = System.Drawing.Color.White;
            this.lblColGroup.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColGroup.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColGroup.Value = "Zona";
            // 
            // lblColTariff
            // 
            this.lblColTariff.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(9.1001996994018555D), Telerik.Reporting.Drawing.Unit.Cm(1.8999994993209839D));
            this.lblColTariff.Name = "lblColTariff";
            this.lblColTariff.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.7999989986419678D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColTariff.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColTariff.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColTariff.Style.Color = System.Drawing.Color.White;
            this.lblColTariff.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColTariff.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColTariff.Value = "Tarifa";
            // 
            // lblColDate
            // 
            this.lblColDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(12.900399208068848D), Telerik.Reporting.Drawing.Unit.Cm(1.8999994993209839D));
            this.lblColDate.Name = "lblColDate";
            this.lblColDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.0474789142608643D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColDate.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColDate.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColDate.Style.Color = System.Drawing.Color.White;
            this.lblColDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColDate.Value = "Fecha";
            // 
            // lblColIniDate
            // 
            this.lblColIniDate.KeepTogether = false;
            this.lblColIniDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(14.948078155517578D), Telerik.Reporting.Drawing.Unit.Cm(1.8999994993209839D));
            this.lblColIniDate.Name = "lblColIniDate";
            this.lblColIniDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.5519232749938965D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColIniDate.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColIniDate.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColIniDate.Style.Color = System.Drawing.Color.White;
            this.lblColIniDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColIniDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColIniDate.Value = "Fecha inicio";
            // 
            // lblColEndDate
            // 
            this.lblColEndDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(17.500200271606445D), Telerik.Reporting.Drawing.Unit.Cm(1.8999994993209839D));
            this.lblColEndDate.Name = "lblColEndDate";
            this.lblColEndDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.79960036277771D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColEndDate.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColEndDate.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColEndDate.Style.Color = System.Drawing.Color.White;
            this.lblColEndDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColEndDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColEndDate.Value = "Fecha fin";
            // 
            // lblColAmount
            // 
            this.lblColAmount.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(20.30000114440918D), Telerik.Reporting.Drawing.Unit.Cm(1.8999994993209839D));
            this.lblColAmount.Name = "lblColAmount";
            this.lblColAmount.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.5198013782501221D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColAmount.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColAmount.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColAmount.Style.Color = System.Drawing.Color.White;
            this.lblColAmount.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColAmount.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColAmount.Value = "Valor";
            // 
            // lblColTime
            // 
            this.lblColTime.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(22.820001602172852D), Telerik.Reporting.Drawing.Unit.Cm(1.8999998569488525D));
            this.lblColTime.Name = "lblColTime";
            this.lblColTime.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999004125595093D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblColTime.Style.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(17)))), ((int)(((byte)(40)))));
            this.lblColTime.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.None;
            this.lblColTime.Style.Color = System.Drawing.Color.White;
            this.lblColTime.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblColTime.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblColTime.Value = "Tiempo";
            // 
            // imgLogo
            // 
            this.imgLogo.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(21.020002365112305D), Telerik.Reporting.Drawing.Unit.Cm(0.30000004172325134D));
            this.imgLogo.MimeType = "image/png";
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.5999999046325684D), Telerik.Reporting.Drawing.Unit.Cm(0.89999997615814209D));
            this.imgLogo.Sizing = Telerik.Reporting.Drawing.ImageSizeMode.ScaleProportional;
            this.imgLogo.Value = ((object)(resources.GetObject("imgLogo.Value")));
            // 
            // lblParamIniDate
            // 
            this.lblParamIniDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(15.447577476501465D), Telerik.Reporting.Drawing.Unit.Cm(0.30000004172325134D));
            this.lblParamIniDate.Name = "lblParamIniDate";
            this.lblParamIniDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999000549316406D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.lblParamIniDate.Style.Font.Bold = true;
            this.lblParamIniDate.Style.Font.Italic = true;
            this.lblParamIniDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(9D);
            this.lblParamIniDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblParamIniDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblParamIniDate.Value = "Desde:";
            // 
            // lblParamEndDate
            // 
            this.lblParamEndDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(15.447577476501465D), Telerik.Reporting.Drawing.Unit.Cm(0.9002000093460083D));
            this.lblParamEndDate.Name = "lblParamEndDate";
            this.lblParamEndDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999000549316406D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.lblParamEndDate.Style.Font.Bold = true;
            this.lblParamEndDate.Style.Font.Italic = true;
            this.lblParamEndDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(9D);
            this.lblParamEndDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblParamEndDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblParamEndDate.Value = "Hasta:";
            // 
            // txtParamIniDate
            // 
            this.txtParamIniDate.Format = "{0:dd/MM/yyyy}";
            this.txtParamIniDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(17.247676849365234D), Telerik.Reporting.Drawing.Unit.Cm(0.30000004172325134D));
            this.txtParamIniDate.Name = "txtParamIniDate";
            this.txtParamIniDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.752122163772583D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtParamIniDate.Style.Font.Bold = true;
            this.txtParamIniDate.Style.Font.Italic = true;
            this.txtParamIniDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(9D);
            this.txtParamIniDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.txtParamIniDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtParamIniDate.Value = "= Parameters.DateIni.Value";
            // 
            // txtParamEndDate
            // 
            this.txtParamEndDate.Format = "{0:dd/MM/yyyy}";
            this.txtParamEndDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(17.247676849365234D), Telerik.Reporting.Drawing.Unit.Cm(0.9002000093460083D));
            this.txtParamEndDate.Name = "txtParamEndDate";
            this.txtParamEndDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.752122163772583D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtParamEndDate.Style.Font.Bold = true;
            this.txtParamEndDate.Style.Font.Italic = true;
            this.txtParamEndDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(9D);
            this.txtParamEndDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.txtParamEndDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtParamEndDate.Value = "= Parameters.DateEnd.Value";
            // 
            // detail
            // 
            this.detail.Height = Telerik.Reporting.Drawing.Unit.Cm(0.60010021924972534D);
            this.detail.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.txtColID,
            this.txtColPlate,
            this.txtColType,
            this.txtColGroup,
            this.txtColTariff,
            this.txtColDate,
            this.txtColIniDate,
            this.txtColEndDate,
            this.txtColAmount,
            this.txtColTime});
            this.detail.Name = "detail";
            // 
            // txtColID
            // 
            this.txtColID.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0.00010012308484874666D), Telerik.Reporting.Drawing.Unit.Cm(0.00010012308484874666D));
            this.txtColID.Name = "txtColID";
            this.txtColID.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.5999000072479248D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColID.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColID.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColID.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColID.Value = "= Fields.OPE_ID";
            // 
            // txtColPlate
            // 
            this.txtColPlate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(1.600199818611145D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColPlate.Name = "txtColPlate";
            this.txtColPlate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.8997995853424072D), Telerik.Reporting.Drawing.Unit.Cm(0.599999725818634D));
            this.txtColPlate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColPlate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColPlate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColPlate.Value = "= Fields.USRP_PLATE";
            // 
            // txtColType
            // 
            this.txtColType.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(3.7000000476837158D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColType.Name = "txtColType";
            this.txtColType.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.4109270572662354D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColType.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColType.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.txtColType.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColType.Value = "= IIf(Fields.OPE_TYPE = 1, \"Aparcamiento\", IIf(Fields.OPE_TYPE = 2,\"Ampliación\", " +
    "\"Desaparcamiento\"))";
            // 
            // txtColGroup
            // 
            this.txtColGroup.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(6.1111268997192383D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColGroup.Name = "txtColGroup";
            this.txtColGroup.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.988872766494751D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColGroup.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColGroup.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.txtColGroup.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColGroup.Value = "= Fields.GRP_DESCRIPTION";
            // 
            // txtColTariff
            // 
            this.txtColTariff.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(9.4001998901367188D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColTariff.Name = "txtColTariff";
            this.txtColTariff.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.4999988079071045D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColTariff.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColTariff.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.txtColTariff.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColTariff.Value = "= Fields.TAR_DESCRIPTION";
            // 
            // txtColDate
            // 
            this.txtColDate.Format = "{0:dd/MM/yyyy HH:mm}";
            this.txtColDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(12.900399208068848D), Telerik.Reporting.Drawing.Unit.Cm(0.00010012308484874666D));
            this.txtColDate.Name = "txtColDate";
            this.txtColDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.0474786758422852D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColDate.Value = "= Fields.OPE_DATE";
            // 
            // txtColIniDate
            // 
            this.txtColIniDate.Format = "{0:dd/MM/yyyy HH:mm}";
            this.txtColIniDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(14.948078155517578D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColIniDate.Name = "txtColIniDate";
            this.txtColIniDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.5519230365753174D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColIniDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColIniDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColIniDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColIniDate.Value = "= IIF(Fields.OPE_TYPE = 3, Fields.OPE_ENDDATE, Fields.OPE_INIDATE)";
            // 
            // txtColEndDate
            // 
            this.txtColEndDate.Format = "{0:dd/MM/yyyy HH:mm}";
            this.txtColEndDate.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(17.500200271606445D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColEndDate.Name = "txtColEndDate";
            this.txtColEndDate.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.7996001243591309D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColEndDate.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColEndDate.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColEndDate.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColEndDate.Value = "= IIF(Fields.OPE_TYPE = 3, Fields.OPE_REFUND_PREVIOUS_ENDDATE, Fields.OPE_ENDDATE" +
    ")";
            // 
            // txtColAmount
            // 
            this.txtColAmount.Format = "{0:C2}";
            this.txtColAmount.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(20.30000114440918D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColAmount.Name = "txtColAmount";
            this.txtColAmount.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(2.519801139831543D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColAmount.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColAmount.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColAmount.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColAmount.Value = "= IIf(Fields.OPE_TYPE = 1, CDbl(IsNull(Fields.OPE_TOTAL_AMOUNT, Fields.OPE_AMOUNT" +
    ")) / 100, IIf(Fields.OPE_TYPE = 2,CDbl(IsNull(Fields.OPE_TOTAL_AMOUNT, Fields.OP" +
    "E_AMOUNT)) / 100, 0))";
            // 
            // txtColTime
            // 
            this.txtColTime.Format = "{0:N0}";
            this.txtColTime.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(22.820001602172852D), Telerik.Reporting.Drawing.Unit.Cm(0D));
            this.txtColTime.Name = "txtColTime";
            this.txtColTime.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999001741409302D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.txtColTime.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Pixel(9D);
            this.txtColTime.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtColTime.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtColTime.Value = "= Fields.OPE_TIME";
            // 
            // pageFooterSection1
            // 
            this.pageFooterSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(1.1000007390975952D);
            this.pageFooterSection1.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.textBox1});
            this.pageFooterSection1.Name = "pageFooterSection1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(22.820001602172852D), Telerik.Reporting.Drawing.Unit.Cm(0.30000025033950806D));
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999000549316406D), Telerik.Reporting.Drawing.Unit.Cm(0.60000008344650269D));
            this.textBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(9D);
            this.textBox1.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.textBox1.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.textBox1.Value = "= Format(\"{0}/{1}\", PageNumber, PageCount)";
            // 
            // dsDetail
            // 
            this.dsDetail.ConnectionString = "integraMobile.Domain.Properties.Settings.integraMobileConnectionString";
            this.dsDetail.Name = "dsDetail";
            this.dsDetail.SelectCommand = resources.GetString("dsDetail.SelectCommand");
            // 
            // reportFooterSection1
            // 
            this.reportFooterSection1.Height = Telerik.Reporting.Drawing.Unit.Cm(1.4998996257781982D);
            this.reportFooterSection1.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.lblTotal,
            this.txtTotal,
            this.lblTotal2});
            this.reportFooterSection1.Name = "reportFooterSection1";
            // 
            // lblTotal
            // 
            this.lblTotal.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(0D), Telerik.Reporting.Drawing.Unit.Cm(0.79989945888519287D));
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(19.745037078857422D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblTotal.Style.BackgroundColor = System.Drawing.Color.Silver;
            this.lblTotal.Style.BorderColor.Top = System.Drawing.Color.Gray;
            this.lblTotal.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.Solid;
            this.lblTotal.Style.BorderStyle.Top = Telerik.Reporting.Drawing.BorderType.Solid;
            this.lblTotal.Style.Color = System.Drawing.Color.Black;
            this.lblTotal.Style.Font.Bold = true;
            this.lblTotal.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Left;
            this.lblTotal.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblTotal.Value = "TOTAL:";
            // 
            // txtTotal
            // 
            this.txtTotal.Format = "{0:C2}";
            this.txtTotal.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(19.745237350463867D), Telerik.Reporting.Drawing.Unit.Cm(0.79989945888519287D));
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(3.0745627880096436D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.txtTotal.Style.BackgroundColor = System.Drawing.Color.Silver;
            this.txtTotal.Style.BorderColor.Top = System.Drawing.Color.Gray;
            this.txtTotal.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.Solid;
            this.txtTotal.Style.BorderStyle.Top = Telerik.Reporting.Drawing.BorderType.Solid;
            this.txtTotal.Style.Color = System.Drawing.Color.Black;
            this.txtTotal.Style.Font.Bold = true;
            this.txtTotal.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.txtTotal.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.txtTotal.Value = "= Sum(IIf(Fields.OPE_TYPE = 1, CDbl(IsNull(Fields.OPE_TOTAL_AMOUNT, Fields.OPE_AM" +
    "OUNT)) / 100, IIf(Fields.OPE_TYPE = 2,CDbl(IsNull(Fields.OPE_TOTAL_AMOUNT, Field" +
    "s.OPE_AMOUNT)) / 100, 0)))";
            // 
            // lblTotal2
            // 
            this.lblTotal2.Location = new Telerik.Reporting.Drawing.PointU(Telerik.Reporting.Drawing.Unit.Cm(22.820001602172852D), Telerik.Reporting.Drawing.Unit.Cm(0.79989945888519287D));
            this.lblTotal2.Name = "lblTotal2";
            this.lblTotal2.Size = new Telerik.Reporting.Drawing.SizeU(Telerik.Reporting.Drawing.Unit.Cm(1.7999006509780884D), Telerik.Reporting.Drawing.Unit.Cm(0.59990018606185913D));
            this.lblTotal2.Style.BackgroundColor = System.Drawing.Color.Silver;
            this.lblTotal2.Style.BorderColor.Top = System.Drawing.Color.Gray;
            this.lblTotal2.Style.BorderStyle.Bottom = Telerik.Reporting.Drawing.BorderType.Solid;
            this.lblTotal2.Style.BorderStyle.Top = Telerik.Reporting.Drawing.BorderType.Solid;
            this.lblTotal2.Style.Color = System.Drawing.Color.Black;
            this.lblTotal2.Style.Font.Bold = true;
            this.lblTotal2.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
            this.lblTotal2.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
            this.lblTotal2.Value = "=\'\'";
            // 
            // ParkingReport
            // 
            this.DataSource = this.dsDetail;
            this.Filters.Add(new Telerik.Reporting.Filter("= Fields.OPE_DATE", Telerik.Reporting.FilterOperator.GreaterOrEqual, "= Parameters.DateIni.Value"));
            this.Filters.Add(new Telerik.Reporting.Filter("= Fields.OPE_DATE", Telerik.Reporting.FilterOperator.LessThan, "= AddDays(Parameters.DateEnd.Value, 1)"));
            this.Filters.Add(new Telerik.Reporting.Filter("= Fields.OPE_USR_ID", Telerik.Reporting.FilterOperator.Equal, "= Parameters.UserId.Value"));
            this.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
            this.pageHeaderSection1,
            this.detail,
            this.pageFooterSection1,
            this.reportFooterSection1});
            this.Name = "ParkingReport";
            this.PageSettings.Landscape = true;
            this.PageSettings.Margins = new Telerik.Reporting.Drawing.MarginsU(Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D), Telerik.Reporting.Drawing.Unit.Mm(25.399999618530273D));
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            reportParameter1.Name = "DateIni";
            reportParameter1.Text = "Fecha Inicio";
            reportParameter1.Type = Telerik.Reporting.ReportParameterType.DateTime;
            reportParameter1.Visible = true;
            reportParameter2.Name = "DateEnd";
            reportParameter2.Text = "Fecha Fin";
            reportParameter2.Type = Telerik.Reporting.ReportParameterType.DateTime;
            reportParameter2.Visible = true;
            reportParameter3.Name = "UserId";
            reportParameter3.Text = "Usuario";
            reportParameter3.Type = Telerik.Reporting.ReportParameterType.Integer;
            reportParameter3.Value = "40636";
            reportParameter3.Visible = true;
            this.ReportParameters.Add(reportParameter1);
            this.ReportParameters.Add(reportParameter2);
            this.ReportParameters.Add(reportParameter3);
            this.Sortings.Add(new Telerik.Reporting.Sorting("= Fields.OPE_DATE", Telerik.Reporting.SortDirection.Asc));
            styleRule1.Selectors.AddRange(new Telerik.Reporting.Drawing.ISelector[] {
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.TextItemBase)),
            new Telerik.Reporting.Drawing.TypeSelector(typeof(Telerik.Reporting.HtmlTextBox))});
            styleRule1.Style.Padding.Left = Telerik.Reporting.Drawing.Unit.Point(2D);
            styleRule1.Style.Padding.Right = Telerik.Reporting.Drawing.Unit.Point(2D);
            this.StyleSheet.AddRange(new Telerik.Reporting.Drawing.StyleRule[] {
            styleRule1});
            this.Width = Telerik.Reporting.Drawing.Unit.Cm(24.6200008392334D);
            this.ItemDataBinding += new System.EventHandler(this.ParkingReport_ItemDataBinding);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        private Telerik.Reporting.PageHeaderSection pageHeaderSection1;
        private Telerik.Reporting.DetailSection detail;
        private Telerik.Reporting.PageFooterSection pageFooterSection1;
        private Telerik.Reporting.SqlDataSource dsDetail;
        private Telerik.Reporting.TextBox lblTitle;
        private Telerik.Reporting.ReportFooterSection reportFooterSection1;
        private Telerik.Reporting.TextBox lblColID;
        private Telerik.Reporting.TextBox txtColID;
        private Telerik.Reporting.TextBox lblTotal;
        private Telerik.Reporting.TextBox lblColPlate;
        private Telerik.Reporting.TextBox lblColType;
        private Telerik.Reporting.TextBox lblColGroup;
        private Telerik.Reporting.TextBox lblColTariff;
        private Telerik.Reporting.TextBox lblColDate;
        private Telerik.Reporting.TextBox lblColIniDate;
        private Telerik.Reporting.TextBox lblColEndDate;
        private Telerik.Reporting.TextBox lblColAmount;
        private Telerik.Reporting.TextBox lblColTime;
        private Telerik.Reporting.TextBox txtColPlate;
        private Telerik.Reporting.TextBox txtColType;
        private Telerik.Reporting.TextBox txtColGroup;
        private Telerik.Reporting.TextBox txtColTariff;
        private Telerik.Reporting.TextBox txtColDate;
        private Telerik.Reporting.TextBox txtColIniDate;
        private Telerik.Reporting.TextBox txtColEndDate;
        private Telerik.Reporting.TextBox txtColAmount;
        private Telerik.Reporting.TextBox txtColTime;
        private Telerik.Reporting.TextBox textBox1;
        private Telerik.Reporting.TextBox txtTotal;
        private Telerik.Reporting.TextBox lblTotal2;
        private Telerik.Reporting.PictureBox imgLogo;
        private Telerik.Reporting.TextBox lblParamIniDate;
        private Telerik.Reporting.TextBox lblParamEndDate;
        private Telerik.Reporting.TextBox txtParamIniDate;
        private Telerik.Reporting.TextBox txtParamEndDate;
    }
}