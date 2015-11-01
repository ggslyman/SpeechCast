using System;

using System.Linq;

// 対応関係が無いと思われる文字は扱っていない

public static class KataHira
{

    public static string ToKatakana(this string s)
    {

        return new string(s.Select(c => (c >= 'ぁ' && c <= 'ゖ') ? (char)(c + 'ァ' - 'ぁ') : c).ToArray());

    }

    public static string ToHiragana(this string s)
    {

        return new string(s.Select(c => (c >= 'ァ' && c <= 'ヶ') ? (char)(c + 'ぁ' - 'ァ') : c).ToArray());

    }

}