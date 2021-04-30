using System;
using System.CodeDom;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_Primitives
    {
        public static CodeExpression ToCodeExpression(this Guid guidValue)
        {
            var guidValueExpresison = new CodePrimitiveExpression(guidValue.ToString());
            return typeof(Guid).CallCodeConstructor(guidValueExpresison);
        }

        public static CodeExpression ToCodeExpression(this DBNull value) { return new CodeSnippetExpression("System.DBNull.Value"); }
        public static CodeExpression ToCodeExpression(this bool value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this char value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this sbyte value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this byte value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this short value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this ushort value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this int value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this uint value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this long value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this ulong value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this float value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this double value) { return new CodePrimitiveExpression() { Value = value }; }
        public static CodeExpression ToCodeExpression(this decimal value) { return new CodePrimitiveExpression() { Value = value }; }

        public static CodeExpression ToCodeExpression(this DateTime def)
        {
            
            // public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
            return typeof(DateTime).CallCodeConstructor(
                def.Year.ToCodeExpression()
                , def.Month.ToCodeExpression()
                , def.Day.ToCodeExpression()
                , def.Hour.ToCodeExpression()
                , def.Minute.ToCodeExpression()
                , def.Second.ToCodeExpression()
                , def.Millisecond.ToCodeExpression()
                , def.Kind.ToCodeExpression()
                );
        }

        public static CodeExpression ToCodeExpression(this string value) { return new CodePrimitiveExpression() { Value = value }; }

        public static CodeExpression ToCodeExpression(this Enum enumValue)
        {
            var enumType = enumValue.GetTypeCode();
            var enumValueString = enumValue.ToString();
            return new CodeFieldReferenceExpression(enumValue.GetType().ToCodeTypeReference().ToCodeExpression(), enumValueString);
        }
    }
}