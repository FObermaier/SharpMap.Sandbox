using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business.Tests.Memory
{
    public class PointOfInterest
    {
        [BusinessObjectIdentifier()]
        public uint ID { get; set; }

        [BusinessObjectGeometry]
        public IGeometry Geometry { get; set; }

        [BusinessObjectAttribute]
        public string Kind { get; set; }

        [BusinessObjectAttribute]
        public string Address { get; set; }

        [BusinessObjectAttribute(AllowNull = true)]
        public List<string> Comments { get; set; }
    }

    public class PointsOfInterest : List<PointOfInterest>
    {
        public static PointsOfInterest Create()
        {
            var res = new PointsOfInterest();
            var f = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            
            res.Add(new PointOfInterest { ID = 1, Address = "ll", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(0, 0))});
            res.Add(new PointOfInterest { ID = 2, Address = "cl", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(1, 0)) });
            res.Add(new PointOfInterest { ID = 3, Address = "rl", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(2, 0)) });
            res.Add(new PointOfInterest { ID = 4, Address = "lc", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(0, 1)) });
            res.Add(new PointOfInterest { ID = 5, Address = "cc", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(1, 1)) });
            res.Add(new PointOfInterest { ID = 6, Address = "cr", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(2, 1)) });
            res.Add(new PointOfInterest { ID = 7, Address = "tl", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(0, 2)) });
            res.Add(new PointOfInterest { ID = 8, Address = "tc", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(1, 2)) });
            res.Add(new PointOfInterest { ID = 9, Address = "tr", Kind = "African food", Geometry = f.CreatePoint(new Coordinate(2, 2)) });

            return res;
        }
    }
}
