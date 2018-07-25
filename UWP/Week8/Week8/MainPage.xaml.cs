using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Week8
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        MediaTimelineController mediaTimeLineController;
        TimeSpan duration;
        MediaSource mediaSource;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /*进度条响应函数*/
        private void timeslider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (mediaTimeLineController == null)
            {
                return;
            }

            /*将用户在进度条上的数据同步到时间线控制器中*/
            TimeSpan value = TimeSpan.FromSeconds(slider.Value);
            mediaPlayer.TimelineController.Position = value;
        }
        /*播放和暂停按钮的响应函数*/
        private void play_Click(object sender, RoutedEventArgs e)
        {
            /*如果时间线控制器不存在，则说明没有影音资源*/
            if (mediaTimeLineController == null)
            {
                var message = new MessageDialog("Please open a file").ShowAsync();
            }
            /*若没有在播放*/
            else if (mediaTimeLineController.State == MediaTimelineControllerState.Paused)
            {
                play.Icon = new SymbolIcon(Symbol.Pause);
                play.Label = "Pause";
                /*若时间不在起点，即是处于暂停状态*/
                if (mediaTimeLineController.Position != TimeSpan.Zero)
                {
                    mediaTimeLineController.Resume();
                    EllStoryboard.Resume();
                }
                /*若时间在起点*/
                else
                {
                    mediaTimeLineController.Start();
                    EllStoryboard.Begin();
                }
            }
            /*若视频处于进行状态，则暂停*/
            else
            {
                play.Icon = new SymbolIcon(Symbol.Play);
                play.Label = "Play";
                mediaTimeLineController.Pause();
                EllStoryboard.Pause();
            }
        }

        /*Stop按钮的响应函数*/
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (mediaTimeLineController == null)
            {
                return;
            }
            if (mediaTimeLineController.State == MediaTimelineControllerState.Running)
            {
                mediaTimeLineController.Pause();
                mediaTimeLineController.Position = TimeSpan.Zero;
                play.Icon = new SymbolIcon(Symbol.Play);
                play.Label = "Play";
                EllStoryboard.Stop();
            }
            else
            {
                mediaTimeLineController.Position = TimeSpan.Zero;
            }
        }

        /*响应音量滑条变动的函数*/
        private void volumnslider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
                double value = slider.Value / 100;
            /*将音量数据同步到mediaPlayer中*/
                mediaPlayer.Volume = value;
        }

        /*点击音量按钮的响应函数*/
        private void volumn_Click(object sender, RoutedEventArgs e)
        {
            /*将volume的浮动控件展示出来*/
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }

        /*全屏按钮的响应函数*/
        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            /*如果当前是全屏状态，则退出全屏*/
            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
                command.Visibility = Visibility.Visible;
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            }
            /*如果当前不是全屏状态，则进入全屏*/
            else
            {
                view.TryEnterFullScreenMode();
                command.Visibility = Visibility.Visible;
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            }
        }

        /*从本地读入文件*/
        private async void opensource_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");

            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            var file = await openPicker.PickSingleFileAsync();
            
            /*获取音乐播放文件的缩略图*/
            var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView);
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
            randomAccessStream.Seek(0);
            bitmapImage.SetSource(randomAccessStream);
            imagebrush.ImageSource = bitmapImage;

            // mediaPlayer is a MediaElement define in XAML
            if (file != null)
            {
                mediaSource = MediaSource.CreateFromStorageFile(file);
                /*如果是播放音乐，则显示圆形封面*/
                if(file.FileType == ".mp3" || file.FileType == ".wma")
                {
                    Picture.Visibility = Visibility.Visible;
                }
                /*否则，不显示封面*/
                else
                {
                    Picture.Visibility = Visibility.Collapsed;
                }

                // 添加对资源添加完成的监听
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;

                /*将影音资源放入mediaPlayer中*/
                mediaPlayer.Source = mediaSource;
                mediaPlayer.TimelineController = null;

                /*将时间线控制器放入mediaPlayer中*/
                mediaplayer.SetMediaPlayer(mediaPlayer);

                /*对时间线控制器添加变化的监听*/
                mediaTimeLineController = new MediaTimelineController();
                mediaPlayer.TimelineController = mediaTimeLineController;
                mediaTimeLineController.PositionChanged += mediaTimeLineController_PositionChanged;
               
            }
        }

        
        /*处理添加影音资源后的响应函数*/
        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            /*解析资源的时间长度*/
            duration = sender.Duration.GetValueOrDefault();

            /*将时间同步给timeslider*/
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timeslider.Minimum = 0;
                timeslider.Maximum = duration.TotalSeconds;
                timeslider.StepFrequency = 1;
            });
        }

        /*处理时间控制线每秒的变化的响应函数*/
        private async void mediaTimeLineController_PositionChanged(MediaTimelineController sender, object args)
        {
            if (duration != TimeSpan.Zero)
            {
                /*将当前值同步给timeslider*/
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    timeslider.Value = sender.Position.TotalSeconds;
                });
            }
        }

        /*鼠标移入控件区域*/
        private void controls_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (mediaTimeLineController == null)
            {
                return;
            }
            controls.Opacity = 80;
        }

        /*鼠标移出控件区域*/
        private void controls_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (mediaTimeLineController == null)
            {
                return;
            }
            controls.Opacity = 0;
        }
    }
}
