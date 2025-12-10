using Microsoft.AspNetCore.SignalR;

namespace JsonServer.Hubs
{
    // Yarışmacı Kartı
    public class UserInfo
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public int Rank { get; set; }         // Sıralama
        public double LatencyMs { get; set; } // Gecikme
    }

    public class AppHub : Hub
    {
        private static List<UserInfo> ConnectedUsers = new();
        private static List<UserInfo> RaceResults = new();
        private static bool IsRaceActive = false;

        // Biri Bağlandığında
        public async Task Login(string username)
        {
            var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
            var user = new UserInfo { ConnectionId = Context.ConnectionId, Username = username, IpAddress = ip };
            ConnectedUsers.Add(user);
            await Clients.All.SendAsync("Log", $"{username} ({ip}) oyuna katıldı.");
        }

        // YARIŞI BAŞLAT (Sen tuşa basınca)
        public async Task StartRace()
        {
            if (IsRaceActive) return;
            IsRaceActive = true;
            RaceResults.Clear();

            // Seni bul ve 1. sıraya koy (Başlatan sensin, gecikme 0)
            var initiator = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (initiator != null)
            {
                initiator.Rank = 1;
                initiator.LatencyMs = 0;
                RaceResults.Add(initiator);
            }

            // Diğer herkese "PING!" diye bağır. (Saat bilgisini de yolla)
            await Clients.AllExcept(Context.ConnectionId).SendAsync("PingRequest", DateTime.UtcNow);

            // 3 Saniye bekle, herkesin cevabını topla
            await Task.Delay(3000);

            IsRaceActive = false;
            // Sonuçları sırala ve yayınla
            await Clients.All.SendAsync("RaceFinished", RaceResults.OrderBy(r => r.Rank).ToList());
        }

        // MİLLETİN CEVABI (Pong)
        public async Task PongResponse(DateTime serverTime)
        {
            if (!IsRaceActive) return;

            // Gecikme Hesabı: Şu an - Server'ın mesaj attığı an
            var latency = (DateTime.UtcNow - serverTime).TotalMilliseconds;
            var user = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

            if (user != null)
            {
                user.LatencyMs = Math.Round(latency, 2);
                // O an listede kaç kişi varsa, bir sonraki sıra benimdir
                user.Rank = RaceResults.Count + 1;
                RaceResults.Add(user);
            }
        }
    }
}