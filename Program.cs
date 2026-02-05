using System;
using System.Threading;
using System.Windows.Forms;


namespace MP_ModbusApp
{
    /// <summary>
    /// The main entry point for the application.
    /// Handles single-instance logic using a Mutex.
    /// </summary>
    internal static class Program
    {
        // Unique GUID for the application to ensure only one instance runs
        private const string AppGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        private static Mutex mutex = new Mutex(true, AppGuid);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Try to acquire the mutex. If it fails, another instance is already running.
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                ApplicationConfiguration.Initialize();

                // Initialize the database on application startup
                DatabaseHelper.InitializeDatabase();
                DatabaseHelper.CreateDeviceTables();

                Application.Run(new MainWindow());

                // Release the mutex when the application closes
                mutex.ReleaseMutex();
            }
            else
            {
                // Show a message and exit if the application is already running.
                MessageBox.Show("The application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}