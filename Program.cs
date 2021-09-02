using iTunesSearch.Library;
using iTunesSearch.Library.Models;
using System;
using System.Linq;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Net.Http;

namespace ConsoleITunes
{
    class Program
    {

        static async Task Main(string[] args)
        {

            Model db = new Model();
            db.Database.EnsureCreated();

            iTunesSearchManager search = new iTunesSearchManager();
            string inputArtist = "";
            do
            {
                try
                {
                    Console.WriteLine("     Давайте найдём Вашего артиста на iTunes:(наберите текст и нажмите Enter) ");
                    inputArtist = Console.ReadLine();
                    Console.WriteLine("     поиск...");

                    var artists = search.GetSongArtistsAsync(inputArtist).Result;

                    var artistsSortAsc = from artistsSort in artists.Artists
                                         orderby artistsSort.ArtistName
                                         select artistsSort;
                    int artistsCount = artistsSortAsc.Count();
                    if (artistsCount == 0) Console.WriteLine("      iTunes поискал и не нашёл такого артиста! ");

                    else
                    {
                        Console.WriteLine("     iTunes поискал и нашёл: ");
                        SeachArtist(artistsSortAsc, search, db);
                    }
                }
                catch (Exception e)
                {

                    if (!CheckConnection())
                    {

                        Cache(inputArtist, db);
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                }

            } while (true);

        }

        static async void SeachArtist(IOrderedEnumerable<SongArtist> artistsSortAsc, iTunesSearchManager search, Model model)
        {
            int i = 1;
            foreach (var artist in artistsSortAsc)
            {
                Console.WriteLine($"{i++} Артист: {artist.ArtistName}");
            }

            Console.WriteLine("     Выберите Вашего артиста, указав порядковый номер из списка:(наберите текст и нажмите Enter)");

            bool result = int.TryParse(Console.ReadLine(), out int numberArtist);
            Console.WriteLine("     поиск...");
            if (result == true)
            {
                long choiceArtistId = artistsSortAsc.ElementAt(numberArtist - 1).ArtistId;
                string choiceArtistName = artistsSortAsc.ElementAt(numberArtist - 1).ArtistName;

                try
                {
                    var albums = search.GetAlbumsFromSongAsync(choiceArtistName).Result.Albums.OrderBy(a => a.CollectionName);
                    int y = 1;
                    DateTime dateTime;

                    var OldAlbums = model.Albums.Where(o => o.AlbumArtistID == choiceArtistId);
                    model.Albums.RemoveRange(OldAlbums);

                    Console.WriteLine($"На сервере iTunes артист {choiceArtistName} представлен альбомами: ");
                    foreach (var album in albums)
                    {
                        DateTime.TryParse(album.ReleaseDate, out dateTime);
                        Console.WriteLine($"{y++} " +
                            $"{album.CollectionName}, дата релиза: {dateTime.ToShortDateString()}");
                        model.Albums.Add(new Album { AlbumArtistID = choiceArtistId, ArtistName = choiceArtistName.ToUpper(), AlbumName = album.ArtistName, DateRelise = dateTime });
                    }

                    model.SaveChanges();
                }

                catch (Exception e)
                {

                    if (!CheckConnection())
                    {

                        Cache(choiceArtistName.ToUpper(), model);
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }

                }

            }
            else 
                Console.WriteLine("     Введите порядковый номер: число!");
        }


        static void Cache(string Artist, Model db)
        {
            string inputArtistUpper = Artist.ToUpper();
            var albumsFromDB = db.Albums.Where(a => a.ArtistName == inputArtistUpper).ToList();
            int o = 1;
            foreach (var albumFromDB in albumsFromDB)
            {
                Console.WriteLine($"{o++} {albumFromDB.AlbumName}, дата релиза: {albumFromDB.DateRelise.ToShortDateString()}");
            }


            if (albumsFromDB.Count() == 0) Console.WriteLine("     Ничего не нашли!");

        }

        static bool CheckConnection()
        {
            HttpWebRequest req = WebRequest.Create("https://music.apple.com") as HttpWebRequest;
            HttpWebResponse rsp;

            try
            {
                rsp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse) rsp = e.Response as HttpWebResponse;
                else rsp = null;
                Console.WriteLine("     Соединение с сервером iTunes потеряно..., но всё равно поищем его альбомы...");
            }

            if (rsp != null) return true;
            else return false;
        }

    }
}
