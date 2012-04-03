using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.Model;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : EditElementPageVMBase<Map>
    {
          #region Properties


        #region Properties
        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        #endregion

        private BitmapImage _savedImage;

        public BitmapImage SavedImage
        {
            get
            {
                return _savedImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SavedImage, ref _savedImage, value);
            }
        }
        #endregion

        public ViewMapVM()            
        {


        }

        private void LoadImage(Map map)
        {


            byte[] data;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream isfs = isf.OpenFile(map.Uri, FileMode.Open, FileAccess.Read))
                {
                    data = new byte[isfs.Length];
                    isfs.Read(data, 0, data.Length);
                    isfs.Close();
                }

            }
            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            SavedImage = bi;
         
        }
      


        protected override void UpdateModel()
        {
            
        }

      

        protected override Map ModelFromState(Services.PageState s)
        {
            if (s.Context != null)
            {
                int id;
                Map map = null;
                if (int.TryParse(s.Context, out id))
                {
                    map= Storage.getMapByID(id);
                }
               
                if (map != null)
                    LoadImage(map);
                return map;
            }
            return null;
        }

        public override void SaveState()
        {
            base.SaveState();

        }


        protected override IObservable<bool> CanSave()
        {
            return Observable.Return(false);
        }

        protected override ElementVMBase<Map> ViewModelFromModel(Map model)
        {
            return new MapVM(Messenger, model, DiversityPhone.Services.Page.Current);
        }
    }
    
}
