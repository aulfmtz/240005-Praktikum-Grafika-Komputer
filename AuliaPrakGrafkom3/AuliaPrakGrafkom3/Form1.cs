using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace AuliaPrakGrafkom3
{
    /// <summary>
    /// Form utama Praktikum 3 — Windowing dan Clipping.
    ///
    /// Seluruh antarmuka dibangun secara programatik (tanpa Designer) agar
    /// posisi, ukuran, warna, dan teks setiap kontrol sama persis dengan
    /// gambar referensi modul praktikum.
    ///
    /// Alur kerja:
    ///   1. "Gambar Window"   -> menampilkan / menggambar kotak clip window.
    ///   2. "Gambar Garis"    -> menampilkan / menggambar garis pada window.
    ///   3. "Gambar Viewport" -> menampilkan / menggambar kotak viewport.
    ///   4. "Clip"            -> memotong garis dengan Cohen-Sutherland lalu
    ///                           memetakannya ke dalam viewport.
    ///
    /// Semua koordinat menggunakan sistem koordinat GDI+ (sumbu Y membesar
    /// ke bawah). Nilai default sengaja dipilih agar hasil clipping persis
    /// seperti contoh pada modul: (97.0, 278.9) dan (271.0, 91.0).
    /// </summary>
    public partial class Form1 : Form
    {
        // ====================================================================
        //  KONSTANTA OUTCODE COHEN-SUTHERLAND (4-BIT)
        // ====================================================================
        private const int INSIDE = 0;  // 0000
        private const int LEFT = 1;    // 0001
        private const int RIGHT = 2;   // 0010
        private const int BOTTOM = 4;  // 0100
        private const int TOP = 8;     // 1000

        // ====================================================================
        //  STATE KOORDINAT
        //  Nilai default mengikuti gambar contoh pada modul praktikum.
        // ====================================================================
        // Kotak Window: (97, 77) sampai (271, 298)
        private double _xwMin = 97, _ywMin = 77, _xwMax = 271, _ywMax = 298;

        // Garis default: (58, 321) sampai (296, 64)
        private double _x1 = 58, _y1 = 321, _x2 = 296, _y2 = 64;

        // Kotak Viewport: (97, 77) sampai (271, 298)
        private double _xvMin = 97, _yvMin = 77, _xvMax = 271, _yvMax = 298;

        // Hasil clipping yang sudah dipetakan ke koordinat viewport.
        private bool _clipValid;
        private double _vx1, _vy1, _vx2, _vy2;

        // Penanda objek mana saja yang sedang terlihat pada kanvas kiri.
        // Flag ini meniru perilaku kanvas bitmap persisten: tombol "Clear"
        // benar-benar mengosongkan kanvas, sedangkan tombol gambar hanya
        // menambahkan objek yang bersangkutan.
        private bool _windowVisible = true;
        private bool _lineVisible = true;

        // ====================================================================
        //  KONTROL UI
        // ====================================================================
        private CanvasPanel pnlWindow = null!;
        private CanvasPanel pnlViewport = null!;
        private Label lblCoordWindow = null!;
        private Label lblCoordViewport = null!;

        private Button btnGambarGaris = null!;
        private Button btnClip = null!;
        private Button btnGambarWindow = null!;
        private Button btnClearWindow = null!;
        private Button btnGambarViewport = null!;
        private Button btnClearViewport = null!;

        // ====================================================================
        //  MODE INTERAKSI MOUSE
        // ====================================================================
        private enum ToolMode { None, Window, Line, Viewport }

        private ToolMode _mode = ToolMode.None;
        private bool _dragging;
        private Point _dragStart;
        private Point _dragEnd;

        // ====================================================================
        //  FONT & WARNA
        // ====================================================================
        private readonly Font _coordFont = new("Arial", 7.5f, FontStyle.Regular);
        private readonly Font _readoutFont = new("Arial", 9f, FontStyle.Bold);
        private readonly Font _buttonFont = new("Arial", 8.5f, FontStyle.Bold);
        private readonly Font _titleFont = new("Segoe UI", 14f, FontStyle.Regular);

        private static readonly Color FormBlue = Color.FromArgb(0, 102, 204);
        private static readonly Color HeaderBlue = Color.FromArgb(0, 120, 215);
        private static readonly Color CanvasSilver = Color.Silver;

        // ====================================================================
        //  KONSTRUKTOR
        // ====================================================================
        public Form1()
        {
            BuildUi();
        }

        /// <summary>
        /// Membangun seluruh antarmuka secara programatik.
        /// Semua koordinat dan ukuran mengikuti spesifikasi modul.
        /// </summary>
        private void BuildUi()
        {
            // --- Properti dasar Form ------------------------------------
            Text = "Windowing Dan Clipping";
            ClientSize = new Size(920, 620);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = FormBlue;
            AutoScaleMode = AutoScaleMode.None;
            DoubleBuffered = true;
            Font = new Font("Arial", 9f, FontStyle.Regular);

            // --- Label judul di atas kanvas ------------------------------
            var lblTitleWindow = new Label
            {
                Text = "Window",
                Location = new Point(20, 15),
                Size = new Size(110, 26),
                Font = _titleFont,
                ForeColor = Color.Black,
                BackColor = HeaderBlue,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                UseMnemonic = false
            };

            var lblTitleViewport = new Label
            {
                Text = "Viewport",
                Location = new Point(465, 15),
                Size = new Size(130, 26),
                Font = _titleFont,
                ForeColor = Color.Black,
                BackColor = HeaderBlue,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                UseMnemonic = false
            };

            // --- Kanvas kiri (Window) ------------------------------------
            pnlWindow = new CanvasPanel
            {
                Location = new Point(20, 50),
                Size = new Size(425, 410),
                BackColor = CanvasSilver,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Cross,
                TabStop = false
            };

            lblCoordWindow = new Label
            {
                Text = "Koordinat Window : x = 0,00 ; y = 0,00",
                Location = new Point(5, 385),
                AutoSize = true,
                Font = _readoutFont,
                ForeColor = Color.Black,
                BackColor = CanvasSilver,
                UseMnemonic = false
            };
            pnlWindow.Controls.Add(lblCoordWindow);

            // --- Kanvas kanan (Viewport) ---------------------------------
            pnlViewport = new CanvasPanel
            {
                Location = new Point(465, 50),
                Size = new Size(425, 410),
                BackColor = CanvasSilver,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Cross,
                TabStop = false
            };

            lblCoordViewport = new Label
            {
                Text = "Koordinat Viewport : x = 0,00 ; y = 0,00",
                Location = new Point(140, 385),
                AutoSize = true,
                Font = _readoutFont,
                ForeColor = Color.Black,
                BackColor = CanvasSilver,
                UseMnemonic = false
            };
            pnlViewport.Controls.Add(lblCoordViewport);

            // --- GroupBox kendali kiri -----------------------------------
            var grpKendaliWindow = new GroupBox
            {
                Text = "Kendali Window",
                Location = new Point(20, 475),
                Size = new Size(425, 95),
                ForeColor = Color.Black,
                Font = new Font("Arial", 9f, FontStyle.Regular)
            };

            btnGambarGaris = MakeButton("Gambar Garis", new Point(10, 22), new Size(135, 26));
            btnClip = MakeButton("Clip", new Point(10, 55), new Size(135, 26));
            btnGambarWindow = MakeButton("Gambar Window", new Point(265, 22), new Size(135, 26));
            btnClearWindow = MakeButton("Clear Window", new Point(265, 55), new Size(135, 26));

            grpKendaliWindow.Controls.Add(btnGambarGaris);
            grpKendaliWindow.Controls.Add(btnClip);
            grpKendaliWindow.Controls.Add(btnGambarWindow);
            grpKendaliWindow.Controls.Add(btnClearWindow);

            // --- GroupBox kendali kanan ----------------------------------
            var grpKendaliViewport = new GroupBox
            {
                Text = "Kendali Viewport",
                Location = new Point(465, 475),
                Size = new Size(425, 95),
                ForeColor = Color.Black,
                Font = new Font("Arial", 9f, FontStyle.Regular)
            };

            btnGambarViewport = MakeButton("Gambar Viewport", new Point(10, 22), new Size(165, 26));
            btnClearViewport = MakeButton("Clear Viewport", new Point(245, 22), new Size(165, 26));

            grpKendaliViewport.Controls.Add(btnGambarViewport);
            grpKendaliViewport.Controls.Add(btnClearViewport);

            // --- Susun seluruh kontrol ke Form ---------------------------
            Controls.Add(lblTitleWindow);
            Controls.Add(lblTitleViewport);
            Controls.Add(pnlWindow);
            Controls.Add(pnlViewport);
            Controls.Add(grpKendaliWindow);
            Controls.Add(grpKendaliViewport);

            // --- Event tombol --------------------------------------------
            btnGambarWindow.Click += (_, _) => MulaiGambarWindow();
            btnGambarGaris.Click += (_, _) => MulaiGambarGaris();
            btnGambarViewport.Click += (_, _) => MulaiGambarViewport();
            btnClip.Click += (_, _) => ProsesClipping();

            btnClearWindow.Click += (_, _) => ClearWindowCanvas();
            btnClearViewport.Click += (_, _) => ClearViewportCanvas();

            // --- Event mouse pada kanvas kiri ----------------------------
            pnlWindow.MouseDown += WindowCanvas_MouseDown;
            pnlWindow.MouseMove += WindowCanvas_MouseMove;
            pnlWindow.MouseUp += WindowCanvas_MouseUp;

            // --- Event mouse pada kanvas kanan ---------------------------
            pnlViewport.MouseDown += ViewportCanvas_MouseDown;
            pnlViewport.MouseMove += ViewportCanvas_MouseMove;
            pnlViewport.MouseUp += ViewportCanvas_MouseUp;
        }

        /// <summary>Membuat tombol dengan gaya visual sesuai modul.</summary>
        private Button MakeButton(string text, Point location, Size size)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Standard,
                Font = _buttonFont,
                UseVisualStyleBackColor = false,
                TabStop = true
            };
        }

        /// <summary>
        /// Dipanggil setelah form benar-benar tampil. Pada saat ini ukuran
        /// client area panel sudah final sehingga back-buffer dapat dibuat
        /// dengan dimensi yang tepat.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _windowVisible = true;
            _lineVisible = true;
            _clipValid = false;

            RenderWindowCanvas();
            RenderViewportCanvas();
        }

        // ====================================================================
        //  AKSI TOMBOL
        // ====================================================================

        /// <summary>
        /// Tombol "Gambar Window": mengaktifkan mode gambar window dan
        /// menampilkan window default (97,77)-(271,298) bila belum diubah.
        /// Pengguna juga dapat melakukan click-and-drag pada kanvas kiri
        /// untuk menentukan window baru.
        /// </summary>
        private void MulaiGambarWindow()
        {
            _mode = ToolMode.Window;
            _windowVisible = true;
            RenderWindowCanvas();
        }

        /// <summary>
        /// Tombol "Gambar Garis": mengaktifkan mode gambar garis dan
        /// menampilkan garis default (58,321)-(296,64) bila belum diubah.
        /// Click-and-drag pada kanvas kiri akan mengganti garis tersebut.
        /// </summary>
        private void MulaiGambarGaris()
        {
            _mode = ToolMode.Line;
            _lineVisible = true;
            RenderWindowCanvas();
        }

        /// <summary>
        /// Tombol "Gambar Viewport": mengaktifkan mode gambar viewport dan
        /// menampilkan kotak viewport default (97,77)-(271,298).
        /// Click-and-drag pada kanvas kanan akan mengganti ukuran viewport.
        /// </summary>
        private void MulaiGambarViewport()
        {
            _mode = ToolMode.Viewport;
            RenderViewportCanvas();
        }

        /// <summary>
        /// Tombol "Clip": menjalankan algoritma Cohen-Sutherland terhadap
        /// garis dan window saat ini, lalu memetakan kedua titik hasil
        /// clipping ke koordinat viewport dengan rumus transformasi
        /// window-to-viewport. Hasil digambar pada kanvas kanan bersama
        /// label koordinat merah.
        /// </summary>
        private void ProsesClipping()
        {
            bool berhasil = ClipLine(
                _x1, _y1, _x2, _y2,
                _xwMin, _ywMin, _xwMax, _ywMax,
                out double cx1, out double cy1,
                out double cx2, out double cy2);

            if (!berhasil)
            {
                // Garis berada sepenuhnya di luar window (trivial reject).
                _clipValid = false;
                RenderViewportCanvas();
                return;
            }

            // Transformasi Window -> Viewport.
            _vx1 = MapXToViewport(cx1);
            _vy1 = MapYToViewport(cy1);
            _vx2 = MapXToViewport(cx2);
            _vy2 = MapYToViewport(cy2);

            _clipValid = true;
            RenderViewportCanvas();
        }

        /// <summary>Mengosongkan kanvas window beserta objek yang tampak.</summary>
        private void ClearWindowCanvas()
        {
            _windowVisible = false;
            _lineVisible = false;
            ClearBitmap(pnlWindow);
        }

        /// <summary>Mengosongkan kanvas viewport dan hasil clipping.</summary>
        private void ClearViewportCanvas()
        {
            _clipValid = false;
            ClearBitmap(pnlViewport);
        }

        /// <summary>Mengisi seluruh bitmap kanvas dengan warna abu-abu perak.</summary>
        private static void ClearBitmap(CanvasPanel canvas)
        {
            using Graphics g = Graphics.FromImage(canvas.EnsureBuffer());
            g.Clear(CanvasSilver);
            canvas.Invalidate();
        }

        // ====================================================================
        //  INTERAKSI MOUSE — KANVAS WINDOW (KIRI)
        // ====================================================================

        /// <summary>
        /// Menangkap posisi awal drag. Drag hanya aktif jika pengguna sudah
        /// memilih mode "Gambar Window" atau "Gambar Garis".
        /// </summary>
        private void WindowCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_mode != ToolMode.Window && _mode != ToolMode.Line) return;

            _dragging = true;
            _dragStart = ClampPoint(e.Location, pnlWindow.ClientSize);
            _dragEnd = _dragStart;
            pnlWindow.Capture = true;
            RenderWindowCanvas();
        }

        /// <summary>
        /// Memperbarui label koordinat secara real-time dan menampilkan
        /// garis bantu (rubber band) selama proses drag.
        /// </summary>
        private void WindowCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            lblCoordWindow.Text =
                $"Koordinat Window : x = {F2(e.X)} ; y = {F2(e.Y)}";

            if (!_dragging) return;

            _dragEnd = ClampPoint(e.Location, pnlWindow.ClientSize);
            RenderWindowCanvas();
        }

        /// <summary>
        /// Menyelesaikan drag: memperbarui koordinat window atau garis
        /// sesuai mode yang aktif, lalu menggambar ulang kanvas.
        /// </summary>
        private void WindowCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            _dragging = false;
            pnlWindow.Capture = false;
            _dragEnd = ClampPoint(e.Location, pnlWindow.ClientSize);

            int lebar = Math.Abs(_dragEnd.X - _dragStart.X);
            int tinggi = Math.Abs(_dragEnd.Y - _dragStart.Y);

            if (_mode == ToolMode.Window && lebar >= 5 && tinggi >= 5)
            {
                // Normalisasi agar xmin < xmax dan ymin < ymax.
                _xwMin = Math.Min(_dragStart.X, _dragEnd.X);
                _xwMax = Math.Max(_dragStart.X, _dragEnd.X);
                _ywMin = Math.Min(_dragStart.Y, _dragEnd.Y);
                _ywMax = Math.Max(_dragStart.Y, _dragEnd.Y);

                _windowVisible = true;
                InvalidateClipResult();
            }
            else if (_mode == ToolMode.Line && (lebar >= 5 || tinggi >= 5))
            {
                _x1 = _dragStart.X;
                _y1 = _dragStart.Y;
                _x2 = _dragEnd.X;
                _y2 = _dragEnd.Y;

                _lineVisible = true;
                InvalidateClipResult();
            }

            RenderWindowCanvas();
        }

        // ====================================================================
        //  INTERAKSI MOUSE — KANVAS VIEWPORT (KANAN)
        // ====================================================================

        /// <summary>Menangkap posisi awal drag untuk menentukan viewport baru.</summary>
        private void ViewportCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_mode != ToolMode.Viewport) return;

            _dragging = true;
            _dragStart = ClampPoint(e.Location, pnlViewport.ClientSize);
            _dragEnd = _dragStart;
            pnlViewport.Capture = true;
            RenderViewportCanvas();
        }

        /// <summary>Memperbarui label koordinat viewport dan preview drag.</summary>
        private void ViewportCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            lblCoordViewport.Text =
                $"Koordinat Viewport : x = {F2(e.X)} ; y = {F2(e.Y)}";

            if (!_dragging) return;

            _dragEnd = ClampPoint(e.Location, pnlViewport.ClientSize);
            RenderViewportCanvas();
        }

        /// <summary>Menyelesaikan drag dan menyimpan ukuran viewport baru.</summary>
        private void ViewportCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            _dragging = false;
            pnlViewport.Capture = false;
            _dragEnd = ClampPoint(e.Location, pnlViewport.ClientSize);

            int lebar = Math.Abs(_dragEnd.X - _dragStart.X);
            int tinggi = Math.Abs(_dragEnd.Y - _dragStart.Y);

            if (lebar >= 5 && tinggi >= 5)
            {
                _xvMin = Math.Min(_dragStart.X, _dragEnd.X);
                _xvMax = Math.Max(_dragStart.X, _dragEnd.X);
                _yvMin = Math.Min(_dragStart.Y, _dragEnd.Y);
                _yvMax = Math.Max(_dragStart.Y, _dragEnd.Y);

                // Hasil clipping lama tidak berlaku untuk viewport baru.
                _clipValid = false;
            }

            RenderViewportCanvas();
        }

        /// <summary>Membatasi titik agar tetap berada di dalam area kanvas.</summary>
        private static Point ClampPoint(Point p, Size size)
        {
            return new Point(
                Math.Clamp(p.X, 0, Math.Max(0, size.Width - 1)),
                Math.Clamp(p.Y, 0, Math.Max(0, size.Height - 1)));
        }

        /// <summary>Menandai hasil clipping lama tidak valid dan membersihkan viewport.</summary>
        private void InvalidateClipResult()
        {
            _clipValid = false;
            RenderViewportCanvas();
        }

        // ====================================================================
        //  RENDERING KANVAS KIRI (WINDOW)
        // ====================================================================

        /// <summary>
        /// Menggambar ulang seluruh isi kanvas window ke back-buffer Bitmap:
        /// kotak window hitam, label sudut merah, garis hitam, label ujung
        /// garis merah, dan preview drag bila sedang aktif.
        /// </summary>
        private void RenderWindowCanvas()
        {
            Bitmap buffer = pnlWindow.EnsureBuffer();
            using Graphics g = Graphics.FromImage(buffer);
            SiapkanGraphics(g);
            g.Clear(CanvasSilver);

            using var pena = new Pen(Color.Black, 1.5f);

            // Kotak window + label keempat sudut.
            if (_windowVisible)
            {
                DrawRectangle(g, _xwMin, _ywMin, _xwMax, _ywMax, pena);
                DrawCornerLabels(g, _xwMin, _ywMin, _xwMax, _ywMax, pnlWindow.ClientSize);
            }

            // Garis + label kedua ujung.
            if (_lineVisible)
            {
                g.DrawLine(pena,
                    (float)_x1, (float)_y1,
                    (float)_x2, (float)_y2);

                DrawEndpointLabels(g, _x1, _y1, _x2, _y2, pnlWindow.ClientSize);
            }

            // Preview saat pengguna melakukan drag.
            if (_dragging)
                DrawDraft(g, pnlWindow.ClientSize);

            pnlWindow.Invalidate();
        }

        // ====================================================================
        //  RENDERING KANVAS KANAN (VIEWPORT)
        // ====================================================================

        /// <summary>
        /// Menggambar ulang kanvas viewport: kotak viewport hitam dengan
        /// label sudut merah, serta garis hasil clipping yang sudah
        /// ditransformasikan ke koordinat viewport beserta label ujungnya.
        /// </summary>
        private void RenderViewportCanvas()
        {
            Bitmap buffer = pnlViewport.EnsureBuffer();
            using Graphics g = Graphics.FromImage(buffer);
            SiapkanGraphics(g);
            g.Clear(CanvasSilver);

            using var pena = new Pen(Color.Black, 1.5f);

            DrawRectangle(g, _xvMin, _yvMin, _xvMax, _yvMax, pena);
            DrawCornerLabels(g, _xvMin, _yvMin, _xvMax, _yvMax, pnlViewport.ClientSize);

            if (_clipValid)
            {
                g.DrawLine(pena,
                    (float)_vx1, (float)_vy1,
                    (float)_vx2, (float)_vy2);

                DrawEndpointLabels(g, _vx1, _vy1, _vx2, _vy2, pnlViewport.ClientSize);
            }

            if (_dragging)
                DrawDraft(g, pnlViewport.ClientSize);

            pnlViewport.Invalidate();
        }

        /// <summary>Pengaturan kualitas rendering GDI+.</summary>
        private static void SiapkanGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        }

        /// <summary>Menggambar persegi dengan koordinat double.</summary>
        private static void DrawRectangle(Graphics g,
            double xmin, double ymin, double xmax, double ymax, Pen pen)
        {
            g.DrawRectangle(pen,
                (float)xmin,
                (float)ymin,
                (float)(xmax - xmin),
                (float)(ymax - ymin));
        }

        // ====================================================================
        //  LABEL KOORDINAT MERAH
        // ====================================================================

        /// <summary>
        /// Menampilkan koordinat keempat sudut kotak (window/viewport)
        /// dengan warna merah di dalam sudut masing-masing.
        /// </summary>
        private void DrawCornerLabels(Graphics g,
            double xmin, double ymin, double xmax, double ymax, Size kanvas)
        {
            string kiriAtas = $"({F0(xmin)},{F0(ymin)})";
            string kananAtas = $"({F0(xmax)},{F0(ymin)})";
            string kiriBawah = $"({F0(xmin)},{F0(ymax)})";
            string kananBawah = $"({F0(xmax)},{F0(ymax)})";

            float tinggi = _coordFont.GetHeight(g);

            DrawTextClamped(g, kiriAtas,
                (float)xmin + 3, (float)ymin + 2, kanvas);

            DrawTextClamped(g, kananAtas,
                (float)xmax - g.MeasureString(kananAtas, _coordFont).Width - 3,
                (float)ymin + 2, kanvas);

            DrawTextClamped(g, kiriBawah,
                (float)xmin + 3,
                (float)ymax - tinggi - 2, kanvas);

            DrawTextClamped(g, kananBawah,
                (float)xmax - g.MeasureString(kananBawah, _coordFont).Width - 3,
                (float)ymax - tinggi - 2, kanvas);
        }

        /// <summary>
        /// Menampilkan koordinat kedua ujung garis (format satu angka
        /// desimal, pemisah desimal koma) di sisi luar titik ujung.
        /// </summary>
        private void DrawEndpointLabels(Graphics g,
            double x1, double y1, double x2, double y2, Size kanvas)
        {
            string ujung1 = $"({F1(x1)},{F1(y1)})";
            string ujung2 = $"({F1(x2)},{F1(y2)})";

            float tinggi = _coordFont.GetHeight(g);

            // Label ujung pertama diletakkan di kiri titik.
            DrawTextClamped(g, ujung1,
                (float)x1 - g.MeasureString(ujung1, _coordFont).Width - 6,
                (float)y1 - tinggi / 2f, kanvas);

            // Label ujung kedua diletakkan di kanan titik.
            DrawTextClamped(g, ujung2,
                (float)x2 + 6,
                (float)y2 - tinggi / 2f, kanvas);
        }

        /// <summary>
        /// Menggambar teks merah dengan posisi yang selalu dibatasi agar
        /// tidak keluar dari area bitmap kanvas.
        /// </summary>
        private void DrawTextClamped(Graphics g, string teks,
            float x, float y, Size kanvas)
        {
            SizeF ukuran = g.MeasureString(teks, _coordFont);

            float maxX = Math.Max(1f, kanvas.Width - ukuran.Width - 1f);
            float maxY = Math.Max(1f, kanvas.Height - ukuran.Height - 1f);

            x = Math.Clamp(x, 1f, maxX);
            y = Math.Clamp(y, 1f, maxY);

            g.DrawString(teks, _coordFont, Brushes.Red, x, y);
        }

        // ====================================================================
        //  PREVIEW DRAG (RUBBER BAND)
        // ====================================================================

        /// <summary>
        /// Menggambar preview putus-putus selama pengguna melakukan drag.
        /// Mode Window/Viewport menampilkan persegi, mode Line menampilkan garis.
        /// </summary>
        private void DrawDraft(Graphics g, Size kanvas)
        {
            using var penaDraft = new Pen(Color.FromArgb(180, 0, 0, 0), 1f)
            {
                DashStyle = DashStyle.Dash
            };

            if (_mode == ToolMode.Window || _mode == ToolMode.Viewport)
            {
                Rectangle r = RectangleFromPoints(_dragStart, _dragEnd);
                g.DrawRectangle(penaDraft, r);
            }
            else if (_mode == ToolMode.Line)
            {
                g.DrawLine(penaDraft, _dragStart, _dragEnd);
            }
        }

        /// <summary>Membentuk Rectangle dari dua titik (dinormalisasi).</summary>
        private static Rectangle RectangleFromPoints(Point a, Point b)
        {
            return new Rectangle(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y),
                Math.Abs(a.X - b.X),
                Math.Abs(a.Y - b.Y));
        }

        // ====================================================================
        //  INTI ALGORITMA COHEN-SUTHERLAND
        // ====================================================================

        /// <summary>
        /// Menghitung outcode 4-bit sebuah titik terhadap clip window.
        ///
        /// Sistem koordinat GDI+ memiliki sumbu Y yang membesar ke bawah,
        /// sehingga:
        ///   - y &lt; ymin  berarti titik berada DI ATAS window -> TOP  (1000)
        ///   - y &gt; ymax  berarti titik berada DI BAWAH window -> BOTTOM (0100)
        ///   - x &lt; xmin  berarti titik berada DI KIRI window   -> LEFT (0001)
        ///   - x &gt; xmax  berarti titik berada DI KANAN window  -> RIGHT (0010)
        /// </summary>
        private static int ComputeOutCode(double x, double y,
            double xmin, double ymin, double xmax, double ymax)
        {
            int kode = INSIDE;

            if (x < xmin) kode |= LEFT;
            else if (x > xmax) kode |= RIGHT;

            if (y < ymin) kode |= TOP;
            else if (y > ymax) kode |= BOTTOM;

            return kode;
        }

        /// <summary>
        /// Memotong segmen garis (x1,y1)-(x2,y2) terhadap window dengan
        /// algoritma Cohen-Sutherland.
        ///
        /// Nilai balik true berarti ada bagian garis yang terlihat; titik
        /// hasil pemotongan dikembalikan melalui parameter out. Nilai balik
        /// false berarti garis ditolak sepenuhnya (trivial reject).
        /// </summary>
        private static bool ClipLine(
            double x1, double y1, double x2, double y2,
            double xmin, double ymin, double xmax, double ymax,
            out double cx1, out double cy1,
            out double cx2, out double cy2)
        {
            cx1 = x1; cy1 = y1;
            cx2 = x2; cy2 = y2;

            // Lindungi dari window yang tidak valid (lebar/tinggi nol).
            if (xmax - xmin < 1e-9 || ymax - ymin < 1e-9)
                return false;

            int kode1 = ComputeOutCode(x1, y1, xmin, ymin, xmax, ymax);
            int kode2 = ComputeOutCode(x2, y2, xmin, ymin, xmax, ymax);

            bool diterima = false;
            int pengaman = 0;

            while (true)
            {
                if ((kode1 | kode2) == 0)
                {
                    // Kedua ujung berada di dalam window: trivial accept.
                    diterima = true;
                    break;
                }

                if ((kode1 & kode2) != 0)
                {
                    // Kedua ujung berada pada sisi luar yang sama:
                    // trivial reject.
                    break;
                }

                // Proteksi tambahan agar tidak terjadi loop tak berujung.
                if (++pengaman > 16) break;

                // Pilih salah satu ujung yang berada di luar window.
                int kodeLuar = kode1 != 0 ? kode1 : kode2;
                double x, y;

                if ((kodeLuar & TOP) != 0)
                {
                    // Titik potong dengan batas atas: y = ymin.
                    if (Math.Abs(y2 - y1) < 1e-9) break;
                    y = ymin;
                    x = x1 + (x2 - x1) * (ymin - y1) / (y2 - y1);
                }
                else if ((kodeLuar & BOTTOM) != 0)
                {
                    // Titik potong dengan batas bawah: y = ymax.
                    if (Math.Abs(y2 - y1) < 1e-9) break;
                    y = ymax;
                    x = x1 + (x2 - x1) * (ymax - y1) / (y2 - y1);
                }
                else if ((kodeLuar & RIGHT) != 0)
                {
                    // Titik potong dengan batas kanan: x = xmax.
                    if (Math.Abs(x2 - x1) < 1e-9) break;
                    x = xmax;
                    y = y1 + (y2 - y1) * (xmax - x1) / (x2 - x1);
                }
                else // LEFT
                {
                    // Titik potong dengan batas kiri: x = xmin.
                    if (Math.Abs(x2 - x1) < 1e-9) break;
                    x = xmin;
                    y = y1 + (y2 - y1) * (xmin - x1) / (x2 - x1);
                }

                // Ganti ujung yang berada di luar dengan titik potong baru.
                if (kodeLuar == kode1)
                {
                    x1 = x;
                    y1 = y;
                    kode1 = ComputeOutCode(x1, y1, xmin, ymin, xmax, ymax);
                }
                else
                {
                    x2 = x;
                    y2 = y;
                    kode2 = ComputeOutCode(x2, y2, xmin, ymin, xmax, ymax);
                }
            }

            if (diterima)
            {
                cx1 = x1; cy1 = y1;
                cx2 = x2; cy2 = y2;
            }

            return diterima;
        }

        // ====================================================================
        //  TRANSFORMASI WINDOW -> VIEWPORT
        // ====================================================================

        /// <summary>
        /// xv = xvmin + (xw - xwmin) * (xvmax - xvmin) / (xwmax - xwmin)
        /// </summary>
        private double MapXToViewport(double xw)
        {
            double skala = (_xvMax - _xvMin) / (_xwMax - _xwMin);
            return _xvMin + (xw - _xwMin) * skala;
        }

        /// <summary>
        /// yv = yvmin + (yw - ywmin) * (yvmax - yvmin) / (ywmax - ywmin)
        /// </summary>
        private double MapYToViewport(double yw)
        {
            double skala = (_yvMax - _yvMin) / (_ywMax - _ywMin);
            return _yvMin + (yw - _ywMin) * skala;
        }

        // ====================================================================
        //  FORMAT ANGKA (PEMISAH DESIMAL KOMA)
        // ====================================================================

        /// <summary>Format bilangan bulat untuk label sudut, mis. 97.</summary>
        private static string F0(double nilai)
        {
            return Math.Round(nilai).ToString("0", CultureInfo.InvariantCulture);
        }

        /// <summary>Format satu angka desimal dengan koma, mis. 278,9.</summary>
        private static string F1(double nilai)
        {
            if (Math.Abs(nilai) < 0.0005) nilai = 0;
            return nilai.ToString("0.0", CultureInfo.InvariantCulture)
                        .Replace('.', ',');
        }

        /// <summary>Format dua angka desimal dengan koma, mis. 97,00.</summary>
        private static string F2(double nilai)
        {
            if (Math.Abs(nilai) < 0.00005) nilai = 0;
            return nilai.ToString("0.00", CultureInfo.InvariantCulture)
                        .Replace('.', ',');
        }
    }

    /// <summary>
    /// Panel kanvas GDI+ dengan back-buffer Bitmap.
    ///
    /// Seluruh penggambaran dilakukan ke sebuah Bitmap di memori, kemudian
    /// Bitmap tersebut ditampilkan pada event Paint. Dengan cara ini gambar
    /// tidak hilang ketika form di-minimize, ditutup sebagian, atau
    /// di-refresh oleh sistem operasi.
    /// </summary>
    internal sealed class CanvasPanel : Panel
    {
        private Bitmap? _buffer;

        public CanvasPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;
        }

        /// <summary>
        /// Mengembalikan Bitmap back-buffer yang ukurannya selalu sama
        /// dengan client area panel. Bitmap dibuat ulang bila ukuran panel
        /// berubah.
        /// </summary>
        public Bitmap EnsureBuffer()
        {
            int lebar = Math.Max(1, ClientSize.Width);
            int tinggi = Math.Max(1, ClientSize.Height);

            if (_buffer == null ||
                _buffer.Width != lebar ||
                _buffer.Height != tinggi)
            {
                _buffer?.Dispose();
                _buffer = new Bitmap(lebar, tinggi);
            }

            return _buffer;
        }

        /// <summary>
        /// Menampilkan isi back-buffer. Tidak ada penggambaran langsung di
        /// sini sehingga tidak terjadi flicker.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_buffer != null)
            {
                e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
            }
            else
            {
                e.Graphics.Clear(BackColor);
            }
        }

        /// <summary>Melewati pengecatan background standar agar tidak berkedip.</summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Sengaja dikosongkan: background sudah tercakup oleh back-buffer.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer?.Dispose();
                _buffer = null;
            }

            base.Dispose(disposing);
        }
    }
}
