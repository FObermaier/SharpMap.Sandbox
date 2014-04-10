using System.Net.Mail;
using System.Reflection;
using GeoAPI.Geometries;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    public class PoI<TCoordinate> where TCoordinate : GeoJsonCoordinates
    {
        private IGeometry _geometry;

        [BusinessObjectIdentifier]
        [BsonId(IdGenerator = typeof (ZeroIdChecker<uint>))]
        public uint Id { get; set; }

        [BusinessObjectAttribute(Ordinal = 1)]
        public string Label { get; set; }

        [BusinessObjectGeometry]
        [BsonSerializer(typeof(BsonGeometrySerializer))]
        public IGeometry Geometry { get; set; }

    }
}