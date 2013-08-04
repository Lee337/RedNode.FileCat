using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedNode.FileCat.Attributes
{
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
  public class OffSetAttribute : Attribute
  {
    public int OffSet { get; private set; }
    public int Length { get; private set; }

    public OffSetAttribute(int offSet, int length)
    {
      this.OffSet = offSet;
      this.Length = length;
    }
  }
}
