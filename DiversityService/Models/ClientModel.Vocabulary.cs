using PetaPoco;
using System;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    public class Analysis
    {
        //Read-Only
        [Column("AnalysisID")]
        public int AnalysisID { get; set; }

        [Column("AnalysisParentID")]
        [IgnoreDataMember]
        public int AnalysisParentID { get; set; }

        [Column("DisplayText")]
        public string DisplayText { get; set; }
        [Column("Description")]
        public string Description { get; set; }
        [Column("MeasurementUnit")]
        public string MeasurementUnit { get; set; }
    }

    public class AnalysisResult
    {
        //Read-Only
        public int AnalysisID { get; set; }
        [Column("AnalysisResult")]
        public string Result { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string DisplayText { get; set; }
    }

    public class Property
    {
        //Read-Only

        public int PropertyID { get; set; }
        //public string PropertyName { get; set; }
        public string DisplayText { get; set; }
        //public string Description { get; set; }        
    }

    public class PropertyValue
    {
        public int PropertyID { get; set; }
        //public int TermID { get; set; }
        //public int BroaderTermID { get; set; }
        public string PropertyUri { get; set; }
        public string DisplayText { get; set; }
        //public string HierarchyCache { get; set; }

    }

    public class PropertyList
    {
        [Column("DataSource")]
        public string Table { get; set; }
        [Column("PropertyID")]
        public int PropertyID { get; set; }
    }

    public class AnalysisTaxonomicGroup
    {
        //Read-Only
        public int AnalysisID { get; set; }
        public string TaxonomicGroup { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(AnalysisTaxonomicGroup))
            {
                var other = (AnalysisTaxonomicGroup)obj;
                return this.AnalysisID == other.AnalysisID && this.TaxonomicGroup == other.TaxonomicGroup;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return AnalysisID ^ TaxonomicGroup.GetHashCode();
        }

    }

    public class Qualification
    {
        public string Code { get; set; }
        public string DisplayText { get; set; }
    }

    public class Repository
    {
        public string DisplayText { get; set; }
        public string Database { get; set; }
    }

    [ExplicitColumns]
    public class TaxonList
    {
        [Column("DataSource")]
        public string Table { get; set; }
        [Column("TaxonomicGroup")]
        public string TaxonomicGroup { get; set; }
        [Column("DisplayText")]
        public string DisplayText { get; set; }

        public bool IsPublicList { get; set; }
    }

    public class TaxonName
    {
        //Read-Only
        [Column("NameURI")]
        public virtual string URI { get; set; }
        [Column("TaxonNameCache")]
        public virtual string TaxonNameCache { get; set; }
        [Column("TaxonNameSinAuthors")]
        public virtual string TaxonNameSinAuth { get; set; }
        [Column("GenusOrSupragenericName")]
        public virtual string GenusOrSupragenic { get; set; }
        [Column("SpeciesEpithet")]
        public virtual string SpeciesEpithet { get; set; }
        [Column("InfraspecificEpithet")]
        public virtual string InfraspecificEpithet { get; set; }
        //Synonymy Features
        [Column("Synonymy")]
        public virtual string Synonymy { get; set; }

        [Column("AcceptedNameURI")]
        public string AcceptedNameURI { get; set; }

        [Column("AcceptedNameCache")]
        public string AcceptedNameCache { get; set; }

    }

    [ExplicitColumns]
    public class Term
    {
        //Read-Only
        [Column]
        public string Code { get; set; }
        
        public DiversityPhone.Model.TermList Source { get; set; }
        public string Description { get; set; }

        [Column]
        public string DisplayText { get; set; }
        public string ParentCode { get; set; }
        public DateTime LogUpdatedWhen { get; set; }
    }
}
