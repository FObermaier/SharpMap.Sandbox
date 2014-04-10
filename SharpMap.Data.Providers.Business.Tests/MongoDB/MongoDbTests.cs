using System;
using System.Threading;
using Common.Logging;
using GeoAPI.Geometries;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using NUnit.Framework;

namespace SharpMap.Data.Providers.Business.Tests.MongoDB
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoDbTests
    {
        private global::MongoDB.Driver.MongoClient _client;

        private const string TestConnection = "mongodb://localhost";
        private const string TestDatabase = "SharpMap_PoiTest";
        private const string TestCollection = "Items";

        private const string StartMongoDbServerCommand = @"c:\mongodb\bin\mongod.exe";
        private const string StartMongoDbServerArguments = @"-directoryperdb -dbpath c:\mongodb\data --journal --noauth";

        static MongoClient GetClient()
        {
            try
            {
                var client = new MongoClient(TestConnection);
                var server = client.GetServer();
                foreach (var dbNames in server.GetDatabaseNames())
                {
                    LogManager.GetCurrentClassLogger().Debug(fmh => fmh("{0}", dbNames));
                }
                return client;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Debug(fmh => fmh("MongoDB Server not found or running"));
            }
            return null;
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _client = GetClient();
            if (_client == null)
            {
                try
                {
                    System.Diagnostics.Process.Start(StartMongoDbServerCommand, StartMongoDbServerArguments);
                    Thread.Sleep(2000);
                }
                finally
                {
                    _client = GetClient();
                }
            }
            if (_client == null)
                throw new IgnoreException("Creation of MongoClient failed");

            var server = _client.GetServer();
            if (server.BuildInfo != null)
            {
                if (server.BuildInfo.Version < new Version(2, 4))
                    throw new IgnoreException("MongoDB server must have at least version 2.4");
            }

            if (server.DatabaseExists(TestDatabase))
                server.DropDatabase(TestDatabase);

            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            CreatePoIDatabase(server);
        }

        private static void CreatePoIDatabase(MongoServer server)
        {
            var db = server.GetDatabase(TestDatabase);
            if (!db.CreateCollection("Items").Ok)
                throw new IgnoreException("Faild to create collection items");

            var col = db.GetCollection<PoI<GeoJson2DCoordinates>>("Items");
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            for (uint oid = 1; oid <= 1000; oid++)
            {
                col.Insert(RndPoi(ref oid, factory));
            }
            


        }

        [Test]
        public void Test1()
        {
            MongoDBBusinessObjectRepository<PoI<GeoJson2DCoordinates>> repo = null;
            Assert.DoesNotThrow( () => repo =
                new MongoDBBusinessObjectRepository<PoI<GeoJson2DCoordinates>>(TestConnection, TestDatabase, TestCollection));

            Assert.IsNotNull(repo);
            Assert.AreEqual(1000, repo.Count);
        }

        #region Poi generation

        private static readonly Random RND = new Random(6658475);

        private static PoI<GeoJson2DCoordinates> RndPoi(ref uint oid, IGeometryFactory factory)
        {
            return new PoI<GeoJson2DCoordinates>
            {
                Id = oid,
                Label = string.Format("POI {0}", oid),
                Geometry = factory.CreatePoint(new Coordinate(RndX(), RndY()))
            };
        }

        private static double RndX()
        {
            return -180 + RND.NextDouble() * 360;
        }

        private static double RndY()
        {
            return -90 + RND.NextDouble() * 180;
        }

        #endregion

    }
}