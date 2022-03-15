# Machine Learning for xAI Workbench Extension

### Features
- Adds the ability to natively interact with xAI Workbench Machine Learning engines

### Requirements
- The extension supports IrAuthor version 5.7.2 and newer

## Installation
1. Download the installation package from [here](https://github.com/InRule/irAuthor-Extensions/releases/download/MachineLearningExtension/MachineLearningExtension.v1.0.0.zip).
2. Extract the archive into a folder on your PC where irAuthor is installed.
3. Ensure there are no instances of irAuthor currently open.
4. Run the appropriate batch file to install the extension.
    + Install_AllUsers_LatestInRuleVersion
        - Installs the extension for all users of the PC, and for only the current version of irAuthor.
        - This can be helpful if you are running many versions of irAuthor in Side-by-Side mode, where some versions installed cannot support the extension.
        - Copies the extension files into "C:\Program Files (x86)\InRule\irAuthor\Extensions\OpenFromSampleTemplate"
    + Install_CurrentUser_AllInRuleVersions
        - Installs the extension for only the currently logged in user, but for all versions of InRule that can be run on the PC.
        - This can be helpful for shared authoring VM environments, so users do not unintentionally receive the extension.
        - Copies the extension files into "%localappdata%\InRule\irAuthor\ExtensionExchange\OpenFromSampleTemplate"
5. Update the irAuthor configuration file (irAuthor.exe.config) as needed
    + Add one of the following lines in the `<appSettings>` section if not already there (the first option if running as a Production customer, the second if running in a Trial environment).
	```xml
	<add key="inrule:authoring:tenantManagementApiUrl" value="https://ir-tenantmgmt-prod-ncus-wa.azurewebsites.net" />
	OR
	<add key="inrule:authoring:tenantManagementApiUrl" value="https://ir-tenantmgmt-trial-ncus-wa.azurewebsites.net" />
	```
    + Add the following Binding Redirect if it is not already there (may be required for 5.7.2 and earlier).  The version number (in this example, 5.7.2.240) in both the oldVersions and newVersion fields should match the other InRule redirects in the config file.
	```xml
	<dependentAssembly>
		<assemblyIdentity name="InRule.Authoring.Authentication" publicKeyToken="1feb8dd25b1ceb6b" />
		<bindingRedirect oldVersion="1.0.0.0-5.7.2.240" newVersion="5.7.2.240" />
	</dependentAssembly>
	```
6. When you launch irAuthor, open a Rule Application (or create a new one) and you should see a new "Machine Learning" section in the main navigation above "Decisions".  If you do not see it, go to File > Extensions and enable the Machine Learning Extension.  
    + If for any reason it does not load correctly, you may see log information in the Event Log.  Running the Uninstallation will remove it if issues arise, and will remove the extension from either installation method.


## Usage
1. Create a new Machine Learning interaction
    + Click on the Machine Learning section in irAuthor.
    + Click on the green plus with "Add" at the top of the section.
2. Connect to an instance of XAI Workbench
    + Click on "Add New..." under the "Machine Learning Service Connection" section
    + Click the "Log In" button if necessary to authenticate, and you should see your tenants listed in the dropdown list - choose the one you would like to use that has an XAI Workbench associated with it, as well as the relevant instance of xAI Workbench.  It may be automatically selected for you.
        - If your environment does not yet have an instance of xAI Workbench provisioned, navigate to your Portal and access it using the link at the top of page to provision it.
        - If you would like to connect to a specific xAI Workbench instance using basic authentication, click on the "Explicit Username/Password" section under "Advanced" and fill out all fields.
        - If a newly created Tenant is not showing up in the dropdown list, try logging out and back in to irAuthor using the Home ribbon "Account" section.
    + Clicking "Save" will load a list of the ML Models that have been set up on the selected instance of XAI Workbench.
    + If you need to load the XAI Workbench web interface, click on the button to the right of the blue arrow next to the Endpoint.
3. Choose an ML Model that you want to use
    + From the "Model" dropdown list, select a model that you would like to interact with during rule execution.
    + If the selected model uses SimClassify+, you can choose to include the Rationale behind the prediction in the output from the ML engine.
    + The "Model Structure" section displays a listing of the various fields that are used by the selected model.
    + To automatically generate rule application schema that conforms to the model's structure, select an option from the Data Target dropdown, then click on "Apply ML Model"
        - If you select an existing Entity, the Entity will have fields added to represent each of the inputs and outputs listed in the Model Structure if needed; existing fields will be left as-is.
        - If you select an existing Decision, the Decision will have Inputs and Outputs updated added to represent each of the inputs and outputs listed in the Model Structure if needed; existing inputs and outputs will be left as-is.
        - If you select a New Entity, a new Entity will be created with fields added to represent each of the inputs and outputs listed in the Model Structure.
        - If you select New Decision, a new Decision will be created with Inputs representing each of the Model Structure Input Fields, and Outputs representing the Prediction, Score, and Rationale (if applicable).
4. Create Rules to execute the Machine Learning Prediction
    + From the Entity or Decision, navigate to a RuleSet that exists underneath that ML-Model-conforming item.
    + Right-Click on the RuleSet, and add a new Machine Learning Prediction.
    + The Inputs and Outputs will be automatically populated with the fields that were generated during the "Apply ML Model" process.
        - This auto-generation and auto-population of inputs and outputs is done to simplify the process, but is by no means required.  You may absolutely map existing fields in your data model into a Machine Learning model's input and output fields, and not have a specific Entity or Decision that represents the ML Model.
    + After the Machine Learning Prediction rule, add any additional rules to consume the prediction, Score, and (if applicable) PredictionRationale.
5. Execute Rules!


## FAQ
 
### I found a bug or don't like the way something works in the extension.  What can I do?
We're sorry you found something that's not quite right!  Send an email to Support@inrule.com and we'll see if we might be able to include your suggestion or report in a future release of the extension.
