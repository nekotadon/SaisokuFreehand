using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TextLib;

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
        string crlf = Environment.NewLine;

        private MouseButtons mb = MouseButtons.Left;

        //ファイルパス
        string apppath;
        string appfolder;
        string appname;
        string imagefolder;
        IniFile iniFile = new IniFile();
        string saveformat;

        //ソフトウェアのメニュー
        int barsize = 28;
        ContextMenuStrip appmenu;

        //履歴
        bool history = false;

        #endregion

        #region 初期化

        public Form1()
        {
            InitializeComponent();

            //設定ファイル
            apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appfolder = System.IO.Path.GetDirectoryName(apppath);
            imagefolder = appfolder + @"\image";
            appname = System.IO.Path.GetFileNameWithoutExtension(apppath);

            //設定ファイル読み込みと保存
            this.起動時に画面をポーズするToolStripMenuItem.Checked = iniFile.GetKeyValueBool("BootMode", "pause", true, true);
            bool bootcapture = iniFile.GetKeyValueBool("BootMode", "capture", false, true);
            int mode_ini = iniFile.GetKeyValueInt("ButtonSelect", "mode", 0, 0, 3, true);
            int penbold_ini = iniFile.GetKeyValueInt("ButtonSelect", "size", 1, 0, 2, true);
            int color_ini = iniFile.GetKeyValueInt("ButtonSelect", "color", 0, 0, 3, true);
            this.画面をキャプチャしたら終了するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Capture", "autoexit", false, true);
            this.画像を自動保存するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Capture", "save", false, true);
            this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked = iniFile.GetKeyValueBool("Soft", "Icon", false, true);
            history = iniFile.GetKeyValueBool("Soft", "History", false, true);
            this.historyToolStripMenuItem.Checked = history;
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
                    this.pNGToolStripMenuItem.Checked = true;
                    break;
                case "jpg":
                    this.jPGToolStripMenuItem.Checked = true;
                    break;
                case "bmp":
                    this.bMPToolStripMenuItem.Checked = true;
                    break;
            }

            iniFile.SetKeyValueString("Capture", "saveformat", saveformat);
            int pos_x = iniFile.GetKeyValueInt("Soft", "Top", 63, true);
            int pos_y = iniFile.GetKeyValueInt("Soft", "Left", 78, true);

            //ウィンドウ
            this.Text = "最速フリーハンド";
            this.Icon = Properties.Resources.paint;
            this.WindowState = FormWindowState.Maximized;
            this.KeyDown += Form1_KeyDown;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;

            this.picbox.MouseDown += Picbox_MouseDown;
            this.picbox.MouseUp += Picbox_MouseUp;
            this.picbox.MouseMove += Picbox_MouseMove;
            this.picbox.DoubleClick += Picbox_DoubleClick;

            //アプリケーションのコンテキストメニュー
            appmenu = new ContextMenuStrip();
            ToolStripMenuItem menu1 = new ToolStripMenuItem();
            menu1.Text = "閉じる(&C)";
            menu1.Font = new Font(menu1.Font, FontStyle.Bold);
            menu1.ShortcutKeys = ((Keys)((Keys.Alt | Keys.F4)));
            menu1.Click += AppClose_Click;
            appmenu.Items.Add(menu1);

            //初期化
            if (this.起動時に画面をポーズするToolStripMenuItem.Checked)
            {
                this.DoubleBuffered = true;
                this.picbox.BackColor = Color.FromArgb(128, 128, 128);
            }
            else
            {
                this.BackColor = this.picbox.BackColor = this.TransparencyKey = Color.DarkGoldenrod;
            }

            if (bootcapture)
            {
                this.画面をキャプチャできる状態で起動するToolStripMenuItem.Checked = true;
                toolStripButton_camera.Checked = true;
                this.Cursor = Cursors.Cross;
            }
            set_mode(mode_ini + 1);
            switch (mode_ini)
            {
                case 0:
                    this.フリーハンドToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    this.四角ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    this.直線ToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    this.矢印ToolStripMenuItem.Checked = true;
                    break;
            }
            switch (penbold_ini)
            {
                case 0:
                    set_pen_bold(penbold1);
                    this.小ToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    set_pen_bold(penbold2);
                    this.中ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    set_pen_bold(penbold3);
                    this.大ToolStripMenuItem.Checked = true;
                    break;
            }

            switch (color_ini)
            {
                case 0:
                    set_pen_color(Color.Red);
                    this.赤ToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    set_pen_color(Color.Blue);
                    this.青ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    set_pen_color(Color.Yellow);
                    this.黄ToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    set_pen_color(Color.Green);
                    this.緑ToolStripMenuItem.Checked = true;
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
            if (!this.起動時に画面をポーズするToolStripMenuItem.Checked)
            {
                toolStripButton_text.Visible = false;
            }

            //設定メニューの自動スケール無効
            foreach (var c in this.toolStripDropDownButton_setting.DropDownItems)
            {
                if (c is ToolStripMenuItem)
                {
                    toolstrip_sizenone((ToolStripMenuItem)c);
                }
            }

            //ボタン
            this.button_close.Font = new Font(SystemFonts.MenuFont.FontFamily, 14);
            this.button_close.FlatAppearance.BorderSize = 0;

            //タイトル
            barsize = 28;
            this.label_title.Text = "最速フリーハンド";
            this.label_title.Font = SystemFonts.MenuFont;
            this.label_title.Location = new Point(29, (barsize - this.label_title.Height) / 2);

            this.pictureBox_icon.Size = new Size(16, 16);
            this.pictureBox_icon.Location = new Point(8, 6);
            this.pictureBox_icon.BackgroundImageLayout = ImageLayout.Zoom;
            this.pictureBox_icon.BackgroundImage = Properties.Resources.paint.ToBitmap();
            this.pictureBox_icon.Click += new EventHandler(this.pictureBox_icon_Click);
            this.pictureBox_icon.DoubleClick += new EventHandler(this.AppClose_Click);
            TitlebarIconView(this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked);

            //マウスで移動
            this.panel1.MouseDown += new MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new MouseEventHandler(this.panel1_MouseUp);
            this.label_title.MouseDown += new MouseEventHandler(this.panel1_MouseDown);
            this.label_title.MouseMove += new MouseEventHandler(this.panel1_MouseMove);
            this.label_title.MouseUp += new MouseEventHandler(this.panel1_MouseUp);

            //ツールバーの位置調整
            this.toolStrip1.Location = new Point(0, 0);

            this.button_close.Height = this.toolStrip1.Height;
            this.button_close.Width = this.toolStrip1.Height * 2;
            this.button_close.Location = new Point(this.toolStrip1.Width - 1, 0);
            this.button_close.BringToFront();

            this.panel_inner.Height = this.toolStrip1.Height;
            this.panel_inner.Width = this.toolStrip1.Width + this.button_close.Width;
            this.panel_inner.Location = new Point(0, barsize);
            this.panel1.Height = this.panel_inner.Height + barsize;
            this.panel1.Width = this.panel_inner.Width;
            this.panel1.BackColor = Color.White;

            int display_h = Screen.GetBounds(this).Height;
            int display_w = Screen.GetBounds(this).Width;

            if (this.起動時に画面をポーズするToolStripMenuItem.Checked)
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

            this.ActiveControl = this.picbox;

            //位置決めと設定保存
            if (pos_x >= display_w - this.panel1.Width)
            {
                pos_x = display_w - this.panel1.Width;
            }
            if (pos_y >= display_h - this.panel1.Height)
            {
                pos_y = display_h - this.panel1.Height;
            }
            if (pos_x < 0)
            {
                pos_x = 0;
            }
            if (pos_y < 0)
            {
                pos_y = 0;
            }
            this.panel1.Location = new Point(pos_x, pos_y);
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
            this.pictureBox_icon.Visible = view;
            this.label_title.Left = view ? 29 : 5;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            iniFile.SetKeyValueInt("Soft", "Top", this.panel1.Location.X);
            iniFile.SetKeyValueInt("Soft", "Left", this.panel1.Location.Y);
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
            Point p = this.panel1.PointToScreen(new Point(-1, barsize));
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
            if (this.ActiveControl != this.picbox)
            {
                this.ActiveControl = this.picbox;
            }

            if (!this.toolStripButton_camera.Checked && (toolStripButton_text.Checked || toolStripButton_textwaku.Checked))
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
            if (!this.toolStripButton_camera.Checked && (toolStripButton_text.Checked || toolStripButton_textwaku.Checked))
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

                        p1 = this.PointToScreen(p1);
                        p4 = this.PointToScreen(p4);

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
                                if (this.画像を自動保存するToolStripMenuItem.Checked)
                                {
                                    if (!System.IO.Directory.Exists(imagefolder))
                                    {
                                        try
                                        {
                                            System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(imagefolder);
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }

                                    if (System.IO.Directory.Exists(imagefolder))
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
                                if (this.画面をキャプチャしたら終了するToolStripMenuItem.Checked)
                                {
                                    Application.Exit();
                                }

                                this.toolStripButton_camera.Image = Properties.Resources.camera_on.ToBitmap();
                                camera_count = 0;
                                this.timer1.Enabled = true;
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
                this.toolStripButton_camera.Image = Properties.Resources.camera.ToBitmap();
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
                double k = ((double)(p2.Y) - (double)(p1.Y)) / ((double)(p2.X) - (double)(p1.X));
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
                double k = Math.Abs((double)(p2.Y) - (double)(p1.Y)) / Math.Abs((double)(p2.X) - (double)(p1.X));
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

            this.panel1.Location = new Point(this.panel1.Left + offsetX, this.panel1.Top + offsetY);

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
            this.Cursor = Cursors.Default;
        }

        private void panel1_MouseHover(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = this.toolStripButton_camera.Checked ? Cursors.Cross : Cursors.Default;
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
            this.Cursor = Cursors.Default;
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
                foreach (Control c in this.picbox.Controls)
                {
                    if (c is label_Panel)
                    {
                        this.picbox.Controls.Remove(c);
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
            foreach (var c in this.picbox.Controls)
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

            label_Panel lp = new label_Panel(pencolor, penbold, this.toolStripButton_textwaku.Checked, this.picbox, this.起動時に画面をポーズするToolStripMenuItem.Checked);
            this.picbox.Controls.Add(lp);
            lp.BringToFront();
            this.panel1.BringToFront();
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
                this.color = _color;
                this.bold = _bold;
                this.waku = _waku;
                this.pic = _pic;

                //色
                this.backcolor = this.color == Color.Yellow ? Color.FromArgb(100, 100, 100) : Color.White;

                //ラベルサイズ
                this.size = 14;
                if (bold == 1) { this.size = 10; }
                if (bold == 3) { this.size = 14; }
                if (bold == 5) { this.size = 22; }

                //ラベルのコンテキストメニューの作成
                ContextMenuStrip cms = new ContextMenuStrip();

                ToolStripMenuItem menu1 = new ToolStripMenuItem();
                menu1.Text = "削除";
                menu1.Click += contextMenuStrip_DeleteClick;
                cms.Items.Add(menu1);

                ToolStripSeparator sep1 = new ToolStripSeparator();
                cms.Items.Add(sep1);

                if (wakumenu)
                {
                    ToolStripMenuItem menu2 = new ToolStripMenuItem();
                    menu2.Text = "枠で囲う";
                    menu2.Click += contextMenuStrip_WakuClick;
                    cms.Items.Add(menu2);

                    ToolStripSeparator sep2 = new ToolStripSeparator();
                    cms.Items.Add(sep2);
                }

                int[] fontsize_arr = { 6, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

                foreach (int fsize in fontsize_arr)
                {
                    ToolStripMenuItem fmenu = new ToolStripMenuItem();
                    fmenu.Text = fsize.ToString();
                    fmenu.Name = "fsize" + fsize.ToString();
                    fmenu.Click += contextMenuStrip_FontSizeClick;
                    cms.Items.Add(fmenu);
                }

                cms.Opening += contextMenuStrip_MenuOpening;

                //ラベル作成
                this.label = new Label();
                this.label.Text = initext;
                this.label.Font = new Font(SystemFonts.MenuFont.Name, this.size);
                this.label.AutoSize = true;
                this.label.Visible = true;
                this.label.ForeColor = this.color;
                this.label.BackColor = this.waku ? this.backcolor : Color.Transparent;
                this.label.Location = new Point(this.waku ? bold : 0, this.waku ? bold : 0);

                this.label.ContextMenuStrip = cms;
                this.label.MouseDown += new MouseEventHandler(this.control_MouseDown);
                this.label.MouseMove += new MouseEventHandler(this.control_MouseMove);
                this.label.MouseDoubleClick += new MouseEventHandler(this.control_MouseDoubleClick);

                //テキストボックス作成
                this.textBox = new TextBox();
                this.textBox.BorderStyle = BorderStyle.None;
                this.textBox.ForeColor = this.label.ForeColor;
                this.textBox.BackColor = this.backcolor;
                this.textBox.Font = this.label.Font;
                this.textBox.Location = this.label.Location;
                this.textBox.Visible = false;
                this.textBox.KeyDown += new KeyEventHandler(this.textBox_KeyDown);
                this.textBox.Leave += new EventHandler(this.textBox_Leave);
                this.textBox.TextChanged += new EventHandler(this.textBox_TextChanged);

                //コントロールの追加
                this.Controls.Add(this.label);
                this.Controls.Add(this.textBox);

                //パネル
                this.MouseDown += new MouseEventHandler(this.control_MouseDown);
                this.MouseMove += new MouseEventHandler(this.control_MouseMove);
                this.MouseDoubleClick += new MouseEventHandler(this.control_MouseDoubleClick);
                this.Leave += new EventHandler(this.panel_Leave);
                this.BackColor = this.waku ? this.color : Color.Transparent;
                this.Width = this.label.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.label.Height + (this.waku ? this.bold * 2 : 0);
                this.Location = pic.PointToClient(Cursor.Position);
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
                this.label.BackColor = this.waku ? this.backcolor : Color.Transparent;
                this.BackColor = this.waku ? this.color : Color.Transparent;

                //サイズ変更
                this.Width = this.label.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.label.Height + (this.waku ? this.bold * 2 : 0);

                //位置変更
                this.label.Location = new Point(this.waku ? bold : 0, this.waku ? bold : 0);
                if (waku)
                {
                    this.Location = new Point(this.Left - this.bold, this.Top - this.bold);
                }
                else
                {
                    this.Location = new Point(this.Left + this.bold, this.Top + this.bold);
                }
            }

            //メニューにチェックを入れる
            private void contextMenuStrip_MenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
            {
                ContextMenuStrip menu = (ContextMenuStrip)sender;

                //現在のラベルのフォントサイズを取得
                float size = this.label.Font.Size;

                foreach (var item in menu.Items)
                {
                    if (item.GetType().Equals(typeof(ToolStripMenuItem)))
                    {
                        //フォントサイズ変更メニューであれば
                        if (((ToolStripMenuItem)item).Name.Contains("fsize"))
                        {
                            //現在のラベルのフォントサイズであればチェックを入れる
                            if (((ToolStripMenuItem)item).Text == this.size.ToString())
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
                            ((ToolStripMenuItem)item).Checked = this.waku;
                        }
                    }
                }
            }

            //ラベルサイズ変更
            private void contextMenuStrip_FontSizeClick(object sender, EventArgs e)
            {
                //メニュー名
                string menutext = ((ToolStripMenuItem)(sender)).Text;

                //メニュー名を数字に
                this.size = int.Parse(menutext);

                //フォントサイズ変更
                this.label.Font = new Font(SystemFonts.MenuFont.Name, this.size);

                this.Width = this.label.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.label.Height + (this.waku ? this.bold * 2 : 0);
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
                    x_down = this.Left;
                    y_down = this.Top;
                }
            }
            private void control_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Left = x_down + (Cursor.Position.X - x_mouse);
                    this.Top = y_down + (Cursor.Position.Y - y_mouse);
                }
            }

            //マウスダブルクリックで編集
            private void control_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                this.textBox.Multiline = true;
                this.textBox.Text = this.label.Text;
                this.textBox.Font = new Font(this.label.Font, FontStyle.Underline);
                this.textBox.Height = this.label.Height;
                this.textBox.Width = this.label.Width + 50;
                this.textBox.Location = this.label.Location;
                this.textBox.ImeMode = ImeMode.NoControl;
                this.textBox.Visible = true;
                this.label.Visible = false;

                this.Width = this.textBox.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.textBox.Height + (this.waku ? this.bold * 2 : 0);

                this.textBox.Focus();
                this.textBox.Select(0, 0);
                this.textBox.Select(this.textBox.Text.Length, 0);

                if (this.textBox.Text == this.initext)
                {
                    this.textBox.SelectAll();
                }

                this.input_now = true;
            }

            //テキスト編集中にリアルタイムでサイズ変更
            private void textBox_TextChanged(object sender, EventArgs e)
            {
                //サイズ判定用ラベル
                Label dummy = new Label();
                dummy.Visible = false;
                dummy.Font = this.textBox.Font;
                dummy.Text = this.textBox.Text;
                dummy.AutoSize = true;
                this.Controls.Add(dummy);

                //高さ差異確認用ラベル
                Label dummy2 = new Label();
                dummy2.Visible = false;
                dummy2.Font = this.textBox.Font;
                dummy2.AutoSize = true;
                dummy2.Text = "A";
                this.Controls.Add(dummy2);

                //高さ差異確認用テキストボックス
                TextBox dummy3 = new TextBox();
                dummy3.Visible = false;
                dummy3.Font = this.textBox.Font;
                dummy3.BorderStyle = this.textBox.BorderStyle;
                dummy3.Text = "A";
                this.Controls.Add(dummy3);

                int diffh = dummy3.Height - dummy2.Height;

                //改行補正
                if (this.textBox.Text.EndsWith("\n"))
                {
                    dummy.Text = this.textBox.Text + "A";
                }

                this.textBox.Width = dummy.Width + 50;
                this.textBox.Height = dummy.Height + diffh;

                //コントロール削除
                this.Controls.Remove(dummy);
                this.Controls.Remove(dummy2);
                this.Controls.Remove(dummy3);

                this.Width = this.textBox.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.textBox.Height + (this.waku ? this.bold * 2 : 0);
            }

            //Enterで確定、Escでキャンセル
            private void textBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    int idx = this.textBox.SelectionStart;
                    string str = this.textBox.Text;
                    this.textBox.Text = str.Substring(0, idx) + System.Environment.NewLine + (this.textBox.Text.Length == idx ? "" : str.Substring(idx));
                    if (idx + System.Environment.NewLine.Length <= this.textBox.Text.Length)
                    {
                        this.textBox.SelectionStart = idx + System.Environment.NewLine.Length;
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
                if (this.textBox.Visible)
                {
                    Label_kakikae(true, true);
                }
            }

            private void Label_kakikae(bool kakikae, bool nonactive)
            {
                if (kakikae)
                {
                    this.label.Text = this.textBox.Text;
                }

                //表示日表示切替
                this.label.Visible = true;
                this.textBox.Visible = false;

                //サイズミニマム
                this.textBox.Multiline = false;
                this.textBox.Height = 1;
                this.textBox.Width = 1;

                //パネルサイズ変更
                this.Width = this.label.Width + (this.waku ? this.bold * 2 : 0);
                this.Height = this.label.Height + (this.waku ? this.bold * 2 : 0);

                if (!nonactive)
                {
                    this.input_now = false;
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

            this.フリーハンドToolStripMenuItem.Checked = false;
            this.四角ToolStripMenuItem.Checked = false;
            this.直線ToolStripMenuItem.Checked = false;
            this.矢印ToolStripMenuItem.Checked = false;

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

            this.小ToolStripMenuItem.Checked = false;
            this.中ToolStripMenuItem.Checked = false;
            this.大ToolStripMenuItem.Checked = false;

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

            this.赤ToolStripMenuItem.Checked = false;
            this.青ToolStripMenuItem.Checked = false;
            this.黄ToolStripMenuItem.Checked = false;
            this.緑ToolStripMenuItem.Checked = false;

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

            this.pNGToolStripMenuItem.Checked = false;
            this.jPGToolStripMenuItem.Checked = false;
            this.bMPToolStripMenuItem.Checked = false;

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
            this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked = !this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked;
            TitlebarIconView(this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked);
            iniFile.SetKeyValueBool("Soft", "Icon", this.タイトルバーにアイコンを表示するToolStripMenuItem.Checked);
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.historyToolStripMenuItem.Checked = !this.historyToolStripMenuItem.Checked;
            iniFile.SetKeyValueBool("Soft", "History", this.historyToolStripMenuItem.Checked);
        }

        #endregion

    }
}
