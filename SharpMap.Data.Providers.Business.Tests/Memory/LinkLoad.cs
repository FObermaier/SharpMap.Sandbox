using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.Rendering.Business;

namespace SharpMap.Data.Providers.Business.Tests.Memory
{
    [Serializable]
    public class LinkWithLoad
    {
        private static uint _lastId = 1;
        public LinkWithLoad()
        {
            ID = _lastId++;
        }

        [BusinessObjectIdentifier]
        public virtual uint ID { get; set; }
        [BusinessObjectGeometry]
        public virtual ILineString LineString { get; set; }
        [BusinessObjectAttribute]
        public virtual double[][] Load { get; set; }
    }

    [Serializable]
    public class LinkWithLoadRenderer : BusinessObjectToImageRenderer<LinkWithLoad>
    {
        private readonly OffsetCurveBuilder _offsetCurveBuilder = new OffsetCurveBuilder(new PrecisionModel(), new BufferParameters());

        /// <summary>
        /// Method to render each individual business object
        /// </summary>
        /// <param name="businessObject">The business object to render</param>
        public override void Render(LinkWithLoad businessObject)
        {
            // Draw the loads in positive direction
            RenderLoadStrips(businessObject.LineString, businessObject.Load[0], Scale);

            // Draw the loads in negative direction
            RenderLoadStrips(businessObject.LineString, businessObject.Load[1], -Scale);

            // Draw the axis
            if (AxisPen != null)
                Graphics.DrawLines(AxisPen, businessObject.LineString.TransformToImage(Map));
        }

        private void RenderLoadStrips(ILineString ls, double[] loads, double scale = 1)
        {
            var start = ls.Coordinates;
            for (var i = 0; i < loads.Length; i++)
            {
                if (loads[i] == 0) continue;
                
                // Compute the offset line
                var offset = _offsetCurveBuilder.GetOffsetCurve(start, scale * loads[i]);

                // Setup coordinate sequence
                var pts = new List<Coordinate>(start);
                pts.AddRange(offset.Reverse());
                pts.Add(start[0]);

                // Build the ring
                var ring = ls.Factory.CreateLinearRing(pts.ToArray());

                // Transform to image coordinates
                var gp = ring.TransformToImage(Map);

                // Draw the polygon
                Brush b;
                if (!LoadBrush.TryGetValue(i, out b))
                    b = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.HotPink, Color.Chartreuse);

                Graphics.FillPolygon(b, gp);
                Graphics.DrawPolygon(FramePen, gp);

                start = offset;
            }
        }

        public Pen AxisPen { get; set; }

        public Pen FramePen { get; set; }

        public double Scale { get; set; }

        public IDictionary<int, Brush> LoadBrush { get; set; }

        public LinkWithLoadRenderer()
        {
            AxisPen = new Pen(Color.Black, 2);
            FramePen = new Pen(Color.Gainsboro, 1);
            Scale = 0.01;
            LoadBrush = new Dictionary<int, Brush>{{0, Brushes.Green}, {1, Brushes.Gold}, {2, Brushes.OrangeRed}};
        }
    }

    public class BusinessObjectLayerTest
    {
        private List<LinkWithLoad> _linksWithLoads;

        [TestFixtureSetUp]
        public void TestSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            var f = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(31467);
            _linksWithLoads = new List<LinkWithLoad>
            {
                new LinkWithLoad
                {
                    LineString = f.CreateLineString(new []{ new Coordinate(0, 0), new Coordinate(100, 400), new Coordinate(500, 500) }),
                    Load = new []{ new [] { 500d, 200, 700, 30 }, new [] { 500d, 200, 30, 700 }}
                },

                new LinkWithLoad
                {
                    LineString = f.CreateLineString(new []{ new Coordinate(500, 0), new Coordinate(100, 400) }),
                    Load = new []{ new [] { 50d, 20, 70, 3 }, new [] { 50d, 20, 70, 3 }}
                }
            };
        }

        [Test]
        public void TestRendering()
        {
            var s = new InMemoryBusinessObjectAccess<LinkWithLoad>();
            s.Insert(_linksWithLoads);

            var l = new BusinessObjectLayer<LinkWithLoad>(s);
            l.Renderer = new LinkWithLoadRenderer{ Scale = 0.05 };

            var m = new Map(new Size(500, 300));
            m.Layers.Add(l);

            m.ZoomToExtents();
            m.Zoom *= 1.15;
            m.GetMap().Save("LinkWithLoadImage.png", ImageFormat.Png);
        }
    }
}