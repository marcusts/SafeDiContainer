namespace SharedIOS
{
   using CoreGraphics;
   using Xamarin.Forms;

   public static class PointExtensions
    {
        public static CGPoint ToCGPoint(this Point point)
        {
            return new CGPoint((double)point.X, (double)point.Y);
        }
    }
}
