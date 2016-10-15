using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using InRule.Authoring.Core.Navigation;
using InRule.Authoring.Windows;

namespace InRule.Authoring.Extensions.NavigationToolWindows
{
	public partial class ToolWindowHost : UserControl
	{
		public IIrAuthorShell IrAuthorShell { get; set; }

		private readonly INavigationBar _navigationBar;
		private readonly string _title;
		private readonly ImageSource _imageSourceSmall;
		private readonly ImageSource _imageSourceLarge;
		private string _popOutToolTip = "Move into tool window.";
		private string _popInToolTip = "Move into the navigation bar.";

		public ToolWindowHost(object control, INavigationBar navigationBar, string title, ImageSource imageSourceSmall, ImageSource imageSourceLarge)
		{
			InitializeComponent();

			content.Content = control;
			_navigationBar = navigationBar;
			_title = title;
			_imageSourceSmall = imageSourceSmall;
			_imageSourceLarge = imageSourceLarge;

			button.ToolTip = _popOutToolTip;
		}

		private void WhenButtonClicked(object sender, RoutedEventArgs e)
		{
			if (Parent != null)
			{
				if (Parent.GetType().Name == "NavigationPane")
				{
					button.Margin = new Thickness(0, -20, 35, 0);
					button.Content = "ß";
					button.ToolTip = _popInToolTip;

					var navigationPane = _navigationBar.GetNavigationPane(_title);

					if (navigationPane != null)
					{
						_navigationBar.RemoveNavigationPane(navigationPane);

						var control = (Control)navigationPane.Content;
						var parentControl = (ContentControl)control.Parent;

						parentControl.Content = null;

						var toolWindow = IrAuthorShell.AddToolWindow(control, "toolWindow" + GetHashCode(), _title, false, _imageSourceSmall);
						toolWindow.CanClose = false;
						toolWindow.Dock();
					}
				}
				else if (Parent.GetType().Name == "InRuleToolWindow")
				{
					button.Margin = new Thickness(0, -23, 0, 0);
					button.Content = "Þ";
					button.ToolTip = _popOutToolTip;

					var toolWindow = (HeaderedContentControl)Parent;

					toolWindow.Content = null;
					toolWindow.GetType().GetMethod("Destroy", new Type[0]).Invoke(toolWindow, new object[0]);

					var navigationBar = IrAuthorShell.NavigationControl as INavigationBar;

					if (navigationBar != null)
					{
						var navigationPane = navigationBar.AddNavigationPane(this, _title + GetHashCode(), _title, _imageSourceSmall, _imageSourceLarge);
						navigationBar.SelectedPane = navigationPane;
					}
				}
			}
		}
	}
}
