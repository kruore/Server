namespace VoucherApplication
{
    partial class Form1
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
            this.StartButton = new MetroFramework.Controls.MetroButton();
            this.Endbutton = new MetroFramework.Controls.MetroButton();
            this.DataTextBox = new System.Windows.Forms.TextBox();
            this.DatainitialisText = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.EquipIndexText = new MetroFramework.Controls.MetroTextBox();
            this.WeightText = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(806, 263);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(261, 141);
            this.StartButton.TabIndex = 2;
            this.StartButton.Text = "StartButton";
            this.StartButton.UseSelectable = true;
            this.StartButton.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // Endbutton
            // 
            this.Endbutton.Location = new System.Drawing.Point(806, 446);
            this.Endbutton.Name = "Endbutton";
            this.Endbutton.Size = new System.Drawing.Size(261, 141);
            this.Endbutton.TabIndex = 3;
            this.Endbutton.Text = "EndButton";
            this.Endbutton.UseSelectable = true;
            this.Endbutton.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // DataTextBox
            // 
            this.DataTextBox.Location = new System.Drawing.Point(34, 101);
            this.DataTextBox.MaxLength = 300;
            this.DataTextBox.Multiline = true;
            this.DataTextBox.Name = "DataTextBox";
            this.DataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DataTextBox.Size = new System.Drawing.Size(578, 486);
            this.DataTextBox.TabIndex = 5;
            this.DataTextBox.TextChanged += new System.EventHandler(this.DataTextBox_TextChanged);
            // 
            // DatainitialisText
            // 
            // 
            // 
            // 
            this.DatainitialisText.CustomButton.Image = null;
            this.DatainitialisText.CustomButton.Location = new System.Drawing.Point(237, 1);
            this.DatainitialisText.CustomButton.Name = "";
            this.DatainitialisText.CustomButton.Size = new System.Drawing.Size(23, 23);
            this.DatainitialisText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.DatainitialisText.CustomButton.TabIndex = 1;
            this.DatainitialisText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.DatainitialisText.CustomButton.UseSelectable = true;
            this.DatainitialisText.CustomButton.Visible = false;
            this.DatainitialisText.Lines = new string[0];
            this.DatainitialisText.Location = new System.Drawing.Point(806, 101);
            this.DatainitialisText.MaxLength = 32767;
            this.DatainitialisText.Name = "DatainitialisText";
            this.DatainitialisText.PasswordChar = '\0';
            this.DatainitialisText.PromptText = "NoName";
            this.DatainitialisText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.DatainitialisText.SelectedText = "";
            this.DatainitialisText.SelectionLength = 0;
            this.DatainitialisText.SelectionStart = 0;
            this.DatainitialisText.ShortcutsEnabled = true;
            this.DatainitialisText.Size = new System.Drawing.Size(261, 25);
            this.DatainitialisText.TabIndex = 6;
            this.DatainitialisText.UseSelectable = true;
            this.DatainitialisText.WaterMark = "NoName";
            this.DatainitialisText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.DatainitialisText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(719, 101);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(63, 19);
            this.metroLabel1.TabIndex = 7;
            this.metroLabel1.Text = "Dataintial";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(625, 141);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(157, 19);
            this.metroLabel2.TabIndex = 8;
            this.metroLabel2.Text = "Exercise equipment index";
            this.metroLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // EquipIndexText
            // 
            // 
            // 
            // 
            this.EquipIndexText.CustomButton.Image = null;
            this.EquipIndexText.CustomButton.Location = new System.Drawing.Point(237, 1);
            this.EquipIndexText.CustomButton.Name = "";
            this.EquipIndexText.CustomButton.Size = new System.Drawing.Size(23, 23);
            this.EquipIndexText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.EquipIndexText.CustomButton.TabIndex = 1;
            this.EquipIndexText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.EquipIndexText.CustomButton.UseSelectable = true;
            this.EquipIndexText.CustomButton.Visible = false;
            this.EquipIndexText.Lines = new string[0];
            this.EquipIndexText.Location = new System.Drawing.Point(806, 141);
            this.EquipIndexText.MaxLength = 32767;
            this.EquipIndexText.Name = "EquipIndexText";
            this.EquipIndexText.PasswordChar = '\0';
            this.EquipIndexText.PromptText = "Index";
            this.EquipIndexText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.EquipIndexText.SelectedText = "";
            this.EquipIndexText.SelectionLength = 0;
            this.EquipIndexText.SelectionStart = 0;
            this.EquipIndexText.ShortcutsEnabled = true;
            this.EquipIndexText.Size = new System.Drawing.Size(261, 25);
            this.EquipIndexText.TabIndex = 9;
            this.EquipIndexText.UseSelectable = true;
            this.EquipIndexText.WaterMark = "Index";
            this.EquipIndexText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.EquipIndexText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // WeightText
            // 
            // 
            // 
            // 
            this.WeightText.CustomButton.Image = null;
            this.WeightText.CustomButton.Location = new System.Drawing.Point(237, 1);
            this.WeightText.CustomButton.Name = "";
            this.WeightText.CustomButton.Size = new System.Drawing.Size(23, 23);
            this.WeightText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.WeightText.CustomButton.TabIndex = 1;
            this.WeightText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.WeightText.CustomButton.UseSelectable = true;
            this.WeightText.CustomButton.Visible = false;
            this.WeightText.Lines = new string[0];
            this.WeightText.Location = new System.Drawing.Point(806, 186);
            this.WeightText.MaxLength = 32767;
            this.WeightText.Name = "WeightText";
            this.WeightText.PasswordChar = '\0';
            this.WeightText.PromptText = "KG";
            this.WeightText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.WeightText.SelectedText = "";
            this.WeightText.SelectionLength = 0;
            this.WeightText.SelectionStart = 0;
            this.WeightText.ShortcutsEnabled = true;
            this.WeightText.Size = new System.Drawing.Size(261, 25);
            this.WeightText.TabIndex = 10;
            this.WeightText.UseSelectable = true;
            this.WeightText.WaterMark = "KG";
            this.WeightText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.WeightText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(732, 186);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(50, 19);
            this.metroLabel3.TabIndex = 11;
            this.metroLabel3.Text = "Weight";
            this.metroLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 652);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.WeightText);
            this.Controls.Add(this.EquipIndexText);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.DatainitialisText);
            this.Controls.Add(this.DataTextBox);
            this.Controls.Add(this.Endbutton);
            this.Controls.Add(this.StartButton);
            this.Name = "Form1";
            this.Text = "VoucherViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroButton StartButton;
        private MetroFramework.Controls.MetroButton Endbutton;
        private System.Windows.Forms.TextBox DataTextBox;
        private MetroFramework.Controls.MetroTextBox DatainitialisText;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox EquipIndexText;
        private MetroFramework.Controls.MetroTextBox WeightText;
        private MetroFramework.Controls.MetroLabel metroLabel3;
    }
}

