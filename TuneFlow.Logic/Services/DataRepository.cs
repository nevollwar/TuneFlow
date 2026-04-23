using System;
using System.Collections.Generic;
using System.Linq;
using TuneFlow.Logic.Models;

namespace TuneFlow.Logic.Services
{
    /// <summary>
    /// Обеспечивает централизованное управление данными приложения.
    /// Класс инкапсулирует коллекции музыкальных треков и пользователей, 
    /// предоставляя методы для инициализации, поиска и фильтрации данных.
    /// </summary>
    public class DataRepository
    {
        private readonly List<Track> _tracks;
        private readonly List<User> _users;
        private readonly Random _random;

        /// <summary>
        /// Инициализирует новый экземпляр репозитория и запускает процесс генерации данных.
        /// </summary>
        public DataRepository()
        {
            _tracks = new List<Track>();
            _users = new List<User>();
            _random = new Random();

            InitializeRepository();
        }

        /// <summary>
        /// Возвращает полный список загруженных музыкальных треков.
        /// </summary>
        public List<Track> AllTracks => _tracks;

        /// <summary>
        /// Возвращает полный список зарегистрированных пользователей.
        /// </summary>
        public List<User> AllUsers => _users;

        /// <summary>
        /// Выполняет инициализацию хранилища, генерируя массив из 1000 треков 
        /// и набор демонстрационных пользователей.
        /// </summary>
        private void InitializeRepository()
        {
            string[] genres = { "Rock", "Pop", "Jazz", "Techno", "Classical", "Hip-Hop", "Metal", "Blues" };
            string[] artists = { "The Beatles", "Daft Punk", "Queen", "Eminem", "The Birthday", "Mozart", "Metallica", "ABBA", "Pink Floyd", "Led Zeppelin", "Hans Zimmer", "the pillows", "Placebo" };

            // 1. Генерация 1000 музыкальных треков
            for (int i = 1; i <= 1000; i++)
            {
                _tracks.Add(new Track
                {
                    Id = i,
                    Title = $"Композиция #{i}",
                    Artist = artists[_random.Next(artists.Length)],
                    Genre = genres[_random.Next(genres.Length)],
                    Year = _random.Next(1970, 2025),
                    Duration = Math.Round(_random.NextDouble() * (7.0 - 1.5) + 1.5, 2), // Длительность от 1.5 до 7 минут
                    Rating = Math.Round(_random.NextDouble() * 5.0, 1) // Рейтинг от 0.0 до 5.0
                });
            }

            // 2. Генерация демонстрационных пользователей
            string[] userNames = { "Алексей", "Мария", "Дмитрий", "Елена", "Иван" };
            for (int i = 1; i <= userNames.Length; i++)
            {
                var user = new User(i, userNames[i - 1]);

                // Имитация предпочтений: каждый пользователь "лайкает" случайные 15-20 треков
                int likesCount = _random.Next(15, 21);
                for (int j = 0; j < likesCount; j++)
                {
                    int randomTrackId = _random.Next(1, 1001);
                    user.LikeTrack(randomTrackId);
                }
                _users.Add(user);
            }
        }

        /// <summary>
        /// Реализует поиск треков по нескольким параметрам одновременно.
        /// </summary>
        /// <param name="titlePart">Часть названия трека (необязательно).</param>
        /// <param name="artistPart">Имя исполнителя (необязательно).</param>
        /// <param name="genre">Конкретный жанр.</param>
        /// <param name="minYear">Нижняя граница года выпуска.</param>
        /// <param name="maxYear">Верхняя граница года выпуска.</param>
        /// <returns>Список треков, удовлетворяющих всем заданным критериям.</returns>
        public List<Track> SearchTracks(string titlePart = null, string artistPart = null, string genre = null, int? minYear = null, int? maxYear = null)
        {
            var results = new List<Track>();

            // Итеративный проход по коллекции для фильтрации по совокупности условий
            foreach (var track in _tracks)
            {
                bool matches = true;

                // Проверка соответствия фрагменту названия
                if (!string.IsNullOrWhiteSpace(titlePart) &&
                    track.Title.IndexOf(titlePart, StringComparison.OrdinalIgnoreCase) < 0)
                    matches = false;

                // Проверка соответствия исполнителю
                if (matches && !string.IsNullOrWhiteSpace(artistPart) &&
                    track.Artist.IndexOf(artistPart, StringComparison.OrdinalIgnoreCase) < 0)
                    matches = false;

                // Проверка соответствия жанру
                if (matches && !string.IsNullOrWhiteSpace(genre) &&
                    !track.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                    matches = false;

                // Проверка временного диапазона
                if (matches && minYear.HasValue && track.Year < minYear.Value)
                    matches = false;

                if (matches && maxYear.HasValue && track.Year > maxYear.Value)
                    matches = false;

                // Если объект прошел все проверки, он добавляется в результирующую выборку
                if (matches)
                {
                    results.Add(track);
                }
            }

            return results;
        }

        /// <summary>
        /// Осуществляет поиск трека по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор трека.</param>
        /// <returns>Экземпляр <see cref="Track"/> или null, если объект не найден.</returns>
        public Track GetTrackById(int id)
        {
            // Ручной поиск объекта в коллекции
            foreach (var track in _tracks)
            {
                if (track.Id == id) return track;
            }
            return null;
        }
    }
}