using BenchmarkDotNet.Attributes;

namespace EfficientCsharp;

[MemoryDiagnoser(false)]
public class GuiderBenchmarks
{
    private static readonly Guid TestIdAsGuid = Guid.Parse("DD616016-C419-4AB3-ADDA-6A049193EB42");
    private const string TestIdAsString = "FmBh3RnEs0qt2moEkZPrQg";

    [Benchmark]
    public Guid ToGuidFromString()
    {
        return Guider.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromStringOptimized()
    {
        return Guider.ToGuidFromStringOptimized(TestIdAsString);
    }

    [Benchmark]
    public string ToStringFromGuid()
    {
        return Guider.ToStringFromGuid(TestIdAsGuid);
    }
    
    [Benchmark]
    public string ToStringFromGuidOptimized()
    {
        return Guider.ToStringFromGuidOptimized(TestIdAsGuid);
    }
}