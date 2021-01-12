# Entity Schema Diagram Tool for IrAuthor
Loads a graph of the entity schema and relationships between entities in the Rule App.  The mouse scroll wheel allows you to zoom in and out, and click+drag allows you to navigate around the diagram.

## Style Key:
* **Collection** relationships are indicated by a thicker line, whereas Entity-type relationships are indicated by a thinner line.
* **Temporary** fields and Collections are indicated by a lighter-colored line and/or name, whereas non-temporary items are indicated by a black line and/or name

## The diagram can be displayed in up to three different ways:
* **Local Navigator:** The diagram will be opened in a new window within irAuthor.  Clicking on an Entity in this view will navigate to that Entity within the main irAuthor window. This is the option when you select the ribbon button itself, as well as being the first sub-option in the dropdown submenu.
* **Firefox:** If installed, this opens the diagram in Firefox.  Firefox's rendering engine is better supported by the diagraming tool (dagre-d3), so the full list of fields and datatypes is included within the box for each Entity, in addition to the entity name (as is displayed in Chrome and Edge).
* **Default Browser:** The diagram will be opened in your default browser, which allows the diagram to be saved and printed.

## Version History
