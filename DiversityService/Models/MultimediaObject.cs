using System;


namespace DiversityService.Model
{
    public class MultimediaObject
    {
        public string OwnerType { get; set; }
        public int RelatedId { get; set; }
        public String Uri {get;set;}
        public String Description { get; set; }
        public String MediaType { get; set; }
        public DateTime LogUpdatedWhen { get; set; }

        public static CollectionEventSeriesImage ToSeriesImage(MultimediaObject mmo)
        {
            if (!mmo.OwnerType.Equals("EventSeries"))
                throw new Exception("Related type mismatch");
            if (mmo.Uri == null)
                throw new Exception("image not uploaded");
            CollectionEventSeriesImage export = new CollectionEventSeriesImage();
            export.SeriesID = mmo.RelatedId;
            export.ImageType = mmo.MediaType.ToString().ToLower();
            export.Uri = mmo.Uri;
            export.LogUpdatedWhen = mmo.LogUpdatedWhen;
            return export;
        }

        public static CollectionEventImage ToEventImage(MultimediaObject mmo)
        {
            if (!mmo.OwnerType.Equals("EventSeries"))
                throw new Exception("Related type mismatch");
            if (mmo.Uri == null)
                throw new Exception("image not uploaded");
            CollectionEventImage export = new CollectionEventImage();
            export.CollectionEventID = mmo.RelatedId;
            export.ImageType = mmo.MediaType.ToString().ToLower();
            export.Uri = mmo.Uri;
            export.LogUpdatedWhen = mmo.LogUpdatedWhen;
            return export;
        }
      
        public static CollectionSpecimenImage ToSpecimenImage(MultimediaObject mmo, IdentificationUnit iu)
        {
            if (!(mmo.OwnerType.Equals("Specimen") || mmo.OwnerType.Equals("IU")))
                throw new Exception("Related type mismatch");
            if (mmo.Uri == null)
                throw new Exception("image not uploaded");
            CollectionSpecimenImage export = new CollectionSpecimenImage();
            if (mmo.OwnerType.Equals("Specimen"))
                export.CollectionSpecimenID = mmo.RelatedId;
            if (mmo.OwnerType.Equals("IdentificationUnit"))
            {
                if (iu == null || iu.DiversityCollectionSpecimenID == null || iu.DiversityCollectionUnitID != mmo.RelatedId)
                    throw new Exception("iu not found or partner not synced");
                export.CollectionSpecimenID = (int)iu.DiversityCollectionSpecimenID;
                export.IdentificationUnitID = (int)mmo.RelatedId;
            }
            export.ImageType = mmo.MediaType.ToString().ToLower();
            export.Uri = mmo.Uri;
            export.Description = mmo.Description;
            export.Notes="Generated via DiversityMobile";
            export.LogUpdatedWhen = mmo.LogUpdatedWhen;
            return export;
        }

    }
}
