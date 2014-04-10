using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BruTile.Web.Wms;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Valid;
using SharpMap.Layers;

namespace SharpMap.Printing
{
    public class MapPrintDocument : PrintDocument
    {
        private Font _headerFont;

        public MapPrintDocument(Map map, Envelope areaOfInterest, uint mapScaleDenominator, SizeF? overlap = null)
        {
            Map = map.Clone();
            AreaOfInterest = areaOfInterest;
            MapScaleDeniminator = mapScaleDenominator;

            Overlap = overlap ?? new SizeF(10, 10);
            NumPages = EstimateNumPages(PrinterSettings, DefaultPageSettings);
        }

        private PrintAction PrintAction { get; set; }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);
            PrintAction = e.PrintAction;
            OriginAtMargins = true;
            EnsureMargins(DefaultPageSettings.Margins, DefaultPageSettings.HardMarginX, DefaultPageSettings.HardMarginY);
        }

        private static void EnsureMargins(Margins margins, float hardMarginX, float hardMarginY)
        {
            if (margins.Left < hardMarginX) margins.Left = (int)Math.Ceiling(hardMarginX);
            if (margins.Right < hardMarginX) margins.Right = (int)Math.Ceiling(hardMarginX);
            if (margins.Top < hardMarginY) margins.Top = (int)Math.Ceiling(hardMarginY);
            if (margins.Bottom < hardMarginY) margins.Bottom = (int)Math.Ceiling(hardMarginY);
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            CurrentArea = GetCurrentArea(e.Graphics, e.PageSettings);

            e.Graphics.PageUnit = GraphicsUnit.Pixel;
            Map.GetMapZoomFromScale(MapScaleDeniminator, (int)e.Graphics.DpiX);

            // Adjust the margins
            var margins = e.PageSettings.Margins;
            if (PrintAction == PrintAction.PrintToPreview)
            {
                e.Graphics.TranslateTransform(margins.Left, margins.Top);
            }

            // Render the static layer collection
            Map.ZoomToBox(CurrentArea);
            if (Map.Layers.Count > 0)
            {
                Map.RenderMap(e.Graphics, LayerCollectionType.Static, false, false);
            }

            // Zoom to the whole area of interest, render decorations seperatly
            // in order to not have map decoratons all over the place
            Map.ZoomToBox(AreaOfInterest);
            foreach (var mapDecoration in Map.Decorations)
            {
                mapDecoration.Render(e.Graphics, Map);
            }

            // increase the number of pages printed
            NumPages++;
            PrintHeader(e.Graphics);
            PrintView(e.Graphics);
            
            // increase the number of pages printed
            e.HasMorePages = NumPagesEstimate > NumPages;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_headerFont != null) _headerFont.Dispose();
            if (HeaderViewFont != null) HeaderViewFont.Dispose();
            Map.DisposeLayersOnDispose = false;
            if (Map != null) Map.Dispose();
        }

        protected virtual void PrintHeader(Graphics g)
        {
            var text = Header;
            if (NumPagesEstimate > 1)
                text += string.Format(" ({0}/{1})", NumPages, NumPagesEstimate);

            g.DrawString(text, new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Black, HeaderLocation);
        }

        /// <summary>
        /// Method to print the 
        /// </summary>
        /// <param name="g"></param>
        protected virtual void PrintView(Graphics g)
        {
            g.Transform = new Matrix(1f, 0f, 1f,1f, HeaderLocation.X, HeaderLocation.Y);
            var text = string.Format("{0}/{1}/{2}", "SharpMap.Printing", DateTime.Now ,CurrentArea);
            g.DrawString(text, new Font("Arial", 9, GraphicsUnit.Pixel) , Brushes.Black, new PointF(), new StringFormat { LineAlignment = StringAlignment.Far});
        }

        /// <summary>
        /// Gets or sets the header
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the header location
        /// </summary>
        public PointF HeaderLocation { get; set; }

        public Font HeaderFont
        {
            get { return _headerFont; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _headerFont = value;
                HeaderViewFont = new Font(_headerFont.FontFamily, _headerFont.Size / 3, FontStyle.Regular);
            }
        }

        private Font HeaderViewFont { get; set; }

        private Envelope GetCurrentArea(Graphics g, PageSettings pageSettings)
        {
            
            var pa = pageSettings.PrintableArea;
            if (pageSettings.Landscape)
                pa = new RectangleF(pa.Top, pa.Left, pa.Height, pa.Width);

            var width = Convert.ToInt32(g.DpiX * (pa.Width - TextRenderer.MeasureText(g, "I", HeaderViewFont).Height) / 100f);
            var height = Convert.ToInt32(g.DpiY * (pa.Height - TextRenderer.MeasureText(g, "I", HeaderFont).Height) / 100f);

            Map.Size = new Size(width, height);
            if (CurrentArea == null)
            {
                CurrentArea = AreaOfInterest;
            }
            return CurrentArea;
        }

        private int EstimateNumPages(PrinterSettings printerSettings, PageSettings pageSetings)
        {
            
            var pa = pageSetings.PrintableArea;
            var width = (pa.Width - Overlap.Width);
            //var envelope = new Envelope(AreaOfInterest.MinX, (double)pa.Width * )
            return 1;
        }

        /// <summary>
        /// Gets or sets a value indicating the map to render
        /// </summary>
        private Map Map { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the map scale denominator
        /// </summary>
        private uint MapScaleDeniminator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the area of interest
        /// </summary>
        private Envelope AreaOfInterest { get; set; }

        /// <summary>
        /// Gets or sets a value of the current area
        /// </summary>
        private Envelope CurrentArea { get; set; }

        /// <summary>
        /// Gets or sets the number of pages
        /// </summary>
        private int NumPages { get; set; }

        /// <summary>
        /// Gets or sets the number of pages estimate
        /// </summary>
        private int NumPagesEstimate { get; set; }

        /// <summary>
        /// Gets or sets the overlap of map tiles
        /// </summary>
        private SizeF Overlap { get; set; }
    }
}