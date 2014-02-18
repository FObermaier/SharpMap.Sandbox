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

using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Quick and dirty implementation of an in-memory business object store
    /// </summary>
    /// <typeparam name="T">The type of the business object</typeparam>
    internal class InMemoryBusinessObjectAccess<T> : IBusinessObjectSource<T>
    {
        private readonly Dictionary<uint, T> _businessObjects;

        private static readonly TypeUtility<T>.MemberGetDelegate<uint> _getId;
        private static readonly TypeUtility<T>.MemberGetDelegate<IGeometry> _getGeometry;
        private Envelope _extents;

        /// <summary>
        /// Static constructor
        /// </summary>
        static InMemoryBusinessObjectAccess()
        {
            _getId = TypeUtility<T>.GetMemberGetDelegate<uint>(typeof(BusinessObjectIdentifierAttribute));
            _getGeometry = TypeUtility<T>.GetMemberGetDelegate<IGeometry>(typeof(BusinessObjectGeometryAttribute));
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public InMemoryBusinessObjectAccess()
        {
            _businessObjects = new Dictionary<uint, T>();
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="features">The features to insert</param>
        public InMemoryBusinessObjectAccess(IEnumerable<T> features)
            :this()
        {
            Insert(features);
            Title = typeof (T).Name;
        }

        /// <summary>
        /// Gets a value identifying the business object
        /// </summary>
        public string Title { get; private set; }


        /// <summary>
        /// Select a set of features based on <paramref name="box"/>
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public IEnumerable<T> Select(Envelope box)
        {
            return Select(new GeometryFactory().ToGeometry(box));
        }

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        public IEnumerable<T> Select(IGeometry geom)
        {
            var prep = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            foreach (T value in _businessObjects.Values)
            {
                var g = _getGeometry(value);
                if (prep.Intersects(g))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Select a feature by its id
        /// </summary>
        /// <param name="id">the id of the feature</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public T Select(uint id)
        {
            T res;
            if (_businessObjects.TryGetValue(id, out res))
                return res;
            throw new ArgumentException("No feature with this id", "id");
        }

        /// <summary>
        /// Update the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be updated</param>
        public void Update(IEnumerable<T> features)
        {
            Delete(features);
            foreach (T feature in features)
            {
                _businessObjects.Add(_getId(feature), feature);
            }
        }

        /// <summary>
        /// Delete the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be deleted</param>
        public void Delete(IEnumerable<T> features)
        {
            foreach (T feature in features)
            {
                _businessObjects.Remove(_getId(feature));
            }
        }

        /// <summary>
        /// Insert the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be inserted</param>
        public void Insert(IEnumerable<T> features)
        {
            _extents = _extents ?? new Envelope();
            foreach (T feature in features)
            {
                _businessObjects.Add(_getId(feature), feature);
                var g = _getGeometry(feature);
                _extents.ExpandToInclude(g.EnvelopeInternal);
            }
        }

        public IGeometry GetGeometry(T feature)
        {
            return _getGeometry(feature);
        }

        public uint GetId(T feature)
        {
            return _getId(feature);
        }

        public int Count
        {
            get { return _businessObjects.Count; }
        }

        public Envelope GetExtents()
        {
            return _extents;
        }
    }
}