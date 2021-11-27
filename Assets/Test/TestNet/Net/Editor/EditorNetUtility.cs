using NUnit.Framework.Internal.Filters;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using XLua;
using Yoozoo.Managers.NetworkV2.Core;

public static class EditorNetUtility
{
    public static string protoInputPath = @"Assets/Test/TestNet/Proto";
    public static string protoExtension = "*.proto";
    public static string protoOutputPath = "Assets/Test/TestNet/Resources/Lua/Proto";
    public static string protocPath = "Tools/protoc.exe";

    [CSharpCallLua]
    public static List<Type> s
    {
        get
        {
            List<Type> list = new List<Type>();
            list.AddRange(new Type[] {
                typeof(NetDelegates.ChannelDelegates.NetworkChannelErrorHandler )
            });
            return list;
        }
    }


    [MenuItem("Tools/Build Proto")]
    public static void BuildProto()
    {
        BuildLuaProto(protoInputPath, protoExtension, protoOutputPath);
    }

    public static void BuildLuaProto(string dir, string filter, string outputDir)
    {
        if (!Directory.Exists(dir))
            throw new System.Exception("Directory not exists. dir: " + dir);


        BuildLuaProto2(Path.Combine(dir, "CS"), filter, Path.Combine(outputDir, "ProtoCSCmd.lua"));

        BuildLuaProto2(Path.Combine(dir, "SC"), filter, Path.Combine(outputDir, "ProtoSCCmd.lua"));

        //BuildLuaProtoBytes(Path.Combine(dir, "CS") + "|" + Path.Combine(dir, "SC"), filter, Path.Combine(outputDir, "Proto.pb.bytes"));
        BuildProtoPB(dir, filter, Path.Combine(outputDir, "Proto.pb.bytes"));
        Debug.Log("Build proto done\n" + outputDir);
    }

    public static void BuildLuaProto2(string dir, string filter, string outputPath)
    {
        List<ProtoMessageInfo> list = new List<ProtoMessageInfo>();

        if (Directory.Exists(dir))
        {
            foreach (var file in Directory.GetFiles(dir, filter, SearchOption.AllDirectories))
            {
                ProtoMessageInfo.Parse(File.ReadAllText(file, Encoding.UTF8), list);
            }

            list.Sort((a, b) => string.Compare(a.FullName, b.FullName));

            for (int i = 0; i < list.Count; i++)
            {
                var msg = list[i];
                msg.id = 10000 + i + 1;
            }

        }

        StringBuilder sb = new StringBuilder();

        if (list.Count > 0)
        {
            string packageName;

            var first = list[0];
            packageName = first.PackageName;
            sb.AppendLine("-- ***该文件为自动生成的***");

            sb.AppendLine($"local package = \"{packageName}\"");
            sb.AppendLine("local p= {");

            for (int i = 0; i < list.Count; i++)
            {
                var msg = list[i];
                sb.Append($"[{(i + 1)}] = ");
                if (string.IsNullOrEmpty(packageName))
                {
                    sb.Append($"\"{msg.Name}\"");
                }
                else
                {
                    sb.Append($"package..\".{msg.Name}\"");
                }
                if (i < list.Count - 1)
                    sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("}");
        }

        sb.AppendLine("return {");

        sb.AppendLine("  id = {");
        for (int i = 0; i < list.Count; i++)
        {
            var msg = list[i];
            sb.Append($"[{msg.id}] = p[{(i + 1)}]");
            if (i < list.Count - 1)
                sb.Append(",");
            sb.AppendLine();
        }
        sb.AppendLine("},");

        sb.AppendLine("  msg = {");
        for (int i = 0; i < list.Count; i++)
        {
            var msg = list[i];
            sb.Append($"[\"{msg.Name}\"] = p[{(i + 1)}]");
            if (i < list.Count - 1)
                sb.Append(",");
            sb.AppendLine();
        }
        sb.AppendLine("},");

        sb.AppendLine("  msgToId = {");
        for (int i = 0; i < list.Count; i++)
        {
            var msg = list[i];
            sb.Append($"[\"{msg.Name}\"] = {msg.id}");
            if (i < list.Count - 1)
                sb.Append(",");
            sb.AppendLine();
        }
        sb.AppendLine("}");
        sb.AppendLine("}");
        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        Encoding encoding = new UTF8Encoding(false);
        File.WriteAllText(outputPath, sb.ToString(), encoding);
    }

    public static void BuildProtoPB(string dir, string filter, string outputPath)
    {
        StringBuilder cmdText = new StringBuilder();
        cmdText.Append("-o \"")
            .Append(Path.GetFullPath(outputPath))
            .Append("\"");

        if (Directory.Exists(dir))
        {
            string fullDir = Path.GetFullPath(dir);
            int index;
            if (dir.EndsWith("\\") || dir.EndsWith("/"))
                index = dir.Length;
            else
                index = dir.Length + 1;
            foreach (var file in Directory.GetFiles(dir, filter, SearchOption.AllDirectories))
            {
                cmdText.Append(" \"")
                   .Append(file.Substring(index))
                    .Append("\"");
            }
        }
        using (var proc = new System.Diagnostics.Process())
        {
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory = dir,
                FileName = Path.GetFullPath(protocPath),
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

        static Regex regexVersion = new Regex("syntax\\s*=\\s*\"proto(?<Version>[^\"]+)");
        static Regex regexPackage = new Regex("package\\s*(?<Name>[^\\s;]+)");
        static Regex regexMessage = new Regex("^\\s*message\\s+(?<Name>[^\\s\\{]+)", RegexOptions.Multiline);

        public static void Parse(string text, List<ProtoMessageInfo> result)
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

                result.Add(msg);

            }
        }
    }
}
