using System;
using System.Windows;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.GenerateSDKCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SDKCodePopupView : Window
    {
        private object SourceObject { get; set; }
        public SDKCodePopupView(object sourceObject)
        {
            InitializeComponent();
            this.Loaded += SDKCodePopupView_Loaded;
            SourceObject = sourceObject;
            if (sourceObject is RuleRepositoryDefBase)
            {
                this.Title = "SDK Code: " + ((RuleRepositoryDefBase) sourceObject).GetFullName();
            }
            else
            {
                this.Title = "SDK Code: " + sourceObject.GetType().Name;
            }
            
        }

        private void SDKCodePopupView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.txt.Text = SourceObject.ToSdkCode();
            }
            catch (Exception ex)
            {
                this.txt.Text = ex.Message;
            }
        }
        
    }
}
