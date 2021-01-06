# irAuthor Extensions
A collection of irAuthor extensions. Most are "user" extensions (as opposed to "system" extensions) and thus must be enabled before they can be used for the first time. You can find the Extensions dialog off of the File menu.

All extension projects reference the InRule SDK in `%programfiles%\InRule`, so it should simply be a matter of downloading and compiling.

## Managed
These extensions are available as compiled assemblies with all installation media included in the release.

|Extension|Description|
|---|---|
|[MergeExtension](MergeExtension)|Identify and review the differences between two different Rule Applications, and then merge selected changes together

## Unmanaged
These extensions are not officially supported by InRule Technology, but the source code is available for you to build or use as an example for an extension being built in-house.

|Extension|Description|
|---|---|
|[CatalogSearch](CatalogSearch)|Allows a search to be performed through all Rule Applications in a Catalog over a variety of fields
|[Commander](Commander)|Allows the user to access and execute any command for the currently selected item via the keyboard
|[DecisionTableExporter](DecisionTableExporter)|Exports Decision Tables to csv
|[DecisionTableImporter](DecisionTableImporter)|Imports Decision Tables from Excel
|[DiagramEntitySchema](DiagramEntitySchema)|Displays an interactive graph of the Rule Application's Entity Schema and relationships
|[ExportTable](ExportTable)|Exports Inline Tables
|[ExtensionManager](ExtensionManager)|Allows management of installed extensions from a central extension feed
|[FieldsInUse](FieldsInUse)|Delivers a rich report about which fields are currently in use in the application
|[FindUnusedSchema](FindUnusedSchema)|Displays a simple note about schema items that are never used
|[NavigationToolWindows](NavigationToolWindows)|Allows navigation panes to be converted to tool windows and vice versa
|[RefreshTemplateEngine](RefreshTemplateEngine)|Adds a ribbon button to refresh the template engine
|[RuleFlowVisualizer](RuleFlowVisualizer)|Builds a visualization of the rule flow logic
|[TitleVersion](TitleVersion)|Adds the current version of irAuthor to the title bar of the application
|[UndoExtension](UndoExtension)|Enhances the Undo functionality of irAuthor

## Building Unmanaged Extensions
1. Retrieve a copy of the source for the extension you're interested in (either via cloning or downloading an archive of the appropriate folder)
2. Open the solution for the extension, and Restore Nuget Packages, making sure that you have your irSDK files configured as an available NuGet source.  Alternatively, add new references for your local irSDK InRule assemblies.
3. Build the solution

## Installing Unmanaged Extensions
1. Make sure IrAuthor is not open
2. Create a subfolder in your Extensions folder for the new extension (default location is C:\Program Files (x86)\InRule\irAuthor\Extensions\)
3. Copy the build extension from your build directory into the new directory, making sure NOT to include any of the InRule.* or ActiproSoftware.* assemblies
4. Open IrAuthor, then select **Extensions** from the **File** menu
5. Check the box next to the name of your new extension to enable it.
