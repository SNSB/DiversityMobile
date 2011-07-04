using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Xaml;
using SterlingDemo.Model;
using System.Reactive.Linq;



namespace WindowsPhoneSterling
{
    public class MainViewModel : ReactiveObject
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<StringISO> Items { get; private set; }
        

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        private string _InsertTitle = "Test";
        
        public string InsertTitle 
        { 
            get
            {
                return _InsertTitle;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.InsertTitle,value);
            }
        }

        private int _InsertCount = 1;
        public int InsertCount
        {
            get
            {
                return _InsertCount;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.InsertCount, value);
            }
        }

        public ICommand Insert { get; protected set; }


        public MainViewModel()
        {
            this.Items = new ObservableCollection<StringISO>();

            var titleValid = this.ObservableForProperty(vm => vm.InsertTitle)
                .Select(title => !string.IsNullOrEmpty(title.Value));
            var lineCountValid = this.ObservableForProperty(vm => vm.InsertCount)
                .Select(count => count.Value > 0);

            var canInsert = titleValid.CombineLatest(lineCountValid, (title, count) => title && count);
            
                

            var insert = new ReactiveCommand(canInsert);
            insert.Subscribe(_ => insertLines(InsertTitle,InsertCount));
            Insert = insert;
        }

        private void insertLines(string title, int count)
        {
            var newLines = new List<StringISO>();
            for (int i = 0; i < count; i++)
                newLines.Add(new StringISO()
                {
                    Value = string.Format("{0} #{1}", title, count),
                    GUID = Guid.NewGuid()
                });

            foreach (var line in newLines)
                App.Database.Save(line);
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            bool hasKeys = false;
            foreach (var item in App.Database.Query<StringISO, Guid>())
            {
                hasKeys = true;
                break;
            }

            if (!hasKeys)
            {
                insertLines("TestLine", 10);
            }

            foreach (var item in App.Database.Query<StringISO, Guid>())
            {
                Items.Add(item.LazyValue.Value);
            }

            this.IsDataLoaded = true;
        }

        




       
    }
}