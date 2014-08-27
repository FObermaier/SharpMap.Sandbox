using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BruTile.Wmts.Generated;
using GeoAPI;
using GeoAPI.Geometries;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

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

    public class GeoJsonConveterUtility<T>
    {
        
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
        {
            _fromHandler = fromHandler;
            _toHandler = toHandler;
            _factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            //_crs = new GeoJsonNamedCoordinateReferenceSystem("EPSG:4326");
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

        public GeoJsonPoint<T> ToPoint(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.Point);
            var coord = geometry.Coordinate;
            return new GeoJsonPoint<T>(_fromHandler(coord));
        }

        public IPoint ToPoint(GeoJsonPoint<T> point)
        {
            CheckInput(point, GeoJsonObjectType.Point);
            return _factory.CreatePoint(_toHandler(point.Coordinates));
        }

        private IEnumerable<T> ToPositions(ICoordinateSequence sequence)
        {
            var res = new List<T>(sequence.Count);
            for (var i = 0; i < sequence.Count; i++)
                res.Add(_fromHandler(sequence.GetCoordinate(i)));
            return res;
        }

        private GeoJsonLineString<T> ToLineString(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.LineString);
            var linestring = (ILineString) geometry;
            var list = new GeoJsonLineStringCoordinates<T>(ToPositions(linestring.CoordinateSequence));
            return new GeoJsonLineString<T>(list);
        }


        private GeoJsonLinearRingCoordinates<T> ToLinearRing(ICoordinateSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException();

            return new GeoJsonLinearRingCoordinates<T>(ToPositions(sequence));
        }

        public GeoJsonPolygon<T> ToPolygon(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.Polygon);
            var polygon = (IPolygon) geometry;
            return new GeoJsonPolygon<T>(ToPolygonCoordinates(polygon));
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
        public GeoJsonMultiPoint<T> ToMultiPoint(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.MultiPoint);
            var multiPoint = (IMultiPoint) geometry;
            
            return new GeoJsonMultiPoint<T>(new GeoJsonMultiPointCoordinates<T>(ToPositions(multiPoint.Coordinates)));
        }

        private IEnumerable<T> ToPositions(IEnumerable<Coordinate> coordinates)
        {
            return coordinates.Select(coordinate => _fromHandler(coordinate));
        }

        public GeoJsonMultiLineString<T> ToMultiLineString(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.LineString);
            var mlp = new GeoJsonMultiLineStringCoordinates<T>(
                ToLineStringCoordinates((IMultiLineString) geometry));
            return new GeoJsonMultiLineString<T>(mlp);
        }

        private IEnumerable<GeoJsonLineStringCoordinates<T>> ToLineStringCoordinates(IMultiLineString geometry)
        {
            for (var i = 0; i < geometry.NumGeometries; i++)
            {
                var ls = (ILineString) geometry.GetGeometryN(i);
                yield return new GeoJsonLineStringCoordinates<T>(ToPositions(ls.CoordinateSequence));
            }
        }

        public GeoJsonMultiPolygon<T> ToMultiPolygon(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.MultiPolygon);

            var pcl = new List<GeoJsonPolygonCoordinates<T>>(geometry.NumGeometries);
            for (var i = 0; i < geometry.NumGeometries; i++)
            {
                var p = (IPolygon) geometry.GetGeometryN(i);
                pcl.Add(ToPolygonCoordinates(p));
            }
            return new GeoJsonMultiPolygon<T>(new GeoJsonMultiPolygonCoordinates<T>(pcl));
        }

        public GeoJsonGeometryCollection<T> ToGeometryCollection(IGeometry geometry)
        {
            CheckInput(geometry, OgcGeometryType.GeometryCollection);

            var list = new List<GeoJsonGeometry<T>>();
            for (var i = 0; i < geometry.NumGeometries; i++)
                list.Add(ToGeometry(geometry.GetGeometryN(i)));
            return new GeoJsonGeometryCollection<T>(list);
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

        private IGeometry ToGeometryCollection(GeoJsonGeometryCollection<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.GeometryCollection);
            var geometries = bsonGeometry.Geometries;
            var col = new IGeometry[geometries.Count];
            for (var i = 0; i < geometries.Count; i++)
                col[i] = ToGeometry(geometries[i]);
            return GeometryServiceProvider.Instance.CreateGeometryFactory().CreateGeometryCollection(col);
        }

        private IGeometry ToMultiPolygon(GeoJsonMultiPolygon<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiPolygon);
            var polygons = bsonGeometry.Coordinates.Polygons;
            var polys = new IPolygon[polygons.Count];
            for (var i = 0; i < polygons.Count; i++)
                polys[i] = ToPolygon(polygons[i]);
            return _factory.CreateMultiPolygon(polys);
        }

        private IGeometry ToMultiLineString(GeoJsonMultiLineString<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiLineString);
            var linestrings = bsonGeometry.Coordinates.LineStrings;
            var lines = new ILineString[linestrings.Count];
            for (var i = 0; i < linestrings.Count; i++)
                lines[i] = ToLineString(linestrings[i]);
            return _factory.CreateMultiLineString(lines);
        }

        private IGeometry ToMultiPoint(GeoJsonMultiPoint<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.MultiPoint);
            var coords = ToCoordinateArray(bsonGeometry.Coordinates.Positions);
            return _factory.CreateMultiPoint(coords);
        }

        private IGeometry ToLineString(GeoJsonLineString<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.LineString);
            return ToLineString(bsonGeometry.Coordinates);
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

        private IGeometry ToPolygon(GeoJsonPolygon<T> bsonGeometry)
        {
            CheckInput(bsonGeometry, GeoJsonObjectType.Polygon);
            return ToPolygon(bsonGeometry.Coordinates);
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

        public GeoJsonGeometry<T> ToPolygon(Envelope geometry)
        {
            return ToPolygon(_factory.ToGeometry(geometry));
        }
    }
}