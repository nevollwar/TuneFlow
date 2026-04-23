using System;
using System.Collections.Generic;

namespace TuneFlow.Logic.Models
{
    /// <summary>
    /// Представляет доменную модель пользователя в информационно-рекомендательной системе.
    /// Класс инкапсулирует персональные данные субъекта, его предпочтения (лайки) 
    /// и структуру пользовательских коллекций (плейлистов).
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// Служит первичным ключом для идентификации узла типа "Пользователь" в графовых структурах.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя или псевдоним пользователя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Коллекция идентификаторов понравившихся треков.
        /// Данный набор данных является ключевым для реализации алгоритмов 
        /// коллаборативной фильтрации и построения ребер графа «пользователь–трек».
        /// </summary>
        public List<int> LikedTrackIds { get; set; }

        /// <summary>
        /// Коллекция персональных плейлистов пользователя.
        /// Ключ — строковое название плейлиста, значение — упорядоченный список идентификаторов треков.
        /// </summary>
        public Dictionary<string, List<int>> Playlists { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="User"/> с созданием пустых коллекций.
        /// </summary>
        public User()
        {
            // Инициализация коллекций для предотвращения исключений типа NullReferenceException
            LikedTrackIds = new List<int>();
            Playlists = new Dictionary<string, List<int>>();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="User"/> с заданными параметрами.
        /// </summary>
        /// <param name="id">Уникальный идентификатор.</param>
        /// <param name="name">Имя пользователя.</param>
        public User(int id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Добавляет идентификатор трека в список предпочтений пользователя.
        /// </summary>
        /// <param name="trackId">Идентификатор трека для добавления.</param>
        public void LikeTrack(int trackId)
        {
            // Проверка на наличие трека в списке для обеспечения уникальности предпочтений
            if (!LikedTrackIds.Contains(trackId))
            {
                LikedTrackIds.Add(trackId);
            }
        }

        /// <summary>
        /// Создает новый плейлист или обновляет существующий.
        /// </summary>
        /// <param name="playlistName">Наименование коллекции.</param>
        /// <param name="trackIds">Список идентификаторов треков, входящих в коллекцию.</param>
        public void SavePlaylist(string playlistName, List<int> trackIds)
        {
            // Метод обеспечивает атомарное обновление или создание именованной подборки треков
            if (Playlists.ContainsKey(playlistName))
            {
                Playlists[playlistName] = new List<int>(trackIds);
            }
            else
            {
                Playlists.Add(playlistName, new List<int>(trackIds));
            }
        }

        /// <summary>
        /// Возвращает текстовое представление объекта пользователя.
        /// </summary>
        /// <returns>Строка, содержащая имя пользователя и общее количество его предпочтений.</returns>
        public override string ToString()
        {
            return $"{Name} (Лайков: {LikedTrackIds.Count}, Плейлистов: {Playlists.Count})";
        }
    }
}