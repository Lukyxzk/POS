using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp4
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private string adapterName = "Ethernet";

        public Form1()
        {
            // Pokud nemáš soubor Form1.Designer.cs, řádek níže smaž nebo zakomentuj
            // InitializeComponent(); 
            
            this.Text = "Nastavení IP Switcheru";
            this.Size = new Size(300, 200);
            SetupInterface(); // Přidá jednoduché prvky do okna
            CreateTrayIcon();
        }

        // Skrytí okna při startu
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
        }

        private void SetupInterface()
        {
            Label lbl = new Label() { Text = "Název adaptéru:", Left = 10, Top = 20, Width = 150 };
            TextBox txtAdapter = new TextBox() { Text = adapterName, Left = 10, Top = 45, Width = 200 };
            Button btnSave = new Button() { Text = "Uložit", Left = 10, Top = 80 };
            
            btnSave.Click += (s, e) => {
                adapterName = txtAdapter.Text;
                MessageBox.Show("Název adaptéru uložen: " + adapterName);
                this.Hide();
            };

            this.Controls.Add(lbl);
            this.Controls.Add(txtAdapter);
            this.Controls.Add(btnSave);
        }

        private void CreateTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            // NOVÉ: Možnost otevřít okno
            trayMenu.Items.Add("Otevřít nastavení", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Nastavit IP 192.168.0.51", null, SetStaticIP_Click);
            trayMenu.Items.Add("Přepnout na DHCP", null, SetDHCP_Click);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Ukončit", null, Exit_Click);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "IP Switcher";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            trayIcon.ContextMenuStrip = trayMenu;

            // Zobrazení okna dvojklikem na ikonu
            trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
        }

        private void SetStaticIP_Click(object sender, EventArgs e)
        {
            string cmd = $"interface ip set address name=\"{adapterName}\" static 192.168.0.51 255.255.255.0 192.168.0.1";
            RunCmd(cmd);
            trayIcon.ShowBalloonTip(2000, "IP Switcher", "Nastavování statické IP...", ToolTipIcon.Info);
        }

        private void SetDHCP_Click(object sender, EventArgs e)
        {
            RunCmd($"interface ip set address name=\"{adapterName}\" dhcp");
            RunCmd($"interface ip set dns name=\"{adapterName}\" dhcp");
            trayIcon.ShowBalloonTip(2000, "IP Switcher", "Přepínám na DHCP...", ToolTipIcon.Info);
        }

        private void RunCmd(string arguments)
        {
            try {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    Verb = "runas", 
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch (Exception ex) {
                MessageBox.Show("Chyba: " + ex.Message);
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        // Místo zavření okna ho jen skryjeme zpět do lišty
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                trayIcon.Visible = false;
                base.OnFormClosing(e);
            }
        }
    }
}