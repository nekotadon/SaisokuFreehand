using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SaisokuFreehand
{
    public partial class Form1 : Form
    {
        #region 変数

        //モード
        int mode;
        int mode_free = 1;
        int mode_square = 2;
        int mode_line = 3;
        int mode_arrow = 4;
        int mode_text = 5;
        int mode_textwaku = 6;

        //ペン
        Pen DrawPen;
        int penbold;
        int penbold1 = 1;
        int penbold2 = 3;
        int penbold3 = 5;
        Color pencolor;

        //矢印の長さと角度
        double length = 25.0;//矢印長さ
        double kakudo = 30.0;//角度 度

        //マウス左ボタンが押されているか
        bool MouseLeftDownFlag = false;

        //その他
        private MouseButtons mb = MouseButtons.Left;

        //ファイルパス
        string imagefolder;
        TextLib.IniFile iniFile = new TextLib.IniFile();
        string saveformat;

        //ソフトウェアのメニュー
        int barsize = 28;
        ContextMenuStrip appmenu;

        //履歴
        bool history = false;

        public static class AppInfo
        {
            public static string Filepath => System.Reflection.Assembly.GetExecutingAssembly().Location;
            public static string Directory => Path.GetDirectoryName(Filepath);
            public static string DirectoryYen => Path.GetDirectoryName(Filepath) + @"\";
            public static string FileName => Path.GetFileName(Filepath);
            public static string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(Filepath);
            public static string Extension => Path.GetExtension(Filepath).ToLower();
        }

        #endregion

        #region 初期化

        public Form1()
        {
            InitializeComponent();

            //設定ファイル
            imagefolder = AppInfo.DirectoryYen + @"\image";

            //設定ファイル読み込みと保存
            起動時に画面をポーズするToolStripMenuItem.Checked = iniFile.GetKeyValueBool("BootMode", "pause", true, true);
            bool bootcapture = iniFile.GetKeyValueBool("BootMode", "capture", false, true);
            int mode_ini = iniFile.GetKeyValueInt("ButtonSelect", "mode", 0, 0, 3, true);
            int penbold_ini = iniFile.GetKeyValueInt("ButtonSelect", "size", 1, 0, 2, true);
            int color_ini = iniFile.GetKeyValueInt("ButtonSelect", "color", 0, 0, 3, true);
            画面をキャプチャしたら終了するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Capture", "autoexit", false, true);
            画像を自動保存するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Capture", "save", false, true);
            タイトルバーにアイコンを表示するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Soft", "Icon", false, true);
            history = iniFile.GetKeyValueBool("Soft", "History", false, true);
            historyToolStripMenuItem.Checked = history;
            saveformat = iniFile.GetKeyValueStringWithoutEmpty("Capture", "saveformat", "png");
            saveformat = saveformat.ToLower();
            if (saveformat != "png" && saveformat != "bmp" && saveformat != "jpg" && saveformat != "jpeg")
            {
                saveformat = "png";
            }
            if (saveformat == "jpeg")
            {
                saveformat = "jpg";
            }
            switch (saveformat)
            {
                case "png":
                    pNGToolStripMenuItem.Checked = true;
                    break;
                case "jpg":
                    jPGToolStripMenuItem.Checked = true;
                    break;
                case "bmp":
                    bMPToolStripMenuItem.Checked = true;
                    break;
            }

            iniFile.SetKeyValueString("Capture", "saveformat", saveformat);
            int pos_x = iniFile.GetKeyValueInt("Soft", "Top", 63, true);
            int pos_y = iniFile.GetKeyValueInt("Soft", "Left", 78, true);

            //ウィンドウ
            Text = "最速フリーハンド";
            Icon = Properties.Resources.paint;
            WindowState = FormWindowState.Maximized;
            KeyDown += Form1_KeyDown;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;

            picbox.MouseDown += Picbox_MouseDown;
            picbox.MouseUp += Picbox_MouseUp;
            picbox.MouseMove += Picbox_MouseMove;
            picbox.DoubleClick += Picbox_DoubleClick;

            //アプリケーションのコンテキストメニュー
            appmenu = new ContextMenuStrip();
            ToolStripMenuItem menu1 = new ToolStripMenuItem { Text = "閉じる(&C)" };
            menu1.Font = new Font(menu1.Font, FontStyle.Bold);
            menu1.ShortcutKeys = ((Keys)((Keys.Alt | Keys.F4)));
            menu1.Click += AppClose_Click;
            appmenu.Items.Add(menu1);

            //初期化
            if (起動時に画面をポーズするToolStripMenuItem.Checked)
            {
                DoubleBuffered = true;
                picbox.BackColor = Color.FromArgb(128, 128, 128);
            }
            else
            {
                BackColor = picbox.BackColor = TransparencyKey = Color.DarkGoldenrod;
            }

            if (bootcapture)
            {
                画面をキャプチャできる状態で起動するToolStripMenuItem.Checked = true;
                toolStripButton_camera.Checked = true;
                Cursor = Cursors.Cross;
            }
            set_mode(mode_ini + 1);
            switch (mode_ini)
            {
                case 0:
                    フリーハンドToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    四角ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    直線ToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    矢印ToolStripMenuItem.Checked = true;
                    break;
            }
            switch (penbold_ini)
            {
                case 0:
                    set_pen_bold(penbold1);
                    小ToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    set_pen_bold(penbold2);
                    中ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    set_pen_bold(penbold3);
                    大ToolStripMenuItem.Checked = true;
                    break;
            }

            switch (color_ini)
            {
                case 0:
                    set_pen_color(Color.Red);
                    赤ToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    set_pen_color(Color.Blue);
                    青ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    set_pen_color(Color.Yellow);
                    黄ToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    set_pen_color(Color.Green);
                    緑ToolStripMenuItem.Checked = true;
                    break;
            }

            //ツールバーボタン設定
            toolStripButton_pen1.Image = Properties.Resources.pen1_off.ToBitmap();
            toolStripButton_pen2.Image = Properties.Resources.pen2_off.ToBitmap();
            toolStripButton_pen3.Image = Properties.Resources.pen3_off.ToBitmap();
            toolStripButton_c_red.Image = Properties.Resources.color_red_off.ToBitmap();
            toolStripButton_c_blue.Image = Properties.Resources.color_blue_off.ToBitmap();
            toolStripButton_c_yellow.Image = Properties.Resources.color_yellow_off.ToBitmap();
            toolStripButton_c_green.Image = Properties.Resources.color_green_off.ToBitmap();
            toolStripButton_mode_free.Image = Properties.Resources.mode_free.ToBitmap();
            toolStripButton_mode_square.Image = Properties.Resources.mode_square.ToBitmap();
            toolStripButton_mode_line.Image = Properties.Resources.mode_line.ToBitmap();
            toolStripButton_mode_arrow.Image = Properties.Resources.mode_arrow.ToBitmap();
            toolStripButton_camera.Image = Properties.Resources.camera.ToBitmap();
            toolStripButton_text.Image = Properties.Resources.text.ToBitmap();
            toolStripButton_textwaku.Image = Properties.Resources.text_waku.ToBitmap();
            toolStripSplitButton_keshigomu.Image = Properties.Resources.keshigomu.ToBitmap();
            toolStripDropDownButton_setting.Image = Properties.Resources.setting.ToBitmap();
            toolStripSplitButton_keshigomu.Image = Properties.Resources.keshigomu.ToBitmap();
            toolStripSeparator_sep1.AutoSize = false;
            toolStripSeparator_sep2.AutoSize = false;
            toolStripSeparator_sep3.AutoSize = false;
            toolStripSeparator_sep4.AutoSize = false;
            toolStripSeparator_sep5.AutoSize = false;
            toolStripSeparator_sep1.Height = toolStripButton_camera.Height;
            toolStripSeparator_sep2.Height = toolStripButton_camera.Height;
            toolStripSeparator_sep3.Height = toolStripButton_camera.Height;
            toolStripSeparator_sep4.Height = toolStripButton_camera.Height;
            toolStripSeparator_sep5.Height = toolStripButton_camera.Height;
            if (!起動時に画面をポーズするToolStripMenuItem.Checked)
            {
                toolStripButton_text.Visible = false;
            }

            //設定メニューの自動スケール無効
            foreach (var c in toolStripDropDownButton_setting.DropDownItems)
            {
                if (c is ToolStripMenuItem)
                {
                    toolstrip_sizenone((ToolStripMenuItem)c);
                }
            }

            //ボタン
            button_close.Font = new Font(SystemFonts.MenuFont.FontFamily, 14);
            button_close.FlatAppearance.BorderSize = 0;

            //タイトル
            barsize = 28;
            label_title.Text = "最速フリーハンド";
            label_title.Font = SystemFonts.MenuFont;
            label_title.Location = new Point(29, (barsize - label_title.Height) / 2);

            pictureBox_icon.Size = new Size(16, 16);
            pictureBox_icon.Location = new Point(8, 6);
            pictureBox_icon.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox_icon.BackgroundImage = Properties.Resources.paint.ToBitmap();
            pictureBox_icon.Click += new EventHandler(pictureBox_icon_Click);
            pictureBox_icon.DoubleClick += new EventHandler(AppClose_Click);
            TitlebarIconView(タイトルバーにアイコンを表示するToolStripMenuItem.Checked);

            //マウスで移動
            panel1.MouseDown += new MouseEventHandler(panel1_MouseDown);
            panel1.MouseMove += new MouseEventHandler(panel1_MouseMove);
            panel1.MouseUp += new MouseEventHandler(panel1_MouseUp);
            label_title.MouseDown += new MouseEventHandler(panel1_MouseDown);
            label_title.MouseMove += new MouseEventHandler(panel1_MouseMove);
            label_title.MouseUp += new MouseEventHandler(panel1_MouseUp);

            //ツールバーの位置調整
            toolStrip1.Location = new Point(0, 0);

            button_close.Height = toolStrip1.Height;
            button_close.Width = toolStrip1.Height * 2;
            button_close.Location = new Point(toolStrip1.Width - 1, 0);
            button_close.BringToFront();

            panel_inner.Height = toolStrip1.Height;
            panel_inner.Width = toolStrip1.Width + button_close.Width;
            panel_inner.Location = new Point(0, barsize);
            panel1.Height = panel_inner.Height + barsize;
            panel1.Width = panel_inner.Width;
            panel1.BackColor = Color.White;

            int display_h = Screen.GetBounds(this).Height;
            int display_w = Screen.GetBounds(this).Width;

            if (起動時に画面をポーズするToolStripMenuItem.Checked)
            {
                //デスクトップキャプチャイメージで初期化
                Bitmap bmp = new Bitmap(display_w, display_h);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp.Size);
                bmp.MakeTransparent(Color.FromArgb(128, 128, 128));
                img_blank = new Bitmap(bmp);
                bmp.Dispose();
            }
            else
            {
                //空白のイメージで初期化
                img_blank = new Bitmap((int)((double)display_w * 1.01), (int)((double)display_h * 1.01), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            //イメージを初期化
            array_img = null;
            array_img = new List<Bitmap>();
            is_blank = false;
            idx = idxmax();
            init();

            ActiveControl = picbox;

            //位置決めと設定保存
            if (pos_x >= display_w - panel1.Width)
            {
                pos_x = display_w - panel1.Width;
            }
            if (pos_y >= display_h - panel1.Height)
            {
                pos_y = display_h - panel1.Height;
            }
            if (pos_x < 0)
            {
                pos_x = 0;
            }
            if (pos_y < 0)
            {
                pos_y = 0;
            }
            panel1.Location = new Point(pos_x, pos_y);
            iniFile.SetKeyValueInt("Soft", "Top", pos_x);
            iniFile.SetKeyValueInt("Soft", "Left", pos_y);
        }

        private void toolstrip_sizenone(ToolStripMenuItem item)
        {
            item.ImageScaling = ToolStripItemImageScaling.None;
            foreach (var c in item.DropDownItems)
            {
                if (c is ToolStripMenuItem)
                {
                    toolstrip_sizenone((ToolStripMenuItem)c);
                }
            }
        }

        #endregion

        #region フォーム

        private void AppClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void pictureBox_icon_Click(object sender, EventArgs e)
        {
            appmenu_show();
        }
        private void TitlebarIconView(bool view)
        {
            pictureBox_icon.Visible = view;
            label_title.Left = view ? 29 : 5;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            iniFile.SetKeyValueInt("Soft", "Top", panel1.Location.X);
            iniFile.SetKeyValueInt("Soft", "Left", panel1.Location.Y);
        }

        #endregion

        #region イメージの履歴と初期化

        //イメージの履歴
        List<Bitmap> array_img;
        int idx;
        Bitmap img_stock = null;

        //空白のイメージ
        Bitmap img_blank = null;
        bool is_blank;

        int idxmax()
        {
            return array_img.Count - 1;
        }
        void addimg(Image img)
        {
            //現在のイメージが最後のイメージでない場合は要素削除
            if (idx < idxmax())
            {
                for (int i = idxmax(); i > idx; i--)//後ろから消していく
                {
                    array_img[i].Dispose();
                    array_img[i] = null;
                    array_img.RemoveAt(i);
                }
            }

            //履歴有効でなければすべて削除
            if (!history)
            {
                idx = -1;
                for (int i = idxmax(); i >= 0; i--)
                {
                    array_img[i].Dispose();
                    array_img[i] = null;
                    array_img.RemoveAt(i);
                }
            }

            //イメージのストック
            array_img.Add(new Bitmap(img));
            idx++;
        }
        void img_set()
        {
            if (0 <= idx && idx <= idxmax() && array_img[idx] != null)
            {
                img_change(new Bitmap(array_img[idx]));
            }
        }
        void img_change(Image newImage)
        {
            if (newImage != null)
            {
                var oldImg = picbox.Image;
                picbox.Image = newImage;
                if (oldImg != null)
                {
                    oldImg.Dispose();
                }
            }
        }

        //初期化
        void init()
        {
            if (!is_blank)
            {
                //イメージのストック
                addimg(img_blank);

                //更新
                img_set();

                //現在空白
                is_blank = true;
            }
        }

        //Ctrl+Z,Y
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (history)
            {
                //Ctrl+Zで戻す
                if (e.KeyCode == Keys.Z && e.Control == true)
                {
                    if (array_img != null)
                    {
                        if (idx > 0)
                        {
                            idx--;
                            img_set();
                        }
                    }
                }
                //F1で戻す
                else if (e.KeyCode == Keys.F1)
                {
                    if (array_img != null)
                    {
                        if (idx > 0)
                        {
                            idx--;
                            img_set();
                        }
                    }
                }
                //Ctrl+Yで戻すのを戻す
                else if (e.KeyCode == Keys.Y && e.Control == true)
                {
                    if (array_img != null)
                    {
                        if (idx < idxmax())
                        {
                            idx++;
                            img_set();
                        }
                    }
                }
                //F2で戻すのを戻す
                else if (e.KeyCode == Keys.F2)
                {
                    if (array_img != null)
                    {
                        if (idx < idxmax())
                        {
                            idx++;
                            img_set();
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Space && e.Alt == true)
            {
                appmenu_show();
            }
        }
        private void appmenu_show()
        {
            Point p = panel1.PointToScreen(new Point(-1, barsize));
            appmenu.Show(p);
        }

        #endregion

        #region マウス処理（描画、初期化）

        // マウス位置
        bool jikkou = true;
        Point LastMousePoint;
        Point MousePoint_down;
        Point MousePoint_up;

        //Pictureboxイベント
        private void Picbox_MouseDown(object sender, MouseEventArgs e)//マウスDown
        {
            if (ActiveControl != picbox)
            {
                ActiveControl = picbox;
            }

            if (!toolStripButton_camera.Checked && (toolStripButton_text.Checked || toolStripButton_textwaku.Checked))
            {
                jikkou = false;
                return;
            }
            else
            {
                //マウス左ボタンが押されたとき
                if (e.Button == MouseButtons.Left)
                {
                    jikkou = true;
                    if (!MouseLeftDownFlag)
                    {
                        //現在のイメージ記憶
                        if (img_stock != null)
                        {
                            img_stock.Dispose();
                            img_stock = null;
                        }
                        if (picbox.Image != null)
                        {
                            try
                            {
                                if (picbox.Image.Width <= 10)
                                {
                                    ;
                                }
                            }
                            catch (Exception)
                            {
                                //全て初期化
                                for (int i = idxmax(); i >= 0; i--)//後ろから消していく
                                {
                                    try
                                    {
                                        array_img[i].Dispose();
                                        array_img[i] = null;
                                        array_img.RemoveAt(i);
                                    }
                                    catch (Exception)
                                    {
                                        ;
                                    }
                                }
                                array_img = new List<Bitmap>();
                                is_blank = false;
                                idx = idxmax();
                                init();
                            }

                            img_stock = new Bitmap(picbox.Image);
                        }
                        else
                        {
                            return;
                        }
                        //フラグON
                        MouseLeftDownFlag = true;

                        //マウス位置記憶
                        MousePoint_down = new Point(e.X, e.Y);
                    }

                    //押された時の位置を記憶
                    LastMousePoint = new Point(e.X, e.Y);
                }
                else
                {
                    //右クリックされた場合
                    jikkou = false;
                    camera_off();
                }
            }
        }
        private void Picbox_MouseUp(object sender, MouseEventArgs e)//マウスUp
        {
            if (!toolStripButton_camera.Checked && (toolStripButton_text.Checked || toolStripButton_textwaku.Checked))
            {
                if (e.Button == MouseButtons.Left && (Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left && (Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
                {
                    textinputfunc();
                }
                return;
            }

            if (!jikkou) return;

            //フラグOFF
            MouseLeftDownFlag = false;
            if (e.Button == MouseButtons.Left)
            {
                MousePoint_up = new Point(e.X, e.Y);

                if (MousePoint_down != MousePoint_up && img_stock != null)
                {
                    if (toolStripButton_camera.Checked)
                    {
                        img_change(new Bitmap(img_stock));
                        picbox.Refresh();

                        //開始位置、終了位置
                        Point p1 = MousePoint_down;
                        Point p4 = new Point();
                        square_point(p1, new Point(e.X, e.Y), ref p4);

                        p1 = PointToScreen(p1);
                        p4 = PointToScreen(p4);

                        Point p2 = new Point(p4.X, p1.Y);
                        Point p3 = new Point(p1.X, p4.Y);

                        if (Math.Abs(p1.X - p4.X) >= 2 && Math.Abs(p1.Y - p4.Y) >= 2)
                        {
                            Point start = new Point();
                            Point end = new Point();
                            if (p1.X > p4.X)
                            {
                                if (p1.Y > p4.Y)
                                {
                                    start = p4;
                                    end = p1;
                                }
                                else
                                {
                                    start = p2;
                                    end = p3;
                                }
                            }
                            else
                            {
                                if (p1.Y > p4.Y)
                                {
                                    start = p3;
                                    end = p2;
                                }
                                else
                                {
                                    start = p1;
                                    end = p4;
                                }
                            }

                            try
                            {
                                //Bitmapの作成
                                Bitmap bmp = new Bitmap(end.X - start.X, end.Y - start.Y);

                                //Graphicsの作成
                                Graphics g = Graphics.FromImage(bmp);

                                //コピー
                                g.CopyFromScreen(start, new Point(0, 0), bmp.Size);

                                //解放
                                g.Dispose();

                                //クリップボードへコピー
                                Clipboard.SetImage(bmp);

                                //画像を保存
                                if (画像を自動保存するToolStripMenuItem.Checked)
                                {
                                    if (!Directory.Exists(imagefolder))
                                    {
                                        try
                                        {
                                            DirectoryInfo di = Directory.CreateDirectory(imagefolder);
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }

                                    if (Directory.Exists(imagefolder))
                                    {
                                        if (saveformat == "png" || saveformat == "jpg" || saveformat == "bmp")
                                        {
                                            string filename = imagefolder + @"\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_ff") + "." + saveformat;
                                            try
                                            {
                                                if (saveformat == "jpg")
                                                {
                                                    bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                }
                                                else if (saveformat == "bmp")
                                                {
                                                    bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                                                }
                                                else
                                                {
                                                    bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                ;
                                            }
                                        }
                                    }
                                }

                                //破棄
                                bmp.Dispose();

                                //キャプチャ後終了
                                if (画面をキャプチャしたら終了するToolStripMenuItem.Checked)
                                {
                                    Application.Exit();
                                }

                                toolStripButton_camera.Image = Properties.Resources.camera_on.ToBitmap();
                                camera_count = 0;
                                timer1.Enabled = true;
                            }
                            catch (Exception)
                            {
                                ;
                            }
                        }
                    }
                    else
                    {
                        if (mode == mode_square)
                        {
                            img_change(new Bitmap(img_stock));

                            using (var graphics = Graphics.FromImage(picbox.Image))
                            {
                                //開始位置、終了位置
                                Point p1 = MousePoint_down;
                                Point p4 = new Point();
                                square_point(p1, new Point(e.X, e.Y), ref p4);
                                Point p2 = new Point(p4.X, p1.Y);
                                Point p3 = new Point(p1.X, p4.Y);

                                //開始位置から終了位置へ直線を引く
                                graphics.DrawLine(DrawPen, p1, p2);
                                graphics.DrawLine(DrawPen, p2, p4);
                                graphics.DrawLine(DrawPen, p4, p3);
                                graphics.DrawLine(DrawPen, p3, p1);
                            }

                            picbox.Refresh();
                        }
                        else if (mode == mode_line)
                        {
                            img_change(new Bitmap(img_stock));

                            using (var graphics = Graphics.FromImage(picbox.Image))
                            {
                                //開始位置、終了位置
                                Point p1 = MousePoint_down;
                                Point p2 = new Point(e.X, e.Y);
                                Point p2d = new Point();
                                line_point(p1, p2, ref p2d);

                                //開始位置から終了位置へ直線を引く
                                graphics.DrawLine(DrawPen, p1, p2d);
                            }

                            picbox.Refresh();
                        }
                        else if (mode == mode_arrow)
                        {
                            img_change(new Bitmap(img_stock));

                            using (var graphics = Graphics.FromImage(picbox.Image))
                            {
                                //開始位置、終了位置
                                Point p1 = MousePoint_down;
                                Point p2 = new Point();
                                line_point(p1, new Point(e.X, e.Y), ref p2);

                                //開始位置から終了位置へ直線を引く
                                graphics.DrawLine(DrawPen, p1, p2);

                                //矢印
                                Point pa1 = new Point();
                                Point pa2 = new Point();
                                arrow_point(p1, p2, ref pa1, ref pa2);

                                graphics.DrawLine(DrawPen, p2, pa1);
                                graphics.DrawLine(DrawPen, p2, pa2);
                            }

                            picbox.Refresh();
                        }
                    }

                    img_stock.Dispose();
                    img_stock = null;

                    addimg(picbox.Image);
                    is_blank = false;
                }
            }

            //ダブルクリック用
            mb = e.Button;
        }

        //左ダブルクリックでクリア
        private void Picbox_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (mb.Equals(MouseButtons.Left))
                {
                    init();
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        //カメラシャッター
        int camera_count = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            camera_count++;

            if (camera_count >= 2)
            {
                timer1.Enabled = false;
                toolStripButton_camera.Image = Properties.Resources.camera.ToBitmap();
                camera_count = 0;
            }
        }

        private void Picbox_MouseMove(object sender, MouseEventArgs e)//マウスMove
        {
            if (!jikkou) return;

            if (MouseLeftDownFlag == false || picbox.Image == null || img_stock == null)
            {
                return;
            }

            if (toolStripButton_camera.Checked)
            {
                using (var b = new Bitmap(img_stock))
                {
                    img_change(b);

                    using (var graphics = Graphics.FromImage(picbox.Image))
                    {
                        //開始位置、終了位置
                        Point p1 = MousePoint_down;
                        Point p4 = new Point();
                        square_point(p1, new Point(e.X, e.Y), ref p4);
                        Point p2 = new Point(p4.X, p1.Y);
                        Point p3 = new Point(p1.X, p4.Y);

                        //開始位置から終了位置へ直線を引く
                        Pen DrawPendummy_b = new Pen(Color.Black, 1);
                        Pen DrawPendummy_w = new Pen(Color.White, 1);

                        int i0, i1;
                        bool kuro;
                        //1->2
                        i0 = p1.X < p2.X ? p1.X : p2.X;
                        i1 = p1.X < p2.X ? p2.X : p1.X;
                        kuro = false;
                        for (int i = i0; i <= i1 - 1; i += 2)
                        {
                            kuro = !kuro;
                            Pen DrawPendummy = kuro ? DrawPendummy_b : DrawPendummy_w;
                            graphics.DrawLine(DrawPendummy, new Point(i, p1.Y), new Point(i + 1, p1.Y));
                        }

                        //2->4
                        i0 = p2.Y < p4.Y ? p2.Y : p4.Y;
                        i1 = p2.Y < p4.Y ? p4.Y : p2.Y;
                        kuro = false;
                        for (int i = i0; i <= i1 - 1; i += 2)
                        {
                            kuro = !kuro;
                            Pen DrawPendummy = kuro ? DrawPendummy_b : DrawPendummy_w;
                            graphics.DrawLine(DrawPendummy, new Point(p2.X, i), new Point(p2.X, i + 1));
                        }

                        //4->3
                        i0 = p4.X < p3.X ? p4.X : p3.X;
                        i1 = p4.X < p3.X ? p3.X : p4.X;
                        kuro = false;
                        for (int i = i0; i <= i1 - 1; i += 2)
                        {
                            kuro = !kuro;
                            Pen DrawPendummy = kuro ? DrawPendummy_b : DrawPendummy_w;
                            graphics.DrawLine(DrawPendummy, new Point(i, p4.Y), new Point(i + 1, p4.Y));
                        }

                        //3->1
                        i0 = p3.Y < p1.Y ? p3.Y : p1.Y;
                        i1 = p3.Y < p1.Y ? p1.Y : p3.Y;
                        kuro = false;
                        for (int i = i0; i <= i1 - 1; i += 2)
                        {
                            kuro = !kuro;
                            Pen DrawPendummy = kuro ? DrawPendummy_b : DrawPendummy_w;
                            graphics.DrawLine(DrawPendummy, new Point(p3.X, i), new Point(p3.X, i + 1));
                        }
                    }

                    picbox.Refresh();
                }
            }
            else
            {
                if (mode == mode_free)
                {
                    using (var graphics = Graphics.FromImage(picbox.Image))
                    {
                        //開始位置、終了位置
                        Point startPoint = LastMousePoint;
                        Point endPoint = new Point(e.X, e.Y);

                        //開始位置から終了位置へ直線を引く
                        graphics.DrawLine(DrawPen, startPoint, endPoint);
                    }

                    picbox.Refresh();

                    //マウスの位置を記憶
                    LastMousePoint = new Point(e.X, e.Y);

                }
                else if (mode == mode_square)
                {
                    using (var b = new Bitmap(img_stock))
                    {
                        img_change(b);

                        using (var graphics = Graphics.FromImage(picbox.Image))
                        {
                            //開始位置、終了位置
                            Point p1 = MousePoint_down;
                            Point p4 = new Point();
                            square_point(p1, new Point(e.X, e.Y), ref p4);
                            Point p2 = new Point(p4.X, p1.Y);
                            Point p3 = new Point(p1.X, p4.Y);

                            //開始位置から終了位置へ直線を引く
                            graphics.DrawLine(DrawPen, p1, p2);
                            graphics.DrawLine(DrawPen, p2, p4);
                            graphics.DrawLine(DrawPen, p4, p3);
                            graphics.DrawLine(DrawPen, p3, p1);
                        }

                        picbox.Refresh();
                    }
                }
                else if (mode == mode_line)
                {
                    using (var b = new Bitmap(img_stock))
                    {
                        img_change(b);

                        using (var graphics = Graphics.FromImage(picbox.Image))
                        {
                            //開始位置、終了位置
                            Point p1 = MousePoint_down;
                            Point p2 = new Point(e.X, e.Y);
                            Point p2d = new Point();
                            line_point(p1, p2, ref p2d);

                            //開始位置から終了位置へ直線を引く
                            graphics.DrawLine(DrawPen, p1, p2d);
                        }

                        picbox.Refresh();
                    }
                }
                else if (mode == mode_arrow)
                {
                    using (var b = new Bitmap(img_stock))
                    {
                        img_change(b);

                        using (var graphics = Graphics.FromImage(picbox.Image))
                        {
                            //開始位置、終了位置
                            Point p1 = MousePoint_down;
                            Point p2 = new Point();
                            line_point(p1, new Point(e.X, e.Y), ref p2);

                            //開始位置から終了位置へ直線を引く
                            graphics.DrawLine(DrawPen, p1, p2);

                            //矢印
                            Point pa1 = new Point();
                            Point pa2 = new Point();
                            arrow_point(p1, p2, ref pa1, ref pa2);

                            graphics.DrawLine(DrawPen, p2, pa1);
                            graphics.DrawLine(DrawPen, p2, pa2);
                        }

                        picbox.Refresh();
                    }
                }
            }
        }
        private void arrow_point(Point p1, Point p2, ref Point pa1, ref Point pa2)
        {
            if (p2.X != p1.X)
            {
                double k = ((double)p2.Y - (double)p1.Y) / ((double)p2.X - (double)p1.X);
                double shita = Math.Atan(k);
                double shita1 = shita + kakudo / 180.0 * Math.PI;
                double shita2 = shita - kakudo / 180.0 * Math.PI;

                if (p2.X > p1.X)
                {
                    pa1 = new Point(p2.X - (int)(Math.Cos(shita1) * length), p2.Y - (int)(Math.Sin(shita1) * length));
                    pa2 = new Point(p2.X - (int)(Math.Cos(shita2) * length), p2.Y - (int)(Math.Sin(shita2) * length));
                }
                else
                {
                    pa1 = new Point(p2.X + (int)(Math.Cos(shita1) * length), p2.Y + (int)(Math.Sin(shita1) * length));
                    pa2 = new Point(p2.X + (int)(Math.Cos(shita2) * length), p2.Y + (int)(Math.Sin(shita2) * length));
                }
            }
            else
            {
                double shita = kakudo / 180.0 * Math.PI;

                if (p2.Y > p1.Y)
                {
                    pa1 = new Point(p2.X - (int)(Math.Sin(shita) * length), p2.Y - (int)(Math.Cos(shita) * length));
                    pa2 = new Point(p2.X + (int)(Math.Sin(shita) * length), p2.Y - (int)(Math.Cos(shita) * length));
                }
                else
                {
                    pa1 = new Point(p2.X - (int)(Math.Sin(shita) * length), p2.Y + (int)(Math.Cos(shita) * length));
                    pa2 = new Point(p2.X + (int)(Math.Sin(shita) * length), p2.Y + (int)(Math.Cos(shita) * length));
                }
            }
        }
        private void line_point(Point p1, Point p2, ref Point p2d)
        {
            if (p1.X != p2.X && p1.Y != p2.Y && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                double k = Math.Abs((double)p2.Y - (double)p1.Y) / Math.Abs((double)p2.X - (double)p1.X);
                double shita = Math.Atan(k);

                if (shita <= Math.PI * 1.0 / 8.0)
                {
                    p2d = new Point(p2.X, p1.Y);
                }
                else if (shita <= Math.PI * 2.0 / 8.0)
                {
                    if (p1.X < p2.X)
                    {
                        p2d = new Point(p1.X + Math.Abs(p2.Y - p1.Y), p2.Y);
                    }
                    else
                    {
                        p2d = new Point(p1.X - Math.Abs(p2.Y - p1.Y), p2.Y);
                    }
                }
                else if (shita <= Math.PI * 3.0 / 8.0)
                {
                    if (p1.Y < p2.Y)
                    {
                        p2d = new Point(p2.X, p1.Y + Math.Abs(p2.X - p1.X));
                    }
                    else
                    {
                        p2d = new Point(p2.X, p1.Y - Math.Abs(p2.X - p1.X));
                    }
                }
                else
                {
                    p2d = new Point(p1.X, p2.Y);
                }
            }
            else
            {
                p2d = p2;
            }
        }
        private void square_point(Point p1, Point p2, ref Point p4)
        {
            int dx = Math.Abs(p1.X - p2.X);
            int dy = Math.Abs(p1.Y - p2.Y);

            if (dx != dy && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (dx > dy)
                {
                    if (p1.Y > p2.Y)
                    {
                        p4 = new Point(p2.X, p1.Y - dx);
                    }
                    else
                    {
                        p4 = new Point(p2.X, p1.Y + dx);
                    }
                }
                else
                {
                    if (p1.X > p2.X)
                    {
                        p4 = new Point(p1.X - dy, p2.Y);
                    }
                    else
                    {
                        p4 = new Point(p1.X + dy, p2.Y);
                    }
                }
            }
            else
            {
                p4 = p2;
            }
        }

        #endregion

        #region ツールバー

        //ツールバーのマウスでの移動
        private Point lastMousePosition;
        private bool mouseCapture;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            lastMousePosition = MousePosition;
            mouseCapture = true;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseCapture == false)
            {
                return;
            }

            Point mp = MousePosition;

            int offsetX = mp.X - lastMousePosition.X;
            int offsetY = mp.Y - lastMousePosition.Y;

            panel1.Location = new Point(panel1.Left + offsetX, panel1.Top + offsetY);

            lastMousePosition = mp;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            mouseCapture = false;
        }

        //ツールバーでのマウスのカーソル
        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void panel1_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = toolStripButton_camera.Checked ? Cursors.Cross : Cursors.Default;
        }

        //カメラ
        private void toolStripButton_camera_Click(object sender, EventArgs e)
        {
            toolStripButton_camera.Checked = !toolStripButton_camera.Checked;
        }

        //消しゴムボタン
        private void camera_off()
        {
            toolStripButton_camera.Checked = false;
            Cursor = Cursors.Default;
        }
        private void toolStripSplitButton_keshigomu_ButtonClick(object sender, EventArgs e)
        {
            init();
            deleteAllText();
            camera_off();
        }
        private void すべての線を削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            init();
            camera_off();
        }

        private void すべてのテキストを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteAllText();
            camera_off();
        }

        private void すべての線とテキストを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            init();
            deleteAllText();
            camera_off();
        }

        private void deleteAllText()
        {
            bool keizoku = true;
            for (; keizoku;)
            {
                keizoku = false;
                foreach (Control c in picbox.Controls)
                {
                    if (c is label_Panel)
                    {
                        picbox.Controls.Remove(c);
                        keizoku = true;
                        break;
                    }
                }
            }
        }

        //設定ボタン
        private void setting_change(object sender, EventArgs e)
        {
            string text = "";

            if (sender.GetType() == typeof(ToolStripMenuItem))
            {
                text = ((ToolStripMenuItem)sender).Text;
            }
            else if (sender.GetType() == typeof(ToolStripButton))
            {
                text = ((ToolStripButton)sender).Text;
            }

            if (text != "")
            {
                camera_off();

                //モード変更
                if (text == toolStripButton_mode_free.Text)
                {
                    set_mode(mode_free);
                }
                else if (text == toolStripButton_mode_square.Text)
                {
                    set_mode(mode_square);
                }
                else if (text == toolStripButton_mode_line.Text)
                {
                    set_mode(mode_line);
                }
                else if (text == toolStripButton_mode_arrow.Text)
                {
                    set_mode(mode_arrow);
                }
                else if (text == toolStripButton_text.Text)
                {
                    set_mode(mode_text);
                }
                else if (text == toolStripButton_textwaku.Text)
                {
                    set_mode(mode_textwaku);
                }
                //ペンサイズ変更
                else if (text == toolStripButton_pen1.Text)
                {
                    set_pen_bold(penbold1);
                }
                else if (text == toolStripButton_pen2.Text)
                {
                    set_pen_bold(penbold2);
                }
                else if (text == toolStripButton_pen3.Text)
                {
                    set_pen_bold(penbold3);
                }
                //ペン色変更
                else if (text == toolStripButton_c_red.Text)
                {
                    set_pen_color(Color.Red);
                }
                else if (text == toolStripButton_c_blue.Text)
                {
                    set_pen_color(Color.Blue);
                }
                else if (text == toolStripButton_c_yellow.Text)
                {
                    set_pen_color(Color.Yellow);
                }
                else if (text == toolStripButton_c_green.Text)
                {
                    set_pen_color(Color.Green);
                }
            }
        }
        void set_mode(int i)
        {
            mode = i;

            toolStripButton_mode_free.Checked = false;
            toolStripButton_mode_square.Checked = false;
            toolStripButton_mode_line.Checked = false;
            toolStripButton_mode_arrow.Checked = false;
            toolStripButton_text.Checked = false;
            toolStripButton_textwaku.Checked = false;

            if (mode == mode_free)
            {
                toolStripButton_mode_free.Checked = true;
            }
            else if (mode == mode_square)
            {
                toolStripButton_mode_square.Checked = true;
            }
            else if (mode == mode_line)
            {
                toolStripButton_mode_line.Checked = true;
            }
            else if (mode == mode_arrow)
            {
                toolStripButton_mode_arrow.Checked = true;
            }
            else if (mode == mode_text)
            {
                toolStripButton_text.Checked = true;
            }
            else if (mode == mode_textwaku)
            {
                toolStripButton_textwaku.Checked = true;
            }
        }
        void set_pen_bold(int i)
        {
            penbold = i;

            DrawPen = new Pen(pencolor, penbold);

            toolStripButton_pen1.Checked = false;
            toolStripButton_pen2.Checked = false;
            toolStripButton_pen3.Checked = false;

            if (penbold == penbold1)
            {
                toolStripButton_pen1.Checked = true;
            }
            else if (penbold == penbold2)
            {
                toolStripButton_pen2.Checked = true;
            }
            else if (penbold == penbold3)
            {
                toolStripButton_pen3.Checked = true;
            }
        }
        void set_pen_color(Color i)
        {
            pencolor = i;

            DrawPen = new Pen(pencolor, penbold);

            //アイコン変更
            toolStripButton_c_red.Checked = false;
            toolStripButton_c_blue.Checked = false;
            toolStripButton_c_yellow.Checked = false;
            toolStripButton_c_green.Checked = false;

            if (pencolor == Color.Red)
            {
                toolStripButton_c_red.Checked = true;
            }
            else if (pencolor == Color.Blue)
            {
                toolStripButton_c_blue.Checked = true;
            }
            else if (pencolor == Color.Yellow)
            {
                toolStripButton_c_yellow.Checked = true;
            }
            else if (pencolor == Color.Green)
            {
                toolStripButton_c_green.Checked = true;
            }
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region ラベル

        //ラベルの追加
        private void textinputfunc()
        {
            //現在テキスト編集中であれば追加しない
            foreach (var c in picbox.Controls)
            {
                if (c is label_Panel)
                {
                    if (((label_Panel)c).input_now)
                    {
                        ((label_Panel)c).input_now = false;
                        return;
                    }
                }
            }

            label_Panel lp = new label_Panel(pencolor, penbold, toolStripButton_textwaku.Checked, picbox, 起動時に画面をポーズするToolStripMenuItem.Checked);
            picbox.Controls.Add(lp);
            lp.BringToFront();
            panel1.BringToFront();
        }

        public class label_Panel : Panel
        {
            private Label label;
            private TextBox textBox;
            private Color color;
            private Color backcolor;
            private int bold;
            private bool waku;
            private PictureBox pic;
            public bool input_now = false;
            private string initext = "ドラッグで移動。ダブルクリックで編集。Alt+Enterで改行。Enterで確定";
            int size;


            public label_Panel(Color _color, int _bold, bool _waku, PictureBox _pic, bool wakumenu)
            {
                //代入
                color = _color;
                bold = _bold;
                waku = _waku;
                pic = _pic;

                //色
                backcolor = color == Color.Yellow ? Color.FromArgb(100, 100, 100) : Color.White;

                //ラベルサイズ
                size = 14;
                if (bold == 1) { size = 10; }
                if (bold == 3) { size = 14; }
                if (bold == 5) { size = 22; }

                //ラベルのコンテキストメニューの作成
                ContextMenuStrip cms = new ContextMenuStrip();

                ToolStripMenuItem menu1 = new ToolStripMenuItem { Text = "削除" };
                menu1.Click += contextMenuStrip_DeleteClick;
                cms.Items.Add(menu1);

                ToolStripSeparator sep1 = new ToolStripSeparator();
                cms.Items.Add(sep1);

                if (wakumenu)
                {
                    ToolStripMenuItem menu2 = new ToolStripMenuItem { Text = "枠で囲う" };
                    menu2.Click += contextMenuStrip_WakuClick;
                    cms.Items.Add(menu2);

                    ToolStripSeparator sep2 = new ToolStripSeparator();
                    cms.Items.Add(sep2);
                }

                int[] fontsize_arr = { 6, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

                foreach (int fsize in fontsize_arr)
                {
                    ToolStripMenuItem fmenu = new ToolStripMenuItem
                    {
                        Text = fsize.ToString(),
                        Name = "fsize" + fsize.ToString()
                    };
                    fmenu.Click += contextMenuStrip_FontSizeClick;
                    cms.Items.Add(fmenu);
                }

                cms.Opening += contextMenuStrip_MenuOpening;

                //ラベル作成
                label = new Label
                {
                    Text = initext,
                    Font = new Font(SystemFonts.MenuFont.Name, size),
                    AutoSize = true,
                    Visible = true,
                    ForeColor = color,
                    BackColor = waku ? backcolor : Color.Transparent,
                    Location = new Point(waku ? bold : 0, waku ? bold : 0),
                    ContextMenuStrip = cms
                };
                label.MouseDown += new MouseEventHandler(control_MouseDown);
                label.MouseMove += new MouseEventHandler(control_MouseMove);
                label.MouseDoubleClick += new MouseEventHandler(control_MouseDoubleClick);

                //テキストボックス作成
                textBox = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    ForeColor = label.ForeColor,
                    BackColor = backcolor,
                    Font = label.Font,
                    Location = label.Location,
                    Visible = false
                };
                textBox.KeyDown += new KeyEventHandler(textBox_KeyDown);
                textBox.Leave += new EventHandler(textBox_Leave);
                textBox.TextChanged += new EventHandler(textBox_TextChanged);

                //コントロールの追加
                Controls.Add(label);
                Controls.Add(textBox);

                //パネル
                MouseDown += new MouseEventHandler(control_MouseDown);
                MouseMove += new MouseEventHandler(control_MouseMove);
                MouseDoubleClick += new MouseEventHandler(control_MouseDoubleClick);
                Leave += new EventHandler(panel_Leave);
                BackColor = waku ? color : Color.Transparent;
                Width = label.Width + (waku ? bold * 2 : 0);
                Height = label.Height + (waku ? bold * 2 : 0);
                Location = pic.PointToClient(Cursor.Position);
            }

            //コンテキストメニュー
            //削除
            private void contextMenuStrip_DeleteClick(object sender, EventArgs e)
            {
                pic.Controls.Remove(this);
            }
            private void contextMenuStrip_WakuClick(object sender, EventArgs e)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                waku = !menu.Checked;

                //色設定の変更
                label.BackColor = waku ? backcolor : Color.Transparent;
                BackColor = waku ? color : Color.Transparent;

                //サイズ変更
                Width = label.Width + (waku ? bold * 2 : 0);
                Height = label.Height + (waku ? bold * 2 : 0);

                //位置変更
                label.Location = new Point(waku ? bold : 0, waku ? bold : 0);
                if (waku)
                {
                    Location = new Point(Left - bold, Top - bold);
                }
                else
                {
                    Location = new Point(Left + bold, Top + bold);
                }
            }

            //メニューにチェックを入れる
            private void contextMenuStrip_MenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
            {
                ContextMenuStrip menu = (ContextMenuStrip)sender;

                //現在のラベルのフォントサイズを取得
                float size = label.Font.Size;

                foreach (var item in menu.Items)
                {
                    if (item.GetType().Equals(typeof(ToolStripMenuItem)))
                    {
                        //フォントサイズ変更メニューであれば
                        if (((ToolStripMenuItem)item).Name.Contains("fsize"))
                        {
                            //現在のラベルのフォントサイズであればチェックを入れる
                            if (((ToolStripMenuItem)item).Text == size.ToString())
                            {
                                ((ToolStripMenuItem)item).Checked = true;
                            }
                            else
                            {
                                ((ToolStripMenuItem)item).Checked = false;
                            }
                        }
                        else if (((ToolStripMenuItem)item).Text.Contains("枠"))
                        {
                            ((ToolStripMenuItem)item).Checked = waku;
                        }
                    }
                }
            }

            //ラベルサイズ変更
            private void contextMenuStrip_FontSizeClick(object sender, EventArgs e)
            {
                //メニュー名
                string menutext = ((ToolStripMenuItem)sender).Text;

                //メニュー名を数字に
                size = int.Parse(menutext);

                //フォントサイズ変更
                label.Font = new Font(SystemFonts.MenuFont.Name, size);

                Width = label.Width + (waku ? bold * 2 : 0);
                Height = label.Height + (waku ? bold * 2 : 0);
            }

            //マウスで位置を変更
            int x_mouse;
            int y_mouse;
            int x_down;
            int y_down;
            private void control_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    x_mouse = Cursor.Position.X;
                    y_mouse = Cursor.Position.Y;
                    x_down = Left;
                    y_down = Top;
                }
            }
            private void control_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Left = x_down + (Cursor.Position.X - x_mouse);
                    Top = y_down + (Cursor.Position.Y - y_mouse);
                }
            }

            //マウスダブルクリックで編集
            private void control_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                textBox.Multiline = true;
                textBox.Text = label.Text;
                textBox.Font = new Font(label.Font, FontStyle.Underline);
                textBox.Height = label.Height;
                textBox.Width = label.Width + 50;
                textBox.Location = label.Location;
                textBox.ImeMode = ImeMode.NoControl;
                textBox.Visible = true;
                label.Visible = false;

                Width = textBox.Width + (waku ? bold * 2 : 0);
                Height = textBox.Height + (waku ? bold * 2 : 0);

                textBox.Focus();
                textBox.Select(0, 0);
                textBox.Select(textBox.Text.Length, 0);

                if (textBox.Text == initext)
                {
                    textBox.SelectAll();
                }

                input_now = true;
            }

            //テキスト編集中にリアルタイムでサイズ変更
            private void textBox_TextChanged(object sender, EventArgs e)
            {
                //サイズ判定用ラベル
                Label dummy = new Label
                {
                    Visible = false,
                    Font = textBox.Font,
                    Text = textBox.Text,
                    AutoSize = true
                };
                Controls.Add(dummy);

                //高さ差異確認用ラベル
                Label dummy2 = new Label
                {
                    Visible = false,
                    Font = textBox.Font,
                    AutoSize = true,
                    Text = "A"
                };
                Controls.Add(dummy2);

                //高さ差異確認用テキストボックス
                TextBox dummy3 = new TextBox
                {
                    Visible = false,
                    Font = textBox.Font,
                    BorderStyle = textBox.BorderStyle,
                    Text = "A"
                };
                Controls.Add(dummy3);

                int diffh = dummy3.Height - dummy2.Height;

                //改行補正
                if (textBox.Text.EndsWith("\n"))
                {
                    dummy.Text = textBox.Text + "A";
                }

                textBox.Width = dummy.Width + 50;
                textBox.Height = dummy.Height + diffh;

                //コントロール削除
                Controls.Remove(dummy);
                Controls.Remove(dummy2);
                Controls.Remove(dummy3);

                Width = textBox.Width + (waku ? bold * 2 : 0);
                Height = textBox.Height + (waku ? bold * 2 : 0);
            }

            //Enterで確定、Escでキャンセル
            private void textBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    int idx = textBox.SelectionStart;
                    string str = textBox.Text;
                    textBox.Text = str.Substring(0, idx) + Environment.NewLine + (textBox.Text.Length == idx ? "" : str.Substring(idx));
                    if (idx + Environment.NewLine.Length <= textBox.Text.Length)
                    {
                        textBox.SelectionStart = idx + Environment.NewLine.Length;
                    }
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    Label_kakikae(true, false);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    Label_kakikae(false, false);
                }
            }

            //テキスト編集内容反映
            private void textBox_Leave(object sender, EventArgs e)
            {
                Label_kakikae(true, true);
            }
            private void panel_Leave(object sender, EventArgs e)
            {
                if (textBox.Visible)
                {
                    Label_kakikae(true, true);
                }
            }

            private void Label_kakikae(bool kakikae, bool nonactive)
            {
                if (kakikae)
                {
                    label.Text = textBox.Text;
                }

                //表示日表示切替
                label.Visible = true;
                textBox.Visible = false;

                //サイズミニマム
                textBox.Multiline = false;
                textBox.Height = 1;
                textBox.Width = 1;

                //パネルサイズ変更
                Width = label.Width + (waku ? bold * 2 : 0);
                Height = label.Height + (waku ? bold * 2 : 0);

                if (!nonactive)
                {
                    input_now = false;
                }
            }
        }

        #endregion

        #region 設定
        private void 起動時に画面をポーズするToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                iniFile.SetKeyValueBool("BootMode", "pause", ((ToolStripMenuItem)sender).Checked);
            }
        }
        private void 画面をキャプチャできる状態で起動するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                iniFile.SetKeyValueBool("BootMode", "capture", ((ToolStripMenuItem)sender).Checked);
            }
        }

        private void 画面をキャプチャしたら終了するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                iniFile.SetKeyValueBool("Capture", "autoexit", ((ToolStripMenuItem)sender).Checked);
            }
        }

        private void 画像を自動保存するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                iniFile.SetKeyValueBool("Capture", "save", ((ToolStripMenuItem)sender).Checked);
            }
        }

        private void ToolStripMenuItemIniMode_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dic = new Dictionary<int, string> { { 0, "フリーハンド" }, { 1, "四角" }, { 2, "直線" }, { 3, "矢印" } };

            フリーハンドToolStripMenuItem.Checked = false;
            四角ToolStripMenuItem.Checked = false;
            直線ToolStripMenuItem.Checked = false;
            矢印ToolStripMenuItem.Checked = false;

            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = true;

                foreach (var d in dic)
                {
                    if (((ToolStripMenuItem)sender).Name.Contains(d.Value))
                    {
                        iniFile.SetKeyValueInt("ButtonSelect", "mode", d.Key);
                    }
                }
            }
        }

        private void ToolStripMenuItemIniSize_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dic = new Dictionary<int, string> { { 0, "小" }, { 1, "中" }, { 2, "大" } };

            小ToolStripMenuItem.Checked = false;
            中ToolStripMenuItem.Checked = false;
            大ToolStripMenuItem.Checked = false;

            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = true;

                foreach (var d in dic)
                {
                    if (((ToolStripMenuItem)sender).Name.Contains(d.Value))
                    {
                        iniFile.SetKeyValueInt("ButtonSelect", "size", d.Key);
                    }
                }
            }
        }

        private void ToolStripMenuItemIniColor_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dic = new Dictionary<int, string> { { 0, "赤" }, { 1, "青" }, { 2, "黄" }, { 3, "緑" } };

            赤ToolStripMenuItem.Checked = false;
            青ToolStripMenuItem.Checked = false;
            黄ToolStripMenuItem.Checked = false;
            緑ToolStripMenuItem.Checked = false;

            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = true;

                foreach (var d in dic)
                {
                    if (((ToolStripMenuItem)sender).Name.Contains(d.Value))
                    {
                        iniFile.SetKeyValueInt("ButtonSelect", "color", d.Key);
                    }
                }
            }
        }

        private void ToolStripMenuItemSaveFormat_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dic = new Dictionary<int, string> { { 0, "png" }, { 1, "jpg" }, { 2, "bmp" } };

            pNGToolStripMenuItem.Checked = false;
            jPGToolStripMenuItem.Checked = false;
            bMPToolStripMenuItem.Checked = false;

            if (sender is ToolStripMenuItem)
            {
                ((ToolStripMenuItem)sender).Checked = true;

                foreach (var d in dic)
                {
                    if (((ToolStripMenuItem)sender).Name.ToLower().Contains(d.Value))
                    {
                        iniFile.SetKeyValueString("Capture", "saveformat", d.Value);
                        saveformat = d.Value;
                    }
                }
            }
        }

        private void タイトルバーにアイコンを表示するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            タイトルバーにアイコンを表示するToolStripMenuItem.Checked = !タイトルバーにアイコンを表示するToolStripMenuItem.Checked;
            TitlebarIconView(タイトルバーにアイコンを表示するToolStripMenuItem.Checked);
            iniFile.SetKeyValueBool("Soft", "Icon", タイトルバーにアイコンを表示するToolStripMenuItem.Checked);
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            historyToolStripMenuItem.Checked = !historyToolStripMenuItem.Checked;
            iniFile.SetKeyValueBool("Soft", "History", historyToolStripMenuItem.Checked);
        }

        #endregion
    }
}
