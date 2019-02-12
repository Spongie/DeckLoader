namespace DeckLoader.Models.TableTop
{
    public class TableTopTransform
    {
        public double posX { get; set; }
        public double posY { get; set; }
        public double posZ { get; set; }
        public double rotX { get; set; }
        public double rotY { get; set; } = 180;
        public double rotZ { get; set; }
        public double scaleX { get; set; } = 1.0;
        public double scaleY { get; set; } = 1.0;
        public double scaleZ { get; set; } = 1.0;
    }
}
