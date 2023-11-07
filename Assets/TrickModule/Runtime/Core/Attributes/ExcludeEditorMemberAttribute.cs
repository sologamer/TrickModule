using System;

namespace TrickModule.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ExcludeEditorMemberAttribute : Attribute
    {

    }
}