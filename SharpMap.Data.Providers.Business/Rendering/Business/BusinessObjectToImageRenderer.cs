// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Business.
// SharpMap.Business is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Business is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Imaging;
using GeoAPI.Geometries;

namespace SharpMap.Rendering.Business
{
    [Serializable]
    public abstract class BusinessObjectToImageRenderer<T> : IBusinessObjectRenderer<T>
    {
        [NonSerialized]
        private Bitmap _image;

        [NonSerialized]
        protected Graphics Graphics;

        [NonSerialized]
        protected Map Map;

        /// <summary>
        /// Method to start the rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        public void StartRendering(Graphics g, Map map)
        {
            _image = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
            Graphics = Graphics.FromImage(_image);

            Graphics.SmoothingMode = g.SmoothingMode;
            Graphics.TextRenderingHint = g.TextRenderingHint;
            Graphics.CompositingMode = g.CompositingMode;
            Graphics.CompositingQuality = g.CompositingQuality;
            Graphics.InterpolationMode = g.InterpolationMode;

            Map = map;
        }

        /// <summary>
        /// Method to render each individual business object
        /// </summary>
        /// <param name="businessObject">The business object to render</param>
        public abstract void Render(T businessObject);

        /// <summary>
        /// Method to finalize rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        public void EndRendering(Graphics g, Map map)
        {
            // Blit image to map
            g.DrawImageUnscaled(_image, 0, 0);
            
            // Dispose objects
            Graphics.Dispose();
            Graphics = null;
            
            _image.Dispose();
            _image = null;
        }

        /// <summary>
        /// Gets or sets a value indicating the math transformation that is to be applied on the geometry prior to rendering
        /// </summary>
        public Func<IGeometry, IGeometry> Transformation { get; set; }
    }
}