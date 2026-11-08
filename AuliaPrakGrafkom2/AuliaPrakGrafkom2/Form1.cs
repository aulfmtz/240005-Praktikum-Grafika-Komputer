using System.Drawing.Drawing2D;

namespace AuliaPrakGrafkom2
{
    public partial class Form1 : Form
    {
        // ============================================================
        // KOMPONEN UI (dibuat sepenuhnya secara terprogram)
        // ============================================================

        // --- Bagian Garis ---
        private TextBox txtLineX1 = null!;
        private TextBox txtLineY1 = null!;
        private TextBox txtLineX2 = null!;
        private TextBox txtLineY2 = null!;
        private Button btnDrawLine = null!;
        private Button btnClearLine = null!;
        private Panel pnlLineDasar = null!;
        private Panel pnlLineDDA = null!;
        private Panel pnlLineBresenham = null!;

        // --- Bagian Lingkaran ---
        private TextBox txtCircleXc = null!;
        private TextBox txtCircleYc = null!;
        private TextBox txtCircleRadius = null!;
        private Button btnDrawCircle = null!;
        private Button btnClearCircle = null!;
        private Panel pnlCircleDasar = null!;
        private Panel pnlCirclePolar = null!;
        private Panel pnlCircleBresenham = null!;

        // --- Back-buffer bitmap untuk setiap kanvas ---
        // Semua piksel digambar ke Bitmap, bukan langsung ke layar.
        // Event Paint hanya menyalin isi Bitmap, sehingga gambar tetap ada
        // walaupun jendela di-minimize, di-refresh, atau tertutup jendela lain.
        private readonly Dictionary<Panel, Bitmap> canvasBuffers = new();

        private const int CanvasSize = 220;

        // Warna kuas piksel (dibuat sekali agar tidak boros resource)
        private static readonly Brush PixelBrush = new SolidBrush(Color.FromArgb(20, 20, 20));

        public Form1()
        {
            InitializeComponent();
            BuildUi();
        }

        // ============================================================
        // PEMBANGUNAN UI SECARA TERPROGRAM
        // ============================================================
        private void BuildUi()
        {
            Text = "Algoritma Penggambaran Objek Primitif";
            ClientSize = new Size(960, 720);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.WhiteSmoke;

            // ---------- BAGIAN ATAS: PENGGAMBARAN GARIS ----------
            var lblHeaderLine = new Label
            {
                Text = "PENGGAMBARAN GARIS",
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 10)
            };
            Controls.Add(lblHeaderLine);

            // Panel input garis (kiri)
            var pnlLineInput = new Panel
            {
                Location = new Point(12, 36),
                Size = new Size(220, 252),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            Controls.Add(pnlLineInput);

            var lblLineColX = new Label { Text = "X", AutoSize = true, Location = new Point(70, 12) };
            var lblLineColY = new Label { Text = "Y", AutoSize = true, Location = new Point(140, 12) };
            pnlLineInput.Controls.Add(lblLineColX);
            pnlLineInput.Controls.Add(lblLineColY);

            var lblRow1 = new Label { Text = "1", AutoSize = true, Location = new Point(18, 42) };
            txtLineX1 = new TextBox { Text = "20", Location = new Point(55, 38), Size = new Size(60, 23) };
            txtLineY1 = new TextBox { Text = "20", Location = new Point(125, 38), Size = new Size(60, 23) };
            pnlLineInput.Controls.Add(lblRow1);
            pnlLineInput.Controls.Add(txtLineX1);
            pnlLineInput.Controls.Add(txtLineY1);

            var lblRow2 = new Label { Text = "2", AutoSize = true, Location = new Point(18, 72) };
            txtLineX2 = new TextBox { Text = "180", Location = new Point(55, 68), Size = new Size(60, 23) };
            txtLineY2 = new TextBox { Text = "150", Location = new Point(125, 68), Size = new Size(60, 23) };
            pnlLineInput.Controls.Add(lblRow2);
            pnlLineInput.Controls.Add(txtLineX2);
            pnlLineInput.Controls.Add(txtLineY2);

            btnDrawLine = new Button
            {
                Text = "Gambar",
                Location = new Point(20, 118),
                Size = new Size(180, 34)
            };
            btnDrawLine.Click += BtnDrawLine_Click;

            btnClearLine = new Button
            {
                Text = "Bersihkan",
                Location = new Point(20, 160),
                Size = new Size(180, 34)
            };
            btnClearLine.Click += BtnClearLine_Click;

            pnlLineInput.Controls.Add(btnDrawLine);
            pnlLineInput.Controls.Add(btnClearLine);

            // Tiga kanvas garis berjejer horizontal
            pnlLineDasar = CreateCanvas("DASAR", 250, 36);
            pnlLineDDA = CreateCanvas("DDA", 485, 36);
            pnlLineBresenham = CreateCanvas("Bresenham", 720, 36);

            // ---------- BAGIAN BAWAH: PENGGAMBARAN LINGKARAN ----------
            var lblHeaderCircle = new Label
            {
                Text = "PENGGAMBARAN LINGKARAN",
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 372)
            };
            Controls.Add(lblHeaderCircle);

            // Panel input lingkaran (kiri)
            var pnlCircleInput = new Panel
            {
                Location = new Point(12, 398),
                Size = new Size(220, 252),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            Controls.Add(pnlCircleInput);

            var lblCircleColX = new Label { Text = "X", AutoSize = true, Location = new Point(70, 12) };
            var lblCircleColY = new Label { Text = "Y", AutoSize = true, Location = new Point(140, 12) };
            pnlCircleInput.Controls.Add(lblCircleColX);
            pnlCircleInput.Controls.Add(lblCircleColY);

            var lblCenter = new Label { Text = "C", AutoSize = true, Location = new Point(18, 42) };
            txtCircleXc = new TextBox { Text = "110", Location = new Point(55, 38), Size = new Size(60, 23) };
            txtCircleYc = new TextBox { Text = "110", Location = new Point(125, 38), Size = new Size(60, 23) };
            pnlCircleInput.Controls.Add(lblCenter);
            pnlCircleInput.Controls.Add(txtCircleXc);
            pnlCircleInput.Controls.Add(txtCircleYc);

            var lblRadius = new Label { Text = "Radius (r)", AutoSize = true, Location = new Point(18, 72) };
            txtCircleRadius = new TextBox { Text = "80", Location = new Point(90, 68), Size = new Size(60, 23) };
            pnlCircleInput.Controls.Add(lblRadius);
            pnlCircleInput.Controls.Add(txtCircleRadius);

            btnDrawCircle = new Button
            {
                Text = "Gambar",
                Location = new Point(20, 118),
                Size = new Size(180, 34)
            };
            btnDrawCircle.Click += BtnDrawCircle_Click;

            btnClearCircle = new Button
            {
                Text = "Bersihkan",
                Location = new Point(20, 160),
                Size = new Size(180, 34)
            };
            btnClearCircle.Click += BtnClearCircle_Click;

            pnlCircleInput.Controls.Add(btnDrawCircle);
            pnlCircleInput.Controls.Add(btnClearCircle);

            // Tiga kanvas lingkaran berjejer horizontal
            pnlCircleDasar = CreateCanvas("DASAR", 250, 398);
            pnlCirclePolar = CreateCanvas("Polar", 485, 398);
            pnlCircleBresenham = CreateCanvas("Bresenham", 720, 398);
        }

        /// <summary>
        /// Membuat satu kanvas (Panel) beserta label judul di atasnya,
        /// back-buffer Bitmap, dan event Paint.
        /// </summary>
        /// <summary>Membebaskan seluruh back-buffer saat form ditutup.</summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (Bitmap bmp in canvasBuffers.Values)
            {
                bmp.Dispose();
            }
            canvasBuffers.Clear();
            base.OnFormClosed(e);
        }

        private Panel CreateCanvas(string title, int x, int y)
        {
            var lbl = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font(Font.FontFamily, 9f, FontStyle.Bold),
                Location = new Point(x, y)
            };
            Controls.Add(lbl);

            var panel = new Panel
            {
                Location = new Point(x, y + 22),
                Size = new Size(CanvasSize, CanvasSize),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Inisialisasi back-buffer putih
            var bmp = new Bitmap(CanvasSize, CanvasSize);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }
            canvasBuffers[panel] = bmp;

            // Event Paint: salin back-buffer ke layar (tanpa menggambar ulang algoritma)
            panel.Paint += (s, e) =>
            {
                e.Graphics.DrawImageUnscaled(canvasBuffers[(Panel)s!], 0, 0);
            };

            Controls.Add(panel);
            return panel;
        }

        // ============================================================
        // HELPER PENGGAMBARAN PIKSEL
        // ============================================================
        /// <summary>
        /// Menggambar SATU piksel. Seluruh algoritma garis/lingkaran hanya
        /// boleh memanggil method ini (tidak memakai DrawLine/DrawEllipse).
        /// Ukuran 2x2 dipilih agar piksel jelas terlihat pada layar
        /// beresolusi tinggi.
        /// </summary>
        private void PutPixel(Graphics g, int x, int y, Brush brush)
        {
            g.FillRectangle(brush, x, y, 2, 2);
        }

        /// <summary>
        /// Menyiapkan kanvas (membersihkan back-buffer), menjalankan aksi
        /// penggambaran dengan satu objek Graphics, lalu memicu repaint.
        /// Satu Graphics dipakai untuk seluruh piksel agar efisien.
        /// </summary>
        private void DrawOnCanvas(Panel canvas, Action<Graphics> drawAction)
        {
            Bitmap bmp = canvasBuffers[canvas];
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.None; // piksel tetap tajam
                g.PixelOffsetMode = PixelOffsetMode.None;
                drawAction(g);
            }
            canvas.Invalidate();
        }

        private void ClearCanvas(Panel canvas)
        {
            using (var g = Graphics.FromImage(canvasBuffers[canvas]))
            {
                g.Clear(Color.White);
            }
            canvas.Invalidate();
        }

        // ============================================================
        // EVENT HANDLER TOMBOL
        // ============================================================
        private void BtnDrawLine_Click(object? sender, EventArgs e)
        {
            // --- Validasi input integer ---
            if (!int.TryParse(txtLineX1.Text, out int x1) ||
                !int.TryParse(txtLineY1.Text, out int y1) ||
                !int.TryParse(txtLineX2.Text, out int x2) ||
                !int.TryParse(txtLineY2.Text, out int y2))
            {
                MessageBox.Show(
                    "Input garis harus berupa bilangan bulat!",
                    "Input Tidak Valid",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // --- Gambar garis yang sama di ketiga kanvas, algoritma berbeda ---
            DrawOnCanvas(pnlLineDasar, g => DrawLineDasar(g, x1, y1, x2, y2));
            DrawOnCanvas(pnlLineDDA, g => DrawLineDDA(g, x1, y1, x2, y2));
            DrawOnCanvas(pnlLineBresenham, g => DrawLineBresenham(g, x1, y1, x2, y2));
        }

        private void BtnClearLine_Click(object? sender, EventArgs e)
        {
            ClearCanvas(pnlLineDasar);
            ClearCanvas(pnlLineDDA);
            ClearCanvas(pnlLineBresenham);
        }

        private void BtnDrawCircle_Click(object? sender, EventArgs e)
        {
            // --- Validasi input integer ---
            if (!int.TryParse(txtCircleXc.Text, out int xc) ||
                !int.TryParse(txtCircleYc.Text, out int yc) ||
                !int.TryParse(txtCircleRadius.Text, out int r))
            {
                MessageBox.Show(
                    "Input lingkaran harus berupa bilangan bulat!",
                    "Input Tidak Valid",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (r <= 0 || r >= CanvasSize)
            {
                MessageBox.Show(
                    $"Radius harus berada di antara 1 dan {CanvasSize - 1}!",
                    "Input Tidak Valid",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // --- Gambar lingkaran yang sama di ketiga kanvas, algoritma berbeda ---
            DrawOnCanvas(pnlCircleDasar, g => DrawCircleDasar(g, xc, yc, r));
            DrawOnCanvas(pnlCirclePolar, g => DrawCirclePolar(g, xc, yc, r));
            DrawOnCanvas(pnlCircleBresenham, g => DrawCircleBresenham(g, xc, yc, r));
        }

        private void BtnClearCircle_Click(object? sender, EventArgs e)
        {
            ClearCanvas(pnlCircleDasar);
            ClearCanvas(pnlCirclePolar);
            ClearCanvas(pnlCircleBresenham);
        }

        // ============================================================
        // ALGORITMA 2.1 — GARIS DASAR
        // Gradien m = (y2 - y1) / (x2 - x1); iterasi x dengan delta 1,
        // y ditambah m setiap langkah (y* = y + m).
        // ============================================================
        private void DrawLineDasar(Graphics g, int x1, int y1, int x2, int y2)
        {
            // Kasus khusus garis vertikal: gradien tidak terdefinisi
            // (pembagian dengan nol), sehingga y diiterasi langsung.
            if (x1 == x2)
            {
                int yStart = Math.Min(y1, y2);
                int yEnd = Math.Max(y1, y2);
                for (int yi = yStart; yi <= yEnd; yi++)
                {
                    PutPixel(g, x1, yi, PixelBrush);
                }
                return;
            }

            // Pastikan iterasi selalu dari kiri ke kanan (x1 <= x2)
            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            double m = (double)(y2 - y1) / (x2 - x1);
            double y = y1;

            for (int x = x1; x <= x2; x++)
            {
                PutPixel(g, x, (int)Math.Round(y), PixelBrush);
                y += m; // y* = y + m
            }
        }

        // ============================================================
        // ALGORITMA 2.2 — GARIS DDA (Digital Differential Analyzer)
        // Jumlah langkah s = max(|dx|, |dy|); inkremen dx/s dan dy/s.
        // ============================================================
        private void DrawLineDDA(Graphics g, int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            // Jumlah langkah mengikuti selisih terpanjang agar garis tidak putus
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            // Titik awal == titik akhir
            if (steps == 0)
            {
                PutPixel(g, x1, y1, PixelBrush);
                return;
            }

            double xInc = (double)dx / steps;
            double yInc = (double)dy / steps;

            double x = x1;
            double y = y1;

            for (int i = 0; i <= steps; i++)
            {
                PutPixel(g, (int)Math.Round(x), (int)Math.Round(y), PixelBrush);
                x += xInc;
                y += yInc;
            }
        }

        // ============================================================
        // ALGORITMA 2.3 — GARIS BRESENHAM (SEMUA 8 OKTAN)
        // Integer penuh: da = max(|dx|,|dy|), db = min(|dx|,|dy|),
        // parameter keputusan d = 2*db - da. Langkah diagonal (m2) jika
        // d >= 0, langkah lurus (m1) jika d < 0.
        // ============================================================
        private void DrawLineBresenham(Graphics g, int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int absDx = Math.Abs(dx);
            int absDy = Math.Abs(dy);

            int x = x1;
            int y = y1;

            // Sumbu dominan (da) selalu bergerak, sumbu minor (db) menyusul
            int da = Math.Max(absDx, absDy);
            int db = Math.Min(absDx, absDy);

            // Parameter keputusan awal
            int d = 2 * db - da;

            // Tentukan vektor langkah lurus (m1) dan diagonal (m2)
            // berdasarkan tanda dx, dy, dan perbandingan |dx| >= |dy|
            int m1x, m1y, m2x, m2y;
            if (absDx >= absDy)
            {
                // Sumbu X dominan
                m1x = Math.Sign(dx); m1y = 0;
                m2x = Math.Sign(dx); m2y = Math.Sign(dy);
            }
            else
            {
                // Sumbu Y dominan
                m1x = 0; m1y = Math.Sign(dy);
                m2x = Math.Sign(dx); m2y = Math.Sign(dy);
            }

            PutPixel(g, x, y, PixelBrush);

            // Iterasi maksimal da langkah sampai mencapai titik akhir
            for (int i = 0; i < da; i++)
            {
                if (d >= 0)
                {
                    // Langkah diagonal: kedua sumbu bergeser
                    x += m2x;
                    y += m2y;
                    d += 2 * db - 2 * da;
                }
                else
                {
                    // Langkah lurus: hanya sumbu dominan bergeser
                    x += m1x;
                    y += m1y;
                    d += 2 * db;
                }

                PutPixel(g, x, y, PixelBrush);

                if (x == x2 && y == y2)
                {
                    break;
                }
            }
        }

        // ============================================================
        // ALGORITMA 2.4 — LINGKARAN DASAR
        // y = yc +/- sqrt(r^2 - x^2); iterasi x dari -r sampai r,
        // setiap x menghasilkan dua piksel (atas dan bawah).
        // ============================================================
        private void DrawCircleDasar(Graphics g, int xc, int yc, int r)
        {
            for (int x = -r; x <= r; x++)
            {
                double yOffset = Math.Sqrt(r * r - x * x);
                double yAtas = yc - yOffset;
                double yBawah = yc + yOffset;

                PutPixel(g, xc + x, (int)Math.Round(yAtas), PixelBrush);
                PutPixel(g, xc + x, (int)Math.Round(yBawah), PixelBrush);
            }
        }

        // ============================================================
        // ALGORITMA 2.5 — LINGKARAN POLAR
        // x = xc + r*cos(theta), y = yc + r*sin(theta);
        // theta 0..2*PI dengan step kecil agar busur rapat.
        // ============================================================
        private void DrawCirclePolar(Graphics g, int xc, int yc, int r)
        {
            // Step 1/r membuat panjang busur antar titik <= 1 satuan.
            // Nilai 0.005 dipakai sebagai batas atas agar radius kecil
            // tetap menghasilkan lingkaran yang rapat.
            double step = Math.Min(1.0 / r, 0.005);

            for (double theta = 0; theta <= 2 * Math.PI; theta += step)
            {
                double x = xc + r * Math.Cos(theta);
                double y = yc + r * Math.Sin(theta);
                PutPixel(g, (int)Math.Round(x), (int)Math.Round(y), PixelBrush);
            }
        }

        // ============================================================
        // ALGORITMA 2.6 & 2.7 — LINGKARAN BRESENHAM (MIDPOINT CIRCLE)
        // Hitung satu oktan dengan operasi integer, lalu cerminkan ke
        // 8 titik simetris. Parameter keputusan d = 3 - 2r.
        // ============================================================
        private void DrawCircleBresenham(Graphics g, int xc, int yc, int r)
        {
            int x = 0;
            int y = r;
            int d = 3 - (2 * r);

            DrawCirclePoints(g, xc, yc, x, y);

            // Selama x <= y, masih di oktan pertama (0..45 derajat)
            while (x <= y)
            {
                // Increment x terlebih dahulu (sesuai Algoritma 2.7 modul)
                x++;

                if (d < 0)
                {
                    // Titik tengah di dalam lingkaran: pilih piksel horizontal
                    d = d + (4 * x) + 6;
                }
                else
                {
                    // Titik tengah di luar/tepat lingkaran: pilih piksel diagonal
                    d = d + 4 * (x - y) + 10;
                    y--;
                }

                DrawCirclePoints(g, xc, yc, x, y);
            }
        }

        /// <summary>
        /// Menggambar 8 titik simetris oktan lingkaran dari satu titik (x, y).
        /// </summary>
        private void DrawCirclePoints(Graphics g, int xc, int yc, int x, int y)
        {
            PutPixel(g, xc + x, yc + y, PixelBrush);
            PutPixel(g, xc - x, yc + y, PixelBrush);
            PutPixel(g, xc + x, yc - y, PixelBrush);
            PutPixel(g, xc - x, yc - y, PixelBrush);
            PutPixel(g, xc + y, yc + x, PixelBrush);
            PutPixel(g, xc - y, yc + x, PixelBrush);
            PutPixel(g, xc + y, yc - x, PixelBrush);
            PutPixel(g, xc - y, yc - x, PixelBrush);
        }
    }
}
