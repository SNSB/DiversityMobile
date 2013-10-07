using DiversityORM;
using System;

namespace DiversityService.Model
{
    public class CollectionEventImage
    {
        public int CollectionEventID { get; set; }
        public string Uri { get; set; }
        public string ImageType { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }        
    }

    public class CollectionEventSeriesImage
    {
        public int SeriesID { get; set; }
        public string Uri { get; set; }
        public string ImageType { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    public class CollectionSpecimenImage
    {
        public int CollectionSpecimenID { get; set; }
        public int? IdentificationUnitID { get; set; }
        public string Uri { get; set; }
        public string ImageType { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    public static class MultimediaObjectExtensions
    {
        private static String MediaTypeToString(MultimediaType type)
        {
            switch (type)
            {
                case MultimediaType.Image:
                    return "photograph";                    
                case MultimediaType.Video:
                    return "video";                    
                case MultimediaType.Audio:
                    return "audio";
                default:
                    throw new NotImplementedException("Forgotten case");       
            }
        }


        public static CollectionEventSeriesImage ToSeriesImage(this MultimediaObject mmo)
        {
            if (mmo.OwnerType != MultimediaOwner.EventSeries)
                throw new ArgumentException("Related type mismatch");
            if (mmo.Uri == null)
                throw new ArgumentException("image not uploaded");
            CollectionEventSeriesImage export = new CollectionEventSeriesImage();
            export.SeriesID = mmo.RelatedCollectionID;
            export.ImageType = MediaTypeToString(mmo.MediaType);
            export.Uri = mmo.Uri.ToString();            
            export.Notes = "Generated via DiversityMobile";
            return export;
        }

        public static CollectionEventImage ToEventImage(this MultimediaObject mmo)
        {
            if (mmo.OwnerType != MultimediaOwner.Event)
                throw new ArgumentException("Related type mismatch");
            if (mmo.Uri == null)
                throw new ArgumentException("image not uploaded");

            CollectionEventImage export = new CollectionEventImage();
            export.CollectionEventID = mmo.RelatedCollectionID;
            export.ImageType = MediaTypeToString(mmo.MediaType);
            export.Uri = mmo.Uri.ToString();            
            export.Notes = "Generated via DiversityMobile";
            return export;
        }       

        public static CollectionSpecimenImage ToSpecimenImage(this MultimediaObject mmo, Diversity db)
        {
            if (mmo.OwnerType != MultimediaOwner.Specimen && mmo.OwnerType != MultimediaOwner.IdentificationUnit)
                throw new ArgumentException("Related type mismatch");
            if (mmo.Uri == null)
                throw new ArgumentException("image not uploaded");


            CollectionSpecimenImage export = new CollectionSpecimenImage();
            switch (mmo.OwnerType)
            {
                case MultimediaOwner.Specimen:
                    export.CollectionSpecimenID = mmo.RelatedCollectionID;
                    break;
                case MultimediaOwner.IdentificationUnit:
                    var iu = db.Single<IdentificationUnit>(mmo.RelatedCollectionID);
                    export.CollectionSpecimenID = iu.CollectionSpecimenID;
                    export.IdentificationUnitID = mmo.RelatedCollectionID;
                    break;
                default:
                    throw new NotImplementedException("Case overlooked");
            }
            export.ImageType = MediaTypeToString(mmo.MediaType);
            export.Uri = mmo.Uri.ToString();
            export.Description = mmo.Description;
            export.Notes = "Generated via DiversityMobile";            
            return export;
        }
    }
}
