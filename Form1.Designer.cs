using static Npgsql.PostgresTypes.PostgresCompositeType;
using System.Windows.Forms;

namespace PublicTransportTool
{
    partial class Form1 : System.Windows.Forms.Form
    {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.map = new GMap.NET.WindowsForms.GMapControl();
            this.cmbLaht = new System.Windows.Forms.ComboBox();
            this.cmbSiht = new System.Windows.Forms.ComboBox();
            this.cmbLiin = new System.Windows.Forms.ComboBox();
            this.cmbSuund = new System.Windows.Forms.ComboBox();
            this.lblLaht = new System.Windows.Forms.Label();
            this.lblSiht = new System.Windows.Forms.Label();
            this.lblLiin = new System.Windows.Forms.Label();
            this.lblSuund = new System.Windows.Forms.Label();
            this.lstInfo = new System.Windows.Forms.ListBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnOtsi = new System.Windows.Forms.Button();
            this.tmrGPSUpdate = new System.Windows.Forms.Timer(this.components);
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkWheelchair = new System.Windows.Forms.CheckBox();
            this.lblTransfersAmount = new System.Windows.Forms.Label();
            this.nudTransfersAmount = new System.Windows.Forms.NumericUpDown();
            this.btnExport = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransfersAmount)).BeginInit();
            this.SuspendLayout();
            // 
            // map
            // 
            this.map.Bearing = 0F;
            this.map.CanDragMap = true;
            this.map.EmptyTileColor = System.Drawing.Color.Navy;
            this.map.GrayScaleMode = false;
            this.map.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.map.LevelsKeepInMemory = 5;
            this.map.Location = new System.Drawing.Point(0, 0);
            this.map.Margin = new System.Windows.Forms.Padding(0);
            this.map.MarkersEnabled = true;
            this.map.MaxZoom = 18;
            this.map.MinZoom = 11;
            this.map.MouseWheelZoomEnabled = true;
            this.map.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            this.map.Name = "map";
            this.map.NegativeMode = false;
            this.map.PolygonsEnabled = true;
            this.map.RetryLoadTile = 0;
            this.map.RoutesEnabled = true;
            this.map.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.map.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.map.ShowTileGridLines = false;
            this.map.Size = new System.Drawing.Size(1400, 833);
            this.map.TabIndex = 2;
            this.map.Zoom = 11D;
            // 
            // cmbLaht
            // 
            this.cmbLaht.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbLaht.FormattingEnabled = true;
            this.cmbLaht.Location = new System.Drawing.Point(12, 168);
            this.cmbLaht.Margin = new System.Windows.Forms.Padding(0);
            this.cmbLaht.Name = "cmbLaht";
            this.cmbLaht.Size = new System.Drawing.Size(121, 21);
            this.cmbLaht.TabIndex = 3;
            // 
            // cmbSiht
            // 
            this.cmbSiht.FormattingEnabled = true;
            this.cmbSiht.Location = new System.Drawing.Point(12, 208);
            this.cmbSiht.Margin = new System.Windows.Forms.Padding(0);
            this.cmbSiht.Name = "cmbSiht";
            this.cmbSiht.Size = new System.Drawing.Size(121, 21);
            this.cmbSiht.TabIndex = 4;
            // 
            // cmbLiin
            // 
            this.cmbLiin.FormattingEnabled = true;
            this.cmbLiin.Location = new System.Drawing.Point(12, 248);
            this.cmbLiin.Margin = new System.Windows.Forms.Padding(0);
            this.cmbLiin.Name = "cmbLiin";
            this.cmbLiin.Size = new System.Drawing.Size(121, 21);
            this.cmbLiin.TabIndex = 5;
            // 
            // cmbSuund
            // 
            this.cmbSuund.FormattingEnabled = true;
            this.cmbSuund.Location = new System.Drawing.Point(12, 288);
            this.cmbSuund.Margin = new System.Windows.Forms.Padding(0);
            this.cmbSuund.Name = "cmbSuund";
            this.cmbSuund.Size = new System.Drawing.Size(121, 21);
            this.cmbSuund.TabIndex = 6;
            // 
            // lblLaht
            // 
            this.lblLaht.AutoSize = true;
            this.lblLaht.Location = new System.Drawing.Point(12, 152);
            this.lblLaht.Margin = new System.Windows.Forms.Padding(0);
            this.lblLaht.Name = "lblLaht";
            this.lblLaht.Size = new System.Drawing.Size(60, 13);
            this.lblLaht.TabIndex = 7;
            this.lblLaht.Text = "Lähtpeatus";
            // 
            // lblSiht
            // 
            this.lblSiht.AutoSize = true;
            this.lblSiht.Location = new System.Drawing.Point(9, 192);
            this.lblSiht.Margin = new System.Windows.Forms.Padding(0);
            this.lblSiht.Name = "lblSiht";
            this.lblSiht.Size = new System.Drawing.Size(57, 13);
            this.lblSiht.TabIndex = 8;
            this.lblSiht.Text = "Sihtpeatus";
            // 
            // lblLiin
            // 
            this.lblLiin.AutoSize = true;
            this.lblLiin.Location = new System.Drawing.Point(12, 232);
            this.lblLiin.Margin = new System.Windows.Forms.Padding(0);
            this.lblLiin.Name = "lblLiin";
            this.lblLiin.Size = new System.Drawing.Size(23, 13);
            this.lblLiin.TabIndex = 9;
            this.lblLiin.Text = "Liin";
            // 
            // lblSuund
            // 
            this.lblSuund.AutoSize = true;
            this.lblSuund.Location = new System.Drawing.Point(12, 272);
            this.lblSuund.Margin = new System.Windows.Forms.Padding(0);
            this.lblSuund.Name = "lblSuund";
            this.lblSuund.Size = new System.Drawing.Size(38, 13);
            this.lblSuund.TabIndex = 10;
            this.lblSuund.Text = "Suund";
            // 
            // lstInfo
            // 
            this.lstInfo.FormattingEnabled = true;
            this.lstInfo.Location = new System.Drawing.Point(12, 12);
            this.lstInfo.Margin = new System.Windows.Forms.Padding(0);
            this.lstInfo.Name = "lstInfo";
            this.lstInfo.Size = new System.Drawing.Size(342, 134);
            this.lstInfo.TabIndex = 11;
            // 
            // btnOtsi
            // 
            this.btnOtsi.Location = new System.Drawing.Point(12, 315);
            this.btnOtsi.Margin = new System.Windows.Forms.Padding(0);
            this.btnOtsi.Name = "btnOtsi";
            this.btnOtsi.Size = new System.Drawing.Size(121, 23);
            this.btnOtsi.TabIndex = 12;
            this.btnOtsi.Text = "Otsi";
            this.btnOtsi.UseVisualStyleBackColor = true;
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
            this.PictureBox1.Location = new System.Drawing.Point(169, 168);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(21, 21);
            this.PictureBox1.TabIndex = 14;
            this.PictureBox1.TabStop = false;
            // 
            // checkWheelchair
            // 
            this.checkWheelchair.AutoSize = true;
            this.checkWheelchair.Location = new System.Drawing.Point(148, 171);
            this.checkWheelchair.Margin = new System.Windows.Forms.Padding(0);
            this.checkWheelchair.Name = "checkWheelchair";
            this.checkWheelchair.Size = new System.Drawing.Size(15, 14);
            this.checkWheelchair.TabIndex = 15;
            this.checkWheelchair.UseVisualStyleBackColor = true;
            // 
            // lblTransfersAmount
            // 
            this.lblTransfersAmount.AutoSize = true;
            this.lblTransfersAmount.Location = new System.Drawing.Point(145, 192);
            this.lblTransfersAmount.Margin = new System.Windows.Forms.Padding(0);
            this.lblTransfersAmount.Name = "lblTransfersAmount";
            this.lblTransfersAmount.Size = new System.Drawing.Size(93, 13);
            this.lblTransfersAmount.TabIndex = 18;
            this.lblTransfersAmount.Text = "Ümberistumise arv";
            // 
            // nudTransfersAmount
            // 
            this.nudTransfersAmount.Location = new System.Drawing.Point(148, 208);
            this.nudTransfersAmount.Margin = new System.Windows.Forms.Padding(0);
            this.nudTransfersAmount.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudTransfersAmount.Name = "nudTransfersAmount";
            this.nudTransfersAmount.ReadOnly = true;
            this.nudTransfersAmount.Size = new System.Drawing.Size(120, 20);
            this.nudTransfersAmount.TabIndex = 19;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(1296, 782);
            this.btnExport.Margin = new System.Windows.Forms.Padding(0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(0, 1);
            this.btnExport.TabIndex = 20;
            this.btnExport.Text = "Export list of all stops to CSV";
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1399, 830);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.nudTransfersAmount);
            this.Controls.Add(this.lblTransfersAmount);
            this.Controls.Add(this.checkWheelchair);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.btnOtsi);
            this.Controls.Add(this.lstInfo);
            this.Controls.Add(this.lblSuund);
            this.Controls.Add(this.lblLiin);
            this.Controls.Add(this.lblSiht);
            this.Controls.Add(this.lblLaht);
            this.Controls.Add(this.cmbSuund);
            this.Controls.Add(this.cmbLiin);
            this.Controls.Add(this.cmbSiht);
            this.Controls.Add(this.cmbLaht);
            this.Controls.Add(this.map);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "Form1";
            this.Text = "Transport Map";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransfersAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private GMap.NET.WindowsForms.GMapControl map;
        private ComboBox cmbLaht;
        private ComboBox cmbSiht;
        private ComboBox cmbLiin;
        private ComboBox cmbSuund;
        private Label lblLaht;
        private Label lblSiht;
        private Label lblLiin;
        private Label lblSuund;
        private ListBox lstInfo;
        private Timer tmrUpdate;
        private Button btnOtsi;
        private Timer tmrGPSUpdate;
        private PictureBox PictureBox1;
        private CheckBox checkWheelchair;
        private Label lblTransfersAmount;
        private NumericUpDown nudTransfersAmount;
        private Button btnExport;

        #endregion
    }
}

