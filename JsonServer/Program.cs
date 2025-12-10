using JsonServer.Hubs; 

var builder = WebApplication.CreateBuilder(args);

// --- SERVÝSLER ---
builder.Services.AddControllers();
builder.Services.AddSignalR(); // <--- ÝÞTE BU: Anlýk haberleþme servisi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS AYARI: Baþka bilgisayarlarýn (arkadaþlarýnýn) baðlanmasýna izin ver
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.SetIsOriginAllowed(_ => true) // Her yerden gelen isteði kabul et
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

var app = builder.Build();

// --- UYGULAMA AYARLARI ---
app.UseCors("AllowAll"); // Yukarýdaki izni aktif et
app.MapControllers();

// Bu satýr "AppHub" sýnýfýný baðlar.
app.MapHub<AppHub>("/apphub");

app.Run();