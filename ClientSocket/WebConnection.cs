using AdvancedBot.client;
using AdvancedBot.ClientSocket.Packets;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.ClientSocket
{
    internal class WebConnection
    {

        internal static HWID computer = new HWID();

        internal static Task<string> getStringAsync(String url)
        {
            if (!isProxying(url)) return null;
            using (var client = new WebClient())
            {

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                client.Headers["User-Agent"] = encryptedAgent();
                return client.DownloadStringTaskAsync(url);
            }
        }

        internal static string getString(String url)
        {
            if (!isProxying(url)) return null;
            using (var client = new WebClient())
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                client.Headers["User-Agent"] = encryptedAgent();
                return client.DownloadString(url);
            }
        }

        internal static void Controls(Form form, bool enabled)
        {
            computer.SetAllowed(enabled);
            if (form == null) return;
            foreach (Control control in form.Controls)
            {
                if (control.Name.Equals(form.Name)) continue;
                control.Enabled = enabled;
            }
        }

        internal static bool check(String IP)
        {
            return false;
        }

        internal static async void write(String str)
        {
        }

        internal static long latest;
        internal static string encryptedAgent()
        {
            latest = Utils.GetTimestamp() / 1000;

            //Debug.WriteLine("Encrypeted "WebEncryption.Encrypt("AdvanvacedBot ( " + Program.AppVersion + " " + (int)(Utils.GetTimestamp() / 32 * 3) + " )"));
            return ("AdvancedBot ( " + Program.AppVersion + " " + (latest) + " )");
        }

        internal static bool isProxying(String url)
        {
            Uri myUri = new Uri(url);
            string server = Dns.GetHostAddresses(myUri.Host)[0].ToString();
            Debug.WriteLine(myUri.Host + " " + server);
            return !server.Equals(GetLocalIPAddress()) && !server.Equals("127.0.0.1");
        }


        internal static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        internal static void checkByte(String n)
        {
            Debug.WriteLine(latest + " " + n);
            long received = Convert.ToInt64(n);

            if ((int)latest != (int)received)
            {
                throw new Exception("Invalid Request");
            }
        }

        internal static async Task<string> DownloadString(String url)
        {
            try
            {
                String request = await getStringAsync(url);
                return WebEncryption.Decrypt(request);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
               internal static async void connect()
        {
            try
            {
                
                Controls(Program.FrmMain, true);
                Controls(Program.FrmMain.frmStart, true);
                Program.pluginManager.Init();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(Program.FrmMain, "Unable to check updates, please try again later\n\nError: " + ex.Message, Translation.getStringKey("Others.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }

    }
}
