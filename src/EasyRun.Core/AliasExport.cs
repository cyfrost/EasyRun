using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EasyRun.Core
{
    /// <summary>A portable alias definition (no secrets) for export/import.</summary>
    [DataContract(Name = "alias")]
    public sealed class AliasExportItem
    {
        [DataMember(Name = "name", Order = 0)] public string Name { get; set; }
        [DataMember(Name = "scope", Order = 1)] public string Scope { get; set; }
        [DataMember(Name = "kind", Order = 2)] public string Kind { get; set; }
        [DataMember(Name = "target", Order = 3)] public string Target { get; set; }
        [DataMember(Name = "arguments", Order = 4, EmitDefaultValue = false)] public string Arguments { get; set; }
        [DataMember(Name = "workingDirectory", Order = 5, EmitDefaultValue = false)] public string WorkingDirectory { get; set; }
        [DataMember(Name = "runAsAccount", Order = 6, EmitDefaultValue = false)] public string RunAsAccount { get; set; }
        [DataMember(Name = "description", Order = 7, EmitDefaultValue = false)] public string Description { get; set; }
    }

    /// <summary>The result of importing an alias file.</summary>
    public sealed class ImportSummary
    {
        public int Created { get; set; }
        public List<string> Skipped { get; } = new List<string>();

        public string Describe()
        {
            string msg = "Imported " + Created + (Created == 1 ? " alias." : " aliases.");
            if (Skipped.Count > 0) msg += "\nSkipped " + Skipped.Count + ":\n  " + string.Join("\n  ", Skipped);
            return msg;
        }
    }

    /// <summary>Reads and writes alias export files (JSON). Never contains passwords.</summary>
    public static class AliasExport
    {
        public static void Save(string file, IEnumerable<AliasExportItem> items)
        {
            var list = new List<AliasExportItem>(items);
            var serializer = new DataContractJsonSerializer(typeof(List<AliasExportItem>),
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            using (var stream = new MemoryStream())
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, ownsStream: false, indent: true))
                {
                    serializer.WriteObject(writer, list);
                }
                File.WriteAllBytes(file, stream.ToArray());
            }
        }

        public static List<AliasExportItem> Load(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            var serializer = new DataContractJsonSerializer(typeof(List<AliasExportItem>),
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            using (var stream = new MemoryStream(bytes))
            {
                try
                {
                    return (List<AliasExportItem>)serializer.ReadObject(stream) ?? new List<AliasExportItem>();
                }
                catch (SerializationException ex)
                {
                    throw new EasyRunException(EasyRunError.InvalidArgument, "Not a valid EasyRun alias file: " + file, ex);
                }
            }
        }
    }
}
