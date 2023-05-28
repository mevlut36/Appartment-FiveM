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

        public DbSet<PropertyTable> Property { get; set; }
        public DbSet<AppartPlayerTable> AppartPlayer { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=fivem;user=root;password=");
        }
    }

    [Table("property")]
    public class PropertyTable
    {
        [Key]
        [Column("id_property")]
        public int Id_property { get; set; }
        [Column("doors_position")]
        public string Doors_position { get; set; }
    }

    [Table("appart_player")]
    public class AppartPlayerTable
    {
        [Key]
        [Column("id_player")]
        public int Id_player { get; set; }
        [Column("id_property")]
        public int Id_property { get; set; } // Cle etrangere avec la colonne id_property de la table property
        [Column("isOpen")]
        public int isOpen { get; set; }
        [Column("chest")]
        public string Chest { get; set; }
    }
}
