using System;
using System.Drawing;
using System.Windows.Forms;

namespace PraktikumGDI2
{
    public partial class FormSoal2 : Form
    {
        public FormSoal2()
        {
            this.Text = "Soal 2 - Objek Kompleks (Perahu Layar)";
            this.Size = new Size(600, 450);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Brush brushLangit = new SolidBrush(Color.MidnightBlue);
            Brush brushBulan = new SolidBrush(Color.LightYellow);
            Brush brushLaut = new SolidBrush(Color.DarkBlue);
            Brush brushBadanPerahu = new SolidBrush(Color.SaddleBrown);
            Brush brushTiang = new SolidBrush(Color.BurlyWood);
            Brush brushLayar = new SolidBrush(Color.WhiteSmoke);
            Pen penGaris = new Pen(Color.Black, 2);

            // 1. Langit malam
            g.FillRectangle(brushLangit, 0, 0, 600, 250);

            // 2. Laut
            g.FillRectangle(brushLaut, 0, 250, 600, 200);

            // 3. Bulan purnama
            g.FillEllipse(brushBulan, 450, 40, 80, 80);

            // 4, 5, 6. Bintang-bintang
            g.FillEllipse(Brushes.White, 100, 80, 5, 5);
            g.FillEllipse(Brushes.White, 250, 50, 4, 4);
            g.FillEllipse(Brushes.White, 350, 120, 6, 6);

            // Titik kordinat untuk badan perahu
            Point[] badanPerahu = {
                new Point(150, 250),
                new Point(400, 250),
                new Point(350, 320),
                new Point(200, 320)
            };

            // 7. Isi badan perahu
            g.FillPolygon(brushBadanPerahu, badanPerahu);
            // 8. Garis pinggir badan perahu
            g.DrawPolygon(penGaris, badanPerahu);

            // 9. Tiang layar
            g.FillRectangle(brushTiang, 265, 80, 10, 170);

            // Titik koordinat layar Kiri 
            Point[] layarKiri = {
                new Point(260, 90),
                new Point(160, 230),
                new Point(260, 230)
            };

            // 10. Isi layar kiri
            g.FillPolygon(brushLayar, layarKiri);
            // 11. Garis layar kiri
            g.DrawPolygon(penGaris, layarKiri);

            // Titik koordinat layar Kanan 
            Point[] layarKanan = {
                new Point(280, 110),
                new Point(380, 230),
                new Point(280, 230)
            };

            // 12. Isi layar kanan
            g.FillPolygon(brushLayar, layarKanan);
            // 13. Garis layar kanan
            g.DrawPolygon(penGaris, layarKanan);

            brushLangit.Dispose();
            brushBulan.Dispose();
            brushLaut.Dispose();
            brushBadanPerahu.Dispose();
            brushTiang.Dispose();
            brushLayar.Dispose();
            penGaris.Dispose();
        }
    }

}