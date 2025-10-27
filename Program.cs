using System;
using System.Threading;
using System.Windows.Forms;


namespace MP_ModbusApp
{
    internal static class Program
    {
        private const string AppGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        private static Mutex mutex = new Mutex(true, AppGuid);

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Dodaj tę linię, aby utworzyć bazę danych przy starcie aplikacji
                DatabaseHelper.InitializeDatabase();
                DatabaseHelper.CreateDeviceTables();

                ApplicationConfiguration.Initialize();
                Application.Run(new MainWindow());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("The application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}