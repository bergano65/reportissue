// Decompiled with JetBrains decompiler
// Type: ReportIssue.StringToIntConverter
// Assembly: ReportIssue, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AC348712-35C3-4312-9603-9127B12624A3
// Assembly location: C:\Users\Admin\Downloads\Dropbox\ri\Release\Release\ReportIssue.exe

using System;
using System.Globalization;
using System.Windows.Data;

namespace ReportIssue
{
    public class StringToIntConverter : IValueConverter
    {
        object IValueConverter.Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            int result;
            if (int.TryParse(value.ToString(), out result))
                return (object)result;
            return (object)-1;
        }

        object IValueConverter.ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return (object)value.ToString();
        }
    }
}
