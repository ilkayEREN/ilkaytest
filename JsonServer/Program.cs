using JsonServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- SERVÝSLER ---
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS AYARI
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.SetIsOriginAllowed(_ => true)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

var app = builder.Build();

// --- UYGULAMA AYARLARI ---

// DÝKKAT: if (app.Environment.IsDevelopment()) satýrýný kaldýrdýk!
// Artýk her durumda (Release modunda bile) Swagger açýlacak.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<AppHub>("/apphub");

app.Run();