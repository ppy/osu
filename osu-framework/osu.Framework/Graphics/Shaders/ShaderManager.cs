//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osu.Framework.Logging;
using osu.Framework.Resources;

namespace osu.Framework.Graphics.Shaders
{
    public class ShaderManager
    {
        private const string shader_prefix = @"sh_";

        private Dictionary<string, ShaderPart> partCache = new Dictionary<string, ShaderPart>();

        IResourceStore<byte[]> store;

        public ShaderManager(IResourceStore<byte[]> store)
        {
            this.store = store;
        }

        private string getFileEnding(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.FragmentShader:
                    return @".fs";
                case ShaderType.VertexShader:
                    return @".vs";
            }

            return string.Empty;
        }

        private string ensureValidName(string name, ShaderType type)
        {
            string ending = getFileEnding(type);
            if (!name.StartsWith(shader_prefix))
                name = shader_prefix + name;
            if (name.EndsWith(ending))
                return name;
            return name + ending;
        }

        internal byte[] LoadRaw(string name)
        {
            byte[] rawData = null;

#if DEBUG
            if (File.Exists(name))
                rawData = File.ReadAllBytes(name);
#endif

            if (rawData == null)
            {
                rawData = store.Get(name);

                if (rawData == null)
                    return null;
            }

            return rawData;
        }

        private ShaderPart createShaderPart(string name, ShaderType type, bool bypassCache = false)
        {
            name = ensureValidName(name, type);

            ShaderPart part;
            if (!bypassCache && partCache.TryGetValue(name, out part))
                return part;

            byte[] rawData = LoadRaw(name);

            part = new ShaderPart(name, rawData, type, this);
            bool compiled = part.Compile();

            //cache even on failure so we don't try and fail every time.
            partCache[name] = part;
            return part;
        }

        public Shader Load(string vertex, string fragment, bool continuousCompilation = false)
        {
            string name = $@"{vertex}/{fragment}";

            List<ShaderPart> parts = new List<ShaderPart>();
            parts.Add(createShaderPart(vertex, ShaderType.VertexShader));
            parts.Add(createShaderPart(fragment, ShaderType.FragmentShader));

            Shader shader = new Shader(name, parts);
#if !DEBUG
            if (!shader.Loaded)
#endif
            {
                StringBuilder logContents = new StringBuilder();
                logContents.AppendLine($@"Loading shader {name}:");
                logContents.Append(shader.Log);
                logContents.AppendLine(@"Parts:");
                foreach (ShaderPart p in parts)
                    logContents.Append(p.Log);
                Logger.Log(logContents.ToString(), LoggingTarget.Runtime, LogLevel.Debug);
            }

//#if DEBUG
//            if (continuousCompilation)
//            {
//                Game.Scheduler.AddDelayed(delegate
//                {
//                    parts.Clear();
//                    parts.Add(createShaderPart(vertex, ShaderType.VertexShader, true));
//                    parts.Add(createShaderPart(fragment, ShaderType.FragmentShader, true));
//                    shader.Compile(parts);

//                    StringBuilder cLogContents = new StringBuilder();
//                    cLogContents.AppendLine($@"Continuously loading shader {name}:");
//                    cLogContents.Append(shader.Log);
//                    cLogContents.AppendLine(@"Parts:");
//                    foreach (ShaderPart p in parts)
//                        cLogContents.Append(p.Log);

//                }, 1000, true);
//            }
//#endif

            return shader;
        }

        public Shader Load(VertexShader vertex, FragmentShader fragment, bool continuousCompilation = false)
        {
            return Load(vertex.ToString(), fragment.ToString(), continuousCompilation);
        }
    }

    public enum VertexShader
    {
        Texture2D,
        Texture3D,
        Position,
        Colour
    }

    public enum FragmentShader
    {
        Texture,
        Colour
    }
}
