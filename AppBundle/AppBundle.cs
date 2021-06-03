using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AppBundle
{
    public class AppBundle
    {

        public readonly string App;
        private static readonly Dictionary<string, object> Resources = new Dictionary<string, object>();

        public AppBundle(string app)
        {
            App = app;
        }

        public AppBundle AddResource(string name, byte[] bytes)
        {
            Resources[name] = bytes;
            return this;
        }

        public AppBundle AddResource(string name, Assembly assembly)
        {
            Resources[name] = assembly;
            return this;
        }

        public AppBundle AddResource(string name, string str)
        {
            Resources[name] = str;
            return this;
        }

        public void Generate()
        {
            using (var programStream = new BinaryWriter(File.Open(App, FileMode.Append)))
            {
                var originalLength = programStream.BaseStream.Length;

                foreach (var resource in Resources)
                {
                    byte type;
                    byte[] bytes;
                    switch (resource.Value)
                    {
                        case byte[] b:
                            type = 0;
                            bytes = b;
                            break;
                        case Assembly assembly:
                            type = 1;
                            bytes = File.ReadAllBytes(assembly.Location);
                            break;
                        case string str:
                            type = 2;
                            bytes = Encoding.ASCII.GetBytes(str);
                            break;
                        default:
                            throw new ArgumentException("Cannot bundle a resource of the type: " +
                                                        (resource.Value == null
                                                            ? "null"
                                                            : resource.GetType().ToString()));
                    }

                    programStream.Write(type);
                    var nameBytes = Encoding.ASCII.GetBytes(resource.Key);
                    programStream.Write((long) nameBytes.Length);
                    programStream.Write(nameBytes);
                    programStream.Write((long) bytes.Length);
                    programStream.Write(bytes);
                }

                programStream.Write(programStream.BaseStream.Length - originalLength);
            }
        }

    }
}