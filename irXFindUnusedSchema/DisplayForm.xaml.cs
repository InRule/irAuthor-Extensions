using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace irXFindUnusedSchema
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DisplayForm : Window
    {
        public DisplayForm()
        {
            
            InitializeComponent();
        }

        public string TheText
        {
            get
            {
                return TheTextBox.Text;
            }
            set
            {
                TheTextBox.Text = value;
            }
        }

        private void CloseItButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    }
