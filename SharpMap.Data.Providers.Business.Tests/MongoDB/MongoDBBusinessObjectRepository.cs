using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GeoJsonObjectModel;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    public class MongoDBBusinessObjectRepository<T> : BusinessObjectAccessBase<T>
    {
        private readonly MongoCollection<T> _collection;

        private MongoDBBusinessObjectRepository()
        {
            BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap());
        }

        public MongoDBBusinessObjectRepository(MongoClientSettings settings, string database, string collection)
            : this(new MongoClient(settings), database, collection)
        {
        }

        public MongoDBBusinessObjectRepository(string connectionString, string database, string collection)
            : this(new MongoClient(connectionString), database, collection)
        {
        }

        private MongoDBBusinessObjectRepository(MongoClient mongoClient, string database, string collection)
            :this()
        {
            var mongoDatabase = mongoClient.GetServer().GetDatabase(database);
            _collection = mongoDatabase.GetCollection<T>(collection);
        }

        #region GeoJsonHelper
        private static GeoJsonBoundingBox<GeoJson2DCoordinates> ToBoundingBox(Envelope env)
        {
            return new GeoJsonBoundingBox<GeoJson2DCoordinates>(
                new GeoJson2DCoordinates(env.MinX, env.MinY),
                new GeoJson2DCoordinates(env.MaxX, env.MinX));
        }

        private static GeoJsonPolygon<GeoJson2DCoordinates> ToPolygon(Envelope env)
        {
            return new GeoJsonPolygon<GeoJson2DCoordinates>(
                new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(
                    new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(
                        new[]
                        {
                            new GeoJson2DCoordinates(env.MinX, env.MinY),
                            new GeoJson2DCoordinates(env.MinX, env.MaxY),
                            new GeoJson2DCoordinates(env.MaxX, env.MinX),
                            new GeoJson2DCoordinates(env.MaxX, env.MinY),
                            new GeoJson2DCoordinates(env.MinX, env.MinY),
                        })));
        }
        #endregion
        public override IEnumerable<T> Select(Envelope box)
        {
            var query = Query<T>.GeoIntersects(t => ToBoundingBox(GetGeometry(t).EnvelopeInternal),
                ToPolygon(box));
            
            return _collection.Find(query);
        }

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