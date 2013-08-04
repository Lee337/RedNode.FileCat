using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedNode.FileCat.Attributes
{
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
  public class PadAttribute : Attribute
  {
    public char Padding { get; private set; }
    public PaddingDirection PadDirection { get; private set; }

    public PadAttribute(char padding, PaddingDirection padDirection = PaddingDirection.Right)
    {
      this.Padding = padding;
      this.PadDirection = padDirection;
    }

    public enum PaddingDirection
    {
      Right,
      Left
    }
  }
}