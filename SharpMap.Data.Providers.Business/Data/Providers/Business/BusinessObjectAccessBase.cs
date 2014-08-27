// Copyright 2013-2014 - Felix Obermaier (www.ivv-aachen.de)
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
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// An abstract base class implementation of an <see cref="IBusinessObjectSource{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the business object</typeparam>
    public abstract class BusinessObjectAccessBase<T> : IBusinessObjectSource<T>
    {
// ReSharper disable InconsistentNaming
        protected static readonly TypeUtility<T>.MemberGetDelegate<uint> _getId;
        protected static TypeUtility<T>.MemberGetDelegate<IGeometry> _getGeometry;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Static constructor
        /// </summary>
        static BusinessObjectAccessBase()
        {
            _getId = TypeUtility<T>.GetMemberGetDelegate<uint>(typeof(BusinessObjectIdentifierAttribute));
            _getGeometry = TypeUtility<T>.GetMemberGetDelegate<IGeometry>(typeof(BusinessObjectGeometryAttribute));
        }

        /// <summary>
        /// Gets a value identifying the business object
        /// </summary>
        public virtual string Title { get; protected set; }

        /// <summary>
        /// Select a set of features based on <paramref name="box"/>
        /// </summary>
        /// <param name="box">The bounding box</param>
        /// <returns>A series of business objects</returns>
        public abstract IEnumerable<T> Select(Envelope box);

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        public abstract IEnumerable<T> Select(IGeometry geom);

        /// <summary>
        /// Select a set of features based on <paramref name="query"/>
        /// </summary>
        /// <param name="query">A query</param>
        /// <returns>A series of business objects</returns>
        public virtual IEnumerable<T> Select(IQueryable<T> query)
        {
            return query.ToList();
        }

        /// <summary>
        /// Select a business object by its id
        /// </summary>
        /// <param name="id">the id of the business object</param>
        /// <returns></returns>
        public abstract T Select(uint id);

        /// <summary>
        /// Update the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The business objects that need to be updated</param>
        public abstract void Update(IEnumerable<T> businessObjects);

        /// <summary>
        /// Delete the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The <typeparamref name="T"/>s that need to be deleted</param>
        public abstract void Delete(IEnumerable<T> businessObjects);

        /// <summary>
        /// Insert the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The features that need to be inserted</param>
        public abstract void Insert(IEnumerable<T> businessObjects);

        /// <summary>
        /// Method to get the geometry of a specific feature
        /// </summary>
        /// <param name="businessObject">The feature</param>
        /// <returns>The geometry</returns>
        public IGeometry GetGeometry(T businessObject)
        {
            return _getGeometry(businessObject);
        }

        /// <summary>
        /// Gets the id of the given business object
        /// </summary>
        /// <param name="businessObject">The business object</param>
        /// <returns>The id</returns>
        public uint GetId(T businessObject)
        {
            return _getId(businessObject);
        }

        /// <summary>
        /// Gets the number of business objects in the store
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets or sets the spatial extent of this repository
        /// </summary>
        protected Envelope CachedExtents { get; set; }

        /// <summary>
        /// Gets the extents of the 
        /// </summary>
        /// <returns>The extents</returns>
        public virtual Envelope GetExtents()
        {
            if (CachedExtents == null)
                throw new InvalidOperationException("You need to set Cached before using this function or override GetExtents");
            return CachedExtents;
        }
    }
}