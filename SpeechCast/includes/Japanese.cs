using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace MMFrame.Text.Language
{
    /// <summary>
    /// 日本語に関するクラス
    /// </summary>
    public class Japanese
    {
        /// <summary>
        /// カタカナのテーブル
        /// </summary>
        private static readonly string[] KATAKANA_TABLE = new string[] 
        {
             "ガ", "ｶﾞ", "ギ", "ｷﾞ", "グ", "ｸﾞ", "ゲ", "ｹﾞ", "ゴ", "ｺﾞ", 
             "ザ", "ｻﾞ", "ジ", "ｼﾞ", "ズ", "ｽﾞ", "ゼ", "ｾﾞ", "ゾ", "ｿﾞ", 
             "ダ", "ﾀﾞ", "ヂ", "ﾁﾞ", "ヅ", "ﾂﾞ", "デ", "ﾃﾞ", "ド", "ﾄﾞ", 
             "バ", "ﾊﾞ", "ビ", "ﾋﾞ", "ブ", "ﾌﾞ", "ベ", "ﾍﾞ", "ボ", "ﾎﾞ",
             "ヴ", "ｳﾞ", "ヷ", "ﾜﾞ", "ヸ", "ｲﾞ", "ヹ", "ｴﾞ", "ヺ", "ｦﾞ",
             "パ", "ﾊﾟ", "ピ", "ﾋﾟ", "プ", "ﾌﾟ", "ペ", "ﾍﾟ", "ポ", "ﾎﾟ", 
             "ア", "ｱ", "イ", "ｲ", "ウ", "ｳ", "エ", "ｴ", "オ", "ｵ",
             "カ", "ｶ", "キ", "ｷ", "ク", "ｸ", "ケ", "ｹ", "コ", "ｺ",
             "サ", "ｻ", "シ", "ｼ", "ス", "ｽ", "セ", "ｾ", "ソ", "ｿ",
             "タ", "ﾀ", "チ", "ﾁ", "ツ", "ﾂ", "テ", "ﾃ", "ト", "ﾄ",
             "ナ", "ﾅ", "ニ", "ﾆ", "ヌ", "ﾇ", "ネ", "ﾈ", "ノ", "ﾉ",
             "ハ", "ﾊ", "ヒ", "ﾋ", "フ", "ﾌ", "ヘ", "ﾍ", "ホ", "ﾎ",
             "マ", "ﾏ", "ミ", "ﾐ", "ム", "ﾑ", "メ", "ﾒ", "モ", "ﾓ",
             "ヤ", "ﾔ",            "ユ", "ﾕ",            "ヨ", "ﾖ", 
             "ラ", "ﾗ", "リ", "ﾘ", "ル", "ﾙ", "レ", "ﾚ", "ロ", "ﾛ",
             "ワ", "ﾜ",            "ヲ", "ｦ",            "ン", "ﾝ",
             "ヱ", "ｴ",
             "ァ", "ｧ", "ィ", "ｨ", "ゥ", "ｩ", "ェ", "ｪ", "ォ", "ｫ",
             "ャ", "ｬ",            "ュ", "ｭ",            "ョ", "ｮ",
             "ッ", "ｯ", 
             "ー", "ｰ", "、", "､", "。", "｡"
        };

        /// <summary>
        /// 文字がひらがなかどうかを評価します。
        /// </summary>
        /// <param name="srcChar">評価する文字</param>
        /// <returns><paramref name="srcChar"/> がひらがななら true</returns>
        public static bool IsHiragana(char srcChar)
        {
            // 、。
            // ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとど
            // なにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ
            // ゝゞゟー
            return ('\u3001' <= srcChar && srcChar <= '\u3002') || ('\u3041' <= srcChar && srcChar <= '\u3096') || ('\u309D' <= srcChar && srcChar <= '\u309F') || srcChar == '\u30FC';
        }

        /// <summary>
        /// 文字列がひらがなかどうかを評価します。
        /// </summary>
        /// <param name="srcString">評価する文字列</param>
        /// <returns><paramref name="srcString"/> がひらがななら true</returns>
        public static bool IsHiragana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return false;
            }

            foreach (char c in srcString)
            {
                if (!IsHiragana(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 文字が全角カタカナかどうかを評価します。
        /// </summary>
        /// <param name="srcChar">評価する文字</param>
        /// <returns><paramref name="srcChar"/> が全角カタカナなら true</returns>
        public static bool IsKatakana(char srcChar)
        {
            // 、。
            // ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトド
            // ナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ
            // ーヽヾヿ
            return ('\u3001' <= srcChar && srcChar <= '\u3002') || ('\u30A1' <= srcChar && srcChar <= '\u30FA') || ('\u30FC' <= srcChar && srcChar <= '\u30FF');
        }

        /// <summary>
        /// 文字列が全角カタカナかどうかを評価します。
        /// </summary>
        /// <param name="srcString">評価する文字列</param>
        /// <returns><paramref name="srcString"/> が全角カタカナなら true</returns>
        public static bool IsKatakana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return false;
            }

            foreach (char c in srcString)
            {
                if (!IsKatakana(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 文字が半角カタカナかどうかを評価します。
        /// </summary>
        /// <param name="srcChar">評価する文字</param>
        /// <returns><paramref name="srcChar"/> が半角カタカナなら true</returns>
        public static bool IsKatakanaHalf(char srcChar)
        {
            // ｦｧｨｩｪｫｬｭｮｯ || ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟｰ､｡
            return srcChar == '\uFF61' || srcChar == '\uFF64' || ('\uFF66' <= srcChar && srcChar <= '\uFF6F') || ('\uFF70' <= srcChar && srcChar <= '\uFF9F');
        }

        /// <summary>
        /// 文字列が半角カタカナかどうかを評価します。
        /// </summary>
        /// <param name="srcString">評価する文字列</param>
        /// <returns><paramref name="srcString"/> が半角カタカナなら true</returns>
        public static bool IsKatakanaHalf(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return false;
            }

            foreach (char c in srcString)
            {
                if (!IsKatakanaHalf(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 文字列が漢字（CJK統合漢字、CJK互換漢字、CJK統合漢字拡張A、CJK統合漢字拡張B）かどうかを評価します。
        /// </summary>
        /// <param name="srcString">評価する文字列</param>
        /// <returns><paramref name="srcString"/> が漢字なら true</returns>
        public static bool IsKanji(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return false;
            }

            for (int i = 0; i < srcString.Length; i++)
            {
                char c1 = srcString[i];
                char c2 = (i < srcString.Length) ? srcString[i + 1] : default(char);

                if (c1 == '\u3005')
                {
                    // 々
                    continue;
                }
                else if ('\u3400' <= c1 && c1 <= '\u4DBF')
                {
                    // CJK統合漢字拡張A
                    continue;
                }
                else if ('\u4E00' <= c1 && c1 <= '\u9FCF')
                {
                    // CJK統合漢字
                    continue;
                }
                else if (System.Char.IsHighSurrogate(c1) && ((('\uD840' <= c1 && c1 < '\uD869') && System.Char.IsLowSurrogate(c2)) || (c1 == '\uD869' && ('\uDC00' <= c2 && c2 <= '\uDEDF'))))
                {
                    // CJK統合漢字拡張B
                    i++;
                    continue;
                }
                else if ('\uF900' <= c1 && c1 <= '\uFAFF')
                {
                    // CJK互換漢字
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 全角カタカナをひらがなに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>全角カタカナがひらがなに変換された <paramref name="srcString"/></returns>
        public static string ToHiraganaFromKatakana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            System.Text.StringBuilder dstStr = new System.Text.StringBuilder();

            foreach (char c in srcString)
            {
                if (c == '\u309F')
                {
                    // ヿ
                    dstStr.Append("こと");
                }
                else if (('\u3001' <= c && c <= '\u3002') || c == '\u30FC')
                {
                    // 、。ー
                    dstStr.Append(c);
                }
                else if ('\u30F7' <= c && c <= '\u30FA')
                {
                    // ヷヸヹヺ
                    dstStr.AppendFormat("{0}゛", (char)(c - 0x0068));
                }
                else if (IsKatakana(c))
                {
                    // ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトド
                    // ナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶ
                    dstStr.Append((char)(c - 0x0060));
                }
                else
                {
                    dstStr.Append(c);
                }
            }

            return dstStr.ToString();
        }

        /// <summary>
        /// 半角カタカナをひらがなに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>半角カタカナがひらがなに変換された <paramref name="srcString"/></returns>
        public static string ToHiraganaFromKatakanaHalf(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            System.Text.StringBuilder dstStr = new System.Text.StringBuilder();

            for (int i = 0; i < srcString.Length; i++)
            {
                char c1 = srcString[i];
                char c2 = (i < srcString.Length - 1) ? srcString[i + 1] : default(char);

                if (IsKatakanaHalf(c1))
                {
                    string half = c1.ToString();

                    if (c2 == 'ﾞ' || c2 == 'ﾟ')
                    {
                        half = string.Format("{0}{1}", c1, c2);
                        i++;
                    }

                    string wide = ToKatakanaFromKatakanaHalf(half);
                    string hiragana = ToHiraganaFromKatakana(wide);

                    dstStr.Append(hiragana);
                }
                else
                {
                    dstStr.Append(c1);
                }
            }

            return dstStr.ToString();
        }

        /// <summary>
        /// ひらがなを全角カタカナに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>ひらがなが全角カタカナに変換された <paramref name="srcString"/></returns>
        public static string ToKatakanaFromHiragana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            System.Text.StringBuilder dstStr = new System.Text.StringBuilder();

            foreach (char c in srcString)
            {
                if (c == '\u309F')
                {
                    // ゟ
                    dstStr.Append("ヨリ");
                }
                else if (('\u3001' <= c && c <= '\u3002') || c == '\u30FC')
                {
                    // 、。ー
                    dstStr.Append(c);
                }
                else if (IsHiragana(c))
                {
                    // ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとど
                    // なにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖゝゞ
                    dstStr.Append((char)(c + 0x0060));
                }
                else
                {
                    dstStr.Append(c);
                }
            }

            return dstStr.ToString();
        }

        /// <summary>
        /// 半角カタカナを全角カタカナに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>半角カタカナが全角カタカナに変換された <paramref name="srcString"/></returns>
        public static string ToKatakanaFromKatakanaHalf(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            for (int i = 0; i < KATAKANA_TABLE.Length; i++)
            {
                srcString = srcString.Replace(KATAKANA_TABLE[i + 1], KATAKANA_TABLE[i]);
                i++;
            }

            return srcString;
        }

        /// <summary>
        /// ひらがなを半角カタカナに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>ひらがなが半角カタカナに変換された <paramref name="srcString"/></returns>
        public static string ToKatakanaHalfFromHiragana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            System.Text.StringBuilder dstStr = new System.Text.StringBuilder();

            foreach (char c in srcString)
            {
                if (c == '\u309F')
                {
                    // ゟ
                    dstStr.Append("ﾖﾘ");
                }
                else if (IsHiragana(c))
                {
                    // ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとど
                    // なにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖゝゞ
                    string wide = ToKatakanaFromHiragana(c.ToString());
                    string half = ToKatakanaHalfFromKatakana(wide);

                    dstStr.Append(half);
                }
                else
                {
                    dstStr.Append(c);
                }
            }

            return dstStr.ToString();
        }

        /// <summary>
        /// 全角カタカナを半角カタカナに変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>全角カタカナが半角カタカナに変換された <paramref name="srcString"/></returns>
        public static string ToKatakanaHalfFromKatakana(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            for (int i = 0; i < KATAKANA_TABLE.Length; i++)
            {
                srcString = srcString.Replace(KATAKANA_TABLE[i], KATAKANA_TABLE[i + 1]);
                i++;
            }

            return srcString;
        }

        /// <summary>
        /// 濁点や半濁点を前の文字と合成して 1 つの文字に変換します。
        /// </summary>
        /// <param name="srcString">変換する文字列</param>
        /// <returns>濁点や半濁点が前の文字と合成された <paramref name="srcString"/></returns>
        public static string NormalizeSoundSymbol(string srcString)
        {
            if (System.String.IsNullOrEmpty(srcString))
            {
                return "";
            }

            System.Text.StringBuilder dstStr = new System.Text.StringBuilder();

            for (int i = 0; i < srcString.Length; i++)
            {
                char c1 = srcString[i];
                char c2 = (i < srcString.Length - 1) ? srcString[i + 1] : default(char);
                char dstChar = c1;

                if (c2 == '\u3099' || c2 == '\u309B' || c2 == '\uFF9E')
                {
                    // 濁点
                    int mod2 = c1 % 2;
                    int mod3 = c1 % 3;

                    if (mod2 == 1 || ('\u304B' <= c1 && c1 <= '\u3061') || ('\u30AB' <= c1 && c1 <= '\u30C1'))
                    {
                        // がぎぐげご || ガギグゲゴ
                        dstChar = (char)(c1 + 0x01);
                    }
                    else if (mod2 == 0 || ('\u3064' <= c1 && c1 <= '\u3068') || ('\u30C4' <= c1 && c1 <= '\u30C8'))
                    {
                        // づでど || ヅデド
                        dstChar = (char)(c1 + 0x01);
                    }
                    else if (mod3 == 0 && ('\u306F' <= c1 && c1 <= '\u307B') || ('\u30CF' <= c1 && c1 <= '\u30DB'))
                    {
                        // ばびぶべぼ || バビブベボ
                        dstChar = (char)(c1 + 0x01);
                    }
                    else if (c1 == '\u3032' || c1 == '\u309D' || c1 == '\u30FD')
                    {
                        // 〲ゞヾ
                        dstChar = (char)(c1 + 0x01);
                    }
                    else if (c1 == '\u3046' || c1 == '\u30A6')
                    {
                        // ゔ || ヴ
                        dstChar = (char)(c1 + 0x004E);
                    }
                    else if ('\u30EF' <= c1 && c1 <= '\u30F2')
                    {
                        // ヷヸヹヺ
                        dstChar = (char)(c1 + 0x08);
                    }

                    if (c1 != dstChar)
                    {
                        i++;
                    }
                }
                else if (c2 == '\u309A' || c2 == '\u309C' || c2 == '\uFF9F')
                {
                    // 半濁点
                    int mod3 = c1 % 3;

                    if (mod3 == 0 && ('\u306F' <= c1 && c1 <= '\u307B') || ('\u30CF' <= c1 && c1 <= '\u30DB'))
                    {
                        // ぱぴぷぺぽ || パピプペポ
                        dstChar = (char)(c1 + 0x02);
                        i++;
                    }
                }

                dstStr.Append(dstChar);
            }

            return dstStr.ToString();
        }
    }
}