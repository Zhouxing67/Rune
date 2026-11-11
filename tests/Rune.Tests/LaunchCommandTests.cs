using Rune.Engine;

namespace Rune.Tests;

public sealed class LaunchCommandTests
{
    [Fact]
    public void BareLaunchDoesNotOpenExecutable()
    {
        Assert.Empty(LaunchCommand.Parse([], "C:\\Rune.exe").Files);
        Assert.Empty(LaunchCommand.Parse(["C:\\Rune.exe"], "C:\\Rune.exe").Files);
    }

    [Fact]
    public void MultipleDocumentsSharePageAndZoomOptions()
    {
        var command = LaunchCommand.Parse(
            ["C:\\Rune.exe", "C:\\first file.pdf", "D:\\second.pdf", "--page", "5", "--zoom", "1.5"],
            "C:\\Rune.exe");
        Assert.Equal(new[] { "C:\\first file.pdf", "D:\\second.pdf" }, command.Files);
        Assert.Equal(5, command.Page);
        Assert.Equal(1.5, command.Zoom);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-1")]
    public void InvalidOptionsAreNotDocuments(string zoom)
    {
        var command = LaunchCommand.Parse(["a.pdf", "--page", "0", "--zoom", zoom], null);
        Assert.Single(command.Files);
        Assert.Null(command.Page);
        Assert.Null(command.Zoom);
    }
}
