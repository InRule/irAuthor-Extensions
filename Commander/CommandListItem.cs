using InRule.Authoring.Commanding;

namespace InRule.Authoring.Extensions.Commander
{
	public class CommandListItem
	{
		public IVisualCommand Command { get; }
		public string DisplayName { get; }

		public CommandListItem(IVisualCommand command)
		{
			Command = command;

			string displayName = null;

			// Visual commands are nested in irAuthor via the following naming conventions: 
			// "Name"				No nesting
			// "Insert.Field"		"Field" is nested under Insert
			// "[Clipboard].Copy"	"Copy" is in the Clipboard group. Groups are separated by horizonal lines and do not have visible
			//						names.
			// Considering the above, a little work is required to make the command names more readable in a flat list.
			if (command?.Name != null)
			{
				var names = command.Name.Split('.');

				if (names.Length == 1)
				{
					// If it's just the name, show the label.
					displayName = command.Label;
				}
				else if (names.Length == 2)
				{
					if (names[0].StartsWith("["))
					{
						// Don't show the group name, just the label.
						displayName = command.Label;
					}
					else
					{
						// Show the group name plus the label.
						displayName = names[0].Substring(0, names[0].Length) + " " + command.Label;
					}
				}
			}

			DisplayName = displayName;
		}
	}
}
