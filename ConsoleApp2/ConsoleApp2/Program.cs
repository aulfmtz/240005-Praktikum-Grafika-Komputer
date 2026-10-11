using System;
using System.Drawing;
using System.Windows.Forms;

namespace PraktikumGDI1
{
    public partial class FormSoal1 : Form
    {
        public FormSoal1()
        {
            this.Text = "Soal 1 - Objek Sederhana";
            this.Size = new Size(400, 400);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen penHitam = new Pen(Color.Black, 2);
            Brush brushHitam = new SolidBrush(Color.Black);

            // 4 perintah GDI+
            g.DrawEllipse(penHitam, 100, 100, 150, 150); // 1. Kepala
            g.FillEllipse(brushHitam, 135, 140, 15, 15); // 2. Mata Kiri
            g.FillEllipse(brushHitam, 195, 140, 15, 15); // 3. Mata Kanan
            g.DrawArc(penHitam, 125, 160, 100, 60, 0, 180); // 4. Senyum

            penHitam.Dispose();
            brushHitam.Dispose();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormSoal1());
            Application.Run(new PraktikumGDI2.FormSoal2());
        }
    }
}