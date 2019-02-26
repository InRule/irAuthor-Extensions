using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using InRule.Authoring.Commanding;

namespace InRule.Authoring.Extensions.Commander
{
	public partial class CommanderWindow : Window
	{
		private string _text;
		private readonly IEnumerable<IVisualCommand> _commands;

		public CommanderWindow(IEnumerable<IVisualCommand> commands)
		{
			InitializeComponent();

			_commands = commands;

			PopulateListBox();

			DataContext = this;
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				ExecuteCommand();
			}
			else if (e.Key == Key.Down)
			{
				e.Handled = true;

				var index = listBox.SelectedIndex + 1;

				if (index < listBox.Items.Count)
				{
					listBox.SelectedIndex = index;
				}
				else if (listBox.Items.Count > 0)
				{
					listBox.SelectedIndex = 0;
				}
			}
			else if (e.Key == Key.Up)
			{
				e.Handled = true;

				var index = listBox.SelectedIndex - 1;

				if (index >= 0)
				{
					listBox.SelectedIndex = index;
				}
				else if (listBox.Items.Count > 0)
				{
					listBox.SelectedIndex = listBox.Items.Count - 1;
				}
			}
			else if (e.Key == Key.Escape)
			{
				e.Handled = true;

				Close();
			}
			else
			{
				base.OnPreviewKeyDown(e);
			}
		}

		private void ExecuteCommand()
		{
			var listItem = listBox.SelectedItem as CommandListItem;

			if (listItem != null)
			{
				Hide();
				listItem.Command.Execute();
				Close();
			}
		}

		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;

				PopulateListBox(_text.ToLower());
			}
		}

		private void PopulateListBox(string text = "")
		{
			// Get list items for all commands, since we're filtering on the list item's display name, whic is derived.
			var listItems = from command in _commands where command.IsEnabled select new CommandListItem(command);

			// Filter based on the display name
			var filteredListItems = (from listItem in listItems
									where listItem.DisplayName.IndexOf(text, StringComparison.OrdinalIgnoreCase) > -1
									select listItem).ToList();

			// Sort the list items.
			filteredListItems.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName));

			listBox.ItemsSource = filteredListItems;

			if (filteredListItems.Any())
			{
				listBox.SelectedIndex = 0;
			}
		}

		private void WhenListBoxDoubleClicked(object sender, MouseButtonEventArgs e)
		{
			ExecuteCommand();
		}
	}
}
