using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Business
{
    public class EF6BusinessObjectRepository
    {
        private static bool Initialized;

        internal static void Configure()
        {
            if (!Initialized)
            {
                Initialized = true;
                try
                {
                    //SqlServerTypes.Utilities.LoadNativeAssemblies(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                }
                catch (Exception ex)
                {
                    throw new TypeInitializationException("EF6BusinessObjectRepository", ex);
                }
            }
        }
    }

    public class EF6BusinessObjectRepository<T> : BusinessObjectAccessBase<T>
        where T:class, IEF6SpatialGeometryObject
    {
        static EF6BusinessObjectRepository()
        {
            EF6BusinessObjectRepository.Configure();
        }

        private readonly Func<DbContext> _createContext;
        private IGeometryFactory _factory;

        public DbContext Context { get { return _createContext(); } }

        public EF6BusinessObjectRepository(Func<DbContext> createContext)
        {

            _createContext = createContext;
        }

        /// <summary>
        /// Gets the entities
        /// </summary>
        public IDbSet<T> Entities { get { return Context.Set<T>(); } }

        /// <summary>
        /// Gets or sets a factory
        /// </summary>
        public IGeometryFactory Factory   
        {
            get
            {
                if (_factory == null)
                {
                    lock (_createContext)
                    {
                        if (_factory == null)
                        {
                            using (var c = _createContext())
                            {
                                var f = c.Set<T>().FirstOrDefault();
                                var srid = f == null ? 0 : f.DbGeometry.CoordinateSystemId;
                                _factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid);
                            }
                        }
                    }
                }
                return _factory;
            }
            set { _factory = value; }
        }

        public override IEnumerable<T> Select(Envelope box)
        {
            var envGeom = Factory.ToGeometry(box);
            return Select(envGeom);
        }

        public override IEnumerable<T> Select(IGeometry geom)
        {
            var dbGeometry = geom.ToDbGeometry();
            using (var c = Context)
            {
                var qry = from u in c.Set<T>()
                    where u.DbGeometry.Intersects(dbGeometry)
                    select u;

                return Select(qry);
            }
            
        }

        public override Envelope GetExtents()
        {
            if (CachedExtents == null)
            {
                using (var c = Context)
                {
                    var q = from t in c.Set<T>()
                        select t.DbGeometry.Envelope;
                    
                    CachedExtents = ToEnvelope(q.AsEnumerable());
                }
            }
            return CachedExtents;
        }

        private static Envelope ToEnvelope(IEnumerable<DbGeometry> dbGeometries)
        {
            var res = new Envelope();
            foreach (var dbGeometry in dbGeometries)
            {
                res.ExpandToInclude(dbGeometry.ToGeometry().EnvelopeInternal);
            }
            return res;
        }

        public override T Select(uint id)
        {
            using (var c = Context)
            {
                return c.Set<T>().Find((int)id);
            }
        }

        public override void Update(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().AddOrUpdate(new List<T>(businessObjects).ToArray());
                c.SaveChanges();
            }
        }

        public override void Delete(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().RemoveRange(businessObjects);
                c.SaveChanges();
            }
        }

        public override void Insert(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().AddRange(businessObjects);
                c.SaveChanges();
            }
        }

        public override int Count
        {
            get
            {
                using (var c = Context)
                {
                    return c.Set<T>().Count();
                }
            }
        }
    }
}
