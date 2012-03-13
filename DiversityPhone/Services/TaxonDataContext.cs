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
using System.Data.Linq;
using DiversityPhone.Model;


namespace DiversityPhone.Services
{
    public class TaxonDataContext : DataContext
    {
        private static string connStr = "isostore:/taxonDB{0}.sdf";

        public TaxonDataContext(int idx)
            :base(String.Format(connStr, idx))
        {
            if (!this.DatabaseExists())
                this.CreateDatabase();
        }
        public Table<TaxonName> TaxonNames;
    }
}
