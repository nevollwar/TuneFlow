using System;
using System.Collections.Generic;

namespace TuneFlow.Logic.Algorithms
{
    /// <summary>
    /// Класс управляет структурой графа «пользователь–трек» и реализует алгоритмы 
    /// поиска на графах для формирования рекомендаций.
    /// Использует список смежности для представления связей.
    /// </summary>
    public class GraphManager
    {
        /// <summary>
        /// Список смежности графа. 
        /// Ключ — строковый идентификатор узла (например, "u1" для пользователя или "t5" для трека).
        /// Значение — список идентификаторов смежных узлов.
        /// </summary>
        private readonly Dictionary<string, List<string>> _adjacencyList;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GraphManager"/>.
        /// </summary>
        public GraphManager()
        {
            _adjacencyList = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Добавляет ребро в неориентированный граф между пользователем и треком.
        /// Формирует связь «пользователь лайкнул трек».
        /// </summary>
        /// <param name="userId">Уникальный идентификатор пользователя.</param>
        /// <param name="trackId">Уникальный идентификатор трека.</param>
        public void AddRelation(int userId, int trackId)
        {
            string userNode = "u" + userId;
            string trackNode = "t" + trackId;

            // Добавление узлов в список смежности, если они отсутствуют
            if (!_adjacencyList.ContainsKey(userNode)) _adjacencyList[userNode] = new List<string>();
            if (!_adjacencyList.ContainsKey(trackNode)) _adjacencyList[trackNode] = new List<string>();

            // Создание двусторонней связи (неориентированный граф)
            if (!_adjacencyList[userNode].Contains(trackNode))
                _adjacencyList[userNode].Add(trackNode);

            if (!_adjacencyList[trackNode].Contains(userNode))
                _adjacencyList[trackNode].Add(userNode);
        }

        /// <summary>
        /// Реализует алгоритм поиска в ширину (BFS) для нахождения рекомендаций.
        /// Алгоритм ищет треки, которые нравятся пользователям с похожими вкусами.
        /// </summary>
        /// <param name="startUserId">Идентификатор целевого пользователя.</param>
        /// <param name="maxDepth">Максимальная глубина поиска (рекомендуется 3 для поиска через один уровень сходства).</param>
        /// <returns>Список идентификаторов рекомендованных треков, отсортированный по релевантности.</returns>
        public List<int> GetRecommendationsBFS(int startUserId, int maxDepth = 3)
        {
            string startNode = "u" + startUserId;
            var recommendedTracks = new Dictionary<int, int>(); // TrackId -> Вес (частота встречи)

            if (!_adjacencyList.ContainsKey(startNode))
                return new List<int>();

            // Очередь для BFS: хранит идентификатор узла и текущую глубину
            var queue = new Queue<(string nodeId, int depth)>();
            // Множество посещенных узлов для предотвращения циклов
            var visited = new HashSet<string>();

            queue.Enqueue((startNode, 0));
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var (currentNode, currentDepth) = queue.Dequeue();

                // Остановка при достижении заданной глубины
                if (currentDepth >= maxDepth) continue;

                if (_adjacencyList.ContainsKey(currentNode))
                {
                    foreach (var neighbor in _adjacencyList[currentNode])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, currentDepth + 1));

                            // Логика формирования рекомендаций:
                            // На глубине 1 находятся треки, которые уже лайкнул пользователь.
                            // На глубине 2 находятся "похожие" пользователи.
                            // На глубине 3 находятся треки, лайкнутые "похожими" пользователями.
                            if (currentDepth + 1 == 3 && neighbor.StartsWith("t"))
                            {
                                int trackId = int.Parse(neighbor.Substring(1));
                                if (!recommendedTracks.ContainsKey(trackId))
                                    recommendedTracks[trackId] = 0;

                                recommendedTracks[trackId]++;
                            }
                        }
                    }
                }
            }

            // Преобразование словаря результатов в отсортированный список ID треков
            return SortResultsByWeight(recommendedTracks);
        }

        /// <summary>
        /// Выполняет вспомогательную сортировку результатов BFS по частоте их упоминания.
        /// </summary>
        /// <param name="counts">Словарь соответствия ID трека и его веса.</param>
        /// <returns>Список ID треков.</returns>
        private List<int> SortResultsByWeight(Dictionary<int, int> counts)
        {
            var result = new List<int>(counts.Keys);
            // Простая сортировка вставками для малого набора рекомендаций
            for (int i = 1; i < result.Count; i++)
            {
                int key = result[i];
                int j = i - 1;
                while (j >= 0 && counts[result[j]] < counts[key])
                {
                    result[j + 1] = result[j];
                    j = j - 1;
                }
                result[j + 1] = key;
            }
            return result;
        }
    }
}