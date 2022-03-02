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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 652);
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
    }
}

