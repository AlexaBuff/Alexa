using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleITunes
{
   public class Model : DbContext
    {
        public DbSet<Album> Albums { get; set; }      

        public string DbPath { get; private set; }       

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {

            
            DbPath = $"{AppDomain.CurrentDomain.BaseDirectory}blogging.db";
            options.UseSqlite($"Data Source={DbPath}");
        }
    }

    public class Album
    {
        public int Id { get; set; }
        public long AlbumArtistID { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }    

        public DateTime DateRelise { get; set; }

       
    }

   
}