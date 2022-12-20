using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.Controls;
using AdvancedBot.ProxyChecker;
using MaxMind.MaxMindDb;

namespace AdvancedBot
{
    public partial class FilterCountryForm : Form
    {
        private MaxMindDbReader dbReader = new MaxMindDbReader(@"GeoLite2-Country\GeoLite2-Country.mmdb");
        private List<Tuple<string, ProxyInfo>> proxyMap = new List<Tuple<string, ProxyInfo>>();
        private ProxyCheckerForm checkerForm;

        public FilterCountryForm(ProxyCheckerForm pcf)
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;
            checkerForm = pcf;
        }

        public void DisplayAllCountries()
        {
            foreach (var entry in dbReader.EnumerateDataSection()) {
                var country = entry["country"] ?? entry["registered_country"];
                if (country != null) {
                    string name = country["names"]["pt-BR"].AsStr();
                    string iso = country["iso_code"].AsStr();

                    lbCountries.Add(iso, name);
                }
            }
        }
        public FilterCountryForm AddFromList(string proxyList)
        {
            var proxies = ProxyUtils.Parse(proxyList);

            Form pForm = null;
            PercentageProgressBar pb = null;

            if (proxies.Count > 1000) {
                pForm = new Form() {
                    ClientSize = new Size(280, 32),
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "Carregando...",
                    Icon = Program.FrmMain.Icon
                };
                pb = new PercentageProgressBar() {
                    Size = new Size(pForm.ClientSize.Width - 8, 24),
                    Maximum = proxies.Count,
                    Location = new Point(4, 4)
                };
                pForm.Controls.Add(pb);
                pForm.Show();
            }

            int curr = 0;
            foreach (var proxy in proxies) {
                var info = dbReader.Find(proxy.IP);
                var country = info?["country"] ?? info?["registered_country"];
                if (country != null) {
                    string name = country["names"]["pt-BR"].AsStr();
                    string iso = country["iso_code"].AsStr();

                    proxyMap.Add(new Tuple<string, ProxyInfo>(iso, proxy));
                    lbCountries.Add(iso, name);
                } else {
                    lbCountries.Add(CountryListBox.CODE_UNKNOWN, "Desconhecido");
                    proxyMap.Add(new Tuple<string, ProxyInfo>(CountryListBox.CODE_UNKNOWN, proxy));
                }
                if (pForm != null && curr++ % 128 == 0) {
                    pb.Value = curr;
                    pb.Refresh();
                }
            }
            if (pForm != null) {
                pForm.Close();
                pForm.Dispose();
            }
            lbCountries.Invalidate();

            return this;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var rtb = checkerForm.rtfProxies;
            rtb.Clear();
            StringBuilder sb = new StringBuilder();

            var selCountries = lbCountries.Items.Where(a => a.Checked).Select(a => a.CountryCode).OrderBy(a => a).ToArray();
            foreach (var kv in proxyMap.Where(a => Array.BinarySearch(selCountries, a.Item1) >= 0)) {
                var proxy = kv.Item2;
                sb.AppendLine($"{proxy.IP}:{proxy.Port}");
            }
            var tag = Program.Config.GetCompound("CountryListBox");
            foreach (var country in lbCountries.Items) {
                tag.AddBoolean(country.CountryCode, country.Checked);
            }
            Program.Config.AddCompound("CountryListBox", tag);

            rtb.Text = sb.ToString();
            Close();
        }

        private void FilterCountryForm_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
        }
    }
}
