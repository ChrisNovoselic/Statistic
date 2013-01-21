namespace Statistic
{
    partial class Parameters
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
            this.nudnDelayTime = new System.Windows.Forms.NumericUpDown();
            this.lblDelayTime = new System.Windows.Forms.Label();
            this.lblRequeryCount = new System.Windows.Forms.Label();
            this.nudnRequeryCount = new System.Windows.Forms.NumericUpDown();
            this.lblQueryPeriod = new System.Windows.Forms.Label();
            this.nudnQueryPeriod = new System.Windows.Forms.NumericUpDown();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDelayTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudnRequeryCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudnQueryPeriod)).BeginInit();
            this.SuspendLayout();
            // 
            // nudnDelayTime
            // 
            this.nudnDelayTime.Location = new System.Drawing.Point(193, 33);
            this.nudnDelayTime.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.nudnDelayTime.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudnDelayTime.Name = "nudnDelayTime";
            this.nudnDelayTime.Size = new System.Drawing.Size(45, 20);
            this.nudnDelayTime.TabIndex = 0;
            this.nudnDelayTime.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lblDelayTime
            // 
            this.lblDelayTime.AutoSize = true;
            this.lblDelayTime.Location = new System.Drawing.Point(12, 35);
            this.lblDelayTime.Name = "lblDelayTime";
            this.lblDelayTime.Size = new System.Drawing.Size(157, 13);
            this.lblDelayTime.TabIndex = 1;
            this.lblDelayTime.Text = "Задержка ошибки в секундах";
            // 
            // lblRequeryCount
            // 
            this.lblRequeryCount.AutoSize = true;
            this.lblRequeryCount.Location = new System.Drawing.Point(12, 61);
            this.lblRequeryCount.Name = "lblRequeryCount";
            this.lblRequeryCount.Size = new System.Drawing.Size(175, 13);
            this.lblRequeryCount.TabIndex = 3;
            this.lblRequeryCount.Text = "Количество переспросов данных";
            // 
            // nudnRequeryCount
            // 
            this.nudnRequeryCount.Location = new System.Drawing.Point(193, 59);
            this.nudnRequeryCount.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudnRequeryCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudnRequeryCount.Name = "nudnRequeryCount";
            this.nudnRequeryCount.Size = new System.Drawing.Size(45, 20);
            this.nudnRequeryCount.TabIndex = 2;
            this.nudnRequeryCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblQueryPeriod
            // 
            this.lblQueryPeriod.AutoSize = true;
            this.lblQueryPeriod.Location = new System.Drawing.Point(12, 9);
            this.lblQueryPeriod.Name = "lblQueryPeriod";
            this.lblQueryPeriod.Size = new System.Drawing.Size(142, 13);
            this.lblQueryPeriod.TabIndex = 5;
            this.lblQueryPeriod.Text = "Период опроса в секундах";
            // 
            // nudnQueryPeriod
            // 
            this.nudnQueryPeriod.Location = new System.Drawing.Point(193, 7);
            this.nudnQueryPeriod.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.nudnQueryPeriod.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudnQueryPeriod.Name = "nudnQueryPeriod";
            this.nudnQueryPeriod.Size = new System.Drawing.Size(45, 20);
            this.nudnQueryPeriod.TabIndex = 4;
            this.nudnQueryPeriod.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(47, 90);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(128, 90);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(75, 23);
            this.btnDefault.TabIndex = 7;
            this.btnDefault.Text = "Сброс";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // Parameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 125);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblQueryPeriod);
            this.Controls.Add(this.nudnQueryPeriod);
            this.Controls.Add(this.lblRequeryCount);
            this.Controls.Add(this.nudnRequeryCount);
            this.Controls.Add(this.lblDelayTime);
            this.Controls.Add(this.nudnDelayTime);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Parameters";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Параметры";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Parameters_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudnDelayTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudnRequeryCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudnQueryPeriod)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudnDelayTime;
        private System.Windows.Forms.Label lblDelayTime;
        private System.Windows.Forms.Label lblRequeryCount;
        private System.Windows.Forms.NumericUpDown nudnRequeryCount;
        private System.Windows.Forms.Label lblQueryPeriod;
        private System.Windows.Forms.NumericUpDown nudnQueryPeriod;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnDefault;
    }
}