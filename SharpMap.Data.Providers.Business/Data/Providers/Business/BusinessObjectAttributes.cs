// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Business.
// SharpMap.Business is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Business is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
using System;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Attribute used to identify the identifier part
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BusinessObjectIdentifierAttribute : BusinessObjectAttributeAttribute
    {
        public BusinessObjectIdentifierAttribute()
        {
            OrdinalValue = 0;
        }

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
        protected int OrdinalValue = 9999;
        
        /// <summary>
        /// Gets the ordinal
        /// </summary>
        public int Ordinal { get { return OrdinalValue; } set { OrdinalValue = value; } }
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

        /// <summary>
        /// Gets or sets a value indicating that the value does not matter
        /// </summary>
        public virtual bool Ignore { get; set; }
    }

}