namespace MissionPlanner.GCSViews.ConfigurationView
{
    partial class ConfigHWECU
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelECUconfig = new System.Windows.Forms.Label();
            this.buttonECU = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelECUconfig
            // 
            this.labelECUconfig.AutoSize = true;
            this.labelECUconfig.Location = new System.Drawing.Point(53, 31);
            this.labelECUconfig.Name = "labelECUconfig";
            this.labelECUconfig.Size = new System.Drawing.Size(94, 13);
            this.labelECUconfig.TabIndex = 0;
            this.labelECUconfig.Text = "ECU Configuration";
            // 
            // buttonECU
            // 
            this.buttonECU.Location = new System.Drawing.Point(56, 70);
            this.buttonECU.Name = "buttonECU";
            this.buttonECU.Size = new System.Drawing.Size(75, 23);
            this.buttonECU.TabIndex = 1;
            this.buttonECU.Text = "ECU";
            this.buttonECU.UseVisualStyleBackColor = true;
            // 
            // ConfigECU
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonECU);
            this.Controls.Add(this.labelECUconfig);
            this.Name = "ConfigECU";
            this.Size = new System.Drawing.Size(536, 346);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelECUconfig;
        private System.Windows.Forms.Button buttonECU;
    }
}
