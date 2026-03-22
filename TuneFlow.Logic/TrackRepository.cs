using System;
using System.Collections.Generic;
using System.Linq;

namespace TuneFlow.Logic
{
    /// <summary>
    /// Репозиторий треков — единственный источник данных для всего приложения.
    /// При создании генерирует 1000 уникальных треков с детерминированными значениями
    /// (фиксированный сид Random гарантирует одинаковый результат на любом ПК).
    /// Предоставляет методы поиска, сортировки, аналитики и рекомендаций.
    /// </summary>
    public class TrackRepository
    {
        /// <summary>Внутреннее хранилище всех треков библиотеки.</summary>
        private List<Track> _tracks = new List<Track>();

        /// <summary>
        /// Инициализирует репозиторий и генерирует базу из 1000 треков.
        /// Использует фиксированный сид (42) для воспроизводимости данных —
        /// при каждом запуске приложения библиотека будет идентичной.
        /// </summary>
        public TrackRepository()
        {
            // Фиксированный сид (42): Random ВСЕГДА выдаёт одну и ту же последовательность,
            // что обеспечивает стабильность базы данных между запусками приложения
            var rnd = new Random(42);

            // Пул исполнителей для случайного распределения по трекам
            string[] artists = { "The Weeknd", "Arctic Monkeys", "Daft Punk", "Eminem", "Radiohead",
                                  "The Birthday", "ACIDMAN", "Lana Del Rey", "Gorillaz", "Coldplay",
                                  "Muse", "Nirvana", "the pillows" };

            // Пул жанров — каждый трек получит один случайный
            string[] genres = { "Rock", "Pop", "Hip-Hop", "Jazz", "Techno", "Indie", "Metal", "Lo-Fi" };

            // Два словаря прилагательных и существительных для генерации названий треков.
            // 35 × 35 = 1225 уникальных комбинаций — заведомо больше нужных 1000
            string[] words1 = { "Neon", "Midnight", "Dark", "Silent", "Electric", "Summer", "Endless",
                                 "Broken", "Crimson", "Crystal", "Golden", "Silver", "Fading", "Rising",
                                 "Falling", "Distant", "Hidden", "Secret", "Sacred", "Wild", "Cold",
                                 "Warm", "Sweet", "Bitter", "Deep", "High", "Blind", "Crazy", "Savage",
                                 "Gentle", "Liquid", "Cosmic", "Lunar", "Solar", "Astral" };

            string[] words2 = { "City", "Night", "Dream", "Echo", "Shadow", "Light", "Heart", "Soul",
                                 "Mind", "Fire", "Water", "Earth", "Wind", "Storm", "Rain", "Snow",
                                 "Sky", "Star", "Sun", "Moon", "Galaxy", "Universe", "Ocean", "River",
                                 "Mountain", "Forest", "Desert", "Island", "Road", "Path", "Journey",
                                 "Illusion", "Memory", "Vision" };

            // Генерируем все возможные комбинации «Прилагательное Существительное»
            List<string> uniqueTitles = new List<string>();
            foreach (var w1 in words1)
                foreach (var w2 in words2)
                    uniqueTitles.Add(w1 + " " + w2);

            // Перемешиваем список — благодаря сиду 42 порядок будет одинаковым всегда
            uniqueTitles = uniqueTitles.OrderBy(x => rnd.Next()).ToList();

            // Берём первые 1000 из перемешанного списка и создаём треки
            for (int i = 0; i < 1000; i++)
            {
                _tracks.Add(new Track
                {
                    Id = i + 1,
                    Title = uniqueTitles[i],
                    Artist = artists[rnd.Next(artists.Length)],       // Случайный исполнитель из пула
                    Genre = genres[rnd.Next(genres.Length)],         // Случайный жанр из пула
                    Year = rnd.Next(1990, 2024),                    // Год от 1990 до 2023 включительно
                    Duration = rnd.Next(120, 360),                      // Длительность: от 2 до 6 минут в секундах
                    Rating = Math.Round(rnd.NextDouble() * 4 + 1, 1) // Рейтинг в диапазоне [1.0 … 5.0]
                });
            }
        }

        /// <summary>
        /// Возвращает полный список всех треков библиотеки без фильтрации.
        /// </summary>
        public List<Track> GetAll() => _tracks;

        /// <summary>
        /// Выполняет поиск треков по подстроке в названии, имени исполнителя или жанре.
        /// Поиск регистронезависимый. При пустом запросе возвращает всю библиотеку.
        /// </summary>
        /// <param name="query">Строка поиска.</param>
        public List<Track> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _tracks;
            query = query.ToLower();

            return _tracks.Where(t =>
                t.Title.ToLower().Contains(query) ||
                t.Artist.ToLower().Contains(query) ||
                t.Genre.ToLower().Contains(query)
            ).ToList();
        }

        /// <summary>
        /// Сортирует полную библиотеку треков по указанному полю.
        /// Используется для серверной сортировки; UI-сортировка реализована отдельно в Form1.
        /// </summary>
        /// <param name="columnName">
        /// Имя свойства модели Track. Для отображаемой длительности передаётся "DurationFormatted",
        /// но сортировка выполняется по числовому полю Duration.
        /// </param>
        /// <param name="ascending">true — по возрастанию, false — по убыванию.</param>
        public List<Track> Sort(string columnName, bool ascending)
        {
            IEnumerable<Track> query = _tracks;
            switch (columnName)
            {
                case "Id": query = ascending ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id); break;
                case "Title": query = ascending ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title); break;
                case "Artist": query = ascending ? query.OrderBy(t => t.Artist) : query.OrderByDescending(t => t.Artist); break;
                case "Genre": query = ascending ? query.OrderBy(t => t.Genre) : query.OrderByDescending(t => t.Genre); break;
                case "Year": query = ascending ? query.OrderBy(t => t.Year) : query.OrderByDescending(t => t.Year); break;
                case "Rating": query = ascending ? query.OrderBy(t => t.Rating) : query.OrderByDescending(t => t.Rating); break;
                case "DurationFormatted": query = ascending ? query.OrderBy(t => t.Duration) : query.OrderByDescending(t => t.Duration); break;
                    // Неизвестное поле — возвращаем несортированный список
            }
            return query.ToList();
        }

        /// <summary>
        /// Рассчитывает коэффициент схожести между двумя треками по трём критериям.
        /// Используется как вспомогательный метод для будущего расширения рекомендательной системы.
        /// На данный момент в <see cref="GetRecommendations"/> напрямую не вызывается.
        /// </summary>
        /// <remarks>
        /// Система весов:
        /// <list type="bullet">
        ///   <item>Совпадение жанра — +70 баллов (главный критерий)</item>
        ///   <item>Близость года выпуска — +5…+20 баллов (чем ближе, тем выше)</item>
        ///   <item>Рейтинг целевого трека — до +10 баллов (Rating × 2)</item>
        /// </list>
        /// Итоговый балл зажат в диапазоне [0, 100].
        /// </remarks>
        /// <param name="baseTrack">Эталонный трек (выбранный пользователем).</param>
        /// <param name="targetTrack">Трек-кандидат для сравнения.</param>
        /// <returns>Коэффициент схожести от 0 до 100.</returns>
        public double CalculateSimilarity(Track baseTrack, Track targetTrack)
        {
            // Трек не может быть похож сам на себя — исключаем из выборки
            if (baseTrack.Id == targetTrack.Id) return 0;

            double score = 0;

            // Критерий 1: совпадение жанра — наиболее весомый фактор (70% от максимума)
            if (baseTrack.Genre == targetTrack.Genre) score += 70;

            // Критерий 2: близость по году выпуска — чем ближе эпохи, тем выше балл
            int yearDiff = Math.Abs(baseTrack.Year - targetTrack.Year);
            if (yearDiff <= 2) score += 20; // Практически одновременно
            else if (yearDiff <= 5) score += 10; // Одна эпоха
            else if (yearDiff <= 10) score += 5;  // Смежные десятилетия

            // Критерий 3: бонус за качество — рейтинг [1.0…5.0] даёт [2…10] баллов
            score += (targetTrack.Rating * 2);

            // Нормализуем: итог не может превышать 100
            return Math.Min(score, 100);
        }

        /// <summary>
        /// Возвращает топ-10 рекомендаций для выбранного трека.
        /// Алгоритм (упрощённый, без вызова <see cref="CalculateSimilarity"/>):
        /// фильтр по жанру → сортировка по рейтингу (убывание) → уточнение по году.
        /// </summary>
        /// <param name="selectedTrack">Трек, для которого строятся рекомендации.</param>
        /// <returns>Список из не более чем 10 похожих треков.</returns>
        public List<Track> GetRecommendations(Track selectedTrack)
        {
            return _tracks
                .Where(t => t.Id != selectedTrack.Id && t.Genre == selectedTrack.Genre) // Только тот же жанр, исключаем сам трек
                .OrderByDescending(t => t.Rating)                                        // Приоритет — высокий рейтинг
                .ThenByDescending(t => Math.Abs(t.Year - selectedTrack.Year))            // Уточнение: ближе по году
                .Take(10)                                                                // Возвращаем не более 10 результатов
                .ToList();
        }

        /// <summary>
        /// Возвращает распределение треков по жанрам (топ-5 жанров по количеству).
        /// Используется для построения гистограммы на дашборде.
        /// Примечание: в UI-методе <c>DrawFullHistogram</c> ограничение Take(5) снято — там берутся все жанры.
        /// </summary>
        public Dictionary<string, int> GetGenreDistribution()
        {
            return _tracks
                .GroupBy(t => t.Genre)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Возвращает количество треков по каждому году выпуска, отсортированное по возрастанию года.
        /// Используется для построения линейного графика динамики на дашборде.
        /// </summary>
        public Dictionary<int, int> GetYearDistribution()
        {
            return _tracks
                .GroupBy(t => t.Year)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Рассчитывает среднюю длительность треков в минутах для каждого жанра.
        /// Duration хранится в секундах, поэтому делим на 60 перед округлением.
        /// </summary>
        public Dictionary<string, double> GetAverageDurationByGenre()
        {
            return _tracks
                .GroupBy(t => t.Genre)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(t => t.Duration) / 60.0, 1) // Секунды → минуты, 1 знак после запятой
                );
        }

        /// <summary>
        /// Возвращает список исполнителей с наибольшим числом треков в библиотеке.
        /// Каждая строка имеет формат «Имя (N треков)» для отображения на дашборде.
        /// </summary>
        /// <param name="count">Количество исполнителей в результате (по умолчанию 5).</param>
        public List<string> GetTopArtists(int count = 5)
        {
            return _tracks
                .GroupBy(t => t.Artist)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => $"{g.Key} ({g.Count()} треков)")
                .ToList();
        }
    }
}