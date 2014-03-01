namespace SpeechCast
{
    partial class FormWrite
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
            this.labelThreadTitle = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxContents = new System.Windows.Forms.TextBox();
            this.textBoxThreadTitle = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxMailAddress = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.buttonRunEditor = new SpeechCast.Controls.ButtonRunEditor();
            this.checkBoxSage = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // labelThreadTitle
            // 
            this.labelThreadTitle.AutoSize = true;
            this.labelThreadTitle.Location = new System.Drawing.Point(13, 13);
            this.labelThreadTitle.Name = "labelThreadTitle";
            this.labelThreadTitle.Size = new System.Drawing.Size(40, 12);
            this.labelThreadTitle.TabIndex = 0;
            this.labelThreadTitle.Text = "スレタイ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "名前";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "メール";
            // 
            // textBoxContents
            // 
            this.textBoxContents.AcceptsReturn = true;
            this.textBoxContents.Location = new System.Drawing.Point(13, 81);
            this.textBoxContents.MaxLength = 0;
            this.textBoxContents.Multiline = true;
            this.textBoxContents.Name = "textBoxContents";
            this.textBoxContents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxContents.Size = new System.Drawing.Size(479, 136);
            this.textBoxContents.TabIndex = 3;
            this.textBoxContents.WordWrap = false;
            // 
            // textBoxThreadTitle
            // 
            this.textBoxThreadTitle.Location = new System.Drawing.Point(69, 10);
            this.textBoxThreadTitle.Name = "textBoxThreadTitle";
            this.textBoxThreadTitle.Size = new System.Drawing.Size(423, 19);
            this.textBoxThreadTitle.TabIndex = 0;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(50, 48);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(165, 19);
            this.textBoxName.TabIndex = 1;
            // 
            // textBoxMailAddress
            // 
            this.textBoxMailAddress.Location = new System.Drawing.Point(260, 48);
            this.textBoxMailAddress.Name = "textBoxMailAddress";
            this.textBoxMailAddress.Size = new System.Drawing.Size(174, 19);
            this.textBoxMailAddress.TabIndex = 2;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(207, 223);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(127, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK (Shift+Enter)";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(383, 224);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Image = global::SpeechCast.Properties.Resources.play;
            this.buttonPlay.Location = new System.Drawing.Point(503, 117);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(46, 23);
            this.buttonPlay.TabIndex = 7;
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonRunEditor
            // 
            this.buttonRunEditor.AutoSize = true;
            this.buttonRunEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonRunEditor.EditorErrorMessage = "エディターが見つかりません。\\n設定→その他タブで設定してください。";
            this.buttonRunEditor.Location = new System.Drawing.Point(499, 81);
            this.buttonRunEditor.Name = "buttonRunEditor";
            this.buttonRunEditor.Size = new System.Drawing.Size(53, 30);
            this.buttonRunEditor.TabIndex = 6;
            this.buttonRunEditor.TextBox = this.textBoxContents;
            this.buttonRunEditor.GetEditorFilePath += new SpeechCast.Controls.GetEditorFilePathEventHandler(this.buttonRunEditor_GetEditorFilePath);
            // 
            // checkBoxSage
            // 
            this.checkBoxSage.AutoSize = true;
            this.checkBoxSage.Location = new System.Drawing.Point(444, 51);
            this.checkBoxSage.Name = "checkBoxSage";
            this.checkBoxSage.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSage.TabIndex = 8;
            this.checkBoxSage.Text = "sage";
            this.checkBoxSage.UseVisualStyleBackColor = true;
            this.checkBoxSage.CheckedChanged += new System.EventHandler(this.checkBoxSage_CheckedChanged);
            // 
            // FormWrite
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(554, 253);
            this.Controls.Add(this.checkBoxSage);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.buttonRunEditor);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxMailAddress);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.textBoxThreadTitle);
            this.Controls.Add(this.textBoxContents);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelThreadTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWrite";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "スレッド作成";
            this.Shown += new System.EventHandler(this.FormWrite_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWrite_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelThreadTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxContents;
        private System.Windows.Forms.TextBox textBoxThreadTitle;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxMailAddress;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private SpeechCast.Controls.ButtonRunEditor buttonRunEditor;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxSage;
    }
}