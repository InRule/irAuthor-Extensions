# Undo extension for irAuthor

##Requirements
* Tested against version 4.6.27 of IrAuthor
* Earlier versions have not been tested but and v > 4.6 should work

##Installation
1. Make sure IrAuthor is not open
2. Unzip the files in UndoExtension.zip into your IrAuthor\Extensions folder (default location is C:\Program Files (x86)\InRule\irAuthor\Extensions\)
3. Open IrAuthor, then select **Extensions** from the **File** menu
4. Check the box next to **UndoExtension** to enable it.

##Usage notes
Supports Undo of delete and insert of *Defs ONLY. Current buffer size is not end-user configurable at this time but can be changed by modifying the source code

##Version History

### v0.0.1 (15-Oct-2015)
* (modification) default buffer size is now set to 5
* (new!) Redo functionality added. Whenever you undo an action, it is now possible to Redo that action
* (improvement) most authoring elements are now supported for undo/redo
* (fixed) Crash when creating various elements - e.g. Vocabulary template
* Assembly version properties added. Be careful when comparing to previous release using version numbers in DLL as previous release used default value of 1.0.0.0


### initial release 8-Oct-2015
* Entity tab - Def index in the RuleElements collection is being reported back as -1 for all FieldDef elements. Undoing a delete will not preserve the original order of fields, however the operation will complete normally
* (currently) Unsupported types 
	* Vocab template 
	* UDF libraries 
	* Data elements 
	* EndPoints 
	* Schemas 