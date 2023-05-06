using System.Text.Json;
using System.Text;
using StreetLibrary;

namespace AddToJson
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            List<Street> streets;
            using (StreamReader reader = new StreamReader("street.json", Encoding.Default))
            {
                string jsonData = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(jsonData))
                {
                    streets = JsonSerializer.Deserialize<List<Street>>(jsonData);
                }
                else
                {
                    streets = new List<Street>();
                }
            }
            streets.Add(new Street(textBoxName.Text, Convert.ToInt32(textBoxZipCode.Text)));
            using (StreamWriter writer = new StreamWriter("street.json", false, Encoding.Default))
            {
                string updatedData = JsonSerializer.Serialize(streets);
                writer.Write(updatedData);
            }

            textBoxName.Text = string.Empty;
            textBoxZipCode.Text = string.Empty;
        }

        private void buttonOutput_Click(object sender, EventArgs e)
        {
            textBoxOutput.Text = string.Empty;

            using (StreamReader reader = new StreamReader("street.json", Encoding.Default))
            {
                string data = reader.ReadLine();
                List<Street> streets = JsonSerializer.Deserialize<List<Street>>(data);
                foreach (Street street in streets)
                {
                    textBoxOutput.Text += $"{street.Name} : {street.ZipCode}{Environment.NewLine}";
                }
            }
        }
    }
}