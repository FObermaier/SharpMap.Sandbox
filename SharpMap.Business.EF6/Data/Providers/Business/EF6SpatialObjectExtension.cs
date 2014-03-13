using System;
using System.Data.Entity.Spatial;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Business
{
    public static class EF6SpatialObjectExtension
    {
        public static DbGeometry ToDbGeometry(this IGeometry self)
        {
            if (self == null)
                return null;
            return DbGeometry.FromBinary(self.AsBinary());
        }

        public static IGeometry ToGeometry(this DbGeometry self)
        {
            if (self == null)
                return null;
            
            var reader = new WKBReader(NetTopologySuite.NtsGeometryServices.Instance);
            return reader.Read(self.AsBinary());
        }

        public static DbGeography ToDbGeography(this IGeometry self)
        {
            if (self == null)
                return null;
            if (self.SRID != 4326)
                throw new ArgumentException();

            return DbGeography.FromBinary(self.AsBinary());
        }

        public static IGeometry ToGeometry(this DbGeography self)
        {
            if (self == null)
                return null;

            var reader = new WKBReader(NetTopologySuite.NtsGeometryServices.Instance);
            return reader.Read(self.AsBinary());
        }
    }
}