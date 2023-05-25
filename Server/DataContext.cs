using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Appartment.DataContext
{
    public class AppartContext : DbContext
    {
        public AppartContext()
        {
        }

        public DbSet<AppartmentTable> Appartment { get; set; }
        public DbSet<AppartPlayerTable> AppartPlayer { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=fivem;user=root;password=");
        }
    }

    [Table("appartment")]
    public class AppartmentTable
    {
        [Key]
        [Column("id_appart")]
        public int Id_appart { get; set; }
        [Column("doors_position")]
        public string Doors_position { get; set; }
    }

    [Table("appart_player")]
    public class AppartPlayerTable
    {
        [Key]
        [Column("id_player")]
        public int Id_player { get; set; }
        [Column("id_appart")]
        public int Id_Appart { get; set; } // Cle etrangere avec la colonne id_appart de la table appartment
        [Column("isOpen")]
        public bool isOpen { get; set; }
        [Column("chest")]
        public string Chest { get; set; }
    }
}
