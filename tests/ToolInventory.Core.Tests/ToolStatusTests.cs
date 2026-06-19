using ToolInventory.Core.Entities;

namespace ToolInventory.Core.Tests;

public class ToolStatusTests
{
    [Fact]
    public void NewTool_ShouldHaveAvailableStatus()
    {
        var tool = new Tool { Name = "Hammer" };
        Assert.Equal(ToolStatus.Available, tool.Status);
    }
}
