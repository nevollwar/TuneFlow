namespace TuneFlow.Logic
{
    /// <summary>
    /// Модель данных одного музыкального трека.
    /// Используется как единица хранения в <see cref="TrackRepository"/>
    /// и как источник биндинга для DataGridView в UI.
    /// </summary>
    public class Track
    {
        /// <summary>
        /// Уникальный числовой идентификатор трека (1-based).
        /// Используется как эталонный порядок для сброса сортировки к исходному состоянию.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Название трека. Генерируется как комбинация двух случайных слов (например, «Neon City»).</summary>
        public string Title { get; set; }

        /// <summary>Имя исполнителя. Выбирается случайно из фиксированного пула в <see cref="TrackRepository"/>.</summary>
        public string Artist { get; set; }

        /// <summary>Музыкальный жанр трека (Rock, Pop, Hip-Hop, Jazz, Techno, Indie, Metal, Lo-Fi).</summary>
        public string Genre { get; set; }

        /// <summary>Год выпуска трека. Генерируется в диапазоне [1990, 2023].</summary>
        public int Year { get; set; }

        /// <summary>
        /// Длительность трека в секундах. Генерируется в диапазоне [120, 359] (от 2 до ~6 минут).
        /// Для отображения в UI используйте <see cref="DurationFormatted"/>.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Пользовательский рейтинг трека в диапазоне [1.0, 5.0] с шагом 0.1.
        /// Используется как один из критериев в алгоритме рекомендаций.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Вычисляемое свойство: длительность в формате «М:СС» для отображения в таблице.
        /// Например, Duration = 215 секунд → «3:35».
        /// Не хранится в источнике данных; биндится в DataGridView как read-only колонка.
        /// </summary>
        public string DurationFormatted => $"{Duration / 60}:{Duration % 60:D2}";
    }
}