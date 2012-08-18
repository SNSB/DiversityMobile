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

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapVM : PageVMBase
    {



        public ViewMapVM()
        {
            
        }
        public Point? calculateGPSToPercentagePoint(double? lat, double? lon)
        {
            if (lat == null || Double.IsNaN((double)lat) || lon == null || Double.IsNaN((double)lon) || Map == null)
                return null;
            return Map.calculatePercentilePositionForMap((double)lat, (double)lon);
        }


        public Point calculatePercentToPixelPoint(Point? percPoint, double iconSizeX, double iconSizeY, double zoom)
        {
            //Check if Point has a Representation on the current map
            if (percPoint == null || percPoint.Value.X < 0 || percPoint.Value.X > 1 || percPoint.Value.Y < 0 || percPoint.Value.Y > 1)
                return new Point(-1, -1);
            else
            {
                try
                {
                    int pixelWidth = MapImage.PixelWidth;
                    int pixelHeight = MapImage.PixelHeight;
                    double x = percPoint.Value.X * pixelWidth * zoom - iconSizeX / 2;
                    double y = percPoint.Value.Y * pixelHeight * zoom - iconSizeY / 2;
                    return new Point(x, y);
                }
                catch (Exception e)
                {
                    return new Point(-1, -1);
                }
            }
        }

        public Point calculatePixelToPercentPoint(Point pixelPoint)
        {
            double width = MapImage.PixelWidth * Zoom;
            double height = MapImage.PixelHeight * Zoom;
            double percX = pixelPoint.X / width;
            double percY = pixelPoint.Y / height;
            return new Point(percX, percY);
        }
    }
}
