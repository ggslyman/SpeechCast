namespace SpeechCast
{
    partial class FormViewResource
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
            this.tabControlContainer = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabControlContainer
            // 
            this.tabControlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlContainer.Location = new System.Drawing.Point(0, 0);
            this.tabControlContainer.Name = "tabControlContainer";
            this.tabControlContainer.SelectedIndex = 0;
            this.tabControlContainer.Size = new System.Drawing.Size(284, 262);
            this.tabControlContainer.TabIndex = 0;
            this.tabControlContainer.DoubleClick += new System.EventHandler(this.tabControlContainer_DoubleClick);
            this.tabControlContainer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControlContainer_MouseDown);
            // 
            // FormViewResource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.tabControlContainer);
            this.Name = "FormViewResource";
            this.Text = "ビューア";
            this.Load += new System.EventHandler(this.FormViewResouce_Load);
            this.Move += new System.EventHandler(this.FormViewResource_Move);
            this.Resize += new System.EventHandler(this.FormViewResource_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlContainer;
    }
}