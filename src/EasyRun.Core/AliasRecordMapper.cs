using System;
using System.Collections.Generic;

namespace EasyRun.Core
{
    /// <summary>Maps an <see cref="AliasRecord"/> to and from the raw App Paths value dictionary.</summary>
    internal static class AliasRecordMapper
    {
        private const string MetaPrefix = "EasyRun.";
        private const string KindKey = MetaPrefix + "Kind";
        private const string TargetKey = MetaPrefix + "Target";
        private const string ArgsKey = MetaPrefix + "Arguments";
        private const string WorkDirKey = MetaPrefix + "WorkingDirectory";
        private const string ShortcutKey = MetaPrefix + "ShortcutFile";
        private const string TaskKey = MetaPrefix + "TaskName";
        private const string RunAsKey = MetaPrefix + "RunAsAccount";
        private const string DescKey = MetaPrefix + "Description";

        public static IDictionary<string, string> ToValues(AliasRecord record)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [string.Empty] = record.LaunchValue ?? string.Empty,
                [KindKey] = record.Kind.ToString(),
                [TargetKey] = record.Target ?? string.Empty,
            };
            // "Path" is the conventional App Paths working-directory value; keep it in sync.
            if (!string.IsNullOrEmpty(record.WorkingDirectory)) values["Path"] = record.WorkingDirectory;
            if (!string.IsNullOrEmpty(record.Arguments)) values[ArgsKey] = record.Arguments;
            if (!string.IsNullOrEmpty(record.WorkingDirectory)) values[WorkDirKey] = record.WorkingDirectory;
            if (!string.IsNullOrEmpty(record.ShortcutFile)) values[ShortcutKey] = record.ShortcutFile;
            if (!string.IsNullOrEmpty(record.TaskName)) values[TaskKey] = record.TaskName;
            if (!string.IsNullOrEmpty(record.RunAsAccount)) values[RunAsKey] = record.RunAsAccount;
            if (!string.IsNullOrEmpty(record.Description)) values[DescKey] = record.Description;
            return values;
        }

        public static AliasRecord FromValues(AliasScope scope, string key, IDictionary<string, string> values)
        {
            string Get(string name) => values.TryGetValue(name, out string v) ? v : null;

            bool managed = values.ContainsKey(KindKey);
            string launch = Get(string.Empty) ?? string.Empty;
            var record = new AliasRecord
            {
                Name = AliasName.Display(key),
                Scope = scope,
                ManagedByEasyRun = managed,
                LaunchValue = launch,
                Arguments = Get(ArgsKey) ?? string.Empty,
                WorkingDirectory = Get(WorkDirKey) ?? Get("Path") ?? string.Empty,
                ShortcutFile = Get(ShortcutKey),
                TaskName = Get(TaskKey),
                RunAsAccount = Get(RunAsKey),
                Description = Get(DescKey) ?? string.Empty,
            };

            if (managed && Enum.TryParse(Get(KindKey), out AliasKind kind))
            {
                record.Kind = kind;
            }
            else
            {
                // A non-EasyRun alias, or one missing metadata: infer a sensible kind from the launch value.
                record.Kind = InferKind(launch);
            }
            record.Target = Get(TargetKey) ?? launch;
            return record;
        }

        private static AliasKind InferKind(string launch)
        {
            if (string.IsNullOrEmpty(launch)) return AliasKind.Executable;
            if (launch.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) return AliasKind.Command;
            return AliasKind.Executable;
        }
    }
}
