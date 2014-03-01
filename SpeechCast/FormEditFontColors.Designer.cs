namespace SpeechCast
{
    partial class FormEditFontColors
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panelHighForeColor = new System.Windows.Forms.Panel();
            this.panelLowForeColor = new System.Windows.Forms.Panel();
            this.panelBorderColor = new System.Windows.Forms.Panel();
            this.panelTransparentColor = new System.Windows.Forms.Panel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "フォント色１";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "フォント色２";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "縁の色";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "透明色（抜き色）";
            this.label4.Visible = false;
            // 
            // panelHighForeColor
            // 
            this.panelHighForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHighForeColor.Location = new System.Drawing.Point(112, 28);
            this.panelHighForeColor.Name = "panelHighForeColor";
            this.panelHighForeColor.Size = new System.Drawing.Size(129, 16);
            this.panelHighForeColor.TabIndex = 4;
            this.panelHighForeColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // panelLowForeColor
            // 
            this.panelLowForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLowForeColor.Location = new System.Drawing.Point(113, 57);
            this.panelLowForeColor.Name = "panelLowForeColor";
            this.panelLowForeColor.Size = new System.Drawing.Size(129, 16);
            this.panelLowForeColor.TabIndex = 5;
            this.panelLowForeColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(113, 86);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(129, 16);
            this.panelBorderColor.TabIndex = 6;
            this.panelBorderColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // panelTransparentColor
            // 
            this.panelTransparentColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTransparentColor.Location = new System.Drawing.Point(113, 114);
            this.panelTransparentColor.Name = "panelTransparentColor";
            this.panelTransparentColor.Size = new System.Drawing.Size(129, 16);
            this.panelTransparentColor.TabIndex = 7;
            this.panelTransparentColor.Visible = false;
            this.panelTransparentColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(38, 160);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(154, 160);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormEditFontColors
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(273, 195);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panelTransparentColor);
            this.Controls.Add(this.panelBorderColor);
            this.Controls.Add(this.panelLowForeColor);
            this.Controls.Add(this.panelHighForeColor);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEditFontColors";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "フォントカラー";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panelHighForeColor;
        private System.Windows.Forms.Panel panelLowForeColor;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Panel panelTransparentColor;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}