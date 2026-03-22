public class Track
{
    public int Id { get; set; } // Оригинальный индекс для сброса сортировки
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Genre { get; set; }
    public int Year { get; set; }
    public int Duration { get; set; }
    public double Rating { get; set; }
    public string DurationFormatted => $"{Duration / 60}:{Duration % 60:D2}";
}