using System;
using System.Collections.Generic;
using TuneFlow.Logic.Models;

namespace TuneFlow.Logic.Algorithms
{
    /// <summary>
    /// Класс предоставляет реализацию алгоритма быстрой сортировки (QuickSort).
    /// </summary>
    public static class QuickSorter
    {
        /// <summary>
        /// Выполняет сортировку списка объектов <see cref="Track"/> на основе заданного критерия.
        /// </summary>
        /// <param name="list">Список треков для сортировки.</param>
        /// <param name="comparison">Делегат, определяющий логику сравнения двух элементов.</param>
        /// <param name="ascending">Флаг направления сортировки: true — по возрастанию, false — по убыванию.</param>
        public static void Sort(List<Track> list, Comparison<Track> comparison, bool ascending = true)
        {
            if (list == null || list.Count <= 1)
                return;

            PerformQuickSort(list, 0, list.Count - 1, comparison, ascending);
        }

        /// <summary>
        /// Рекурсивный метод, реализующий разделение списка и последующую сортировку подмассивов.
        /// </summary>
        /// <param name="list">Сортируемый список.</param>
        /// <param name="left">Индекс левой границы подмассива.</param>
        /// <param name="right">Индекс правой границы подмассива.</param>
        /// <param name="comparison">Логика сравнения.</param>
        /// <param name="ascending">Направление сортировки.</param>
        private static void PerformQuickSort(List<Track> list, int left, int right, Comparison<Track> comparison, bool ascending)
        {
            if (left < right)
            {
                // Определение индекса опорного элемента (pivot) путем разбиения массива
                int pivotIndex = Partition(list, left, right, comparison, ascending);

                // Рекурсивный вызов для элементов левее опорного
                PerformQuickSort(list, left, pivotIndex - 1, comparison, ascending);

                // Рекурсивный вызов для элементов правее опорного
                PerformQuickSort(list, pivotIndex + 1, right, comparison, ascending);
            }
        }

        /// <summary>
        /// Метод разбиения массива (с использованием схемы Ломуто).
        /// Выбирает последний элемент в качестве опорного и переставляет элементы так, 
        /// чтобы слева оказались элементы меньше опорного, а справа — больше.
        /// </summary>
        /// <param name="list">Список для обработки.</param>
        /// <param name="left">Начальный индекс диапазона.</param>
        /// <param name="right">Конечный индекс диапазона.</param>
        /// <param name="comparison">Критерий сравнения.</param>
        /// <param name="ascending">Направление.</param>
        /// <returns>Итоговый индекс опорного элемента.</returns>
        private static int Partition(List<Track> list, int left, int right, Comparison<Track> comparison, bool ascending)
        {
            // Выбор крайнего правого элемента в качестве опорного (Pivot)
            Track pivot = list[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                // Сравнение текущего элемента с опорным через делегат
                int compareResult = comparison(list[j], pivot);

                // Условие перестановки зависит от требуемого порядка (прямой или обратный)
                bool shouldSwap = ascending ? (compareResult <= 0) : (compareResult >= 0);

                if (shouldSwap)
                {
                    i++;
                    Swap(list, i, j);
                }
            }

            // Установка опорного элемента в его окончательную позицию
            Swap(list, i + 1, right);
            return i + 1;
        }

        /// <summary>
        /// Вспомогательный метод для обмена значениями двух элементов списка.
        /// </summary>
        /// <param name="list">Список, в котором производится обмен.</param>
        /// <param name="indexA">Индекс первого элемента.</param>
        /// <param name="indexB">Индекс второго элемента.</param>
        private static void Swap(List<Track> list, int indexA, int indexB)
        {
            if (indexA == indexB) return;

            Track temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }
    }
}