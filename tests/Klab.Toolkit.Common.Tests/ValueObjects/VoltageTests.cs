using Klab.Toolkit.Common.ValueObjects;

namespace Klab.Toolkit.CommonTests.ValueObjects;

[TestClass]
public class VoltageTests
{
    [TestMethod]
    public void TestVoltage()
    {
        var voltage = Voltage.Create(volt: 1.0);
        Assert.AreEqual(1.0, voltage.Volts);
        Assert.AreEqual(1000.0, voltage.Millivolts);
        Assert.AreEqual(1000000.0, voltage.Microvolts);
        Assert.AreEqual(0.001, voltage.Kilovolts);
        Assert.AreEqual(0.000001, voltage.Megavolts);
    }
}
