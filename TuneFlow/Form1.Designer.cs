namespace TuneFlow
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
            pnlTitleBar = new Panel();
            lblName = new Label();
            btnMinimize = new Button();
            btnMaximize = new Button();
            btnClose = new Button();
            mainTable = new TableLayoutPanel();
            pnlSidebar = new Panel();
            btnMenuStats = new Button();
            btnMenuRecs = new Button();
            btnMenuPlaylists = new Button();
            btnMenuCatalog = new Button();
            tabControlMain = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            pnlTitleBar.SuspendLayout();
            mainTable.SuspendLayout();
            pnlSidebar.SuspendLayout();
            tabControlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTitleBar
            // 
            pnlTitleBar.BackColor = Color.Black;
            pnlTitleBar.Controls.Add(lblName);
            pnlTitleBar.Controls.Add(btnMinimize);
            pnlTitleBar.Controls.Add(btnMaximize);
            pnlTitleBar.Controls.Add(btnClose);
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Location = new Point(0, 0);
            pnlTitleBar.Name = "pnlTitleBar";
            pnlTitleBar.Size = new Size(1200, 109);
            pnlTitleBar.TabIndex = 0;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.BackColor = Color.Transparent;
            lblName.FlatStyle = FlatStyle.Flat;
            lblName.Font = new Font("Segoe UI", 30F);
            lblName.ForeColor = Color.FromArgb(30, 215, 96);
            lblName.Location = new Point(3, 20);
            lblName.Name = "lblName";
            lblName.Size = new Size(238, 67);
            lblName.TabIndex = 3;
            lblName.Text = "TuneFlow";
            // 
            // btnMinimize
            // 
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 40);
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 64, 64);
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Location = new Point(1050, 3);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(45, 40);
            btnMinimize.TabIndex = 2;
            btnMinimize.Text = "—";
            btnMinimize.UseVisualStyleBackColor = true;
            // 
            // btnMaximize
            // 
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 40);
            btnMaximize.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 64, 64);
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.ForeColor = Color.White;
            btnMaximize.Location = new Point(1101, 3);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(45, 40);
            btnMaximize.TabIndex = 1;
            btnMaximize.Text = "🗖";
            btnMaximize.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.Red;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(1152, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 40);
            btnClose.TabIndex = 0;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // mainTable
            // 
            mainTable.ColumnCount = 2;
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(pnlSidebar, 0, 0);
            mainTable.Controls.Add(tabControlMain, 1, 0);
            mainTable.Dock = DockStyle.Fill;
            mainTable.Location = new Point(0, 109);
            mainTable.Name = "mainTable";
            mainTable.RowCount = 1;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.Size = new Size(1200, 641);
            mainTable.TabIndex = 1;
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.Black;
            pnlSidebar.Controls.Add(btnMenuStats);
            pnlSidebar.Controls.Add(btnMenuRecs);
            pnlSidebar.Controls.Add(btnMenuPlaylists);
            pnlSidebar.Controls.Add(btnMenuCatalog);
            pnlSidebar.Dock = DockStyle.Fill;
            pnlSidebar.Location = new Point(3, 3);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(214, 635);
            pnlSidebar.TabIndex = 0;
            // 
            // btnMenuStats
            // 
            btnMenuStats.FlatAppearance.BorderSize = 0;
            btnMenuStats.FlatStyle = FlatStyle.Flat;
            btnMenuStats.Font = new Font("Segoe UI", 15F);
            btnMenuStats.ForeColor = Color.Gray;
            btnMenuStats.Location = new Point(2, 171);
            btnMenuStats.Name = "btnMenuStats";
            btnMenuStats.Padding = new Padding(10, 0, 0, 0);
            btnMenuStats.Size = new Size(208, 50);
            btnMenuStats.TabIndex = 3;
            btnMenuStats.Text = "Статистика";
            btnMenuStats.TextAlign = ContentAlignment.MiddleLeft;
            btnMenuStats.UseVisualStyleBackColor = true;
            btnMenuStats.Click += btnMenuStats_Click;
            btnMenuStats.MouseEnter += MenuButton_MouseEnter;
            btnMenuStats.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnMenuRecs
            // 
            btnMenuRecs.FlatAppearance.BorderSize = 0;
            btnMenuRecs.FlatStyle = FlatStyle.Flat;
            btnMenuRecs.Font = new Font("Segoe UI", 15F);
            btnMenuRecs.ForeColor = Color.Gray;
            btnMenuRecs.Location = new Point(2, 116);
            btnMenuRecs.Name = "btnMenuRecs";
            btnMenuRecs.Padding = new Padding(10, 0, 0, 0);
            btnMenuRecs.Size = new Size(208, 50);
            btnMenuRecs.TabIndex = 2;
            btnMenuRecs.Text = "Рекомендации";
            btnMenuRecs.TextAlign = ContentAlignment.MiddleLeft;
            btnMenuRecs.UseVisualStyleBackColor = true;
            btnMenuRecs.Click += btnMenuRecs_Click;
            btnMenuRecs.MouseEnter += MenuButton_MouseEnter;
            btnMenuRecs.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnMenuPlaylists
            // 
            btnMenuPlaylists.FlatAppearance.BorderSize = 0;
            btnMenuPlaylists.FlatStyle = FlatStyle.Flat;
            btnMenuPlaylists.Font = new Font("Segoe UI", 15F);
            btnMenuPlaylists.ForeColor = Color.Gray;
            btnMenuPlaylists.Location = new Point(3, 61);
            btnMenuPlaylists.Name = "btnMenuPlaylists";
            btnMenuPlaylists.Padding = new Padding(10, 0, 0, 0);
            btnMenuPlaylists.Size = new Size(208, 50);
            btnMenuPlaylists.TabIndex = 1;
            btnMenuPlaylists.Text = "Плейлисты";
            btnMenuPlaylists.TextAlign = ContentAlignment.MiddleLeft;
            btnMenuPlaylists.UseVisualStyleBackColor = true;
            btnMenuPlaylists.Click += btnMenuPlaylists_Click;
            btnMenuPlaylists.MouseEnter += MenuButton_MouseEnter;
            btnMenuPlaylists.MouseLeave += MenuButton_MouseLeave;
            // 
            // btnMenuCatalog
            // 
            btnMenuCatalog.FlatAppearance.BorderSize = 0;
            btnMenuCatalog.FlatStyle = FlatStyle.Flat;
            btnMenuCatalog.Font = new Font("Segoe UI", 15F);
            btnMenuCatalog.ForeColor = Color.Gray;
            btnMenuCatalog.Location = new Point(2, 6);
            btnMenuCatalog.Name = "btnMenuCatalog";
            btnMenuCatalog.Padding = new Padding(10, 0, 0, 0);
            btnMenuCatalog.Size = new Size(208, 50);
            btnMenuCatalog.TabIndex = 0;
            btnMenuCatalog.Text = "Каталог";
            btnMenuCatalog.TextAlign = ContentAlignment.MiddleLeft;
            btnMenuCatalog.UseVisualStyleBackColor = true;
            btnMenuCatalog.Click += btnMenuCatalog_Click;
            btnMenuCatalog.MouseEnter += MenuButton_MouseEnter;
            btnMenuCatalog.MouseLeave += MenuButton_MouseLeave;
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPage1);
            tabControlMain.Controls.Add(tabPage2);
            tabControlMain.Controls.Add(tabPage3);
            tabControlMain.Controls.Add(tabPage4);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.ItemSize = new Size(0, 1);
            tabControlMain.Location = new Point(223, 3);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(974, 635);
            tabControlMain.SizeMode = TabSizeMode.Fixed;
            tabControlMain.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.FromArgb(18, 18, 18);
            tabPage1.Location = new Point(4, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(966, 626);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = Color.FromArgb(18, 18, 18);
            tabPage2.Location = new Point(4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(966, 626);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            // 
            // tabPage3
            // 
            tabPage3.BackColor = Color.FromArgb(18, 18, 18);
            tabPage3.Location = new Point(4, 5);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(966, 626);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "tabPage3";
            // 
            // tabPage4
            // 
            tabPage4.BackColor = Color.FromArgb(18, 18, 18);
            tabPage4.Location = new Point(4, 5);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(966, 626);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "tabPage4";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 18, 18);
            ClientSize = new Size(1200, 750);
            Controls.Add(mainTable);
            Controls.Add(pnlTitleBar);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(1200, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TuneFlow";
            pnlTitleBar.ResumeLayout(false);
            pnlTitleBar.PerformLayout();
            mainTable.ResumeLayout(false);
            pnlSidebar.ResumeLayout(false);
            tabControlMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTitleBar;
        private TableLayoutPanel mainTable;
        private Panel pnlSidebar;
        private TabControl tabControlMain;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private Button btnMenuCatalog;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private Button btnMenuPlaylists;
        private Button btnMenuRecs;
        private Button btnMenuStats;
        private Label lblName;
    }
}
