using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using GoVip_FileUploader.Data.Models;

namespace GoVip_FileUploader.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().HasMany(t => t.Tokens).WithOne(i => i.User);

            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<Item>().Property(i => i.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Token>().ToTable("Tokens");
            modelBuilder.Entity<Token>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Token>().HasOne(u => u.User).WithMany(t => t.Tokens);
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Token> Tokens { get; set; }
    }
}
