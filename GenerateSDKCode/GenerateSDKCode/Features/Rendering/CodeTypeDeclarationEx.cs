using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using InRule.Repository;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public class CodeTypeDeclarationEx
    {
        public HashSet<string> UsedNames = new HashSet<string>();
        public CodeTypeDeclarationEx ParentClass { get; set; }
        public CodeTypeDeclaration InnerDeclaration { get; set; }
        public List<CodeTypeDeclarationEx> ChildClasses = new List<CodeTypeDeclarationEx>();
        public bool IsDefFactory { get; set; }
        private CodeTypeDeclarationEx(CodeTypeDeclarationEx parentClass)
        {
            ParentClass = parentClass;
        }
        
        public CodeTypeDeclarationEx AddChildClass(object sourceDef, string name)
        {
            UsedNames.Add(name);
            var ret = new CodeTypeDeclarationEx(this);
            var innerTypeDeclaration = ret.SetInnerTypeDeclaration(this, sourceDef, name);


            var firstFactoryMember = this.ChildClasses.FirstOrDefault(c=>c.IsDefFactory);
            if (firstFactoryMember != null)
            {
                // Push the factory members to the end of the file
                var index = this.InnerDeclaration.Members.IndexOf(firstFactoryMember.InnerDeclaration);
                this.InnerDeclaration.Members.Insert(index, innerTypeDeclaration);
            }
            else
            {
                this.InnerDeclaration.Members.Add(innerTypeDeclaration);
            }
            ChildClasses.Add(ret);
            return ret;
        }
        
        private CodeTypeDeclaration SetInnerTypeDeclaration(CodeTypeDeclarationEx parent, object sourceDef, string name)
        {
            string typeName = name;
            if (sourceDef != null)
            {
                var defaultTypeName = sourceDef.GetType().Name + "_" + name;
                if (parent != null)
                {
                    // This adds it to the parent used names list
                    typeName = parent.GetSafeName(defaultTypeName, true);
                }
            }
            
            // This is the local used names list
            UsedNames.Add(typeName);
            var codeTypeDeclaration = CreateCodeTypeDeclarationInternal(typeName);
            codeTypeDeclaration.IsClass = true;
            codeTypeDeclaration.TypeAttributes = TypeAttributes.Public;
            this.InnerDeclaration = codeTypeDeclaration;
            return codeTypeDeclaration;
        }

        private static CodeTypeDeclaration CreateCodeTypeDeclarationInternal(string name)
        {
            return new CodeTypeDeclaration(name);
        }
        private string GetSafeName(string defaultName, bool addToUsedNamesList)
        {

            defaultName = defaultName.Replace("@", "");
            defaultName = defaultName.Replace(".", "_");
            defaultName = defaultName.Replace("+", "_");
            var typeName = defaultName;

            if (this.HasTypeName(defaultName))
            {
                int count = 0;
                typeName = defaultName + "_" + count;
                while (HasTypeName(typeName))
                {
                    count ++;
                    typeName = defaultName + "_" + count;
                }
            }

            if (addToUsedNamesList)
            {
                UsedNames.Add(typeName);
            }
            return typeName;
        }

        private bool HasTypeName(string name)
        {
            if (UsedNames.Contains(name)) return true;
            //if (this.GetMemberTypes().Any(t => t.Name == typeName))
            //{
            //    return true;
            //}
            //if (this.GetMemberMethods().Any(t => t.Name == typeName))
            //{
            //    return true;
            //}
            if (this.ParentClass != null)
            {
                return this.ParentClass.HasTypeName(name);
            }
            return false;
        }

        public string Name { get { return this.InnerDeclaration.Name; } }
        public IEnumerable<CodeTypeMember> GetMembers()
        {
            foreach (CodeTypeMember member in this.InnerDeclaration.Members)
            {
                yield return member;
            }
        }
        public CodeMemberMethodEx AddFactoryMethod(CodeTypeReferenceEx returnType, string methodName, out CodeMethodVariableReferenceEx returnVariable)
        {
            var method = new CodeMemberMethodEx(this);
            returnVariable = new CodeMethodVariableReferenceEx(method, method.AddMethodVariable("newDef", returnType, returnType.ReferencedType.CallCodeConstructor()));
            method.Name = methodName;
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.ReturnType = returnType;
            method.IsSharedFactoryMethod = true;
            this.AddMethod(method);
            

            return method;
        }
        public void AddMethod(CodeMemberMethodEx method)
        {
            UsedNames.Add(method.Name);
            this.InnerDeclaration.Members.Add(method);
        }

        public bool ContainsMethod(string methodName)
        {
            return GetMembers().OfType<CodeMemberMethodEx>().Any(m => m.Name == methodName);
        }
        public bool ContainsChildClass(string className)
        {
            return GetMembers().OfType<CodeTypeDeclarationEx>().Any(m => m.Name == className);
        }
        public bool ContainsSharedFactoryMethod(string methodName, params CodeDefPropertyDescriptor[] properties)
        {
            if (GetMembers().OfType<CodeMemberMethodEx>().Any(m => m.Name == methodName && m.MatchesPropertyAssignments(properties)))
            {
                return true;
            }

            if (properties.Length > 0)
            {
                var matchByOverload = GetMembers().OfType<CodeMemberMethodEx>().Where(m => m.Name == methodName && m.MatchesPropertyAssignmentOverloads(properties)).FirstOrDefault();
                if(matchByOverload != null)
                {
                    if (MessageBox.Show("Warning: Duplicate Factory Method Overloads", "Method Overload Collision", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        throw new Exception();
                    }
                }
            }
            return false;
        }

        public CodeMemberMethodEx AddCreateMethod(object sourceDef, bool justUseCreateForName = false)
        {
            var createMethod = new CodeMemberMethodEx(this);
            if (justUseCreateForName)
            {
                createMethod.Name = "Create";
            }
            else
            {
                var methodSuffix = sourceDef.GetType().Name;
                if (sourceDef is RuleRepositoryDefBase)
                {
                    var name = methodSuffix +"_" + ((RuleRepositoryDefBase) sourceDef).Name;
                    methodSuffix = name;
                }
                // Used name is captured below
                createMethod.Name = GetSafeName("Create_" + methodSuffix, false);
            }
            
            createMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            createMethod.ReturnType = sourceDef.GetType().ToCodeTypeReference();
            this.InnerDeclaration.Members.Add(createMethod);
            UsedNames.Add(createMethod.Name);
        
            return createMethod;
        }

        public static CodeTypeDeclarationEx CreateRoot(object sourceRuleAppDef)
        {
            string name = sourceRuleAppDef.GetType().Name;
            if (sourceRuleAppDef.IsInstanceOf<RuleApplicationDef>())
            {
                name = "RuleAppGen_" + ((RuleApplicationDef) sourceRuleAppDef).Name;
            }
            var ret = new CodeTypeDeclarationEx(null);
            ret.SetInnerTypeDeclaration(null, sourceRuleAppDef, name);
            return ret;
        }

       
        public CodeTypeDeclarationEx GetDefFactory()
        {
            if (this.ParentClass != null)
            {
                return this.ParentClass.GetDefFactory();
            }

            var defFactory = this.ChildClasses.FirstOrDefault(c => c.IsDefFactory);
            if (defFactory == null)
            {
                defFactory = this.AddChildClass(null, "DefFactory");
                defFactory.IsDefFactory = true;
            }
            return defFactory;
        }
    }
}