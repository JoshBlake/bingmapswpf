using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InfoStrat.VE.Utilities;

namespace DataBindingSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DataBindingSampleWindow : Window
    {
        DataModel dataModel;

        public DataBindingSampleWindow()
        {
            InitializeComponent();

            dataModel = new DataModel();

            this.DataContext = dataModel;
        }

        private void btnResetData_Click(object sender, RoutedEventArgs e)
        {
            dataModel.ResetData();
        }

    }
}
