using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    inputName = inputName.ToUpper();
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

            
        }


        // mấy cái đối tượng và hàm cần thiết để chạy chương trình 

        FileOrFolderDAO toolDAO = new FileOrFolderDAO();
        ObservableCollection<FileOrFolder> fileData = null;
        ObservableCollection<FileOrFolder> folderData = null;
        ObservableCollection<string> filePreview = null;
        ObservableCollection<string> folderPreview = null;


        private void getEveryThingReady(bool isFile)
        {
            if (isFile && filePreview == null)
                this.filePreview = toolDAO.cloneToPreview(this.fileData);
            else if (!isFile && folderPreview == null)
                this.folderPreview = toolDAO.cloneToPreview(this.folderData);
               
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

        //Hàm cũ của tứng
        /*
        /// <summary>
        /// Viết hoa mỗi chữ cái đầu của mỗi từ
        /// </summary>
        /// <param name="oldName"></param>
        /// <returns></returns>
        private string UpperFirstLetter(string oldName)
        {
            string newName = null;
            StringBuilder oldNameStringBuilder = new StringBuilder(oldName);
            oldNameStringBuilder[0] = char.ToUpper(oldNameStringBuilder[0]);

            for (int i = 1; i < oldName.Length; i++)
            {
                if (oldName[i] != ' ' && oldName[i - 1] == ' ')
                {
                    oldNameStringBuilder[i] = char.ToUpper(oldNameStringBuilder[i]);
                }
                else
                {
                    oldNameStringBuilder[i] = char.ToLower(oldNameStringBuilder[i]);

                }
            }
            newName = oldNameStringBuilder.ToString();
            return newName;

        }
        private ObservableCollection<FileOrFolder> NewCase(ObservableCollection<FileOrFolder> input, int type)
        {
            ObservableCollection<FileOrFolder> result = new ObservableCollection<FileOrFolder>();
            IEnumerable<FileOrFolder> array = input;
            string newName;
            foreach (var i in array)
            {
                if (type == 1)
                    newName = i.name.ToUpper();
                else if (type == 2)
                    newName = i.name.ToLower();
                else newName = UpperFirstLetter(i.name);
                result.Add(new FileOrFolder() { name = newName, path = i.path });
            }


            return result;
        }


        ObservableCollection<FileOrFolder> output;
        ObservableCollection<FileOrFolder> previousOutput = new ObservableCollection<FileOrFolder>();

        /// <summary>
        /// Hàm lưu tên file
        /// </summary>
        /// <param name="output"></param>
        void SaveFile(ObservableCollection<FileOrFolder> output, bool isFileRename)
        {
            // lấy tên file từ trong output ra xong rồi lưu lại tên mới
            IEnumerable<FileOrFolder> oldNameArray;
            if (isFileRename)
            {
                oldNameArray = fileData;
            }
            else
            {
                oldNameArray = folderData;
            }

            IEnumerable<FileOrFolder> newNameArray = output;
            string sourceFileName;
            string destFileName;
            int arraySize = newNameArray.Count();
            for (int i = 0; i < arraySize; i++)
            {
                if (arraySize == 1)
                {
                    sourceFileName = oldNameArray.ElementAt(i).path + oldNameArray.ElementAt(i).name;
                    destFileName = newNameArray.ElementAt(i).path + newNameArray.ElementAt(i).name;
                }
                else
                {
                    sourceFileName = oldNameArray.ElementAt(i).path + "\\" + oldNameArray.ElementAt(i).name;
                    destFileName = newNameArray.ElementAt(i).path + "\\" + newNameArray.ElementAt(i).name;
                }
                if (isFileRename)
                {
                    File.Move(sourceFileName, destFileName);
                }
                else
                {
                    Debug.WriteLine($"{sourceFileName}{destFileName}");
                    Directory.Move(sourceFileName, destFileName);
                }
            }
            MessageBox.Show("Save Successfully");

        }
        */
        //Hết hàm cũ của tứng
        
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileData.Clear();
            this.folderData.Clear();
            this.filePreview.Clear();
            this.folderPreview.Clear();
        }

        private void StartBatchButton_Click(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();
            
            bool mode = (tabSelected == "Rename File") ? true : false;

            for (int i = 0; i < this.fileData.Count; i++)
            {
               var temp = fileData[i];
               FileOrFolderBus.applyChange(ref temp, filePreview[i], mode);
               fileData[i] = temp;
            }
            
        }

        private void UpperCaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var tabSelected = ((TabItem)(this.tabControl.SelectedItem)).Header.ToString();

            if(tabSelected == "Rename File")
            {
                
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
                for (int i = 0; i < this.fileData.Count; i++)
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
                for (int i = 0; i < this.fileData.Count; i++)
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
                for (int i = 0; i < this.fileData.Count; i++)
                {
                    var temp = folderPreview[i];
                    FileOrFolderBus.UpperFirstLetter(ref temp);
                    folderPreview[i] = temp;
                }
            }
        }
        //
        //==================================CÁC HÀNH ĐỘNG VỚI TỆP TIN================================

        // VIẾT HOA


        private void UpperCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //output = previousOutput;
            //if (fileData != null)
            //{
            //    NewFileNameListView.ItemsSource = output;
            //}
            //else
            //    NewFolderNameListView.ItemsSource = output;
        }

        // VIẾT THƯỜNG
       

        private void LowerCaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        // VIẾT HOA CHỮ CÁI ĐẦU
       

        private void UpperFirstLetterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //trashhhhhhhhhhhhhhhhhhhhhhhh
        /*
CheckBox upperCaseCheckBox;
/// <summary>
/// tạo ra khung khi chọn method
/// </summary>
/// <returns></returns>
private TreeViewItem createChildTreeView()
{
TreeViewItem item = new TreeViewItem();
item.IsExpanded = true;
StackPanel stack = new StackPanel();

//child stackpanel
StackPanel childStack = new StackPanel();
childStack.Orientation = Orientation.Horizontal;
//checkbox
upperCaseCheckBox = new CheckBox();
upperCaseCheckBox.Name = "UpperCaseCheckBox";
upperCaseCheckBox.VerticalAlignment = VerticalAlignment.Center;
childStack.Children.Add(upperCaseCheckBox);

//Label
Label upperCaseLabel = new Label();
upperCaseLabel.Content = "Upper All Elements";
childStack.Children.Add(upperCaseLabel);

//add to father stack
stack.Children.Add(childStack);
item.Header = stack;
return item;
}

private void AddMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
var item = (ComboBox)sender;
var index = item.SelectedIndex;
if (index == 1)
{
TreeViewItem treeItem = new TreeViewItem();
treeItem.Header = "New Case";
treeItem.Items.Add(createChildTreeView());

//MethodListView.Items.Add("New case");
MethodTreeView.Items.Add(treeItem);
}
else if (index == 2)
{

TreeViewItem treeItem = new TreeViewItem();
treeItem.Header = "Unique Name";
treeItem.Items.Add(new TreeViewItem());
//MethodListView.Items.Add("Unique Name");
MethodTreeView.Items.Add(treeItem);
}
}
private void ClearButton_Click(object sender, RoutedEventArgs e)
{

}

private ObservableCollection<FileOrFolder> UpperCase(ObservableCollection<FileOrFolder>input)
{
ObservableCollection<FileOrFolder> result =null;
IEnumerable<FileOrFolder> array = input.Where(ten => ten.name !=null);
foreach(var i in array)
{
i.name = i.name.ToUpper();
result.Add(new FileOrFolder() { name = i.name, path = null });
}
MessageBox.Show(result.ToString());

return result;
}

//private void uppperCaseCheckBox_Check(object sender, RoutedEventArgs e)
//{
//    UpperCase
//    MessageBox.Show("dit me m");
//}
void Output()
{
Debug.WriteLine("true");
ObservableCollection<FileOrFolder> output = fileData;
if (output == null)
{
output = folderData;
}
if (upperCaseCheckBox.IsChecked ?? false)
{

output = UpperCase(output);
}
NewFileNameListView.ItemsSource = output;
}
*/

    }

}

