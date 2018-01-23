using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ComparisonExpressionVisualizer
{

    public class History<T>
    {
        private IList<T> _history = new List<T>();
        public IList<T> Load(string name)
        {
            if (string.IsNullOrEmpty(name) || File.Exists($"{name}.dat") == false)
                return default(List<T>);

            using (var stream = File.OpenRead($"{name}.dat"))
            {
                try
                {
                    _history = (List<T>)new XmlSerializer(typeof(List<T>)).Deserialize(stream);
                }
                catch(Exception ex)
                {
                    throw new Exception("Error while serialization of history data. Please delete dat files from bin directory.");
                }
            }
            return _history;
        }

        public void Save(string name, T obj)
        {
            _history.Add(obj);
            _history = _history.Distinct().ToList();
            using (var stream = File.OpenWrite($"{name}.dat"))
            {
                new XmlSerializer(typeof(List<T>)).Serialize(stream, _history);
            }
        }
    }
    public static class SerializationExtensions
    {
        public static XElement Serialize(this object source)
        {
            try
            {
                var serializer = XmlSerializerFactory.GetSerializerFor(source.GetType());
                var xdoc = new XDocument();
                using (var writer = xdoc.CreateWriter())
                {
                    serializer.Serialize(writer, source, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
                }

                return (xdoc.Document != null) ? xdoc.Document.Root : new XElement("Error", "Document Missing");
            }
            catch (Exception x)
            {
                return new XElement("Error", x.ToString());
            }
        }

        public static T Deserialize<T>(this XElement source) where T : class
        {
            try
            {
                var serializer = XmlSerializerFactory.GetSerializerFor(typeof(T));

                return (T)serializer.Deserialize(source.CreateReader());
            }
            catch //(Exception x)
            {
                return null;
            }
        }
    }

    public static class XmlSerializerFactory
    {
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        public static XmlSerializer GetSerializerFor(Type typeOfT)
        {
            if (!serializers.ContainsKey(typeOfT))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("XmlSerializerFactory.GetSerializerFor(typeof({0}));", typeOfT));

                var newSerializer = new XmlSerializer(typeOfT);
                serializers.Add(typeOfT, newSerializer);
            }

            return serializers[typeOfT];
        }
    }
}
