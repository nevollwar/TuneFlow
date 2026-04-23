using System;
using System.Collections.Generic;
using TuneFlow.Logic.Models;
using TuneFlow.Logic.Services;

namespace TuneFlow.Logic.Algorithms
{
    /// <summary>
    /// Класс реализует комплексную логику формирования рекомендаций.
    /// Включает в себя контентную фильтрацию по жанрам, коллаборативную фильтрацию
    /// и интеграцию с графовыми алгоритмами поиска.
    /// </summary>
    public class Recommender
    {
        private readonly DataRepository repository;
        private readonly GraphManager graphManager;

        /// <summary>
        /// Инициализирует экземпляр класса <see cref="Recommender"/>.
        /// </summary>
        /// <param name="repository">Репозиторий данных для доступа к трекам и пользователям.</param>
        /// <param name="graphManager">Менеджер графа для выполнения BFS-поиска.</param>
        public Recommender(DataRepository repository, GraphManager graphManager)
        {
            repository = repository;
            graphManager = graphManager;
        }

        /// <summary>
        /// Реализует простой алгоритм рекомендаций на основе жанровых предпочтений.
        /// Если пользователю нравится трек жанра X, предлагаются другие треки этого жанра с высоким рейтингом.
        /// </summary>
        /// <param name="genre">Целевой жанр для поиска.</param>
        /// <param name="excludeTrackIds">Список ID треков, которые следует исключить (например, уже прослушанные).</param>
        /// <param name="count">Количество возвращаемых рекомендаций.</param>
        /// <returns>Список наиболее подходящих треков по жанру и рейтингу.</returns>
        public List<Track> GetSimpleRecommendations(string genre, List<int> excludeTrackIds, int count = 10)
        {
            var candidates = new List<Track>();

            // Фильтрация треков по жанру вручную (без LINQ)
            foreach (var track in repository.AllTracks)
            {
                if (track.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                {
                    // Исключаем те, что пользователь уже лайкнул
                    if (!excludeTrackIds.Contains(track.Id))
                    {
                        candidates.Add(track);
                    }
                }
            }

            // Сортировка кандидатов по рейтингу (используем наш QuickSorter)
            QuickSorter.Sort(candidates, (a, b) => a.Rating.CompareTo(b.Rating), false);

            // Возврат топ-N результатов
            var result = new List<Track>();
            for (int i = 0; i < Math.Min(count, candidates.Count); i++)
            {
                result.Add(candidates[i]);
            }

            return result;
        }

        /// <summary>
        /// Реализует алгоритм пользовательской коллаборативной фильтрации (User-Based Collaborative Filtering).
        /// Находит пользователей с похожими вкусами и предлагает треки, которые им понравились.
        /// </summary>
        /// <param name="targetUser">Пользователь, для которого формируются рекомендации.</param>
        /// <returns>Список рекомендованных треков.</returns>
        public List<Track> GetCollaborativeRecommendations(User targetUser)
        {
            var userSimilarities = new List<(User user, double score)>();

            // 1. Поиск похожих пользователей на основе коэффициента Жаккара
            foreach (var otherUser in repository.AllUsers)
            {
                if (otherUser.Id == targetUser.Id) continue;

                double similarity = CalculateJaccardIndex(targetUser.LikedTrackIds, otherUser.LikedTrackIds);
                if (similarity > 0)
                {
                    userSimilarities.Add((otherUser, similarity));
                }
            }

            // Сортировка пользователей по степени схожести (по убыванию)
            userSimilarities.Sort((a, b) => b.score.CompareTo(a.score));

            // 2. Сбор треков от наиболее похожих пользователей
            var recommendedIds = new Dictionary<int, double>();
            int topUsersCount = Math.Min(5, userSimilarities.Count);

            for (int i = 0; i < topUsersCount; i++)
            {
                var simUser = userSimilarities[i];
                foreach (var trackId in simUser.user.LikedTrackIds)
                {
                    if (!targetUser.LikedTrackIds.Contains(trackId))
                    {
                        if (!recommendedIds.ContainsKey(trackId)) recommendedIds[trackId] = 0;
                        // Вес рекомендации зависит от степени схожести пользователей
                        recommendedIds[trackId] += simUser.score;
                    }
                }
            }

            // 3. Преобразование ID в объекты Track
            var result = new List<Track>();
            foreach (var entry in recommendedIds)
            {
                var track = repository.GetTrackById(entry.Key);
                if (track != null) result.Add(track);
            }

            return result;
        }

        /// <summary>
        /// Формирует рекомендации на основе обхода графа в ширину (BFS).
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список треков, найденных через социальные связи в графе.</returns>
        public List<Track> GetGraphRecommendations(int userId)
        {
            // Вызов метода BFS из GraphManager
            List<int> trackIds = graphManager.GetRecommendationsBFS(userId);

            var result = new List<Track>();
            foreach (int id in trackIds)
            {
                var track = repository.GetTrackById(id);
                if (track != null) result.Add(track);
            }
            return result;
        }

        /// <summary>
        /// Вспомогательный метод для расчета коэффициента сходства Жаккара.
        /// Рассчитывается как отношение мощности пересечения множеств к мощности их объединения.
        /// </summary>
        private double CalculateJaccardIndex(List<int> setA, List<int> setB)
        {
            if (setA.Count == 0 || setB.Count == 0) return 0;

            int intersectionCount = 0;
            foreach (var item in setA)
            {
                if (setB.Contains(item)) intersectionCount++;
            }

            int unionCount = setA.Count + setB.Count - intersectionCount;
            return (double)intersectionCount / unionCount;
        }
    }
}