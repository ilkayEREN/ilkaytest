using JsonServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. IP AYARI: Herkese açýk (Ayný kalýyor)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// 2. SERVÝSLER (Swagger satýrlarýný sildik)
builder.Services.AddControllers();
builder.Services.AddSignalR();

// 3. ÝZÝNLER (CORS - Ayný kalýyor)
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.SetIsOriginAllowed(_ => true)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

var app = builder.Build();

// 4. UYGULAMA AYARLARI (Swagger UI middleware'lerini sildik)
app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<AppHub>("/apphub"); // Ana baðlantý noktasý burasý

// Sunucu baþladýðýnda konsola bilgi yazsýn
Console.WriteLine("Server Baslatildi!");
Console.WriteLine("Baglanti Adresi: http://[SENIN_IP_ADRESIN]:5000/apphub");
Console.WriteLine("Client uygulamasindan bu IP ile baglanabilirsiniz.");

app.Run();