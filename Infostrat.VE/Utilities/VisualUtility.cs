using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace InfoStrat.VE.Utilities
{
    public static class VisualUtility
    {
        public static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;

            if (element is VEShape) return element;

            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }

        public static DependencyObject GetChildByType(DependencyObject element, Type type)
        {
            if (element == null) 
                return null;

            if (type.IsAssignableFrom(element.GetType()))
                return element;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                DependencyObject ret = GetChildByType(VisualTreeHelper.GetChild(element, i), type);
                if (ret != null)
                    return ret;
            }
            return null;
        }
    }
}
