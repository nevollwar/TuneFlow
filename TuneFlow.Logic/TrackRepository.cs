using System;
using System.Collections.Generic;
using System.Linq;

namespace TuneFlow.Logic
{
    public class TrackRepository
    {
        private List<Track> _allTracks = new List<Track>();

        public TrackRepository()
        {
            GenerateDummyData(1000);
        }

        private void GenerateDummyData(int count)
        {
            var rnd = new Random();
            string[] genres = { "Rock", "Pop", "Hip-Hop", "Jazz", "Techno", "Indie" };
            string[] artists = { "The Weeknd", "Arctic Monkeys", "Daft Punk", "Eminem", "Radiohead", "Lana Del Rey" };

            for (int i = 1; i <= count; i++)
            {
                _allTracks.Add(new Track
                {
                    Title = $"Track {i}",
                    Artist = artists[rnd.Next(artists.Length)],
                    Genre = genres[rnd.Next(genres.Length)],
                    Year = rnd.Next(1990, 2024),
                    Duration = rnd.Next(120, 300),
                    Rating = Math.Round(rnd.NextDouble() * 5, 1)
                });
            }
        }

        public List<Track> GetAll() => _allTracks;

        // Поиск по названию или артисту
        public List<Track> Search(string query)
        {
            return _allTracks.Where(t =>
                t.Title.ToLower().Contains(query.ToLower()) ||
                t.Artist.ToLower().Contains(query.ToLower())).ToList();
        }
    }
}