using GeoAPI.Geometries;
using NUnit.Framework;

namespace SharpMap.Data.Providers.Business.Tests.Memory
{
    [TestFixture]
    public class InMemoryTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        [Test]
        public void TestConstruction()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));
            
            Assert.AreEqual(data.Count, p.GetFeatureCount());
        }

        [Test]
        public void TestGetFeature()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            FeatureDataRow row = null;
            Assert.DoesNotThrow(() => row = p.GetFeature(1));
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.Geometry);
            Assert.AreEqual(OgcGeometryType.Point, row.Geometry.OgcGeometryType);
            Assert.AreEqual(new Coordinate(0, 0), row.Geometry.Coordinate);

            var bop = ((BusinessObjectProvider<PointOfInterest>) p).Source;
            var bo = bop.Select(1);
            
            Assert.AreEqual(bo.ID, row["ID"]);
            Assert.AreEqual(bo.Geometry, row.Geometry);
            Assert.AreEqual(bo.Address, row["Address"]);
        }

        [Test]
        public void TestGetExtetnts()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var e = p.GetExtents();

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            var e2 = bop.GetExtents();

            Assert.AreEqual(e, e2);
            Assert.AreEqual(new Envelope(0, 2, 0, 2), e);

        }

        [Test]
        public void TestExecuteFeatureQuery()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var fds = new FeatureDataSet();
            p.ExecuteIntersectionQuery(new Envelope(0.5, 1.5, 0.5, 1.5), fds);
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.AreEqual("PointOfInterest", fds.Tables[0].TableName);
            Assert.AreEqual(1, fds.Tables[0].Rows.Count);
            Assert.AreEqual(5, fds.Tables[0].Rows[0]["ID"]);
            Assert.AreEqual(new Coordinate(1, 1), ((FeatureDataRow)fds.Tables[0].Rows[0]).Geometry.Coordinate);
        }

    }
}