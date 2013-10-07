
namespace DiversityPhone.Model
{
    public class Coordinate : ILocalizable
    {
        public static readonly Coordinate Unknown = new Coordinate();


        public double? Altitude
        {
            get;
            set;
        }

        public double? Latitude
        {
            get;
            set;
        }

        public double? Longitude
        {
            get;
            set;
        }
    }

    public static class CoordinateMixin
    {
        public static bool IsUnknown(this Coordinate This)
        {
            return !This.Latitude.HasValue && !This.Longitude.HasValue && !This.Altitude.HasValue;
        }
    }
}
