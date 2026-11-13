using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace EasyRun.Core
{
    /// <summary>A backed-up alias definition (taken before it was overwritten or removed).</summary>
    [DataContract(Name = "backup")]
    public sealed class AliasBackup
    {
        /// <summary>The backup file name (used as the restore id).</summary>
        public string Id { get; set; }

        [DataMember(Name = "timestamp", Order = 0)]
        public string TimestampIso { get; set; }

        [DataMember(Name = "item", Order = 1)]
        public AliasExportItem Item { get; set; }

        public DateTime TimestampUtc =>
            DateTime.TryParse(TimestampIso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime t)
                ? t.ToUniversalTime()
                : DateTime.MinValue;
    }

    /// <summary>Stores backups of overwritten/removed aliases (or fakes them, in tests).</summary>
    public interface IAliasBackupStore
    {
        void Save(AliasExportItem item);

        IReadOnlyList<AliasBackup> List();

        AliasBackup Load(string id);

        string Directory { get; }
    }

    /// <summary>File-backed backups under %LOCALAPPDATA%\EasyRun\backups (one JSON file each).</summary>
    public sealed class FileAliasBackupStore : IAliasBackupStore
    {
        private readonly IEasyRunPaths _paths;
        private readonly int _keep;

        public FileAliasBackupStore(IEasyRunPaths paths, int keep = 200)
        {
            _paths = paths;
            _keep = keep;
        }

        public string Directory => _paths.BackupsDirectory;

        public void Save(AliasExportItem item)
        {
            if (item == null) return;
            System.IO.Directory.CreateDirectory(_paths.BackupsDirectory);
            DateTime now = DateTime.UtcNow;
            string baseName = now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + "-" + SafeName(item.Name);
            string file = Path.Combine(_paths.BackupsDirectory, baseName + ".json");
            for (int i = 2; File.Exists(file); i++)
            {
                file = Path.Combine(_paths.BackupsDirectory, baseName + "-" + i + ".json");
            }
            var backup = new AliasBackup { TimestampIso = now.ToString("o", CultureInfo.InvariantCulture), Item = item };
            JsonFile.Write(file, backup);
            Prune();
        }

        public IReadOnlyList<AliasBackup> List()
        {
            var result = new List<AliasBackup>();
            if (!System.IO.Directory.Exists(_paths.BackupsDirectory)) return result;
            foreach (string file in System.IO.Directory.GetFiles(_paths.BackupsDirectory, "*.json"))
            {
                try
                {
                    AliasBackup b = JsonFile.Read<AliasBackup>(file);
                    if (b?.Item == null) continue;
                    b.Id = Path.GetFileName(file);
                    result.Add(b);
                }
                catch (Exception)
                {
                    // Skip unreadable/foreign files.
                }
            }
            return result.OrderByDescending(b => b.TimestampUtc).ToList();
        }

        public AliasBackup Load(string id)
        {
            string file = Path.Combine(_paths.BackupsDirectory, Path.GetFileName(id));
            if (!File.Exists(file)) throw new EasyRunException(EasyRunError.NotFound, "Backup not found: " + id);
            AliasBackup b = JsonFile.Read<AliasBackup>(file);
            if (b?.Item == null) throw new EasyRunException(EasyRunError.InvalidArgument, "Not a valid backup: " + id);
            b.Id = Path.GetFileName(file);
            return b;
        }

        private void Prune()
        {
            try
            {
                foreach (string old in System.IO.Directory.GetFiles(_paths.BackupsDirectory, "*.json")
                             .OrderByDescending(Path.GetFileName, StringComparer.Ordinal).Skip(_keep))
                {
                    File.Delete(old);
                }
            }
            catch (Exception)
            {
                // Best effort.
            }
        }

        private static string SafeName(string name)
        {
            char[] bad = Path.GetInvalidFileNameChars();
            string clean = new string((name ?? "alias").Select(c => bad.Contains(c) ? '_' : c).ToArray());
            return clean.Length > 40 ? clean.Substring(0, 40) : clean;
        }
    }
}
