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

            // P�id�n� slu�eb do kontejneru Dependency Injection (DI)
            builder.Services.AddControllersWithViews();

            // Registrace SignalR slu�by pro real-time komunikaci (websockets apod.)
            builder.Services.AddSignalR();

            // Konfigurace Entity Framework Core pro p�ipojen� k PostgreSQL datab�zi
            // Connection string se na��t� z appsettings.json v sekci "ConnectionStrings:DefaultConnection"
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Vytvo�en� instance webov� aplikace se v�emi nakonfigurovan�mi slu�bami
            var app = builder.Build();

            // Nastaven�, na jak�m URL a portu aplikace poslouch�
            // Naslouch� na v�ech IP adres�ch a na portu, kter� poskytne Railway p�es env. prom�nnou
            app.Urls.Add($"http://*:{port}");

            // Povolen� serv�rov�n� statick�ch soubor� (CSS, JS, obr�zky)
            app.UseStaticFiles();

            // Mapa (endpoint) pro SignalR hub - zde m��e klient p�istupovat k /chatHub pro realtime komunikaci
            app.MapHub<ChatHub>("/chatHub");

            // V produk�n�m re�imu p�esm�rov�n� na vlastn� chybovou str�nku a zapnut� HSTS (bezpe�nost)
            if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var errorFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var exception = errorFeature?.Error;

            if (exception != null)
            {
                Console.WriteLine($"Exception handled: {exception}");
            }

            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Internal Server Error");
        });
    });

    app.UseHsts();
}


            // P�esm�rov�n� HTTP na HTTPS
            // Pokud chce�, m��e� tady pro Railway proxy HTTPS vypnout (nap�. zakomentovat), pokud zp�sobuje probl�my
            app.UseHttpsRedirection();

            // Zapnut� sm�rov�n� po�adavk� (routing)
            app.UseRouting();

            // Zapnut� autorizace (pokud pou��v� n�jak� zabezpe�en�)
            app.UseAuthorization();

            // Nastaven� v�choz� MVC trasy: pokud nen� jinak zad�no, pou�ije se kontroler Chat a akce Index
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Chat}/{action=Index}/{id?}");

            // Spu�t�n� aplikace
            app.Run();
        }
    }
}
