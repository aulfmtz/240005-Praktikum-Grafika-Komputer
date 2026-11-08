using System.Drawing.Drawing2D;
using System.Globalization;

namespace AuliaPrakGrafkom4
{
    public partial class Form1 : Form
    {
        // =====================================================================
        // KONSTANTA KANVAS
        // Kanvas berukuran 260 x 260 piksel. Titik (0,0) Kartesius berada
        // tepat di tengah kanvas, yaitu (130, 130) dalam koordinat piksel.
        // =====================================================================
        private const int CanvasSize = 260;
        private const float CanvasCenter = CanvasSize / 2f; // 130

        // =====================================================================
        // DATA OBJEK
        // Semua titik disimpan dalam koordinat Kartesius relatif terhadap
        // titik tengah kanvas, bukan koordinat piksel layar.
        // =====================================================================
        private readonly List<PointF> originalPoints = new();
        private readonly List<PointF> pendingPoints = new();
        private readonly List<PointF> resultPoints = new();
        private bool pickCenterMode;

        // =====================================================================
        // KONTROL UI
        // =====================================================================
        private readonly ComboBox cmbObjek = new();
        private readonly CheckBox chkMouseClick = new();
        private readonly Button btnDraw = new();
        private readonly Button btnClear = new();

        private readonly TabControl tabTransformasi = new();
        private readonly TabPage tabTranslasi = new("Translasi");
        private readonly TabPage tabRotasi = new("Rotasi");
        private readonly TabPage tabRefleksi = new("Refleksi");
        private readonly TabPage tabDilatasi = new("Dilatasi");

        private readonly TextBox txtTx = new();
        private readonly TextBox txtTy = new();
        private readonly TextBox txtSudut = new();
        private readonly RadioButton rbCCW = new();
        private readonly RadioButton rbCW = new();
        private readonly TextBox txtPusatX = new();
        private readonly TextBox txtPusatY = new();

        private readonly ComboBox cmbSumbu = new();
        private readonly TextBox txtM = new();
        private readonly TextBox txtC = new();

        private readonly TextBox txtSx = new();
        private readonly TextBox txtSy = new();

        private readonly CanvasPanel pnlAsal = new();
        private readonly CanvasPanel pnlHasil = new();

        private readonly ToolStripStatusLabel lblKoordinat = new();
        private readonly ToolStripStatusLabel lblInfo = new();

        public Form1()
        {
            InitializeComponent();
            BuildUserInterface();
        }

        // =====================================================================
        // PEMBUATAN SELURUH ANTARMUKA (PROGRAMMATIC)
        // =====================================================================
        private void BuildUserInterface()
        {
            SuspendLayout();

            Text = "Transformasi 2D & Homogen";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(920, 480);
            MaximizeBox = false;
            DoubleBuffered = true;
            KeyPreview = true;

            BuildObjectSection();
            BuildTransformationSection();
            BuildCanvasSection();
            BuildStatusBar();

            KeyDown += Form1_KeyDown;

            ResumeLayout(false);
            PerformLayout();
        }

        // ---------------------------------------------------------------------
        // Bagian kiri atas: pemilihan / pembuatan objek
        // ---------------------------------------------------------------------
        private void BuildObjectSection()
        {
            Controls.Add(CreateSectionLabel("Pilih / Buat Objek", 20, 15));

            cmbObjek.Location = new Point(20, 40);
            cmbObjek.Size = new Size(200, 25);
            cmbObjek.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbObjek.Items.AddRange(new object[] { "Segitiga", "Persegi", "Jajar Genjang" });
            cmbObjek.SelectedIndex = 0;
            Controls.Add(cmbObjek);

            chkMouseClick.Text = "Gambar via Mouse Click";
            chkMouseClick.Location = new Point(20, 75);
            chkMouseClick.AutoSize = true;
            chkMouseClick.CheckedChanged += ChkMouseClick_CheckedChanged;
            Controls.Add(chkMouseClick);

            btnDraw.Text = "Draw";
            btnDraw.Location = new Point(20, 102);
            btnDraw.Size = new Size(92, 30);
            btnDraw.Click += BtnDraw_Click;
            Controls.Add(btnDraw);

            btnClear.Text = "Clear";
            btnClear.Location = new Point(122, 102);
            btnClear.Size = new Size(92, 30);
            btnClear.Click += BtnClear_Click;
            Controls.Add(btnClear);
        }

        // ---------------------------------------------------------------------
        // Bagian kiri bawah: TabControl berisi 4 operasi transformasi
        // ---------------------------------------------------------------------
        private void BuildTransformationSection()
        {
            Controls.Add(CreateSectionLabel("Operasi Transformasi", 20, 148));

            tabTransformasi.Location = new Point(20, 170);
            tabTransformasi.Size = new Size(280, 210);

            BuildTranslationTab();
            BuildRotationTab();
            BuildReflectionTab();
            BuildDilationTab();

            tabTransformasi.Controls.Add(tabTranslasi);
            tabTransformasi.Controls.Add(tabRotasi);
            tabTransformasi.Controls.Add(tabRefleksi);
            tabTransformasi.Controls.Add(tabDilatasi);
            tabTransformasi.SelectedIndex = 0;

            Controls.Add(tabTransformasi);
        }

        private void BuildTranslationTab()
        {
            AddLabel(tabTranslasi, "Tx :", 12, 20);
            AddTextBox(tabTranslasi, txtTx, 52, 17, 70, "50");

            AddLabel(tabTranslasi, "Ty :", 12, 55);
            AddTextBox(tabTranslasi, txtTy, 52, 52, 70, "30");

            var btnTranslasi = new Button
            {
                Text = "Translasi",
                Location = new Point(182, 25),
                Size = new Size(80, 30)
            };
            btnTranslasi.Click += BtnTranslasi_Click;
            tabTranslasi.Controls.Add(btnTranslasi);

            AddHint(tabTranslasi,
                "Matriks translasi homogen 3 x 3:\n" +
                "[ 1  0  0 ]\n" +
                "[ 0  1  0 ]\n" +
                "[ Tx Ty 1 ]",
                12, 92);
        }

        private void BuildRotationTab()
        {
            AddLabel(tabRotasi, "Sudut :", 12, 15);
            AddTextBox(tabRotasi, txtSudut, 80, 12, 55, "45");
            AddLabel(tabRotasi, "derajat", 142, 15);

            rbCCW.Text = "CCW";
            rbCCW.Location = new Point(12, 42);
            rbCCW.AutoSize = true;
            rbCCW.Checked = true;
            tabRotasi.Controls.Add(rbCCW);

            rbCW.Text = "CW";
            rbCW.Location = new Point(82, 42);
            rbCW.AutoSize = true;
            tabRotasi.Controls.Add(rbCW);

            var btnRotasi = new Button
            {
                Text = "Rotasi",
                Location = new Point(182, 40),
                Size = new Size(80, 30)
            };
            btnRotasi.Click += BtnRotasi_Click;
            tabRotasi.Controls.Add(btnRotasi);

            AddLabel(tabRotasi, "Pusat X :", 12, 76);
            AddTextBox(tabRotasi, txtPusatX, 78, 73, 45, "0");

            AddLabel(tabRotasi, "Pusat Y :", 130, 76);
            AddTextBox(tabRotasi, txtPusatY, 195, 73, 45, "0");

            var btnSetPusat = new Button
            {
                Text = "Set Pusat via Kanvas",
                Location = new Point(12, 105),
                Size = new Size(160, 28)
            };
            btnSetPusat.Click += BtnSetPusat_Click;
            tabRotasi.Controls.Add(btnSetPusat);

            AddHint(tabRotasi, "CCW = berlawanan arah jarum jam.", 12, 142);
        }

        private void BuildReflectionTab()
        {
            AddLabel(tabRefleksi, "Sumbu Cermin :", 12, 10);

            cmbSumbu.Location = new Point(12, 30);
            cmbSumbu.Size = new Size(165, 24);
            cmbSumbu.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSumbu.Items.AddRange(new object[]
            {
                "Sumbu X (y = 0)",
                "Sumbu Y (x = 0)",
                "Garis y = x",
                "Garis y = -x",
                "Garis y = mx + c"
            });
            cmbSumbu.SelectedIndex = 0;
            cmbSumbu.SelectedIndexChanged += CmbSumbu_SelectedIndexChanged;
            tabRefleksi.Controls.Add(cmbSumbu);

            var btnRefleksi = new Button
            {
                Text = "Refleksi",
                Location = new Point(187, 28),
                Size = new Size(78, 28)
            };
            btnRefleksi.Click += BtnRefleksi_Click;
            tabRefleksi.Controls.Add(btnRefleksi);

            AddLabel(tabRefleksi, "m =", 12, 72);
            AddTextBox(tabRefleksi, txtM, 50, 69, 45, "1");
            txtM.Enabled = false;

            AddLabel(tabRefleksi, "c =", 110, 72);
            AddTextBox(tabRefleksi, txtC, 138, 69, 45, "0");
            txtC.Enabled = false;

            AddHint(tabRefleksi,
                "Nilai m dan c dipakai saat memilih\n" +
                "garis y = mx + c.",
                12, 105);
        }

        private void BuildDilationTab()
        {
            AddLabel(tabDilatasi, "Sx :", 12, 20);
            AddTextBox(tabDilatasi, txtSx, 52, 17, 70, "1.5");

            AddLabel(tabDilatasi, "Sy :", 12, 55);
            AddTextBox(tabDilatasi, txtSy, 52, 52, 70, "1.5");

            var btnDilatasi = new Button
            {
                Text = "Dilatasi",
                Location = new Point(182, 25),
                Size = new Size(80, 30)
            };
            btnDilatasi.Click += BtnDilatasi_Click;
            tabDilatasi.Controls.Add(btnDilatasi);

            AddHint(tabDilatasi,
                "Dilatasi terhadap pusat (0,0).\n" +
                "Skala > 1 memperbesar, < 1 memperkecil.",
                12, 95);
        }

        // ---------------------------------------------------------------------
        // Bagian kanan: dua kanvas GDI+
        // ---------------------------------------------------------------------
        private void BuildCanvasSection()
        {
            var lblAsal = new Label
            {
                Text = "Objek Asal Sebelum Transformasi",
                Location = new Point(340, 18),
                Size = new Size(262, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold)
            };
            Controls.Add(lblAsal);

            pnlAsal.Location = new Point(340, 45);
            pnlAsal.ClientSize = new Size(CanvasSize, CanvasSize);
            pnlAsal.Paint += PnlAsal_Paint;
            pnlAsal.MouseDown += Pnl_MouseDown;
            pnlAsal.MouseMove += Pnl_MouseMove;
            pnlAsal.MouseLeave += Pnl_MouseLeave;
            Controls.Add(pnlAsal);

            var lblHasil = new Label
            {
                Text = "Objek Hasil Setelah Transformasi",
                Location = new Point(620, 18),
                Size = new Size(262, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold)
            };
            Controls.Add(lblHasil);

            pnlHasil.Location = new Point(620, 45);
            pnlHasil.ClientSize = new Size(CanvasSize, CanvasSize);
            pnlHasil.Paint += PnlHasil_Paint;
            pnlHasil.MouseDown += Pnl_MouseDown;
            pnlHasil.MouseMove += Pnl_MouseMove;
            pnlHasil.MouseLeave += Pnl_MouseLeave;
            Controls.Add(pnlHasil);

            var lblCatatan = new Label
            {
                Text = "Catatan: setiap operasi selalu dihitung dari objek asal.\n" +
                       "Titik (0,0) berada di tengah kanvas; sumbu X dan Y berwarna abu-abu.",
                Location = new Point(340, 318),
                Size = new Size(540, 36),
                ForeColor = Color.Gray
            };
            Controls.Add(lblCatatan);
        }

        // ---------------------------------------------------------------------
        // Status bar: koordinat kursor real-time
        // ---------------------------------------------------------------------
        private void BuildStatusBar()
        {
            var status = new StatusStrip { SizingGrip = false };

            lblKoordinat.Text = "Koordinat: (-)";
            lblKoordinat.AutoSize = false;
            lblKoordinat.Width = 230;
            lblKoordinat.TextAlign = ContentAlignment.MiddleLeft;

            lblInfo.Text = "Siap";
            lblInfo.Spring = true;
            lblInfo.TextAlign = ContentAlignment.MiddleRight;

            status.Items.AddRange(new ToolStripItem[] { lblKoordinat, lblInfo });
            Controls.Add(status);
        }

        // =====================================================================
        // HELPER PEMBUATAN KONTROL
        // =====================================================================
        private Label CreateSectionLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold)
            };
        }

        private static void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private static void AddTextBox(Control parent, TextBox box, int x, int y, int width, string text)
        {
            box.Location = new Point(x, y);
            box.Size = new Size(width, 23);
            box.Text = text;
            parent.Controls.Add(box);
        }

        private static void AddHint(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                MaximumSize = new Size(250, 0),
                ForeColor = Color.Gray
            });
        }

        // =====================================================================
        // EVENT: PEMBUATAN OBJEK
        // =====================================================================
        private void BtnDraw_Click(object? sender, EventArgs e)
        {
            if (chkMouseClick.Checked)
            {
                if (pendingPoints.Count < 3)
                {
                    MessageBox.Show(
                        "Klik minimal 3 titik pada kanvas asal sebelum menekan Draw.",
                        "Titik belum cukup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                originalPoints.Clear();
                originalPoints.AddRange(pendingPoints);
                pendingPoints.Clear();
                lblInfo.Text = $"Objek kustom dibuat dari {originalPoints.Count} titik.";
            }
            else
            {
                originalPoints.Clear();
                originalPoints.AddRange(CreatePreset(cmbObjek.SelectedItem?.ToString() ?? "Segitiga"));
                pendingPoints.Clear();
                lblInfo.Text = $"Objek: {cmbObjek.SelectedItem}";
            }

            // Hasil lama dihapus karena objek asal berubah.
            resultPoints.Clear();
            pnlAsal.Invalidate();
            pnlHasil.Invalidate();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            originalPoints.Clear();
            pendingPoints.Clear();
            resultPoints.Clear();
            lblInfo.Text = "Kanvas dibersihkan.";
            pnlAsal.Invalidate();
            pnlHasil.Invalidate();
        }

        private void ChkMouseClick_CheckedChanged(object? sender, EventArgs e)
        {
            pendingPoints.Clear();
            cmbObjek.Enabled = !chkMouseClick.Checked;
            pnlAsal.Invalidate();

            lblInfo.Text = chkMouseClick.Checked
                ? "Klik beberapa titik di kanvas asal, lalu tekan Draw untuk menutup poligon."
                : "Mode preset aktif. Pilih bentuk lalu tekan Draw.";
        }

        // Preset objek dalam koordinat Kartesius (relatif titik tengah).
        private static List<PointF> CreatePreset(string nama)
        {
            return nama switch
            {
                "Persegi" => new List<PointF>
                {
                    new(-70, -70), new(70, -70), new(70, 70), new(-70, 70)
                },
                "Jajar Genjang" => new List<PointF>
                {
                    new(-90, -55), new(30, -55), new(90, 55), new(-30, 55)
                },
                _ => new List<PointF>
                {
                    new(0, 95), new(-85, -55), new(85, -55)
                }
            };
        }

        // =====================================================================
        // EVENT: MOUSE PADA KANVAS
        // =====================================================================
        private void Pnl_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // Mode pemilihan pusat rotasi (dipicu tombol "Set Pusat via Kanvas").
            if (pickCenterMode)
            {
                PointF world = ScreenToWorld(e.Location);
                txtPusatX.Text = FormatNumber(world.X);
                txtPusatY.Text = FormatNumber(world.Y);

                pickCenterMode = false;
                UpdateCanvasCursors();
                tabTransformasi.SelectedTab = tabRotasi;
                lblInfo.Text = $"Pusat rotasi diatur ke ({FormatNumber(world.X)}, {FormatNumber(world.Y)}).";
                return;
            }

            // Mode penggambaran poligon lewat klik mouse (hanya kanvas asal).
            if (sender == pnlAsal && chkMouseClick.Checked)
            {
                pendingPoints.Add(ScreenToWorld(e.Location));
                pnlAsal.Invalidate();
                lblInfo.Text = $"Titik kustom: {pendingPoints.Count}. " +
                               "Klik lagi atau tekan Draw untuk menutup poligon.";
            }
        }

        private void Pnl_MouseMove(object? sender, MouseEventArgs e)
        {
            PointF world = ScreenToWorld(e.Location);
            int x = (int)Math.Round(world.X);
            int y = (int)Math.Round(world.Y);
            lblKoordinat.Text = $"Koordinat: ({x}, {y})";
        }

        private void Pnl_MouseLeave(object? sender, EventArgs e)
        {
            lblKoordinat.Text = "Koordinat: (-)";
        }

        private void BtnSetPusat_Click(object? sender, EventArgs e)
        {
            // Tombol berfungsi sebagai toggle (aktif / batal).
            if (pickCenterMode)
            {
                pickCenterMode = false;
                UpdateCanvasCursors();
                lblInfo.Text = "Pemilihan pusat rotasi dibatalkan.";
                return;
            }

            pickCenterMode = true;
            UpdateCanvasCursors();
            tabTransformasi.SelectedTab = tabRotasi;
            lblInfo.Text = "Klik salah satu kanvas untuk menetapkan pusat rotasi.";
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && pickCenterMode)
            {
                pickCenterMode = false;
                UpdateCanvasCursors();
                lblInfo.Text = "Pemilihan pusat rotasi dibatalkan.";
            }
        }

        private void UpdateCanvasCursors()
        {
            Cursor cursor = pickCenterMode ? Cursors.Cross : Cursors.Default;
            pnlAsal.Cursor = cursor;
            pnlHasil.Cursor = cursor;
        }

        // =====================================================================
        // EVENT: TOMBOL TRANSFORMASI
        // =====================================================================
        private void BtnTranslasi_Click(object? sender, EventArgs e)
        {
            if (!EnsureObjectExists())
                return;

            if (!TryReadNumber(txtTx, "Tx", out double tx) ||
                !TryReadNumber(txtTy, "Ty", out double ty))
                return;

            // Matriks translasi homogen (vektor baris: P' = P x M):
            // [ 1  0  0 ]
            // [ 0  1  0 ]
            // [ Tx Ty 1 ]
            double[,] matrix =
            {
                { 1, 0, 0  },
                { 0, 1, 0  },
                { tx, ty, 1 }
            };

            ApplyMatrix(matrix, $"Translasi: Tx = {FormatNumber(tx)}, Ty = {FormatNumber(ty)}");
        }

        private void BtnRotasi_Click(object? sender, EventArgs e)
        {
            if (!EnsureObjectExists())
                return;

            if (!TryReadNumber(txtSudut, "Sudut", out double sudut) ||
                !TryReadNumber(txtPusatX, "Pusat X", out double pusatX) ||
                !TryReadNumber(txtPusatY, "Pusat Y", out double pusatY))
                return;

            // CCW bernilai positif, CW bernilai negatif.
            double radian = sudut * Math.PI / 180.0;
            if (rbCW.Checked)
                radian = -radian;

            // Rotasi terhadap pusat (m, n) adalah komposisi:
            // T(-m, -n) x R(theta) x T(m, n)
            double[,] t1 = CreateTranslationMatrix(-pusatX, -pusatY);
            double[,] r = CreateRotationMatrix(radian);
            double[,] t2 = CreateTranslationMatrix(pusatX, pusatY);

            double[,] matrix = MultiplyMatrix(MultiplyMatrix(t1, r), t2);

            string arah = rbCW.Checked ? "CW" : "CCW";
            ApplyMatrix(matrix,
                $"Rotasi {FormatNumber(sudut)} derajat {arah} di pusat " +
                $"({FormatNumber(pusatX)}, {FormatNumber(pusatY)})");
        }

        private void BtnRefleksi_Click(object? sender, EventArgs e)
        {
            if (!EnsureObjectExists())
                return;

            double[,] matrix;

            switch (cmbSumbu.SelectedIndex)
            {
                case 0: // Sumbu X (y = 0): (x, y) -> (x, -y)
                    matrix = new double[,]
                    {
                        { 1,  0, 0 },
                        { 0, -1, 0 },
                        { 0,  0, 1 }
                    };
                    break;

                case 1: // Sumbu Y (x = 0): (x, y) -> (-x, y)
                    matrix = new double[,]
                    {
                        { -1, 0, 0 },
                        {  0, 1, 0 },
                        {  0, 0, 1 }
                    };
                    break;

                case 2: // Garis y = x: (x, y) -> (y, x)
                    matrix = new double[,]
                    {
                        { 0, 1, 0 },
                        { 1, 0, 0 },
                        { 0, 0, 1 }
                    };
                    break;

                case 3: // Garis y = -x: (x, y) -> (-y, -x)
                    matrix = new double[,]
                    {
                        {  0, -1, 0 },
                        { -1,  0, 0 },
                        {  0,  0, 1 }
                    };
                    break;

                default: // Garis sembarang y = mx + c
                    if (!TryReadNumber(txtM, "m", out double m) ||
                        !TryReadNumber(txtC, "c", out double c))
                        return;

                    double alpha = Math.Atan(m); // sudut kemiringan garis

                    // Komposisi 5 langkah:
                    // T(0,-c) x R(-alpha) x Ref_X x R(alpha) x T(0,c)
                    double[,] t1 = CreateTranslationMatrix(0, -c);
                    double[,] rMinus = CreateRotationMatrix(-alpha);
                    double[,] refX = new double[,]
                    {
                        { 1,  0, 0 },
                        { 0, -1, 0 },
                        { 0,  0, 1 }
                    };
                    double[,] rPlus = CreateRotationMatrix(alpha);
                    double[,] t2 = CreateTranslationMatrix(0, c);

                    matrix = MultiplyMatrix(
                        MultiplyMatrix(
                            MultiplyMatrix(
                                MultiplyMatrix(t1, rMinus),
                                refX),
                            rPlus),
                        t2);

                    ApplyMatrix(matrix,
                        $"Refleksi garis y = {FormatNumber(m)}x + {FormatNumber(c)}");
                    return;
            }

            string namaSumbu = cmbSumbu.SelectedItem?.ToString() ?? "sumbu";
            ApplyMatrix(matrix, $"Refleksi: {namaSumbu}");
        }

        private void BtnDilatasi_Click(object? sender, EventArgs e)
        {
            if (!EnsureObjectExists())
                return;

            if (!TryReadNumber(txtSx, "Sx", out double sx) ||
                !TryReadNumber(txtSy, "Sy", out double sy))
                return;

            // Matriks penskalaan terhadap pusat (0, 0):
            // [ Sx  0  0 ]
            // [  0 Sy  0 ]
            // [  0  0  1 ]
            double[,] matrix =
            {
                { sx, 0,  0 },
                { 0,  sy, 0 },
                { 0,  0,  1 }
            };

            ApplyMatrix(matrix, $"Dilatasi: Sx = {FormatNumber(sx)}, Sy = {FormatNumber(sy)}");
        }

        private void CmbSumbu_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool aktif = cmbSumbu.SelectedIndex == 4;
            txtM.Enabled = aktif;
            txtC.Enabled = aktif;
        }

        // =====================================================================
        // ALGORITMA 4.1: PERKALIAN MATRIKS
        // C[i,j] = jumlah (A[i,k] * B[k,j]) untuk seluruh k.
        // =====================================================================
        private static double[,] MultiplyMatrix(double[,] a, double[,] b)
        {
            int aRows = a.GetLength(0);
            int aCols = a.GetLength(1);
            int bRows = b.GetLength(0);
            int bCols = b.GetLength(1);

            if (aCols != bRows)
            {
                throw new ArgumentException(
                    "Jumlah kolom matriks A harus sama dengan jumlah baris matriks B.");
            }

            double[,] hasil = new double[aRows, bCols];

            for (int i = 0; i < aRows; i++)
            {
                for (int j = 0; j < bCols; j++)
                {
                    double jumlah = 0;
                    for (int k = 0; k < aCols; k++)
                        jumlah += a[i, k] * b[k, j];

                    hasil[i, j] = jumlah;
                }
            }

            return hasil;
        }

        // =====================================================================
        // MATRIKS-MATRIKS DASAR 3 x 3
        // =====================================================================
        private static double[,] CreateTranslationMatrix(double tx, double ty)
        {
            // Vektor baris: P' = P x M, sehingga Tx dan Ty berada di baris ke-3.
            return new double[,]
            {
                { 1, 0, 0  },
                { 0, 1, 0  },
                { tx, ty, 1 }
            };
        }

        private static double[,] CreateRotationMatrix(double radian)
        {
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);

            // Rotasi CCW terhadap titik asal (0, 0), konvensi vektor baris:
            // [  cos  sin  0 ]
            // [ -sin  cos  0 ]
            // [   0    0   1 ]
            return new double[,]
            {
                {  cos, sin, 0 },
                { -sin, cos, 0 },
                {  0,    0,  1 }
            };
        }

        // =====================================================================
        // PENERAPAN MATRIKS KE SELURUH TITIK OBJEK
        // Titik disusun sebagai matriks N x 3: [x  y  1].
        // Hasilnya: P' = P x M.
        // =====================================================================
        private void ApplyMatrix(double[,] matrix, string keterangan)
        {
            int n = originalPoints.Count;

            double[,] titik = new double[n, 3];
            for (int i = 0; i < n; i++)
            {
                titik[i, 0] = originalPoints[i].X;
                titik[i, 1] = originalPoints[i].Y;
                titik[i, 2] = 1; // koordinat homogen
            }

            double[,] hasil = MultiplyMatrix(titik, matrix);

            resultPoints.Clear();
            for (int i = 0; i < n; i++)
            {
                // Setelah perkalian, koordinat homogen w = 1 sehingga
                // x' = hasil[i,0] dan y' = hasil[i,1].
                resultPoints.Add(new PointF(
                    (float)hasil[i, 0],
                    (float)hasil[i, 1]));
            }

            lblInfo.Text = keterangan;
            pnlHasil.Invalidate();
        }

        // =====================================================================
        // MENGGAMBAR KANVAS (EVENT PAINT)
        // Gambar selalu digambar ulang dari data, sehingga tidak hilang
        // ketika kanvas di-refresh / ditutup jendela lain.
        // =====================================================================
        private void PnlAsal_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawAxes(e.Graphics, pnlAsal.ClientSize);

            if (originalPoints.Count > 0)
                DrawPolygon(e.Graphics, originalPoints, Color.Black, true);

            // Titik-titik yang sedang diklik (belum ditekan Draw).
            if (pendingPoints.Count > 0)
                DrawPendingPoints(e.Graphics);
        }

        private void PnlHasil_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawAxes(e.Graphics, pnlHasil.ClientSize);

            if (resultPoints.Count > 0)
                DrawPolygon(e.Graphics, resultPoints, Color.Black, true);
        }

        // Sumbu Kartesius tipis berwarna abu-abu muda, berpusat di (130, 130).
        private static void DrawAxes(Graphics g, Size size)
        {
            float cx = size.Width / 2f;
            float cy = size.Height / 2f;

            using var axisPen = new Pen(Color.LightGray, 1f);

            // Sumbu X (horizontal) dan sumbu Y (vertikal).
            g.DrawLine(axisPen, 0, cy, size.Width - 1, cy);
            g.DrawLine(axisPen, cx, 0, cx, size.Height - 1);

            // Label sumbu dan titik asal.
            using var font = new Font("Segoe UI", 7f);
            using var brush = new SolidBrush(Color.Gray);
            g.DrawString("X", font, brush, size.Width - 14, cy + 2);
            g.DrawString("Y", font, brush, cx + 3, 2);
            g.DrawString("0", font, brush, cx - 11, cy + 2);
        }

        private static void DrawPolygon(
            Graphics g,
            List<PointF> points,
            Color color,
            bool showVertices)
        {
            if (points.Count == 0)
                return;

            PointF[] screen = ToScreen(points);

            using var pen = new Pen(color, 2f);

            if (screen.Length >= 3)
                g.DrawPolygon(pen, screen);
            else if (screen.Length == 2)
                g.DrawLine(pen, screen[0], screen[1]);

            if (showVertices)
            {
                using var brush = new SolidBrush(Color.RoyalBlue);
                foreach (PointF p in screen)
                    g.FillEllipse(brush, p.X - 2.5f, p.Y - 2.5f, 5f, 5f);
            }
        }

        private void DrawPendingPoints(Graphics g)
        {
            PointF[] screen = ToScreen(pendingPoints);

            using var pen = new Pen(Color.RoyalBlue, 1.5f)
            {
                DashStyle = DashStyle.Dash
            };

            if (screen.Length >= 2)
                g.DrawLines(pen, screen);

            using var brush = new SolidBrush(Color.RoyalBlue);
            foreach (PointF p in screen)
                g.FillEllipse(brush, p.X - 3f, p.Y - 3f, 6f, 6f);
        }

        // =====================================================================
        // KONVERSI KOORDINAT
        // Kartesius (0,0) di tengah kanvas, sumbu Y ke atas.
        // Piksel layar memiliki sumbu Y ke bawah, sehingga Y dibalik.
        // =====================================================================
        private static PointF WorldToScreen(PointF world)
        {
            return new PointF(CanvasCenter + world.X, CanvasCenter - world.Y);
        }

        private static PointF ScreenToWorld(Point point)
        {
            return new PointF(point.X - CanvasCenter, CanvasCenter - point.Y);
        }

        private static PointF[] ToScreen(List<PointF> points)
        {
            PointF[] screen = new PointF[points.Count];
            for (int i = 0; i < points.Count; i++)
                screen[i] = WorldToScreen(points[i]);
            return screen;
        }

        // =====================================================================
        // HELPER VALIDASI DAN FORMAT
        // =====================================================================
        private bool EnsureObjectExists()
        {
            if (originalPoints.Count >= 3)
                return true;

            MessageBox.Show(
                "Gambar objek asal terlebih dahulu dengan menekan tombol Draw.",
                "Objek belum ada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        private static bool TryReadNumber(TextBox box, string nama, out double nilai)
        {
            // Menerima format desimal titik maupun koma (mis. 1.5 atau 1,5).
            string teks = box.Text.Trim().Replace(',', '.');

            if (double.TryParse(
                    teks,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out nilai) &&
                double.IsFinite(nilai))
                return true;

            MessageBox.Show(
                $"Nilai {nama} harus berupa angka yang valid.",
                "Input tidak valid",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            box.Focus();
            box.SelectAll();
            return false;
        }

        private static string FormatNumber(double nilai)
        {
            return nilai.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // =====================================================================
        // PANEL KANVAS DENGAN DOUBLE BUFFER (ANTI-FLICKER)
        // =====================================================================
        private sealed class CanvasPanel : Panel
        {
            public CanvasPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = Color.White;
                BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }
}
