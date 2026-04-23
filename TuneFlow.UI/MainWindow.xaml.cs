using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using TuneFlow.Logic.Algorithms;
using TuneFlow.Logic.Helpers;
using TuneFlow.Logic.Models;
using TuneFlow.Logic.Services;

using MusicTrack = TuneFlow.Logic.Models.Track;

namespace TuneFlow.UI
{
    public partial class MainWindow : Window
    {
        private readonly DataRepository repository;
        private readonly StatsService statsService;
        private readonly GraphManager graphManager;
        private User currentUser;

        /// <summary>
        /// Возвращает список ID лайкнутых треков. Используется конвертером для подсветки строк.
        /// </summary>
        public List<int> CurrentLikedIds => currentUser?.LikedTrackIds ?? new List<int>();

        public MainWindow()
        {
            InitializeComponent();

            repository = new DataRepository();
            statsService = new StatsService();
            graphManager = new GraphManager();
            currentUser = repository.AllUsers[0];

            // Настройка графа на основе лайков демонстрационных пользователей
            foreach (var user in repository.AllUsers)
                foreach (int tid in user.LikedTrackIds)
                    graphManager.AddRelation(user.Id, tid);

            SortByCombo.Items.Add("По названию");
            SortByCombo.Items.Add("По рейтингу");
            SortByCombo.Items.Add("По году");
            SortByCombo.SelectedIndex = 0;

            RefreshInterface();
        }

        /// <summary>
        /// Выполняет обновление данных, сброс фильтров и мгновенную отрисовку графиков.
        /// </summary>
        private void Tab_Changed(object sender, RoutedEventArgs e)
        {
            if (TabPlay != null && TabPlay.IsChecked == true)
            {
                SearchTitleBox.Text = ""; // Сброс поиска при переходе в плейлист
                RefreshInterface();
            }

            if (TabStats != null && TabStats.IsChecked == true)
            {
                // Принудительная отрисовка графика через Dispatcher
                Dispatcher.BeginInvoke(new Action(() => UpdateStats_Display(this, EventArgs.Empty)),
                    System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private void RefreshInterface()
        {
            // Обновление основной таблицы (Binding срабатывает заново)
            TracksDataGrid.ItemsSource = null;
            TracksDataGrid.ItemsSource = repository.AllTracks;

            // Обновление плейлиста
            var myTracks = repository.AllTracks.Where(t => currentUser.LikedTrackIds.Contains(t.Id)).ToList();
            PlaylistDataGrid.ItemsSource = null;
            PlaylistDataGrid.ItemsSource = myTracks;

            FooterStatus.Text = $"Сессия активна | Треков в базе: 1000 | В плейлисте: {myTracks.Count}";
        }

        private void LikeTrack_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (!currentUser.LikedTrackIds.Contains(id))
                {
                    currentUser.LikeTrack(id);
                    graphManager.AddRelation(currentUser.Id, id);
                }
                else
                {
                    currentUser.LikedTrackIds.Remove(id);
                }

                repository.SaveUserData(currentUser);
                RefreshInterface();
            }
        }

        private void RemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                currentUser.LikedTrackIds.Remove(id);

                repository.SaveUserData(currentUser);
                RefreshInterface();
            }
        }

        private void FindSimilar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var baseTrack = repository.GetTrackById(id);
                if (baseTrack == null) return;

                var similar = repository.AllTracks
                    .Select(t => new { T = t, Score = Similarity.CalculateSimilarity(baseTrack, t) })
                    .Where(x => x.T.Id != baseTrack.Id && x.Score > 0.4)
                    .OrderByDescending(x => x.Score).Select(x => x.T).Take(20).ToList();

                TracksDataGrid.ItemsSource = similar;
                TabLib.IsChecked = true;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            TracksDataGrid.ItemsSource = repository.SearchTracks(SearchTitleBox.Text, null, null, null, null);
            TabLib.IsChecked = true;
        }

        private void Sort_Changed(object sender, EventArgs e)
        {
            if (TracksDataGrid?.ItemsSource is IEnumerable<MusicTrack> items)
            {
                var list = new List<MusicTrack>(items);
                bool asc = ReverseSortCheck.IsChecked == false;

                Comparison<MusicTrack> comp = (t1, t2) => t1.Title.CompareTo(t2.Title);
                if (SortByCombo.SelectedIndex == 1) comp = (t1, t2) => t1.Rating.CompareTo(t2.Rating);
                if (SortByCombo.SelectedIndex == 2) comp = (t1, t2) => t1.Year.CompareTo(t2.Year);

                QuickSorter.Sort(list, comp, asc);
                TracksDataGrid.ItemsSource = list;
            }
        }

        /// <summary>
        /// Реализует высокоуровневую графическую визуализацию статистики.
        /// Отрисовывает интерактивную гистограмму распределения с использованием сетки, 
        /// градиентов и аналитических меток.
        /// </summary>
        private void UpdateStats_Display(object sender, EventArgs e)
        {
            if (MainCanvas == null || MainCanvas.ActualHeight < 100) return;
            MainCanvas.Children.Clear();

            // 1. Подготовка данных
            var rawData = statsService.GetYearlyDistribution(repository.AllTracks);
            var sortedData = rawData.OrderBy(x => x.Key).ToList();
            if (sortedData.Count == 0) return;

            double h = MainCanvas.ActualHeight;
            double w = MainCanvas.ActualWidth;
            double margin = 50; // Отступы для осей и подписей

            int maxVal = sortedData.Max(x => x.Value);
            int minYear = sortedData.Min(x => x.Key);
            int maxYear = sortedData.Max(x => x.Key);

            double stepX = (w - margin * 2) / sortedData.Count;
            double scaleY = (h - margin * 2) / maxVal;

            // 2. ОТРИСОВКА ГОРИЗОНТАЛЬНОЙ СЕТКИ (Grid Lines)
            int gridSteps = 5;
            for (int i = 0; i <= gridSteps; i++)
            {
                double yLevel = h - margin - (i * (h - margin * 2) / gridSteps);

                // Линия сетки
                Line gridLine = new Line
                {
                    X1 = margin,
                    Y1 = yLevel,
                    X2 = w - margin,
                    Y2 = yLevel,
                    Stroke = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 2 } // Пунктир
                };
                MainCanvas.Children.Add(gridLine);

                // Значение на оси Y
                TextBlock yLabel = new TextBlock
                {
                    Text = ((maxVal / gridSteps) * i).ToString(),
                    Foreground = Brushes.Gray,
                    FontSize = 10,
                    Margin = new Thickness(margin - 25, yLevel - 7, 0, 0)
                };
                MainCanvas.Children.Add(yLabel);
            }

            // 3. ОТРИСОВКА СТОЛБЦОВ С ГРАДИЕНТОМ
            // Создаем кисть с градиентом (от ярко-зеленого к темно-зеленому)
            LinearGradientBrush barGradient = new LinearGradientBrush();
            barGradient.StartPoint = new Point(0, 0);
            barGradient.EndPoint = new Point(0, 1);
            barGradient.GradientStops.Add(new GradientStop(Color.FromRgb(30, 215, 96), 0.0)); // Spotify Green
            barGradient.GradientStops.Add(new GradientStop(Color.FromRgb(15, 100, 45), 1.0));

            for (int i = 0; i < sortedData.Count; i++)
            {
                var entry = sortedData[i];
                double barHeight = entry.Value * scaleY;
                double xPos = margin + (i * stepX);
                double yPos = h - margin - barHeight;

                // Элемент столбца
                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(2, stepX - 4),
                    Height = barHeight,
                    Fill = barGradient,
                    RadiusX = 3,
                    RadiusY = 3, // Мягкое скругление сверху
                    ToolTip = new ToolTip { Content = $"Год: {entry.Key}\nТреков: {entry.Value}", Style = null }
                };

                // Эффект при наведении (Highlight)
                rect.MouseEnter += (s, ev) => { ((Rectangle)s).Opacity = 0.7; };
                rect.MouseLeave += (s, ev) => { ((Rectangle)s).Opacity = 1.0; };

                Canvas.SetLeft(rect, xPos);
                Canvas.SetTop(rect, yPos);
                MainCanvas.Children.Add(rect);

                // Подписи годов (каждые 10 лет или по краям)
                if (entry.Key % 10 == 0 || i == 0 || i == sortedData.Count - 1)
                {
                    TextBlock xLabel = new TextBlock
                    {
                        Text = entry.Key.ToString(),
                        Foreground = Brushes.DarkGray,
                        FontSize = 10,
                        RenderTransform = new RotateTransform(-45) // Наклон для стиля
                    };
                    Canvas.SetLeft(xLabel, xPos);
                    Canvas.SetTop(xLabel, h - margin + 10);
                    MainCanvas.Children.Add(xLabel);
                }
            }

            // 4. ОТРИСОВКА ОСЕЙ
            Line xAxis = new Line
            {
                X1 = margin,
                Y1 = h - margin,
                X2 = w - margin,
                Y2 = h - margin,
                Stroke = Brushes.DimGray,
                StrokeThickness = 2
            };
            MainCanvas.Children.Add(xAxis);

            // 5. ДОПОЛНИТЕЛЬНАЯ АНАЛИТИКА (Пик)
            var peak = sortedData.OrderByDescending(x => x.Value).First();
            StatsText.Inlines.Clear();
            StatsText.Inlines.Add(new Bold(new Run("АНАЛИТИЧЕСКИЙ ОТЧЕТ ТЕКУЩЕЙ БАЗЫ\n")) { Foreground = Brushes.White });
            StatsText.Inlines.Add(new Run($"Всего объектов: {sortedData.Sum(x => x.Value)} | "));
            StatsText.Inlines.Add(new Run($"Период: {minYear} — {maxYear} гг.\n"));
            StatsText.Inlines.Add(new Run($"Наибольшая активность: ") { Foreground = Brushes.Gray });
            StatsText.Inlines.Add(new Run($"{peak.Key} год ({peak.Value} треков)") { Foreground = new SolidColorBrush(Color.FromRgb(30, 215, 96)), FontWeight = FontWeights.Bold });
        }
    }
}