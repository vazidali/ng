using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using myapi.Entities;
using myapi.Models;
using myapi.Models.DB;

namespace myapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            var builder = new ConfigurationBuilder()
       .SetBasePath(env.ContentRootPath)
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
       .AddEnvironmentVariables();
            Configuration = builder.Build();

        }
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequiredUniqueChars =0;
                options.Password.RequireUppercase = false;
            });

            services.AddCors();

            //services.AddDbContext<TimeVaultNewContext>();
            //services.AddScoped<DbContext>(sp => sp.GetService<TimeVaultNewContext>());
            //container = services.BuildServiceProvider();


           // services.AddDbContext<TimeVaultNewContext>(opts => opts.UseSqlServer(Configuration["Data:UCASAppDatabase:ConnectionString"]));
            services.AddDbContext<TimeVaultNewContext>(options =>
                                                          options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));



            //    //===== Add our DbContext ========
            //    //services.AddDefaultIdentity<AppUser>();
            //    //services.AddDbContext<ApplicationDbContext>();

            //     //===== Add Identity ========
            //     //services.AddIdentity<IdentityUser, IdentityRole>()
            //    services.AddIdentity<AppUser, IdentityRole>()
            //      .AddEntityFrameworkStores<ApplicationDbContext>()
            //     .AddDefaultTokenProviders();

            //    services.AddDefaultIdentity<AppUser>()
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultUI()
            //.AddDefaultTokenProviders();

            //services.AddDefaultIdentity<AppUser>();
            services.AddDbContext<IndentityDbCOntext>(options =>
                                                                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AppUser, IdentityRole>()
                                                                .AddEntityFrameworkStores<IndentityDbCOntext>()
                                                                .AddDefaultUI()
                                                                .AddRoles<IdentityRole>()
                                                                .AddRoleManager<RoleManager<IdentityRole>>()
                                                                .AddEntityFrameworkStores<IndentityDbCOntext>()
                                                                .AddDefaultTokenProviders();




            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });


            services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                );
        });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

       

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IndentityDbCOntext dbContext, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseIdentity();
            app.UseCors(
        options => options.WithOrigins("http://domain:444/").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
            //        http://localhost:4200app.UseCors(builder => builder 
            //.AllowAnyOrigin()
            //.AllowAnyMethod()
            //.AllowAnyHeader()
            //.AllowCredentials());
            app.UseCors();
            app.UseMvc();
            dbContext.Database.EnsureCreated();
           //Scaffold-DbContext "Server=abc;Database=db1;UID=sa;PWD=123;" 'Microsoft.EntityFrameworkCore.SqlServer' -OutputDir Models/DB -Verbose -force
        }



    }
}
