using System;
using System.Windows.Forms;

namespace SaisokuFreehand
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                //image保存フォルダ
                string appfolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string imagefolder = appfolder + @"\image";

                //image保存フォルダがなければ作成
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

                //image保存フォルダを開く
                if (System.IO.Directory.Exists(imagefolder))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(imagefolder);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
