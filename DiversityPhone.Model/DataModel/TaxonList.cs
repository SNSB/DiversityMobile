using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;

namespace DiversityPhone.Model
{
    [Table]
    public class TaxonList : ReactiveObject, IEquatable<TaxonList>
    {
        public TaxonList()
        {
            TableID = InvalidTableID;
        }

        [Column(IsPrimaryKey = true)]
        public int TableID { get; set; }

        [Column]
        public string TableName { get; set; }

        [Column]
        public string TableDisplayName { get; set; }

        [Column]
        public string TaxonomicGroup { get; set; }

        [Column]
        public bool IsPublicList { get; set; }

        private bool _IsSelected;

        [Column]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { this.RaiseAndSetIfChanged(x => x.IsSelected, ref _IsSelected, value); }
        }

        public static IEnumerable<int> ValidTableIDs
        {
            get
            {
                return Enumerable.Range(0, 100);
            }
        }

        public const int InvalidTableID = -1;

        public bool Equals(TaxonList other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;
            return this.TableName == other.TableName && this.TaxonomicGroup == other.TaxonomicGroup;
        }

        public static bool operator ==(TaxonList one, TaxonList other)
        {
            return (Object.ReferenceEquals(one, null))
                ? Object.ReferenceEquals(other, null)
                : one.Equals(other);
        }

        public static bool operator !=(TaxonList one, TaxonList other)
        {
            return !(one == other);
        }

        public override int GetHashCode()
        {
            return TaxonomicGroup.GetHashCode() ^ TableName.GetHashCode() ^ ((IsPublicList) ? Int32.MaxValue : 0);
        }

        public override bool Equals(object obj)
        {
            if (obj is TaxonList)
                return Equals(obj as TaxonList);
            return false;
        }
    }
}