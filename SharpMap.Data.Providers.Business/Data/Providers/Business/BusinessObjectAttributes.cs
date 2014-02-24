using System;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Attribute used to identify the identifier part
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BusinessObjectIdentifierAttribute : BusinessObjectAttributeAttribute
    {
        public override int Ordinal { get { return 0; } set { throw new NotSupportedException();}}
        public override bool IsUnique { get { return true; } set { throw new NotSupportedException(); } }
    }

    /// <summary>
    /// Attribute used to identify the geometry of the business object
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BusinessObjectGeometryAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute identifying valid properties of the business object
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class BusinessObjectAttributeAttribute : Attribute
    {
        /// <summary>
        /// Gets the ordinal
        /// </summary>
        public virtual int Ordinal { get; set; }
        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value can be modified
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the value is unique
        /// </summary>
        public virtual bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the value is unique
        /// </summary>
        public virtual bool AllowNull { get; set; }
    }

}