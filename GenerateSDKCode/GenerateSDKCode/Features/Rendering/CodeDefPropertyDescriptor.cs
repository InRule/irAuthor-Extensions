using System;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public class CodeDefPropertyDescriptor
    {
        public CodeDefPropertyDescriptor(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        public string Name;
        public Type Type;
    }
}