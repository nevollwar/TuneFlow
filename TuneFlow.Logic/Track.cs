namespace TuneFlow.Logic
{
    public class Track
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public int Duration { get; set; } // в секундах
        public double Rating { get; set; }

        // Удобный вывод длительности
        public string DurationFormatted => $"{Duration / 60}:{Duration % 60:D2}";
    }
}