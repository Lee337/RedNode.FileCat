using System;
using System.Linq;
using System.Collections.Generic;

namespace RedNode.FileCat
{
  public class MultiParser
  {
    private Common _common { get; set; }

    public MultiParser(params Type[] types)
    {
      _common = new Common(types);
    }
    
    public string WriteLines(List<object> items)
    {
      return _common.WriteLines(items);
    }

    public string WriteLine(object item)
    {
      return _common.WriteLine(item);
    }

    public object CreateObjectFromString(string line)
    {
      return _common.CreateObjectFromString(line);
    }
  }
}
