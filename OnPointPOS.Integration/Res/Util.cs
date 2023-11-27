using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POSSUM.Res
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
}
