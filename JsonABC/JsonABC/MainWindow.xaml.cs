using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JsonABC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Sırala Butonu Tıklandığında İlkay Seni Selamlar.
        private void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            string rawJson = InputJson.Text;

            // Boşsa işlem yapma
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                MessageBox.Show("Sol tarafı boş bırakma keke.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. JSON'ı oku (Parse et)
                var parsedJson = JToken.Parse(rawJson);

                // 2. Sıralama fonksiyonunu çağır (Recursive)
                SortJson(parsedJson);

                // 3. Düzenlenmiş halini sağ kutuya yaz
                OutputJson.Text = parsedJson.ToString(Formatting.Indented);
            }
            catch (JsonReaderException ex)
            {
                MessageBox.Show($"JSON Format Hatası!\n\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmedik bir hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Temizle Butonu
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InputJson.Clear();
            OutputJson.Clear();
            InputJson.Focus(); // İmleci tekrar giriş kutusuna koy
        }

        // -.-.- Recursive (Özyinelemeli) Sıralama Mantığı -.-.-
        // Bu fonksiyon, iç içe geçmiş objeleri bile bulup sıralar.
        private void SortJson(JToken token)
        {
            if (token is JObject jObj)
            {
                // Özellikleri al
                var properties = jObj.Properties().ToList();

                // Objeyi boşalt
                jObj.RemoveAll();

                // İsme göre sırala ve tekrar ekle
                foreach (var prop in properties.OrderBy(p => p.Name))
                {
                    // Eğer özelliğin değeri de bir obje ise, tekrar içeri gir (Recursion)
                    SortJson(prop.Value);
                    jObj.Add(prop);
                }
            }
            else if (token is JArray jArray)
            {
                // Eğer bu bir diziyse (Array), içindeki her elemanı kontrol et
                foreach (var item in jArray)
                {
                    SortJson(item);
                }
            }
        }
    }
}