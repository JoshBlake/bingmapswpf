using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace DataBindingSample
{
    public class PushPinTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template1 { get; set; }
        public DataTemplate Template2 { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window mainWindow = Application.Current.MainWindow;

            LocationModel loc = item as LocationModel;
            if (loc == null || loc.AlternateItem)
            {
                return Template1;
            }

            return Template2;
        }
    }
}
