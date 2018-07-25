using Microsoft.Toolkit.Uwp.Notifications;
using MyList.Models;
using MyList.ViewModels;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TodoItemViewModel ViewModel = ViewModels.TodoItemViewModel.ViewModel;
        /*share variables*/
        private string shareTitle = "";
        private string shareDescription = "";
        private string shareDate = "";
        private StorageFile shareImage;

        /*DateBase*/
        private SQLiteConnection conn = App.conn;

        public MainPage()
        {
            //NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();

            // Activate Tile
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        /*Add按钮处理函数*/
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (newpage.Visibility == Visibility.Visible)
            {
                return;
            }
            Frame frame = Window.Current.Content as Frame;
            // 如果正在点了编辑再新建，则忽视编辑
            ViewModels.TodoItemViewModel.ViewModel.selectedItem = null;
            frame.Navigate(typeof(NewPage), "jump");
        }

        /*item点击处理函数*/
        private void TodoItem_itemClicked(object sender, ItemClickEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            TodoItem todo = e.ClickedItem as TodoItem;
            TodoItemViewModel.ViewModel.selectedItem = todo;
            /*窄屏情况*/
            if (newpage.Visibility == Visibility.Collapsed)
            {
                frame.Navigate(typeof(NewPage));
            }
            else
            {
                create.Content = "Update";
                title.Text = todo.title;
                detail.Text = todo.description;
                dueDatePicker.Date = todo.date;
                demo.Source = todo.imageSource;
            }
            DeleteAppBarButton.Visibility = Visibility.Visible;
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
                    imageslider.Value = 70;
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
                    imageslider.Value = 70;
                }
            }

        }

        /*cancel按钮的处理函数*/
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            /*新建模式*/
            if ((string)create.Content == "Create")
            {
                title.Text = "";
                detail.Text = "";
                dueDatePicker.Date = System.DateTime.Today;
                demo.Source = null;
                imageslider.Value = 70;
            }
            /*edit 模式*/
            else
            {
                title.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.title;
                detail.Text = ViewModels.TodoItemViewModel.ViewModel.selectedItem.description;
                dueDatePicker.Date = ViewModels.TodoItemViewModel.ViewModel.selectedItem.date;
                demo.Source = ViewModels.TodoItemViewModel.ViewModel.selectedItem.imageSource;
                imageslider.Value = 70;
            }

        }

        /*处理失败的创建*/
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

        /*处理成功创建*/
        private async void handleSuccessCreate(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            ViewModels.TodoItemViewModel.ViewModel.AddTodoItem(title.Text, detail.Text, dueDatePicker.Date, imagetoken);
            UpdateTile();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));

            var messageDialog = new Windows.UI.Popups.MessageDialog("创建成功： " + title.Text, "提示");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            await messageDialog.ShowAsync();
        }

        /*处理成功编辑*/
        private async void handleSuccessEdit(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            ViewModels.TodoItemViewModel.ViewModel.UpdateTodoItem(ViewModels.TodoItemViewModel.ViewModel.selectedItem.getID(), title.Text, detail.Text, dueDatePicker.Date, imagetoken);
            UpdateTile();
            var messageDialog = new Windows.UI.Popups.MessageDialog("编辑成功： " + title.Text, "提示");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
            await messageDialog.ShowAsync();

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        /*处理失败的编辑*/
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
        /*上传图片*/
        private async void upload_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                imagetoken = StorageApplicationPermissions.FutureAccessList.Add(file);
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                demo.Source = bi;
            }
        }

        /*删除函数*/
        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModels.TodoItemViewModel.ViewModel.selectedItem != null)
            {
                ViewModels.TodoItemViewModel.ViewModel.RemoveTodoItem(ViewModels.TodoItemViewModel.ViewModel.selectedItem.getID());
                UpdateTile();
                create.Content = "Create";
                cancel_Click(sender, e);
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
            }
        }

        /*离开页面的处理函数*/
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                
                /*分屏情况*/
                if (newpage.Visibility == Visibility.Visible)
                {
                    composite["title_right"] = title.Text;
                    composite["description_right"] = detail.Text;
                    composite["date_right"] = dueDatePicker.Date;
                    composite["button_right"] = create.Content;
                    composite["image_right"] = imagetoken;
                    composite["slider_right"] = imageslider.Value; 
                }
                else
                {
                    composite["title_right"] = "";
                    composite["description_right"] = "";
                    composite["date_right"] = DateTimeOffset.Now;
                    composite["button_right"] = "Create";
                    composite["image_right"] = imagetoken;
                    composite["slider_right"] = 70.0;
                }

                ApplicationData.Current.LocalSettings.Values["mainpage"] = composite;  
            }
        }

        /*第一次来到页面的处理函数*/
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                // If this is a new navigation, that is a fresh launch so we can
                // discard any saved state
                ApplicationData.Current.LocalSettings.Values.Remove("mainpage");
            }
            else
            {
                // Try to restore state if anym in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("mainpage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["mainpage"] as ApplicationDataCompositeValue;

                    /*恢复当前页面下，列表中第一个Item的数据*/
                    /*
                    string titleText = (string)composite["title"];
                    string detailText = (string)composite["description"];
                    DateTimeOffset dueDate = (DateTimeOffset)composite["date"];
                    bool iscomplete = (bool)composite["iscomplete"];
                    string imageString = (string)composite["imagestring"];
                    */
                    /*
                    ViewModels.TodoItemViewModel.ViewModel.AllItems[0].title = titleText;
                    ViewModels.TodoItemViewModel.ViewModel.AllItems[0].description = detailText;
                    ViewModels.TodoItemViewModel.ViewModel.AllItems[0].date = dueDate;
                    ViewModels.TodoItemViewModel.ViewModel.AllItems[0].imageString = imageString;
                    ViewModels.TodoItemViewModel.ViewModel.AllItems[0].completed = iscomplete;
                    */
                    /*恢复右侧数据*/
                    if (newpage.Visibility == Visibility.Visible)
                    {
                        title.Text = (string)composite["title_right"];
                        detail.Text = (string)composite["description_right"];
                        dueDatePicker.Date = (DateTimeOffset)composite["date_right"];
                        create.Content = (string)composite["button_right"];
                        imageslider.Value = (double)composite["slider_right"];
                        string image = (string)composite["image_right"];

                        TodoItem todo = new TodoItem(title.Text, detail.Text, dueDatePicker.Date, image,"");
                        ViewModels.TodoItemViewModel.ViewModel.selectedItem = todo;

                        /*处理图片*/
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
                        
                    }

                    // Remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("mainpage");
                }
            }

            /*Tile*/
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            UpdateTile();
            /*添加Share委托*/
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        /*返回磁贴内容*/
        private TileContent getNewTileContent(string title, string description, string date, string src)
        {
            return new TileContent()
            {
                // 展示模型的定义
                Visual = new TileVisual()
                {
                    // 小型
                    TileSmall = new TileBinding()
                    {
                        // 主要内容
                        Content = new TileBindingContentAdaptive()
                        {
                            // 背景
                            BackgroundImage = new TileBackgroundImage()
                            {                             
                                 Source = src
                            },
                            // 成员
                            Children =
                            {
                                // 标题
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                    // 中型
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "MyList",

                        //主要内容
                        Content = new TileBindingContentAdaptive()
                        {
                            // 背景
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = src
                            },
                            // 成员
                            Children =
                            {
                                // 标题
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintWrap = true
                                },
                                // 详情
                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,

                                },
                                // 日期
                                new AdaptiveText()
                                {
                                    Text = date,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,

                                }
                            }
                        }
                    },
                    // 大型
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "MyList",
                        // 内容
                        Content = new TileBindingContentAdaptive()
                        {
                            // 背景
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = src
                            },
                            // 成员
                            Children =
                            {
                                // 标题
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle,
                                    HintWrap = true
                                },
                                // 详情
                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                // 日期
                                new AdaptiveText()
                                {
                                    Text = date,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }
                }
            };
        }

        /*为列表中的每个Item创建一个磁贴*/
        private async void UpdateTile()
        {
            // Activate Notification Queue
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            int num = 0;
            foreach (TodoItem temp in TodoItemViewModel.ViewModel.AllItems)
            {
                // dateFormat: 2018-01-01
                string dateString = "" + temp.date.Year + "/" + temp.date.Month + "/" + temp.date.Day;
                string image = temp.imageString;
                string src;
                /*处理图片*/
                if (image != null && image != "")
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(image);
                    src = file.Path;
                }
                else
                {
                    src = "Assets/background.png";
                }

                TileContent content = getNewTileContent(temp.title, temp.description, dateString, src);
                var notification = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
                num++;
                if (num == 5)
                    break;
            }
        }

        /*分享响应函数*/
        void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var req = args.Request;

            req.Data.Properties.Title = shareTitle;
            req.Data.Properties.Description = shareDescription;
            req.Data.SetText(shareDescription + "\nDate: " +shareDate);

            req.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(shareImage));
            req.GetDeferral().Complete();
        }

        /*处理Share按钮的函数*/
        private async void Share_Click(object sender, RoutedEventArgs e)
        {
            /*获取当前Item*/
            var dc = (sender as FrameworkElement).DataContext;
            var item = (listview.ContainerFromItem(dc) as ListViewItem).Content as TodoItem;

            /*存储分享的信息*/
            shareTitle = item.title;
            shareDescription = item.description;
            shareDate = "" + item.date.Year + "/" + item.date.Month + "/" + item.date.Day;
            string temp = item.imageString;
            /*处理默认图片的情况*/
            if (temp == "")
            {
                shareImage = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/background.png"));

            }
            else
            {
                shareImage = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(temp);
            }
            
            /*显示分享框*/
            DataTransferManager.ShowShareUI();
        }

        /*处理Search按钮的函数*/
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string searchInfo = SearchBox.Text;
            if (searchInfo == "")
                return;
            StringBuilder showStr = new StringBuilder();
            try
            {
                var sql = @"SELECT Title, Description, Date FROM TodoItem WHERE Title LIKE ? OR Description LIKE ? OR Date LIKE ?";
                using (var statement = conn.Prepare(sql))
                {
                    statement.Bind(1, "%" + searchInfo + "%");
                    statement.Bind(2, "%" + searchInfo + "%");
                    statement.Bind(3, "%" + searchInfo + "%");

                    while (SQLiteResult.ROW == statement.Step())
                    {
                        showStr.Append("Title: ");
                        showStr.Append((string)statement[0]);
                        showStr.Append("  Description: ");
                        showStr.Append((string)statement[1]);
                        showStr.Append("   Date: ");
                        showStr.Append((string)statement[2]);
                        showStr.Append("\n");
                    }

                    if (showStr.Equals(new StringBuilder()))
                    {
                        showStr.Append("Not Found!\n");
                    }
                        
                    await new MessageDialog(showStr.ToString()).ShowAsync();
                    SearchBox.Text = "";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
