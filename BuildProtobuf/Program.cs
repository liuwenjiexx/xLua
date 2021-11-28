using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildProtobuf
{
    class Program
    {
        public static string SourceDir = "./";
        public static string Extension = ".proto";
        public static string OutputLuaDir;
        public static string ProtocPath = "protoc.exe";
        public static string OutputPbFile;
        public static string DataFile = "data.txt";
        public static string ClientToServerDir = "CS";
        public static string ServerToClientDir = "SC";
        public static string CSLuaFile = "ProtoCSCmd.lua";
        public static string SCLuaFile = "ProtoSCCmd.lua";
        public static string LuaFile = "Proto.lua";
        public static bool ResetID = false;
        public static int InitalizeID = 10000;

        static void Main(string[] args)
        {
            try
            {
                var dic = ParseArgs(args);

                TryGetArg(dic, "-Source", ref SourceDir);
                TryGetArg(dic, "-Extension", ref Extension);
                TryGetArg(dic, "-Protoc", ref ProtocPath);
                TryGetArg(dic, "-SC", ref ServerToClientDir);
                TryGetArg(dic, "-CS", ref ClientToServerDir);
                // TryGetArg(dic, "-Lua", ref LuaFile);

                string str = null;
                if (TryGetArg(dic, "-InitalizeID", ref str))
                {
                    InitalizeID = int.Parse(str);
                }
                if (TryGetArg(dic, "-ResetID", ref str))
                {
                    ResetID = bool.Parse(str);
                }
                string fullSrcDir = Path.GetFullPath(SourceDir);

                if (!Directory.Exists(fullSrcDir))
                {
                    throw new System.Exception("Directory not exists. dir: " + fullSrcDir);
                }

                var messages = LoadMessages(fullSrcDir);

                if (TryGetArg(dic, "-Pb", ref OutputPbFile))
                {
                    BuildProtoPB(messages.Select(o => o.Path).Distinct(), fullSrcDir, OutputPbFile);
                    Console.WriteLine($"Build {OutputPbFile} done");
                }

                if (TryGetArg(dic, "-Lua", ref LuaFile))
                {
                    BuildProtoLua(messages, LuaFile);
                    //BuildProtoLua(messages, Path.Combine(OutputLuaDir, CSLuaFile));

                    //BuildProtoLua(messages.Where(o => !o.IsClientToServer), Path.Combine(OutputLuaDir, SCLuaFile));

                }

                StringBuilder sb = new StringBuilder();
                foreach (var msg in messages)
                {
                    sb.Append(msg.FullName)
                        .Append("=")
                        .Append(msg.id)
                        .AppendLine();
                }
                sb.AppendLine();
                File.WriteAllText(DataFile, sb.ToString(), Encoding.UTF8);
                Console.WriteLine("Build success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        static bool TryGetArg(Dictionary<string, string> args, string key, ref string value)
        {
            if (args.ContainsKey(key))
            {
                value = args[key];
                return true;
            }
            return false;
        }

        static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var arg in args)
            {
                var parts = arg.Split('=');
                string key, value = null;
                key = parts[0];
                if (parts.Length > 1)
                {
                    value = parts[1];
                }
                values[key] = value;
            }
            return values;
        }
        static List<ProtoMessageInfo> LoadMessages(string dir)
        {
            string filter = "*" + Extension;

            Dictionary<string, int> oldIds = new Dictionary<string, int>();

            if (!ResetID && File.Exists(DataFile))
            {
                foreach (string line in File.ReadAllLines(DataFile))
                {
                    if (string.IsNullOrEmpty(line))
                        break;
                    var parts = line.Split('=');
                    if (parts.Length == 1)
                        break;
                    string msgName = parts[0];
                    int msgId = 0;
                    int.TryParse(parts[1], out msgId);
                    oldIds[msgName] = msgId;
                }

            }

            List<ProtoMessageInfo> messages = new List<ProtoMessageInfo>();
            foreach (var msg in FindProtoFiles(Path.Combine(dir, ClientToServerDir), filter))
            {
                msg.IsClientToServer = true;
                messages.Add(msg);
            }

            foreach (var msg in FindProtoFiles(Path.Combine(dir, ServerToClientDir), filter))
            {
                msg.IsClientToServer = false;
                messages.Add(msg);
            }

            if (oldIds.Count > 0)
            {
                foreach (var msg in messages)
                {
                    if (!oldIds.ContainsKey(msg.FullName))
                    {
                        oldIds.Remove(msg.FullName);
                    }
                }
            }

            messages = messages.OrderBy(o => o.FullName).ToList();

            int index = 0;
            int nextId = InitalizeID;
            foreach (var msg in messages)
            {
                msg.index = index;

                if (oldIds.TryGetValue(msg.FullName, out var id))
                {
                    msg.id = id;
                }
                else
                {

                    while (true)
                    {
                        nextId++;
                        if (!oldIds.ContainsValue(nextId))
                            break;
                    }
                    msg.id = nextId;
                }
                index++;
            }


            return messages;
        }
        static IEnumerable<ProtoMessageInfo> FindProtoFiles(string dir, string filter)
        {
            string fullDir = Path.GetFullPath(dir);

            if (Directory.Exists(fullDir))
            {
                foreach (var file in Directory.GetFiles(fullDir, filter, SearchOption.AllDirectories))
                {
                    foreach (var msg in ProtoMessageInfo.Parse(File.ReadAllText(file, Encoding.UTF8)))
                    {
                        msg.Path = file;
                        yield return msg;
                    }
                }
            }
        }

        public static void BuildProtoPB(IEnumerable<string> protoFiles, string rootDir, string outputPath)
        {
            string pbcPath = Path.GetFullPath(ProtocPath);
            if (!File.Exists(pbcPath))
                throw new Exception("Protoc file not exists . " + pbcPath);

            StringBuilder cmdText = new StringBuilder();
            cmdText.Append("-o \"")
                .Append(Path.GetFullPath(outputPath))
                .Append("\"");

            int index;
            if (rootDir.EndsWith("\\") || rootDir.EndsWith("/"))
                index = rootDir.Length;
            else
                index = rootDir.Length + 1;
            foreach (var file in protoFiles)
            {
                cmdText.Append(" \"")
                   .Append(file.Substring(index))
                    .Append("\"");
            }
            using (var proc = new System.Diagnostics.Process())
            {
                proc.StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WorkingDirectory = rootDir,
                    FileName = pbcPath,
                    Arguments = cmdText.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                };
                StringBuilder error = new StringBuilder();
                proc.OutputDataReceived += (o, e) =>
                {
                    error.AppendLine(e.Data);
                };
                proc.ErrorDataReceived += (o, e) =>
                {
                    error.AppendLine(e.Data);
                };

                proc.Start();
                proc.WaitForExit();
                if (error.Length > 0)
                {
                    throw new Exception(error.ToString());
                }
            }
        }


        static void BuildProtoLua(IEnumerable<ProtoMessageInfo> messages, string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;

            int count = messages.Count();
            var mapIndex = new Dictionary<ProtoMessageInfo, int>();

            if (count > 0)
            {
                string packageName;

                var first = messages.First();
                packageName = first.PackageName;
                sb.AppendLine("-- ***该文件为自动生成的***");

                sb.AppendLine($"local package = \"{packageName}\"");
                sb.AppendLine("local p= {");

                index = 0;
                foreach (var msg in messages)
                {
                    sb.Append($"[{(index + 1)}] = ");
                    if (string.IsNullOrEmpty(packageName))
                    {
                        sb.Append($"\"{msg.Name}\"");
                    }
                    else
                    {
                        sb.Append($"package..\".{msg.Name}\"");
                    }
                    if (index < count - 1)
                        sb.Append(",");
                    sb.AppendLine();
                    mapIndex[msg] = index;
                    index++;
                }
                sb.AppendLine("}");
            }

            sb.AppendLine("return {");
            sb.AppendLine("  cs = {");
            Build(messages.Where(o => o.IsClientToServer));
            sb.AppendLine("},")
                .AppendLine("sc ={");
            Build(messages.Where(o => !o.IsClientToServer));
            sb.AppendLine("}");



            sb.AppendLine("}");
            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(outputPath, sb.ToString(), encoding);


            Console.WriteLine($"Build {outputPath} done.");


            void Build(IEnumerable<ProtoMessageInfo> items)
            {
                int count = items.Count();

                sb.AppendLine("    id = {");
                index = 0;
                foreach (var msg in items)
                {
                    sb.Append($"[{msg.id}] = p[{(mapIndex[msg] + 1)}]");
                    if (index < count - 1)
                        sb.Append(",");
                    sb.AppendLine();
                    index++;
                }
                sb.AppendLine("},");

                sb.AppendLine("    msg = {");
                index = 0;
                foreach (var msg in items)
                {
                    sb.Append($"[\"{msg.Name}\"] = p[{(mapIndex[msg] + 1)}]");
                    if (index < count - 1)
                        sb.Append(",");
                    sb.AppendLine();
                    index++;
                }
                sb.AppendLine("},");

                sb.AppendLine("  msgToId = {");
                index = 0;
                foreach (var msg in items)
                {
                    sb.Append($"[\"{msg.Name}\"] = {msg.id}");
                    if (index < count - 1)
                        sb.Append(",");
                    sb.AppendLine();
                    index++;
                }
                sb.AppendLine("}");
            }

        }


    }


    class ProtoMessageInfo
    {
        public string PackageName;
        public string Name;
        public string FullName;
        public int Version;
        public int id;
        public int index;
        public bool IsClientToServer;
        public string Path;

        static Regex regexVersion = new Regex("syntax\\s*=\\s*\"proto(?<Version>[^\"]+)");
        static Regex regexPackage = new Regex("package\\s*(?<Name>[^\\s;]+)");
        static Regex regexMessage = new Regex("^\\s*message\\s+(?<Name>[^\\s\\{]+)", RegexOptions.Multiline);

        public static IEnumerable<ProtoMessageInfo> Parse(string text)
        {
            int version = 3;
            string packageName = "";

            var m = regexVersion.Match(text);

            if (m.Success)
            {
                if (int.TryParse(m.Groups["Version"].Value, out var n))
                {
                    version = n;
                }
            }

            m = regexPackage.Match(text);

            if (m.Success)
            {
                packageName = m.Groups["Name"].Value;
            }

            foreach (Match m1 in regexMessage.Matches(text))
            {
                ProtoMessageInfo msg = new ProtoMessageInfo()
                {
                    Name = m1.Groups["Name"].Value,
                    PackageName = packageName,
                    Version = version,

                };
                if (!string.IsNullOrEmpty(packageName))
                {
                    msg.FullName = msg.PackageName + "." + msg.Name;
                }
                else
                {
                    msg.FullName = msg.Name;
                }

                yield return msg;

            }
        }
    }
}
