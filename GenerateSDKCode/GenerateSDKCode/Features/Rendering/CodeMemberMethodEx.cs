using System;
using System.CodeDom;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public class CodeMemberMethodEx : CodeMemberMethod
    {
        public CodeTypeDeclarationEx OuterClass { get; set; }

        public CodeMemberMethodEx(CodeTypeDeclarationEx outerClass) : base()
        {
            OuterClass = outerClass;
        }

        public bool IsSharedFactoryMethod { get; set; }

        public void AddReturnStatment(CodeExpression variable)
        {
            var returnMethod = new CodeMethodReturnStatement(variable);
            this.Statements.Add(returnMethod);
        }
        public CodeVariableReferenceExpression AddMethodVariable(string name, Type type, CodeExpression value = null)
        {
            return this.AddMethodVariable(name, type.ToCodeTypeReference(), value);
        }
        public CodeVariableReferenceExpression AddMethodVariable(string name, CodeTypeReference type, CodeExpression value = null)
        {
            var variable = new CodeVariableDeclarationStatement();
            variable.Name = name;
            variable.Type = type;
            variable.InitExpression = value;
            this.Statements.Add(variable);
            return new CodeVariableReferenceExpression(name);
        }

        /// <summary>
        /// /// Adds a variable of the return type of the method
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeMethodVariableReferenceEx AddReturnVariable(string name, CodeExpression value = null)
        {
            this.AddMethodVariable(name, this.ReturnType, value);
            var variable = new CodeVariableReferenceExpression(name);
            return new CodeMethodVariableReferenceEx(this, variable);
        }

        public void AddStatement(CodeStatement statement)
        {
            this.Statements.Add(statement);
        }

        public CodeDefPropertyDescriptor[] DefPropertyAssignments { get; set; }
        
        public bool MatchesPropertyAssignmentNames(CodeDefPropertyDescriptor[] candidateProps)
        {
            var existingProps = this.DefPropertyAssignments;
            if(existingProps == null) existingProps = new CodeDefPropertyDescriptor[0];
            if (existingProps.Length != candidateProps.Length)
            {
                return false;
            }

            bool matchesByName = true;
            for(int i=0; i<candidateProps.Length; i++)
            {
                if (existingProps[i].Name != candidateProps[i].Name)
                {
                    matchesByName = false;
                }
            }
            return matchesByName;
        }

        public bool MatchesPropertyAssignments(CodeDefPropertyDescriptor[] candidateProps)
        {
            return MatchesPropertyAssignmentNames(candidateProps) && MatchesPropertyAssignmentOverloads(candidateProps);
        }

        public bool MatchesPropertyAssignmentOverloads(CodeDefPropertyDescriptor[] candidateProps)
        {
            var existingProps = this.DefPropertyAssignments;
            if (existingProps == null) existingProps = new CodeDefPropertyDescriptor[0];
            if (existingProps.Length != candidateProps.Length)
            {
                return false;
            }
            
            bool matchesTypeOverloads = true;
            // Check for bad overloads
            for (int i = 0; i < candidateProps.Length; i++)
            {
                if (existingProps[i].Type != candidateProps[i].Type)
                {
                    matchesTypeOverloads = false;
                }
            }
            return matchesTypeOverloads;
        }
    }
}