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
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using Common.Logging;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Business;
using SharpMap.Rendering.Business;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    [Serializable]
    public class BusinessObjectLayer<T> : Layer, ICanQueryLayer
    {
        protected static ILog Logger = LogManager.GetLogger<Layer>();
        
        private IBusinessObjectSource<T> _source;
        private IBusinessObjectRenderer<T> _businessObjectRenderer;
        private IGeometryFactory _targetFactory;

        [NonSerialized]
        private IProvider _provider;

        private ITheme _theme;

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
        public BusinessObjectLayer(IBusinessObjectSource<T> source)
            :this(source, null)
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
            Logger.Info( fmh => fmh("Source changed: {0}", _source != null ? _source.Title : "null") );

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
            Logger.Info(fmh => fmh("Renderer changed: {0}", _businessObjectRenderer != null ? _businessObjectRenderer.GetType().Name : "null"));

            if (RendererChanged != null)
                RendererChanged(this, e);
        }

        public override ICoordinateTransformation CoordinateTransformation
        {
            get
            {
                return base.CoordinateTransformation;
            }
            set
            {
                if (value == CoordinateTransformation)
                    return;

                if (value == null)
                    _targetFactory = null;
                else
                    GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(Convert.ToInt32(value.TargetCS.AuthorityCode));

                base.CoordinateTransformation = value;
            }
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
                Logger.Info(fmh => fmh("Rendering using {0}", _businessObjectRenderer.GetType().Name));

                // Get smoothing mode
                var sm = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Set up renderer
                _businessObjectRenderer.StartRendering(g, map);
                _businessObjectRenderer.Transformation = (bog) => ToTarget(bog);

                // Get query envelope
                var env = ToSource(map.Envelope);

                // Render all objects
                foreach (var bo in _source.Select(env))
                {
                    _businessObjectRenderer.Render(bo);
                }

                // Finish rendering
                _businessObjectRenderer.EndRendering(g, map);

                // reset smoothing mode
                g.SmoothingMode = sm;

            }
            else
            {
                Logger.Info(fmh => fmh("Rendering using VectorLayer approach"));

                using (var vl = new VectorLayer(LayerName, Provider))
                {
                    if (!(Style is VectorStyle))
                    {
                        Logger.Info(fmh => fmh("The assigned style is not a VectorStyle. Creating a random VectorStyle"));

                        var s = VectorStyle.CreateRandomStyle();
                        s.Enabled = Enabled;
                        s.MinVisible = MinVisible;
                        s.MaxVisible = MaxVisible;

                        Style = s;
                    }

                    vl.Style = ((VectorStyle) Style).Clone();
                    vl.Theme = Theme;
                    vl.CoordinateTransformation = CoordinateTransformation;
                    vl.ReverseCoordinateTransformation = ReverseCoordinateTransformation;

                    vl.Render(g, map);
                }
            }

            base.Render(g, map);
        }

        /// <summary>
        /// Gets or sets a theme
        /// </summary>
        public ITheme Theme
        {
            get { return _theme; }
            set
            {
                if (value == _theme)
                    return;

                if (_businessObjectRenderer != null && value != null)
                {
                    Logger.Info(fmh => fmh("The assigned theme will not come to action as this layer has a business object renderer assigned!"));
                }
                _theme = value;
            }
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

#if !SharpMap_v1_2
        protected Envelope ToTarget(Envelope envelope)
        {
            if (CoordinateTransformation == null)
                return envelope;
#if !DotSpatialProjections
            return GeometryTransform.TransformBox(envelope, CoordinateTransformation.MathTransform);
#else
            return GeometryTransform.TransformBox(box, CoordinateTransformation.Source, CoordinateTransformation.Target);
#endif
        }

        protected IGeometry ToTarget(IGeometry geometry)
        {
            if (CoordinateTransformation == null)
                return geometry;
#if !DotSpatialProjections
            return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.MathTransform, geometry.Factory);
#else
            return GeometryTransform.TransformGeometry(geometry, CoordinateTransformation.Source, CoordinateTransformation.Target, targetFactory);
#endif
        }

        protected Envelope ToSource(Envelope envelope)
        {
            if (ReverseCoordinateTransformation == null)
            {
                if (CoordinateTransformation == null)
                    return envelope;

                var mt = CoordinateTransformation.MathTransform;
#if !DotSpatialProjections
                mt.Invert();
                var res = GeometryTransform.TransformBox(envelope, mt);
                mt.Invert();
#else
                return GeometryTransform.TransformBox(envelope, CoordinateTransformation.Target, CoordinateTransformation.Source);
#endif
                return res;
            }

#if !DotSpatialProjections
            return GeometryTransform.TransformBox(envelope, ReverseCoordinateTransformation.MathTransform);
#else
            return GeometryTransform.TransformBox(box, ReverseCoordinateTransformation.Source, ReverseCoordinateTransformation.Target);
#endif
        }
#endif
        
        /// <summary>
        /// Method to set <see cref="Provider"/> after deserialization
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _provider = new BusinessObjectProvider<T>(_source.Title, _source);
        }
    }
}