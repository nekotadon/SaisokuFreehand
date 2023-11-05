//2023.11.05-01
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TextLib
{
    public static class AppInfo
    {
        public static string Filepath => System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string Directory => Path.GetDirectoryName(Filepath);
        public static string FileName => Path.GetFileName(Filepath);
        public static string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(Filepath);
        public static string Extension => Path.GetExtension(Filepath);
        public static string BaseIniFile => Directory + @"\" + FileNameWithoutExtension + ".ini";
    }

    public class IniFile
    {
        private string filepath { get; set; }
        private Encoding encoding = null;

        public IniFile()
        {
            filepath = AppInfo.BaseIniFile;
            FileExistCheck();
        }
        public IniFile(string _filepath)
        {
            filepath = _filepath;
            FileExistCheck();
        }
        public IniFile(string _filepath, Encoding _encoding)
        {
            filepath = _filepath;
            encoding = _encoding;
            FileExistCheck();
        }
        public IniFile(Encoding _encoding)
        {
            filepath = AppInfo.BaseIniFile;
            encoding = _encoding;
            FileExistCheck();
        }

        private bool FileExistCheck()
        {
            //対象ファイルが存在しない場合は新規作成
            if (!File.Exists(filepath))
            {
                try
                {
                    File.Create(filepath).Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        //文字列取得
        //該当キーがない場合はnullを返す。
        public string GetKeyValue(string section, string key)
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass IniFileInnerClass = new IniFileInnerClass(filepath, enc);
            IniFileInnerClass.load();

            //文字列取得
            return IniFileInnerClass.getValue(section, key);
        }

        //文字列取得
        //該当キーがない場合は空文字列を返す。
        public string GetKeyValueString(string section, string key, bool save = false)
        {
            string value = GetKeyValue(section, key);

            //存在しない場合はデフォルト値を書き込み
            if (value == null && save)
            {
                SetKeyValueString(section, key, "");
            }

            return value ?? "";
        }

        //文字列取得
        //該当キーがないか空の場合は第3引数の値を返す
        public string GetKeyValueStringWithoutEmpty(string section, string key, string defaultValue, bool save = false)
        {
            string value = GetKeyValue(section, key);

            //存在しない場合はデフォルト値を書き込み
            if (value != null && value != "")
            {
                return value;
            }
            else
            {
                if (save)
                {
                    SetKeyValueString(section, key, defaultValue);
                }
                return defaultValue;
            }
        }

        //真偽取得
        //キーが存在し値が1ならtrue
        //キーが存在し値が0ならfalse
        //上記以外は第3引数
        public bool GetKeyValueBool(string section, string key, bool defaultValue, bool save = false)
        {
            string value = GetKeyValue(section, key);

            if (value != null)
            {
                if (value == "1")
                {
                    return true;
                }
                else if (value == "0")
                {
                    return false;
                }
            }

            if (save)
            {
                SetKeyValueBool(section, key, defaultValue);
            }

            return defaultValue;
        }

        //数値取得
        //該当キーがないか数値変換できない場合は第3引数を返す。
        public int GetKeyValueInt(string section, string key, int defaultValue, bool save = false)
        {
            string value = GetKeyValue(section, key);

            int ret;
            if (value != null && int.TryParse(value, out ret))
            {
                return ret;
            }

            if (save)
            {
                SetKeyValueInt(section, key, defaultValue);
            }

            return defaultValue;
        }

        //数値取得
        //該当キーがあり、数値変換できる場合
        //  範囲が適切ならその値を返す
        //  範囲が不適切なら値を修正しその値を返す。
        //該当キーがない、または数値変換できない場合
        //  第3引数の値を返す。
        //  第3引数の範囲が不適切なら適切にした後返す。
        public int GetKeyValueInt(string section, string key, int defaultValue, int vmin, int vmax, bool save = false)
        {
            string value = GetKeyValue(section, key);

            int ret;
            if (value != null && int.TryParse(value, out ret))
            {
                if (vmin <= ret && ret <= vmax)
                {
                    return ret;
                }
                else
                {
                    if (ret < vmin)
                    {
                        ret = vmin;
                    }
                    if (vmax < ret)
                    {
                        ret = vmax;
                    }
                    if (save)
                    {
                        SetKeyValueInt(section, key, ret);
                    }

                    return ret;
                }
            }
            else
            {
                ret = defaultValue;

                if (ret < vmin) ret = vmin;
                if (ret > vmax) ret = vmax;

                if (save)
                {
                    SetKeyValueInt(section, key, ret);
                }

                return ret;
            }
        }
        public (string, string, string)[] GetKeyValueAsArray(string section = "")
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass IniFileInnerClass = new IniFileInnerClass(filepath, enc);
            IniFileInnerClass.load();

            //値を取得
            return IniFileInnerClass.getValues(section).ToArray();
        }

        //設定
        public void SetKeyValueString(string section, string key, string value)
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass iniFile = new IniFileInnerClass(filepath, enc);
            iniFile.load();

            //値の設定
            iniFile.setValue(section, key, value);

            //ファイル保存
            iniFile.WriteIniFile();
        }
        public void SetKeyValueInt(string section, string key, int value)
        {
            SetKeyValueString(section, key, value.ToString());
        }
        public void SetKeyValueBool(string section, string key, bool value)
        {
            SetKeyValueInt(section, key, value ? 1 : 0);
        }
        public void SetKeyValueFromArray((string, string, object)[] datas)
        {
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;

            IniFileInnerClass iniFile = new IniFileInnerClass(filepath, enc);
            iniFile.load();

            foreach ((string section, string key, object value) in datas)
            {
                if (value.GetType() == typeof(int))//int
                {
                    iniFile.setValue(section, key, ((int)value).ToString());
                }
                else if (value.GetType() == typeof(string))//string
                {
                    iniFile.setValue(section, key, (string)value);
                }
                else if (value.GetType() == typeof(bool))//bool
                {
                    iniFile.setValue(section, key, (bool)value ? "1" : "0");
                }
            }

            iniFile.WriteIniFile();
        }

        //キーの値を削除
        public void DeleteKeyValue(string section, string key)
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass iniFile = new IniFileInnerClass(filepath, enc);
            iniFile.load();

            //キーの値削除
            iniFile.deleteValue(section, key);

            //ファイル保存
            iniFile.WriteIniFile();
        }
        //キーを削除
        public void DeleteKey(string section, string key)
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass iniFile = new IniFileInnerClass(filepath, enc);
            iniFile.load();

            //キー削除
            iniFile.deleteKey(section, key);

            //ファイル保存
            iniFile.WriteIniFile();
        }
        //セクションを削除
        public void DeleteSection(string section)
        {
            //ファイル読み込み
            Encoding enc = (encoding ?? EncodeLib.GetJpEncoding(filepath)) ?? EncodeLib.UTF8;
            IniFileInnerClass iniFile = new IniFileInnerClass(filepath, enc);
            iniFile.load();

            //セクション削除
            iniFile.deleteSection(section);

            //ファイル保存
            iniFile.WriteIniFile();
        }
    }

    public class IniFileInnerClass
    {
        //iniファイル
        private string IniFilePath = null;//ファイルパス
        private Encoding encoding = null;//文字コード

        #region section/key

        //セクションとキー
        private Items items = new Items();

        private class Items//全項目
        {
            public List<Section> sections { get; set; }

            public Items()
            {
                sections = new List<Section>();
            }

            //指定のセクション名が存在するか。存在しない場合は-1。する場合はindexを返す
            public int getSectionIndex(string sectionName)
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Name == sectionName)
                    {
                        return i;
                    }
                }

                return -1;
            }

            //全項目設定文字列を返す
            public string ToStr()
            {
                StringBuilder sb = new StringBuilder();

                foreach (Section section in sections)
                {
                    if (section.keys.Count != 0)
                    {
                        sb.Append("[");
                        sb.Append(section.Name);
                        sb.AppendLine("]");

                        foreach (Key key in section.keys)
                        {
                            sb.Append(key.Name);
                            sb.Append("=");
                            sb.AppendLine(key.Value);
                        }
                    }
                }

                return sb.ToString();
            }
        }

        private class Section//セクション
        {
            public string Name { get; set; }
            public List<Key> keys { get; set; }

            public Section(string _Name = "")
            {
                Name = _Name;
                keys = new List<Key>();
            }

            //指定のキー名が存在するか。存在しない場合は-1。する場合はindexを返す
            public int getKeyIndex(string keyName)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i].Name == keyName)
                    {
                        return i;
                    }
                }

                return -1;
            }

        }
        private class Key//キー
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public Key(string _Name = "", string _Value = "")
            {
                Name = _Name;
                Value = _Value;
            }
        }

        #endregion

        //コンストラクタ
        public IniFileInnerClass(string f)
        {
            IniFilePath = f;
            encoding = EncodeLib.UTF8;
        }

        public IniFileInnerClass(string f, Encoding e)
        {
            IniFilePath = f;
            encoding = e;
        }

        #region load

        public bool load()
        {
            //対象ファイルが設定されていない場合
            if (IniFilePath == "")
            {
                return false;
            }

            //対象ファイルが存在しない場合は新規作成
            if (!File.Exists(IniFilePath))
            {
                try
                {
                    File.Create(IniFilePath).Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            //ファイルが存在する場合
            if (File.Exists(IniFilePath))
            {
                //空ファイルでなければ
                if (new FileInfo(IniFilePath).Length != 0)
                {
                    //ファイル読み込み
                    string allText = TextFile.Read(IniFilePath, encoding);
                    if (allText == null)
                    {
                        return false;
                    }

                    //中身確認
                    if (allText != "")
                    {
                        //改行コードで切り分けて配列に格納
                        string[] lines = allText.Replace("\r\n", "\n").Split('\n');

                        //読み込んだ内容を格納
                        bool isSectionExist = false;
                        foreach (string line in lines)
                        {
                            if (line.Length > 0)
                            {
                                //section
                                if (line.StartsWith("[") && line.EndsWith("]"))
                                {
                                    isSectionExist = false;
                                    if (line.Length >= 3)//何らかの中身があるはずで
                                    {
                                        //取得
                                        string sectionName = line.Substring(1, line.Length - 2).Trim();

                                        if (sectionName.Length != 0)//セクション名が空白でない場合
                                        {
                                            //二重セクション名は禁止
                                            bool isNotDouble = true;
                                            foreach (Section section in items.sections)
                                            {
                                                if (sectionName == section.Name)
                                                {
                                                    isNotDouble = false;
                                                    break;
                                                }
                                            }

                                            if (isNotDouble)//二重セクション名でない場合
                                            {
                                                //確保
                                                Section section = new Section(line.Substring(1, line.Length - 2));
                                                items.sections.Add(section);
                                                isSectionExist = true;
                                            }
                                        }
                                    }
                                }
                                //key
                                else if (line.IndexOf('=') >= 1)
                                {
                                    //取得
                                    string name = line.Substring(0, line.IndexOf('='));
                                    string value = line.Substring(line.IndexOf('=') + 1);

                                    if (isSectionExist)
                                    {
                                        Key key = new Key(name, value);

                                        //現在のsectionにキー追加
                                        items.sections[items.sections.Count - 1].keys.Add(key);
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
        public string getValue(string sectionName, string keyName)
        {
            int sectionIndex = items.getSectionIndex(sectionName);

            if (sectionIndex >= 0)
            {
                int keyIndex = items.sections[sectionIndex].getKeyIndex(keyName);

                if (keyIndex >= 0)
                {
                    return items.sections[sectionIndex].keys[keyIndex].Value;
                }
            }

            return null;
        }

        //値を配列で取得
        public List<(string, string, string)> getValues(string sectionName = "")
        {
            List<(string, string, string)> datas = new List<(string, string, string)>();

            if (sectionName == "")
            {
                foreach (Section section in items.sections)
                {
                    foreach (Key key in section.keys)
                    {
                        datas.Add((section.Name, key.Name, key.Value));
                    }
                }
            }
            else
            {
                int sectionIndex = items.getSectionIndex(sectionName);
                if (sectionIndex >= 0)
                {
                    foreach (Key key in items.sections[sectionIndex].keys)
                    {
                        datas.Add((sectionName, key.Name, key.Value));
                    }
                }
            }

            return datas;
        }

        //値の設定
        public void setValue(string sectionName, string keyName, string value)
        {
            int sectionIndex = items.getSectionIndex(sectionName);
            Key key = new Key(keyName, value);

            if (sectionIndex >= 0)//sectionあり
            {
                int keyIndex = items.sections[sectionIndex].getKeyIndex(keyName);

                if (keyIndex >= 0)//keyあり
                {
                    items.sections[sectionIndex].keys[keyIndex].Value = value;
                }
                else//keyなし
                {
                    items.sections[sectionIndex].keys.Add(key);
                }
            }
            else//sectionなし
            {
                Section section = new Section(sectionName);
                items.sections.Add(section);

                sectionIndex = items.getSectionIndex(sectionName);
                items.sections[sectionIndex].keys.Add(key);
            }
        }

        //値の削除
        public void deleteValue(string sname, string kname)
        {
            int sectionIndex = items.getSectionIndex(sname);

            if (sectionIndex >= 0)//section
            {
                int keyIndex = items.sections[sectionIndex].getKeyIndex(kname);

                if (keyIndex >= 0)//keyあり
                {
                    items.sections[sectionIndex].keys[keyIndex].Value = "";
                }
            }
        }

        //キーの削除
        public void deleteKey(string sname, string kname)
        {
            int sectionIndex = items.getSectionIndex(sname);

            if (sectionIndex >= 0)//section
            {
                int keyIndex = items.sections[sectionIndex].getKeyIndex(kname);

                if (keyIndex >= 0)//keyあり
                {
                    try
                    {
                        items.sections[sectionIndex].keys.RemoveAt(keyIndex);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            }
        }

        //セクションの削除
        public void deleteSection(string sname)
        {
            int sectionIndex = items.getSectionIndex(sname);

            if (sectionIndex >= 0)//section
            {
                items.sections.RemoveAt(sectionIndex);
            }
        }

        #endregion

        //設定の書き込み
        public bool WriteIniFile()
        {
            return TextFile.Write(IniFilePath, items.ToStr(), false, encoding);
        }
    }

    #region TextFile

    public class TextFile
    {
        //書き込み
        public static bool Write(string file, string word)//上書き保存
        {
            return Write(file, word, false, EncodeLib.GetJpEncoding(file) ?? EncodeLib.UTF8);
        }
        public static bool Write(string file, string word, bool add, Encoding e)//ファイル名、書き込み文字列、追加書き込み(上書きfalse)、エンコーディング
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
                sw?.Close();
            }

            return check;
        }

        //読み込み
        public static string Read(string file)
        {
            return Read(file, EncodeLib.GetJpEncoding(file) ?? EncodeLib.UTF8);
        }
        public static string Read(string file, Encoding e)
        {
            string allText = "";

            StreamReader Reader = null;

            try
            {
                if (File.Exists(file))
                {
                    if (new FileInfo(file).Length != 0)
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))//読み取り専用で開く
                        {
                            Reader = new StreamReader(fs, e);
                            allText = Reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Reader?.Close();
            }

            return allText;
        }
    }

    #endregion

    #region EncodeLib

    public static class EncodeLib
    {
        public static Encoding SJIS => Encoding.GetEncoding(932);
        public static Encoding UTF8withBOM => new UTF8Encoding(true);
        public static Encoding UTF8 => new UTF8Encoding(false);

        //文字コード変更
        public static bool ChangeEncode(string file, Encoding encodingNext)
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
                    Encoding encodingCurrent = GetJpEncoding(file);

                    if (encodingCurrent == null)
                    {
                        return false;
                    }
                    else
                    {
                        if (encodingCurrent != encodingNext)
                        {
                            //読み込み
                            string allText = TextFile.Read(file, encodingCurrent);

                            if (allText == null)
                            {
                                return false;
                            }
                            else
                            {
                                TextFile.Write(file, allText, false, encodingNext);
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
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
