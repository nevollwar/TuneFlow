namespace TuneFlow.UI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            btnMinimize = new Button();
            btnMaximize = new Button();
            btnClose = new Button();
            lblLogo = new Label();
            pnlPlayer = new Panel();
            pnlSidebar = new Panel();
            dgvTracks = new DataGridView();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTracks).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(10, 10, 10);
            pnlHeader.Controls.Add(btnMinimize);
            pnlHeader.Controls.Add(btnMaximize);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Controls.Add(lblLogo);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1082, 40);
            pnlHeader.TabIndex = 0;
            // 
            // btnMinimize
            // 
            btnMinimize.Dock = DockStyle.Right;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 12F);
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Location = new Point(947, 0);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(45, 40);
            btnMinimize.TabIndex = 3;
            btnMinimize.Text = "—";
            btnMinimize.UseVisualStyleBackColor = true;
            btnMinimize.Click += btnMinimize_Click;
            // 
            // btnMaximize
            // 
            btnMaximize.Dock = DockStyle.Right;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.Font = new Font("Segoe UI", 10F);
            btnMaximize.ForeColor = Color.White;
            btnMaximize.Location = new Point(992, 0);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(45, 40);
            btnMaximize.TabIndex = 2;
            btnMaximize.Text = "▢";
            btnMaximize.UseVisualStyleBackColor = true;
            btnMaximize.Click += btnMaximize_Click;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(1037, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 40);
            btnClose.TabIndex = 1;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblLogo
            // 
            lblLogo.AutoSize = true;
            lblLogo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(30, 215, 96);
            lblLogo.Location = new Point(10, 10);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(102, 28);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "TuneFlow";
            // 
            // pnlPlayer
            // 
            pnlPlayer.BackColor = Color.FromArgb(24, 24, 24);
            pnlPlayer.Dock = DockStyle.Bottom;
            pnlPlayer.Location = new Point(0, 573);
            pnlPlayer.Name = "pnlPlayer";
            pnlPlayer.Size = new Size(1082, 80);
            pnlPlayer.TabIndex = 1;
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.Black;
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 40);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(200, 533);
            pnlSidebar.TabIndex = 2;
            // 
            // dgvTracks
            // 
            dgvTracks.BackgroundColor = Color.FromArgb(18, 18, 18);
            dgvTracks.BorderStyle = BorderStyle.None;
            dgvTracks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTracks.Dock = DockStyle.Fill;
            dgvTracks.Location = new Point(200, 40);
            dgvTracks.Name = "dgvTracks";
            dgvTracks.RowHeadersWidth = 51;
            dgvTracks.Size = new Size(882, 533);
            dgvTracks.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 18, 18);
            ClientSize = new Size(1082, 653);
            Controls.Add(dgvTracks);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlPlayer);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TuneFlow - Рекомендательная система";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTracks).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblLogo;
        private Panel pnlPlayer;
        private Panel pnlSidebar;
        private DataGridView dgvTracks;
        private Button btnClose;
        private Button btnMaximize;
        private Button btnMinimize;
    }
}
