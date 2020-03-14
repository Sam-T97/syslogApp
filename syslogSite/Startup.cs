using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SyslogShared;


namespace syslogSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection"),mySqlOptions => {
                mySqlOptions.ServerVersion(new Version(8, 0, 17), ServerType.MySql);
                mySqlOptions.MigrationsAssembly("syslogSite");
                    }));
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //.AddEntityFrameworkStores<ApplicationDbContext>();
                services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.AddRazorPages();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            CreateRoles(serviceProvider);
        }

        public void CreateRoles(IServiceProvider serviceProvider)
        {
            string[] roles = new[] {"Admin", "Engineer", "Standard", "Has2FA"};
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            foreach (string role in roles)
            {
                Task<bool> hasRole = roleManager.RoleExistsAsync(role);
                hasRole.Wait();
                if (!hasRole.Result)
                {
                    var roleResult = roleManager.CreateAsync(new IdentityRole(role));
                    roleResult.Wait();
                }
            }

            Task<IdentityUser> testUser = userManager.FindByEmailAsync("test@test.com");
            testUser.Wait();
            if (testUser.Result == null)
            {
                IdentityUser admin = new IdentityUser
                {
                    Email = "test@test.com",
                    UserName = "test@test.com",
                    EmailConfirmed = true,
                    NormalizedUserName = "TEST"
                };
                Task<IdentityResult> newUser = userManager.CreateAsync(admin, "P@ssword1!");
                newUser.Wait();
                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(admin, "Admin");
                    newUserRole.Wait();
                }
            }

            testUser = userManager.FindByEmailAsync("standardtest@test.com");
            testUser.Wait();
            if (testUser.Result == null)
            {
                IdentityUser standard = new IdentityUser
                {
                    Email = "standardtest@test.com",
                    UserName = "standardtest@test.com",
                    EmailConfirmed = true,
                    NormalizedUserName = "STANDARDTEST@TEST.COM"
                };
                Task<IdentityResult> newUser = userManager.CreateAsync(standard, "Password1.");
                newUser.Wait();
                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(standard, "Standard");
                    newUserRole.Wait();
                }
            }
        }
    }
}
