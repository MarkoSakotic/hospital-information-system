using EntityProject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryProject.Context
{
    public class HISContext : IdentityDbContext<ApiUser, IdentityRole, string>
    {

        public HISContext(DbContextOptions<HISContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>().ToTable("Patient");
            modelBuilder.Entity<Doctor>().ToTable("Doctor");
            modelBuilder.Entity<Technician>().ToTable("Technician");
            modelBuilder.Entity<Appointment>().ToTable("Appointment");


            modelBuilder.Entity<Appointment>()
           .HasOne<Patient>(s => s.Patient)
           .WithMany(g => g.Appointments)
           .HasForeignKey(s => s.PatientId);


            modelBuilder.Entity<Appointment>()
           .HasOne<Doctor>(s => s.Doctor)
           .WithMany(g => g.Appointments)
           .HasForeignKey(s => s.DoctorId);


            modelBuilder
            .Entity<Patient>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        }
    }
}
