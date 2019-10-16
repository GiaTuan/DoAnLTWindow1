using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace WpfApp2
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
        class FileOrFolder
        {
            public string name { set; get; }
            public string path { set; get; }
        }

        //Truy cập CSDL - lấy tên file/folder
        class FileOrFolderDAO
        {
            /// <summary>
            /// Lấy đường dẫn cho sự kiện load tất cả file/folder
            /// </summary>
            /// <returns>Đường dẫn dưới dạng chuỗi</returns>
            private static string getDirectoryPath()
            {
                var dlg = new CommonOpenFileDialog();
                dlg.IsFolderPicker = true;

                var status = dlg.ShowDialog();

                if (status == CommonFileDialogResult.Ok)
                    return dlg.FileName.ToString();
                else if (status == CommonFileDialogResult.Cancel)
                    return "Cancel";
                else
                    return "None";
            }
            /// <summary>
            /// Lấy các đường dẫn của các file/folder được chọn
            /// </summary>
            /// <param name="isFileSelected">Lấy file hay folder</param>
            /// <returns>Một list các đường dẫn dưới dạng chuỗi</returns>
            private static List<string> getMutiDirectoryPath(bool isFileSelected)
            {
                var dlg = new CommonOpenFileDialog();
                dlg.Multiselect = true;

                if (!isFileSelected)
                    dlg.IsFolderPicker = true;

                var status = dlg.ShowDialog();

                if (status == CommonFileDialogResult.Ok)
                    return dlg.FileNames.ToList<string>();
                else if (status == CommonFileDialogResult.Cancel)
                    return null;
                else
                    return null;
            }

            /// <summary>
            /// Lấy tên và đường dẫn từ đường dẫn ban đầu
            /// </summary>
            /// <param name="rawPath"> đường dẫn thô ban đầu</param>
            /// <param name="name">tên file/folder sẽ trả ra</param>
            /// <param name="path">tên đường dẫn sẽ trả ra</param>
            private static void getNameAndPathFromRawPath(string rawPath, out string name, out string path)
            {
                name = null;
                path = null;
                int index = rawPath.LastIndexOf("\\");

                path = rawPath.Substring(0, index + 1);
                name = rawPath.Substring(++index);
            }

            /// <summary>
            /// load các file/folder vào ObservableCollection
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="directoryOfFileList">danh sách file hoặc folder</param>
            /// <param name="Path">đường dẫn ban đầu</param>
            /// <returns></returns>
            private static ObservableCollection<FileOrFolder> loadAllFileOrFolder<T>(T[] directoryOfFileList, string Path)
            {
                ObservableCollection<FileOrFolder> output = new ObservableCollection<FileOrFolder>();

                foreach (var item in directoryOfFileList)
                    output.Add(new FileOrFolder() { name = item.ToString(), path = Path });

                return output;
            }

            /// <summary>
            /// binding dữ liệu của sự kiện add single element vào item source
            /// </summary>
            /// <param name="isFile">binding vào file hay folder</param>
            /// <returns></returns>
            public ObservableCollection<FileOrFolder> bindingSingleData(bool isFile)
            {
                ObservableCollection<FileOrFolder> output = new ObservableCollection<FileOrFolder>();
                var rawPath = getMutiDirectoryPath(isFile);

                if (rawPath != null)
                {
                    string Path = null;
                    string Name = null;
                    foreach (var item in rawPath)
                    {
                        getNameAndPathFromRawPath(item, out Name, out Path);
                        output.Add(new FileOrFolder() { name = Name, path = Path });
                    }

                }
                return output;
            }
            /// <summary>
            /// binding dữ liệu của sự kiện add all elements vào itemsource
            /// </summary>
            /// <param name="istListFile">binding vào file hay folder</param>
            /// <returns></returns>
            public ObservableCollection<FileOrFolder> bindingData(bool istListFile)
            {
                ObservableCollection<FileOrFolder> output = null;
                var path = getDirectoryPath();
                if (path != "Cancel" && path != "None")
                {
                    var currentDirectory = new DirectoryInfo(path);
                    if (istListFile)
                        output = loadAllFileOrFolder(currentDirectory.GetFiles(), path);
                    else
                        output = loadAllFileOrFolder(currentDirectory.GetDirectories(), path);
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
                   output.Add(item.name);
                return output;
            }
           
        }

        class FileOrFolderBus // Lu ý: class này chỉ thao tác với con preview (cái cột file name) kiểu string
        {
            /// <summary>
            /// Viết hoa từng kí từng đầu của mỗi chữ cách nhau bởi khoảng trắng
            /// </summary>
            /// <param name="inputName">tên cần viết hoa</param>
            public static void UpperFirstLetter(ref string inputName)
            {
                string dummyString = inputName;
                StringBuilder newName = new StringBuilder(dummyString);

                if (dummyString[0] != ' ')
                {
                    newName[0] = char.ToUpper(newName[0]);
                }

                for (int i = 1; i < dummyString.Length; i++)
                    if (dummyString[i] != ' ' && dummyString[i - 1] == ' ')
                        newName[i] = char.ToUpper(newName[i]);                 
                    else                   
                        newName[i] = char.ToLower(newName[i]);

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
                    inputName = inputName.ToUpper();
                else if (mode == 2)
                    inputName = inputName.ToLower();
            }

            public static void Replace(ref string inputName, string oldString, string newString)
            {
                bool isContain=inputName.Contains(oldString);
                if(isContain)
                {
                    if(newString== null)
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
            /// <param name="currentFileOrFolder">đối tưởng file/folder hiện tại</param>
            /// <param name="newName">tên mà đối tượng hiện tại sẽ đổi sang</param>
            /// <param name="isFileRename">đổi tên file hay folder</param>
            public static void applyChange(ref FileOrFolder currentFileOrFolder, string newName,bool isFileRename)
            {
                
                string oldPath = currentFileOrFolder.path + "\\" + currentFileOrFolder.name;
                string newPath = currentFileOrFolder.path + "\\" + newName;

                if(isFileRename)
                    File.Move(oldPath, newPath);               
                else // vấn đề: với folder thì nó không phân biệt hoa thường, vd FILENAME và fileaname là không khác nhau với hệ thống
                {// nên khi người dùng chọn viết hoa tất cả chúng ta sẽ gặp lỗi => đổi qua tên trung gian rồi đổi lại theo ý người dùng
                    string dummyName = "asd" + currentFileOrFolder.name; // đây là con trung gian
                    string dummyPath = currentFileOrFolder.path + "\\" + dummyName;
                    Directory.Move(oldPath, dummyPath);
                    Directory.Move(dummyPath, newPath);
                }

                currentFileOrFolder.name = newName;
            }

            public static void FullNameNormalize(ref string inputName)
            {
                const string spliter = " ";
                FileOrFolderBus.NewCase(ref inputName, 2);
                FileOrFolderBus.UpperFirstLetter(ref inputName);

                //tach tokens
                string[] tokens;
                tokens = inputName.Split(new string[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder newName = new StringBuilder();

                //kiểm tra là đang lưu tên file hay tên folder
                int type = 0;
                int tokensSize = tokens.Count();
                if(tokens[tokensSize-1].ToString()[0]=='.')  //kiểm tra string cuối cùng có chứa dấu . ở đầu hay không
                {
                    type = 1; //là đang lưu tên file
                }

                for(int i=0;i< tokensSize;i++)
                {
                    if (type == 1 && i == tokensSize - 2)
                    {
                        newName = newName.Append(tokens[i]);
                        continue;
                    }
                    newName = newName.Append(tokens[i]);
                    newName = newName.Append(" ");
                }
                Debug.WriteLine(newName.ToString());
                inputName = newName.ToString();
            }

        }


        // mấy cái đối tượng và hàm cần thiết để chạy chương trình 

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
                NewFileNameListView.ItemsSource = filePreview;
            }
            else if (!isFile && folderPreview == null)
            {
                this.folderPreview = toolDAO.cloneToPreview(this.folderData);
                NewFolderNameListView.ItemsSource = folderPreview;

            }

        }

        // Xử lí sự kiện từ khúc này 
        /// <summary>
        /// Thêm file 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var modeSelected = ((ComboBox)sender).SelectedIndex;

            if (modeSelected != 0)
            {
                if (fileData != null)
                    fileData.Clear();

                if (modeSelected == 2)
                    fileData = toolDAO.bindingData(true);
                else if (modeSelected == 1)
                    fileData = toolDAO.bindingSingleData(true);

                fileNameListView.ItemsSource = fileData;
                pathFileListView.ItemsSource = fileData;
            }
        }

        /// <summary>
        /// Thêm folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFoulderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var modeSelected = ((ComboBox)sender).SelectedIndex;

            if (modeSelected != 0)
            {
                if (folderData != null)
                    folderData.Clear();

                if (modeSelected == 2)
                    folderData = toolDAO.bindingData(false);
                else if (modeSelected == 1)
                    folderData = toolDAO.bindingSingleData(false);
                folderNameListView.Items.Clear();
                folderNameListView.ItemsSource = folderData;
                pathFolderListView.ItemsSource = folderData;
            }
        }

        private void AddMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileData.Clear();
            this.folderData.Clear();
            this.filePreview.Clear();
            this.folderPreview.Clear();
        }

        private void StartBatchButton_Click(object sender, RoutedEventArgs e)
        {
            // nếu filePreview không có giá trị thì hiện thông báo bạn chưa đổi tên file hoặc folder
            if (filePreview == null && folderPreview == null)
            {
                MessageBox.Show("Not change name yet");
                return;
            }
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();
        
            bool mode = (tabSelected == "Rename File") ? true : false;
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

        private void UpperCaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();

            if(tabSelected == "Rename File")
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

        private void LowerCaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename File")
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

        private void UpperFirstLetterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename File")
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

        // VIẾT HOA


        private void UpperCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
           
        }

        // VIẾT THƯỜNG
       

        private void LowerCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        // VIẾT HOA CHỮ CÁI ĐẦU
       

        private void UpperFirstLetterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

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


        /// <summary>
        /// Lưu những chức năng đã chọn xuống file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 screen = new Window1();

            //mở cửa sổ mới để người dụng nhập tên preset
            if (screen.ShowDialog() == true)
            {
                string nameFile=screen.PreSetName;
                StreamWriter fileOut = new StreamWriter($"Preset/{nameFile}.txt");
                WriteToFile(fileOut);             
                presetNames.Add(nameFile);
            }
        }

        bool _isReplaceButtonClicked = false;
        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            _isReplaceButtonClicked = true;
            oldString = this.OldStringTextBox.Text;
            newString = this.NewStringTextBox.Text;
            var tabSelected = ((TabItem)this.tabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename File")
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
        bool _isUniqueNameButtonClicked =false;
        private void UniqueNameButton_Click(object sender, RoutedEventArgs e)
        {
            _isUniqueNameButtonClicked = true;
            var tabSelected = ((TabItem)this.tabControl.SelectedItem).Header.ToString();
            if (tabSelected == "Rename File")
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


      
        bool _isFullNameNormalizeButtonClicked = false;
        private void FullNameNormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            _isFullNameNormalizeButtonClicked = true;
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();

            if (tabSelected == "Rename File")
            {
                this.getEveryThingReady(true);

                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = filePreview[i];
                    FileOrFolderBus.FullNameNormalize(ref temp);
                    filePreview[i] = temp;
                }
            }
            else
            {
                this.getEveryThingReady(false);
                for (int i = 0; i < this.folderData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.FullNameNormalize(ref temp);
                    folderPreview[i] = temp;
                }
            }
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var presetName = this.PresetComboBox.SelectedItem.ToString();
            //mo file
            try
            {
                var fileIn = File.ReadAllLines($"Preset\\{presetName}.txt");
                foreach (var line in fileIn)
                {
                    ExecuteLine(line);
                }
            }
            catch(FileNotFoundException)
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
                this.FullNameNormalizeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                return;
            }

        }

        private string[] SplitLine(string line)
        {
            const string spliter = " ";
            string[] tokens;
            tokens=line.Split(new string[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }



        BindingList<string> presetNames = new BindingList<string>();// chuỗi dùng để chưa các các tên của preset
        //sau đó sẽ binding tên này vào combobox preset
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            var dir = new DirectoryInfo("Preset\\");
            var presetNameFiles = dir.GetFiles();

            foreach (var file in presetNameFiles)
            {
               
                presetNames.Add(file.Name.Replace(".txt",""));
            }

            PresetComboBox.Items.Clear();
            PresetComboBox.ItemsSource = presetNames;
        }

    }
}

