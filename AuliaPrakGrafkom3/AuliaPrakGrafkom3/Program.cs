namespace AuliaPrakGrafkom3
{
    internal static class Program
    {
        /// <summary>
        /// Titik masuk aplikasi Praktikum 3 — Windowing dan Clipping.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Menginisialisasi konfigurasi WinForms (.NET): DPI, visual styles,
            // dan default font. Konfigurasi ini harus dipanggil sebelum form
            // pertama dibuat.
            ApplicationConfiguration.Initialize();

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                string path = Path.Combine(Path.GetTempPath(), "windowing_clipping_error.txt");
                try
                {
                    File.WriteAllText(path,
                        DateTime.Now + Environment.NewLine + ex);
                }
                catch
                {
                    // Abaikan kegagalan penulisan log.
                }

                MessageBox.Show(
                    ex.ToString(),
                    "Windowing Dan Clipping - Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
