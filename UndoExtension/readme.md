# Undo extension for irAuthor

##Requirements
* Tested against version 4.6.27 of IrAuthor
* Earlier versions have not been tested but and v > 4.6 should work

##Usage notes
Adds support for undo/redo of delete and insert operations within irAuthor. If there are actions that can be undone/redone, the appropriate button will enable itself.

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