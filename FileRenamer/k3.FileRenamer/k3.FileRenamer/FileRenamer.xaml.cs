using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace k3.FileRenamer
{
    /// <summary>
    /// Interaktionslogik für FileRenamer.xaml
    /// </summary>
    public partial class FileRenamer : Window
    {
        private string _folderPath;
        private List<string> _fileNamesSrc;

        public FileRenamer()
        {
            _folderPath = String.Empty;
            _fileNamesSrc = new List<string>();

            InitializeComponent();
        }

        private void Btn_openFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sb_status.Items.Clear();
                getPathFromFileDialog();
                getFilesFromDirectory();
                fillFileExtensionFilterBox();
                sb_status.Items.Add("Folder path and list for filter added.");
            }
            catch (Exception ex)
            {
                sb_status.Items.Add(ex.Message);
            }

        }

        private void fillFileExtensionFilterBox()
        {
            //get file extension from path
            List<string> filter = new List<string>();
            foreach (var item in _fileNamesSrc)
            {
                filter.Add(System.IO.Path.GetExtension(item));
            }

            //remove dublicated entries
            string neighbor = string.Empty;
            filter.Sort();
            neighbor = filter[0];
            for (int i = 1; i < filter.Count(); i++)
            {
                if (filter[i] == neighbor)
                {
                    filter.RemoveAt(i);
                    i--;
                }
                else
                {
                    neighbor = filter[i];
                }
            }

            //put allFiles to first place
            filter.Insert(0, ".*");

            //add asterisk to all items
            for (int i = 0; i < filter.Count(); i++)
            {
                filter[i] = "*" + filter[i];
            }

            //fill filter box
            cb_fileTyp.ItemsSource = filter;

        }

        private void getFilesFromDirectory()
        {
            string[] fileEntries = Directory.GetFiles(_folderPath);
            foreach (string fileName in fileEntries)
            {
                _fileNamesSrc.Add(fileName);
            }

        }


        private void getPathFromFileDialog()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            _folderPath = dialog.FileName.ToString();
            tb_path.Text = _folderPath;
        }
    }
}
