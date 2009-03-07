using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace InfoStrat.VE.Utilities
{
    [ValueConversion(typeof(double), typeof(double))]
    public class RadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double radius = (double)value;
            return radius * 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double diameter = (double)value;
            return diameter / 2;
        }
    }
}
