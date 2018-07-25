using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList.Models
{
    class TodoItem : INotifyPropertyChanged
    {
        private string id { get; }

        public string getID() { return id; }

        public string title { set; get; }

        public string description { set; get; }

        private bool completeValue { set; get; }
        public System.Nullable<bool> completed {
            set {
                if (value != completeValue)
                {
                    this.completeValue = value.Value;
                    NotifyPropertyChanged();
                    /*db*/
                    var conn = App.conn;
                    try
                    {
                        string sql = @"UPDATE TodoItem SET IsComplete = ? WHERE Id = ?";
                        using (var updateItem = conn.Prepare(sql))
                        {
                            string iscomplete;
                            if (completeValue == true)
                            {
                                iscomplete = "true";

                            }
                            else
                            {
                                iscomplete = "false";
                            }
                            updateItem.Bind(1, iscomplete);
                            updateItem.Bind(2, this.id);
                            updateItem.Step();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle Exception
                    }
                }
            }
            get {
                return (System.Nullable<bool>)this.completeValue;
            } }

        public DateTimeOffset date { set; get; }

        private ImageSource imageSourceTmp { set; get; }
        public ImageSource imageSource {
            set {
                if (value != this.imageSourceTmp)
                {
                    this.imageSourceTmp = value;
                    NotifyPropertyChanged();
                    
                }
            }
            get {
                return imageSourceTmp;
            } }

        private string imagetmp { set; get; }
        public string imageString
        {
            set
            {
                if (value != imagetmp)
                {
                    this.imagetmp = value;
                    ReadImage();
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return imagetmp;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public TodoItem(string title, string description, DateTimeOffset date, string imageString, string id, bool iscomplete = false)
        {
            if (id == "")
            {
                this.id = Guid.NewGuid().ToString(); // 生成id
            }
            else
            {
                this.id = id;
            }
            this.title = title;
            this.description = description;
            this.completed = iscomplete; 
            this.date = date;
            if (imageString == null)
            {
                this.imageString = "";
            }
            else
            {
                this.imageString = imageString;
            }
            
        }

        /*string to ImageSource*/
        private async void ReadImage()
        {
            if (imageString == null || imageString == "")
            {
                imageSource = new BitmapImage(new Uri("ms-appx:Assets/background.png"));
            }
            else
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(imageString);
                if (file != null)
                {
                    IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    await bi.SetSourceAsync(ir);
                    imageSource = bi;
                }
            }
            
        }
        
    }
    
    
}
