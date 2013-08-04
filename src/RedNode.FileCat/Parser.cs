using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RedNode.FileCat.Attributes;
using System.Globalization;
using System.ComponentModel;

namespace RedNode.FileCat
{
  public class Parser<TItem>
  {
    private PropertyInfo[] _properties { get; set; }

    public Parser()
    {
      _properties = typeof(TItem).GetProperties();
    }

    public string WriteLine(TItem item)
    {
      var line = new string(' ', GetLineLength());

      foreach (var property in _properties)
      {
        var attributes = property.GetCustomAttributes();

        var offsetAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(OffSetAttribute)) as OffSetAttribute;
        if (offsetAttribute == null)
          throw new ArgumentNullException(string.Format("Property {0} does not have an OffSet Attribute", property.Name));

        var itemContent = string.Empty;
        var value = property.GetValue(item, null);
        if (value == null)
        {
          // Get Default Value
          var defaultValueAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(DefaultValueAttribute)) as DefaultValueAttribute;
          if (defaultValueAttribute == null)
            throw new ArgumentNullException(string.Format("Property {0} does not have a value", property.Name));
          itemContent = defaultValueAttribute.Value.ToString();
        }
        else
        {
          itemContent = GetFormattedString(value, property);
        }


        if (itemContent.Length < offsetAttribute.Length)
        {
          // set padding
          var padValue = ' ';
          var padDirection = PadAttribute.PaddingDirection.Right;
          var padAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(PadAttribute)) as PadAttribute;
          if (padAttribute != null)
          {
            padValue = padAttribute.Padding;
            padDirection = padAttribute.PadDirection;
          }

          if (padDirection == PadAttribute.PaddingDirection.Right)
            itemContent = itemContent.PadRight(offsetAttribute.Length, padValue);
          else
            itemContent = itemContent.PadLeft(offsetAttribute.Length, padValue);
        }
        else
        {
          itemContent = itemContent.Substring(0, offsetAttribute.Length);
        }

        line = line.Remove(offsetAttribute.OffSet - 1, offsetAttribute.Length);
        line = line.Insert(offsetAttribute.OffSet - 1, itemContent);
      }

      return line;
    }

    private string GetFormattedString(object value, PropertyInfo propertyInfo)
    {
      var formatAttribute = propertyInfo.GetCustomAttribute<FormatStringAttribute>();
      if (formatAttribute == null)
        return value.ToString();

      return ((dynamic)value).ToString(formatAttribute.Format);
    }

    public TItem GetObject(string line)
    {
      var item = (TItem)Activator.CreateInstance(typeof(TItem));

      foreach (var property in _properties)
      {
        var attributes = property.GetCustomAttributes();

        var offSetAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(OffSetAttribute)) as OffSetAttribute;
        if (offSetAttribute == null)
          throw new ArgumentNullException(string.Format("Property {0} does not have an OffSet Attribute", property.Name));

        var itemContent = line.Substring(offSetAttribute.OffSet - 1, offSetAttribute.Length);

        property.SetValue(item, GetObject(itemContent, property));
      }

      return item;
    }

    private object GetObject(string itemContent, PropertyInfo propertyInfo)
    {
      if (string.IsNullOrEmpty(itemContent))
        return null;
      TypeConverter tc = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
      var formatAttribute = propertyInfo.GetCustomAttribute<FormatStringAttribute>();
      if (formatAttribute == null)
        return tc.ConvertFromInvariantString(itemContent);
      else if (propertyInfo.PropertyType == typeof(DateTime)
          || propertyInfo.PropertyType == typeof(DateTime?))
        return DateTime.ParseExact(itemContent, formatAttribute.Format, null);

      return null;
    }

    private int GetLineLength()
    {
      var maxOffSet = _properties.Select(a => a.GetCustomAttributes<OffSetAttribute>().FirstOrDefault()).Max(a => a.OffSet);
      if (maxOffSet == 0)
        return 0;
      return (maxOffSet - 1 + _properties.Select(a => a.GetCustomAttributes<OffSetAttribute>().FirstOrDefault()).FirstOrDefault(a => a.OffSet == maxOffSet).Length);
    }
  }
}