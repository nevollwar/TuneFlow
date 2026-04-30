using System;
using System.Collections.Generic;
using System.Linq;
using TuneFlow.Logic.Models;
using TuneFlow.Logic.Services;

namespace TuneFlow.Logic.Algorithms
{
    /// <summary>
    /// Представляет интеллектуальный модуль формирования музыкальных рекомендаций.
    /// Класс интегрирует стратегии контентной фильтрации, графового анализа и 
    /// коллаборативной фильтрации для построения персонализированных выборок.
    /// </summary>
    public class Recommender
    {
        private readonly DataRepository _repository;
        private readonly GraphManager _graphManager;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Recommender"/>.
        /// </summary>
        /// <param name="repository">Репозиторий данных, выступающий источником музыкальных объектов.</param>
        /// <param name="graphManager">Менеджер графа для реализации структурного поиска связей.</param>
        public Recommender(DataRepository repository, GraphManager graphManager)
        {
            // Инициализация полей с поддержкой инъекции зависимостей
            _repository = repository;
            _graphManager = graphManager;
        }

        /// <summary>
        /// Формирует рекомендации на основе жанровой принадлежности и рейтинга (Контентная фильтрация).
        /// </summary>
        /// <param name="genre">Целевой музыкальный жанр.</param>
        /// <param name="excludeTrackIds">Список идентификаторов треков для исключения из результирующей выборки.</param>
        /// <param name="count">Максимальное количество возвращаемых элементов.</param>
        /// <returns>Список объектов <see cref="Track"/>, соответствующих критериям подобия.</returns>
        public List<Track> GetSimpleRecommendations(string genre, List<int> excludeTrackIds, int count = 10)
        {
            // Валидация входных данных и состояния зависимых сервисов
            if (_repository == null || _repository.AllTracks == null || string.IsNullOrEmpty(genre))
            {
                return new List<Track>();
            }

            var candidates = new List<Track>();
            var excludeSet = new HashSet<int>(excludeTrackIds ?? new List<int>());

            // Итеративный отбор кандидатов по признаку жанрового соответствия
            foreach (var track in _repository.AllTracks)
            {
                if (track != null && track.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                {
                    // Проверка на отсутствие трека в списке исключений (уже прослушанные/лайкнутые)
                    if (!excludeSet.Contains(track.Id))
                    {
                        candidates.Add(track);
                    }
                }
            }

            // Ранжирование кандидатов по убыванию экспертного рейтинга
            candidates.Sort((a, b) => b.Rating.CompareTo(a.Rating));

            // Ограничение выборки заданным количественным параметром
            return candidates.Take(count).ToList();
        }

        /// <summary>
        /// Генерирует рекомендации на основе структурного анализа связей в графе (BFS).
        /// </summary>
        /// <param name="userId">Идентификатор субъекта, для которого выполняется поиск.</param>
        /// <returns>Коллекция рекомендованных музыкальных произведений.</returns>
        public List<Track> GetGraphRecommendations(int userId)
        {
            // Проверка целостности компонентов системы
            if (_graphManager == null || _repository == null)
            {
                return new List<Track>();
            }

            // Выполнение алгоритма поиска в ширину для нахождения узлов на дистанции N
            var trackIds = _graphManager.GetRecommendationsBFS(userId);
            var result = new List<Track>();

            // Преобразование найденных идентификаторов в полноценные доменные модели
            foreach (var id in trackIds)
            {
                var track = _repository.GetTrackById(id);
                if (track != null)
                {
                    result.Add(track);
                }
            }

            return result;
        }

        /// <summary>
        /// Реализует пользовательскую коллаборативную фильтрацию на основе коэффициента Жаккара.
        /// Находит пользователей с похожими паттернами предпочтений и предлагает их контент.
        /// </summary>
        /// <param name="targetUser">Целевой пользователь для анализа подобия.</param>
        /// <returns>Список релевантных треков, ранжированных по весовым коэффициентам сходства.</returns>
        public List<Track> GetCollaborativeRecommendations(User targetUser)
        {
            // Защитное программирование: обработка неопределенных состояний субъекта и базы данных
            if (targetUser == null || targetUser.LikedTrackIds == null || _repository?.AllUsers == null)
            {
                return new List<Track>();
            }

            var recommendationScores = new Dictionary<int, double>();

            // Попарное сравнение векторов предпочтений текущего пользователя с остальными субъектами
            foreach (var otherUser in _repository.AllUsers)
            {
                // Исключение самосравнения и обработка пустых профилей
                if (otherUser == null || otherUser.Id == targetUser.Id || otherUser.LikedTrackIds == null)
                {
                    continue;
                }

                // Вычисление меры сходства Жаккара между множествами лайков
                double similarity = CalculateJaccardIndex(targetUser.LikedTrackIds, otherUser.LikedTrackIds);

                // Если сходство обнаружено, аккумулируем веса для треков, отсутствующих у целевого пользователя
                if (similarity > 0)
                {
                    foreach (var trackId in otherUser.LikedTrackIds)
                    {
                        if (!targetUser.LikedTrackIds.Contains(trackId))
                        {
                            if (!recommendationScores.ContainsKey(trackId))
                            {
                                recommendationScores[trackId] = 0;
                            }
                            recommendationScores[trackId] += similarity;
                        }
                    }
                }
            }

            // Формирование итогового списка путем сортировки по накопленному весу релевантности
            return recommendationScores
                .OrderByDescending(x => x.Value)
                .Select(x => _repository.GetTrackById(x.Key))
                .Where(t => t != null)
                .ToList();
        }

        /// <summary>
        /// Вспомогательный метод для вычисления индекса подобия Жаккара.
        /// Рассчитывается как отношение мощности пересечения множеств к мощности их объединения.
        /// </summary>
        private double CalculateJaccardIndex(List<int> setA, List<int> setB)
        {
            if (setA == null || setB == null || setA.Count == 0 || setB.Count == 0)
            {
                return 0.0;
            }

            // Нахождение количества общих элементов (пересечение)
            int intersectionCount = 0;
            foreach (var item in setA)
            {
                if (setB.Contains(item))
                {
                    intersectionCount++;
                }
            }

            // Мощность объединения рассчитывается по формуле: |A| + |B| - |A ∩ B|
            int unionCount = setA.Count + setB.Count - intersectionCount;

            return unionCount == 0 ? 0 : (double)intersectionCount / unionCount;
        }
    }
}