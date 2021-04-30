using System;
using System.CodeDom;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public class CodeTypeReferenceEx : CodeTypeReference
    {
        public Type ReferencedType { get; private set; }
        public CodeTypeReferenceEx(Type referencedType) : base(referencedType)
        {
            ReferencedType = referencedType;
        }
        public CodeObjectCreateExpression CallCodeConstructor(params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(this.ReferencedType, parameters);
        }
    }
}