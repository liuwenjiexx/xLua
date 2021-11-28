using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class EditorProtobufUtility
{
    public static string SourcePath = @"Assets/Test/TestProtobuf/Proto";
    public static string ProtoExtension = ".proto";
    public static string OutputPath = "Assets/Test/TestProtobuf/Resources/Lua/Proto";
    public static string ProtocPath = "Tools/protoc.exe";

    [MenuItem("Tools/Build Proto Lua")]
    public static void BuildProtoLua()
    {
        BuildProtoLua(SourcePath, ProtoExtension, OutputPath);
    }

    static void BuildProtoLua(string dir, string extension, string outputDir)
    {
        if (!Directory.Exists(dir))
            throw new System.Exception("Directory not exists. dir: " + dir);

        string filter = "*" + extension;

        List<ProtoMessageInfo> messages = new List<ProtoMessageInfo>();
        foreach (var msg in FindProtoFiles(Path.Combine(dir, "CS"), filter))
        {
            msg.IsClientToServer = true;
            messages.Add(msg);
        }

        foreach (var msg in FindProtoFiles(Path.Combine(dir, "SC"), filter))
        {
            msg.IsClientToServer = false;
            messages.Add(msg);
        }

        int index = 0;
        foreach (var msg in messages.OrderBy(o => o.FullName))
        {
            msg.index = index;
            msg.id = 10001 + index;
            index++;
        }


        BuildProtoLua(messages.Where(o => o.IsClientToServer), Path.Combine(outputDir, "ProtoCSCmd.lua"));

        BuildProtoLua(messages.Where(o => !o.IsClientToServer), Path.Combine(outputDir, "ProtoSCCmd.lua"));

        BuildProtoPB(messages.Select(o => o.Path).Distinct(), dir, Path.Combine(outputDir, "Proto.pb.bytes"));

        AssetDatabase.Refresh();
        Debug.Log("Build proto done\n" + outputDir);
    }

    static IEnumerable<ProtoMessageInfo> FindProtoFiles(string dir, string filter)
    {
        string fullDir = Path.Combine(dir);

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

    static void BuildProtoLua(IEnumerable<ProtoMessageInfo> messages, string outputPath)
    {
        StringBuilder sb = new StringBuilder();
        int index = 0;

        int count = messages.Count();
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
                index++;
            }
            sb.AppendLine("}");
        }

        sb.AppendLine("return {");

        sb.AppendLine("  id = {");
        index = 0;
        foreach (var msg in messages)
        {
            sb.Append($"[{msg.id}] = p[{(index + 1)}]");
            if (index < count - 1)
                sb.Append(",");
            sb.AppendLine();
            index++;
        }
        sb.AppendLine("},");

        sb.AppendLine("  msg = {");
        index = 0;
        foreach (var msg in messages)
        {
            sb.Append($"[\"{msg.Name}\"] = p[{(index + 1)}]");
            if (index < count - 1)
                sb.Append(",");
            sb.AppendLine();
            index++;
        }
        sb.AppendLine("},");

        sb.AppendLine("  msgToId = {");
        index = 0;
        foreach (var msg in messages)
        {
            sb.Append($"[\"{msg.Name}\"] = {msg.id}");
            if (index < count - 1)
                sb.Append(",");
            sb.AppendLine();
            index++;
        }
        sb.AppendLine("}");
        sb.AppendLine("}");
        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        Encoding encoding = new UTF8Encoding(false);
        File.WriteAllText(outputPath, sb.ToString(), encoding);
    }

    public static void BuildProtoPB(IEnumerable<string> protoFiles, string rootDir, string outputPath)
    {
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
                FileName = Path.GetFullPath(ProtocPath),
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
    public static void BuildLuaProtoBytes(string dirs, string filter, string outputPath)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var dir in dirs.Split('|'))
        {
            if (Directory.Exists(dir))
            {
                Regex[] regices = new Regex[]{
                new Regex("^\\s*(\\r\\n|\\n)"),
                new Regex("^\\s*syntax\\s*=\\s*\"proto(?<Version>[^\"]+).*(\\r\\n|\\n)", RegexOptions.Multiline),
                new Regex("^\\s*package\\s*(?<Name>[^\\s;]+).*(\\r\\n|\\n)", RegexOptions.Multiline)
            };
                foreach (var file in Directory.GetFiles(dir, filter))
                {
                    string text = File.ReadAllText(file);
                    if (sb.Length == 0)
                    {
                        sb.AppendLine(text);
                        continue;
                    }

                    Match m = null;
                    bool match = true;
                    int index = 0;
                    while (match)
                    {
                        match = false;
                        foreach (var reg in regices)
                        {
                            m = reg.Match(text, index);
                            if (m.Success)
                            {
                                index = m.Index + m.Length;
                                match = true;
                                break;
                            }
                        }
                    }

                    if (index > 0)
                        sb.Append(text, index, text.Length - index);
                    else
                        sb.Append(text);
                    sb.AppendLine();
                }
            }
        }
        Encoding encoding = new UTF8Encoding(false);
        File.WriteAllText(outputPath, sb.ToString(), encoding);
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
