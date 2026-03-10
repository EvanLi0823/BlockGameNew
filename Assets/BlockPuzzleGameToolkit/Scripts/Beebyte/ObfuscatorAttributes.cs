// ©2015 - 2025 Beebyte
// Obfuscator Attributes for Unity

using System;

namespace Beebyte.Obfuscator
{
    /// <summary>
    /// Rename a member to a specific name during obfuscation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct |
                    AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public class RenameAttribute : Attribute
    {
        public string NewName { get; }

        public RenameAttribute(string newName)
        {
            NewName = newName;
        }
    }

    /// <summary>
    /// Skip obfuscation entirely for this member
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct |
                    AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Parameter,
                    Inherited = false)]
    public class SkipAttribute : Attribute
    {
    }

    /// <summary>
    /// Skip renaming but allow other obfuscation techniques
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct |
                    AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Parameter,
                    Inherited = false)]
    public class SkipRenameAttribute : Attribute
    {
    }

    /// <summary>
    /// Obfuscate string literals in the method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
    public class ObfuscateLiteralsAttribute : Attribute
    {
    }

    /// <summary>
    /// Replace all string literals matching the method name with the obfuscated name
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ReplaceLiteralsWithNameAttribute : Attribute
    {
    }

    /// <summary>
    /// Mark a method as RPC method (for legacy networking)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RPCAttribute : Attribute
    {
    }
}