using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using NetTopologySuite.Utilities;
using NHibernate.Linq;
using NUnit.Framework;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    public class PoI
    {
        [BusinessObjectIdentifier]
        [BsonId(IdGenerator = typeof(ZeroIdChecker<uint>))]
        uint Id { get; set; }

        [BusinessObjectAttribute(Ordinal = 1)]
        string Label { get; set; }

        [BusinessObjectGeometry]
        [BsonSerializer(typeof(NetTopologySuite.IO.GeoJsonSerializer))]
        IGeometry Geometry { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MongoDbTests
    {
        private global::MongoDB.Driver.MongoClient _client;

        [TestFixtureSetUp]
        public void SetUp()
        {
            try
            {
                _client = new global::MongoDB.Driver.MongoClient("mongodb://localhost");
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Creation of MongoClient failed", ex);
            }

            var server = _client.GetServer();
            if (server.BuildInfo.Version < new Version(2, 4))
                throw new IgnoreException("MongoDB server must have at least version 2.4");

            if (server.DatabaseExists("PoIs"))
            {
                CreatePoIDatabase();
            }
        }

        private void CreatePoIDatabase()
        {
            var server = _client.GetServer();

        }

        
    }

    //public class MongoDBBusinessObjectRepository<T> : BusinessObjectAccessBase<T>
    //{
    //    private readonly Dictionary<ObjectId, uint> _fidLookUp = new Dictionary<ObjectId, uint>();
    //    private readonly MongoClient _client;

    //    private MongoDBBusinessObjectRepository()
    //    {
    //        BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap());
    //    }

    //    public MongoDBBusinessObjectRepository(MongoClientSettings settings)
    //        : this()
    //    {
    //        _client = new MongoClient(settings);
    //    }

    //    public MongoDBBusinessObjectRepository(string connectionString)
    //        : this()
    //    {
    //        _client = new MongoClient(connectionString);
    //    }

    //    public override IEnumerable<T> Select(Envelope box)
    //    {
    //        _client.GetServer(
    //    }

    //    public override IEnumerable<T> Select(IGeometry geom)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override T Select(uint id)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Update(IEnumerable<T> businessObjects)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Delete(IEnumerable<T> businessObjects)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Insert(IEnumerable<T> businessObjects)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override int Count
    //    {
    //        get { return _}
    //    }
    //}
}