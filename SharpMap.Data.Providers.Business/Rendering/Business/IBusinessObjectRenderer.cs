using System.Drawing;

namespace SharpMap.Rendering.Business
{
    /// <summary>
    /// Interface for business object renderers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBusinessObjectRenderer<in T>
    {
        /// <summary>
        /// Method to start the rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void StartRendering(Graphics g, Map map);

        /// <summary>
        /// Method to render each individual business object
        /// </summary>
        /// <param name="businessObject">The business object to render</param>
        void Render(T businessObject);

        /// <summary>
        /// Method to finalize rendering of business objects
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        void EndRendering(Graphics g, Map map);
    }
}