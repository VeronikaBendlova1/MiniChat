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

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR(); // Registrace slu�by

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            // MUS� B�T P�ED Build();

            var app = builder.Build();

            // P�id�n� podpory pro statick� soubory (css, js, obr�zky atd.)
            app.UseStaticFiles();

            app.MapHub<ChatHub>("/chatHub");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Chat}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

