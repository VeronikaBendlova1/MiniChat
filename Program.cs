using Microsoft.EntityFrameworkCore;
using MiniChatApp.Data;
using MiniChatApp.Hubs;

namespace MiniChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Z�sk�n� portu z environment�ln� prom�nn� "PORT"
            // Railway nastavuje port zde, pokud nen� nastavena, pou�ije 5000 jako v�choz�
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

            // P�id�n� slu�eb do kontejneru DI (Dependency Injection)
            builder.Services.AddControllersWithViews();

            // Registrace SignalR slu�by pro realtime komunikaci
            builder.Services.AddSignalR();

            // P�ipojen� k datab�zi PostgreSQL pomoc� connection stringu z appsettings.json
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Vytvo�en� aplikace
            var app = builder.Build();

            // Nastaven� aplikace, aby naslouchala na zvolen�m portu (v�ech IP adres�ch)
            app.Urls.Add($"http://*:{port}");

            // Povolen� statick�ch soubor� (nap�. CSS, JS, obr�zky)
            app.UseStaticFiles();

            // Mapa pro SignalR hub na URL /chatHub
            app.MapHub<ChatHub>("/chatHub");

            // Pokud nejde o v�vojov� prost�ed�, nastav�me chyt�n� v�jimek a HSTS
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // P�esm�rov�n� HTTP na HTTPS
            app.UseHttpsRedirection();

            // Nastaven� sm�rov�n� HTTP po�adavk�
            app.UseRouting();

            // Povolen� autorizace (pokud ji n�kde pou��v�)
            app.UseAuthorization();

            // Nastaven� v�choz� trasy pro MVC kontrolery
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Chat}/{action=Index}/{id?}");

            // Spu�t�n� aplikace
            app.Run();
        }
    }
}

