using System;
using System.Drawing;
using System.Runtime.Serialization;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Business;
using SharpMap.Rendering.Business;

namespace SharpMap.Layers
{
    [Serializable]
    public class BusinessObjectLayer <T> : Layer, ICanQueryLayer
    {
        private IBusinessObjectSource<T> _source;
        private IBusinessObjectRenderer<T> _businessObjectRenderer;
        [NonSerialized]
        private IProvider _provider;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public BusinessObjectLayer() 
        {
        }

        /// <summary>
        /// Creates an instance of this class assigning the given business object renderer
        /// </summary>
        /// <param name="source">The source for the business objects</param>
        /// <param name="renderer">The renderer for the business objects</param>
        public BusinessObjectLayer(IBusinessObjectSource<T> source, IBusinessObjectRenderer<T> renderer)
        {
            _source = source;
            _businessObjectRenderer = renderer;
            LayerName = _source.Title;
        }

        /// <summary>
        /// Gets or sets a value indicating the business object source
        /// </summary>
        public IBusinessObjectSource<T> Source
        {
            get { return _source; }
            set
            {
                if (_source == value)
                    return;
                _source = value;
                _provider = null;
                OnSourceChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the <see cref="Source"/> has been changed.
        /// </summary>
        public event EventHandler SourceChanged;

        /// <summary>
        /// Event invoker for the <see cref="SourceChanged"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnSourceChanged(EventArgs e)
        {
            if (SourceChanged != null)
                SourceChanged(this, e);
        }

        /// <summary>
        /// Gets or sets a value indicating the business object renderer
        /// </summary>
        public IBusinessObjectRenderer<T> Renderer
        {
            get { return _businessObjectRenderer; }
            set
            {
                if (_businessObjectRenderer == value)
                    return;
                _businessObjectRenderer = value;
                OnRendererChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the <see cref="Renderer"/> has been changed.
        /// </summary>
        public event EventHandler RendererChanged;

        /// <summary>
        /// Event invoker for the <see cref="RendererChanged"/> event.
        /// </summary>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnRendererChanged(EventArgs e)
        {
            if (RendererChanged != null)
                RendererChanged(this, e);
        }

        /// <summary>
        /// Gets a provider 
        /// </summary>
        public IProvider Provider
        {
            get
            {
                return _provider ?? (_provider = new BusinessObjectProvider<T>(_source.Title, _source));
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>
        /// Bounding box corresponding to the extent of the features in the layer
        /// </returns>
        public override Envelope Envelope
        {
            get { return _source.GetExtents(); }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param><param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (_businessObjectRenderer != null)
            {
                _businessObjectRenderer.StartRendering(g, map);
                foreach (var bo in _source.Select(map.Envelope))
                {
                    _businessObjectRenderer.Render(bo);
                }
                _businessObjectRenderer.StartRendering(g, map);
            }

            base.Render(g, map);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Bounding box to intersect with</param><param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            if (IsQueryEnabled)
            {
                Provider.ExecuteIntersectionQuery(box, ds);
                if (ds.Tables.Count > 0)
                {
                    ds.Tables[0].TableName = LayerName;
                }
            }
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geometry">Geometry to intersect with</param><param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geometry, FeatureDataSet ds)
        {
            if (IsQueryEnabled)
            {
                Provider.ExecuteIntersectionQuery(geometry, ds);
                if (ds.Tables.Count > 0)
                {
                    ds.Tables[0].TableName = LayerName;
                }
            }
        }

        /// <summary>
        /// Whether the layer is queryable when used in a SharpMap.Web.Wms.WmsServer, 
        ///             ExecuteIntersectionQuery() will be possible in all other situations when set to FALSE.
        ///             This property currently only applies to WMS and should perhaps be moved to a WMS
        ///             specific class.
        /// </summary>
        public bool IsQueryEnabled { get; set; }

        /// <summary>
        /// Method to set <see cref="_provider"/> after deserialization
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _provider = new BusinessObjectProvider<T>(_source.Title, _source);
        }
    }
}