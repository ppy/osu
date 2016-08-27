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
using System.Threading.Tasks;

namespace osu.Framework.Graphics.Shaders
{
    public class Shader : IDisposable
    {
        internal StringBuilder Log = new StringBuilder();
        internal bool Loaded;
        internal bool IsBound;

        private string name;
        private int programID = -1;

        /// <summary>
        /// This is used when binding and unbinding to remember which shader was previously bound.
        /// </summary>
        private int previousShader;

        private static List<Shader> allShaders = new List<Shader>();
        private static Dictionary<string, object> globalProperties = new Dictionary<string, object>();

        private Dictionary<string, UniformBase> uniforms = new Dictionary<string, UniformBase>();

        internal Shader(string name)
        {
            this.name = name;
        }

        internal Shader(string name, List<ShaderPart> parts)
            : this(name)
        {
            Compile(parts);
        }

        #region Disposal
        ~Shader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (Loaded)
            {
                Unbind();

                GLWrapper.DeleteProgram(this);
                Loaded = false;
                programID = -1;
                allShaders.Remove(this);
            }
        }
        #endregion

        internal void Compile(List<ShaderPart> parts)
        {
            parts.RemoveAll(p => p == null);
            uniforms.Clear();
            Log.Clear();

            if (programID != -1)
                Dispose(true);

            if (parts.Count == 0)
                return;

            programID = GL.CreateProgram();
            for (int i = 0; i < parts.Count; i++)
                GL.AttachShader(this, parts[i]);

            GL.BindAttribLocation(this, 0, "m_Position");
            GL.BindAttribLocation(this, 1, "m_Colour");
            GL.BindAttribLocation(this, 2, "m_TexCoord");
            GL.BindAttribLocation(this, 3, "m_Time");
            GL.BindAttribLocation(this, 4, "m_Direction");

            GL.LinkProgram(this);

            int linkResult = 0;
            GL.GetProgram(this, GetProgramParameterName.LinkStatus, out linkResult);
            string linkLog = GL.GetProgramInfoLog(this);

            Log.AppendLine(string.Format(ShaderPart.BOUNDARY, name));
            Log.AppendLine(string.Format("Linked: {0}", linkResult == 1));
            if (linkResult == 0)
            {
                Log.AppendLine("Log:");
                Log.AppendLine(linkLog);
            }

            for (int i = 0; i < parts.Count; i++)
                GL.DetachShader(this, parts[i]);

            Loaded = linkResult == 1;

            if (Loaded)
            {
                //Obtain all the shader uniforms
                int uniformCount = 0;
                GL.GetProgram(this, GetProgramParameterName.ActiveUniforms, out uniformCount);
                for (int i = 0; i < uniformCount; i++)
                {
                    int size = 0;
                    int length = 0;
                    ActiveUniformType type;
                    StringBuilder uniformName = new StringBuilder(100);
                    GL.GetActiveUniform(this, i, 100, out length, out size, out type, uniformName);

                    string strName = uniformName.ToString();

                    uniforms.Add(strName, new UniformBase(this, strName, GL.GetUniformLocation(this, strName), type));
                }

                foreach (KeyValuePair<string, object> kvp in globalProperties)
                {
                    if (!uniforms.ContainsKey(kvp.Key))
                        continue;
                    uniforms[kvp.Key].Value = kvp.Value;
                }

                allShaders.Add(this);
            }
        }

        public void Bind()
        {
            if (IsBound)
                return;

            if (!Loaded)
                return;

            GLWrapper.UseProgram(this);
            previousShader = GLWrapper.CurrentShader;

            foreach (var kvp in uniforms)
                kvp.Value.Update();

            IsBound = true;
        }

        public void Unbind()
        {
            if (!IsBound)
                return;

            GLWrapper.UseProgram(previousShader);

            IsBound = false;
        }

        /// <summary>
        /// Returns a uniform from the shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <returns>Returns a base uniform.</returns>
        public Uniform<T> GetUniform<T>(string name)
        {
            if (!uniforms.ContainsKey(name))
                return new Uniform<T>(null);
            return new Uniform<T>(uniforms[name]);
        }

        /// <summary>
        /// Sets a uniform for all shaders that contain this property.
        /// <para>Any future-initialized shaders will also have this uniform set.</para>
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The uniform value.</param>
        public static void SetGlobalProperty(string name, object value)
        {
            globalProperties[name] = value;

            for (int i = 0; i < allShaders.Count; i++)
            {
                if (allShaders[i].uniforms.ContainsKey(name))
                    allShaders[i].uniforms[name].Value = value;
            }
        }

        public static implicit operator int(Shader shader)
        {
            return shader.programID;
        }
    }
}
