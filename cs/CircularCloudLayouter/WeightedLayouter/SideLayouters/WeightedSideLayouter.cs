using System.Drawing;
using CircularCloudLayouter.WeightedLayouter.Forming;

namespace CircularCloudLayouter.WeightedLayouter.SideLayouters;

public abstract class WeightedSideLayouter
{
    private const int NeighboursSpace = 2;
    private const double OffsetCoefficient = 0.8;

    protected readonly WeightedSegmentsCollection SideWeights = new();
    protected readonly Point Center;
    protected readonly FormFactor FormFactor;

    protected WeightedSideLayouter(Point center, FormFactor formFactor)
    {
        Center = center;
        FormFactor = formFactor;
    }

    public virtual double CalculateCoefficient() =>
        SideWeights.AverageWeight;

    public abstract Rectangle GetNextRectangle(Size rectSize);

    public abstract void UpdateWeights(Rectangle rect);

    protected (int Absolute, int Relative) FindNextRectPos(int sideLength, int middle)
    {
        sideLength += 2 * NeighboursSpace;

        var mergedSegments = new Queue<WeightedSegment>();
        var mergedWeight = int.MinValue;

        var bestScore = double.MinValue;
        var bestSegment = new WeightedSegment(0, 0, int.MaxValue);

        foreach (var segment in GetSegmentsWithOffset(GetOffsetLength(sideLength)))
        {
            mergedWeight = HandleNewSegment(segment, mergedSegments, sideLength, mergedWeight);
            var min = mergedSegments.Peek().Start;
            var max = segment.End;

            if (max - min < sideLength)
                continue;

            var mergedStart = FormFactor.GetPreferredStart(min, max, sideLength, middle);
            
            var mergedDistToCenter = GetDistToCenter(mergedStart, sideLength, middle);
            var score = FormFactor.GetSegmentScore(mergedWeight, mergedDistToCenter);

            if (score <= bestScore)
                continue;

            bestSegment = new WeightedSegment(mergedStart, mergedStart + sideLength, mergedWeight);
            bestScore = score;
        }

        return GetResultPos(bestSegment);
    }

    private static int GetOffsetLength(int sideLength) =>
        (int) Math.Floor(OffsetCoefficient * sideLength);

    private IEnumerable<WeightedSegment> GetSegmentsWithOffset(int offsetLength)
    {
        var startOffset = new WeightedSegment(SideWeights.Start - offsetLength, SideWeights.Start);
        var endOffset = new WeightedSegment(SideWeights.End, SideWeights.End + offsetLength);

        return new[] {startOffset}
            .Concat(SideWeights.Segments)
            .Concat(new[] {endOffset});
    }

    private static int HandleNewSegment(
        WeightedSegment segment,
        Queue<WeightedSegment> mergedSegments,
        int sideLength, int mergedWeight
    )
    {
        mergedSegments.Enqueue(segment);
        mergedWeight = Math.Max(mergedWeight, segment.Weight);

        if (segment.End - mergedSegments.Peek().End > sideLength && mergedSegments.Dequeue().Weight == mergedWeight)
            mergedWeight = mergedSegments.Max(sgm => sgm.Weight);

        return mergedWeight;
    }

    private static double GetDistToCenter(int start, int sideLength, int middle) =>
        Math.Abs(middle - (start + sideLength / 2d));

    private static (int Absolute, int Relative) GetResultPos(WeightedSegment segment) =>
        (segment.Start + NeighboursSpace, segment.Weight + NeighboursSpace);
}