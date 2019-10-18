using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;

namespace _1712862_1712867_1712884
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class Method
        {
            public string Name { get; set; }

            override public string ToString()
            {
                return Name;
            }
        }

        class FileOrFolder
        {
            public string Name { set; get; }
            public string Path { set; get; }
        }

        //Truy cập CSDL - Lấy tên File/Folder
        class FileOrFolderDAO
        {
            /// <summary>
            /// Lấy đường dẫn cho sự kiện load tất cả các Files/Folders
            /// </summary>
            /// <returns>Đường dẫn dưới dạng chuỗi</returns>
            private static string getDirectoryPath()
            {
                var dlg = new CommonOpenFileDialog();
                dlg.IsFolderPicker = true;

                var status = dlg.ShowDialog();

                if (status == CommonFileDialogResult.Ok)
                {
                    return dlg.FileName.ToString();
                }
                else if (status == CommonFileDialogResult.Cancel)
                {
                    return "Cancel";
                }
                else
                {
                    return "None";
                }
            }

            /// <summary>
            /// Lấy các đường dẫn của các Files/Folders được chọn
            /// </summary>
            /// <param name="isFileSelected">Lấy File hay Folder</param>
            /// <returns>Một list các đường dẫn dưới dạng chuỗi</returns>
            private static List<string> getMutiDirectoryPath(bool isFileSelected)
            {
                var dlg = new CommonOpenFileDialog();
                dlg.Multiselect = true;

                if (!isFileSelected)
                {
                    dlg.IsFolderPicker = true;
                }

                var status = dlg.ShowDialog();

                if (status == CommonFileDialogResult.Ok)
                {
                    return dlg.FileNames.ToList<string>();
                }
                else if (status == CommonFileDialogResult.Cancel)
                {
                    return null;
                }
                else
                {
                    return null;
                }
            }


            /// <summary>
            /// Lấy tên và đường dẫn từ đường dẫn ban đầu
            /// </summary>
            /// <param name="rawPath">Đường dẫn thô ban đầu</param>
            /// <param name="name">Tên File/Folder sẽ trả ra</param>
            /// <param name="path">Tên đường dẫn sẽ trả ra</param>
            private static void getNameAndPathFromRawPath(string rawPath, out string name, out string path)
            {
                name = null;
                path = null;
                int index = rawPath.LastIndexOf("\\");

                path = rawPath.Substring(0, index + 1);
                name = rawPath.Substring(++index);
            }

            /// <summary>
            /// Load các Files/Folders vào ObservableCollection
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="directoryOfFileList">Danh sách Files hoặc Folders</param>
            /// <param name="Path">Đường dẫn ban đầu</param>
            /// <returns></returns>
            private static ObservableCollection<FileOrFolder> loadAllFileOrFolder<T>(T[] directoryOfFileList, string path)
            {
                ObservableCollection<FileOrFolder> output = new ObservableCollection<FileOrFolder>();

                foreach (var item in directoryOfFileList)
                {
                    output.Add(new FileOrFolder() { Name = item.ToString(), Path = path });
                }

                return output;
            }

            /// <summary>
            /// Binding dữ liệu của sự kiện "Add single element" vào item source
            /// </summary>
            /// <param name="isFile">Binding vào File hay Folder</param>
            /// <returns></returns>
            public ObservableCollection<FileOrFolder> bindingSingleData(bool isFile)
            {
                ObservableCollection<FileOrFolder> output = new ObservableCollection<FileOrFolder>();
                var rawPath = getMutiDirectoryPath(isFile);

                if (rawPath != null)
                {
                    string path = null;
                    string name = null;

                    foreach (var item in rawPath)
                    {
                        getNameAndPathFromRawPath(item, out name, out path);
                        output.Add(new FileOrFolder() { Name = name, Path = path });
                    }
                }
                return output;
            }

            /// <summary>
            /// Binding dữ liệu của sự kiện "Add all elements" vào itemsource
            /// </summary>
            /// <param name="istListFile">binding vào file hay folder</param>
            /// <returns></returns>
            public ObservableCollection<FileOrFolder> bindingAllData(bool istListFile)
            {
                ObservableCollection<FileOrFolder> output = null;
                var path = getDirectoryPath();

                if (path != "Cancel" && path != "None")
                {
                    var currentDirectory = new DirectoryInfo(path);
                    if (istListFile)
                    {
                        output = loadAllFileOrFolder(currentDirectory.GetFiles(), path);
                    }
                    else
                    {
                        output = loadAllFileOrFolder(currentDirectory.GetDirectories(), path);
                    }
                }
                return output;
            }


            /// <summary>
            /// Chép tên file từ source sang preview, cho nó có dữ liệu để thao tác
            /// </summary>
            /// <param name="sourc">file/folder Data</param>
            /// <param name="dest">file/folder preview</param>
            public ObservableCollection<string> cloneToPreview(ObservableCollection<FileOrFolder> source)
            {
                ObservableCollection<string> output = new ObservableCollection<string>();
                foreach (var item in source)
                {
                    output.Add(item.Name);
                }
                return output;
            }
        }

        class FileOrFolderBus // Lưu ý: Class này chỉ thao tác với con preview (cái cột file name) kiểu string
        {
            /// <summary>
            /// Viết hoa từng kí từng đầu của mỗi chữ cách nhau bởi khoảng trắng
            /// </summary>
            /// <param name="inputName">tên cần viết hoa</param>
            public static void UpperFirstLetter(ref string inputName)
            {
                string dummyString = inputName;
                StringBuilder newName = new StringBuilder(dummyString);
                newName[0] = char.ToUpper(newName[0]);

                for (int i = 1; i < dummyString.Length; i++)
                {
                    if (dummyString[i] != ' ' && dummyString[i - 1] == ' ')
                    {
                        newName[i] = char.ToUpper(newName[i]);
                    }
                    else
                    {
                        newName[i] = char.ToLower(newName[i]);
                    }
                }

                inputName = newName.ToString();
            }

            /// <summary>
            /// Viết hoa tất cả kí tự hoặc viết thường tất cả kí tự
            /// </summary>
            /// <param name="inputName">cái tên cần chỉnh</param>
            /// <param name="mode">mode = 1 : viết hoa, mode = 2 : viết thường</param>
            public static void NewCase(ref string inputName, int mode)
            {
                if (mode == 1)
                {
                    inputName = inputName.ToUpper();
                }
                else if (mode == 2)
                {
                    inputName = inputName.ToLower();
                }
            }

            public static void Replace(ref string inputName, string oldString, string newString)
            {
                bool isContain = inputName.Contains(oldString);
                if (isContain)
                {
                    if (newString == null)
                    {
                        inputName = inputName.Remove(inputName.IndexOf(oldString), oldString.Count());
                    }
                    else
                    {
                        inputName = inputName.Replace(oldString, newString);
                    }
                }
                else
                {

                }
            }


            public static void Unique(ref string inputName)
            {
                Guid guid = Guid.NewGuid();
                inputName = guid.ToString();
            }

            /// <summary>
            /// Áp dụng các tùy chỉnh của người dùng lên tên file/folder
            /// </summary>
            /// <param name="currentFileOrFolder">Đối tưởng File/Folder hiện tại</param>
            /// <param name="newName">Tên mà đối tượng hiện tại sẽ đổi sang</param>
            /// <param name="isFileRename">Đổi tên File hay Folder</param>
            public static void applyChange(ref FileOrFolder currentFileOrFolder, string newName, bool isFileRename)
            {

                string oldPath = currentFileOrFolder.Path + "\\" + currentFileOrFolder.Name;
                string newPath = currentFileOrFolder.Path + "\\" + newName;

                if (isFileRename)
                {
                    File.Move(oldPath, newPath);
                }
                else //Vấn đề: với Folder thì nó không phân biệt hoa thường, vd FILENAME và fileaname là không khác nhau với hệ thống
                {   //Nên khi người dùng chọn viết hoa tất cả chúng ta sẽ gặp lỗi => đổi qua tên trung gian rồi đổi lại theo ý người dùng
                    string dummyName = "asd" + currentFileOrFolder.Name; // đây là con trung gian
                    string dummyPath = currentFileOrFolder.Path + "\\" + dummyName;
                    Directory.Move(oldPath, dummyPath);
                    Directory.Move(dummyPath, newPath);
                }
                currentFileOrFolder.Name = newName;
            }

            /// <summary>
            /// Chuẩn hóa họ tên
            /// </summary>
            /// <param name="inputName">Tên File/Folder cần chuẩn hóa</param>
            public static void normalizeFullName(ref string inputName)
            {
                inputName = inputName.Trim();
                
                for (int i = 1; i < inputName.Length; i++)
                {
                    if (inputName[i] == ' ' && inputName[i - 1] == ' ')
                    {
                        inputName = inputName.Remove(i, 1);
                        i--;
                    }
                }
                UpperFirstLetter(ref inputName);
            }

            /// <summary>
            /// Kiểm tra xem 2 Files/Folders có trùng nhau hay không??
            /// </summary>
            /// <param name="name1">Tên File/Folder 1</param>
            /// <param name="name2">Tên File/Folder 2</param>
            /// <returns></returns>
            public static int checkAlikeFileOrFolder(string name1, string name2)
            {
                int check = String.Compare(name1, name2);
                return check;
            }


            /// <summary>
            /// Thêm hậu tố vào tên File/Folder
            /// </summary>
            /// <param name="inputName">Tên File/Folder cần thêm hậu tố</param>
            /// <param name="suffix">Hậu tố cần thêm vào File/Folder</param>
            public static void addSuffix(ref string inputName, int suffix)
            {
                inputName = inputName + suffix;
            }


            /// <summary>
            /// Kiểm tra xem tên File có theo thứ tự "Số ISBN - Tên file" hay không??
            /// </summary>
            /// <param name="inputName">Tên File cần kiểm tra</param>
            /// <returns></returns>
            public static bool checkOrdered(string inputName)
            {
                bool check = true;
                for (int i = 0; i <= 12; i++)
                { 
                    if (inputName[i] == '-' || ((Convert.ToInt32(inputName[i]) - 48) >= 0 && (Convert.ToInt32(inputName[i]) - 48 <= 9)))
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }

                if (check == true)
                {
                    if (inputName[13] != ' ')
                    {
                        check = false;
                    }
                }
                return check;
            }

            /// <summary>
            /// Chuyển mã số ISBN ra phía trước tên File/Folder
            /// </summary>
            /// <param name="inputName">Tên File/Folder cần thay đổi</param>
            /// <param name="isFile">File or Folder</param>
            public static void moveISBNBefore(ref string inputName, bool isFile)
            {
                if (checkOrdered(inputName) == true)
                {
                    return;
                }
                string temp1 = null;
                string temp2 = null;
                if (isFile)
                {
                    int count = 0;
                    string extensionName = null;
                    for (int i = inputName.Length - 1; i >= 0; i--)
                    {
                        count++;
                        if (inputName[i] == '.')
                        {
                            break;
                        }
                    }
                    if (count == inputName.Length)
                    {
                        return;
                    }
                    extensionName = inputName.Substring(inputName.Length - count);
                    temp2 = inputName.Substring(inputName.Length - 13 - count);
                    temp2 = temp2.Substring(0, temp2.Length - count);
                    temp1 = inputName.Substring(0, inputName.Length - 14 - count);
                    temp2 += ' ';
                    temp2 += temp1;
                    temp2 += extensionName;
                }
                else
                {
                    temp2 = inputName.Substring(inputName.Length - 13);
                    temp1 = inputName.Substring(0, inputName.Length - 14);
                    temp2 += ' ';
                    temp2 += temp1;
                }
                inputName = temp2;
            }


            /// <summary>
            /// Chuyển mã số ISBN ra phía sau tên File/Folder
            /// </summary>
            /// <param name="inputName">Tên File/Folder cần thay đổi</param>
            /// <param name="isFile">File or Folder</param>
            public static void moveISBNAfter(ref string inputName, bool isFile)
            {
                if (checkOrdered(inputName) == false)
                {
                    return;
                }
                string temp1 = inputName.Substring(0, 13);
                string temp2 = null;
                if(isFile)
                {
                    int count = 0;
                    string extensionName = null;
                    for (int i = inputName.Length - 1; i >= 0; i--)
                    {
                        count++;
                        if (inputName[i] == '.')
                        {
                            break;
                        }
                    }
                    extensionName = inputName.Substring(inputName.Length - count);
                    temp2 = inputName.Substring(14, inputName.Length - 14 - count);
                    temp2 += ' ';
                    temp2 += temp1;
                    temp2 += extensionName;
                }
               else
                {
                    temp2 = inputName.Substring(14);
                    temp2 += ' ';
                    temp2 += temp1;
                }
                inputName = temp2;
            }
        }






        /// <summary>
        /// ///////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addMethod.SelectedItem.ToString().Contains("New Case") == true)
            {
                checkAddMethod(Method1, "New Case");
            }
            else if (addMethod.SelectedItem.ToString().Contains("Replace") == true)
            {
                checkAddMethod(Method2, "Replace");
            }
            else if (addMethod.SelectedItem.ToString().Contains("Unique Name") == true)
            {
                checkAddMethod(Method3, "Unique Name");
            }
            else if (addMethod.SelectedItem.ToString().Contains("Fullname Normalize") == true)
            {
                checkAddMethod(Method4, "Fullname Normalize");
            }
            else if (addMethod.SelectedItem.ToString().Contains("Move") == true)
            {
                checkAddMethod(Method5, "Move");
            }

            addMethod.SelectedIndex = 0;
        }

        void checkAddMethod(StackPanel numberMethod, string methodName)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == methodName)
                {
                    check = 1;
                    break;
                }
            }
            if (check == 0)
            {
                method.Add(new Method() { Name = methodName });
                numberMethod.Background = Brushes.Blue;
            }
        }


        // Cái đối tượng, biến và hàm cần thiết để chạy chương trình 
        BindingList<Method> method = new BindingList<Method>();
        FileOrFolderDAO toolDAO = new FileOrFolderDAO();
        ObservableCollection<FileOrFolder> fileData = null;
        ObservableCollection<FileOrFolder> folderData = null;
        ObservableCollection<string> filePreview = null;
        ObservableCollection<string> folderPreview = null;
        string oldString = null;
        string newString = null;


        private void getEveryThingReady(bool isFile)
        {
            if (isFile && filePreview == null)
            {
                this.filePreview = toolDAO.cloneToPreview(this.fileData);
                NewFileNameList.ItemsSource = filePreview;
            }
            else if (!isFile && folderPreview == null)
            {
                this.folderPreview = toolDAO.cloneToPreview(this.folderData);
                NewFolderNameList.ItemsSource = folderPreview;

            }

        }

        //--------------------------------SỰ KIỆN---------------------------------------



        private void addFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addFile.SelectedItem.ToString().Contains("Simple Element") == true)
            {
                if (fileData != null)
                {
                    fileData.Clear();
                }
                fileData = toolDAO.bindingSingleData(true);
                FileNameList.ItemsSource = fileData;
                FilePath.ItemsSource = fileData;
            }
            else if (addFile.SelectedItem.ToString().Contains("All Elements") == true)
            {
                if (fileData != null)
                {
                    fileData.Clear();
                }
                fileData = toolDAO.bindingAllData(true);
                FileNameList.ItemsSource = fileData;
                FilePath.ItemsSource = fileData;
            }
            addFile.SelectedIndex = 0;
        }

        private void addFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(addFolder.SelectedItem.ToString().Contains("Simple Element")==true)
            {
                if (folderData != null)
                {
                    folderData.Clear();
                }
                folderData = toolDAO.bindingSingleData(false);
                FolderNameList.ItemsSource = folderData;
                FolderPath.ItemsSource = folderData;
            }
            else if(addFolder.SelectedItem.ToString().Contains("All Elements")==true)
            {
                if (folderData != null)
                {
                    folderData.Clear();
                }
                folderData = toolDAO.bindingAllData(false);
                FolderNameList.ItemsSource = folderData;
                FolderPath.ItemsSource = folderData;
            }
            addFolder.SelectedIndex = 0;
        }


        private void UpperCaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.TabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.NewCase(ref temp, 1);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.NewCase(ref temp, 1);
                    folderPreview[i] = temp;
                }
            }
        }

        private void UpperCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void LowerCaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.TabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.NewCase(ref temp, 2);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.NewCase(ref temp, 2);
                    folderPreview[i] = temp;
                }
            }
        }

        private void LowerCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void UpperFirstLetterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.TabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.UpperFirstLetter(ref temp);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);
                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.UpperFirstLetter(ref temp);
                    folderPreview[i] = temp;
                }
            }
        }

        private void UpperFirstLetterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void StartBatchButton_Click(object sender, RoutedEventArgs e)
        {
            //Nếu filePreview không có giá trị thì hiện thông báo bạn chưa đổi tên file hoặc folder
            if (filePreview == null && folderPreview == null)
            {
                MessageBox.Show("Not change name yet");
                return;
            }

            var tabSelected = ((TabItem)(this.TabControl.SelectedItem)).Header.ToString();

            bool mode = (tabSelected == "Rename Files") ? true : false;

            //Kiểm tra trùng tên
            bool check = false;
            if (mode == true)
            {
                for (int i = 0; i < filePreview.Count - 1; i++)
                {
                    for (int j = i + 1; j < filePreview.Count; j++)
                    {
                        if (FileOrFolderBus.checkAlikeFileOrFolder(filePreview[i], filePreview[j]) == 0)
                        {
                            check = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < folderPreview.Count - 1; i++)
                {
                    for (int j = i + 1; j < folderPreview.Count; j++)
                    {
                        if (FileOrFolderBus.checkAlikeFileOrFolder(folderPreview[i], folderPreview[j]) == 0)
                        {
                            check = true;
                            break;
                        }
                    }
                }
            }

            if (check == true)
            {
                MessageBoxResult result = MessageBox.Show("Having 2 Files Aliked.\nChoose: Yes (Add Suffix); No (Get Old Name)", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch(result)
                {
                    case MessageBoxResult.Yes:
                        if (mode == true)
                        {
                            int suffix = 1;
                            for (int i = filePreview.Count - 1; i >= 1; i--)
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    if (FileOrFolderBus.checkAlikeFileOrFolder(filePreview[i], filePreview[j]) == 0)
                                    {
                                        var temp = filePreview[j];
                                        FileOrFolderBus.addSuffix(ref temp, suffix);
                                        filePreview[j] = temp;
                                        suffix++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int suffix = 1;
                            for (int i = folderPreview.Count - 1; i >= 1; i--)
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    if (FileOrFolderBus.checkAlikeFileOrFolder(folderPreview[i], folderPreview[j]) == 0)
                                    {
                                        var temp = folderPreview[j];
                                        FileOrFolderBus.addSuffix(ref temp, suffix);
                                        folderPreview[j] = temp;
                                        suffix++;
                                    }
                                }
                            }
                        }
                        break;
                    case MessageBoxResult.No:
                        if (mode == true)
                        {
                            bool check1 = false;
                            for (int i = 0; i < filePreview.Count - 1; i++)
                            {
                                for (int j = i + 1; j < filePreview.Count; j++)
                                {
                                    if (FileOrFolderBus.checkAlikeFileOrFolder(filePreview[i], filePreview[j]) == 0)
                                    {
                                        check1 = true;
                                        filePreview[j] = fileData[j].Name;
                                    }
                                }
                                if (check1 == true)
                                {
                                    filePreview[i] = fileData[i].Name;
                                    check1 = false;
                                }
                            }
                        }
                        else
                        {
                            bool check1 = false;
                            for (int i = 0; i < folderPreview.Count - 1; i++)
                            {
                                for (int j = i + 1; j < folderPreview.Count; j++)
                                {
                                    if (FileOrFolderBus.checkAlikeFileOrFolder(folderPreview[i], folderPreview[j]) == 0)
                                    {
                                        check1 = true;
                                        folderPreview[j] = folderData[j].Name;
                                    }
                                }
                                if (check1 == true)
                                {
                                    folderPreview[i] = folderData[i].Name;
                                    check1 = false;
                                }
                            }
                        }
                        break;
                }
            }

            int numbersOfData = this.fileData != null ? this.fileData.Count : this.folderData.Count;
            for (int i = 0; i < numbersOfData; i++)
            {
                if (mode == true)
                {
                    var temp = fileData[i];
                    FileOrFolderBus.applyChange(ref temp, filePreview[i], mode);
                    fileData[i] = temp;
                }
                else
                {
                    var temp = folderData[i];
                    FileOrFolderBus.applyChange(ref temp, folderPreview[i], mode);
                    folderData[i] = temp;
                }
            }
            MessageBox.Show("Saved");
        }

        private void OldStringTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NewStringTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        bool _isReplaceButtonClicked = false;
        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            _isReplaceButtonClicked = true;
            oldString = this.OldStringTextBox.Text;
            newString = this.NewStringTextBox.Text;
            var tabSelected = ((TabItem)this.TabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.Replace(ref temp, oldString, newString);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.Replace(ref temp, oldString, newString);
                    folderPreview[i] = temp;
                }
            }
        }

        bool _isUniqueNameButtonClicked = false;
        private void UniqueNameButton_Click(object sender, RoutedEventArgs e)
        {
            _isUniqueNameButtonClicked = true;
            var tabSelected = ((TabItem)this.TabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.Unique(ref temp);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.Unique(ref temp);
                    folderPreview[i] = temp;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileData.Clear();
            this.folderData.Clear();
            this.filePreview.Clear();
            this.folderPreview.Clear();
        }



        bool _isFullNameNormalizeButtonClicked = false;
        private void Nomalize_Click(object sender, RoutedEventArgs e)
        {
            _isFullNameNormalizeButtonClicked = true;
            var tabSelected = ((TabItem)this.TabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.normalizeFullName(ref temp);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.normalizeFullName(ref temp);
                    folderPreview[i] = temp;
                }
            }
        }

        private void NewCaseMethod_Expanded(object sender, RoutedEventArgs e)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == "New Case")
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                MessageBox.Show("You just haven't Added Method. Please add method before.");
                NewCaseMethod.IsExpanded = false;
            }
        }

        private void ReplaceMethod_Expanded(object sender, RoutedEventArgs e)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == "Replace")
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                MessageBox.Show("You just haven't Added Method. Please add method before.");
                ReplaceMethod.IsExpanded = false;
            }
        }

        private void UniqueNameMethod_Expanded(object sender, RoutedEventArgs e)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == "Unique Name")
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                MessageBox.Show("You just haven't Added Method. Please add method before.");
                UniqueNameMethod.IsExpanded = false;
            }
        }

        private void FullnameNormalizeMethod_Expanded(object sender, RoutedEventArgs e)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == "Fullname Normalize")
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                MessageBox.Show("You just haven't Added Method. Please add method before.");
                FullnameNormalizeMethod.IsExpanded = false;
            }
        }

        private void MoveMethod_Expanded(object sender, RoutedEventArgs e)
        {
            int check = 0;
            foreach (var i in method)
            {
                if (i.Name.ToString() == "Move")
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                MessageBox.Show("You just haven't Added Method. Please add method before.");
                MoveMethod.IsExpanded = false;
            }
        }

        private void MoveBefore_Click(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)this.TabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename Files")
            {
                
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.moveISBNBefore(ref temp, true);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.moveISBNBefore(ref temp, false);
                    folderPreview[i] = temp;
                }
            }
        }

        private void MoveAfter_Click(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)this.TabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename Files")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.moveISBNAfter(ref temp, true);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);

                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.moveISBNAfter(ref temp, false);
                    folderPreview[i] = temp;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveWindow screen = new SaveWindow();

            //Mở cửa sổ mới để người dụng nhập tên preset
            if (screen.ShowDialog() == true)
            {
                string nameFile = screen.PreSetName;
                StreamWriter fileOut = new StreamWriter($"Preset/{nameFile}.txt");
                WriteToFile(fileOut);
                presetNames.Add(nameFile);
            }
        }

        private void WriteToFile(StreamWriter fileOut)
        {
            if (UpperCaseCheckBox.IsChecked.HasValue && UpperCaseCheckBox.IsChecked.Value == true)
            {
                fileOut.WriteLine("1");
            }
            if (LowerCaseCheckBox.IsChecked.HasValue && LowerCaseCheckBox.IsChecked.Value == true)
            {
                fileOut.WriteLine("2");
            }
            if (UpperFirstLetterCheckBox.IsChecked.HasValue && UpperFirstLetterCheckBox.IsChecked.Value == true)
            {
                fileOut.WriteLine("3");
            }
            if (_isReplaceButtonClicked)
            {
                fileOut.WriteLine($"4 {OldStringTextBox.Text} {NewStringTextBox.Text}");
            }
            if (_isUniqueNameButtonClicked)
            {
                fileOut.WriteLine($"5");
            }
            if (_isFullNameNormalizeButtonClicked)
            {
                fileOut.WriteLine($"6");
            }
            fileOut.Close();
        }


        //Chuỗi dùng để chưa các các tên của preset sau đó sẽ binding tên này vào combobox preset
        BindingList<string> presetNames = new BindingList<string>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var dir = new DirectoryInfo("Preset\\");
            var presetNameFiles = dir.GetFiles();

            foreach (var file in presetNameFiles)
            {

                presetNames.Add(file.Name.Replace(".txt", ""));
            }

            PresetComboBox.Items.Clear();
            PresetComboBox.ItemsSource = presetNames;
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var presetName = this.PresetComboBox.SelectedItem.ToString();
            //Mo File
            try
            {
                var fileIn = File.ReadAllLines($"Preset\\{presetName}.txt");
                foreach (var line in fileIn)
                {
                    ExecuteLine(line);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Cannot open file");
            }
        }

        private void ExecuteLine(string line)
        {
            var newLine = SplitLine(line);
            if (Int32.Parse(newLine[0]) == 1)
            {
                UpperCaseCheckBox.IsChecked = true;
                return;
            }
            if (Int32.Parse(newLine[0]) == 2)
            {
                LowerCaseCheckBox.IsChecked = true;
                return;
            }
            if (Int32.Parse(newLine[0]) == 3)
            {
                UpperFirstLetterCheckBox.IsChecked = true;
                return;
            }
            if (Int32.Parse(newLine[0]) == 4)
            {
                this.OldStringTextBox.Text = newLine[1];
                this.NewStringTextBox.Text = newLine[2];
                this.ReplaceButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                return;
            }
            if (Int32.Parse(newLine[0]) == 5)
            {
                this.UniqueNameButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                return;
            }
            if (Int32.Parse(newLine[0]) == 6)
            {
                this.FullnameNormalizeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                return;
            }
        }
        private string[] SplitLine(string line)
        {
            const string spliter = " ";
            string[] tokens;
            tokens = line.Split(new string[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }
    }
}
