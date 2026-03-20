using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TuneFlow.Logic;

namespace TuneFlow.UI
{
    public partial class Form1 : Form
    {
        private TrackRepository _repo;

        // WinAPI для перетаскивания окна
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public Form1()
        {
            InitializeComponent();
            _repo = new TrackRepository();

            // 1. Убираем рамку Windows
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // 2. Настраиваем всё программно
            ApplyTheme();
            BindControls();
        }

        private void ApplyTheme()
        {
            // Цвета Spotify
            this.BackColor = Color.FromArgb(18, 18, 18);

            // Проверяем, что панели созданы, и красим их
            if (pnlHeader != null) pnlHeader.BackColor = Color.FromArgb(10, 10, 10);
            if (pnlSidebar != null) pnlSidebar.BackColor = Color.Black;
            if (pnlPlayer != null) pnlPlayer.BackColor = Color.FromArgb(24, 24, 24);

            if (lblLogo != null)
            {
                lblLogo.Text = "TUNEFLOW";
                lblLogo.ForeColor = Color.FromArgb(30, 215, 96);
                lblLogo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }

            // Таблица
            if (dgvTracks != null)
            {
                dgvTracks.BackgroundColor = Color.FromArgb(18, 18, 18);
                dgvTracks.BorderStyle = BorderStyle.None;
                dgvTracks.EnableHeadersVisualStyles = false;
                dgvTracks.RowHeadersVisible = false;
                dgvTracks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvTracks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(18, 18, 18);
                dgvTracks.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gray;
                dgvTracks.DefaultCellStyle.BackColor = Color.FromArgb(18, 18, 18);
                dgvTracks.DefaultCellStyle.ForeColor = Color.White;
                dgvTracks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            }

            // Стили кнопок
            StyleBtn(btnClose, "✕");
            StyleBtn(btnMaximize, "▢");
            StyleBtn(btnMinimize, "—");
        }

        private void StyleBtn(Button btn, string text)
        {
            if (btn == null) return;
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
        }

        private void BindControls()
        {
            // --- 1. КНОПКИ (КЛИКИ И ЦВЕТА) ---
            if (btnClose != null)
            {
                btnClose.Click += (s, e) => Application.Exit();
                // При наведении — красный как в Windows 11
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(232, 17, 35);
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
                // При нажатии — темно-красный
                btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 0, 0);
            }

            if (btnMinimize != null)
            {
                btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
                // При наведении — мягкий серый (Spotify Style)
                btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(40, 40, 40);
                btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;
            }

            if (btnMaximize != null)
            {
                btnMaximize.Click += (s, e) => {
                    this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    btnMaximize.Text = this.WindowState == FormWindowState.Maximized ? "❐" : "▢";
                };
                btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = Color.FromArgb(40, 40, 40);
                btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.Transparent;
            }

            // --- 2. ПЕРЕТАСКИВАНИЕ ОКНА (Оставляем) ---
            if (pnlHeader != null)
            {
                pnlHeader.MouseDown += (s, e) => {
                    ReleaseCapture();
                    SendMessage(this.Handle, 0x112, 0xf012, 0);
                };
            }

            // --- 3. ЗАГРУЗКА ДАННЫХ (ВОТ ЭТОТ КУСОК ПРОПАДАЛ) ---
            this.Load += (s, e) => {
                if (dgvTracks != null && _repo != null)
                {
                    // Берем 1000 треков из репозитория
                    dgvTracks.DataSource = _repo.GetAll();

                    // Настраиваем колонки, чтобы выглядело профессионально
                    if (dgvTracks.Columns["Duration"] != null) dgvTracks.Columns["Duration"].Visible = false;

                    dgvTracks.Columns["Title"].HeaderText = "НАЗВАНИЕ";
                    dgvTracks.Columns["Artist"].HeaderText = "ИСПОЛНИТЕЛЬ";
                    dgvTracks.Columns["Genre"].HeaderText = "ЖАНР";
                    dgvTracks.Columns["Year"].HeaderText = "ГОД";
                    dgvTracks.Columns["Rating"].HeaderText = "РЕЙТИНГ";

                    // Чтобы текст не обрезался
                    dgvTracks.Columns["Title"].FillWeight = 150;
                }
            };
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {

        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {

        }
    }
}