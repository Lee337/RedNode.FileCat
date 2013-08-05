using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedNode.FileCat.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace RedNode.FileCat.Tests
{
  [TestClass]
  public class FlatFileParsing
  {
    [TestMethod]
    public void CreateLineTest()
    {
      var parser = new Parser<HeaderRecord>();

      var line = parser.WriteLine(new HeaderRecord()
      {
        TransmissionNo = 1,
        Date = new DateTime(2013, 08, 04)
      });

      Assert.AreEqual(line, "000T040820130000001");
    }

    [TestMethod]
    public void CreateObjectFromLine()
    {
      var parser = new Parser<HeaderRecord>();

      var item = parser.CreateObjectFromString("000T040820130000001");

      var resultingItem = new HeaderRecord()
      {
        RecordIdentifier = 0,
        RecordStatus = 'T',
        Date = new DateTime(2013, 08, 04),
        TransmissionNo = 1
      };

      Assert.AreEqual(resultingItem.RecordIdentifier, item.RecordIdentifier);
      Assert.AreEqual(resultingItem.RecordStatus, item.RecordStatus);
      Assert.AreEqual(resultingItem.Date, item.Date);
      Assert.AreEqual(resultingItem.TransmissionNo, item.TransmissionNo);
    }

    [TestMethod]
    public void CreateFileContentTest()
    {
      var parser = new Parser<HeaderRecord>();
      var testObjects = new List<HeaderRecord>();

      testObjects.Add(new HeaderRecord()
      {
        TransmissionNo = 1,
        Date = new DateTime(2013, 08, 04)
      });

      testObjects.Add(new HeaderRecord()
      {
        TransmissionNo = 2,
        Date = new DateTime(2013, 08, 04)
      });

      var fileContent = parser.WriteLines(testObjects);

      Assert.AreEqual(fileContent, "000T040820130000001" + Environment.NewLine + "000T040820130000002" + Environment.NewLine);
    }

    [TestMethod]
    public void CreateLineMultiTest()
    {
      var parser = new MultiParser(typeof(HeaderRecord), typeof(Transaction), typeof(TrailerRecord));


      var list = new List<object>();
      list.Add(new HeaderRecord()
      {
        TransmissionNo = 1,
        Date = new DateTime(2013, 08, 04)
      });


      for (var i = 1; i <= 3; i++)
      {
        list.Add(new Transaction()
        {
          SequenceNo = i,
          AccountNo = (i * 123).ToString(),
          IdNumber = (i * i).ToString(),
          Amount = (i * 50)
        });
      }

      list.Add(new TrailerRecord()
      {
        Date = new DateTime(2013, 08, 04),
        TotalLines = 5
      });

      var content = parser.WriteLines(list);

      Assert.AreEqual(content, "000T040820130000001" + Environment.NewLine +
          "000T030000000100000001231            0000005000" + Environment.NewLine +
          "000T030000000200000002464            0000010000" + Environment.NewLine +
          "000T030000000300000003699            0000015000" + Environment.NewLine +
          "999T000000504082013" + Environment.NewLine);
    }

    [TestMethod]
    public void GetObjectFromMultiLineTest()
    {
      var parser = new MultiParser(typeof(HeaderRecord), typeof(Transaction));

      var item = (Transaction)parser.CreateObjectFromString("000T030000000200000002464            0000010000");

      var resultingItem = new Transaction()
        {
          SequenceNo = 2,
          AccountNo = "0000000246",
          IdNumber = "4            ",
          Amount = 100,
          RecordIdentifier = 0,
          RecordStatus = 'T',
          TransactionType = 30
        };


      Assert.AreEqual(resultingItem.RecordIdentifier, item.RecordIdentifier);
      Assert.AreEqual(resultingItem.RecordStatus, item.RecordStatus);
      Assert.AreEqual(resultingItem.TransactionType, item.TransactionType);
      Assert.AreEqual(resultingItem.SequenceNo, item.SequenceNo);
      Assert.AreEqual(resultingItem.AccountNo, item.AccountNo);
      Assert.AreEqual(resultingItem.IdNumber, item.IdNumber);
      Assert.AreEqual(resultingItem.Amount, item.Amount);
    }
  }

  public class HeaderRecord
  {
    [DefaultValue(0)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(3)]
    [Key]
    public int? RecordIdentifier { get; set; }

    [DefaultValue('T')]
    [StringLength(1)]
    public char? RecordStatus { get; set; }

    [Format("ddMMyyyy")]
    [StringLength(8)]
    public DateTime Date { get; set; }

    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(7)]
    public int? TransmissionNo { get; set; }
  }

  public class Transaction
  {
    [DefaultValue(0)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(3)]
    [Key]
    public int? RecordIdentifier { get; set; }

    [DefaultValue('T')]
    [StringLength(1)]
    public char? RecordStatus { get; set; }

    [DefaultValue(30)]
    [StringLength(3)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [Key]
    public int? TransactionType { get; set; }

    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(7)]
    public int? SequenceNo { get; set; }

    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(10)]
    public string AccountNo { get; set; }

    [StringLength(13)]
    public string IdNumber { get; set; }

    [Format("0.00", 2)]
    [StringLength(10)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    public Decimal Amount { get; set; }
  }

  public class TrailerRecord
  {
    [DefaultValue(999)]
    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(3)]
    [Key]
    public int? RecordIdentifier { get; set; }

    [DefaultValue('T')]
    [StringLength(1)]
    public char? RecordStatus { get; set; }

    [Pad('0', PadAttribute.PaddingDirection.Left)]
    [StringLength(7)]
    public int? TotalLines { get; set; }

    [Format("ddMMyyyy")]
    [StringLength(8)]
    public DateTime Date { get; set; }
  }
}