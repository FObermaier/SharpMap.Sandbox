// Copyright 2014 -      Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Data.Providers.Business.
// SharpMap.Data.Providers.Business is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.Business is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Simple CRUD interface for buisness objects
    /// </summary>
    /// <typeparam name="T">The typ of the business object</typeparam>
    public interface IBusinessObjectSource<T>
    {
        /// <summary>
        /// Gets a value identifying the business object
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Select a set of features based on <paramref name="box"/>
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        IEnumerable<T> Select(Envelope box);

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        IEnumerable<T> Select(IGeometry geom);

        /// <summary>
        /// Select a business object by its id
        /// </summary>
        /// <param name="id">the id of the business object</param>
        /// <returns></returns>
        T Select(uint id);

        /// <summary>
        /// Update the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The business objects that need to be updated</param>
        void Update(IEnumerable<T> businessObjects);

        /// <summary>
        /// Delete the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The <typeparamref name="T"/>s that need to be deleted</param>
        void Delete(IEnumerable<T> businessObjects);

        /// <summary>
        /// Insert the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The features that need to be inserted</param>
        void Insert(IEnumerable<T> businessObjects);

        /// <summary>
        /// Method to get the geometry of a specific feature
        /// </summary>
        /// <param name="businessObject">The feature</param>
        /// <returns></returns>
        IGeometry GetGeometry(T businessObject);

        /// <summary>
        /// Gets the id of the given business object
        /// </summary>
        /// <param name="businessObject">The business object</param>
        /// <returns>The id</returns>
        uint GetId(T businessObject);
        
        /// <summary>
        /// Gets the number of business objects in the store
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the extents of the 
        /// </summary>
        /// <returns>The extents</returns>
        Envelope GetExtents();
    }
}