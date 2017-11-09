using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtaFormatix.Model;
using static UtaFormatix.Data;

namespace UtaFormatix
{
    public class Lyric
    {
        private Data _data;
        private List<string> _kanaList = new List<string>();
        private List<string> _romajiList = new List<string>();
        public Lyric(Data data, bool doAnalyze)
        {
            _kanaList = _kanas.ToList();
            _kanaList.Sort((s1, s2) => s2.Length - s1.Length);
            _romajiList = _romajis.ToList();
            _romajiList.Sort((s1, s2) => s2.Length - s1.Length);
            this._data = data;
            if (doAnalyze)
            {
                LyricTypeAnalyze();
            }
        }

        public LyricType AnalyzedType = LyricType.None;

        private LyricType InferLyricType(string target)
        {
            bool haveSpace = target.Contains(" ");
            foreach (var g in _kanaList)
            {
                if (target.Contains(g))
                {
                    return haveSpace ? LyricType.Kana_Tandoku : LyricType.Kana_Renzoku;
                }
            }
            foreach (var g in _romajiList)
            {
                if (target.Contains(g))
                {
                    return haveSpace ? LyricType.Romaji_Tandoku : LyricType.Romaji_Renzoku;
                }
            }
            return LyricType.None;
        }

        void LyricTypeAnalyze()
        {
            LyricType projectLyrictype = LyricType.None;
            foreach (Track track in _data.TrackList)
            {
                List<Note> noteList = track.NoteList;
                int[] typecount = new int[4] { 0, 0, 0, 0 };
                foreach (Note note in noteList)
                {
                    switch (InferLyricType(note.NoteLyric))
                    {
                        case LyricType.None:
                            break;
                        case LyricType.Romaji_Tandoku:
                            typecount[0]++;
                            break;
                        case LyricType.Romaji_Renzoku:
                            typecount[1]++;
                            break;
                        case LyricType.Kana_Tandoku:
                            typecount[2]++;
                            break;
                        case LyricType.Kana_Renzoku:
                            typecount[3]++;
                            break;
                    }
                }
                LyricType trackLyrictype = LyricType.None;
                for (int i = 0; i < 4; i++)
                {
                    if (100 * typecount[i] / noteList.Count > 50)
                    {
                        if (trackLyrictype == LyricType.None)
                        {
                            trackLyrictype = (LyricType)(i + 1);
                        }
                        else
                        {
                            trackLyrictype = LyricType.None;
                            break;
                        }
                    }
                }
                if (trackLyrictype > 0)
                {
                    if (projectLyrictype == LyricType.None)
                    {
                        projectLyrictype = trackLyrictype;
                    }
                    else
                    {
                        if (projectLyrictype != trackLyrictype)
                        {
                            return;
                        }
                    }
                }
            }
            AnalyzedType = projectLyrictype;
        }
        public void Transform(LyricType fromtype, LyricType totype)
        {
            if (fromtype != LyricType.None && totype != LyricType.None)
            {
                CleanLyric(fromtype);
                switch (fromtype - totype)
                {
                    case 0: break;
                    case 1:
                        {
                            if (fromtype != LyricType.Kana_Tandoku)
                            {
                                TransLyricsTan2Ren(TransDirection.Reverse);
                            }
                            else
                            {
                                TransLyricsRomaji2Kana(TransDirection.Reverse);
                                TransLyricsTan2Ren(TransDirection.Sequential);
                            }
                            break;
                        }
                    case 2:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Reverse);
                            break;
                        }
                    case 3:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Reverse);
                            TransLyricsTan2Ren(TransDirection.Reverse);
                            break;
                        }
                    case -1:
                        {
                            if (fromtype != LyricType.Romaji_Renzoku)
                            {
                                TransLyricsTan2Ren(TransDirection.Sequential);
                            }
                            else
                            {
                                TransLyricsRomaji2Kana(TransDirection.Sequential);
                                TransLyricsTan2Ren(TransDirection.Reverse);
                            }
                            break;
                        }
                    case -2:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Sequential);
                            break;
                        }
                    case -3:
                        {
                            TransLyricsRomaji2Kana(TransDirection.Sequential);
                            TransLyricsTan2Ren(TransDirection.Sequential);
                            break;
                        }
                }
            }
        }


        void CleanLyric(LyricType type)
        {
            foreach (Track track in _data.TrackList)
            {
                List<Note> noteList = track.NoteList;
                switch (type)
                {
                    case LyricType.Romaji_Tandoku:
                        foreach (Note note in noteList)
                        {
                            var lrc = note.NoteLyric.ToLower();
                            if (string.IsNullOrWhiteSpace(lrc))
                            {
                                continue;
                            }
                            lrc = lrc.Replace("?", "");
                            note.NoteLyric = _romajiList.Find(s => lrc.Contains(s)) ?? lrc;
                        }
                        break;
                    //未考虑罗马音单独音除“?”以外的前缀
                    case LyricType.Romaji_Renzoku:
                        foreach (Note note in noteList)
                        {
                            var lrc = note.NoteLyric.ToLower();
                            if (string.IsNullOrWhiteSpace(lrc))
                            {
                                continue;
                            }

                            if (lrc.Contains(" "))
                            {
                                var lrcs = lrc.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                                StringBuilder sb = new StringBuilder();
                                foreach (var s in lrcs)
                                {
                                    sb.Append(_romajiList.Find(roma => s.Contains(roma)) ?? lrc);
                                    sb.Append(' ');
                                }
                                note.NoteLyric = sb.ToString().TrimEnd();
                                //if (body != "" && IsGobi(note.NoteLyric.Substring(blankPos - 1, 1)))
                                //{
                                //    note.NoteLyric = note.NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                //}
                            }
                            else
                            {
                                lrc = lrc.Replace("?", "");
                                note.NoteLyric = _romajiList.Find(s => lrc.Contains(s)) ?? lrc;
                            }
                        }
                        break;
                    case LyricType.Kana_Tandoku:
                        {
                            foreach (Note note in noteList)
                            {
                                var lrc = note.NoteLyric.ToLower();
                                if (string.IsNullOrWhiteSpace(lrc))
                                {
                                    continue;
                                }
                                lrc = lrc.Replace("?", "");
                                note.NoteLyric = _kanaList.Find(s => lrc.Contains(s)) ?? lrc;
                            }
                            break;
                        }
                    case LyricType.Kana_Renzoku:
                        {
                            foreach (Note note in noteList)
                            {
                                var lrc = note.NoteLyric.ToLower();
                                if (string.IsNullOrWhiteSpace(lrc))
                                {
                                    continue;
                                }

                                if (lrc.Contains(" "))
                                {
                                    var lrcs = lrc.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    StringBuilder sb = new StringBuilder();
                                    foreach (var s in lrcs)
                                    {
                                        sb.Append(_kanaList.Find(kana => s.Contains(kana)) ?? lrc);
                                        sb.Append(' ');
                                    }
                                    note.NoteLyric = sb.ToString().TrimEnd();
                                    //if (body != "" && IsGobi(note.NoteLyric.Substring(blankPos - 1, 1)))
                                    //{
                                    //    note.NoteLyric = note.NoteLyric.Substring(blankPos - 1, 1) + " " + body;
                                    //}
                                }
                                else
                                {
                                    lrc = lrc.Replace("?", "");
                                    note.NoteLyric = _kanaList.Find(s => lrc.Contains(s)) ?? lrc;
                                }
                            }
                            break;
                        }
                }
            }
        }
        void TransLyricsRomaji2Kana(TransDirection mode)
        {
            foreach (Track track in _data.TrackList)
            {
                List<Note> noteList = track.NoteList;
                switch (mode)
                {
                    case TransDirection.None:  //Do nothing 
                        break;
                    case TransDirection.Sequential:  //Romaji to Kana
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            if (noteList[i].NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = noteList[i].NoteLyric.Substring(0, 1);
                                string body = noteList[i].NoteLyric.Remove(0, 2);
                                noteList[i].NoteLyric = head + " " + ConvertRomajiToKana(body);
                            }
                            else //is TanDokuOn
                            {
                                noteList[i].NoteLyric = ConvertRomajiToKana(noteList[i].NoteLyric);
                            }
                        }
                        break;
                    case TransDirection.Reverse:  //Kana to Romaji 
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            if (noteList[i].NoteLyric.Contains(" ")) //is RenZokuOn
                            {
                                string head = noteList[i].NoteLyric.Substring(0, 1);
                                string body = noteList[i].NoteLyric.Remove(0, 2);
                                noteList[i].NoteLyric = head + " " + ConvertKanaToRomaji(body);
                            }
                            else //is TanDokuOn
                            {
                                noteList[i].NoteLyric = ConvertKanaToRomaji(noteList[i].NoteLyric);
                            }
                        }
                        break;
                }
            }
        }
        void TransLyricsTan2Ren(TransDirection mode)
        {
            foreach (Track track in _data.TrackList)
            {
                List<Note> noteList = track.NoteList;
                switch (mode)
                {
                    case TransDirection.None:  //Do nothing 
                        break;
                    case TransDirection.Sequential:  //TanDoku to RenZoku
                        string tailOfLast = "-";
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            string tail = tailOfLast;
                            if (i > 0 && noteList[i].NoteTimeOn > noteList[i - 1].NoteTimeOff)
                            {
                                tail = "-";
                            }
                            if (FindKana(noteList[i].NoteLyric) != -1)  //is Kana
                            {
                                tailOfLast = ConvertKanaToRomaji(noteList[i].NoteLyric).Substring(ConvertKanaToRomaji(noteList[i].NoteLyric).Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else if (FindRomaji(noteList[i].NoteLyric) != -1) //is Romaji
                            {
                                tailOfLast = noteList[i].NoteLyric.Substring(noteList[i].NoteLyric.Length - 1, 1);
                                noteList[i].NoteLyric = tail + " " + noteList[i].NoteLyric;
                            }
                            else
                            {
                                tailOfLast = "-";
                            }
                        }
                        break;
                    case TransDirection.Reverse:  //RenZoku to TanDoku
                        for (int i = 0; i < noteList.Count; i++)
                        {
                            if (noteList[i].NoteLyric.Contains(" ") && (FindKana(noteList[i].NoteLyric.Remove(0, 2)) != -1 || FindRomaji(noteList[i].NoteLyric.Remove(0, 2)) != -1))
                            {
                                noteList[i].NoteLyric = noteList[i].NoteLyric.Remove(0, 2);
                            }
                        }
                        break;
                }
            }
        }
        string ConvertKanaToRomaji(string kana)
        {
            for (int i = 0; i < _kanas.Length; i++)
            {
                if (kana == _kanas[i])
                {
                    return _romajis[i];
                }
            }
            return kana;
        }
        string ConvertRomajiToKana(string romaji)
        {
            for (int i = 0; i < _kanas.Length; i++)
            {
                if (romaji == _romajis[i])
                {
                    return _kanas[i];
                }
            }
            return romaji;
        }
        int FindKana(string kana)
        {
            for (int i = 0; i < _kanas.Length; i++)
            {
                if (kana == _kanas[i])
                {
                    return i;
                }
            }
            return -1;
        }
        int FindRomaji(string romaji)
        {
            for (int i = 0; i < _kanas.Length; i++)
            {
                if (romaji == _romajis[i])
                {
                    return i;
                }
            }
            return -1;
        }
        bool IsGobi(string gobi)
        {
            if (gobi.Length != 1)
            {
                return false;
            }
            return gobi == "a" || gobi == "i" || gobi == "u" || gobi == "e" || gobi == "o" || gobi == "n" || gobi == "-";
        }
        public enum LyricType
        {
            None,
            Romaji_Tandoku,
            Romaji_Renzoku,
            Kana_Tandoku,
            Kana_Renzoku,
        }
        enum TransDirection
        {
            None,
            Sequential,
            Reverse,
        }
        static string[] _kanas = { "あ", "い", "いぇ", "う", "わ", "うぁ", "うぁ", "うぃ", "うぃ", "うぇ", "え", "お", "か", "が", "き", "きぇ", "きゃ", "きゅ", "きょ", "ぎ", "ぎぇ", "ぎゃ", "ぎゅ", "ぎょ", "く", "くぁ", "くぃ", "くぇ", "くぉ", "ぐ", "ぐぁ", "ぐぃ", "ぐぇ", "ぐぉ", "け", "げ", "こ", "ご", "さ", "ざ", "し", "し", "しぇ", "しぇ", "しゃ", "しゃ", "しゅ", "しゅ", "しょ", "しょ", "じ", "じぇ", "じぇ", "じゃ", "じゃ", "じゅ", "じゅ", "じょ", "じょ", "す", "すぁ", "すぃ", "すぇ", "すぉ", "ず", "ずぁ", "ずぃ", "ずぇ", "ずぉ", "せ", "ぜ", "そ", "ぞ", "た", "だ", "ち", "ちぇ", "ちゃ", "ちゅ", "ちょ", "つ", "つ", "つぁ", "つぁ", "つぃ", "つぃ", "つぇ", "つぇ", "つぉ", "つぉ", "て", "てぃ", "てゅ", "で", "でぃ", "でゅ", "と", "とぅ", "とぅ", "ど", "どぅ", "どぅ", "な", "に", "にぇ", "にゃ", "にゅ", "にょ", "ぬ", "ぬぁ", "ぬぃ", "ぬぇ", "ぬぉ", "ね", "の", "は", "ば", "ぱ", "ひ", "ひぇ", "ひゃ", "ひゅ", "ひょ", "び", "びぇ", "びゃ", "びゅ", "びょ", "ぴ", "ぴぇ", "ぴゃ", "ぴゅ", "ぴょ", "ふ", "ふぁ", "ふぃ", "ふぇ", "ふぉ", "ぶ", "ぶぁ", "ぶぃ", "ぶぇ", "ぶぉ", "ぷ", "ぷぁ", "ぷぃ", "ぷぇ", "ぷぉ", "へ", "べ", "ぺ", "ほ", "ぼ", "ぽ", "ま", "み", "みぇ", "みゃ", "みゅ", "みょ", "む", "むぁ", "むぃ", "むぇ", "むぉ", "め", "も", "や", "ゆ", "よ", "ら", "り", "りぇ", "りゃ", "りゅ", "りょ", "る", "るぁ", "るぃ", "るぇ", "るぉ", "れ", "ろ", "わ", "を", "うぉ", "ん", "ー" };
        static string[] _romajis = { "a", "i", "ye", "u", "wa", "wa", "ua", "wi", "ui", "we", "e", "o", "ka", "ga", "ki", "kye", "kya", "kyu", "kyo", "gi", "gye", "gya", "gyu", "gyo", "ku", "kua", "kui", "kue", "kuo", "gu", "gua", "gui", "gue", "guo", "ke", "ge", "ko", "go", "sa", "za", "shi", "si", "she", "sye", "sha", "sya", "shu", "syu", "sho", "syo", "ji", "je", "jye", "ja", "jya", "ju", "jyu", "jo", "jyo", "su", "sua", "sui", "sue", "suo", "zu", "zua", "zui", "zue", "zuo", "se", "ze", "so", "zo", "ta", "da", "chi", "che", "cha", "chu", "cho", "tsu", "tu", "tsa", "tua", "tsi", "tui", "tse", "tue", "tso", "tuo", "te", "ti", "tyu", "de", "di", "dyu", "to", "tu", "twu", "do", "du", "dwu", "na", "ni", "nye", "nya", "nyu", "nyo", "nu", "nua", "nui", "nue", "nuo", "ne", "no", "ha", "ba", "pa", "hi", "hye", "hya", "hyu", "hyo", "bi", "bye", "bya", "byu", "byo", "pi", "pye", "pya", "pyu", "pyo", "fu", "fa", "fi", "fe", "fo", "bu", "bua", "bui", "bue", "buo", "pu", "pua", "pui", "pue", "puo", "he", "be", "pe", "ho", "bo", "po", "ma", "mi", "mye", "mya", "myu", "myo", "mu", "mua", "mui", "mue", "muo", "me", "mo", "ya", "yu", "yo", "ra", "ri", "rye", "rya", "ryu", "ryo", "ru", "rua", "rui", "rue", "ruo", "re", "ro", "wa", "o", "wo", "n", "-" };
    }
}
