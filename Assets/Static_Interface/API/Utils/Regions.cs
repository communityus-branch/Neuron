namespace Static_Interface.API.Utils
{
    public class Regions
    {
        public static bool CheckArea(byte x0, byte y0, byte x1, byte y1, byte area)
        {
            if ((x0 < (x1 - area)) || (y0 < (y1 - area)))
            {
                return false;
            }
            return ((x0 <= (x1 + area)) && (y0 <= (y1 + area)));
        }
    }
}