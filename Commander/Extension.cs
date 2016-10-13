using System;
using System.Windows;
using System.Windows.Input;
using InRule.Authoring.Commanding;
using InRule.Authoring.Windows;

namespace InRule.Authoring.Extensions.Commander
{
	public class Extension : ExtensionBase
	{
		private KeyBinding _inputBinding;

		public Extension()
			: base("Commander", "Popup that allows command execution from the keyboard", new Guid("{E5F16940-71F7-48E0-9859-15B9209E9352}"))
		{}

		public override void Enable()
		{
			// Tell irAuthor to call our DoIt method if the user presses Control+.
			_inputBinding = new KeyBinding(new DelegateCommand(DoIt), Key.OemPeriod, ModifierKeys.Control);
			IrAuthorShell.InputBindings.Add(_inputBinding);
		}

		public override void Disable()
		{
			IrAuthorShell.InputBindings.Remove(_inputBinding);
		}

		private void DoIt(object obj)
		{
			var commands = CommandService.GetCommands(SelectionManager.SelectedItem);

			if (commands == null)
			{
				MessageBoxFactory.Show("no commands", "no commads");
			}
			else
			{
				var window = new CommanderWindow(commands);
				window.Owner = Application.Current.MainWindow;
				window.Show();
			}
		}
	}
}
