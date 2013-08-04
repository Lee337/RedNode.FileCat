using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedNode.FileCat.Attributes;
using System.ComponentModel;

namespace RedNode.FileCat.Tests
{
  [TestClass]
  public class FlatFileParsing
  {
    [TestMethod]
    public void CreateLineTest()
    {
      var parser = new Parser<TestObject>();

      var line = parser.WriteLine(new TestObject()
      {
        AccountNo = "789654",
        IdNumber = "1234567890123",
        SequenceNo = 321,
        Date = new DateTime(2013, 08, 04)
      });

      Assert.AreEqual(line, "031T00003210000789654123456789012304082013");
    }

    [TestMethod]
    public void CreateObjectFromLine()
    {
      var parser = new Parser<TestObject>();

      var item = parser.GetObject("031T00003210000789654123456789012304082013");

      var resultingItem = new TestObject()
      {
        RecordIdentifier = 31,
        RecordStatus = 'T',
        AccountNo = "0000789654",
        IdNumber = "1234567890123",
        SequenceNo = 321,
        Date = new DateTime(2013, 08, 04)
      };

      Assert.AreEqual(resultingItem.AccountNo, item.AccountNo);
      Assert.AreEqual(resultingItem.Date, item.Date);
      Assert.AreEqual(resultingItem.IdNumber, item.IdNumber);
      Assert.AreEqual(resultingItem.RecordIdentifier, item.RecordIdentifier);
      Assert.AreEqual(resultingItem.RecordStatus, item.RecordStatus);
      Assert.AreEqual(resultingItem.SequenceNo, item.SequenceNo);
    }
  }

  public class TestObject
  {
    [DefaultValueAttribute(31)]
    [OffSet(1, 3)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    public int? RecordIdentifier { get; set; }

    [DefaultValueAttribute('T')]
    [OffSet(4, 1)]
    public char? RecordStatus { get; set; }

    [OffSet(5, 7)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    public int? SequenceNo { get; set; }

    [OffSet(12, 10)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    public string AccountNo { get; set; }

    [OffSet(22, 13)]
    public string IdNumber { get; set; }

    [OffSet(35, 8)]
    [FormatString("ddMMyyyy")]
    public DateTime Date { get; set; }
  }
}