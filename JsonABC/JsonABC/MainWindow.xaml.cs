using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JsonABC
{
    public class UserInfo
    {
        public string Username { get; set; }
        public int Rank { get; set; }
        public double LatencyMs { get; set; }
        public string IpAddress { get; set; }
    }

    public partial class MainWindow : Window
    {
        HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            // DİKKAT: Artık uygulama açılınca otomatik bağlanmıyoruz.
            // Kullanıcı butona basınca bağlanacak.
        }

        // --- BAĞLAN BUTONU ---
        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ip = TxtServerIp.Text;
            string user = TxtUsername.Text;

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show("Lütfen IP adresi ve Kullanıcı Adı giriniz.");
                return;
            }

            await ConnectToServer(ip, user);
        }

        // --- SUNUCUYA BAĞLANMA MANTIĞI ---
        private async Task ConnectToServer(string ipAddress, string username)
        {
            // Adresi dinamik oluşturuyoruz
            string url = $"http://{ipAddress}:5000/apphub";

            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .WithAutomaticReconnect()
                    .Build();

                // 1. PING GELDİĞİNDE (Yarış Başlıyor)
                connection.On<DateTime>("PingRequest", async (serverTime) =>
                {
                    await connection.InvokeAsync("PongResponse", serverTime);
                });

                // 2. YARIŞ BİTTİĞİNDE
                connection.On<List<UserInfo>>("RaceFinished", (results) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (ResultList != null) ResultList.ItemsSource = results;
                        MessageBox.Show("Yarış Bitti! Sonuçlar güncellendi.");
                    });
                });

                await connection.StartAsync();
                await connection.InvokeAsync("Login", username);

                MessageBox.Show($"Bağlantı Başarılı!\nBağlanılan: {url}");
                LoginPanel.Visibility = Visibility.Collapsed; // Paneli gizle
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BAĞLANTI HATASI!\n\n1. Karşı tarafın sunucuyu (Siyah Ekran) açtığından emin olun.\n2. IP adresini doğru yazdığınızdan emin olun.\n3. Güvenlik duvarına izin verildiğinden emin olun.\n\nHata Detayı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- YARIŞ BAŞLAT BUTONU ---
        private async void BtnRace_Click(object sender, RoutedEventArgs e)
        {
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("StartRace");
                MessageBox.Show("Yarış başlatıldı! Sonuçlar bekleniyor...");
            }
            else
            {
                MessageBox.Show("Sunucuya bağlı değilsiniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // --- JSON SIRALAMA ---
        private void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputJson.Text))
            {
                MessageBox.Show("Lütfen sol tarafa JSON yapıştırın.", "Eksik Veri", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var parsed = JToken.Parse(InputJson.Text);
                SortJson(parsed);
                OutputJson.Text = parsed.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geçersiz JSON!\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InputJson.Clear();
            OutputJson.Clear();
        }

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