using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EasyRun.Core
{
    /// <summary>Tiny JSON (de)serialization helper over DataContractJsonSerializer, used for export and backups.</summary>
    internal static class JsonFile
    {
        private static DataContractJsonSerializer Serializer(System.Type type) =>
            new DataContractJsonSerializer(type, new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

        public static void Write(string file, object value)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, ownsStream: false, indent: true))
                {
                    Serializer(value.GetType()).WriteObject(writer, value);
                }
                File.WriteAllBytes(file, stream.ToArray());
            }
        }

        public static T Read<T>(string file)
        {
            using (var stream = new MemoryStream(File.ReadAllBytes(file)))
            {
                return (T)Serializer(typeof(T)).ReadObject(stream);
            }
        }
    }
}
