# Test using Sample Data Extension

### Features
- Launches irVerify with data pre-populated from a local JSON file

### Requirements
- IrAuthor version 5.2 or newer

## Installation
1. Download the source code and build the solution
2. Copy InRule.Authoring.Extensions.TestUsingSampleData.dll into the irAuthor Extensions directory or a subdirectory therein
3. When you launch irAuthor, enable the Extension, and the functionality will be included in the Home ribbon's Test dropdown menu

## Usage
When testing a rule application, select the dropdown menu under the "Test" option on the "Home" ribbon of irAuthor.  There will initially be a "Sample Data" section containing only an option to "Change Sample Data Folder".  Select a directory that contains the appropriate directory and file structure as indicated below:

- Sample Data Folder
  - Root Entity Name
    - Sample Data One.json
    - Sample Data Two.json
  - Second Root Entity Name
    - Sample Data Three.json
    - Sample Data Four.json

If you are running from a file-based rule application and have not selected a Sample Data Folder, the extension will attempt using the folder containing the Rule Application file as the Sample Data Folder.

When a Sample Data Folder is selected, the "Sample Data" Section of the Test dropdown will contain each available sample dataset using the naming scheme "EntityName : ScenarioName".  To launch irVerify with the data pre-populated, simply select the appropriate Sample Data option from that menu.
