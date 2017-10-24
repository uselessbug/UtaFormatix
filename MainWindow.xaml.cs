using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace UtaFormatix
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Version = "ver1.51EX";
        Data maindata;
        Data exportingdata;
        bool Imported = false;
        public MainWindow()
        {
            InitializeComponent();
            LblVer.Content = Version;
        }

        private void ExportVsq4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if(!Transformlyrics(UtaFormat.Vsq4))
                {
                    return;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Export";
                saveFileDialog.Filter = "VOCALOID Project|*.vsqx";
                saveFileDialog.FileName = exportingdata.ProjectName;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    try
                    {
                        exportingdata.ExportVsq4(filename);
                        MessageBox.Show("Vsqx is successfully exported." + Environment.NewLine + "If it only pronounces \"a\", please select all notes and run \"Lyrics\"→\"Convert Phonemes\". ", "Export Vsqx");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        MessageBox.Show("Error occured", "Failed");
                    }
                }
                exportingdata = null;
            }
            else
            {
                MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportCcs(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if (!Transformlyrics(UtaFormat.Ccs))
                {
                    return;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Export";
                saveFileDialog.Filter = "CeVIO Project|*.ccs";
                saveFileDialog.FileName = exportingdata.ProjectName;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    try
                    {
                        exportingdata.ExportCcs(filename);
                        MessageBox.Show("Ccs is successfully exported", "Succeed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        MessageBox.Show("Error occured", "Failed");
                    }
                }
                exportingdata = null;
            }
            else
            {
                MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportUst(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if (!Transformlyrics(UtaFormat.Ust))
                {
                    return;
                }
                FolderBrowserDialog selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Please choose a folder to export ust(s):";
                selectFolder.SelectedPath = exportingdata.Files[0].Replace(System.IO.Path.GetFileName(exportingdata.Files[0]), "");
                if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string foldpath = selectFolder.SelectedPath;
                    string omit = exportingdata.ExportUst(foldpath);
                    MessageBox.Show(omit + "Ust is successfully exported.", "Export Ust");
                }
                exportingdata = null;
            }
            else
            {
                MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void Import(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import",
                Filter =
                    "Project File|*.vsqx;*.ccs;*.ust|VOCALOID Project|*.vsqx|UTAU Project|*.ust|CeVIO Project|*.ccs",
                FileName = string.Empty,
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            List<string> fileNames = new List<string>();
            fileNames.AddRange(openFileDialog.FileNames);
            maindata = new Data();
            Imported = maindata.Import(fileNames);
            if (!Imported)
            {
                MessageBox.Show("The format of this file is not supported.", "Import");
            }
        }

        private void BtnImport_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 0.1;
            BtnImport_Cover.Visibility = Visibility.Visible;
        }

        private void BtnImport_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 1;
            BtnImport_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 0.1;
            BtnExportCcs_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportUst_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 0.1;
            BtnExportUst_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 0.1;
            BtnExportVsqx_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 1;
            BtnExportVsqx_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportUst_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 1;
            BtnExportUst_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 1;
            BtnExportCcs_Cover.Visibility = Visibility.Hidden;
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            Droping.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            Droping.Visibility = Visibility.Hidden;
        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) ?
                System.Windows.DragDropEffects.Link : System.Windows.DragDropEffects.None;
            string[] dropfiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            maindata = new Data();
            Imported = maindata.Import(dropfiles?.ToList());
            Droping.Visibility = Visibility.Hidden;
            if (!Imported)
            {
                MessageBox.Show("The format of this file is not supported.", "Import");
            }
        }

        bool Transformlyrics(UtaFormat toFormat)
        {
            ChangeLyrics changelyrics = new ChangeLyrics();
            switch (maindata.Lyric.AnalyzedType)
            {
                case Lyric.LyricType.None:
                    MessageBox.Show("The type of the lyrics is not detected, please select the correct type by yourself.", "Lyrics Transformation");
                    changelyrics.RdoRomaCV.IsChecked = true;
                    break;
                case Lyric.LyricType.Romaji_Tandoku:
                    changelyrics.RdoRomaCV.IsChecked = true;
                    break;
                case Lyric.LyricType.Romaji_Renzoku:
                    changelyrics.RdoRomaVCV.IsChecked = true;
                    break;
                case Lyric.LyricType.Kana_Tandoku:
                    changelyrics.RdoKanaCV.IsChecked = true;
                    break;
                case Lyric.LyricType.Kana_Renzoku:
                    changelyrics.RdoKanaVCV.IsChecked = true;
                    break;
                default:
                    break;
            }
            changelyrics.Rdo2KanaCV.IsChecked = true;
            switch (toFormat)
            {
                case UtaFormat.Vsq4:
                    changelyrics.Rdo2RomaVCV.Visibility = Visibility.Hidden;
                    changelyrics.Rdo2KanaVCV.Visibility = Visibility.Hidden;
                    changelyrics.Rdo2KanaCV.Margin = changelyrics.Rdo2RomaVCV.Margin;
                    break;
                case UtaFormat.Ccs:
                    changelyrics.Rdo2RomaCV.Visibility = Visibility.Hidden;
                    changelyrics.Rdo2RomaVCV.Visibility = Visibility.Hidden;
                    changelyrics.Rdo2KanaVCV.Visibility = Visibility.Hidden;
                    changelyrics.Rdo2KanaCV.Margin = changelyrics.Rdo2RomaCV.Margin;
                    break;
                default:
                    break;
            }
            var dialogResult = changelyrics.ShowDialog();
            if (dialogResult == true)
            {
                exportingdata.Lyric.Transform(changelyrics.fromType, changelyrics.ToType);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
