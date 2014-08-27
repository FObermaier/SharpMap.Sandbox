using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Common.Logging.Configuration;
using GeoAPI.Geometries;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;
using SharpMap.Converters;

namespace SharpMap.Data.Providers.Business
{
    public abstract class MongoDBBusinessObjectRepository<T, TCoordinate> : BusinessObjectAccessBase<T>
        where TCoordinate: GeoJsonCoordinates
    {
        private readonly MongoCollection<T> _collection;
        protected readonly GeoJsonConverter<TCoordinate> Converter;

        private MongoDBBusinessObjectRepository(GeoJsonConverter<TCoordinate> converter)
        {
            Converter = converter;
            if (!BsonClassMap.IsClassMapRegistered(typeof (T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    //cm.GetMemberMap("BsonGeometry").SetSerializer(new GeoJson2DCoordinatesSerializer());
                }
                    );
            }
        }

        public MongoDBBusinessObjectRepository(GeoJsonConverter<TCoordinate> converter, MongoClientSettings settings, string database, string collection)
            : this(converter,new MongoClient(settings), database, collection)
        {
        }

        public MongoDBBusinessObjectRepository(GeoJsonConverter<TCoordinate> converter, string connectionString, string database, string collection)
            : this(converter,new MongoClient(connectionString), database, collection)
        {
        }

        private MongoDBBusinessObjectRepository(GeoJsonConverter<TCoordinate> converter, MongoClient mongoClient, string database, string collection)
            :this(converter)
        {
            var mongoDatabase = mongoClient.GetServer().GetDatabase(database);
            _collection = mongoDatabase.GetCollection<T>(collection);
        }

        public override Envelope GetExtents()
        {
            if (CachedExtents != null)
                return CachedExtents;

            var extent = new Envelope();
            foreach (var t in _collection.FindAll())
                extent.ExpandToInclude(GetGeometry(t).EnvelopeInternal);
            return CachedExtents = extent;
        }

        public override IEnumerable<T> Select(Envelope box)
        {
            return _collection.Find(BuildEnvelopeQuery(box));
        }

        /// <summary>
        /// Method to create the mongo query for the bounding box search
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        protected abstract IMongoQuery BuildEnvelopeQuery(Envelope box);
 
        public override IEnumerable<T> Select(IGeometry geom)
        {
            var candidates = Select(geom.EnvelopeInternal);
            var p = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            return candidates.Where(candidate => p.Intersects(GetGeometry(candidate)));
        }

        public override T Select(uint id)
        {
            return _collection.FindOneById(id);
        }

        public override void Update(IEnumerable<T> businessObjects)
        {
            foreach (var businessObject in businessObjects)
            {
                _collection.Save(businessObject);
            }
        }

        public override void Delete(IEnumerable<T> businessObjects)
        {
            foreach (var businessObject in businessObjects)
            {
                var query = Query<T>.EQ(t  => GetId(t), GetId(businessObject));
                _collection.Remove(query);
            }
        }

        public override void Insert(IEnumerable<T> businessObjects)
        {
            _collection.InsertBatch(businessObjects);
        }

        public override int Count
        {
            get { return (int)_collection.Count(); }
        }
    }
}