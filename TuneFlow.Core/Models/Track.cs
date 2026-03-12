namespace TuneFlow.Core.Models
{
    public class Track
    {
        // Уникальный номер трека (нужен для поиска и базы треков)
        public int Id { get; set; }

        // Название песни
        public string Title { get; set; }

        // Имя исполнителя или название группы
        public string Artist { get; set; }
        
        // Жанр (например: Rock, Pop, Jazz)
        public string Genre { get; set; }

        // Год выпуска
        public int Year { get; set; }

        // Длительность (в секундах)
        public int DurationInSeconds { get; set; }

        // Рейтинг от 1.0 до 5.0
        public double Rating { get; set; }

        // Вспомогательное свойство, которое красиво выведет время (например, 03:45)
        public string FormattedDuration => $"{DurationInSeconds / 60:D2}:{DurationInSeconds % 60:D2}";
    }
}
