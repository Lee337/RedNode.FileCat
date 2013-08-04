using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedNode.FileCat.Attributes
{
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
  public class FormatStringAttribute : Attribute
  {
    public string Format { get; set; }

    public FormatStringAttribute(string format)
    {
      this.Format = format;
    }
  }
}