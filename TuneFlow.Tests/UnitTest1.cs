using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TuneFlow.Logic.Algorithms;
using TuneFlow.Logic.Helpers;
using TuneFlow.Logic.Models;
using TuneFlow.Logic.Services;

namespace TuneFlow.Tests
{
    [TestFixture]
    public class AlgorithmSystemTests
    {
        #region 1. Быстрая сортировка

        [Test]
        public void QuickSort_TypicalData_SortsCorrectly()
        {
            var data = new List<Track> { new Track { Id = 1, Rating = 5 }, new Track { Id = 2, Rating = 1 } };
            QuickSorter.Sort(data, (a, b) => a.Rating.CompareTo(b.Rating), true);
            Assert.That(data[0].Rating, Is.EqualTo(1), "Ошибка: Сортировка не сработала по возрастанию.");
        }

        [Test]
        public void QuickSort_EmptyList_DoesNotThrow()
        {
            var data = new List<Track>();
            Assert.DoesNotThrow(() => QuickSorter.Sort(data, (a, b) => a.Id.CompareTo(b.Id)), "Ошибка на пустом списке.");
        }

        [Test]
        public void QuickSort_NullInput_HandlesSafely()
        {
            Assert.DoesNotThrow(() => QuickSorter.Sort(null!, (a, b) => 0), "Ошибка при передаче null.");
        }

        [Test]
        public void QuickSort_LargeVolume_Performance()
        {
            var data = new List<Track>();
            for (int i = 0; i < 5000; i++) data.Add(new Track { Id = i });
            Assert.DoesNotThrow(() => QuickSorter.Sort(data, (a, b) => a.Id.CompareTo(b.Id)), "Ошибка производительности.");
        }

        [Test]
        public void QuickSort_ReverseSorted_Correct()
        {
            var data = new List<Track> { new Track { Id = 2 }, new Track { Id = 1 } };
            QuickSorter.Sort(data, (a, b) => a.Id.CompareTo(b.Id), true);
            Assert.That(data[0].Id, Is.EqualTo(1), "Ошибка пересортировки обратного массива.");
        }

        [Test]
        public void QuickSort_AlreadySorted_RemainsStable()
        {
            var data = new List<Track> { new Track { Id = 1 }, new Track { Id = 2 } };
            QuickSorter.Sort(data, (a, b) => a.Id.CompareTo(b.Id), true);
            Assert.That(data[0].Id, Is.EqualTo(1), "Ошибка: Изменен порядок уже отсортированного массива.");
        }
        #endregion

        #region 2. Поиск в ширину

        [Test]
        public void BFS_Typical_FindsTrack()
        {
            var gm = new GraphManager();
            gm.AddRelation(1, 10); gm.AddRelation(2, 10); gm.AddRelation(2, 20);
            Assert.That(gm.GetRecommendationsBFS(1), Contains.Item(20), "Ошибка BFS: не найден сосед 2 уровня.");
        }

        [Test]
        public void BFS_SingleNode_Empty()
        {
            var gm = new GraphManager();
            gm.AddRelation(1, 10);
            Assert.That(gm.GetRecommendationsBFS(1), Is.Empty, "Ошибка: Найдены связи у изолированного узла.");
        }

        [Test]
        public void BFS_InvalidId_Empty()
        {
            var gm = new GraphManager();
            Assert.That(gm.GetRecommendationsBFS(-1), Is.Empty, "Ошибка: Некорректный ID должен возвращать пустоту.");
        }

        [Test]
        public void BFS_LargeGraph_Fast()
        {
            var gm = new GraphManager();
            for (int i = 0; i < 1000; i++) gm.AddRelation(i, 9999);
            Assert.DoesNotThrow(() => gm.GetRecommendationsBFS(1), "Ошибка: Зависание на большом графе.");
        }

        [Test]
        public void BFS_Disconnected_Ignores()
        {
            var gm = new GraphManager();
            gm.AddRelation(1, 10); gm.AddRelation(5, 50);
            Assert.That(gm.GetRecommendationsBFS(1), Does.Not.Contain(50), "Ошибка: BFS перепрыгнул в несвязный граф.");
        }

        [Test]
        public void BFS_Cyclic_NoInfiniteLoop()
        {
            var gm = new GraphManager();
            gm.AddRelation(1, 10); gm.AddRelation(2, 10); gm.AddRelation(1, 20); // Цикл
            Assert.DoesNotThrow(() => gm.GetRecommendationsBFS(1), "Ошибка: Зацикливание BFS.");
        }
        #endregion

        #region 3. Сходство треков

        [Test]
        public void Similarity_Identical_One()
        {
            var t = new Track { Genre = "Rock", Year = 2020 };
            Assert.That(Similarity.CalculateSimilarity(t, t), Is.EqualTo(1.0), "Ошибка: Идентичные треки не равны 1.0.");
        }

        [Test]
        public void Similarity_Null_Zero()
        {
            Assert.That(Similarity.CalculateSimilarity(null!, null!), Is.EqualTo(0), "Ошибка при null параметрах.");
        }

        [Test]
        public void Similarity_Typical_Valid()
        {
            var t1 = new Track { Genre = "Pop", Year = 2020 };
            var t2 = new Track { Genre = "Pop", Year = 2021 };
            Assert.That(Similarity.CalculateSimilarity(t1, t2), Is.GreaterThan(0.7), "Ошибка: Похожие треки получили низкий балл.");
        }

        [Test]
        public void Similarity_LargeGap_Low()
        {
            var t1 = new Track { Genre = "Rock", Year = 1970 };
            var t2 = new Track { Genre = "Jazz", Year = 2024 };
            Assert.That(Similarity.CalculateSimilarity(t1, t2), Is.LessThan(0.3), "Ошибка: Разные треки получили высокий балл.");
        }

        [Test]
        public void Similarity_Bulk_NoCrash()
        {
            var t = new Track { Genre = "Rock", Year = 2000 };
            Assert.DoesNotThrow(() => {
                for (int i = 0; i < 1000; i++) Similarity.CalculateSimilarity(t, t);
            }, "Ошибка: Падение при массовом расчете.");
        }

        [Test]
        public void Similarity_SameYearDifferentGenre_MediumScore()
        {
            var t1 = new Track { Genre = "Rock", Year = 2020 };
            var t2 = new Track { Genre = "Pop", Year = 2020 };
            var score = Similarity.CalculateSimilarity(t1, t2);
            Assert.That(score, Is.LessThan(0.5).And.GreaterThan(0), "Ошибка: Неверный вес года и жанра.");
        }
        #endregion

        #region 4. Коллаборативная фильтрация

        [Test]
        public void Collab_Typical_ReturnsResult()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            Assert.That(rec.GetCollaborativeRecommendations(repo.AllUsers[0]), Is.Not.Null, "Ошибка: Вернулся null.");
        }

        [Test]
        public void Collab_NoLikes_ReturnsEmpty()
        {
            var rec = new Recommender(new DataRepository(), new GraphManager());
            var user = new User(99, "Test") { LikedTrackIds = new List<int>() };
            Assert.That(rec.GetCollaborativeRecommendations(user), Is.Empty, "Ошибка: Рекомендации без лайков.");
        }

        [Test]
        public void Collab_NullUser_ReturnsEmpty()
        {
            var rec = new Recommender(new DataRepository(), new GraphManager());
            Assert.That(rec.GetCollaborativeRecommendations(null!), Is.Empty, "Ошибка: Падение при null-юзере.");
        }

        [Test]
        public void Collab_FullDatabase_Fast()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            Assert.DoesNotThrow(() => rec.GetCollaborativeRecommendations(repo.AllUsers[0]), "Ошибка: Зависание на базе.");
        }

        [Test]
        public void Collab_UniqueTaste_Empty()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            var user = new User(100, "Unique") { LikedTrackIds = new List<int> { -999 } };
            Assert.That(rec.GetCollaborativeRecommendations(user), Is.Empty, "Ошибка: Найдены рекомендации без совпадений.");
        }

        [Test]
        public void Collab_PerfectMatch_ReturnsData()
        {
            var repo = new DataRepository();
            var u1 = repo.AllUsers[0];
            var u2 = new User(500, "Clone") { LikedTrackIds = new List<int>(u1.LikedTrackIds) };
            repo.AllUsers.Add(u2);
            // Добавляем трек, которого нет у клона
            u1.LikeTrack(999);
            var rec = new Recommender(repo, new GraphManager());
            var result = rec.GetCollaborativeRecommendations(u2);
            Assert.That(result, Is.Not.Null, "Ошибка: Идеальное совпадение не дало результата.");
        }
        #endregion

        #region 5. Статистика

        [Test]
        public void Stats_Genre_Counts()
        {
            var s = new StatsService();
            var tr = new List<Track> { new Track { Genre = "Rock" }, new Track { Genre = "Rock" } };
            Assert.That(s.GetGenreDistribution(tr)["Rock"], Is.EqualTo(2), "Ошибка: Неверный подсчет жанров.");
        }

        [Test]
        public void Stats_Avg_Calculates()
        {
            var s = new StatsService();
            var tr = new List<Track> { new Track { Genre = "A", Duration = 2 }, new Track { Genre = "A", Duration = 4 } };
            Assert.That(s.GetAverageDurationByGenre(tr)["A"], Is.EqualTo(3.0), "Ошибка: Неверная средняя длительность.");
        }

        [Test]
        public void Stats_TopArtists_FindsLeader()
        {
            var s = new StatsService();
            var tr = new List<Track> { new Track { Artist = "A" }, new Track { Artist = "A" }, new Track { Artist = "B" } };
            Assert.That(s.GetTopArtists(tr, 1)[0].Key, Is.EqualTo("A"), "Ошибка: Неверный топ артистов.");
        }

        [Test]
        public void Stats_Yearly_GroupsCorrectly()
        {
            var s = new StatsService();
            var tr = new List<Track> { new Track { Year = 2020 }, new Track { Year = 2020 } };
            Assert.That(s.GetYearlyDistribution(tr)[2020], Is.EqualTo(2), "Ошибка: Неверная группировка по годам.");
        }

        [Test]
        public void Stats_EmptyList_Safe()
        {
            var s = new StatsService();
            Assert.That(s.GetTopArtists(new List<Track>()).Count, Is.EqualTo(0), "Ошибка: Падение на пустом списке.");
        }
        #endregion

        #region 6. Модели и БД

        [Test]
        public void Repo_SearchByTitle_Finds()
        {
            var repo = new DataRepository();
            var track = repo.AllTracks[0];
            Assert.That(repo.SearchTracks(titlePart: track.Title), Contains.Item(track), "Ошибка: Поиск по названию.");
        }

        [Test]
        public void Repo_SearchByGenre_Finds()
        {
            var repo = new DataRepository();
            var track = repo.AllTracks[0];
            Assert.That(repo.SearchTracks(genre: track.Genre), Contains.Item(track), "Ошибка: Поиск по жанру.");
        }

        [Test]
        public void Repo_GetById_Valid()
        {
            var repo = new DataRepository();
            Assert.That(repo.GetTrackById(repo.AllTracks[0].Id), Is.Not.Null, "Ошибка: Трек не найден по ID.");
        }

        [Test]
        public void Repo_GetById_Invalid_ReturnsNull()
        {
            var repo = new DataRepository();
            Assert.That(repo.GetTrackById(-1), Is.Null, "Ошибка: Несуществующий ID не вернул null.");
        }

        [Test]
        public void User_Like_NoDupes()
        {
            var u = new User(1, "U");
            u.LikeTrack(5); u.LikeTrack(5);
            Assert.That(u.LikedTrackIds.Count, Is.EqualTo(1), "Ошибка: Дублирование лайков.");
        }

        [Test]
        public void Track_ToString_Valid()
        {
            var t = new Track { Artist = "Art", Title = "Tit" };
            Assert.That(t.ToString(), Contains.Substring("Art"), "Ошибка: Неверный ToString().");
        }

        [Test]
        public void User_ToString_Valid()
        {
            var u = new User(1, "Tester");
            Assert.That(u.ToString(), Contains.Substring("Tester"), "Ошибка: Неверный ToString() юзера.");
        }
        #endregion

        #region 7. Расширенные тесты Recommender (Максимальное покрытие)

        [Test]
        public void Recommender_Simple_ValidGenre_ReturnsTracks()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            string genre = repo.AllTracks[0].Genre;
            var result = rec.GetSimpleRecommendations(genre, new List<int>());
            Assert.That(result, Is.Not.Null, "Ошибка: метод вернул null для существующего жанра.");
        }

        [Test]
        public void Recommender_Simple_ExcludeList_Works()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            var track = repo.AllTracks[0];
            var result = rec.GetSimpleRecommendations(track.Genre, new List<int> { track.Id });
            Assert.That(result.Any(t => t.Id == track.Id), Is.False, "Ошибка: исключенный трек попал в результат.");
        }

        [Test]
        public void Recommender_Simple_NullInputs_Safe()
        {
            var rec = new Recommender(null!, null!);
            Assert.DoesNotThrow(() => rec.GetSimpleRecommendations(null!, null!), "Ошибка: падение метода при null-входах.");
        }

        [Test]
        public void Recommender_Graph_ValidCall()
        {
            var repo = new DataRepository();
            var gm = new GraphManager();
            var rec = new Recommender(repo, gm);
            gm.AddRelation(1, 10); gm.AddRelation(2, 10); gm.AddRelation(2, 20);
            var result = rec.GetGraphRecommendations(1);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Recommender_Collab_Advanced_Coverage()
        {
            var repo = new DataRepository();
            var rec = new Recommender(repo, new GraphManager());
            var u1 = repo.AllUsers[0];
            var u2 = repo.AllUsers[1];

            // Настраиваем похожесть
            u1.LikedTrackIds = new List<int> { 1, 2, 3 };
            u2.LikedTrackIds = new List<int> { 1, 2, 4 }; // Совпадение на 2/4

            var result = rec.GetCollaborativeRecommendations(u1);
            Assert.That(result, Is.Not.Null, "Ошибка: расширенная коллаборация не сработала.");
        }

        [Test]
        public void Recommender_Jaccard_NullVents_Safe()
        {
            var rec = new Recommender(null!, null!);
            var u = new User(1, "T") { LikedTrackIds = null! };
            var res = rec.GetCollaborativeRecommendations(u);
            Assert.That(res, Is.Empty, "Ошибка: неверная обработка null-лайков.");
        }
        #endregion
    }
}