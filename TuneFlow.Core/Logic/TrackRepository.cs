using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO; // Используется для корректной обработки CSV (учитывает запятые в кавычках)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneFlow.Core.Models;

namespace TuneFlow.Core.Logic
{
    /// <summary>
    /// Класс отвечает за взаимодействие с файловой системой и загрузку базы треков.
    /// </summary>
    public class TrackRepository
    {
        // Путь к файлу данных в текущей директории приложения
        private readonly string _filePath = "tracks.csv";

        /// <summary>
        /// Читает данные из CSV-файла и формирует список объектов Track.
        /// </summary>
        /// <returns>Список загруженных треков.</returns>
        public List<Track> LoadTracks()
        {
            // Объявление результирующего списка и вспомогательных переменных
            List<Track> tracks = new List<Track>();
            int idCounter = 1;

            // Проверка наличия файла перед началом выполнения логики
            if (!File.Exists(_filePath))
            {
                return tracks; // Возврат пустого списка, если файл отсутствует
            }

            // Блок try-catch для обработки исключительных ситуаций при чтении файла
            try
            {
                // Инициализация объекта TextFieldParser для разбора структуры CSV
                using (TextFieldParser parser = new TextFieldParser(_filePath))
                {
                    // Настройка параметров парсера
                    parser.TextFieldType = FieldType.Delimited; // Формат с разделителями
                    parser.SetDelimiters(",");                  // Установка запятой в качестве разделителя
                    parser.HasFieldsEnclosedInQuotes = true;    // Обработка текстовых полей в кавычках

                    // Пропуск строки заголовков (первая строка файла)
                    if (!parser.EndOfData)
                    {
                        parser.ReadFields();
                    }

                    // Чтение данных до достижения конца файла
                    while (!parser.EndOfData)
                    {
                        // Получение массива полей из текущей строки
                        string[] fields = parser.ReadFields();

                        // Проверка на минимально необходимое количество колонок (6 столбцов)
                        if (fields == null || fields.Length < 6)
                        {
                            continue;
                        }

                        try
                        {
                            // Инициализация нового объекта Track и заполнение свойств с конвертацией типов
                            Track track = new Track();

                            track.Id = idCounter++;
                            track.Artist = fields[0].Trim();
                            track.Title = fields[1].Trim();

                            // Конвертация строковых значений в числовые форматы
                            track.Year = int.Parse(fields[2]);
                            track.Genre = fields[3].Trim();

                            // Обработка рейтинга: замена точки на запятую для корректного double.Parse
                            string ratingRaw = fields[4].Replace(".", ",");
                            track.Rating = double.Parse(ratingRaw);

                            // Перевод длительности из миллисекунд в секунды
                            track.DurationInSeconds = int.Parse(fields[5]) / 1000;

                            // Добавление успешно созданного объекта в общий список
                            tracks.Add(track);
                        }
                        catch (Exception)
                        {
                            // В случае ошибки парсинга конкретной строки, она игнорируется
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке при возникновении проблем с доступом к файлу
                Console.WriteLine("Ошибка при чтении из файла " + _filePath + ": " + ex.Message);
            }

            // Возврат сформированной коллекции данных
            return tracks;
        }
    }
}
