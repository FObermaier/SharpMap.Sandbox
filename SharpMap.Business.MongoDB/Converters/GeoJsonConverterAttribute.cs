using System;

namespace SharpMap.Converters
{
    public enum GeoJsonCoordinate
    {
        Regular2D,
        Projected2D,
        Geographic2D,
        Regular3D,
        Projected3D,
        Geographic3D,
    }

    public class GeoJsonConverterAttribute : Attribute
    {
        public GeoJsonConverterAttribute()
            :this(GeoJsonCoordinate.Regular2D)
        {
        }

        private GeoJsonConverterAttribute(GeoJsonCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public GeoJsonCoordinate Coordinate { get; set; }
    }
}