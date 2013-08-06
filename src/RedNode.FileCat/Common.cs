using RedNode.FileCat.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedNode.FileCat
{
  internal class Common
  {
    private Dictionary<Type, PropertyInfo[]> _typeProperties { get; set; }

    public Common(params Type[] types)
    {
      _typeProperties = new Dictionary<Type, PropertyInfo[]>();
      foreach (var type in types)
      {
        _typeProperties.Add(type, type.GetProperties());
      }
    }

    public string WriteLines(List<object> items)
    {
      var sb = new StringBuilder();

      items.ForEach(item =>
      {
        sb.AppendLine(WriteLine(item));
      });

      return sb.ToString();
    }

    public string WriteLine(object item)
    {
      var lineBuilder = new StringBuilder();

      foreach (var property in _typeProperties.FirstOrDefault(t => t.Key == item.GetType()).Value)
      {
        var attributes = property.GetCustomAttributes();

        var stringLength = attributes.FirstOrDefault(a => a.GetType() == typeof(StringLengthAttribute)) as StringLengthAttribute;
        if (stringLength == null)
          throw new ArgumentNullException(string.Format("Property {0} does not have an StringLengthAttribute", property.Name));

        var itemContent = string.Empty;
        var value = property.GetValue(item, null);
        if (value == null)
        {
          // Get Default Value
          var defaultValueAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(DefaultValueAttribute)) as DefaultValueAttribute;
          if (defaultValueAttribute == null && Nullable.GetUnderlyingType(property.GetType()) != null)
            throw new ArgumentNullException(string.Format("Property {0} does not have a value", property.Name));
          if (defaultValueAttribute != null)
            itemContent = defaultValueAttribute.Value.ToString();
        }
        else
        {
          itemContent = GetFormattedString(value, property);
        }

        if (itemContent.Length < stringLength.MaximumLength)
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
            itemContent = itemContent.PadRight(stringLength.MaximumLength, padValue);
          else
            itemContent = itemContent.PadLeft(stringLength.MaximumLength, padValue);
        }
        else
        {
          itemContent = itemContent.Substring(0, stringLength.MaximumLength);
        }
        lineBuilder.Append(itemContent);
      }

      return lineBuilder.ToString();
    }

    private string GetFormattedString(object value, PropertyInfo propertyInfo)
    {
      var formatAttribute = propertyInfo.GetCustomAttribute<FormatAttribute>();
      if (formatAttribute == null)
        return value.ToString();

      var strValue = ((dynamic)value).ToString(formatAttribute.Format);
      if (formatAttribute.RemoveCharAt == 0)
        return strValue;
      else
        return strValue.Remove(strValue.Length - formatAttribute.RemoveCharAt - 1, 1);
    }

    public object CreateObjectFromString(string line)
    {
      Type type;
      if (_typeProperties.Count > 1)
      {
        type = GetTypeForLine(line);
      }
      else
        type = _typeProperties.FirstOrDefault().Key;

      var item = Activator.CreateInstance(type);

      var offSet = 0;
      foreach (var property in _typeProperties.FirstOrDefault(t => t.Key == type).Value)
      {
        var attributes = property.GetCustomAttributes();

        var stringLengthAttribute = attributes.FirstOrDefault(a => a.GetType() == typeof(StringLengthAttribute)) as StringLengthAttribute;
        if (stringLengthAttribute == null)
          throw new ArgumentNullException(string.Format("Property {0} does not have an OffSet Attribute", property.Name));

        var itemContent = line.Substring(offSet, stringLengthAttribute.MaximumLength);
        offSet += stringLengthAttribute.MaximumLength;

        property.SetValue(item, GetObject(itemContent, property));
      }

      return item;
    }

    private Type GetTypeForLine(string line)
    {
      Tuple<Type, int> topMatch = null; // Item1 = best type match, Item2 = number of keys matched
      foreach (var typeProperty in _typeProperties)
      {
        var offSet = 0;
        Tuple<Type, int> tempMatch = null;
        foreach (var property in typeProperty.Value)
        {
          var stringLengthAttribute = property.GetCustomAttribute<StringLengthAttribute>();
          if (stringLengthAttribute == null)
            throw new ArgumentNullException(string.Format("Property {0} does not have an OffSet Attribute", property.Name));

          var keyAttribute = property.GetCustomAttribute<KeyAttribute>();
          if (keyAttribute != null)
          {
            var value = GetObject(line.Substring(offSet, stringLengthAttribute.MaximumLength), property).ToString();

            var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute == null)
              throw new ArgumentNullException(string.Format("Property {0} does not have a value", property.Name));
            var keyValue = defaultValueAttribute.Value.ToString();

            if (string.Compare(value, keyValue) == 0)
              tempMatch = new Tuple<Type, int>(typeProperty.Key, (tempMatch == null) ? 1 : tempMatch.Item2 + 1);
          }

          offSet += stringLengthAttribute.MaximumLength;
        }
        if (tempMatch != null)
        {
          if (topMatch == null)
            topMatch = tempMatch;
          else if (topMatch.Item2 < tempMatch.Item2)
            topMatch = tempMatch;
        }
      }
      if (topMatch == null)
        throw new InvalidFilterCriteriaException(string.Format("Cannot find a type for this line {0}", line));

      return topMatch.Item1;
    }

    private object GetObject(string itemContent, PropertyInfo propertyInfo)
    {
      if (string.IsNullOrEmpty(itemContent))
        return null;
      TypeConverter tc = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
      var formatAttribute = propertyInfo.GetCustomAttribute<FormatAttribute>();
      if (formatAttribute == null)
        return tc.ConvertFromInvariantString(itemContent);

      if (propertyInfo.PropertyType == typeof(DateTime)
          || propertyInfo.PropertyType == typeof(DateTime?))
        return DateTime.ParseExact(itemContent, formatAttribute.Format, null);

      if (propertyInfo.PropertyType == typeof(decimal)
          || propertyInfo.PropertyType == typeof(decimal?))
      {
        if (formatAttribute.RemoveCharAt > 0)
          itemContent = itemContent.Insert(itemContent.Length - formatAttribute.RemoveCharAt, ".");

        return Convert.ToDecimal(itemContent, CultureInfo.InvariantCulture);
      }
      throw new NotSupportedException(string.Format("Property {0}, Type not supported", propertyInfo.Name));
    }
  }
}