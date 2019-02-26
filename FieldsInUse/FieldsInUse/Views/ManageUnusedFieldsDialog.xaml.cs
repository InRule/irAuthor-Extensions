using System.Collections.Generic;
using System.Windows;
using InRule.Authoring.Services;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{
	internal partial class ManageUnusedFieldsDialog : Window
	{
	    public FieldDefListView FieldDefListView;
	    public RuleApplicationController _controller;
        public ManageUnusedFieldsDialog(RuleApplicationDef ruleAppDef, RuleApplicationController controller)
	    {
	        InitializeComponent();
	        _controller = controller;

            var actions = new List<UnusedFieldAction>();
	        actions.Add(new UnusedFieldAction("Delete", (list, target) =>
	        {
	            _controller.RemoveDef(target.InnerDef);
	            list.RemoveItem(target);
	        }));
            
	        var columns = FieldDefListView.Options.NameColumn;
	        columns |= FieldDefListView.Options.TypeNameColumn;
	        
            
	        FieldDefListView = new FieldDefListView(ruleAppDef, columns, actions, "Name");
	        FieldDefListView.OnCloseView += FieldDefListViewOnCloseView;
            
            this.grdMain.Children.Add(FieldDefListView);
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
        }
        public RuleRepositoryDefBase InnerDef { get; set; }


        public string Name { get { return InnerDef.AuthoringElementPath; } }

        public string TypeName
        {
            get
            {
                var def = InnerDef as FieldDef;
                if (def == null) return "Unknown";

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
        }

        public override string ToString()
        {
            return InnerDef.GetFullName();
        }
    }

}
