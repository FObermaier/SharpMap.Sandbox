using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers.Business.Tests.Memory;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace SharpMap.Data.Providers.Business.Tests.NHibernating
{
    [TestFixture]
    public class NHibernateTests
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
            var source = new BusinessObjectRepository<Country>();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

            Assert.AreEqual(source.Count, p.GetFeatureCount());
        }

        [Test]
        public void TestGetFeature()
        {
            IProvider p = null;
            var source = new BusinessObjectRepository<Country>();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

            FeatureDataRow row = null;
            Assert.DoesNotThrow(() => row = p.GetFeature(1));
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.Geometry);
            Assert.AreEqual(OgcGeometryType.MultiPolygon, row.Geometry.OgcGeometryType);
            //Assert.AreEqual(new Coordinate(0, 0), row.Geometry.Coordinate);

            var bop = source;
            var bo = bop.Select(1);

            Assert.AreEqual(bo.PKID, row["PKID"]);
            Assert.IsTrue(bo.Geometry.EqualsExact(row.Geometry));
            Assert.AreEqual(bo.Admin, row["Admin"]);
        }

        [Test]
        public void TestGetExtents()
        {
            IProvider p = null;
            var source = new BusinessObjectRepository<Country>();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

            var e = p.GetExtents();

            var bop = source;
            var e2 = bop.GetExtents();

            Assert.AreEqual(e, e2);
            Assert.IsTrue(new Envelope(-180, 181, -90, 90).Contains(e));

        }

        [Test]
        public void TestExecuteFeatureQuery1()
        {
            IProvider p = null;
            var source = new BusinessObjectRepository<Country>();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

            var fds = new FeatureDataSet();
            //http://tools.wmflabs.org/geohack/geohack.php?pagename=Bonn&language=de&params=50.719113888889_N_7.1175722222222_E_region:DE-NW_type:landmark&title=Bundeskanzlerplatz
            var bn = new Coordinate(7.1175722222222, 50.719113888889);
            var e = new Envelope(bn).Grow(0.00001);
            p.ExecuteIntersectionQuery(e, fds);
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.AreEqual("Country", fds.Tables[0].TableName);
            Assert.AreEqual(5, fds.Tables[0].Rows.Count);
        }

        [Test]
        public void TestExecuteFeatureQuery2()
        {
            IProvider p = null;
            var source = new BusinessObjectRepository<Country>();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

            var fds = new FeatureDataSet();
            //http://tools.wmflabs.org/geohack/geohack.php?pagename=Bonn&language=de&params=50.719113888889_N_7.1175722222222_E_region:DE-NW_type:landmark&title=Bundeskanzlerplatz
            var bn = new Coordinate(7.1175722222222, 50.719113888889);
            var pt = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(bn);

            p.ExecuteIntersectionQuery(pt, fds);
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.AreEqual("Country", fds.Tables[0].TableName);
            Assert.AreEqual(1, fds.Tables[0].Rows.Count);
        }
        [Test]
        public void TestGetMap()
        {
            using (var m = new Map(new Size(720, 360)))
            {
                VectorRenderer.SizeOfString = (g, s, f) => TextRenderer.MeasureText(g, s, f);
                m.BackColor = Color.White;

                IProvider p = null;
                var source = new BusinessObjectRepository<Country>();
                Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(source));

                m.Layers.Add(new VectorLayer("Countries", p));
                var ll = new LabelLayer("CountryLabels");
                ll.DataSource = p;
                ll.LabelStringDelegate = (fdr) => string.Format("{0} - {1}", fdr["PKID"], fdr["Admin"]);
                ll.PriorityDelegate = (fdr) => (int)fdr.Geometry.Area;
                ll.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;
                ll.LabelFilter = Rendering.LabelCollisionDetection.SimpleCollisionDetection;

                ll.Style.Halo = new Pen(Color.Wheat, 2f) { LineJoin = LineJoin.MiterClipped };
                ll.Style.ForeColor = Color.Brown;
                ll.Style.CollisionBuffer = new SizeF(2,2);
                ll.Style.CollisionDetection = true;
                
                m.Layers.Add(ll);

                m.ZoomToExtents();
                m.Zoom *= 1.1;

                using (var img = m.GetMap())
                {
                    img.Save("CountriesImage.png", ImageFormat.Png);
                }
            }

        }
    }
}