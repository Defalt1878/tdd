using System.Drawing;
using CircularCloudLayouter.WeightedLayouter.Forming;

namespace CircularCloudLayouter.WeightedLayouter.SideLayouters;

public class BottomSideLayouter : WeightedSideLayouter
{
    public BottomSideLayouter(Point center, FormFactor formFactor) : base(center, formFactor)
    {
    }

    public override Rectangle GetNextRectangle(Size rectSize)
    {
        var resPos = FindNextRectPos(rectSize.Width, Center.X);
        return new Rectangle(
            resPos.Absolute,
            Center.Y + resPos.Relative + 2,
            rectSize.Width, rectSize.Height
        );
    }

    public override void UpdateWeights(Rectangle rect)
    {
        var weight = rect.Bottom - Center.Y;
        if (weight < 0)
            return;
        SideWeights.UpdateGreaterWeights(new WeightedSegment(rect.Left, rect.Right, weight));
    }
}