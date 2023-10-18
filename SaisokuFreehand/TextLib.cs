using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TextLib
{
    class IniFile
    {
        //iniファイル
        string ProfileName = null;//ファイルパス
        System.Text.Encoding fileenc = null;//文字コード

        #region section/key

        //セクションとキー
        public inifile_items_calss items = new inifile_items_calss();

        public class inifile_items_calss//全項目
        {
            public List<section_class> sections { get; set; }

            public inifile_items_calss()
            {
                sections = new List<section_class>();
            }

            //セクションの数
            public int sectionsCount()
            {
                if (sections == null)
                {
                    return 0;
                }
                else
                {
                    return sections.Count;
                }
            }

            //指定のセクション名が存在するか。存在しない場合は-1。する場合はindexを返す
            public int section_exist(string sname)
            {
                int idx = -1;

                if (sectionsCount() != 0)
                {
                    foreach (var item in sections.Select((Value, Index) => new { Value, Index }))
                    {
                        if (item.Value.name == sname)
                        {
                            return item.Index;
                        }
                    }
                }

                return idx;
            }

            //全項目設定文字列を返す
            public string ToStr()
            {
                string r = "";
                if (sectionsCount() != 0)
                {
                    foreach (section_class sc in sections)
                    {
                        if (sc.keysCount() != 0)
                        {
                            r += "[" + sc.name + "]" + Environment.NewLine;

                            foreach (key_class kc in sc.keys)
                            {
                                r += kc.name + "=" + kc.value + Environment.NewLine;
                            }
                        }
                    }
                }

                return r;
            }
        }

        public class section_class//セクション
        {
            public string name { get; set; }
            public List<key_class> keys { get; set; }

            public section_class(string buf = "")
            {
                name = buf;
                keys = new List<key_class>();
            }


            //キーの数
            public int keysCount()
            {
                if (keys == null)
                {
                    return 0;
                }
                else
                {
                    return keys.Count;
                }
            }

            //指定のキー名が存在するか。存在しない場合は-1。する場合はindexを返す
            public int key_exist(string kname)
            {
                int idx = -1;

                if (keysCount() != 0)
                {
                    foreach (var item in keys.Select((Value, Index) => new { Value, Index }))
                    {
                        if (item.Value.name == kname)
                        {
                            return item.Index;
                        }
                    }
                }

                return idx;
            }

        }
        public class key_class//キー
        {
            public string name { get; set; }
            public string value { get; set; }

            public key_class(string x = "", string y = "")
            {
                name = x;
                value = y;
            }
        }

        #endregion

        //コンストラクタ
        public IniFile(string f)
        {
            ProfileName = f;
            fileenc = TextFile.encoding_utf8;
        }

        public IniFile(string f, Encoding e)
        {
            ProfileName = f;
            fileenc = e;
        }

        #region load

        public bool load()
        {
            //対象ファイルが設定されていない場合
            if (ProfileName == "")
            {
                return false;
            }

            //対象ファイルが存在しない場合は新規作成
            if (!File.Exists(ProfileName))
            {
                try
                {
                    File.Create(ProfileName).Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            //ファイルが存在する場合
            if (File.Exists(ProfileName))
            {
                //空ファイルでなければ
                if ((new FileInfo(ProfileName)).Length != 0)
                {
                    //文字コード確認
                    if (!TextFile.ChangeEncode(ProfileName, fileenc))
                    {
                        return false;
                    }

                    //ファイル読み込み
                    string strall = TextFile.Read(ProfileName, fileenc);
                    if (strall == null)
                    {
                        return false;
                    }

                    //中身確認
                    if (strall != "")
                    {
                        //改行コードで切り分けて配列に格納
                        strall = strall.Replace("\r\n", "\n");
                        string[] onelines = strall.Split('\n');

                        //読み込んだ内容を格納
                        bool section_exist = false;
                        foreach (string buf in onelines)
                        {
                            string oneline = buf;//.Trim();

                            if (oneline.Length > 0)
                            {
                                //section
                                if ((oneline.StartsWith("[") && oneline.EndsWith("]")))
                                {
                                    section_exist = false;
                                    if (oneline.Length >= 3)//何らかの中身があるはずで
                                    {
                                        //取得
                                        string name = oneline.Substring(1, oneline.Length - 2).Trim();

                                        if (name.Length != 0)//セクション名が空白でない場合
                                        {
                                            //二重セクション名は禁止
                                            bool check = true;
                                            if (items.sectionsCount() != 0)
                                            {
                                                foreach (section_class s in items.sections)
                                                {
                                                    if (name == s.name)
                                                    {
                                                        check = false;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (check)//二重セクション名でない場合
                                            {
                                                //確保
                                                section_class sc = new section_class();
                                                sc.name = oneline.Substring(1, oneline.Length - 2);

                                                items.sections.Add(sc);
                                                section_exist = true;
                                            }
                                        }
                                    }
                                }
                                //key
                                else if (oneline.IndexOf('=') >= 1)
                                {
                                    //取得
                                    string name = oneline.Substring(0, oneline.IndexOf('='));
                                    string value = oneline.Substring(oneline.IndexOf('=') + 1);

                                    //value = value.Trim();

                                    if (section_exist)
                                    {
                                        key_class current_key = new key_class();
                                        current_key.name = name;
                                        current_key.value = value;

                                        //現在のsectionにキー追加
                                        items.sections[items.sectionsCount() - 1].keys.Add(current_key);
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region get/set
        //値の取得
        public string getvalue(string sname, string kname)
        {
            int idx_s = items.section_exist(sname);

            if (idx_s >= 0)
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)
                {
                    return items.sections[idx_s].keys[idx_k].value;
                }
            }

            return null;
        }

        //値の取得。ない場合はデフォルト値を書き込み
        public string getvalue(string sname, string kname, string defvalue, bool value_empty_ok = true)
        {
            int idx_s = items.section_exist(sname);

            if (idx_s >= 0)
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)
                {
                    string s = items.sections[idx_s].keys[idx_k].value;

                    if (s != "")
                    {
                        return s;
                    }
                    else if (value_empty_ok)
                    {
                        return s;
                    }
                }
            }
            setvalue(sname, kname, defvalue);

            return defvalue;
        }

        //int値の取得。ない場合はデフォルト値を書き込み
        public int getvalue(string sname, string kname, int defvalue)
        {
            int idx_s = items.section_exist(sname);

            if (idx_s >= 0)
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)
                {
                    string buf = items.sections[idx_s].keys[idx_k].value;

                    int ri = defvalue;
                    if (int.TryParse(buf, out ri))
                    {
                        return ri;
                    }
                }
            }
            setvalue(sname, kname, defvalue.ToString());

            return defvalue;
        }

        //int値の取得。ない場合はデフォルト値を書き込み（上下限あり）
        public int getvalue(string sname, string kname, int defvalue, int vmin, int vmax)
        {
            int idx_s = items.section_exist(sname);

            if (defvalue < vmin) defvalue = vmin;
            if (defvalue > vmax) defvalue = vmax;

            if (idx_s >= 0)
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)
                {
                    string buf = items.sections[idx_s].keys[idx_k].value;

                    int ri = defvalue;
                    if (int.TryParse(buf, out ri))
                    {
                        if (vmin <= ri && ri <= vmax)
                        {
                            return ri;
                        }
                        else if (ri < vmin)
                        {
                            return vmin;
                        }
                        else if (vmax < ri)
                        {
                            return vmax;
                        }
                    }
                }
            }
            setvalue(sname, kname, defvalue.ToString());

            return defvalue;
        }

        //値の設定
        public void setvalue(string sname, string kname, string value)
        {
            int idx_s = items.section_exist(sname);
            key_class kc = new key_class(kname, value);

            if (idx_s >= 0)//sectionあり
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)//keyあり
                {
                    items.sections[idx_s].keys[idx_k].value = value;
                }
                else//keyなし
                {
                    items.sections[idx_s].keys.Add(kc);
                }
            }
            else//sectionなし
            {
                section_class sc = new section_class(sname);
                items.sections.Add(sc);
                idx_s = items.section_exist(sname);

                items.sections[idx_s].keys.Add(kc);
            }
        }

        //値の削除
        public void deletevalue(string sname, string kname)
        {
            int idx_s = items.section_exist(sname);

            if (idx_s >= 0)//section
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)//keyあり
                {
                    items.sections[idx_s].keys[idx_k].value = "";
                }
            }
        }

        //キーの削除
        public void deletekey(string sname, string kname)
        {
            int idx_s = items.section_exist(sname);

            if (idx_s >= 0)//section
            {
                int idx_k = items.sections[idx_s].key_exist(kname);

                if (idx_k >= 0)//keyあり
                {
                    try
                    {
                        items.sections[idx_s].keys.RemoveAt(idx_k);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            }
        }

        #endregion

        #region WriteIniFile
        //設定の書き込み
        public bool WriteIniFile()
        {
            return TextFile.Write(ProfileName, items.ToStr(), false, fileenc);
        }
        public bool WriteIniFile(string sname, string kname, string kvalue)//指定のセクション、キーに書き込み
        {
            //ファイルが存在する場合
            if (File.Exists(ProfileName))
            {
                //空ファイルでなければ
                if ((new FileInfo(ProfileName)).Length != 0)
                {
                    //文字コード確認
                    if (!TextFile.ChangeEncode(ProfileName, fileenc))
                    {
                        return false;
                    }

                    //ファイル読み込み
                    string strall = TextFile.Read(ProfileName, fileenc);
                    if (strall == null)
                    {
                        return false;
                    }

                    //中身確認
                    if (strall != "")
                    {
                        string kakikomi = "";

                        //改行コードで切り分けて配列に格納
                        strall = strall.Replace("\r\n", "\n");
                        string[] onelines = strall.Split('\n');

                        //読み込んだ内容を格納
                        bool current_section = false;
                        bool current_action = false;
                        foreach (string buf in onelines)
                        {
                            string oneline = buf.Trim();

                            current_action = false;

                            if (oneline.Length > 0)
                            {
                                //section
                                if ((oneline.StartsWith("[") && oneline.EndsWith("]")))
                                {
                                    current_section = false;
                                    if (oneline.Length >= 3)//何らかの中身があるはずで
                                    {
                                        //取得
                                        string name = oneline.Substring(1, oneline.Length - 2).Trim();

                                        if (name.Length != 0)//セクション名が空白でない場合
                                        {
                                            if (name == sname)
                                            {
                                                current_section = true;
                                            }
                                        }
                                    }
                                }
                                //key
                                else if (oneline.IndexOf('=') > 1 && current_section)
                                {
                                    //取得
                                    string name = oneline.Substring(0, oneline.IndexOf('='));

                                    if (name == kname)
                                    {
                                        current_action = true;
                                    }
                                }

                                if (current_action)
                                {
                                    kakikomi += kname + "=" + kvalue + Environment.NewLine;
                                }
                                else
                                {
                                    kakikomi += oneline + Environment.NewLine;
                                }
                            }
                        }

                        //書き込み
                        return TextFile.Write(ProfileName, kakikomi, false, fileenc);
                    }
                }
            }
            return false;
        }

        #endregion
    }

    #region TextFile

    class TextFile
    {
        public static Encoding encoding_sjis
        {
            get
            {
                return Encoding.GetEncoding(932);
            }
        }

        public static Encoding encoding_utf8_with_BOM
        {
            get
            {
                return (new UTF8Encoding(true));
            }
        }
        public static Encoding encoding_utf8
        {
            get
            {
                return (new UTF8Encoding(false));
            }
        }

        //書き込み
        public static bool Write(string file, string word)//上書き保存
        {
            Encoding e = GetJpEncoding(file, true);
            return Write(file, word, false, e);
        }
        public static bool Write(string file, string word, bool add, Encoding e)//ファイル名、書き込み文字列、追加書き込み(上書きfalse)、エンコード
        {
            bool check = false;
            StreamWriter sw = null;

            try
            {
                //ファイルを作成
                sw = new StreamWriter(file, add, e);
                sw.Write(word);
                check = true;
            }
            catch (Exception)
            {
                check = false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }

            return check;
        }

        //読み込み
        public static string Read(string file)
        {
            string strall = "";

            Encoding e = GetJpEncoding(file);

            if (e != null)
            {
                strall = Read(file, e);
            }

            return strall;
        }
        public static string Read(string file, Encoding e)
        {
            //対象ファイルを読込
            string strall = "";
            StreamReader Reader = null;
            try
            {
                if (File.Exists(file))
                {
                    if ((new FileInfo(file)).Length != 0)
                    {
                        Reader = new StreamReader(file, e);
                        strall = Reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (Reader != null)
                {
                    Reader.Close();
                }
            }

            return strall;
        }

        //文字コード変更
        public static bool ChangeEncode(string file, Encoding henkougo)
        {
            try
            {
                if (!File.Exists(file))//ファイルが存在しない場合
                {
                    return true;
                }
                else if (new FileInfo(file).Length == 0)//ファイルサイズが0の場合
                {
                    return true;
                }
                else
                {
                    //文字コード確認
                    Encoding enc = GetJpEncoding(file, true);

                    if (enc != henkougo)
                    {
                        //読み込み
                        string alltext = Read(file, enc);

                        if (alltext == null)
                        {
                            return true;
                        }
                        else
                        {
                            Write(file, alltext, false, henkougo);
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Encoding GetJpEncoding(string file, bool forcesjis)
        {
            Encoding enc = GetJpEncoding(file);

            if (enc == null)
            {
                if (forcesjis)
                {
                    return Encoding.GetEncoding(932);
                }
                else
                {
                    return enc;
                }
            }
            else
            {
                return enc;
            }
        }

        public static Encoding GetJpEncoding(string file, long maxSize = 50 * 1024)//ファイルパス、最大読み取りバイト数
        {
            try
            {
                if (!File.Exists(file))//ファイルが存在しない場合
                {
                    return null;
                }
                else if (new FileInfo(file).Length == 0)//ファイルサイズが0の場合
                {
                    return null;
                }
                else//ファイルが存在しファイルサイズが0でない場合
                {
                    //バイナリ読み込み
                    byte[] bytes = null;
                    bool readAll = false;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        long size = fs.Length;

                        if (size <= maxSize)
                        {
                            bytes = new byte[size];
                            fs.Read(bytes, 0, (int)size);
                            readAll = true;
                        }
                        else
                        {
                            bytes = new byte[maxSize];
                            fs.Read(bytes, 0, (int)maxSize);
                        }
                    }

                    //判定
                    return GetJpEncoding(bytes, readAll);
                }
            }
            catch
            {
                return null;
            }
        }
        
        public static Encoding GetJpEncoding(byte[] bytes, bool readAll = false)
        {
            int len = bytes.Length;

            //BOM判定
            if (len >= 2 && bytes[0] == 0xfe && bytes[1] == 0xff)//UTF-16BE
            {
                return Encoding.BigEndianUnicode;
            }
            else if (len >= 2 && bytes[0] == 0xff && bytes[1] == 0xfe)//UTF-16LE
            {
                return Encoding.Unicode;
            }
            else if (len >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)//UTF-8
            {
                return new UTF8Encoding(true, true);
            }
            else if (len >= 3 && bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76)//UTF-7
            {
                return Encoding.UTF7;
            }
            else if (len >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xfe && bytes[3] == 0xff)//UTF-32BE
            {
                return new UTF32Encoding(true, true);
            }
            else if (len >= 4 && bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0x00 && bytes[3] == 0x00)//UTF-32LE
            {
                return new UTF32Encoding(false, true);
            }

            //文字コード判定と日本語の文章らしさをまとめて確認

            //Shift_JIS判定用
            bool sjis = true;         //すべてのバイトがShift_JISで使用するバイト範囲かどうか
            bool sjis_2ndbyte = false;//次回の判定がShift_JISの2バイト目の判定かどうか
            bool sjis_kana = false;   //かな判定用
            bool sjis_kanji = false;  //常用漢字判定用
            int counter_sjis = 0;     //Shift_JISらしさ

            //UTF-8判定用
            bool utf8 = true;            //すべてのバイトがUTF-8で使用するバイト範囲かどうか
            bool utf8_multibyte = false; //次回の判定がUTF-8の2バイト目以降の判定かどうか
            bool utf8_kana_kanji = false;//かな・常用漢字判定用
            int counter_utf8 = 0;        //UTF-8らしさ
            int counter_utf8_multibyte = 0;

            //EUC-JP判定用
            bool eucjp = true;            //すべてのバイトがEUC-JPで使用するバイト範囲かどうか
            bool eucjp_multibyte = false; //次回の判定がEUC-JPの2バイト目以降の判定かどうか
            bool eucjp_kana_kanji = false;//かな・常用漢字判定用
            int counter_eucjp = 0;        //EUC-JPらしさ
            int counter_eucjp_multibyte = 0;

            for (int i = 0; i < len; i++)
            {
                byte b = bytes[i];

                //Shift_JIS判定
                if (sjis)
                {
                    if (!sjis_2ndbyte)
                    {
                        if (b == 0x0D                   //CR
                            || b == 0x0A                //LF
                            || b == 0x09                //tab
                            || (0x20 <= b && b <= 0x7E))//ASCII文字
                        {
                            counter_sjis++;
                        }
                        else if ((0x81 <= b && b <= 0x9F) || (0xE0 <= b && b <= 0xFC))//Shift_JISの2バイト文字の1バイト目の場合
                        {
                            //2バイト目の判定を行う
                            sjis_2ndbyte = true;

                            if (0x82 <= b && b <= 0x83)//Shift_JISのかな
                            {
                                sjis_kana = true;
                            }
                            else if ((0x88 <= b && b <= 0x9F) || (0xE0 <= b && b <= 0xE3) || b == 0xE6 || b == 0xE7)//Shift_JISの常用漢字
                            {
                                sjis_kanji = true;
                            }
                        }
                        else if (0xA1 <= b && b <= 0xDF)//Shift_JISの1バイト文字の場合(半角カナ)
                        {
                            ;
                        }
                        else if (0x00 <= b && b <= 0x7F)//ASCIIコード
                        {
                            ;
                        }
                        else
                        {
                            //Shift_JISでない
                            counter_sjis = 0;
                            sjis = false;
                        }
                    }
                    else
                    {
                        if ((0x40 <= b && b <= 0x7E) || (0x80 <= b && b <= 0xFC))//Shift_JISの2バイト文字の2バイト目の場合
                        {
                            if (sjis_kana && 0x40 <= b && b <= 0xF1)//Shift_JISのかな
                            {
                                counter_sjis += 2;
                            }
                            else if (sjis_kanji && 0x40 <= b && b <= 0xFC && b != 0x7F)//Shift_JISの常用漢字
                            {
                                counter_sjis += 2;
                            }

                            sjis_2ndbyte = sjis_kana = sjis_kanji = false;
                        }
                        else
                        {
                            //Shift_JISでない
                            counter_sjis = 0;
                            sjis = false;
                        }
                    }
                }

                //UTF-8判定
                if (utf8)
                {
                    if (!utf8_multibyte)
                    {
                        if (b == 0x0D                   //CR
                            || b == 0x0A                //LF
                            || b == 0x09                //tab
                            || (0x20 <= b && b <= 0x7E))//ASCII文字
                        {
                            counter_utf8++;
                        }
                        else if (0xC2 <= b && b <= 0xDF)//2バイト文字の場合
                        {
                            utf8_multibyte = true;
                            counter_utf8_multibyte = 1;
                        }
                        else if (0xE0 <= b && b <= 0xEF)//3バイト文字の場合
                        {
                            utf8_multibyte = true;
                            counter_utf8_multibyte = 2;

                            if (b == 0xE3 || (0xE4 <= b && b <= 0xE9))
                            {
                                utf8_kana_kanji = true;//かな・常用漢字
                            }
                        }
                        else if (0xF0 <= b && b <= 0xF3)//4バイト文字の場合
                        {
                            utf8_multibyte = true;
                            counter_utf8_multibyte = 3;
                        }
                        else if (0x00 <= b && b <= 0x7F)//ASCIIコード
                        {
                            ;
                        }
                        else
                        {
                            //UTF-8でない
                            counter_utf8 = 0;
                            utf8 = false;
                        }
                    }
                    else
                    {
                        if (counter_utf8_multibyte > 0)
                        {
                            counter_utf8_multibyte--;

                            if (b < 0x80 || 0xBF < b)
                            {
                                //UTF-8でない
                                counter_utf8 = 0;
                                utf8 = false;
                            }
                        }

                        if (utf8 && counter_utf8_multibyte == 0)
                        {
                            if (utf8_kana_kanji)
                            {
                                counter_utf8 += 3;
                            }
                            utf8_multibyte = utf8_kana_kanji = false;
                        }
                    }
                }

                //EUC-JP判定
                if (eucjp)
                {
                    if (!eucjp_multibyte)
                    {
                        if (b == 0x0D                   //CR
                            || b == 0x0A                //LF
                            || b == 0x09                //tab
                            || (0x20 <= b && b <= 0x7E))//ASCII文字
                        {
                            counter_eucjp++;
                        }
                        else if (b == 0x8E || (0xA1 <= b && b <= 0xA8) || b == 0xAD || (0xB0 <= b && b <= 0xFE))//2バイト文字の場合
                        {
                            eucjp_multibyte = true;
                            counter_eucjp_multibyte = 1;

                            if (b == 0xA4 || b == 0xA5 || (0xB0 <= b && b <= 0xEE))
                            {
                                eucjp_kana_kanji = true;
                            }
                        }
                        else if (b == 0x8F)//3バイト文字の場合
                        {
                            eucjp_multibyte = true;
                            counter_eucjp_multibyte = 2;
                        }
                        else if (0x00 <= b && b <= 0x7F)//ASCIIコード
                        {
                            ;
                        }
                        else
                        {
                            //EUC-JPでない
                            counter_eucjp = 0;
                            eucjp = false;
                        }
                    }
                    else
                    {
                        if (counter_eucjp_multibyte > 0)
                        {
                            counter_eucjp_multibyte--;

                            if (b < 0xA1 || 0xFE < b)
                            {
                                //EUC-JPでない
                                counter_eucjp = 0;
                                eucjp = false;
                            }
                        }

                        if (eucjp && counter_eucjp_multibyte == 0)
                        {
                            if (eucjp_kana_kanji)
                            {
                                counter_eucjp += 2;
                            }
                            eucjp_multibyte = eucjp_kana_kanji = false;
                        }
                    }
                }

                //ISO-2022-JP
                if (b == 0x1B)
                {
                    if ((i + 2 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x40)                                                                           //1B-24-40
                        || (i + 2 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x42)                                                                        //1B-24-42
                        || (i + 2 < len && bytes[i + 1] == 0x28 && bytes[i + 2] == 0x4A)                                                                        //1B-28-4A
                        || (i + 2 < len && bytes[i + 1] == 0x28 && bytes[i + 2] == 0x49)                                                                        //1B-28-49
                        || (i + 2 < len && bytes[i + 1] == 0x28 && bytes[i + 2] == 0x42)                                                                        //1B-28-42
                        || (i + 3 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x48 && bytes[i + 3] == 0x44)                                                //1B-24-48-44
                        || (i + 3 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x48 && bytes[i + 3] == 0x4F)                                                //1B-24-48-4F
                        || (i + 3 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x48 && bytes[i + 3] == 0x51)                                                //1B-24-48-51
                        || (i + 3 < len && bytes[i + 1] == 0x24 && bytes[i + 2] == 0x48 && bytes[i + 3] == 0x50)                                                //1B-24-48-50
                        || (i + 5 < len && bytes[i + 1] == 0x26 && bytes[i + 2] == 0x40 && bytes[i + 3] == 0x1B && bytes[i + 4] == 0x24 && bytes[i + 5] == 0x42)//1B-26-40-1B-24-42
                    )
                    {
                        return Encoding.GetEncoding(50220);//iso-2022-jp
                    }
                }
            }

            // すべて読み取った場合で、最後が多バイト文字の途中で終わっている場合は判定NG
            if (readAll)
            {
                if (sjis && sjis_2ndbyte)
                {
                    sjis = false;
                }

                if (utf8 && utf8_multibyte)
                {
                    utf8 = false;
                }

                if (eucjp && eucjp_multibyte)
                {
                    eucjp = false;
                }
            }

            if (sjis || utf8 || eucjp)
            {
                //日本語らしさの最大値確認
                int max_value = counter_eucjp;
                if (counter_sjis > max_value)
                {
                    max_value = counter_sjis;
                }
                if (counter_utf8 > max_value)
                {
                    max_value = counter_utf8;
                }

                //文字コード判定
                if (max_value == counter_utf8)
                {
                    return new UTF8Encoding(false, true);//utf8
                }
                else if (max_value == counter_sjis)
                {
                    return Encoding.GetEncoding(932);//ShiftJIS
                }
                else
                {
                    return Encoding.GetEncoding(51932);//EUC-JP
                }
            }
            else
            {
                return null;
            }
        }
    }

    #endregion
}
