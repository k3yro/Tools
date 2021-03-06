﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        struct __renameObj
        {
            public string path;
            public string extension;
            public string filenameOld;
            public string filenameNew;
        }

        private List<__renameObj> _renameObj;

        public FileRenamer()
        {
            _folderPath = String.Empty;
            _fileNamesSrc = new List<string>();
            _renameObj = new List<__renameObj>();

            InitializeComponent();
        }

        #region Trigger
        private void Btn_openFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sb_status.Items.Clear();
                getPathFromFileDialog();
                initFolder();
                sb_status.Items.Add("Folder path and list for filter added.");
                loadAllFileNamesIntoResultView();
            }
            catch (Exception ex)
            {
                sb_status.Items.Add(ex.Message);
            }

        }

        private void Cb_fileTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            loadAllFileNamesIntoResultView();
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            renameAllFilesInFolder(true/*preview*/);
        }

        private void Btn_rename_Click(object sender, RoutedEventArgs e)
        {
            renameAllFilesInFolder(false/*no preview*/);
        }
        #endregion

        private void renameAllFilesInFolder(bool preview)
        {
            sb_status.Items.Clear();
            if (String.IsNullOrEmpty(tb_search.Text))
            {
                sb_status.Items.Add("Search/replace field empty.");
                return;
            }

            try
            {
                loadPreviewIntoResultView();//2in1

                //Progressbar
                pb_rename.Value = 0;
                int progressHelper = 0;
                tbl_result.Text = string.Empty;

                foreach (__renameObj item in _renameObj)
                {
                    if (!preview)
                    {
                        //Progressbar
                        progressHelper++;
                        pb_rename.Value = progressHelper * 100 / _fileNamesSrc.Count;
                    }

                    if (null != cb_fileTyp.SelectedValue)
                    {
                        if ("" != cb_fileTyp.SelectedValue.ToString())
                        {
                            if (".*" != cb_fileTyp.SelectedValue.ToString())
                            {
                                if (item.extension != cb_fileTyp.SelectedValue.ToString())
                                {
                                    continue;
                                }
                            }
                        }
                    }            
                    
                    if (item.filenameNew != item.filenameOld)
                    {
                        string fileNew = _folderPath + "\\" + item.filenameNew + item.extension;
                        string fileOld = item.path;

                        if (!preview)
                        {
                            File.Move(fileOld, fileNew);
                        }

                        tbl_result.Text += item.filenameNew + item.extension + Environment.NewLine;
                    }
                }
                sb_status.Items.Clear();
                sb_status.Items.Add("Files renamed successfully.");
                initFolder();
            }
            catch (Exception ex)
            {
                sb_status.Items.Clear();
                sb_status.Items.Add(ex.Message);
            }
        }

        private void initFolder()
        {
            getFilesFromDirectory();
            fillFileExtensionFilterBox();
        }

        //calls also createNewFilename Method
        private void loadPreviewIntoResultView()
        {
            _renameObj = null;
            _renameObj = new List<__renameObj>();

            tbl_result.Text = string.Empty;

            foreach (var item in _fileNamesSrc)
            {
                __renameObj robj = new __renameObj();
                robj.path = item;
                robj.extension = System.IO.Path.GetExtension(item);
                robj.filenameOld = System.IO.Path.GetFileNameWithoutExtension(item);
                robj.filenameNew = createNewFilename(robj.filenameOld);
                _renameObj.Add(robj);
            }
        }

        private string createNewFilename(string filenameOld)
        {
            if (String.IsNullOrEmpty(tb_search.Text))
            {
                return filenameOld;
            }
            string newFileName = string.Empty;
            string search = tb_search.Text;
            string replace = tb_replace.Text;

            if (cb_regex.IsChecked == true)
            {
                newFileName = Regex.Replace(filenameOld, search, replace);
            }
            else
            {
                newFileName = filenameOld.Replace(search, replace);
            }

            return newFileName;
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

            //fill filter box
            cb_fileTyp.ItemsSource = filter;

        }

        private void getFilesFromDirectory()
        {
            _fileNamesSrc = null;
            _fileNamesSrc = new List<string>();
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

        private void loadAllFileNamesIntoResultView()
        {
            //clear Resultbox
            tbl_result.Text = string.Empty;

            //get extension from textbox
            string extension = string.Empty;
            if (null != cb_fileTyp.SelectedValue)
            {
                extension = cb_fileTyp.SelectedValue.ToString();
            }

            //print file names
            foreach (var item in _fileNamesSrc)
            {
                string result = string.Empty;
                string curr_extension = System.IO.Path.GetExtension(item);
                if (string.IsNullOrEmpty(extension) || curr_extension == extension || ".*" == extension)
                {
                    result = System.IO.Path.GetFileNameWithoutExtension(item);
                    tbl_result.Text += result + curr_extension + Environment.NewLine;
                }
            }
        }
    }
}
