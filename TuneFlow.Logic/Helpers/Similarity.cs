using System;
using TuneFlow.Logic.Models;

namespace TuneFlow.Logic.Helpers
{
    /// <summary>
    /// Предоставляет математический инструментарий для вычисления метрик сходства между объектами типа <see cref="Track"/>.
    /// Класс реализует алгоритмы оценки релевантности на основе атрибутивного состава треков.
    /// </summary>
    public static class Similarity
    {
        // Весовые коэффициенты значимости характеристик (в сумме дают 1.0)
        private const double GenreWeight = 0.7; // Жанр имеет приоритетное значение
        private const double YearWeight = 0.3;  // Год выпуска является уточняющим фактором

        // Константа для нормализации разницы лет (максимально допустимая дистанция)
        private const int MaxYearDifference = 50;

        /// <summary>
        /// Вычисляет интегральный коэффициент похожести между двумя треками.
        /// Результат варьируется в диапазоне от 0.0 (полное различие) до 1.0 (идентичность характеристик).
        /// </summary>
        /// <param name="t1">Первый музыкальный трек для сравнения.</param>
        /// <param name="t2">Второй музыкальный трек для сравнения.</param>
        /// <returns>Значение типа double, представляющее степень сходства.</returns>
        public static double CalculateSimilarity(Track t1, Track t2)
        {
            if (t1 == null || t2 == null) return 0.0;

            // 1. Вычисление частного сходства по жанру
            double genreScore = CalculateGenreSimilarity(t1.Genre, t2.Genre);

            // 2. Вычисление частного сходства по году выпуска
            double yearScore = CalculateYearSimilarity(t1.Year, t2.Year);

            // 3. Расчет взвешенной суммы (Weighted Average)
            // Формула: S = (w1 * s1) + (w2 * s2)
            double totalSimilarity = (genreScore * GenreWeight) + (yearScore * YearWeight);

            return Math.Round(totalSimilarity, 2);
        }

        /// <summary>
        /// Оценивает сходство жанров. 
        /// Используется бинарная оценка: 1.0 при совпадении, 0.0 при различии.
        /// </summary>
        /// <param name="g1">Жанр первого трека.</param>
        /// <param name="g2">Жанр второго трека.</param>
        /// <returns>Коэффициент сходства жанров.</returns>
        private static double CalculateGenreSimilarity(string g1, string g2)
        {
            if (string.IsNullOrEmpty(g1) || string.IsNullOrEmpty(g2)) return 0.0;

            return g1.Equals(g2, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;
        }

        /// <summary>
        /// Вычисляет сходство на основе временной дистанции между годами выпуска треков.
        /// Используется линейная функция затухания.
        /// </summary>
        /// <param name="y1">Год выпуска первого трека.</param>
        /// <param name="y2">Год выпуска второго трека.</param>
        /// <returns>Коэффициент временного сходства (от 0.0 до 1.0).</returns>
        private static double CalculateYearSimilarity(int y1, int y2)
        {
            // Нахождение абсолютной разницы между годами
            int difference = Math.Abs(y1 - y2);

            // Если разница превышает установленный порог, сходство принимается равным 0
            if (difference >= MaxYearDifference) return 0.0;

            // Расчет коэффициента: чем меньше разница, тем выше результат
            // Формула: 1 - (diff / maxDiff)
            return 1.0 - ((double)difference / MaxYearDifference);
        }
    }
}