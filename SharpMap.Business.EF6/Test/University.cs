using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using Common.Logging;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Test
{
    public class University : EF6SpatialGeometryObjectBase
    {
        private DbGeometry _dbGeometry;
        
        [NotMapped, BusinessObjectIdentifier]
        public override uint Fid { get { return (uint)Id; } }
        
        [Key]
        public int Id { get; set; }

        [Required, BusinessObjectAttribute]
        public string Name { get; set; }

        [Required]
        public override DbGeometry DbGeometry
        {
            get { return _dbGeometry; }
            set
            {
                if (value == _dbGeometry)
                    return;
                _dbGeometry = value;
                SetGeometry(_dbGeometry.ToGeometry());
            }
        }

        protected override void SetDbGeometry(DbGeometry dbGeometry)
        {
            _dbGeometry = dbGeometry;
        }
    }

    public class UniversityContext : DbContext
    {
        public DbSet<University> Universities { get; set; }
    }

    public class UniversityTest
    {
        /// <summary>
        /// 
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            using (var context = new UniversityContext())
            {
                var connectionString = context.Database.Connection.ConnectionString;
                LogManager.GetCurrentClassLogger().Debug(fmh => fmh("Connected with: {0}", connectionString));

                //Delete old entries
                var entries = context.Universities.AsEnumerable();
                context.Universities.RemoveRange(entries);
                context.SaveChanges();

                // Add new
                context.Universities.Add(new University
                {
                    Name = "Graphic Design Institute",
                    DbGeometry = DbGeometry.FromText("POINT(-122.336106 47.605049)"),
                });

                context.Universities.Add(new University
                {
                    Name = "School of Fine Art",
                    DbGeometry = DbGeometry.FromText("POINT(-122.335197 47.646711)"),
                });
                context.SaveChanges();
            }            
        }

        [Test]
        public void ContextTest()
        {
            var myLocation = DbGeometry.FromText("POINT(-122.296623 47.640405)");

            using (var context = new UniversityContext())
            {
                var university = (from u in context.Universities
                    orderby u.DbGeometry.Distance(myLocation)
                    select u).FirstOrDefault();

                Assert.IsNotNull(university);
                Assert.AreEqual("School of Fine Art", university.Name);
            }
        }

        [Test]
        public void BusinessObjectRepositoryTest()
        {
            EF6BusinessObjectRepository<University> bos = null;

            Assert.DoesNotThrow( () => bos = new EF6BusinessObjectRepository<University>(() => new UniversityContext()));
            Assert.IsNotNull(bos);
            Assert.AreEqual(2, bos.Count);

            IEnumerable<University> us;
            using (var c = bos.Context)
            {
                us = c.Set<University>().ToList();
            }

            var env = new Envelope();
            foreach (var u in us)
            {
                var u2 = bos.Select(u.Fid);
                Assert.IsNotNull(u2);
                Assert.AreEqual(u.Id, u2.Id);
                Assert.AreEqual(u.DbGeometry.AsText(), u2.DbGeometry.AsText());
                env.ExpandToInclude(u2.Geometry.EnvelopeInternal);
            }

            us = bos.Select(env);
            Assert.AreEqual(2, us.Count());

            var p1 = DbGeometry.FromText("POINT(-122.296623 47.640405)").ToGeometry();
            var p2 = DbGeometry.FromText("POINT(-122.335197 47.646711)").ToGeometry();
            var d = p1.Distance(p2);
            var pBuf = p1.Buffer(1.02*d);
            us = bos.Select(pBuf);
            
            Assert.IsNotNull(us);
            Assert.AreEqual(1, us.Count());
            Assert.AreEqual("School of Fine Art", us.First().Name);

            var pDbBuf = pBuf.ToDbGeometry();
            using (var c = bos.Context)
            {
                var q = from tmp in c.Set<University>()
                    where tmp.Name == "Graphic Design Institute" || tmp.DbGeometry.Intersects(pDbBuf)
                    select tmp;

                us = bos.Select(q);
            }

            us = bos.Select(env);
            Assert.AreEqual(2, us.Count());
        }

        [Test]
        public void ProviderTest()
        {
            BusinessObjectProvider<University> p = null;
            Assert.DoesNotThrow(() => p= new BusinessObjectProvider<University>("XYZ",
                new EF6BusinessObjectRepository<University>(() => new UniversityContext())));

            Assert.IsNotNull(p);
            Assert.AreEqual(2, p.GetFeatureCount());

            Envelope env = null;
            Assert.DoesNotThrow(() => env = p.GetExtents());
            Assert.IsNotNull(env);
            Assert.IsFalse(env.IsNull);

            var fds = new FeatureDataSet();
            Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(env, fds));
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.AreEqual(2, fds.Tables[0].Rows.Count);

            fds = new FeatureDataSet();
            var f = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(0);
            var pBuf = f.CreatePoint(new Coordinate(-122.296623, 47.640405)).Buffer(0.001);
            Assert.DoesNotThrow(() => p.ExecuteIntersectionQuery(pBuf, fds));
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.GreaterOrEqual(1, fds.Tables[0].Rows.Count);

            fds = new FeatureDataSet();
            var r = p.Source as EF6BusinessObjectRepository<University>;
            using (var c = r.Context)
            {
                var pDbBuf = pBuf.ToDbGeometry();
                var q = from u in c.Set<University>()
                    where u.Name == "Graphic Design Institute" || u.DbGeometry.Intersects(pDbBuf)
                    orderby u.Name
                    select u;

                Assert.DoesNotThrow(() => p.ExecuteQueryable(q, fds));
            }
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.GreaterOrEqual(2, fds.Tables[0].Rows.Count);
        }
    }
}