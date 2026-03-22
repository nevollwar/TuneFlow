using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TuneFlow.Logic;

namespace TuneFlow.UI
{
    /// <summary>
    /// Главный модуль управления интерфейсом TuneFlow (Build 2026).
    /// Обеспечивает расширенную визуализацию метаданных музыкальной библиотеки,
    /// включая таблицу треков, аналитический дашборд, плейлист и поиск.
    /// </summary>
    public partial class Form1 : Form
    {
        #region Состояние и логика

        /// <summary>Репозиторий треков — основной источник данных для всех операций.</summary>
        private TrackRepository _repo;

        /// <summary>Список треков, добавленных пользователем в избранный плейлист.</summary>
        private List<Track> _favorites = new List<Track>();

        /// <summary>Текущий отображаемый список треков (зависит от активного режима — главная или плейлист).</summary>
        private List<Track> _currentViewList = new List<Track>();

        /// <summary>Флаг: true — отображается режим «Мой плейлист», false — главная страница библиотеки.</summary>
        private bool _isPlaylistView = false;

        /// <summary>
        /// Флаг переключения режима нижнего линейного графика на дашборде.
        /// true — отображать средний рейтинг по годам, false — количество треков по годам.
        /// </summary>
        private bool _statModeIsRating = false;

        /// <summary>Поле ввода для поиска треков по названию, исполнителю или жанру.</summary>
        private TextBox txtSearch;

        /// <summary>Кастомный скроллбар, заменяющий стандартный для таблицы треков.</summary>
        private CustomScrollbar _customScrollbar;

        /// <summary>Текущее направление сортировки: true — по возрастанию, false — по убыванию.</summary>
        private bool _sortAscending = true;

        /// <summary>Имя свойства (DataPropertyName), по которому в данный момент отсортирована таблица.</summary>
        private string _currentSortColumn = "Id";

        #endregion

        #region Секция WinAPI

        /// <summary>
        /// Освобождает захват мыши, чтобы разрешить системе перетаскивать окно.
        /// Используется совместно с SendMessage для реализации drag-move на безрамочном окне.
        /// </summary>
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        /// <summary>
        /// Отправляет системное сообщение окну.
        /// Вызывается с параметром WM_SYSCOMMAND (0x112) + SC_MOVE (0xF012)
        /// для имитации перетаскивания заголовка окна.
        /// </summary>
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        /// <summary>
        /// Позволяет установить атрибуты окна Desktop Window Manager (DWM),
        /// в частности — включить тёмный режим оформления заголовка (атрибут 20 = DWMWA_USE_IMMERSIVE_DARK_MODE).
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        #endregion

        /// <summary>
        /// Инициализирует форму: применяет тему, создаёт динамический UI и привязывает обработчики событий.
        /// </summary>
        public Form1()
        {
            // Инициализация базовых компонентов WinForms
            InitializeComponent();

            // Инициализация логического ядра
            _repo = new TrackRepository();

            // Конфигурация параметров бесшовного окна
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // Активация программного Dark Mode
            int isDarkMode = 1;
            DwmSetWindowAttribute(this.Handle, 20, ref isDarkMode, sizeof(int));

            // Запуск СРАЗУ в полноэкранном режиме
            this.WindowState = FormWindowState.Maximized;

            // Построение интерфейса
            ApplyTheme();
            CreateDynamicUI();
            BindControls();

            // КРИТИЧЕСКИЙ ФИКС: Подписка на изменение размера для синхронизации иконки кнопки
            this.Resize += (s, e) => {
                if (btnMaximize != null)
                    btnMaximize.Text = (this.WindowState == FormWindowState.Maximized) ? "❐" : "▢";
            };
        }

        /// <summary>
        /// Применяет визуальный стиль Spotify Dark Theme ко всем элементам формы.
        /// Настраивает цвета фона, шрифты, стили заголовков таблицы и кнопок управления окном.
        /// </summary>
        private void ApplyTheme()
        {
            // Базовый тёмный цвет фона всего приложения
            Color darkBg = Color.FromArgb(18, 18, 18);
            this.BackColor = darkBg;

            // Секция шапки — самый тёмный оттенок для разделения от контента
            if (pnlHeader != null) pnlHeader.BackColor = Color.FromArgb(10, 10, 10);

            // Боковая панель навигации — чисто чёрная для контраста
            if (pnlSidebar != null) pnlSidebar.BackColor = Color.Black;

            // Нижняя панель плеера — чуть светлее основного фона
            if (pnlPlayer != null) pnlPlayer.BackColor = Color.FromArgb(24, 24, 24);

            // Логотип — фирменный зелёный цвет Spotify, жирный шрифт
            if (lblLogo != null)
            {
                lblLogo.Text = "  TUNEFLOW";
                lblLogo.ForeColor = Color.FromArgb(30, 215, 96); // Фирменный зелёный
                lblLogo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }

            // Стилизация DataGridView под тёмную тему
            if (dgvTracks != null)
            {
                dgvTracks.BackgroundColor = darkBg;
                dgvTracks.BorderStyle = BorderStyle.None;
                dgvTracks.ReadOnly = true; // Запрещаем inline-редактирование ячеек
                dgvTracks.AllowUserToResizeColumns = false; // Фиксируем ширину колонок
                dgvTracks.AllowUserToResizeRows = false;
                dgvTracks.EnableHeadersVisualStyles = false; // Отключаем системный стиль заголовков — иначе тема не применится
                dgvTracks.RowHeadersVisible = false; // Скрываем левую колонку с индексами строк
                dgvTracks.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделяем всю строку
                dgvTracks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Колонки растягиваются на всю ширину
                dgvTracks.ScrollBars = ScrollBars.None; // Скроллбар заменён кастомным виджетом

                // Цвет линий сетки — едва заметные горизонтальные разделители
                dgvTracks.GridColor = Color.FromArgb(35, 35, 35);
                dgvTracks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

                // Стиль обычных ячеек
                dgvTracks.DefaultCellStyle.BackColor = darkBg;
                dgvTracks.DefaultCellStyle.ForeColor = Color.FromArgb(220, 220, 220);
                dgvTracks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 45); // Подсветка выделенной строки
                dgvTracks.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                dgvTracks.RowTemplate.Height = 40; // Увеличенная высота строк для удобства чтения

                // Стиль заголовков колонок
                dgvTracks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvTracks.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
                dgvTracks.ColumnHeadersDefaultCellStyle.BackColor = darkBg;
                dgvTracks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvTracks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgvTracks.ColumnHeadersHeight = 45;

                // Автоматически захватываем фокус при наведении мыши — для работы колесика прокрутки
                dgvTracks.MouseEnter += (s, e) => dgvTracks.Focus();

                // Открываем контекстное меню по правой кнопке мыши
                dgvTracks.MouseDown += (s, e) => {
                    if (e.Button == MouseButtons.Right) CreateDynamicContextMenu(e.Location);
                };
            }

            // Применяем единый стиль к трём кнопкам управления окном (закрыть, развернуть, свернуть)
            StyleWindowBtn(btnClose, "✕");
            StyleWindowBtn(btnMaximize, "▢");
            StyleWindowBtn(btnMaximize, this.WindowState == FormWindowState.Maximized ? "❐" : "▢");
        }

        /// <summary>
        /// Создаёт и отображает контекстное меню при правом клике на строке таблицы треков.
        /// Набор пунктов зависит от текущего режима (главная / плейлист).
        /// </summary>
        /// <param name="location">Координаты клика мышью относительно таблицы.</param>
        private void CreateDynamicContextMenu(Point location)
        {
            // Определяем, на какую ячейку/строку нажали
            var hit = dgvTracks.HitTest(location.X, location.Y);

            // Если клик вне строк данных — ничего не делаем
            if (hit.RowIndex < 0) return;

            // Выделяем строку под курсором
            dgvTracks.ClearSelection();
            dgvTracks.Rows[hit.RowIndex].Selected = true;

            // Создаём стилизованное контекстное меню в тёмной теме
            ContextMenuStrip menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                ShowImageMargin = false // Убираем пустое место под иконки
            };

            // Получаем трек, связанный с данной строкой через DataBinding
            Track selected = (Track)dgvTracks.Rows[hit.RowIndex].DataBoundItem;

            if (_isPlaylistView)
            {
                // В режиме плейлиста — предлагаем удалить трек из избранного
                var remove = menu.Items.Add("Удалить из плейлиста");
                remove.Click += (s, e) => {
                    _favorites.Remove(selected);
                    LoadData(_favorites, false); // Перезагружаем список без сброса сортировки
                };
            }
            else
            {
                // На главной странице — предлагаем добавить трек в плейлист
                var add = menu.Items.Add("Добавить в избранное");
                add.Click += (s, e) => {
                    // Проверяем дубликаты перед добавлением
                    if (!_favorites.Contains(selected))
                    {
                        _favorites.Add(selected);
                        dgvTracks.Invalidate(); // Перерисовываем таблицу, чтобы обновить иконку ★
                    }
                };
            }

            // Пункт рекомендаций доступен в обоих режимах
            var rec = menu.Items.Add("Показать похожие композиции");
            rec.Click += (s, e) => {
                _isPlaylistView = false; // Переключаемся на главный вид
                txtSearch.Text = $" Похожее на: {selected.Title}"; // Визуально отображаем контекст поиска
                LoadData(_repo.GetRecommendations(selected)); // Загружаем похожие треки из репозитория
            };

            menu.Show(dgvTracks, location);
        }

        /// <summary>
        /// Создаёт динамические элементы интерфейса, которые не были добавлены через дизайнер форм:
        /// строку поиска в шапке, кнопки навигации в сайдбаре и кастомный скроллбар.
        /// </summary>
        private void CreateDynamicUI()
        {
            if (pnlHeader != null)
            {
                // Вычисляем позицию поля поиска, чтобы оно располагалось по центру шапки
                int sw = 420; // Ширина поля поиска
                int sx = (pnlHeader.Width - sw) / 2; // X-координата для центрирования

                // Подпись к полю поиска
                Label lblSearch = new Label
                {
                    Text = "Поиск:",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top,
                    Location = new Point(sx - 70, 10)
                };
                pnlHeader.Controls.Add(lblSearch);

                // Само текстовое поле поиска
                txtSearch = new TextBox
                {
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    Width = sw,
                    Location = new Point(sx, 8),
                    Anchor = AnchorStyles.Top,
                    Text = " Поиск трека, артиста или жанра..." // Placeholder-текст
                };

                // Очищаем placeholder при фокусировке
                txtSearch.Enter += (s, e) => {
                    if (txtSearch.Text.Contains("Поиск")) txtSearch.Text = "";
                };

                // Восстанавливаем placeholder, если поле пустое при потере фокуса
                txtSearch.Leave += (s, e) => {
                    if (string.IsNullOrWhiteSpace(txtSearch.Text))
                        txtSearch.Text = " Поиск трека, артиста или жанра...";
                };

                // Фильтрация треков в реальном времени при каждом нажатии клавиши
                txtSearch.TextChanged += (s, e) => {
                    if (!txtSearch.Text.Contains("Поиск"))
                    {
                        string q = txtSearch.Text.ToLower();

                        // Источник фильтрации зависит от текущего режима отображения
                        List<Track> baseSource = _isPlaylistView ? _favorites : _repo.GetAll();

                        // Фильтруем по названию, исполнителю и жанру одновременно
                        var filtered = baseSource.Where(t =>
                            t.Title.ToLower().Contains(q) ||
                            t.Artist.ToLower().Contains(q) ||
                            t.Genre.ToLower().Contains(q)
                        ).ToList();

                        // Отображаем результат без сброса текущей сортировки
                        LoadData(filtered, false);
                    }
                };

                pnlHeader.Controls.Add(txtSearch);
            }

            if (pnlSidebar != null)
            {
                // Главная — отображает всю библиотеку треков
                CreateSidebarButton("Главная", 20, () => {
                    _isPlaylistView = false;
                    txtSearch.Text = " Поиск трека, артиста или жанра...";
                    LoadData(_repo.GetAll());
                });

                // Мой плейлист — отображает только избранные треки
                CreateSidebarButton("Мой плейлист", 65, () => {
                    _isPlaylistView = true;
                    txtSearch.Text = " Поиск по вашему плейлисту...";
                    LoadData(_favorites);
                });

                // Статистика — открывает полноэкранный аналитический дашборд
                CreateSidebarButton("Статистика", 110, ShowStatisticsDialog);

                // Вспомогательные кнопки располагаются внизу сайдбара (bot = true)
                CreateSidebarButton("Помощь", pnlSidebar.Height - 110, ShowHelpDialog, true);
                CreateSidebarButton("О приложении", pnlSidebar.Height - 65, ShowAboutDialog, true);
            }

            if (dgvTracks != null)
            {
                // Добавляем кастомный скроллбар справа от таблицы
                _customScrollbar = new CustomScrollbar
                {
                    Dock = DockStyle.Right,
                    Grid = dgvTracks // Передаём ссылку на таблицу, которой скроллбар управляет
                };
                dgvTracks.Parent.Controls.Add(_customScrollbar);

                // Кастомный скроллбар должен перекрывать другие элементы (находиться поверх таблицы)
                _customScrollbar.BringToFront();
            }
        }

        /// <summary>
        /// Привязывает обработчики событий к элементам формы: кнопки окна,
        /// перетаскивание, колёсико мыши, сортировка по заголовкам и форматирование ячеек.
        /// </summary>
        private void BindControls()
        {
            // Кнопки управления окном
            btnClose.Click += (s, e) => Application.Exit();
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Переключение между развёрнутым и нормальным состоянием окна
            btnMaximize.Click += (s, e) =>
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;

            // Подсветка кнопки закрытия красным при наведении (стандартное поведение Windows)
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(232, 17, 35);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;

            // Drag-to-move: перетаскивание безрамочного окна за шапку
            pnlHeader.MouseDown += (s, e) => {
                ReleaseCapture();                              // Снимаем захват мыши
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            };

            // Загружаем данные при открытии формы
            this.Load += (s, e) => LoadData(_repo.GetAll());

            if (dgvTracks != null)
            {
                // Сортировка по клику на заголовок колонки 
                dgvTracks.ColumnHeaderMouseClick += (s, e) => {
                    // Получаем имя свойства модели, привязанного к этой колонке
                    string colName = dgvTracks.Columns[e.ColumnIndex].DataPropertyName;
                    if (string.IsNullOrEmpty(colName)) return; // Колонка без биндинга (например, TrackNum) — игнорируем

                    // Если кликнули на уже активную колонку — инвертируем направление, иначе — по возрастанию
                    _sortAscending = (_currentSortColumn == colName) ? !_sortAscending : true;
                    _currentSortColumn = colName;

                    // DurationFormatted — отображаемое свойство, сортировать нужно по числовому Duration
                    var sorted = _sortAscending
                        ? _currentViewList.OrderBy(t =>
                            t.GetType().GetProperty(colName == "DurationFormatted" ? "Duration" : colName).GetValue(t, null)).ToList()
                        : _currentViewList.OrderByDescending(t =>
                            t.GetType().GetProperty(colName == "DurationFormatted" ? "Duration" : colName).GetValue(t, null)).ToList();

                    // Обновляем таблицу с новым порядком, не сбрасывая состояние сортировки
                    LoadData(sorted, false);
                };

                // Форматирование первой колонки: номер строки + признак избранного
                dgvTracks.CellFormatting += (s, e) => {
                    if (dgvTracks.Columns[e.ColumnIndex].Name == "TrackNum")
                    {
                        Track rowTrack = (Track)dgvTracks.Rows[e.RowIndex].DataBoundItem;
                        bool isFav = _favorites.Contains(rowTrack);

                        // Формат: «★ 01» для избранного или «  01» для обычного трека
                        e.Value = (isFav ? "★ " : "  ") + (e.RowIndex + 1).ToString("D2");

                        // Зелёный цвет для избранных треков, серый — для остальных
                        e.CellStyle.ForeColor = isFav ? Color.FromArgb(30, 215, 96) : Color.Gray;
                    }
                };
            }

            // Прокрутка колёсиком мыши: перемещение на 3 строки за раз
            this.MouseWheel += (s, e) => {
                if (dgvTracks == null || dgvTracks.RowCount == 0) return;

                int cur = dgvTracks.FirstDisplayedScrollingRowIndex;
                if (e.Delta > 0)
                    // Прокрутка вверх — уменьшаем индекс первой видимой строки
                    dgvTracks.FirstDisplayedScrollingRowIndex = Math.Max(0, cur - 3);
                else if (cur >= 0)
                    // Прокрутка вниз — увеличиваем, не выходя за границу
                    dgvTracks.FirstDisplayedScrollingRowIndex = Math.Min(dgvTracks.RowCount - 1, cur + 3);

                // Синхронизируем позицию кастомного скроллбара с новой позицией таблицы
                _customScrollbar?.UpdateScroll();
            };
        }

        /// <summary>
        /// Загружает список треков в таблицу и обновляет заголовки колонок с учётом сортировки.
        /// </summary>
        /// <param name="data">Список треков для отображения.</param>
        /// <param name="resetSort">
        /// Если true — сбрасывает индикаторы сортировки в заголовках.
        /// Передавайте false при перезагрузке уже отсортированных данных.
        /// </param>
        private void LoadData(List<Track> data, bool resetSort = true)
        {
            if (dgvTracks == null) return;

            // Сохраняем текущий список для последующей сортировки и фильтрации
            _currentViewList = data;

            // Сбрасываем и переустанавливаем DataSource — единственный надёжный способ
            // обновить DataGridView при изменении коллекции без ObservableCollection
            dgvTracks.DataSource = null;
            dgvTracks.DataSource = _currentViewList;

            // Добавляем колонку с номером строки/индикатором избранного, если её ещё нет
            if (!dgvTracks.Columns.Contains("TrackNum"))
            {
                var numCol = new DataGridViewTextBoxColumn
                {
                    Name = "TrackNum",
                    HeaderText = "",
                    Width = 50,
                    MinimumWidth = 50
                };
                dgvTracks.Columns.Insert(0, numCol); // Вставляем первой колонкой
            }

            // Скрываем служебные поля, которые не нужны пользователю в таблице
            if (dgvTracks.Columns["Id"] != null) dgvTracks.Columns["Id"].Visible = false;
            if (dgvTracks.Columns["Duration"] != null) dgvTracks.Columns["Duration"].Visible = false; // Показываем DurationFormatted

            // Маппинг: DataPropertyName -> отображаемое название заголовка
            var headers = new Dictionary<string, string> {
                {"Title", "НАЗВАНИЕ"},
                {"Artist","ИСПОЛНИТЕЛЬ"},
                {"Genre", "ЖАНР"},
                {"Year", "ГОД"},
                {"Rating", "РЕЙТИНГ"},
                {"DurationFormatted","ДЛИТЕЛЬНОСТЬ"}
            };

            // Применяем заголовки и добавляем визуальный индикатор активной сортировки (▲ / ▼)
            foreach (var h in headers)
            {
                if (dgvTracks.Columns[h.Key] != null)
                {
                    string text = h.Value;
                    if (h.Key == _currentSortColumn)
                        text += _sortAscending ? "  ▲" : "  ▼"; // Показываем направление сортировки

                    dgvTracks.Columns[h.Key].HeaderText = text;

                    // Активная колонка сортировки выделяется белым, остальные — серым
                    dgvTracks.Columns[h.Key].HeaderCell.Style.ForeColor =
                        (h.Key == _currentSortColumn) ? Color.White : Color.Gray;
                }
            }

            // Снимаем выделение и обновляем позицию скроллбара
            dgvTracks.ClearSelection();
            _customScrollbar?.UpdateScroll();
        }

        #region Продвинутая Аналитика (Interactive Full Dashboard)

        /// <summary>
        /// Формирует и отображает полноэкранный аналитический дашборд.
        /// Включает: блок KPI-метрик библиотеки, гистограмму жанров и интерактивный линейный график по годам.
        /// </summary>
        private void ShowStatisticsDialog()
        {
            // Увеличиваем ширину окна до 1150px, чтобы все жанры (Рок и др.) влезли без обрезки
            Form f = new Form
            {
                Size = new Size(1150, 700),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            // Рисуем тонкую рамку вокруг безрамочного окна для обозначения его границ
            f.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, f.ClientRectangle, Color.FromArgb(45, 45, 45), ButtonBorderStyle.Solid);

            // Заголовок дашборда
            Label title = new Label
            {
                Text = "АНАЛИТИЧЕСКИЙ ДЭШБОРД TUNEFLOW SYSTEM",
                Dock = DockStyle.Top,
                Height = 80,
                ForeColor = Color.FromArgb(30, 215, 96),
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Расчёт агрегированных KPI-метрик
            var all = _repo.GetAll();
            double hours = Math.Round(all.Sum(t => t.Duration) / 3600.0, 1); // Общая длительность в часах

            // Левая панель с текстовыми метриками библиотеки
            Panel pnlStats = new Panel { Dock = DockStyle.Left, Width = 350, Padding = new Padding(35) };

            string infoText =
                $"МЕТРИКИ БИБЛИОТЕКИ:\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                $"Объем данных: {all.Count} треков\n" +
                $"Общий тайминг: {hours} ч.\n" +
                $"Временной охват: {all.Min(t => t.Year)} - {all.Max(t => t.Year)}\n\n" +
                $"ЛИДЕРЫ ПРОСЛУШИВАНИЙ:\n" +
                $"{string.Join("\n", _repo.GetTopArtists(3))}\n\n" + // Топ-3 исполнителя по количеству треков
                $"ОБЩИЙ РЕЙТИНГ СИСТЕМЫ:\n" +
                $"{Math.Round(all.Average(t => t.Rating), 2)} / 5.0\n\n" +
                $"СРЕДНЕЕ ВРЕМЯ ПО ЖАНРАМ:\n" +
                $"{string.Join("\n", _repo.GetAverageDurationByGenre().Take(4).Select(x => $"{x.Key}: {x.Value} м."))}"; // Первые 4 жанра

            Label lblMetrics = new Label
            {
                Text = infoText,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill
            };
            pnlStats.Controls.Add(lblMetrics);

            // Центральный контейнер для двух графиков (расположены вертикально)
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            // Верхний график занимает 45% высоты, нижний — 55%
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

            // Верхний график: гистограмма по жанрам
            PictureBox pbHist = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                Margin = new Padding(10)
            };
            // Делегируем рисование в отдельный метод при каждом событии Paint
            pbHist.Paint += (s, e) => DrawFullHistogram(e.Graphics, pbHist.Width, pbHist.Height);

            // Нижний график: линейный тренд по годам (переключаемый)
            PictureBox pbLine = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                Margin = new Padding(10)
            };
            pbLine.Paint += (s, e) => DrawAdvancedLineChart(e.Graphics, pbLine.Width, pbLine.Height);

            // Панель с кнопкой переключения режима нижнего графика
            FlowLayoutPanel pnlSwitch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15, 5, 0, 0)
            };

            Button btnToggle = new Button
            {
                Text = "СМЕНИТЬ РЕЖИМ ГРАФИКА: КОЛИЧЕСТВО / СРЕДНИЙ РЕЙТИНГ",
                Width = 500, // Широкая кнопка — текст длинный, иначе обрезается
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(30, 215, 96),
                BackColor = Color.FromArgb(35, 35, 35),
                Height = 35,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggle.FlatAppearance.BorderColor = Color.FromArgb(30, 215, 96);

            // При нажатии переключаем флаг и перерисовываем только нижний график
            btnToggle.Click += (s, e) => {
                _statModeIsRating = !_statModeIsRating;
                pbLine.Invalidate(); // Принудительно вызываем Paint для обновления графика
            };
            pnlSwitch.Controls.Add(btnToggle);

            // Размещаем оба графика в сетке
            tlp.Controls.Add(pbHist, 0, 0);
            tlp.Controls.Add(pbLine, 0, 1);

            // Кнопка закрытия дашборда
            Button bClose = new Button
            {
                Text = "ЗАКРЫТЬ ПАНЕЛЬ УПРАВЛЕНИЯ",
                Dock = DockStyle.Bottom,
                Height = 65,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            bClose.Click += (s, e) => f.Close();

            // Порядок добавления элементов важен: Dock определяет расположение
            f.Controls.Add(tlp);
            f.Controls.Add(pnlSwitch);
            f.Controls.Add(pnlStats);
            f.Controls.Add(title);
            f.Controls.Add(bClose);

            f.ShowDialog();
        }

        /// <summary>
        /// Отрисовывает гистограмму для ВСЕХ жанров библиотеки без каких-либо ограничений.
        /// Ширина столбцов рассчитывается автоматически исходя из количества жанров.
        /// Высота столбцов пропорциональна максимальному количеству треков в жанре.
        /// </summary>
        /// <param name="g">Графический контекст PictureBox.</param>
        /// <param name="w">Ширина области отрисовки в пикселях.</param>
        /// <param name="h">Высота области отрисовки в пикселях.</param>
        private void DrawFullHistogram(Graphics g, int w, int h)
        {
            // Группируем все треки по жанру и сортируем по убыванию числа треков
            var data = _repo.GetAll()
                .GroupBy(t => t.Genre)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());

            if (data.Count == 0) return; // Нет данных — ничего не рисуем

            int max = data.Values.Max(); // Максимальное значение для масштабирования высот
            int margin = 45;             // Отступ снизу и слева для подписей осей

            // Автоматически рассчитываем ширину столбца под количество жанров
            int bw = (w - margin * 2) / data.Count - 12;
            int x = margin; // Начальная X-позиция первого столбца

            // Включаем сглаживание для более чёткого текста и скруглений
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Заголовок гистограммы
            g.DrawString("ПОПУЛЯРНОСТЬ ЖАНРОВ (КОЛИЧЕСТВО ТРЕКОВ)",
                new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Gray, margin, 10);

            foreach (var item in data)
            {
                // Высота столбца пропорциональна доле от максимума
                int bh = (int)((float)item.Value / max * (h - margin * 2.8f));
                Rectangle r = new Rectangle(x, h - bh - margin, bw, bh);

                // Вертикальный градиент: яркий зелёный сверху, тёмный снизу
                using (var br = new LinearGradientBrush(r,
                    Color.FromArgb(30, 215, 96),  // Верхний цвет (светлый)
                    Color.FromArgb(10, 60, 30),   // Нижний цвет (тёмный)
                    90F))
                {
                    g.FillRectangle(br, r);
                }

                // Подпись жанра под столбцом
                g.DrawString(item.Key, new Font("Segoe UI", 7), Brushes.DarkGray, x, h - margin + 5);

                // Числовое значение над столбцом (количество треков)
                g.DrawString(item.Value.ToString(),
                    new Font("Segoe UI", 8, FontStyle.Bold), Brushes.White,
                    x + (bw / 4), h - bh - margin - 22);

                // Смещаемся к следующему столбцу (ширина + межстолбцовый зазор)
                x += bw + 12;
            }
        }

        /// <summary>
        /// Отрисовывает интерактивный линейный график динамики по годам.
        /// Поддерживает два режима: анализ объёма (количество треков) и анализ качества (средний рейтинг).
        /// Отображает последние 12 лет из имеющихся данных.
        /// </summary>
        /// <param name="g">Графический контекст PictureBox.</param>
        /// <param name="w">Ширина области отрисовки в пикселях.</param>
        /// <param name="h">Высота области отрисовки в пикселях.</param>
        private void DrawAdvancedLineChart(Graphics g, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Берём последние 12 лет данных, отсортированных по возрастанию года
            var displayData = _repo.GetAll()
                .GroupBy(t => t.Year)
                .OrderBy(g => g.Key)
                .Skip(Math.Max(0, _repo.GetAll().GroupBy(t => t.Year).Count() - 12))
                .ToList();

            // Заголовок зависит от текущего режима отображения
            string title = _statModeIsRating
                ? "АНАЛИЗ КАЧЕСТВА (СРЕДНИЙ РЕЙТИНГ)"
                : "АНАЛИЗ ОБЪЕМА (КОЛИЧЕСТВО ТРЕКОВ)";
            g.DrawString(title, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Gray, 45, 10);

            // Максимальное значение шкалы Y: для рейтинга — фиксированные 5.0, для счётчика — реальный максимум
            float maxVal = _statModeIsRating ? 5.0f : displayData.Max(g => g.Count());
            int margin = 40;

            // Горизонтальный шаг между точками данных
            float stepX = (float)(w - margin * 2) / (displayData.Count - 1);

            // Вычисляем экранные координаты каждой точки графика
            List<PointF> pts = new List<PointF>();
            for (int i = 0; i < displayData.Count; i++)
            {
                // Значение Y: средний рейтинг или количество треков за год
                float val = _statModeIsRating
                    ? (float)displayData[i].Average(t => t.Rating)
                    : (float)displayData[i].Count();

                // Инвертируем Y (экранная система: Y растёт вниз, данные — вверх)
                pts.Add(new PointF(
                    margin + (i * stepX),
                    (h - margin) - (val / maxVal * (h - margin * 2))
                ));
            }

            // Оси координат 
            using (Pen axis = new Pen(Color.FromArgb(50, 50, 50), 2))
            {
                g.DrawLine(axis, margin, margin, margin, h - margin);       // Ось Y (вертикальная)
                g.DrawLine(axis, margin, h - margin, w - margin, h - margin); // Ось X (горизонтальная)
            }

            // Сглаженная кривая тренда через все точки
            using (Pen p = new Pen(Color.FromArgb(30, 215, 96), 3))
                g.DrawCurve(p, pts.ToArray()); // DrawCurve строит кардинальный сплайн

            // Маркеры точек данных с подписями года
            for (int i = 0; i < pts.Count; i++)
            {
                // Белая точка-маркер в узле данных
                g.FillEllipse(Brushes.White, pts[i].X - 3, pts[i].Y - 3, 6, 6);

                // Подпись года под осью X
                g.DrawString(displayData[i].Key.ToString(),
                    new Font("Segoe UI", 7), Brushes.Gray,
                    pts[i].X - 10, h - margin + 10);
            }
        }

        #endregion

        #region Секция Справочных Окон

        /// <summary>
        /// Открывает модальное окно с интерактивным руководством пользователя.
        /// Описывает все ключевые функции приложения: поиск, плейлист, аналитику и рекомендации.
        /// </summary>
        private void ShowHelpDialog()
        {
            Form f = new Form
            {
                Size = new Size(650, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            // Тонкая рамка для визуального оформления безрамочного окна
            f.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, f.ClientRectangle, Color.FromArgb(45, 45, 45), ButtonBorderStyle.Solid);

            // Заголовок руководства пользователя
            Label t = new Label
            {
                Text = "РУКОВОДСТВО ПОЛЬЗОВАТЕЛЯ",
                Dock = DockStyle.Top,
                Height = 80,
                ForeColor = Color.FromArgb(30, 215, 96),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Основной текст руководства с описанием каждого модуля приложения
            string hText =
                "✦ ОСНОВНАЯ НАВИГАЦИЯ\n" +
                "Переключайтесь между 'Главной' (вся база) и 'Моим плейлистом' через левое меню. Система запомнит ваш выбор.\n\n" +
                "✦ УМНЫЙ ПОИСК И ФИЛЬТРЫ\n" +
                "Введите запрос в поле сверху. Программа мгновенно отфильтрует текущий список по названию, исполнителю или жанру.\n\n" +
                "✦ РЕКОМЕНДАТЕЛЬНЫЙ СЕРВИС\n" +
                "Нажмите ПРАВОЙ КНОПКОЙ МЫШИ на любую песню -> 'Показать похожие'. Алгоритм найдет лучшие треки того же жанра.\n\n" +
                "✦ ИНТЕРАКТИВНАЯ АНАЛИТИКА\n" +
                "В разделе 'Статистика' доступны графики. Используйте кнопку 'Сменить режим', чтобы переключаться между анализом количества треков и анализом их рейтинга.\n\n" +
                "✦ УПРАВЛЕНИЕ ИЗБРАННЫМ\n" +
                "Добавленные треки помечаются ★. Вы можете удалять их из плейлиста через контекстное меню в соответствующем разделе.";

            Label body = new Label
            {
                Text = hText,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                Padding = new Padding(35),
                TextAlign = ContentAlignment.TopLeft
            };

            Button b = new Button
            {
                Text = "ПОНЯТНО",
                Dock = DockStyle.Bottom,
                Height = 65,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.Click += (s, e) => f.Close();

            f.Controls.Add(body);
            f.Controls.Add(t);
            f.Controls.Add(b);
            f.ShowDialog();
        }

        /// <summary>
        /// Открывает модальное окно «О приложении» — презентационную карточку продукта TuneFlow.
        /// Содержит версию сборки, используемые технологии и информацию о разработчике.
        /// </summary>
        private void ShowAboutDialog()
        {
            Form f = new Form
            {
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(15, 15, 15)
            };

            // Зелёная рамка вместо стандартной — фирменный акцент TuneFlow
            f.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, f.ClientRectangle, Color.FromArgb(30, 215, 96), ButtonBorderStyle.Solid);

            // Крупный логотип приложения
            Label l1 = new Label
            {
                Text = "TUNEFLOW",
                Dock = DockStyle.Top,
                Height = 110,
                ForeColor = Color.FromArgb(30, 215, 96),
                Font = new Font("Segoe UI", 42, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Технические характеристики и копирайт
            string abText =
                "Intelligent Music Analytics System\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "Сборка: 2.0.12 (Stable 2026)\n" +
                "Технологии: .NET 8.0, C#, WinForms\n" +
                "База данных: 1000 нормализованных записей\n" +
                "Алгоритм: Multi-Weight Recommendation Engine\n\n" +
                "Разработано в рамках курса 'Алгоритмизация и программирование'\n" +
                "© 2026 TuneFlow Project. Все права защищены.";

            // Используем моноширинный шрифт Consolas для технического вида текста
            f.Controls.Add(new Label
            {
                Text = abText,
                ForeColor = Color.DarkGray,
                Font = new Font("Consolas", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });

            Button b = new Button
            {
                Text = "ЗАКРЫТЬ",
                Dock = DockStyle.Bottom,
                Height = 70,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 25, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.Click += (s, e) => f.Close();

            f.Controls.Add(l1);
            f.Controls.Add(b);
            f.ShowDialog();
        }

        #endregion

        #region Утилиты визуализации

        /// <summary>
        /// Применяет единый стиль к кнопкам управления окном (закрыть / развернуть / свернуть):
        /// плоский стиль без рамки, белый текст, эффект наведения с изменением фона.
        /// Для кнопки «✕» при наведении используется красный фон (стандарт Windows).
        /// </summary>
        /// <param name="btn">Кнопка, к которой применяется стиль.</param>
        /// <param name="text">Символ-иконка кнопки (✕, ▢ или —).</param>
        private void StyleWindowBtn(Button btn, string text)
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0; // Убираем рамку в плоском стиле
            btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand;

            // Цвет при наведении: красный для закрытия, серый для остальных
            btn.MouseEnter += (s, e) => btn.BackColor = text == "✕" ? Color.Red : Color.FromArgb(60, 60, 60);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
        }

        /// <summary>
        /// Создаёт кнопку навигации в боковой панели с заданным текстом и обработчиком клика.
        /// </summary>
        /// <param name="text">Текст кнопки (отображается с отступом слева для псевдо-иконки).</param>
        /// <param name="y">Y-координата кнопки внутри сайдбара.</param>
        /// <param name="onClick">Действие, выполняемое при нажатии.</param>
        /// <param name="bot">
        /// Если true — кнопка привязана к нижнему краю сайдбара (AnchorStyles.Bottom),
        /// используется для вспомогательных пунктов меню (Помощь, О приложении).
        /// </param>
        private void CreateSidebarButton(string text, int y, Action onClick, bool bot = false)
        {
            Button b = new Button
            {
                Text = "   " + text, // Отступ слева имитирует пространство под иконку
                ForeColor = Color.Gray,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Width = pnlSidebar.Width,
                Height = 45,
                Location = new Point(0, y),
                Cursor = Cursors.Hand
            };

            // Кнопки в нижней части сайдбара привязаны к нижнему краю, чтобы не смещаться при ресайзе
            if (bot) b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            b.FlatAppearance.BorderSize = 0; // Убираем рамку кнопки

            // Эффект наведения: подсвечиваем текст белым
            b.MouseEnter += (s, e) => b.ForeColor = Color.White;
            b.MouseLeave += (s, e) => b.ForeColor = Color.Gray;

            b.Click += (s, e) => onClick();
            pnlSidebar.Controls.Add(b);
        }

        #endregion
    }
}