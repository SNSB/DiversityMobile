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
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : EditElementPageVMBase<Map>
    {

        #region Services

        IMapStorageService Maps;

        #endregion

        #region Properties
        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value); }
        }
       

        private BitmapImage _mapImage;

        public BitmapImage MapImage
        {
            get
            {
                return _mapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _mapImage, value);
            }
        }
        
        public int? Height
        {
            get { return _mapImage.PixelHeight; }
        }

        public int? Width
        {
            get { return _mapImage.PixelWidth; }
        }
        #endregion

        public ViewMapVM(IMapStorageService maps)            
        {
            Maps = maps;
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
            MapImage = bi;  
        }
      


        protected override void UpdateModel()
        {
            
        }

      

        protected override Map ModelFromState(Services.PageState s)
        {
            if (s.Context != null)
            {
                Map map=null;
                try
                {
                    map = Maps.getMapbyServerKey(s.Context);

                    if (map != null)
                    {
                        LoadImage(map);
                        Description = map.Description;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    map = null;
                }
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
