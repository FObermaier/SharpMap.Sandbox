using System.Net.Mail;
using System.Reflection;
using GeoAPI.Geometries;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GeoJsonObjectModel;
using SharpMap.Converters;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    public class PoIRepository : MongoDBBusinessObjectRepository<PoI, GeoJson2DCoordinates>
    {
        public PoIRepository(GeoJsonConverter<GeoJson2DCoordinates> converter, MongoClientSettings settings, string database, string collection) 
            : base(converter, settings, database, collection)
        {
        }

        public PoIRepository(GeoJsonConverter<GeoJson2DCoordinates> converter, string connectionString, string database, string collection) 
            : base(converter, connectionString, database, collection)
        {
        }


        protected override IMongoQuery BuildEnvelopeQuery(Envelope box)
        {
            return Query<PoI>.GeoIntersects(t => t.BsonGeometry, Converter.ToPolygon(box));        }
    }
    public class PoI//<T> where T : GeoJsonCoordinates
    {
        private IGeometry _geometry;
        public static GeoJsonConverter<GeoJson2DCoordinates> Converter;

        [BusinessObjectIdentifier]
        [BsonId(IdGenerator = typeof (ZeroIdChecker<uint>))]
        public uint Id { get; set; }

        [BusinessObjectAttribute(Ordinal = 1)]
        public string Label { get; set; }

        [BusinessObjectGeometry]
        [BsonIgnore]
        public IGeometry Geometry
        {
            get { return _geometry ?? (_geometry = Converter.ToPoint(BsonGeometry)); }
            set
            {
                _geometry = value;
                BsonGeometry = Converter.ToPoint(value);
            }
        }

        [BusinessObjectAttribute(Ignore = true)]
        public GeoJsonPoint<GeoJson2DCoordinates> BsonGeometry { get; set; }

    }
}