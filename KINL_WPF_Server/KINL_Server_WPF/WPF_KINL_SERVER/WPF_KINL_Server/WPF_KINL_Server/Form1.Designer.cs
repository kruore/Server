namespace WPF_KINL_Server
{
    partial class Server
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.StateLabel = new System.Windows.Forms.Label();
            this.serverStateLabel = new System.Windows.Forms.Label();
            this.serverResetButton = new System.Windows.Forms.Button();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControllDevice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviceData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.UserName,
            this.ConnectDate,
            this.Data,
            this.ControllDevice,
            this.DeviceData});
            this.dataGridView1.Location = new System.Drawing.Point(0, 40);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(995, 599);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // StateLabel
            // 
            this.StateLabel.AutoSize = true;
            this.StateLabel.Location = new System.Drawing.Point(1012, 67);
            this.StateLabel.Name = "StateLabel";
            this.StateLabel.Size = new System.Drawing.Size(81, 12);
            this.StateLabel.TabIndex = 1;
            this.StateLabel.Text = "ServerState : ";
            this.StateLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // serverStateLabel
            // 
            this.serverStateLabel.AutoSize = true;
            this.serverStateLabel.ForeColor = System.Drawing.Color.Red;
            this.serverStateLabel.Location = new System.Drawing.Point(1100, 66);
            this.serverStateLabel.Name = "serverStateLabel";
            this.serverStateLabel.Size = new System.Drawing.Size(49, 12);
            this.serverStateLabel.TabIndex = 2;
            this.serverStateLabel.Text = "Activate";
            // 
            // serverResetButton
            // 
            this.serverResetButton.Location = new System.Drawing.Point(1014, 96);
            this.serverResetButton.Name = "serverResetButton";
            this.serverResetButton.Size = new System.Drawing.Size(110, 29);
            this.serverResetButton.TabIndex = 3;
            this.serverResetButton.Text = "serverReset";
            this.serverResetButton.UseVisualStyleBackColor = true;
            this.serverResetButton.Click += new System.EventHandler(this.serverResetButton_Click);
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(1014, 571);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(138, 55);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // ID
            // 
            this.ID.FillWeight = 50F;
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.Width = 50;
            // 
            // UserName
            // 
            this.UserName.HeaderText = "UserName";
            this.UserName.Name = "UserName";
            // 
            // ConnectDate
            // 
            this.ConnectDate.HeaderText = "ConnectDate";
            this.ConnectDate.Name = "ConnectDate";
            // 
            // Data
            // 
            this.Data.FillWeight = 300F;
            this.Data.HeaderText = "Data";
            this.Data.Name = "Data";
            this.Data.Width = 300;
            // 
            // ControllDevice
            // 
            this.ControllDevice.HeaderText = "ControllDevice";
            this.ControllDevice.Name = "ControllDevice";
            // 
            // DeviceData
            // 
            this.DeviceData.FillWeight = 300F;
            this.DeviceData.HeaderText = "DeviceData";
            this.DeviceData.Name = "DeviceData";
            this.DeviceData.Width = 300;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 638);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.serverResetButton);
            this.Controls.Add(this.serverStateLabel);
            this.Controls.Add(this.StateLabel);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Server";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label StateLabel;
        private System.Windows.Forms.Label serverStateLabel;
        private System.Windows.Forms.Button serverResetButton;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControllDevice;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviceData;
    }
}

