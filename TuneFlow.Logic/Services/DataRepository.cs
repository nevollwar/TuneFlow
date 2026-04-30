using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TuneFlow.Logic.Models;

namespace TuneFlow.Logic.Services
{
    public class DataRepository
    {
        private const string DbPath = "music_db.json";
        private const string UserPath = "user_data.json";

        private readonly List<Track> tracks = new List<Track>();
        private readonly List<User> users = new List<User>();

        public List<Track> AllTracks => tracks;
        public List<User> AllUsers => users;

        public DataRepository()
        {
            InitializeRepository();
        }

        private void InitializeRepository()
        {
            // Загрузка треков
            if (File.Exists(DbPath))
            {
                string json = File.ReadAllText(DbPath);
                var data = JsonSerializer.Deserialize<List<Track>>(json);
                if (data != null) tracks.AddRange(data);
            }
            else
            {
                GenerateNewDatabase();
            }

            // Создание пользователя
            var mainUser = new User(1, "Пользователь");

            // Загрузка лайков (сохранение между заходами)
            if (File.Exists(UserPath))
            {
                try
                {
                    string userJson = File.ReadAllText(UserPath);
                    var likedIds = JsonSerializer.Deserialize<List<int>>(userJson);
                    if (likedIds != null) mainUser.LikedTrackIds = likedIds;
                }
                catch { }
            }

            users.Add(mainUser);

            // Демо-пользователи для работы BFS
            for (int i = 2; i <= 51; i++)
            {
                var otherUser = new User(i, $"Пользователь {i}");
                Random r = new Random(i * 1337); // Уникальный сид для каждого пользователя

                // Каждый пользователь лайкает 100 случайных треков из 1000 (10% базы)
                for (int j = 0; j < 100; j++)
                {
                    int randomTrackId = r.Next(1, 1001);
                    otherUser.LikeTrack(randomTrackId);
                }
                users.Add(otherUser);
            }
        }

       
        public void SaveUserData(User user)
        {
            string json = JsonSerializer.Serialize(user.LikedTrackIds);
            File.WriteAllText(UserPath, json);
        }

        private void GenerateNewDatabase()
        {
            string[] prefixes = { "Deep", "Electric", "Midnight", "Silent", "Golden", "Crimson", "Ocean", "Urban", "Wild", "Mystic", "Solar", "Lunar", "Eternal", "Neon", "Cosmic", "Velvet", "Dark", "Radiant", "Hidden", "Summer", "Winter", "Lost", "Found", "Ancient", "Modern", "Plastic", "Natural", "Liquid", "Solid", "Broken", "Frozen", "Electric" };
            string[] suffixes = { "Dreams", "Echoes", "Horizon", "Storm", "Heart", "Night", "Silence", "Vibe", "Rhythm", "Soul", "Shadow", "Light", "Rain", "Fire", "Road", "Place", "Waves", "Wind", "Dust", "Stars", "Stone", "Cloud", "Forest", "City", "Ghost", "Spirit", "Mind", "Power", "Flow", "Breath", "Voice", "Vision" };
            string[] artists = { "The Beatles", "Daft Punk", "Queen", "Eminem", "Mozart", "Metallica", "ABBA", "Pink Floyd", "Led Zeppelin", "Hans Zimmer" };
            string[] genres = { "Rock", "Pop", "Jazz", "Techno", "Classical", "Hip-Hop", "Metal", "Blues" };

            Random rand = new Random();
            for (int i = 1; i <= 1000; i++)
            {
                tracks.Add(new Track
                {
                    Id = i,
                    Title = $"{prefixes[rand.Next(32)]} {suffixes[rand.Next(32)]}",
                    Artist = artists[rand.Next(artists.Length)],
                    Genre = genres[rand.Next(genres.Length)],
                    Year = rand.Next(1970, 2027),
                    Duration = Math.Round(rand.NextDouble() * 5 + 2, 2),
                    Rating = Math.Round(rand.NextDouble() * 5, 1)
                });
            }
            File.WriteAllText(DbPath, JsonSerializer.Serialize(tracks));
        }

        public List<Track> SearchTracks(string? titlePart = null, string? artistPart = null, string? genre = null, int? minYear = null, int? maxYear = null)
        {
            var results = new List<Track>();
            foreach (var track in tracks)
            {
                bool matches = true;

                if (!string.IsNullOrWhiteSpace(titlePart) &&
                    track.Title.IndexOf(titlePart, StringComparison.OrdinalIgnoreCase) < 0 &&
                    track.Artist.IndexOf(titlePart, StringComparison.OrdinalIgnoreCase) < 0)
                    matches = false;

                if (matches && !string.IsNullOrWhiteSpace(genre) &&
                    !track.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                    matches = false;

                if (matches) results.Add(track);
            }
            return results;
        }

        public Track GetTrackById(int id) { foreach (var t in tracks) if (t.Id == id) return t; return null; }
    }
}