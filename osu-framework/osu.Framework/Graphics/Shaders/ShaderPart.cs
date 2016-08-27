//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework.Resources;

namespace osu.Framework.Graphics.Shaders
{
    internal class ShaderPart : IDisposable
    {
        internal const string BOUNDARY = @"----------------------{0}";

        internal StringBuilder Log = new StringBuilder();

        internal string Name;
        internal bool HasCode;
        internal bool Compiled;

        internal ShaderType Type;

        private int partID = -1;

        private List<string> shaderCodes = new List<string>();

        private Regex includeRegex = new Regex("^\\s*#\\s*include\\s+[\"<](.*)[\">]");

        private ShaderManager manager;

        internal ShaderPart(string name, byte[] data, ShaderType type, ShaderManager manager)
        {
            Name = name;
            Type = type;

            this.manager = manager;

            shaderCodes.Add(loadFile(data));
            shaderCodes.RemoveAll(c => string.IsNullOrEmpty(c));

            if (shaderCodes.Count == 0)
                return;

            HasCode = true;
        }

        private string loadFile(byte[] bytes)
        {
            if (bytes == null)
                return null;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (StreamReader sr = new StreamReader(ms))
            {
                string code = string.Empty;

                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();

                    Match includeMatch = includeRegex.Match(line);
                    if (includeMatch.Success)
                    {
                        string includeName = includeMatch.Groups[1].Value.Trim();

                        byte[] rawData = null;
#if DEBUG
                        if (File.Exists(includeName))
                            rawData = File.ReadAllBytes(includeName);
#endif
                        shaderCodes.Add(loadFile(manager.LoadRaw(includeName)));
                    }
                    else
                        code += '\n' + line;
                }

                return code;
            }
        }

        internal bool Compile()
        {
            if (!HasCode)
                return false;

            if (partID == -1)
                partID = GL.CreateShader(Type);

            int[] codeLengths = new int[shaderCodes.Count];
            for (int i = 0; i < shaderCodes.Count; i++)
                codeLengths[i] = shaderCodes[i].Length;

            GL.ShaderSource(this, shaderCodes.Count, shaderCodes.ToArray(), codeLengths);
            GL.CompileShader(this);

            int compileResult = 0;
            GL.GetShader(this, ShaderParameter.CompileStatus, out compileResult);
            Compiled = compileResult == 1;
            string compileLog = GL.GetShaderInfoLog(this);

            if (!Compiled)
                Dispose(true);

#if DEBUG
            Log.AppendLine(string.Format('\t' + BOUNDARY, Name));
            Log.AppendLine(string.Format("\tCompiled: {0}", Compiled));
            if (!Compiled)
            {
                Log.AppendLine("\tLog:");
                Log.AppendLine('\t' + compileLog);
            }
#endif

            return Compiled;
        }

        public static implicit operator int(ShaderPart program)
        {
            return program.partID;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || partID == -1) return;

            GLWrapper.DeleteShader(this);
            Compiled = false;
            partID = -1;
        }
    }
}
