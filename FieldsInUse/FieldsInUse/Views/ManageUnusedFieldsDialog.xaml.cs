using System.Collections.Generic;
using System.Windows;
using InRule.Authoring.Services;
using InRule.Repository;
using InRule.Authoring.Extensions;
using System.Linq;
using System.Xml.Linq;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions;
using InRule.Repository.EndPoints;
using System.Diagnostics;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{
	internal partial class ManageUnusedFieldsDialog : Window
	{
	    public FieldDefListView FieldDefListView;
	    public RuleApplicationController _controller;
        private IEnumerable<IRuleRepositoryDefBase> _schemas;
        public ManageUnusedFieldsDialog(RuleApplicationDef ruleAppDef, RuleApplicationController controller)
	    {
	        InitializeComponent();
	        _controller = controller;

            var actions = new List<UnusedFieldAction>();
            _schemas = ruleAppDef.SchemasRoot.AsEnumerable();

            actions.Add(new UnusedFieldAction("Delete", DeleteAction));
            
	        var columns = FieldDefListView.Options.NameColumn;
	        columns |= FieldDefListView.Options.TypeNameColumn;
            
	        FieldDefListView = new FieldDefListView(ruleAppDef, columns, actions, DeleteManyAction, "Name");
	        FieldDefListView.OnCloseView += FieldDefListViewOnCloseView;
            
            this.grdMain.Children.Add(FieldDefListView);
	    }
        private void DeleteAction(FieldDefListView list, UnusedFieldReference target)
        {
            //If the field is bound to a schema, the field must be removed from the schema as well, otherwise there will be validation errors
            if (target.ExternalSchemaFieldDef != null)
            {
                var info = target.ExternalSchemaFieldDef;
                var parentName = info.AuthoringElementPath.Substring(0, info.AuthoringElementPath.LastIndexOf('.'));
                var elementName = info.AuthoringElementPath.Replace(parentName, "").Substring(1);

                var schema = _schemas.FirstOrDefault(s => s.Name == info.AuthoringElementPath.Split('.')[0]);
                if (schema is XmlSchemaDef xmlSchema)
                {
                    if (target.ExternalSchemaFieldDef is EntityDefInfo)
                    {
                        var schemaItem = xmlSchema.EntityDefsInfo.EntityDefInfos.FirstOrDefault(i => i.Name == elementName);
                        xmlSchema.EntityDefsInfo.EntityDefInfos.Remove(schemaItem);
                    }
                    else if (target.ExternalSchemaFieldDef is FieldDefInfo)
                    {
                        var schemaItem = xmlSchema.EntityDefsInfo.EntityDefInfos.FirstOrDefault(i => i.AuthoringElementPath == parentName);
                        var fieldItem = schemaItem.Fields[elementName];
                        schemaItem.Fields.Remove(fieldItem);
                    }
                }
            }

            _controller.RemoveDef(target.InnerDef);
            list.RemoveItem(target);
        }
        public void DeleteManyAction(FieldDefListView list, IEnumerable<UnusedFieldReference> targets)
        {
            foreach (var target in targets)
            {
                //If the field is bound to a schema, the field must be removed from the schema as well, otherwise there will be validation errors
                if (target.ExternalSchemaFieldDef != null)
                {
                    var info = target.ExternalSchemaFieldDef;
                    var parentName = info.AuthoringElementPath.Substring(0, info.AuthoringElementPath.LastIndexOf('.'));
                    var elementName = info.AuthoringElementPath.Replace(parentName, "").Substring(1);

                    var schema = _schemas.FirstOrDefault(s => s.Name == info.AuthoringElementPath.Split('.')[0]);
                    if (schema is XmlSchemaDef xmlSchema)
                    {
                        if (target.ExternalSchemaFieldDef is EntityDefInfo)
                        {
                            var schemaItem = xmlSchema.EntityDefsInfo.EntityDefInfos.FirstOrDefault(i => i.Name == elementName);
                            xmlSchema.EntityDefsInfo.EntityDefInfos.Remove(schemaItem);
                        }
                        else if (target.ExternalSchemaFieldDef is FieldDefInfo)
                        {
                            var schemaItem = xmlSchema.EntityDefsInfo.EntityDefInfos.FirstOrDefault(i => i.AuthoringElementPath == parentName);
                            var fieldItem = schemaItem.Fields[elementName];
                            schemaItem.Fields.Remove(fieldItem);
                        }
                    }
                }

                //This is the bit that takes the 99% of the time 
                _controller.RemoveDef(target.InnerDef);
            }

            list.RemoveRange(targets);
        }

        private void FieldDefListViewOnCloseView(object sender, object e)
	    {
	        this.Close();
	    }

	    public void Dispose()
	    {
	        if (this.FieldDefListView != null)
	        {
	            this.FieldDefListView.Dispose();
	        }
	    }
    }

    public class UnusedFieldReference
    {
        public UnusedFieldReference(RuleRepositoryDefBase innerDef)
        {
            this.InnerDef = innerDef;

            if(InnerDef is FieldDef fd)
                this.ExternalSchemaFieldDef = fd.ExternalSchemaFieldDef;
            else if(InnerDef is EntityDef ed)
                this.ExternalSchemaFieldDef = ed.ExternalSchemaEntityDef;
        }
        public RuleRepositoryDefBase InnerDef { get; set; }
        public RuleRepositoryDefBase ExternalSchemaFieldDef { get; set; }


        public string Name { get { return InnerDef.AuthoringElementPath; } }

        public string TypeName
        {
            get
            {
                if (InnerDef == null) return "Unknown";

                if (InnerDef is FieldDef def)
                {

                    switch (def.DataType)
                    {
                        case DataType.Entity:
                            {
                                if (def.IsCollection)
                                {
                                    return $"{def.DataTypeEntityName}[]";
                                }
                                return $"{def.DataTypeEntityName}";
                            }
                        case DataType.Complex:
                            {
                                if (def.IsCollection)
                                {
                                    return $"{def.DataType.ToString()}[]";
                                }
                                return $"{def.DataType.ToString()}";
                            }
                        default:
                            return def.DataType.ToString();
                    }
                }
                else if (InnerDef is EntityDef)
                {
                    return "Entity";
                }
                else
                {
                    return "Error";
                }
            }
        }

        public override string ToString()
        {
            return InnerDef.GetFullName();
        }
    }

}
