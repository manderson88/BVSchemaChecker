namespace BVSchemaChecker
{
    partial class SchemaUtilityForm
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
            this.btnRoot = new System.Windows.Forms.Button();
            this.txtRootDir = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.dgvDirs = new System.Windows.Forms.DataGridView();
            this.lblMessages = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDirs)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRoot
            // 
            this.btnRoot.Location = new System.Drawing.Point(12, 213);
            this.btnRoot.Name = "btnRoot";
            this.btnRoot.Size = new System.Drawing.Size(75, 23);
            this.btnRoot.TabIndex = 0;
            this.btnRoot.Text = "Set Root Dir";
            this.btnRoot.UseVisualStyleBackColor = true;
            this.btnRoot.Click += new System.EventHandler(this.btnRoot_Click);
            // 
            // txtRootDir
            // 
            this.txtRootDir.Location = new System.Drawing.Point(93, 213);
            this.txtRootDir.Name = "txtRootDir";
            this.txtRootDir.Size = new System.Drawing.Size(179, 20);
            this.txtRootDir.TabIndex = 1;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 170);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(115, 23);
            this.btnSelectFolder.TabIndex = 3;
            this.btnSelectFolder.Text = "Select Folder(s)";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(154, 170);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(118, 23);
            this.btnRun.TabIndex = 4;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // dgvDirs
            // 
            this.dgvDirs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDirs.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvDirs.Location = new System.Drawing.Point(0, 0);
            this.dgvDirs.Name = "dgvDirs";
            this.dgvDirs.Size = new System.Drawing.Size(284, 150);
            this.dgvDirs.TabIndex = 5;
            // 
            // lblMessages
            // 
            this.lblMessages.AutoSize = true;
            this.lblMessages.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblMessages.Location = new System.Drawing.Point(0, 254);
            this.lblMessages.Name = "lblMessages";
            this.lblMessages.Size = new System.Drawing.Size(0, 13);
            this.lblMessages.TabIndex = 6;
            // 
            // SchemaUtilityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 267);
            this.Controls.Add(this.lblMessages);
            this.Controls.Add(this.dgvDirs);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtRootDir);
            this.Controls.Add(this.btnRoot);
            this.Name = "SchemaUtilityForm";
            this.Text = "SchemaUtilityForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvDirs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRoot;
        private System.Windows.Forms.TextBox txtRootDir;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DataGridView dgvDirs;
        private System.Windows.Forms.Label lblMessages;
    }
}