using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TuneFlow.Logic;

namespace TuneFlow.UI
{
    /// <summary>
    /// Главный контроллер интерфейса музыкальной системы TuneFlow.
    /// Реализует логику ViewState (состояния экрана) и визуализацию метаданных.
    /// </summary>
    public partial class Form1 : Form
    {
        #region Поля и объекты бизнес-логики

        private TrackRepository _repo;
        private List<Track> _favorites = new List<Track>();
        private List<Track> _currentViewList = new List<Track>();
        private bool _isPlaylistView = false;

        private TextBox txtSearch;
        private CustomScrollbar _customScrollbar;
        private bool _sortAscending = true;
        private string _currentSortColumn = "Id";

        #endregion

        #region Импорт функций Windows API (P/Invoke)

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        public Form1()
        {
            InitializeComponent();
            _repo = new TrackRepository();

            // Базовая конфигурация окна
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // Активация Dark Mode для системных компонентов
            int isDarkMode = 1;
            DwmSetWindowAttribute(this.Handle, 20, ref isDarkMode, sizeof(int));

            ApplyTheme();
            CreateDynamicUI();
            BindControls();
        }

        /// <summary>
        /// Применяет визуальные стили (Spotify Dark Theme).
        /// </summary>
        private void ApplyTheme()
        {
            Color darkBg = Color.FromArgb(18, 18, 18);
            this.BackColor = darkBg;

            if (pnlHeader != null) pnlHeader.BackColor = Color.FromArgb(10, 10, 10);
            if (pnlSidebar != null) pnlSidebar.BackColor = Color.Black;
            if (pnlPlayer != null) pnlPlayer.BackColor = Color.FromArgb(24, 24, 24);

            if (lblLogo != null)
            {
                lblLogo.Text = "  TUNEFLOW";
                lblLogo.ForeColor = Color.FromArgb(30, 215, 96);
                lblLogo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }

            if (dgvTracks != null)
            {
                dgvTracks.BackgroundColor = darkBg;
                dgvTracks.BorderStyle = BorderStyle.None;
                dgvTracks.ReadOnly = true;
                dgvTracks.AllowUserToResizeColumns = false;
                dgvTracks.AllowUserToResizeRows = false;
                dgvTracks.EnableHeadersVisualStyles = false;
                dgvTracks.RowHeadersVisible = false;
                dgvTracks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvTracks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvTracks.ScrollBars = ScrollBars.None;

                dgvTracks.GridColor = Color.FromArgb(35, 35, 35);
                dgvTracks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvTracks.DefaultCellStyle.BackColor = darkBg;
                dgvTracks.DefaultCellStyle.ForeColor = Color.FromArgb(220, 220, 220);
                dgvTracks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 45);
                dgvTracks.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                dgvTracks.RowTemplate.Height = 40;

                dgvTracks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvTracks.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
                dgvTracks.ColumnHeadersDefaultCellStyle.BackColor = darkBg;
                dgvTracks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvTracks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgvTracks.ColumnHeadersHeight = 45;

                dgvTracks.MouseEnter += (s, e) => dgvTracks.Focus();

                // Обработка правого клика мыши
                dgvTracks.MouseDown += (s, e) => {
                    if (e.Button == MouseButtons.Right) CreateDynamicContextMenu(e.Location);
                };
            }

            StyleWindowBtn(btnClose, "✕");
            StyleWindowBtn(btnMaximize, "▢");
            StyleWindowBtn(btnMinimize, "—");
        }

        /// <summary>
        /// Контекстное меню, меняющееся в зависимости от текущего экрана (Главная/Плейлист).
        /// </summary>
        private void CreateDynamicContextMenu(Point location)
        {
            var hit = dgvTracks.HitTest(location.X, location.Y);
            if (hit.RowIndex < 0) return;

            dgvTracks.ClearSelection();
            dgvTracks.Rows[hit.RowIndex].Selected = true;

            ContextMenuStrip menu = new ContextMenuStrip { BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, ShowImageMargin = false };
            Track selected = (Track)dgvTracks.Rows[hit.RowIndex].DataBoundItem;

            // Логика пунктов меню
            if (_isPlaylistView)
            {
                var remove = menu.Items.Add("Удалить из плейлиста");
                remove.Click += (s, e) => { _favorites.Remove(selected); LoadData(_favorites); };
            }
            else
            {
                var add = menu.Items.Add("Добавить в избранное");
                add.Click += (s, e) => { if (!_favorites.Contains(selected)) { _favorites.Add(selected); dgvTracks.Invalidate(); } };
            }

            // Рекомендации доступны всегда
            var rec = menu.Items.Add("Показать похожие треки");
            rec.Click += (s, e) => {
                _isPlaylistView = false;
                txtSearch.Text = $" Похожее на: {selected.Title}";
                LoadData(_repo.GetRecommendations(selected));
            };

            menu.Show(dgvTracks, location);
        }

        private void CreateDynamicUI()
        {
            if (pnlHeader != null)
            {
                int sw = 420;
                int sx = (pnlHeader.Width - sw) / 2;

                Label lblSearch = new Label { Text = "Поиск:", ForeColor = Color.Gray, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.Top, Location = new Point(sx - 70, 10) };
                pnlHeader.Controls.Add(lblSearch);

                txtSearch = new TextBox { BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Width = sw, Location = new Point(sx, 8), Anchor = AnchorStyles.Top, Text = " Поиск трека, артиста или жанра..." };
                txtSearch.Enter += (s, e) => { if (txtSearch.Text.Contains("Поиск")) txtSearch.Text = ""; };
                txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = " Поиск трека, артиста или жанра..."; };

                txtSearch.TextChanged += (s, e) => {
                    if (!txtSearch.Text.Contains("Поиск"))
                    {
                        string q = txtSearch.Text.ToLower();
                        List<Track> baseList = _isPlaylistView ? _favorites : _repo.GetAll();
                        var filtered = baseList.Where(t => t.Title.ToLower().Contains(q) || t.Artist.ToLower().Contains(q) || t.Genre.ToLower().Contains(q)).ToList();
                        LoadData(filtered, false);
                    }
                };
                pnlHeader.Controls.Add(txtSearch);
            }

            if (pnlSidebar != null)
            {
                CreateSidebarButton("Главная", 20, () => {
                    _isPlaylistView = false;
                    txtSearch.Text = " Поиск трека, артиста или жанра...";
                    LoadData(_repo.GetAll());
                });

                CreateSidebarButton("Мой плейлист", 65, () => {
                    _isPlaylistView = true;
                    txtSearch.Text = " Поиск по вашему плейлисту...";
                    LoadData(_favorites);
                });

                CreateSidebarButton("Помощь", pnlSidebar.Height - 110, ShowHelpDialog, true);
                CreateSidebarButton("О приложении", pnlSidebar.Height - 65, ShowAboutDialog, true);
            }

            if (dgvTracks != null)
            {
                _customScrollbar = new CustomScrollbar { Dock = DockStyle.Right, Grid = dgvTracks };
                dgvTracks.Parent.Controls.Add(_customScrollbar);
                _customScrollbar.BringToFront();
            }
        }

        private void BindControls()
        {
            // Настройка анимации кнопок управления окном
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(232, 17, 35);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;

            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(60, 60, 60);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

            btnMaximize.Click += (s, e) => this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = Color.FromArgb(60, 60, 60);
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.Transparent;

            pnlHeader.MouseDown += (s, e) => { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); };

            this.Load += (s, e) => LoadData(_repo.GetAll());

            if (dgvTracks != null)
            {
                dgvTracks.ColumnHeaderMouseClick += (s, e) => {
                    string colName = dgvTracks.Columns[e.ColumnIndex].DataPropertyName;
                    if (string.IsNullOrEmpty(colName)) return;
                    _sortAscending = (_currentSortColumn == colName) ? !_sortAscending : true;
                    _currentSortColumn = colName;

                    var sorted = _sortAscending
                        ? _currentViewList.OrderBy(t => t.GetType().GetProperty(colName == "DurationFormatted" ? "Duration" : colName).GetValue(t, null)).ToList()
                        : _currentViewList.OrderByDescending(t => t.GetType().GetProperty(colName == "DurationFormatted" ? "Duration" : colName).GetValue(t, null)).ToList();

                    LoadData(sorted, false);
                };

                // ФОРМАТИРОВАНИЕ НУМЕРАЦИИ И ЗВЕЗДОЧКИ
                dgvTracks.CellFormatting += (s, e) => {
                    if (dgvTracks.Columns[e.ColumnIndex].Name == "TrackNum")
                    {
                        Track rowTrack = (Track)dgvTracks.Rows[e.RowIndex].DataBoundItem;
                        string prefix = _favorites.Contains(rowTrack) ? "★ " : "  ";
                        e.Value = prefix + (e.RowIndex + 1).ToString("D2");

                        if (_favorites.Contains(rowTrack))
                            e.CellStyle.ForeColor = Color.FromArgb(30, 215, 96); // Подсвечиваем номер зеленым
                        else
                            e.CellStyle.ForeColor = Color.Gray;
                    }
                };
            }

            this.MouseWheel += (s, e) => {
                if (dgvTracks == null || dgvTracks.RowCount == 0) return;
                int cur = dgvTracks.FirstDisplayedScrollingRowIndex;
                if (e.Delta > 0) dgvTracks.FirstDisplayedScrollingRowIndex = Math.Max(0, cur - 3);
                else if (cur >= 0) dgvTracks.FirstDisplayedScrollingRowIndex = Math.Min(dgvTracks.RowCount - 1, cur + 3);
                _customScrollbar?.UpdateScroll();
            };
        }

        private void LoadData(List<Track> data, bool resetSort = true)
        {
            if (dgvTracks == null) return;

            _currentViewList = data;
            dgvTracks.DataSource = null;
            dgvTracks.DataSource = _currentViewList;

            if (!dgvTracks.Columns.Contains("TrackNum"))
            {
                var numCol = new DataGridViewTextBoxColumn { Name = "TrackNum", HeaderText = "", Width = 50, MinimumWidth = 50 };
                numCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgvTracks.Columns.Insert(0, numCol);
            }

            if (dgvTracks.Columns["Id"] != null) dgvTracks.Columns["Id"].Visible = false;
            if (dgvTracks.Columns["Duration"] != null) dgvTracks.Columns["Duration"].Visible = false;

            var headers = new Dictionary<string, string> {
                {"Title", "НАЗВАНИЕ"}, {"Artist", "ИСПОЛНИТЕЛЬ"}, {"Genre", "ЖАНР"},
                {"Year", "ГОД"}, {"Rating", "РЕЙТИНГ"}, {"DurationFormatted", "ДЛИТЕЛЬНОСТЬ"}
            };

            foreach (var h in headers)
            {
                if (dgvTracks.Columns[h.Key] != null)
                {
                    string text = h.Value;
                    if (h.Key == _currentSortColumn) text += _sortAscending ? "  ▲" : "  ▼";
                    dgvTracks.Columns[h.Key].HeaderText = text;
                    dgvTracks.Columns[h.Key].HeaderCell.Style.ForeColor = (h.Key == _currentSortColumn) ? Color.White : Color.Gray;
                }
            }
            dgvTracks.ClearSelection();
            _customScrollbar?.UpdateScroll();
        }

        private void CreateSidebarButton(string text, int yPos, Action onClick, bool isBottom = false)
        {
            Button btn = new Button { Text = "   " + text, ForeColor = Color.Gray, BackColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Width = pnlSidebar.Width, Height = 45, Location = new Point(0, yPos), Cursor = Cursors.Hand };
            if (isBottom) btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.Gray;
            btn.Click += (s, e) => onClick();
            pnlSidebar.Controls.Add(btn);
        }

        private void ShowHelpDialog()
        {
            Form f = new Form { Size = new Size(550, 450), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.None, BackColor = Color.FromArgb(25, 25, 25) };
            Label t = new Label { Text = "РУКОВОДСТВО TUNEFLOW", Dock = DockStyle.Top, Height = 60, ForeColor = Color.FromArgb(30, 215, 96), Font = new Font("Segoe UI", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            string helpText = "ДОБРО ПОЖАЛОВАТЬ В СИСТЕМУ TUNEFLOW!\n\n" +
                              "1. НАВИГАЦИЯ: Используйте левую панель для переключения между общим списком и вашим плейлистом.\n\n" +
                              "2. ПОИСК: Введите текст в верхнее поле. Поиск работает динамически по текущему списку.\n\n" +
                              "3. УПРАВЛЕНИЕ: Нажмите ПРАВОЙ КНОПКОЙ мыши на трек, чтобы добавить его в избранное или найти похожие композиции.\n\n" +
                              "4. РЕКОМЕНДАЦИИ: Алгоритм подберет 10 лучших треков того же жанра на основе рейтинга и года выпуска.\n\n" +
                              "5. ИЗБРАННОЕ: Треки из вашего плейлиста отмечены символом ★ в основной таблице.\n\n" +
                              "6. СОРТИРОВКА: Кликните по заголовку любого столбца.";

            Label i = new Label { Text = helpText, ForeColor = Color.White, Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill, Padding = new Padding(25), TextAlign = ContentAlignment.TopLeft };
            Button b = new Button { Text = "ПОНЯТНО", Dock = DockStyle.Bottom, Height = 55, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(40, 40, 40) };
            b.Click += (s, e) => f.Close();
            f.Controls.Add(i); f.Controls.Add(t); f.Controls.Add(b); f.ShowDialog();
        }

        private void ShowAboutDialog()
        {
            Form f = new Form { Size = new Size(450, 320), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.None, BackColor = Color.FromArgb(20, 20, 20) };
            Label l1 = new Label { Text = "TUNEFLOW", Dock = DockStyle.Top, Height = 90, ForeColor = Color.FromArgb(30, 215, 96), Font = new Font("Segoe UI", 26, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            string about = "ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ TUNEFLOW\n" +
                           "Версия: 1.0.8 (Stable Build)\n\n" +
                           "Разработано в рамках курса ООП.\n" +
                           "Технологии: .NET 8.0, WinForms, LINQ.\n\n" +
                           "© 2025 Академический проект.\nВсе права защищены.";

            Label l2 = new Label { Text = about, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Button b = new Button { Text = "ЗАКРЫТЬ", Dock = DockStyle.Bottom, Height = 55, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(30, 30, 30) };
            b.Click += (s, e) => f.Close();
            f.Controls.Add(l2); f.Controls.Add(l1); f.Controls.Add(b); f.ShowDialog();
        }

        private void StyleWindowBtn(Button btn, string text)
        {
            btn.Text = text; btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0; btn.ForeColor = Color.White; btn.Cursor = Cursors.Hand;
        }
    }
}