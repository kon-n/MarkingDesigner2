namespace MarkingDesigner.ViewModels
{
    public class SequenceLinkSegment
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }

        public double DeltaX => EndX - StartX;
        public double DeltaY => EndY - StartY;
    }
}
