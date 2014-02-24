using System;
using System.Drawing;
using System.Drawing.Imaging;

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
    }
}