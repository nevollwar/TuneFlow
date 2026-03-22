using System;
using System.Collections.Generic;
using System.Linq;

namespace TuneFlow.Logic
{
    public class TrackRepository
    {
        private List<Track> _tracks = new List<Track>();

        public TrackRepository()
        {
            // МАГИЯ: Фиксированный сид (42). Благодаря ему рандом ВСЕГДА выдает одни и те же значения!
            var rnd = new Random(42);

            string[] artists = { "The Weeknd", "Arctic Monkeys", "Daft Punk", "Eminem", "Radiohead","The Birthday", "ACIDMAN", "Lana Del Rey", "Gorillaz", "Coldplay", "Muse", "Nirvana", "the pillows" };
            string[] genres = { "Rock", "Pop", "Hip-Hop", "Jazz", "Techno", "Indie", "Metal", "Lo-Fi" };

            // Словари для генерации уникальных названий (35 * 35 = 1225 комбинаций)
            string[] words1 = { "Neon", "Midnight", "Dark", "Silent", "Electric", "Summer", "Endless", "Broken", "Crimson", "Crystal", "Golden", "Silver", "Fading", "Rising", "Falling", "Distant", "Hidden", "Secret", "Sacred", "Wild", "Cold", "Warm", "Sweet", "Bitter", "Deep", "High", "Blind", "Crazy", "Savage", "Gentle", "Liquid", "Cosmic", "Lunar", "Solar", "Astral" };
            string[] words2 = { "City", "Night", "Dream", "Echo", "Shadow", "Light", "Heart", "Soul", "Mind", "Fire", "Water", "Earth", "Wind", "Storm", "Rain", "Snow", "Sky", "Star", "Sun", "Moon", "Galaxy", "Universe", "Ocean", "River", "Mountain", "Forest", "Desert", "Island", "Road", "Path", "Journey", "Illusion", "Memory", "Vision" };

            // Создаем все возможные уникальные комбинации
            List<string> uniqueTitles = new List<string>();
            foreach (var w1 in words1)
            {
                foreach (var w2 in words2)
                {
                    uniqueTitles.Add(w1 + " " + w2);
                }
            }

            // Перемешиваем названия (из-за сида 42 они перемешаются одинаково на любом ПК)
            uniqueTitles = uniqueTitles.OrderBy(x => rnd.Next()).ToList();

            // Берем ровно 1000 треков
            for (int i = 0; i < 1000; i++)
            {
                _tracks.Add(new Track
                {
                    Id = i + 1,
                    Title = uniqueTitles[i],
                    Artist = artists[rnd.Next(artists.Length)],
                    Genre = genres[rnd.Next(genres.Length)],
                    Year = rnd.Next(1990, 2024),
                    Duration = rnd.Next(120, 360),
                    Rating = Math.Round(rnd.NextDouble() * 4 + 1, 1)
                });
            }
        }

        public List<Track> GetAll() => _tracks;

        public List<Track> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _tracks;
            query = query.ToLower();

            return _tracks.Where(t =>
                t.Title.ToLower().Contains(query) ||
                t.Artist.ToLower().Contains(query) ||
                t.Genre.ToLower().Contains(query)).ToList();
        }

        public List<Track> Sort(string columnName, bool ascending)
        {
            IEnumerable<Track> query = _tracks;
            switch (columnName)
            {
                case "Id": query = ascending ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id); break;
                case "Title": query = ascending ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title); break;
                case "Artist": query = ascending ? query.OrderBy(t => t.Artist) : query.OrderByDescending(t => t.Artist); break;
                case "Genre": query = ascending ? query.OrderBy(t => t.Genre) : query.OrderByDescending(t => t.Genre); break;
                case "Year": query = ascending ? query.OrderBy(t => t.Year) : query.OrderByDescending(t => t.Year); break;
                case "Rating": query = ascending ? query.OrderBy(t => t.Rating) : query.OrderByDescending(t => t.Rating); break;
                case "DurationFormatted": query = ascending ? query.OrderBy(t => t.Duration) : query.OrderByDescending(t => t.Duration); break;
            }
            return query.ToList();
        }
    }
}