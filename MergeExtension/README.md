# Merge Extension

### Features
- Select Rule Applications to compare as the source and target from local files, any configured Catalog references, or the currently open instance of irAuthor.
- Compare the two selected versions of a Rule Application and see a representation of each individual change.
- Select individual changes to merge, and open a new instance of irAuthor with the selected changes merged into the target Rule Application.

### Requirements
- IrAuthor version 5.2 or newer

## Installation
1. Download the installation package from [here](https://github.com/InRule/irAuthor-Extensions/releases/download/MergeExtension_v1.0.1/Merge.Extension.v1.0.1.zip).  If needed, right-click on the download file, go to "Properties", and tick the box to "Un-block" the downloaded file.
2. Extract the archive into a folder on your PC (which should also have irAuthor is installed).
3. Ensure there are no instances of irAuthor currently open.
4. Run the appropriate batch file to install the extension (or manually copy the directory to the appropriate location).
    + Install_AllUsers_LatestInRuleVersion
        - Installs the extension for all users of the PC, and for only the current version of irAuthor.
        - This can be helpful if you are running many versions of irAuthor in Side-by-Side mode, where some versions installed cannot support the extension.
        - Copies the extension files into "C:\Program Files (x86)\InRule\irAuthor\Extensions\MergeExtension"
    + Install_CurrentUser_AllInRuleVersions
        - Installs the extension for only the currently logged in user, but for all versions of InRule that can be run on the PC.
        - This can be helpful for shared authoring VM environments, so users do not unintentionally receive the extension.
        - Copies the extension files into "%localappdata%\InRule\irAuthor\ExtensionExchange\MergeExtension"
5. When you launch irAuthor, you should see the new icon available on the ribbon under "Catalog".  If for any reason it does not load correctly, you may see log information in the Event Log.  Running the Uninstallation will remove it if issues arise.

Note: the Uninstall batch file will remove the extension from either installation method.


## Usage
1. Select the Rule Application that contains the changes you're interested in as the "Source" Rule Application.
    - This may be the currently open Rule App, a .ruleapp or .ruleappx file, or a selection from a Catalog connection you have configured in irAuthor.
2. Select the Rule Application that you would like to push the changes into as the "Target" Rule Application.
    - This may be the currently open Rule App, a file path to a .ruleapp or .ruleappx file, or a selection from a Catalog connection you have configured in irAuthor.
3. Click "Compare" to run the engine against the two rules and genereate a list of differences between the two
4. Look through the list of changes, and select the changes that you would like to have merged into the "Target" Rule Application.  
5. Once you've selected all the changes that you want to include, click "Merge" to create the merged Rule Application
6. The merged Rule Application will open in a new instance of irAuthor.  You may now save it to a file or check it into a Catalog as desired.

### Example Use Cases
- **Logical Feature Branching:** Make changes for a new functionality in a separate file or catalog, then merge the changes into the main Rule Application once complete.
- **Granular Feature Promotion:** By comparing the latest from one environment against the next environment (IE comparing Development against Test), you can carefully select only the changes you want to move to allow for a more granular promtion of functionality.
- **Changeset Review:** By comparing a revision from a Catalog against the previous review, you can see all the changes that were made in that changest to ensure it contains what it should.
- **Rule Logic Migration:** If you have multiple Rule Applications that you want to combine into a single one, or logic in one Rule Application that would be helpful to have access to in another (and you don't want to use irCatalog's Shared Schema functionality), you could move the specific bits of logic around as desire.

## FAQ
 
### What is the grouping structure used when displaying the differences?
The list of changes in the rule application is grouped into a variety of categories:
+ Entity
    - RuleSets
        - Rule Changes
    - RuleSet Additions/Removals
    - Entity Schema Changes
    - Entity Metadata Changes
+ Entity Additions/Removals
+ Data Changes
+ Rule Application Metadata/Settings Changes

### Which version should I select for the "Source" and "Target" Rule Applications
The "Source" Rule Application should contain the changes you want to merge into another application.  The "Target" Rule Application should contain "old" information that needs to be updated with changes from the "Source" Rule Application.

### Why does "Merge" open a new instance of irAuthor instead of saving directly to the "Target"?
Because different users may need to perform different next steps after completing the merge, we simply open it in a new instance of irAuthor and allow the user to take whatever next step they need.  Save it to a new file, overwrite the "Target" file, check it into a Catalog as a new revision with a label, check it into a different catalog as a promotion - it's up to you!

### I found a bug or don't like the way something works in the extension.  What can I do?
We're sorry you found something that's not quite right!  Send an email to Support@inrule.com and we'll see if we might be able to include your suggestion or report in a future release of the extension.
