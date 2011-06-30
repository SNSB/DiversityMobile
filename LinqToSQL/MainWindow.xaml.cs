using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LinqToSQL
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataClasses1DataContext db = new DataClasses1DataContext();

        public IEnumerable<string> IUnits { get; private set; }


        public MainWindow()
        {
            InitializeComponent();
            IUnits = (from iu in db.IdentificationUnit where iu.UnitDescription != "" select iu.UnitDescription).Take(10);
        }
    }
}
