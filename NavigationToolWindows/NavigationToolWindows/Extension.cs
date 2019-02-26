using System;
using System.Windows.Controls;
using InRule.Authoring.Core.Navigation;
using InRule.Authoring.Windows;
using System.Linq;

namespace InRule.Authoring.Extensions.NavigationToolWindows
{
	public class Extension : ExtensionBase
	{
		public static Guid ID = new Guid("{871C3491-096B-4351-9F7A-BE006CE147CE}");

		public Extension()
			: base("NavigationToolWindows", "Allows navigation panes to be converted to tool windows and vice versa", ID)
		{}

		public override void Enable()
		{
			var navBar = IrAuthorShell.NavigationControl as INavigationBar;

			if (navBar != null)
			{
				var navPanes = navBar.NavigationPanes.ToList();

				foreach (var navPane in navPanes)
				{
					var control = (Control)navPane.Content;
					var parentControl = (ContentControl)control.Parent;

					navBar.RemoveNavigationPane(navPane);

					parentControl.Content = null;

					var host = ServiceManager.Compose<ToolWindowHost>(control, navBar, navPane.Title, navPane.ImageSourceSmall, navPane.ImageSourceLarge);

					navBar.AddNavigationPane(host, navPane.Name, navPane.Title, navPane.ImageSourceSmall, navPane.ImageSourceLarge);
				}
			}
		}
	}
}
