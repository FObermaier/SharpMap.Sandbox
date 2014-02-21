using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business.Tests.NHibernating
{
    public class Country
    {
        [BusinessObjectIdentifier] 
        public virtual uint PKID { get; set; }

        [BusinessObjectGeometry]
        public virtual IGeometry Geometry { get; set; }

        [BusinessObjectAttribute(Ordinal = 1)]
        public virtual string Sovereignt { get; set; }
        [BusinessObjectAttribute(Ordinal = 2)]
        public virtual string Sovereignt_Abbreviation { get; set; }
        [BusinessObjectAttribute(Ordinal = 3)]
        public virtual string Admin { get; set; }
        [BusinessObjectAttribute(Ordinal = 4)]
        public virtual string Type { get; set; }
        [BusinessObjectAttribute(Ordinal = 5)]
        public virtual string Economy { get; set; }
        [BusinessObjectAttribute(Ordinal = 6)]
        public virtual double Population_Estimate { get; set; }
        [BusinessObjectAttribute(Ordinal = 7)]
        public virtual double Gdp_Estimate { get; set; }
    }
}