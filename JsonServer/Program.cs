using JsonServer.Hubs;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// 1. DÝNLEME AYARI: Tüm aðlara aç (0.0.0.0)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// 2. SERVÝSLER (Sade ve Net)
builder.Services.AddControllers();
builder.Services.AddSignalR();

// 3. ÝZÝNLER
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.SetIsOriginAllowed(_ => true)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

var app = builder.Build();

// 4. MIDDLEWARE (Swagger yok, tarayýcý yok)
app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<AppHub>("/apphub");

// --- OTOMATÝK IP BULMA VE BÝLGÝLENDÝRME EKRANI ---
// Senin IP adresini sistemden otomatik çeker
string yerelIP = "Bulunamadi";
try
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            yerelIP = ip.ToString();
            break; // Ýlk IPv4 adresini al (Genelde Wi-Fi veya Ethernet IP'sidir)
        }
    }
}
catch { }

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("==================================================");
Console.WriteLine("           HIZ YARIÞI SUNUCUSU (v3.5)             ");
Console.WriteLine("==================================================");
Console.WriteLine("");
Console.WriteLine("  [ DURUM ]  : Sunucu Aktif ve Dinliyor.");
Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("  [ OFÝSTEKÝ ARKADAÞINA VERECEÐÝN IP ]:");
Console.WriteLine($"  ->  {yerelIP}");
Console.WriteLine("");
Console.WriteLine("  [ NGROK KULLANIYORSAN ]: ");
Console.WriteLine($"  ->  ngrok http http://localhost:5000  komutunu kullan.");
Console.WriteLine("");
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("==================================================");
Console.WriteLine("Loglar aþaðýda akacak...");
Console.WriteLine("");

app.Run();