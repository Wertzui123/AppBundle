using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppBundle
{
    public static class Loader
    {

        private static Dictionary<string, object> _resources;
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        public static Dictionary<string, object> GetResources()
        {
            if (_resources == null) LoadResources();
            return _resources;
        }

        public static object GetResource(string name)
        {
            return GetResources()[name];
        }

        public static Assembly GetAssembly(string name)
        {
            if (_resources == null) LoadResources();
            if (Assemblies.ContainsKey(name)) return Assemblies[name];
            return (Assembly) _resources[name];
        }

        public static void Main()
        {
            LoadResources();
        }

        private static void LoadResources()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, a) =>
            {
                var assembly = GetAssembly(a.Name);
                return assembly;
            };

            var resources = new Dictionary<string, object>();

            using (var stream = new BinaryReader(new FileStream(Process.GetCurrentProcess().MainModule.FileName,
                FileMode.Open, FileAccess.Read)))
            {
                stream.BaseStream.Position = stream.BaseStream.Length - sizeof(long);
                stream.BaseStream.Position -= stream.ReadInt64();

                while (stream.BaseStream.Length - stream.BaseStream.Position > sizeof(long))
                {
                    var type = stream.ReadByte();

                    var nameLength = stream.ReadInt64();
                    var nameBytes = stream.ReadBytes((int) nameLength);

                    var name = Encoding.ASCII.GetString(nameBytes.ToArray());
                    var length = stream.ReadInt64();
                    var bytes = stream.ReadBytes((int) length);

                    switch (type)
                    {
                        case 0:
                            resources[name] = bytes;
                            break;
                        case 1:
                            resources[name] = Assembly.Load(bytes);
                            Assemblies[((Assembly) resources[name]).FullName] = (Assembly) resources[name];
                            break;
                        case 2:
                            resources[name] = Encoding.ASCII.GetString(bytes);
                            break;
                        default:
                            throw new ArgumentException("Unknown resource type id: " + type);
                    }
                }

                _resources = resources;
            }
        }

    }
}