using AutoFixture;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Tests;

[TestClass]
public class DefaultMetaWriterTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    public async Task WriteMetaAsync_EmptyMeta_JustWritesHeader()
    {
        using var ms = new MemoryStream();
        var meta = new Meta();

        var dw = new DefaultMetaWriter(new DefaultNavWriter());

        await dw.WriteMetaAsync(ms, meta);

        ms.Seek(0, SeekOrigin.Begin);
        var str = Encoding.UTF8.GetString(ms.ToArray());

        Assert.AreEqual("1\r\nCondAct\r\n5\r\nCType\r\nAType\r\nCData\r\nAData\r\nState\r\nn\r\nn\r\nn\r\nn\r\nn\r\n0\r\n", str);
    }

    [TestMethod]
    public async Task WriteMetaAsync_MetaWithRule_WritesMetaWithRule()
    {
        using var ms = new MemoryStream();
        var meta = new Meta();

        meta.Rules.Add(new Rule()
        {
            Condition = Condition.CreateCondition(ConditionType.Never),
            Action = MetaAction.CreateMetaAction(ActionType.None),
            State = "Default"
        });

        var dw = new DefaultMetaWriter(new DefaultNavWriter());

        await dw.WriteMetaAsync(ms, meta);

        ms.Seek(0, SeekOrigin.Begin);
        var str = Encoding.UTF8.GetString(ms.ToArray());

        Assert.AreEqual("1\r\nCondAct\r\n5\r\nCType\r\nAType\r\nCData\r\nAData\r\nState\r\nn\r\nn\r\nn\r\nn\r\nn\r\n1\r\ni\r\n0\r\ni\r\n0\r\ni\r\n0\r\ni\r\n0\r\ns\r\nDefault\r\n", str);
    }
}
