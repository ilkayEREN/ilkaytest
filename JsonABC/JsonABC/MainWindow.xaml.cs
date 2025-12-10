using Microsoft.AspNetCore.SignalR.Client; // Kütüphaneyi yükleyince burası düzelecek
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JsonABC
{
    // Listede göstereceğimiz veri modeli
    public class UserInfo
    {
        public string Username { get; set; }
        public int Rank { get; set; }
        public double LatencyMs { get; set; }
        public string IpAddress { get; set; }
    }

    public partial class MainWindow : Window
    {
        // Bağlantı nesnesini sınıfın en tepesinde tanımlıyoruz ki her yerden erişebilelim
        HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSignalR(); // Uygulama açılınca bağlantıyı kur
        }

        // SignalR Ayarları ve Bağlantı
        private async void InitializeSignalR()
        {
            // BURASI ÖNEMLİ: Kendi IP adresini veya localhost'u yaz
            string url = "http://localhost:5000/apphub";

            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            // 1. Sunucu "PingRequest" gönderirse (Yarış başlıyor)
            connection.On<DateTime>("PingRequest", async (serverTime) =>
            {
                // Hemen cevap ver (Pong)
                await connection.InvokeAsync("PongResponse", serverTime);
            });

            // 2. Yarış bittiğinde sonuçları al
            connection.On<List<UserInfo>>("RaceFinished", (results) =>
            {
                Dispatcher.Invoke(() =>
                {
                    // XAML tarafında ResultList adında bir ListView olmalı
                    if (ResultList != null)
                    {
                        ResultList.ItemsSource = results;
                    }
                    MessageBox.Show("Yarış Bitti! Sonuçlar listelendi.");
                });
            });

            try
            {
                await connection.StartAsync();
                // Rastgele bir isimle giriş yap
                string randomName = "PC_" + new Random().Next(100, 999);
                await connection.InvokeAsync("Login", randomName);
            }
            catch
            {
                // Sunucu kapalıysa uygulama çökmesin diye boş geçiyoruz
            }
        }

        // HIZ YARIŞI BUTONU
        private async void BtnRace_Click(object sender, RoutedEventArgs e)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("StartRace");
                MessageBox.Show("Yarış başlatıldı! 3 saniye sonra sonuçlar gelecek.");
            }
            else
            {
                MessageBox.Show("Sunucuya bağlı değil! Lütfen Server uygulamasını çalıştırın.");
            }
        }

        // ESKİ JSON SIRALAMA BUTONU
        private void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputJson.Text)) return;
            try
            {
                var parsed = JToken.Parse(InputJson.Text);
                SortJson(parsed);
                OutputJson.Text = parsed.ToString(Formatting.Indented);
            }
            catch { MessageBox.Show("Geçersiz JSON formatı."); }
        }

        // TEMİZLE BUTONU
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InputJson.Clear();
            OutputJson.Clear();
        }

        // Recursive Sıralama Mantığı
        private void SortJson(JToken token)
        {
            if (token is JObject jObj)
            {
                var props = jObj.Properties().ToList();
                jObj.RemoveAll();
                foreach (var prop in props.OrderBy(p => p.Name))
                {
                    SortJson(prop.Value);
                    jObj.Add(prop);
                }
            }
            else if (token is JArray jArray)
            {
                foreach (var item in jArray) SortJson(item);
            }
        }
    }
}