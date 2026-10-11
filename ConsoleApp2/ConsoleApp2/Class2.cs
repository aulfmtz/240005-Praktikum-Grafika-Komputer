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

            g.FillRectangle(brushLangit, 0, 0, 600, 250);
            g.FillRectangle(brushLaut, 0, 250, 600, 200);
            g.FillEllipse(brushBulan, 450, 40, 80, 80);

            g.FillEllipse(Brushes.White, 100, 80, 5, 5);
            g.FillEllipse(Brushes.White, 250, 50, 4, 4);
            g.FillEllipse(Brushes.White, 350, 120, 6, 6);

            Point[] badanPerahu = {
                new Point(150, 250),
                new Point(400, 250),
                new Point(350, 320),
                new Point(200, 320)
            };

            g.FillPolygon(brushBadanPerahu, badanPerahu);
            g.DrawPolygon(penGaris, badanPerahu);
            g.FillRectangle(brushTiang, 265, 80, 10, 170);

            Point[] layarKiri = {
                new Point(260, 90),
                new Point(160, 230),
                new Point(260, 230)
            };

            g.FillPolygon(brushLayar, layarKiri);
            g.DrawPolygon(penGaris, layarKiri);

            Point[] layarKanan = {
                new Point(280, 110),
                new Point(380, 230),
                new Point(280, 230)
            };

            g.FillPolygon(brushLayar, layarKanan);
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
