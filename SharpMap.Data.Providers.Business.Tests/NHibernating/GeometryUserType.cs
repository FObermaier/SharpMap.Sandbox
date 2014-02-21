using System;
using System.Data;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.IO;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace SharpMap.Data.Providers.Business.Tests.NHibernating
{
    [Serializable]
    public class GeometryUserType : IUserType
    {
        private static readonly IBinaryGeometryReader Reader = new GaiaGeoReader();
        private static readonly IBinaryGeometryWriter Writer = new GaiaGeoWriter();

        public new bool Equals(object x, object y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var value = rs.GetValue(rs.GetOrdinal(names[0]));
            if (value == DBNull.Value)
                return null;

            return Reader.Read((byte[]) value);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var geom = (IGeometry) value;
            NHibernateUtil.Binary.NullSafeSet(cmd, Writer.Write(geom), index);
        }

        public object DeepCopy(object value)
        {
            if (ReferenceEquals(value, null))
                return null;
            return ((IGeometry) value).Clone();
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes { get { return new [] { new SqlType(DbType.Binary)  };} }

        public Type ReturnedType { get { return typeof(IGeometry); } }

        public bool IsMutable
        {
            get { return true; }
        }
    }
}