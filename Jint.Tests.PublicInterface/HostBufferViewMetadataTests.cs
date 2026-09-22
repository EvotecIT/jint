using Jint.Native;
using Xunit;

namespace Jint.Tests.PublicInterface;

public class HostBufferViewMetadataTests
{
    [Theory]
    [InlineData("Uint8Array")]
    [InlineData("DataView")]
    public void ReportsConstructionModeAcrossResizeAndDetach(string constructor)
    {
        using var engine = new Engine();
        engine.Execute($$"""
            const buffer = new ArrayBuffer(8, { maxByteLength: 16 });
            const tracking = new {{constructor}}(buffer, 2);
            const fixedLength = new {{constructor}}(buffer, 2, 6);
            const empty = new {{constructor}}(buffer, 8, 0);
            """);
        var tracking = engine.Evaluate("tracking");
        var fixedLength = engine.Evaluate("fixedLength");
        var empty = engine.Evaluate("empty");
        foreach (var mutation in new[] { "", "buffer.resize(16)", "buffer.resize(1)", "buffer.resize(8)", "buffer.transfer()" })
        {
            engine.Execute(mutation);
            Assert.True(tracking.IsLengthTrackingArrayBufferView());
            Assert.False(fixedLength.IsLengthTrackingArrayBufferView());
            Assert.False(empty.IsLengthTrackingArrayBufferView());
        }
    }

    [Theory]
    [InlineData("new Uint8Array(new ArrayBuffer(8))")]
    [InlineData("new DataView(new ArrayBuffer(8))")]
    [InlineData("new Proxy(new Uint8Array(new ArrayBuffer(8, {maxByteLength:16})), {})")]
    [InlineData("new Proxy(new DataView(new ArrayBuffer(8, {maxByteLength:16})), {})")]
    [InlineData("({get length(){throw new Error('getter')}, [Symbol.toStringTag]:'Uint8Array'})")]
    [InlineData("null")]
    public void RejectsNonTrackingValuesWithoutReadingTheirProperties(string source)
    {
        using var engine = new Engine();
        Assert.False(engine.Evaluate(source).IsLengthTrackingArrayBufferView());
    }

    [Theory]
    [InlineData("Uint8Array")]
    [InlineData("DataView")]
    public void ReportsLengthTrackingForGrowableSharedBuffers(string constructor)
    {
        using var engine = new Engine();
        var value = engine.Evaluate($"new {constructor}(new SharedArrayBuffer(8, {{maxByteLength:16}}))");
        Assert.True(value.IsLengthTrackingArrayBufferView());
    }
}
