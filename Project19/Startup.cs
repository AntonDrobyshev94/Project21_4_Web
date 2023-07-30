using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project19.AuthContactApp;
using Project19.ContextFolder;
using Project19.Data;
using Project19.Interfaces;

namespace Project19
{
    /// <summary>
    /// Класс обработки запросов
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get;}  
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Метод, который служит для добавления необходимых сервисов 
        /// в контейнер.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IContactData, ContactDataApi>();

            services.AddMvc();
            services.AddMvc(options => { options.EnableEndpointRouting = false; });

            #region //

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders().AddRoles<IdentityRole>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6; // минимальное количество знаков в пароле

                options.Lockout.MaxFailedAccessAttempts = 10; // количество попыток о блокировки
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.AllowedForNewUsers = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // конфигурация Cookie с целью использования их для хранения авторизации
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.SlidingExpiration = true;
            });

            #endregion
        }

        /// <summary>
        /// Метод настройки конфигурации
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(
                r =>
                {
                    r.MapRoute(name: "default",
                        template: "{controller=MyDefault}/{action=Index}/{id?}");
                });
        }
    }
}
