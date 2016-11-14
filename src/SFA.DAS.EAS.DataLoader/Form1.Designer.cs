namespace SFA.DAS.EAS.DataLoader
{
    partial class Form1
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
            this.processFile = new System.Windows.Forms.Button();
            this.browseFile = new System.Windows.Forms.Button();
            this.txtExcelFile = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // processFile
            // 
            this.processFile.Location = new System.Drawing.Point(75, 99);
            this.processFile.Name = "processFile";
            this.processFile.Size = new System.Drawing.Size(280, 69);
            this.processFile.TabIndex = 0;
            this.processFile.Text = "Process File";
            this.processFile.UseVisualStyleBackColor = true;
            this.processFile.Click += new System.EventHandler(this.processFile_Click);
            // 
            // browseFile
            // 
            this.browseFile.Location = new System.Drawing.Point(324, 45);
            this.browseFile.Name = "browseFile";
            this.browseFile.Size = new System.Drawing.Size(31, 23);
            this.browseFile.TabIndex = 1;
            this.browseFile.Text = "...";
            this.browseFile.UseVisualStyleBackColor = true;
            this.browseFile.Click += new System.EventHandler(this.browseFile_Click);
            // 
            // txtExcelFile
            // 
            this.txtExcelFile.Location = new System.Drawing.Point(84, 45);
            this.txtExcelFile.Name = "txtExcelFile";
            this.txtExcelFile.Size = new System.Drawing.Size(234, 22);
            this.txtExcelFile.TabIndex = 2;
            this.txtExcelFile.TextChanged += new System.EventHandler(this.txtExcelFile_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 192);
            this.Controls.Add(this.txtExcelFile);
            this.Controls.Add(this.browseFile);
            this.Controls.Add(this.processFile);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button processFile;
        private System.Windows.Forms.Button browseFile;
        private System.Windows.Forms.TextBox txtExcelFile;
    }
}

