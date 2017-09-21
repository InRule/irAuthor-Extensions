using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{
    /// <summary>
    /// Interaction logic for TextPopupWindow.xaml
    /// </summary>
    public partial class TextPopupWindow : Window
    {
        public TextPopupWindow()
        {
            InitializeComponent();
        }
        public void Populate(string title, string text)
        {
            this.Title = title;
            this.txtSummary.Text = text;
        }
       }
}
