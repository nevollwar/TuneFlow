using System;

namespace TuneFlow.Logic.Models
{
    /// <summary>
    /// Представляет доменную модель музыкальной композиции (трека) в системе.
    /// Данный класс инкапсулирует характеристики аудиозаписи, необходимые для 
    /// реализации алгоритмов сортировки, поиска и фильтрации.
    /// </summary>
    public class Track
    {
        /// <summary>
        /// Уникальный идентификатор музыкального трека.
        /// Используется для однозначной идентификации объекта в структурах данных и графах.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название музыкального произведения.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Сведения об исполнителе или музыкальном коллективе.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Жанровая принадлежность трека. 
        /// Используется в качестве первичного критерия для алгоритма рекомендаций.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Год официального выпуска или публикации трека.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Продолжительность трека в минутах.
        /// Данный параметр используется для расчета статистических показателей по жанрам.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Рейтинг популярности трека (обычно по шкале от 0.0 до 5.0).
        /// Является весовым коэффициентом при формировании списка рекомендаций.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Track"/> со значениями по умолчанию.
        /// </summary>
        public Track()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Track"/> с заданными параметрами.
        /// </summary>
        /// <param name="id">Первичный ключ объекта.</param>
        /// <param name="title">Наименование трека.</param>
        /// <param name="artist">Исполнитель.</param>
        /// <param name="genre">Жанр.</param>
        /// <param name="year">Год издания.</param>
        /// <param name="duration">Длительность в минутах.</param>
        /// <param name="rating">Оценка/Рейтинг.</param>
        public Track(int id, string title, string artist, string genre, int year, double duration, double rating)
        {
            // Выполняется инициализация свойств через параметры конструктора
            Id = id;
            Title = title;
            Artist = artist;
            Genre = genre;
            Year = year;
            Duration = duration;
            Rating = rating;
        }

        /// <summary>
        /// Формирует строковое представление объекта для отладки и отображения в простых интерфейсах.
        /// </summary>
        /// <returns>Строка, содержащая основные сведения о треке: исполнитель и название.</returns>
        public override string ToString()
        {
            // Возвращается форматированная строка в классическом виде "Исполнитель - Название"
            return $"{Artist} — {Title} ({Genre}, {Year})";
        }
    }
}