// Copyright 2013-2014 Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Business.MongoDB.
// SharpMap.Business.MongoDB is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Business.MongoDB is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI;
using GeoAPI.Geometries;
using MongoDB.Driver.GeoJsonObjectModel;

namespace SharpMap.Converters
{
    public static class GeoJsonConverter
    {
        public static GeoJsonConverter<GeoJson2DCoordinates> Converter2D
        {
            get { return new GeoJsonConverter<GeoJson2DCoordinates>(To2DCoordinates, From2DCoordinates); }
        }

        public static GeoJsonConverter<GeoJson2DProjectedCoordinates> Converter2DProjected
        {
            get { return new GeoJsonConverter<GeoJson2DProjectedCoordinates>(To2DProjectedCoordinates, From2DProjectedCoordinates); }
        }

        public static GeoJsonConverter<GeoJson2DGeographicCoordinates> Converter2DGeographic
        {
            get { return new GeoJsonConverter<GeoJson2DGeographicCoordinates>(To2DGeographicCoordinates, From2DGeographicCoordinates); }
        }

        public static GeoJsonConverter<GeoJson3DCoordinates> Converter3D
        {
            get { return new GeoJsonConverter<GeoJson3DCoordinates>(To3DCoordinates, From3DCoordinates); }
        }

        public static GeoJsonConverter<GeoJson3DProjectedCoordinates> Converter3DProjected
        {
            get { return new GeoJsonConverter<GeoJson3DProjectedCoordinates>(To3DProjectedCoordinates, From3DProjectedCoordinates); }
        }

        public static GeoJsonConverter<GeoJson3DGeographicCoordinates> Converter3DGeographic
        {
            get { return new GeoJsonConverter<GeoJson3DGeographicCoordinates>(To3DGeographicCoordinates, From3DGeographicCoordinates); }
        }

        private static GeoJson2DCoordinates To2DCoordinates(Coordinate c)
        {
            return new GeoJson2DCoordinates(c.X, c.Y);
        }

        private static Coordinate From2DCoordinates(GeoJson2DCoordinates c)
        {
            return new Coordinate(c.X, c.Y);
        }

        private static GeoJson2DGeographicCoordinates To2DGeographicCoordinates(Coordinate self)
        {
            return new GeoJson2DGeographicCoordinates(self.Y, self.X);
        }

        private static Coordinate From2DGeographicCoordinates(GeoJson2DGeographicCoordinates c)
        {
            return new Coordinate(c.Latitude, c.Longitude);
        }

        private static GeoJson2DProjectedCoordinates To2DProjectedCoordinates(Coordinate self)
        {
            return new GeoJson2DProjectedCoordinates(self.X, self.Y);
        }

        private static Coordinate From2DProjectedCoordinates(GeoJson2DProjectedCoordinates self)
        {
            return new Coordinate(self.Easting, self.Northing);
        }

        private static GeoJson3DCoordinates To3DCoordinates(Coordinate self)
        {
            return new GeoJson3DCoordinates(self.X, self.Y, self.Z);
        }

        private static Coordinate From3DCoordinates(GeoJson3DCoordinates self)
        {
            return new Coordinate(self.X, self.Y, self.Z);
        }

        private static GeoJson3DGeographicCoordinates To3DGeographicCoordinates(Coordinate self)
        {
            return new GeoJson3DGeographicCoordinates(self.Y, self.X, self.Z);
        }

        private static Coordinate From3DGeographicCoordinates(GeoJson3DGeographicCoordinates self)
        {
            return new Coordinate(self.Longitude, self.Latitude, self.Altitude);
        }

        private static GeoJson3DProjectedCoordinates To3DProjectedCoordinates(Coordinate self)
        {
            return new GeoJson3DProjectedCoordinates(self.X, self.Y, self.Z);
        }

        private static Coordinate From3DProjectedCoordinates(GeoJson3DProjectedCoordinates self)
        {
            return new Coordinate(self.Easting, self.Northing, self.Altitude);
        }

        
    }

    public class GeoJsonConverter<T>
        where T:GeoJsonCoordinates
    {
        /// <summary>
        /// Method definition to create <see cref="GeoJsonCoordinates"/> from an <see cref="Coordinate"/>
        /// </summary>
        /// <param name="coordinate">The coordinate sequence</param>
        /// <returns>A coordinate</returns>
        public delegate T FromCoordinateHandler(Coordinate coordinate);

        /// <summary>
        /// Method definition to create <see cref="Coordinate"/> from an <see cref="GeoJsonCoordinates"/>
        /// </summary>
        /// <param name="coordinate">The coordinate sequence</param>
        /// <returns>A coordinate</returns>
        public delegate Coordinate ToCoordinateHandler(T coordinate);

        private readonly FromCoordinateHandler _fromHandler;
        private readonly ToCoordinateHandler _toHandler;
        private readonly IGeometryFactory _factory;
        private readonly GeoJsonCoordinateReferenceSystem _crs;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="fromHandler">The handler to convert the coordinates</param>
        /// <param name="toHandler">The handler to convert the coordinates</param>
        public GeoJsonConverter(FromCoordinateHandler fromHandler, ToCoordinateHandler toHandler)
            :this(fromHandler, toHandler, GeometryServiceProvider.Instance.CreateGeometryFactory(0), null)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="fromHandler">The handler to convert the coordinates</param>
        /// <param name="toHandler">The handler to convert the coordinates</param>
        /// <param name="factory">The geometry factory to use</param>
        /// <param name="crs">The coordinate reference system to assign or verify</param>
        public GeoJsonConverter(FromCoordinateHandler fromHandler, ToCoordinateHandler toHandler, 
            IGeometryFactory factory, GeoJsonCoordinateReferenceSystem crs)
        {
            _fromHandler = fromHandler;
            _toHandler = toHandler;
            _factory = factory;
            _crs = crs;
        }

        private void CheckInput(IGeometry geometry, OgcGeometryType type)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");
            if (geometry.OgcGeometryType != type)
                throw new ArgumentOutOfRangeException("geometry is not of desired type");
            if (geometry.SRID != _factory.SRID)
                throw new ArgumentException("Wrong SRID", "geometry");
        }

        private void CheckInput(GeoJsonGeometry<T> geometry, GeoJsonObjectType type)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");
            if (geometry.Type != type)
                throw new ArgumentOutOfRangeException("geometry is not of desired type");
            if (geometry.CoordinateReferenceSystem != _crs)
                throw new ArgumentException("Wrong CoordinateReferenceSystem", "geometry");
        }

        private GeoJsonObjectArgs<T> CreateObjectArgs(Envelope env)
        {
            return new GeoJsonObjectArgs<T>
            {
                CoordinateReferenceSystem = _crs,
                BoundingBox = ToBoundingBox(env)
            };
        }

        #region To GeoJson
        public GeoJsonPoint<T> ToPoint(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.Point);
            var coord = geometry.Coordinate;
            return new GeoJsonPoint<T>(CreateObjectArgs(geometry.EnvelopeInternal), _fromHandler(coord));
        }

        private GeoJsonLineString<T> ToLineString(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.LineString);
            var linestring = (ILineString)geometry;
            var list = new GeoJsonLineStringCoordinates<T>(ToPositions(linestring.CoordinateSequence));
            return new GeoJsonLineString<T>(CreateObjectArgs(geometry.EnvelopeInternal), list);
        }

        public GeoJsonPolygon<T> ToPolygon(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.Polygon);
            var polygon = (IPolygon)geometry;
            return new GeoJsonPolygon<T>(CreateObjectArgs(geometry.EnvelopeInternal), ToPolygonCoordinates(polygon));
        }

        public GeoJsonMultiPoint<T> ToMultiPoint(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.MultiPoint);
            var multiPoint = (IMultiPoint)geometry;

            return new GeoJsonMultiPoint<T>(CreateObjectArgs(geometry.EnvelopeInternal),
                new GeoJsonMultiPointCoordinates<T>(ToPositions(multiPoint.Coordinates)));
        }

        public GeoJsonMultiLineString<T> ToMultiLineString(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.LineString);
            var mlp = new GeoJsonMultiLineStringCoordinates<T>(
                ToLineStringCoordinates((IMultiLineString)geometry));
            return new GeoJsonMultiLineString<T>(CreateObjectArgs(geometry.EnvelopeInternal), mlp);
        }

        public GeoJsonMultiPolygon<T> ToMultiPolygon(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.MultiPolygon);

            var pcl = new List<GeoJsonPolygonCoordinates<T>>(geometry.NumGeometries);
            for (var i = 0; i < geometry.NumGeometries; i++)
            {
                var p = (IPolygon)geometry.GetGeometryN(i);
                pcl.Add(ToPolygonCoordinates(p));
            }
            return new GeoJsonMultiPolygon<T>(CreateObjectArgs(geometry.EnvelopeInternal),
                new GeoJsonMultiPolygonCoordinates<T>(pcl));
        }

        public GeoJsonGeometryCollection<T> ToGeometryCollection(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.GeometryCollection);

            var list = new List<GeoJsonGeometry<T>>();
            for (var i = 0; i < geometry.NumGeometries; i++)
                list.Add(ToGeometry(geometry.GetGeometryN(i)));
            return new GeoJsonGeometryCollection<T>(CreateObjectArgs(geometry.EnvelopeInternal), list);
        }

        public GeoJsonGeometry<T> ToGeometry(IGeometry geometry)
        {
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    return ToPoint(geometry);
                case OgcGeometryType.LineString:
                    return ToLineString(geometry);
                case OgcGeometryType.Polygon:
                    return ToPolygon(geometry);
                case OgcGeometryType.MultiPoint:
                    return ToMultiPoint(geometry);
                case OgcGeometryType.MultiLineString:
                    return ToMultiLineString(geometry);
                case OgcGeometryType.MultiPolygon:
                    return ToMultiPolygon(geometry);
                case OgcGeometryType.GeometryCollection:
                    return ToGeometryCollection(geometry);
                default:
                    throw new ArgumentException("geometry");
            }
        }

        #region GeoJsonCoordinates utilities
        private IEnumerable<T> ToPositions(ICoordinateSequence sequence)
        {
            var res = new List<T>(sequence.Count);
            for (var i = 0; i < sequence.Count; i++)
                res.Add(_fromHandler(sequence.GetCoordinate(i)));
            return res;
        }

        private GeoJsonLinearRingCoordinates<T> ToLinearRing(ICoordinateSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException();

            return new GeoJsonLinearRingCoordinates<T>(ToPositions(sequence));
        }

        private GeoJsonPolygonCoordinates<T> ToPolygonCoordinates(IPolygon polygon)
        {
            var shell = ToLinearRing(polygon.Shell.CoordinateSequence);
            var holes = new List<GeoJsonLinearRingCoordinates<T>>(polygon.NumInteriorRings);
            for (var i = 0; i < polygon.NumInteriorRings; i++)
            {
                var ring = polygon.GetInteriorRingN(i);
                holes.Add(ToLinearRing(ring.CoordinateSequence));
            }

            return new GeoJsonPolygonCoordinates<T>(shell, holes);
        }

        private IEnumerable<T> ToPositions(IEnumerable<Coordinate> coordinates)
        {
            return coordinates.Select(coordinate => _fromHandler(coordinate));
        }

        private IEnumerable<GeoJsonLineStringCoordinates<T>> ToLineStringCoordinates(IMultiLineString geometry)
        {
            for (var i = 0; i < geometry.NumGeometries; i++)
            {
                var ls = (ILineString) geometry.GetGeometryN(i);
                yield return new GeoJsonLineStringCoordinates<T>(ToPositions(ls.CoordinateSequence));
            }
        }
        #endregion
        #endregion

        #region Envelope/BoundingBox

        public Envelope ToBoundingBox(GeoJsonBoundingBox<T> box)
        {
            return new Envelope(_toHandler(box.Min), _toHandler(box.Max));
        }

        public GeoJsonBoundingBox<T> ToBoundingBox(Envelope box)
        {
            var min = _fromHandler(new Coordinate(box.MinX, box.MinY));
            var max = _fromHandler(new Coordinate(box.MaxX, box.MaxY));
            return new GeoJsonBoundingBox<T>(min, max);
        }

        public GeoJsonGeometry<T> ToPolygon(Envelope geometry)
        {
            return ToPolygon(_factory.ToGeometry(geometry));
        }

        #endregion

        #region To GeoAPI.Geometries

        public IGeometry ToGeometry(GeoJsonGeometry<T> bsonGeometry)
        {
            switch (bsonGeometry.Type)
            {
                case GeoJsonObjectType.Point:
                    return ToPoint((GeoJsonPoint<T>) bsonGeometry);
                case GeoJsonObjectType.LineString:
                    return ToLineString((GeoJsonLineString<T>)bsonGeometry);
                case GeoJsonObjectType.Polygon:
                    return ToPolygon((GeoJsonPolygon<T>)bsonGeometry);
                case GeoJsonObjectType.MultiPoint:
                    return ToMultiPoint((GeoJsonMultiPoint<T>)bsonGeometry);
                case GeoJsonObjectType.MultiLineString:
                    return ToMultiLineString((GeoJsonMultiLineString<T>)bsonGeometry);
                case GeoJsonObjectType.MultiPolygon:
                    return ToMultiPolygon((GeoJsonMultiPolygon<T>)bsonGeometry);
                case GeoJsonObjectType.GeometryCollection:
                    return ToGeometryCollection((GeoJsonGeometryCollection<T>)bsonGeometry);
                default:
                    throw new ArgumentException("bsonGeometry");
            }
        }

        public IGeometry ToGeometryCollection(GeoJsonGeometryCollection<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.GeometryCollection);
            var geometries = bsonGeometry.Geometries;
            var col = new IGeometry[geometries.Count];
            for (var i = 0; i < geometries.Count; i++)
                col[i] = ToGeometry(geometries[i]);
            return GeometryServiceProvider.Instance.CreateGeometryFactory().CreateGeometryCollection(col);
        }

        public IGeometry ToMultiPolygon(GeoJsonMultiPolygon<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiPolygon);
            var polygons = bsonGeometry.Coordinates.Polygons;
            var polys = new IPolygon[polygons.Count];
            for (var i = 0; i < polygons.Count; i++)
                polys[i] = ToPolygon(polygons[i]);
            return _factory.CreateMultiPolygon(polys);
        }

        public IGeometry ToMultiLineString(GeoJsonMultiLineString<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiLineString);
            var linestrings = bsonGeometry.Coordinates.LineStrings;
            var lines = new ILineString[linestrings.Count];
            for (var i = 0; i < linestrings.Count; i++)
                lines[i] = ToLineString(linestrings[i]);
            return _factory.CreateMultiLineString(lines);
        }

        public IGeometry ToMultiPoint(GeoJsonMultiPoint<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiPoint);
            var coords = ToCoordinateArray(bsonGeometry.Coordinates.Positions);
            return _factory.CreateMultiPoint(coords);
        }

        public IGeometry ToPoint(GeoJsonPoint<T> point)
        {
            CheckInput(point, GeoJsonObjectType.Point);
            return _factory.CreatePoint(_toHandler(point.Coordinates));
        }

        public IGeometry ToLineString(GeoJsonLineString<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.LineString);
            return ToLineString(bsonGeometry.Coordinates);
        }

        internal IGeometry ToPolygon(GeoJsonPolygon<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.Polygon);
            return ToPolygon(bsonGeometry.Coordinates);
        }

        private ILineString ToLineString(GeoJsonLineStringCoordinates<T> coordinates)
        {
            var coords = ToCoordinateArray(coordinates.Positions);
            return _factory.CreateLineString(coords);
        }

        private Coordinate[] ToCoordinateArray(ReadOnlyCollection<T> coordinates)
        {
            var res = new Coordinate[coordinates.Count];
            for (var i = 0; i < coordinates.Count; i++)
                res[i] = _toHandler(coordinates[i]);
            return res;
        }

        private IPolygon ToPolygon(GeoJsonPolygonCoordinates<T> coordinates)
        {
            var shell = _factory.CreateLinearRing(ToCoordinateArray(coordinates.Exterior.Positions));
            if (coordinates.Holes.Count > 0)
            {
                var holes = new ILinearRing[coordinates.Holes.Count];
                for (var i = 0; i < coordinates.Holes.Count; i++)
                    holes[i] = _factory.CreateLinearRing(ToCoordinateArray(coordinates.Holes[i].Positions));
                return _factory.CreatePolygon(shell, holes);
            }
            return _factory.CreatePolygon(shell);
        }

        #endregion
    }
}