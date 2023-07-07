using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DtoEntityProject.Configuration;
using EntityProject;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RepositoryProject.Context;
using ServiceProject.Implementation;
using ServiceProject.Interface;
using ServiceProject.Seeder;
using ServiceProject.TokenGenerator;
using ServiceProject.Utility;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<HisConfiguration>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));

            services.AddCors(options =>
            {
                //options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            services.AddIdentity<ApiUser, IdentityRole>(o => {
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<HisContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = new JwtSettings();
            Configuration.GetSection(nameof(JwtSettings)).Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            services.ConfigureAuthentication(jwtSettings);
            services.AddAuthorization();
            services.AddDbContext<HisContext>(options => options.UseSqlServer(Configuration.GetConnectionString("HISConnection")));
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.ConfigureSwagger();
            services.AddControllers()
                .AddNewtonsoftJson(cfg => cfg.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IDoctorService, DoctorService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IAppointmentService, AppointmentService>();
            services.AddTransient<ITokenGenerator, TokenGenerator>();

            services.AddTransient<TechnicianSeeder>();
            services.AddTransient<JwtParser>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var swaggerOptions = new DtoEntityProject.Configuration.SwaggerOptions();
            Configuration.GetSection(nameof(DtoEntityProject.Configuration.SwaggerOptions)).Bind(swaggerOptions);

            app.UseSwagger(options => { options.RouteTemplate = swaggerOptions.RouteTemplate; });
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description);
            });

            app.UseRouting();
            app.UseCors("default");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                /*
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });*/
            });
        }
    }
}
