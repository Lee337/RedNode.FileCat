using System.Collections.Generic;
using System.Linq;

namespace RedNode.FileCat
{
  public class Parser<TItem>
  {
    private Common _common { get; set; }

    public Parser()
    {
      _common = new Common(typeof(TItem));
    }

    public string WriteLines(List<TItem> items)
    {
      return _common.WriteLines(items.Cast<object>().ToList());
    }

    public string WriteLine(TItem item)
    {
      return _common.WriteLine(item);
    }

    public TItem CreateObjectFromString(string line)
    {
      return (TItem)_common.CreateObjectFromString(line);
    }
  }
}