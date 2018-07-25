using MyList.Models;
using MyList.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        TodoItemViewModel ViewModel = ViewModels.TodoItemViewModel.ViewModel;
        public NewPage()
        {
            this.InitializeComponent();
        }

        /*create按钮的处理函数*/
        private void create_Click(object sender, RoutedEventArgs e)
        {
            /*新建模式*/
            if ((string)create.Content == "Create")
            {
                bool isValid = true;
                if (title.Text == "")
                {
                    isValid = false;
                    title.Focus(FocusState.Pointer);
                }
                else if (detail.Text == "")
                {
                    isValid = false;
                    detail.Focus(FocusState.Pointer);
                }
                else if (dueDatePicker.Date < System.DateTime.Today)
                {
                    isValid = false;
                }
                else if (demo.Source == null)
                {
                    isValid = false;
                }
                if (!isValid)
                {
                    handleFailCreate(sender, e);
                }
                else
                {
                    handleSuccessCreate(sender, e);
                    title.Text = "";
                    detail.Text = "";
                    dueDatePicker.Date = System.DateTime.Today;
                    demo.Source = null;
                }
            }
            /*edit 模式*/
            else if ((string)create.Content == "Update")
            {
                bool isValid = true;
                if (title.Text == "")
                {
                    isValid = false;
                    title.Focus(FocusState.Pointer);
                }
                else if (detail.Text == "")
                {
                    isValid = false;
                    detail.Focus(FocusState.Pointer);
                }
                else if (dueDatePicker.Date < System.DateTime.Today)
                {
                    isValid = false;
                }
                else if (demo.Source == null)
                {
                    isValid = false;
                }
                if (!isValid)
                {
                    handleFailEdit(sender, e);
                }
                else
                {
                    handleSuccessEdit(sender, e);
                    title.Text = "";
                    detail.Text = "";
                    dueDatePicker.Date = System.DateTime.Today;
                    demo.Source = null;
                }
            }
        }

        /*cancel按钮处理函数*/
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            /*新建模式*/
            if ((string)create.Content == "Create")
            {
                title.Text = "";
                detail.Text = "";
                dueDatePicker.Date = System.DateTime.Today;
                demo.Source = null;
            }
            /*edit 模式*/
            else
            {
                title.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.title;
                detail.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.description;
                dueDatePicker.Date = ViewModels.TodoItemViewModel.ViewModel.selectedItem.date;
                demo.Source = ViewModels.TodoItemViewModel.ViewModel.selectedItem.imageSource;
            }
            
        }

        /*创建失败的处理函数*/
        private async void handleFailCreate(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            string str = "您的输入有误， 请重新输入\nError Message: ";

            if (title.Text == "")
            {
                str += "Title can't be empty!\n";
            }
            if (detail.Text == "")
            {
                str += "Detail can't be empty!\n";
            }
            if (dueDatePicker.Date < System.DateTime.Today)
            {
                str += "The due date has passed!\n";
            }
            if (demo.Source == null)
            {
                str += "The picture can't be empty!\n";
            }
            var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
            await messageDialog.ShowAsync();
        }

        /*成功创建的处理函数*/
        private async void handleSuccessCreate(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            ViewModels.TodoItemViewModel.ViewModel.AddTodoItem(title.Text, detail.Text, dueDatePicker.Date, imagetoken);

            Frame rootFrame = Window.Current.Content as Frame;
            

            var messageDialog = new Windows.UI.Popups.MessageDialog("创建成功： " + title.Text, "提示");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            await messageDialog.ShowAsync();
            rootFrame.GoBack();
        }

        /*成功编辑的处理函数*/
        private async void handleSuccessEdit(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            ViewModels.TodoItemViewModel.ViewModel.UpdateTodoItem(ViewModels.TodoItemViewModel.ViewModel.selectedItem.getID(), title.Text, detail.Text, dueDatePicker.Date, imagetoken);
            create.Content = "Create";
            var messageDialog = new Windows.UI.Popups.MessageDialog("编辑成功： " + title.Text, "提示");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            await messageDialog.ShowAsync();

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }

        /*编辑失败的处理函数*/
        private async void handleFailEdit(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            string str = "您的输入有误， 请重新输入\nError Message: ";

            if (title.Text == "")
            {
                str += "Title can't be empty!\n";
            }
            if (detail.Text == "")
            {
                str += "Detail can't be empty!\n";
            }
            if (dueDatePicker.Date < System.DateTime.Today)
            {
                str += "The due date has passed!\n";
            }
            if (demo.Source == null)
            {
                str += "The picture can't be empty!\n";
            }
            var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
            await messageDialog.ShowAsync();
        }
        string imagetoken;
        /*上传图片的处理函数*/
        private async void upload_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSingleFileAsync();
            
            

            //StorageFile asd = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);

            if (file != null)
            {
                imagetoken = StorageApplicationPermissions.FutureAccessList.Add(file);
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                demo.Source = bi;
            }
        }
        
        /*删除的处理函数*/
        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModels.TodoItemViewModel.ViewModel.selectedItem != null)
            {
                ViewModels.TodoItemViewModel.ViewModel.RemoveTodoItem(ViewModels.TodoItemViewModel.ViewModel.selectedItem.getID());
                create.Content = "Create";
                cancel_Click(sender, e);
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
            }
        }

        /*离开页面的处理函数*/
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            ViewModels.TodoItemViewModel.ViewModel.selectedItem = null;
            if (suspending)
            {
                /*保存当前页面下，正在创建或修改的Item的数据*/
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["description"] = detail.Text;
                composite["date"] = dueDatePicker.Date;
                composite["image"] = imagetoken;
                composite["button"] = create.Content;
                composite["slider"] = imageslider.Value;
                ApplicationData.Current.LocalSettings.Values["newpage"] = composite;
            }
        }

        /*第一次来到页面的处理函数*/
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.NavigationMode == NavigationMode.New)
            {
                // If this is a new navigation, that is a fresh launch so we can
                // discard any saved state
                
                if (ViewModels.TodoItemViewModel.ViewModel.selectedItem != null)
                {
                    create.Content = "Update";
                    DeleteAppBarButton.Visibility = Visibility.Visible;
                    title.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.title;
                    detail.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.description;
                    dueDatePicker.Date = ViewModels.TodoItemViewModel.ViewModel.selectedItem.date;
                    demo.Source = ViewModels.TodoItemViewModel.ViewModel.selectedItem.imageSource;
                }
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                
            }
            else
            {
                // Try to restore state if anym in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
                {
                    /*恢复当前页面下，正在创建或修改的Item的数据*/
                    var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                    //string itemID = (string)composite["id"];
                    string titleText = (string)composite["title"];
                    string detailText = (string)composite["description"];
                    DateTimeOffset dueDate = (DateTimeOffset)composite["date"];
                    string image = (string)composite["image"];
                    string createContent = (string)composite["button"];

                    TodoItem todo = new TodoItem(titleText, detailText, dueDate, null, "");
                    ViewModels.TodoItemViewModel.ViewModel.selectedItem = todo;
                    
                    title.Text = titleText;
                    detail.Text = detailText;
                    dueDatePicker.Date = dueDate;
                    create.Content = createContent;
                    imageslider.Value = (double)composite["slider"];
                    if (image != null)
                    {
                        var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(image);
                        if (file != null)
                        {
                            IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                            BitmapImage bi = new BitmapImage();
                            await bi.SetSourceAsync(ir);
                            demo.Source = bi;
                        }
                    }
                    
                    // Remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                }
                else
                {
                    title.Text = "Invalid condition";
                }
            }
        }
    }
}
