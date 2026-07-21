
using System.Reflection;
using Windows.ApplicationModel;

namespace MP_ModbusApp
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            //this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelVersion.Text = $"Version: {GetAppVersion()}";
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;

        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void labelRate_Click(object sender, EventArgs e)
        {
            string productId = "9MX01TWX2G8B";
            string storeUrl = $"ms-windows-store://review/?ProductId={productId}";

            DialogResult result = MessageBox.Show(
                "Your feedback helps me improve this tool for everyone. " +
                "It only takes a minute to leave a review. Would you like to do it now?",
                "Support ModbusApp",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = storeUrl,
                        UseShellExecute = true
                    });
                    labelRate.ForeColor = Color.FromArgb(128, 0, 128);
                }
                catch (Exception)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"https://apps.microsoft.com/store/detail/{productId}",
                        UseShellExecute = true
                    });
                }
            }
        }

        private void labelCoffe_Click(object sender, EventArgs e)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Support ModbusApp",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label()
            {
                Left = 20,
                Top = 20,
                Width = 350,
                Height = 60,
                Text = "Enjoying ModbusApp? Your support helps me develop new features. How would you like to support the project?"
            };

            Button btnBrowser = new Button() { Text = "Open Browser", Left = 20, Width = 100, Top = 90, DialogResult = DialogResult.Yes };
            Button btnQR = new Button() { Text = "Show QR Code", Left = 130, Width = 100, Top = 90, DialogResult = DialogResult.No };
            Button btnCancel = new Button() { Text = "Later", Left = 240, Width = 100, Top = 90, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(btnBrowser);
            prompt.Controls.Add(btnQR);
            prompt.Controls.Add(btnCancel);

            // Wyświetlamy okno i sprawdzamy wynik
            DialogResult result = prompt.ShowDialog();

            if (result == DialogResult.Yes)
            {
                // Opcja: Przeglądarka
                OpenBmcInBrowser("https://www.buymeacoffee.com/MarcinPindel");
            }
            else if (result == DialogResult.No)
            {
                // Opcja: Kod QR
                ShowQrCodeWindow();
            }
        }

        private void OpenBmcInBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                labelCoffe.ForeColor = Color.FromArgb(128, 0, 128);
            }
            catch (Exception)
            {
                ShowQrCodeWindow();
            }
        }

        private void ShowQrCodeWindow()
        {
            Form qrPopup = new Form
            {
                Text = "Scan to Support",
                Size = new Size(320, 380),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            PictureBox pb = new PictureBox
            {
                Image = Properties.Resources.bmc_qr,
                Dock = DockStyle.Top,
                Height = 280,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Button btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Width = 100,
                Height = 30
            };
            btnClose.Click += (s, e) => qrPopup.Close();

            qrPopup.Controls.Add(pb);
            qrPopup.Controls.Add(btnClose);
            qrPopup.ShowDialog();
        }

        public string GetAppVersion()
        {
            try
            {
                // Sprawdź, czy aplikacja działa jako pakiet (np. zainstalowana ze Store)
                var package = Package.Current;
                var v = package.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
            catch (System.InvalidOperationException)
            {
                // Fallback dla trybu Debug/bez pakietu
                return System.Windows.Forms.Application.ProductVersion;
            }
        }
    }
}
