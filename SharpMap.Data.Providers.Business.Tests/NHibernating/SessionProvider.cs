using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace SharpMap.Data.Providers.Business.Tests.NHibernating
{
    public class SessionProvider
    {
        private static ISessionFactory _sessionFactory;
        private static Configuration _config;

        static SessionProvider()
        {
            
        }

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = CreateSessionFactory();
                }
                return _sessionFactory;
            }
        }
 
        public static Configuration Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new Configuration();
                    _config.Configure();
                    var ca = Assembly.GetCallingAssembly();
                    _config.AddAssembly(ca);
                }
                return _config;
            }
        }
 
        private static ISessionFactory CreateSessionFactory()
        {
            return Config.BuildSessionFactory();
        }
 
        public static void RebuildSchema()
        {
            var schema = new SchemaExport(Config);
            schema.Create(true,true);
        }
    }
}