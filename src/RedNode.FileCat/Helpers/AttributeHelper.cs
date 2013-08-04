using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace RedNode.FileCat.Helpers
{
  public static class AttributeHelper
  {
    public static TAttribute GetValue<TAttribute>(Type type) where TAttribute : Attribute
    {
      return (TAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(TAttribute)).FirstOrDefault();
    }
  }
}
