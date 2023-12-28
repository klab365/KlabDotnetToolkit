using System.Collections.Generic;

namespace Klab.Toolkit.Common.Extensions.Tests;

[TestClass]
public class EnumExtensionsTests
{
    [TestMethod]
    public void GetDescription()
    {
        Assert.AreEqual("This is a test", Test.EnumA.GetDescription());
    }

    [TestMethod]
    public void GetDictionaryWithEnumNameAndDescription()
    {
        Dictionary<string, string> table = EnumExtensions.GetDictionaryWithEnumNameAndDescription<Test>();
        Assert.AreEqual(1, table.Count);
        Assert.AreEqual("This is a test", table["EnumA"]);
    }
}

internal enum Test
{
    [System.ComponentModel.DescriptionAttribute("This is a test")]
    EnumA = 10,
}
