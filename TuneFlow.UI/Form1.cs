using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TuneFlow.Logic;

namespace TuneFlow.UI
{
    /// <summary>
    /// Главная форма приложения. Предоставляет пользовательский интерфейс 
    /// для взаимодействия с рекомендательной музыкальной системой.
    /// Выполнена в современном безрамочном стиле (Dark Theme).
    /// </summary>
    public partial class Form1 : Form
    {
        #region Поля класса

        /// <summary>Экземпляр репозитория для работы с коллекцией музыкальных треков.</summary>
        private TrackRepository _repo;

        /// <summary>Текстовое поле для осуществления динамического поиска.</summary>
        private TextBox txtSearch;

        /// <summary>Кастомный элемент управления полосой прокрутки.</summary>
        private CustomScrollbar _customScrollbar;

        /// <summary>Флаг, определяющий направление текущей сортировки (по возрастанию/убыванию).</summary>
        private bool _sortAscending = true;

        /// <summary>Название колонки, по которой в данный момент отсортирована таблица.</summary>
        private string _currentSortColumn = "";

        #endregion

        #region Импорт функций Windows API

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Form1"/>.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            _repo = new TrackRepository();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            int isDarkMode = 1;
            DwmSetWindowAttribute(this.Handle, 20, ref isDarkMode, sizeof(int));
            DwmSetWindowAttribute(this.Handle, 19, ref isDarkMode, sizeof(int));

            ApplyTheme();
            CreateDynamicUI();
            BindControls();
        }

        /// <summary>
        /// Применяет заданную цветовую схему ко всем элементам управления формы.
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
                dgvTracks.DefaultCellStyle.ForeColor = Color.FromArgb(240, 240, 240);
                dgvTracks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 45);
                dgvTracks.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                dgvTracks.RowTemplate.Height = 40;

                dgvTracks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvTracks.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
                dgvTracks.ColumnHeadersDefaultCellStyle.BackColor = darkBg;
                dgvTracks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvTracks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgvTracks.ColumnHeadersDefaultCellStyle.SelectionBackColor = darkBg;
                dgvTracks.ColumnHeadersHeight = 45;
                dgvTracks.Cursor = Cursors.Hand;

                dgvTracks.MouseEnter += (s, e) => dgvTracks.Focus();
            }

            StyleWindowBtn(btnClose, "✕");
            StyleWindowBtn(btnMaximize, "▢");
            StyleWindowBtn(btnMinimize, "—");
        }

        private void StyleWindowBtn(Button btn, string text)
        {
            if (btn == null) return;
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10f);
            btn.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Динамически генерирует строку поиска, лейбл и меню навигации.
        /// </summary>
        private void CreateDynamicUI()
        {
            if (pnlHeader != null)
            {
                // Определение параметров геометрии для центрирования поискового блока
                int searchBoxWidth = 400;
                int searchBoxX = (pnlHeader.Width - searchBoxWidth) / 2;

                // 1. Инициализация пояснительной надписи для поля поиска
                Label lblSearch = new Label
                {
                    Text = "Поиск:",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top,
                    Location = new Point(searchBoxX - 70, 10)
                };
                pnlHeader.Controls.Add(lblSearch);

                // 2. Инициализация и настройка текстового поля ввода поискового запроса
                txtSearch = new TextBox
                {
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    Width = searchBoxWidth,
                    Location = new Point(searchBoxX, 8),
                    Anchor = AnchorStyles.Top,
                    Text = " Введите название трека, имя артиста или жанр..."
                };

                // Реализация логики интерактивного текстового заполнителя (Placeholder)
                txtSearch.Enter += (s, e) => { if (txtSearch.Text == " Введите название трека, имя артиста или жанр...") txtSearch.Text = ""; };
                txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = " Введите название трека, имя артиста или жанр..."; };

                // Подписка на событие изменения текста для реализации "живого" поиска
                txtSearch.TextChanged += (s, e) => {
                    if (txtSearch.Text != " Введите название трека, имя артиста или жанр...")
                    {
                        LoadData(_repo.Search(txtSearch.Text));
                        _customScrollbar?.UpdateScroll();
                    }
                };
                pnlHeader.Controls.Add(txtSearch);
            }

            // Инициализация навигационного меню в боковой панели
            if (pnlSidebar != null)
            {
                /* 
                 * Кнопка "Главная":
                 * Выполняет полный сброс состояния фильтрации и сортировки приложения.
                 * Устанавливает исходный порядок отображения записей (по возрастанию индекса Id).
                 */
                CreateSidebarButton("Главная", 20, () => {
                    // Очистка параметров поиска
                    txtSearch.Text = " Введите название трека, имя артиста или жанр...";

                    // Сброс параметров сортировки к исходному состоянию (по идентификатору Id)
                    _currentSortColumn = "Id";
                    _sortAscending = true;

                    // Обновление представления данных из репозитория
                    LoadData(_repo.Sort("Id", true));
                    _customScrollbar?.UpdateScroll();
                });

                // Кнопка "О приложении": вызов информационного диалогового окна
                CreateSidebarButton("О приложении", 65, ShowAboutDialog);
            }

            // Инстанцирование и интеграция кастомного компонента полосы прокрутки
            if (dgvTracks != null)
            {
                _customScrollbar = new CustomScrollbar();
                _customScrollbar.Dock = DockStyle.Right;

                // Добавление в иерархию элементов управления родительского контейнера таблицы
                dgvTracks.Parent.Controls.Add(_customScrollbar);
                _customScrollbar.BringToFront();

                // Ассоциация с целевым объектом DataGridView
                _customScrollbar.Grid = dgvTracks;
            }
        }

        private void CreateSidebarButton(string text, int yPos, Action onClick)
        {
            Button btn = new Button
            {
                Text = "   " + text,
                ForeColor = Color.Gray,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Width = pnlSidebar.Width,
                Height = 45,
                Location = new Point(0, yPos),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 30, 30);

            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.Gray;
            btn.Click += (s, e) => onClick();

            pnlSidebar.Controls.Add(btn);
        }

        private void ShowAboutDialog()
        {
            Form aboutForm = new Form
            {
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 30),
            };

            Label text = new Label
            {
                Text = "TUNEFLOW\nРекомендательная система музыкальных треков.\n\nРазработано в рамках курсового проекта.\nВерсия 1.0.0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnOk = new Button
            {
                Text = "ЗАКРЫТЬ",
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(30, 215, 96),
                BackColor = Color.FromArgb(20, 20, 20),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) => aboutForm.Close();

            aboutForm.Controls.Add(text);
            aboutForm.Controls.Add(btnOk);
            aboutForm.ShowDialog();
        }

        private void BindControls()
        {
            if (btnClose != null)
            {
                btnClose.Click += (s, e) => Application.Exit();
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(232, 17, 35);
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
                btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 0, 0);
            }
            if (btnMinimize != null)
            {
                btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
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

            if (pnlHeader != null)
            {
                pnlHeader.MouseDown += (s, e) => {
                    ReleaseCapture();
                    SendMessage(this.Handle, 0x112, 0xf012, 0);
                };
            }

            this.Load += (s, e) => {
                LoadData(_repo.GetAll());
                _customScrollbar?.UpdateScroll();
            };

            if (dgvTracks != null)
            {
                dgvTracks.Resize += (s, e) => _customScrollbar?.UpdateScroll();
                dgvTracks.SelectionChanged += (s, e) => _customScrollbar?.UpdateScroll();

                // Динамическая нумерация строк
                dgvTracks.CellFormatting += (s, e) =>
                {
                    if (dgvTracks.Columns[e.ColumnIndex].Name == "TrackNum")
                    {
                        e.Value = (e.RowIndex + 1).ToString(); // Всегда 1, 2, 3...
                    }
                };

                dgvTracks.ColumnHeaderMouseClick += (s, e) =>
                {
                    string colName = dgvTracks.Columns[e.ColumnIndex].DataPropertyName;
                    if (string.IsNullOrEmpty(colName)) return; // Игнорируем клик по колонке с номерами

                    if (_currentSortColumn == colName) _sortAscending = !_sortAscending;
                    else { _currentSortColumn = colName; _sortAscending = true; }

                    LoadData(_repo.Sort(colName, _sortAscending));
                    _customScrollbar?.UpdateScroll();
                };
            }

            this.MouseWheel += (s, e) =>
            {
                if (dgvTracks == null || dgvTracks.RowCount == 0) return;

                int scrollStep = 3;
                int currentIndex = dgvTracks.FirstDisplayedScrollingRowIndex;

                if (e.Delta > 0)
                    dgvTracks.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollStep);
                else if (e.Delta < 0)
                    dgvTracks.FirstDisplayedScrollingRowIndex = Math.Min(dgvTracks.RowCount - 1, currentIndex + scrollStep);

                _customScrollbar?.UpdateScroll();
            };
        }

        /// <summary>
        /// Выполняет привязку данных к таблице, настраивает столбцы и добавляет индикаторы сортировки (стрелки).
        /// </summary>
        /// <param name="dataSource">Коллекция треков для отображения.</param>
        private void LoadData(object dataSource)
        {
            if (dgvTracks == null) return;

            // Назначение источника данных
            dgvTracks.DataSource = dataSource;

            // 1. Инициализация и настройка столбца нумерации (визуальный индекс)
            if (!dgvTracks.Columns.Contains("TrackNum"))
            {
                var numCol = new DataGridViewTextBoxColumn
                {
                    Name = "TrackNum",
                    HeaderText = "",
                    ReadOnly = true,
                    FillWeight = 5 // Минимальная ширина для порядкового номера
                };
                numCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                numCol.DefaultCellStyle.ForeColor = Color.Gray;
                dgvTracks.Columns.Insert(0, numCol);
            }

            // 2. Скрытие технических и служебных столбцов (не предназначенных для пользователя)
            // Скрываем Id, так как он используется только для внутренней логики сортировки
            if (dgvTracks.Columns["Id"] != null) dgvTracks.Columns["Id"].Visible = false;
            // Скрываем длительность в секундах, заменяя её форматированной строкой
            if (dgvTracks.Columns["Duration"] != null) dgvTracks.Columns["Duration"].Visible = false;

            // 3. Локализация интерфейса: установка кириллических заголовков столбцов
            var headers = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Title", "НАЗВАНИЕ" },
                { "Artist", "ИСПОЛНИТЕЛЬ" },
                { "Genre", "ЖАНР" },
                { "Year", "ГОД" },
                { "Rating", "РЕЙТИНГ" },
                { "DurationFormatted", "ДЛИТЕЛЬНОСТЬ" }
            };

            foreach (var header in headers)
            {
                if (dgvTracks.Columns[header.Key] != null)
                {
                    string finalHeader = header.Value;

                    // Индикация активной сортировки посредством добавления глифов ▲ / ▼
                    if (header.Key == _currentSortColumn)
                    {
                        finalHeader += _sortAscending ? "  ▲" : "  ▼";
                        dgvTracks.Columns[header.Key].HeaderCell.Style.ForeColor = Color.White;
                    }
                    else
                    {
                        dgvTracks.Columns[header.Key].HeaderCell.Style.ForeColor = Color.Gray;
                    }

                    dgvTracks.Columns[header.Key].HeaderText = finalHeader;
                }
            }

            // 4. Оптимизация распределения экранного пространства (FillWeight)
            if (dgvTracks.Columns["Title"] != null) dgvTracks.Columns["Title"].FillWeight = 30;
            if (dgvTracks.Columns["Artist"] != null) dgvTracks.Columns["Artist"].FillWeight = 25;
            if (dgvTracks.Columns["Genre"] != null) dgvTracks.Columns["Genre"].FillWeight = 15;
            if (dgvTracks.Columns["Year"] != null) dgvTracks.Columns["Year"].FillWeight = 10;
            if (dgvTracks.Columns["Rating"] != null) dgvTracks.Columns["Rating"].FillWeight = 10;
            if (dgvTracks.Columns["DurationFormatted"] != null) dgvTracks.Columns["DurationFormatted"].FillWeight = 10;

            // Сброс выделения для предотвращения визуального акцента на первой строке при загрузке
            dgvTracks.ClearSelection();
        }
    }
}