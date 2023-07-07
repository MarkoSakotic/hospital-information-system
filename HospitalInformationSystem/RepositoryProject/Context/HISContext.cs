using EntityProject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryProject.Context
{
    public class HisContext : IdentityDbContext<ApiUser, IdentityRole, string>
    {

        public HisContext(DbContextOptions<HisContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Patient>().ToTable("Patient");
            builder.Entity<Doctor>().ToTable("Doctor");
            builder.Entity<Technician>().ToTable("Technician");
            builder.Entity<Appointment>().ToTable("Appointment");


            builder.Entity<Appointment>()
           .HasOne<Patient>(s => s.Patient)
           .WithMany(g => g.Appointments)
           .HasForeignKey(s => s.PatientId);


            builder.Entity<Appointment>()
           .HasOne<Doctor>(s => s.Doctor)
           .WithMany(g => g.Appointments)
           .HasForeignKey(s => s.DoctorId);


            builder
            .Entity<Patient>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        }
    }
}
