using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace POSSUM
{
    [ValueConversion(typeof(object), typeof(String))]
    public class EnumToNameConverter : IValueConverter
    {
        #region IValueConverter implementation


        public static String Convert(object value)
        {
            // To get around the stupid WPF designer bug
            if (value != null)
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                // To get around the stupid WPF designer bug
                if (fi != null)
                {
                    var attributes =
                        (LocalizedDescModelAttribute[])
            fi.GetCustomAttributes(typeof
            (LocalizedDescModelAttribute), false);

                    return ((attributes.Length > 0) &&
                            (!String.IsNullOrEmpty(attributes[0].Description)))
                               ?
                                   attributes[0].Description
                               : value.ToString();
                }

                return value.ToString();
            }

            return string.Empty;
        }


        /// <summary>
        /// Convert value for binding from source object
        /// </summary>
        public object Convert(object value, Type targetType,
                object parameter, CultureInfo culture)
        {
            // To get around the stupid WPF designer bug
            if (value != null)
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                // To get around the stupid WPF designer bug
                if (fi != null)
                {
                    var attributes =
                        (LocalizedDescModelAttribute[])
            fi.GetCustomAttributes(typeof
            (LocalizedDescModelAttribute), false);

                    return ((attributes.Length > 0) &&
                            (!String.IsNullOrEmpty(attributes[0].Description)))
                               ?
                                   attributes[0].Description
                               : value.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// ConvertBack value from binding back to source object
        /// </summary>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Cant convert back");
        }
        #endregion
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public BoolToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
                return true;
            if (Equals(value, FalseValue))
                return false;
            return null;
        }
    }


    public class StatusToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public StatusToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            //if (!(value is int))
            //    return null;
            return (int)value==0 ? FalseValue : TrueValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            bool visibility = (int)(value) == 0 ? false : true;
            if (Equals(visibility, TrueValue))
                return true;
            if (Equals(visibility, FalseValue))
                return false;
            return null;
        }
    }
    public class StringToColorConverter : IValueConverter
    {
        public StringToColorConverter()
        {

        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
           
            return Utilities.GetColorBrush(value==null? "#FFDCDEDE" : value.ToString());
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return 1;
        }

       
    }

    public class StatusToColorConverter : IValueConverter
    {
        public StatusToColorConverter()
        {

        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return GetItemStatusColorBrush(System.Convert.ToInt32(value));
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return 1;
        }

        public static SolidColorBrush GetItemStatusColorBrush(int id)
        {
            switch (id)
            {
                case 0:
                    var brush = (Brush)new BrushConverter().ConvertFromString("#FFC7FFD1");
                    return new SolidColorBrush(((SolidColorBrush)brush).Color);
                case 3:
                    var brush1 = (Brush)new BrushConverter().ConvertFromString("#FFFFFFFF");
                    return new SolidColorBrush(((SolidColorBrush)brush1).Color);
                case 4:
                    return new SolidColorBrush(Colors.Orange);
                case 5:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.IndianRed);
            }
        }
    }

    public class CurrencyFormatConverter : IValueConverter
    {
        public CurrencyFormatConverter()
        {

        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Math.Round(System.Convert.ToDecimal(value), 2).ToString("N", (CultureInfo)Defaults.UICultureInfo);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return 0;
        }
    }


}
