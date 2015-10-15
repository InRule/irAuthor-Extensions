# Undo extension for irAuthor

##Usage notes
Supports Undo of delete and insert of *Defs ONLY. Current buffer size is not end-user configurable, defaults to 3 (for testing). Number picked is arbitrary
##Known issues
* Entity tab - Def index in the RuleElements collection is being reported back as -1 for all FieldDef elements. Undoing a delete will not preserve the original order of fields, however the operation will complete normally
* (currently) Unsupported types 
	* Vocab template 
	* UDF libraries 
	* Data elements 
	* EndPoints 
	* Schemas 