using System;
using GeoAPI.Geometries;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    public class BsonGeometrySerializer : BsonBaseSerializer
    {
        private static readonly Int32Serializer _intSerializer = new Int32Serializer();
        private static readonly DoubleSerializer _doubleSerializer = new DoubleSerializer();
        private static readonly CoordinateSerializer _coordinateSerializer = new CoordinateSerializer();
        private static readonly CoordinateSequenceSerializer _coordinateSequenceSerializer = 
            new CoordinateSequenceSerializer(GeoAPI.GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory);

        public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }

            var srid = DeserializeSrid(bsonReader);
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(srid);
            switch (DeserializeType(bsonReader))
            {
                case OgcGeometryType.Point:
                    return DeserializePoint(bsonReader, nominalType, options, factory);
                case OgcGeometryType.LineString:
                    return DeserializeLineString(bsonReader, nominalType, options, factory);
                case OgcGeometryType.Polygon:
                    return DeserializePolygon(bsonReader, nominalType, options, factory);
                case OgcGeometryType.MultiPoint:
                    return DeserializeMultiPoint(bsonReader, nominalType, options, factory);
                case OgcGeometryType.MultiLineString:
                    return DeserializeMultiLineString(bsonReader, nominalType, options, factory);
                case OgcGeometryType.MultiPolygon:
                    return DeserializeMultiPolygon(bsonReader, nominalType, options, factory);
                case OgcGeometryType.GeometryCollection:
                    return DeserializeGeometryCollection(bsonReader, nominalType, options, factory);
                default:
                    throw new BsonSerializationException();
            }
        }

        protected OgcGeometryType DeserializeType(BsonReader reader)
        {
            var bm = reader.GetBookmark();
            var type = reader.FindStringElement("type");

            OgcGeometryType res;
            switch (type)
            {
                case "Point":
                    res = OgcGeometryType.Point;
                    break;
                case "LineString":
                    res = OgcGeometryType.LineString;
                    break;
                case "Polygon":
                    res = OgcGeometryType.Polygon;
                    break;
                case "MultiPoint":
                    res = OgcGeometryType.MultiPoint;
                    break;
                case "MultiLineString":
                    res = OgcGeometryType.MultiLineString;
                    break;
                case "MultiPolygon":
                    res = OgcGeometryType.MultiPolygon;
                    break;
                case "GeometryCollection":
                    res = OgcGeometryType.GeometryCollection;
                    break;
                default:

                    throw new BsonSerializationException(string.IsNullOrEmpty(type)
                        ? "Type information not found!"
                        :    string.Format("Unhandled type: '{0}'!", type));
            }

            reader.ReturnToBookmark(bm);
            return res;
        }

        #region Deserialization

        protected int DeserializeSrid(BsonReader reader)
        {
            var bm = reader.GetBookmark();
            var hasCrs = reader.FindElement("crs");
            if (hasCrs)
            {
                switch (reader.GetCurrentBsonType())
                {
                    case BsonType.String:

                }
                //reader.GetCurrentBsonType()
            }

            reader.ReturnToBookmark(bm);
        }

        protected IPoint DeserializePoint(BsonReader reader, Type nominalType, IBsonSerializationOptions options, IGeometryFactory factory)
        {
            var seq = (ICoordinateSequence) _coordinateSequenceSerializer.Deserialize(reader, nominalType, options);
            return factory.CreatePoint(seq);
        }

        protected object DeserializeLineString(BsonReader reader, Type nominalType, IBsonSerializationOptions options, IGeometryFactory factory, bool asRing = false)
        {
            var seq = (ICoordinateSequence)_coordinateSequenceSerializer.Deserialize(reader, nominalType, options);
            return asRing ? factory.CreateLinearRing(seq) : factory.CreateLineString(seq);
        }

        protected object DeserializePolygon(BsonReader reader, Type nominalType, IBsonSerializationOptions options, IGeometryFactory factory)
        {
            
            var seq = (ICoordinateSequence)_coordinateSequenceSerializer.Deserialize(reader, nominalType, options);
            return factory.CreatePoint(seq);
        }

        #endregion

        
        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            base.Serialize(bsonWriter, nominalType, value, options);
        }

        #region nested class

        private class CoordinateSerializer : BsonBaseSerializer
        {
            private int _numOrdinates = 2;
            public CoordinateSerializer()
                :this(2)
            {}

            public CoordinateSerializer(int numOrdinates)
            {
                if (numOrdinates < 2) numOrdinates = 2;
                if (numOrdinates > 3) numOrdinates = 3;

                _numOrdinates = numOrdinates;
            }

            public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
            {
                if (bsonReader.GetCurrentBsonType() == BsonType.Null)
                {
                    bsonReader.ReadNull();
                    return null;
                }

                bsonReader.ReadStartArray();
                var c = new Coordinate((double)_doubleSerializer.Deserialize(bsonReader, typeof(double), null),
                    (double)_doubleSerializer.Deserialize(bsonReader, typeof(double), null));
                if (_numOrdinates > 2)
                    c.Z = (double) _doubleSerializer.Deserialize(bsonReader, typeof (double), null);

                bsonReader.ReadEndArray();

                return c;
            }

            public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
            {
                if (value == null)
                {
                    bsonWriter.WriteNull();
                    return;
                }

                var c = (Coordinate) value;

                bsonWriter.WriteStartArray();
                bsonWriter.WriteDouble(c.X);
                bsonWriter.WriteDouble(c.Y);
                if (_numOrdinates>2)
                    bsonWriter.WriteDouble(c.Z);

                bsonWriter.WriteEndArray();
            }
        }

        private class CoordinateSequenceSerializer : BsonBaseSerializer
        {
            private readonly ICoordinateSequenceFactory _coordinateSequenceFactory;

            public CoordinateSequenceSerializer(ICoordinateSequenceFactory coordinateSequenceFactory)
            {
                _coordinateSequenceFactory = coordinateSequenceFactory;
            }

            public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
            {
                if (bsonReader.GetCurrentBsonType() == BsonType.Null)
                {
                    bsonReader.ReadNull();
                    return null;
                }

                bsonReader.ReadStartArray();
                var c = new Coordinate((double)_doubleSerializer.Deserialize(bsonReader, typeof(double), null),
                    (double)_doubleSerializer.Deserialize(bsonReader, typeof(double), null));
                if (_numOrdinates > 2)
                    c.Z = (double)_doubleSerializer.Deserialize(bsonReader, typeof(double), null);

                bsonReader.ReadEndArray();

                return c;
            }

            public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
            {
                if (value == null)
                {
                    bsonWriter.WriteNull();
                    return;
                }

                var c = (ICoordinateSequence)value;

                bsonWriter.WriteStartArray();
                bsonWriter.WriteDouble(c.X);
                bsonWriter.WriteDouble(c.Y);
                if (_numOrdinates > 2)
                    bsonWriter.WriteDouble(c.Z);

                bsonWriter.WriteEndArray();
            }
        }

        #endregion

    }
}