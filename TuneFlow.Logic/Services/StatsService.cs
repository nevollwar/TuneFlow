using System;
using System.Collections.Generic;
using TuneFlow.Logic.Models;

namespace TuneFlow.Logic.Services
{
    /// <summary>
    /// Предоставляет функционал для статистического анализа базы музыкальных треков.
    /// Класс реализует алгоритмы агрегации данных для вычисления популярности жанров, 
    /// производительности исполнителей и средних метрик длительности.
    /// </summary>
    public class StatsService
    {
        /// <summary>
        /// Вычисляет распределение треков по жанрам для определения наиболее популярных направлений.
        /// </summary>
        /// <param name="tracks">Исходная выборка музыкальных треков.</param>
        /// <returns>Словарь, где ключ — название жанра, значение — количество треков в данном жанре.</returns>
        public Dictionary<string, int> GetGenreDistribution(List<Track> tracks)
        {
            var distribution = new Dictionary<string, int>();

            // Итеративный проход по коллекции для подсчета частоты встречаемости жанров
            foreach (var track in tracks)
            {
                if (distribution.ContainsKey(track.Genre))
                {
                    distribution[track.Genre]++;
                }
                else
                {
                    distribution.Add(track.Genre, 1);
                }
            }

            return distribution;
        }

        /// <summary>
        /// Определяет исполнителей с наибольшим количеством произведений в системе.
        /// </summary>
        /// <param name="tracks">Исходная выборка музыкальных треков.</param>
        /// <param name="topCount">Количество позиций в итоговом рейтинге.</param>
        /// <returns>Список пар "Исполнитель - Количество треков", отсортированный по убыванию.</returns>
        public List<KeyValuePair<string, int>> GetTopArtists(List<Track> tracks, int topCount = 5)
        {
            var artistCounts = new Dictionary<string, int>();

            // Агрегация данных по исполнителям
            foreach (var track in tracks)
            {
                if (artistCounts.ContainsKey(track.Artist))
                    artistCounts[track.Artist]++;
                else
                    artistCounts.Add(track.Artist, 1);
            }

            // Преобразование словаря в список для последующей ручной сортировки
            var sortedArtists = new List<KeyValuePair<string, int>>(artistCounts);

            // Реализация сортировки вставками для упорядочивания рейтинга исполнителей
            for (int i = 1; i < sortedArtists.Count; i++)
            {
                var key = sortedArtists[i];
                int j = i - 1;
                while (j >= 0 && sortedArtists[j].Value < key.Value)
                {
                    sortedArtists[j + 1] = sortedArtists[j];
                    j--;
                }
                sortedArtists[j + 1] = key;
            }

            // Формирование итоговой выборки (Top-N)
            var result = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < Math.Min(topCount, sortedArtists.Count); i++)
            {
                result.Add(sortedArtists[i]);
            }

            return result;
        }

        /// <summary>
        /// Рассчитывает среднюю длительность музыкальных композиций в разрезе каждого жанра.
        /// </summary>
        /// <param name="tracks">Исходная выборка музыкальных треков.</param>
        /// <returns>Словарь, сопоставляющий жанр и среднее арифметическое значение длительности.</returns>
        public Dictionary<string, double> GetAverageDurationByGenre(List<Track> tracks)
        {
            // Использование вспомогательных словарей для накопления суммы и количества элементов
            var sumDuration = new Dictionary<string, double>();
            var countPerGenre = new Dictionary<string, int>();

            foreach (var track in tracks)
            {
                if (!sumDuration.ContainsKey(track.Genre))
                {
                    sumDuration[track.Genre] = 0;
                    countPerGenre[track.Genre] = 0;
                }

                sumDuration[track.Genre] += track.Duration;
                countPerGenre[track.Genre]++;
            }

            // Вычисление итогового среднего значения
            var averageDurations = new Dictionary<string, double>();
            foreach (var genre in sumDuration.Keys)
            {
                double average = sumDuration[genre] / countPerGenre[genre];
                averageDurations.Add(genre, Math.Round(average, 2));
            }

            return averageDurations;
        }

        /// <summary>
        /// Формирует данные для визуализации распределения треков по годам выпуска.
        /// </summary>
        /// <param name="tracks">Исходная выборка музыкальных треков.</param>
        /// <returns>Словарь с распределением количества треков по годам.</returns>
        public Dictionary<int, int> GetYearlyDistribution(List<Track> tracks)
        {
            var yearlyData = new Dictionary<int, int>();

            foreach (var track in tracks)
            {
                if (yearlyData.ContainsKey(track.Year))
                    yearlyData[track.Year]++;
                else
                    yearlyData.Add(track.Year, 1);
            }

            return yearlyData;
        }
    }
}