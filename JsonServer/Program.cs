using JsonServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- KRÝTÝK AYAR: HERKESE AÇIL ---
// Bu satýr olmazsa arkadaþýn baðlanamaz.
// Uygulamayý 5000 portuna ve tüm IP'lere sabitliyoruz.
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// --- SERVÝSLER ---
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS AYARI (Ýzinler)
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.SetIsOriginAllowed(_ => true)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

var app = builder.Build();

// --- UYGULAMA AYARLARI ---

// Release modunda bile Swagger (API Dokümaný) görünsün
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<AppHub>("/apphub");

app.Run();