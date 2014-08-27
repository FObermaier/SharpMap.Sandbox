// Copyright 2013 - 2014 Felix Obermaier (www.ivv-aachen.de)
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
    public class InMemoryBusinessObjectAccess<T> : BusinessObjectAccessBase<T>
    {
        private readonly Dictionary<uint, T> _businessObjects;

        private readonly string _title;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public InMemoryBusinessObjectAccess()
            :this(typeof(T).Name)
        {}

        /// <summary>
        /// Creates an instance of this class, assigning a <see cref="Title"/>
        /// </summary>
        public InMemoryBusinessObjectAccess(string title)
        {
            _businessObjects = new Dictionary<uint, T>();
            _title = title;
        }
        /// <summary>
        /// Gets a value identifying the business object
        /// </summary>
        public override string Title
        {
            get { return _title; }
        }


        /// <summary>
        /// Select a set of features based on <paramref name="box"/>
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override IEnumerable<T> Select(Envelope box)
        {
            return Select(new GeometryFactory().ToGeometry(box));
        }

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        public override IEnumerable<T> Select(IGeometry geom)
        {
            var prep = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            foreach (T value in _businessObjects.Values)
            {
                var g = _getGeometry(value);
                if (g != null && prep.Intersects(g))
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
        public override T Select(uint id)
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
        public override void Update(IEnumerable<T> features)
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
        public override void Delete(IEnumerable<T> features)
        {
            foreach (T feature in features)
            {
                _businessObjects.Remove(_getId(feature));
            }
            CachedExtents = null;
        }

        /// <summary>
        /// Insert the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be inserted</param>
        public override void Insert(IEnumerable<T> features)
        {
            foreach (T feature in features)
            {
                _businessObjects.Add(_getId(feature), feature);
            }
            CachedExtents = null;
        }

        public override int Count
        {
            get { return _businessObjects.Count; }
        }

        public override Envelope GetExtents()
        {
            return CachedExtents ?? (CachedExtents = ComputeExtents());
        }

        private Envelope ComputeExtents()
        {
            var res = new Envelope();
            foreach (var bo in _businessObjects.Values)
            {
                var g = _getGeometry(bo);
                if (g != null && !g.IsEmpty)
                    res.ExpandToInclude(g.EnvelopeInternal);
            }
            return res;
        }
    }
}