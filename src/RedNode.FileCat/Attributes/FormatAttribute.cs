using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedNode.FileCat.Attributes
{
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
  public class FormatAttribute : Attribute
  {
    public string Format { get; set; }
    public int RemoveCharAt { get; set; } // This will depend on the Format string added mainly for decimals (money)
    public string StringFormat { get; set; }

    public FormatAttribute(string format, int removeCharAt = 0)
    {
      this.Format = format;
      RemoveCharAt = removeCharAt;

    }
  }
}