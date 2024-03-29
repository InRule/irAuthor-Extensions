## Installation
1. To install the Machine Learning extension, download the installation package from [here](https://github.com/InRule/irAuthor-Extensions/releases/download/MachineLearningExtension_v1.0.1/MachineLearningExtension.v1.0.1.zip) to the PC where irAuthor is installed. 
2. Once downloaded, right-click on the ZIP file and select Properties. At the bottom of the Properties popup, check the box to Unblock, click Apply, and close the Properties popup. Then extract the zip archive into a folder.
3. Ensure there are no instances of irAuthor currently open.
4. Run one of the following batch files, as appropriate, to install the extension.
    + Install_AllUsers_LatestInRuleVersion
        - Installs the extension for all PC users and only the current version of irAuthor.
        This can be helpful if you run many versions of irAuthor in Side-by-Side mode, where some installed versions cannot support the extension.
        - Copies the extension files into "C:\Program Files (x86)\InRule\irAuthor\Extensions\Machine Learning Extension"
    + Install_CurrentUser_AllInRuleVersions
        - Installs the extension for only the currently logged-in user, but for all versions of InRule that can be run on the PC.
        - This can be helpful for shared authoring VM environments, so users do not unintentionally receive the extension.
        - Note that the user won’t be able to launch versions of irAuthor prior to 5.7.2 after running this batch, due to those versions being unable to load the extension.
        - Copies the extension files into "%localappdata%\InRule\irAuthor\ExtensionExchange\Machine Learning Extension"
5. Update the irAuthor configuration file (irAuthor.exe.config)located in C:\Program Files (x86)\InRule\irAuthor as needed
    + Add the following line in the `<appSettings>` section of the irAuthor configuration file (irAuthor.exe.config)  if not already there.
	```xml
	<add key="inrule:global:discovery:service" value="https://global.inrule.com/.well-known/discovery" />
	```
    + Add the following Binding Redirect if it is not already there (may be required for 5.7.2 and earlier).  As you add it, edit the version number (in this example, 5.7.2.240) in both the oldVersions and newVersion fields so that it matches the other InRule redirects in the config file.”
	```xml
	<dependentAssembly>
		<assemblyIdentity name="InRule.Authoring.Authentication" publicKeyToken="1feb8dd25b1ceb6b" />
		<bindingRedirect oldVersion="1.0.0.0-5.7.2.240" newVersion="5.7.2.240" />
	</dependentAssembly>
	```
6. When you launch irAuthor, open a Rule Application (or create a new one), and you should see a new "Machine Learning" section in the main navigation on the left-hand side under "Rules".
If you do not see it, go to File > Extensions and enable the Machine Learning Extension. 
    + If it does not appear in the list of extensions, then ensure that the extracted DLLs are unblocked by viewing their properties pages.
 
    + If, for any reason, it does not load correctly, you may see log information in the Event Log.  
Running the Uninstallation batch file will remove it if issues arise and remove the extension from either installation method. Ensure that there are no open instances of irAuthor when running the uninstallation.
