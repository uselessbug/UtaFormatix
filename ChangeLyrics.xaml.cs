using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UtaFormatix
{
    /// <summary>
    /// ChangeLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeLyrics : Window
    {
        public ChangeLyrics()
        {
            InitializeComponent();
        }
        public Lyric.LyricType fromType = Lyric.LyricType.None;
        public Lyric.LyricType ToType = Lyric.LyricType.None;

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void button_confirm_Click(object sender, RoutedEventArgs e)
        {
            if(RdoRomaCV.IsChecked==true || RdoRomaVCV.IsChecked == true || RdoKanaCV.IsChecked == true || RdoKanaVCV.IsChecked == true)
            {
                if (Rdo2RomaCV.IsChecked == true || Rdo2RomaVCV.IsChecked == true || Rdo2KanaCV.IsChecked == true || Rdo2KanaVCV.IsChecked == true)
                {
                    if (RdoRomaCV.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.Romaji_Tandoku;
                    }
                    else if (RdoRomaVCV.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.Romaji_Renzoku;
                    }
                    else if (RdoKanaCV.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.Kana_Tandoku;
                    }
                    else if (RdoKanaVCV.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.Kana_Renzoku;
                    }
                    if (Rdo2RomaCV.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.Romaji_Tandoku;
                    }
                    else if (Rdo2RomaVCV.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.Romaji_Renzoku;
                    }
                    else if (Rdo2KanaCV.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.Kana_Tandoku;
                    }
                    else if (Rdo2KanaVCV.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.Kana_Renzoku;
                    }
                    DialogResult = true;
                    Close();
                    return;
                }
            }
            DialogResult = false;
            MessageBox.Show("Please select the types correctly.", "Lyrics Transformation");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
