using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using SQLitePCL;
using System.Diagnostics;

namespace MyList.ViewModels
{
    class TodoItemViewModel
    {
        /*db*/
        private SQLiteConnection conn = App.conn;
        public Models.TodoItem selectedItem;

        private static TodoItemViewModel todoItemViewModel = new TodoItemViewModel();
        public static TodoItemViewModel ViewModel {
            get {
                if (todoItemViewModel == null)
                {
                    todoItemViewModel = new TodoItemViewModel();
                }
                return todoItemViewModel;
            }
            set
            {
                todoItemViewModel = value;
            }
        }

        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        public ObservableCollection<Models.TodoItem> AllItems { get { return this.allItems; } }
        
        /*构造函数*/
        public TodoItemViewModel()
        {
            /*将db中全部数据添加到应用列表*/
            var sql = "SELECT * FROM TodoItem";
            try
            {
                using (var statement = conn.Prepare(sql))
                {
                    while (SQLiteResult.ROW == statement.Step())
                    {
                        string dateString = (string)statement[3];
                        dateString = dateString.Substring(0, dateString.IndexOf(' '));
                        DateTime dateTemp = new DateTime(int.Parse(dateString.Split('/')[0]), int.Parse(dateString.Split('/')[1]), int.Parse(dateString.Split('/')[2]));
                        DateTimeOffset date = DateTime.SpecifyKind(dateTemp, DateTimeKind.Utc);
                        //this.AddTodoItem((string)statement[1], (string)statement[2], date, null);
                        bool iscomplete;
                        if ((string)statement[4] == "true")
                        {
                            iscomplete = true;
                        }
                        else
                        {
                            iscomplete = false;
                        }
                        this.allItems.Add(new Models.TodoItem((string)statement[1], (string)statement[2], date, (string)statement[5],(string)statement[0], iscomplete));
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        /*添加Item的函数*/
        public void AddTodoItem(string title, string description, DateTimeOffset date, string imageString, bool iscomplete = false)
        {
            this.allItems.Add(new Models.TodoItem(title, description, date, imageString,"" ,iscomplete));
            string id = ViewModel.AllItems[AllItems.Count-1].getID();
            /*向db中插入一个Item*/
            try
            {
                using (var newItem = conn.Prepare("INSERT INTO TodoItem (Id, Title, Description, Date, IsComplete, Image) VALUES (?, ?, ?, ?, ?, ?)"))
                {
                    newItem.Bind(1, id);
                    newItem.Bind(2, title);
                    newItem.Bind(3, description);
                    newItem.Bind(4, date.Date.ToString());
                    string iscompleteTemp;
                    if (iscomplete == true)
                    {
                        iscompleteTemp = "true";
                    }
                    else
                    {
                        iscompleteTemp = "false";
                    }
                    newItem.Bind(5, iscompleteTemp);
                    newItem.Bind(6, imageString);
                    newItem.Step();
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        /*删除指定Item的函数*/
        public void RemoveTodoItem(string id)
        {
            /*根据id在db删除指定Item*/
            using (var statement = conn.Prepare("DELETE FROM TodoItem WHERE Id = ?"))
            {
                statement.Bind(1, selectedItem.getID());
                statement.Step();
            }
            this.allItems.Remove(selectedItem);
            this.selectedItem = null;
        }

        /*更新指定Item的函数*/
        public void UpdateTodoItem(string id, string title, string description, DateTimeOffset date, string imageString)
        {
            this.selectedItem.title = title;
            this.selectedItem.description = description;
            this.selectedItem.date = date;
            this.selectedItem.imageString = imageString;
            this.selectedItem = null;

            /*根据id更新db中的某个Item的信息，其中IsComplete在ListItem中更新*/
            try
            {
                string sql = @"UPDATE TodoItem SET Title = ?, Description = ?, Date = ?, Image = ? WHERE Id = ?";
                using (var updateItem = conn.Prepare(sql))
                {
                    updateItem.Bind(1, title);
                    updateItem.Bind(2, description);
                    updateItem.Bind(3, date.Date.ToString());
                    updateItem.Bind(4, imageString);
                    updateItem.Bind(5, id);
                    updateItem.Step();
                }
            }
            catch (Exception ex)
            {
                // Handle Exception
            }
        }
    }
    
}
