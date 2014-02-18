using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Static utility for the creation of <see cref="BusinessObjectProvider"/>s.
    /// </summary>
    public static class BusinessObjectProvider
    {
        /// <summary>
        /// Creates a provider for the given set of <paramref name="features"/>
        /// </summary>
        /// <typeparam name="T">The type of the features</typeparam>
        /// <param name="features">The features</param>
        /// <returns>A provider</returns>
        public static IProvider Create<T>(IEnumerable<T> features)
        {
            return Create(new InMemoryBusinessObjectAccess<T>(features));
        }

        /// <summary>
        /// Creates a provider for the given <paramref name="source"/>
        /// </summary>
        /// <typeparam name="T">The type of the features</typeparam>
        /// <param name="source">The feature source</param>
        /// <returns>A provider</returns>
        public static IProvider Create<T>(IBusinessObjectSource<T> source)
        {
            return new BusinessObjectProvider<T>(typeof(T).Name + "s", source);
        }
    }

    public class BusinessObjectProvider<TFeature> : IProvider
    {
        private static FeatureDataTable SchemaTable;
        private static readonly List<Func<TFeature, object>> GetDelegates;
        
        static BusinessObjectProvider()
        {
            GetDelegates = new List<Func<TFeature, object>>();
            SchemaTable = new FeatureDataTable();
            Configure();
        }

        static Func<TFeature, object> CreateGetFuncFor<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "obj");
            var property = Expression.PropertyOrField(parameter, propertyName);
            var convert = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda(typeof(Func<T, object>), convert, parameter);

            return (Func<TFeature, object>)lambda.Compile();
        }

        static void Configure()
        {
            foreach (var tuple in GetPublicMembers(typeof(TFeature)))
            {
                var memberName = tuple.Item1;
                var attributes = tuple.Item2;

                GetDelegates.Add(CreateGetFuncFor<TFeature>(memberName));
                var col = SchemaTable.Columns.Add(memberName, TypeUtility<TFeature>.GetMemberType(memberName));
                col.Caption = string.IsNullOrEmpty(attributes.Name) ? memberName : attributes.Name;
                col.AllowDBNull = attributes.AllowNull;
                col.ReadOnly = attributes.IsReadOnly;
                col.Unique = attributes.IsUnique;
            }
        }

        static IEnumerable<Tuple<string, BusinessObjectAttributeAttribute>> GetPublicMembers(Type t)
        {
            var list = new List<Tuple<string, BusinessObjectAttributeAttribute>>();

            GetPublicMembers(t, list);
            list.Sort((a, b) => a.Item2.Ordinal.CompareTo(b.Item2.Ordinal));

            return list;
        }

        static void GetPublicMembers(Type t,
            List<Tuple<string, BusinessObjectAttributeAttribute>> collection)
        {
            var pis = t.GetProperties(/*BindingFlags.Public | BindingFlags.Instance*/);
            foreach (var pi in pis)
            {
                var att = pi.GetCustomAttributes(typeof (BusinessObjectAttributeAttribute), true);
                if (att.Length > 0)
                    collection.Add(Tuple.Create(pi.Name, (BusinessObjectAttributeAttribute)att[0]));
            }
            var fis = t.GetFields(/*BindingFlags.Public | BindingFlags.Instance*/);
            foreach (var fi in fis)
            {
                var att = fi.GetCustomAttributes(typeof(BusinessObjectAttributeAttribute), true);
                if (att.Length > 0)
                    collection.Add(Tuple.Create(fi.Name, (BusinessObjectAttributeAttribute)att[0]));
            }
            if (t.BaseType != typeof (object))
                GetPublicMembers(t.BaseType, collection);
        }

        private readonly IBusinessObjectSource<TFeature> _source;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="connectionID"/>
        /// <param name="featureSource">The feature source</param>
        public BusinessObjectProvider(string connectionID, IBusinessObjectSource<TFeature> featureSource)
        {
            ConnectionID = connectionID;
            _source = featureSource;
            SchemaTable.TableName = featureSource.Title;
        }

        public void Dispose()
        {
            if (_source is IDisposable)
                ((IDisposable)_source).Dispose();
        }

        public string ConnectionID { get; private set; }
        
        public bool IsOpen { get; private set; }
        
        public int SRID { get; set; }

        /// <summary>
        /// Gets a reference to the business object source
        /// </summary>
        public IBusinessObjectSource<TFeature> Source { get { return _source; } }

        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var res = new Collection<IGeometry>();
            foreach (TFeature feature in _source.Select(bbox))
            {
                res.Add(_source.GetGeometry(feature));
            }
            return res;
        }

        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var res = new Collection<uint>();
            foreach (TFeature feature in _source.Select(bbox))
            {
                res.Add(_source.GetId(feature));
            }
            return res;
        }

        public IGeometry GetGeometryByID(uint oid)
        {
            var f = _source.Select(oid);
            if (f != null)
                return _source.GetGeometry(f);
            return null;
        }

        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var resTable = (FeatureDataTable)SchemaTable.Copy();
            resTable.BeginLoadData();
            foreach (var feature in _source.Select(geom))
            {
                var fdr = (FeatureDataRow)resTable.LoadDataRow(ToItemArray(feature), LoadOption.OverwriteChanges);
                fdr.Geometry = _source.GetGeometry(feature);
            }
            resTable.EndLoadData();
            ds.Tables.Add(resTable);
        }

        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            var resTable = (FeatureDataTable)SchemaTable.Copy();
            resTable.BeginLoadData();
            foreach (var feature in _source.Select(box))
            {
                var fdr = (FeatureDataRow)resTable.LoadDataRow(ToItemArray(feature), LoadOption.OverwriteChanges);
                fdr.Geometry = _source.GetGeometry(feature);
            }
            resTable.EndLoadData();
            ds.Tables.Add(resTable);
        }

        public int GetFeatureCount()
        {
            return _source.Count;
        }

        public FeatureDataRow GetFeature(uint rowId)
        {
            var fdr = (FeatureDataRow)SchemaTable.NewRow();
            var f = _source.Select(rowId);
            fdr.ItemArray = ToItemArray(f);
            fdr.Geometry = _source.GetGeometry(f);
            return fdr;
        }

        private static object[] ToItemArray(TFeature feature)
        {
            var items = new List<object>();
            foreach (var d in GetDelegates)
            {
                items.Add(d(feature));
            }
            return items.ToArray();
        }

        public Envelope GetExtents()
        {
            return _source.GetExtents();
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }
    }
}