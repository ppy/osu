//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics;
using System.Drawing;
using System.IO;
using OpenTK;
using System.Collections.Generic;
using System.Threading;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Logging;

namespace osu.Framework.Framework
{
    public class GLControl : OpenTK.GLControl
    {
        private string SupportedExtensions;

        internal Version GLVersion;
        internal Version GLSLVersion;

        public GLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
            : base(mode, major, minor, flags)
        {
        }

        public void Initialize()
        {
            string version = GL.GetString(StringName.Version);
            GLVersion = new Version(version.Split(' ')[0]);
            version = GL.GetString(StringName.ShadingLanguageVersion);
            if (!string.IsNullOrEmpty(version))
            {
                try
                {
                    GLSLVersion = new Version(version.Split(' ')[0]);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"couldn't set GLSL version using string '{version}'");
                }
            }

            if (GLSLVersion == null)
                GLSLVersion = new Version();

            //Set up OpenGL related characteristics
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);

            Logger.Log($@"GL Initialized
                        GL Version:                 { GL.GetString(StringName.Version)}
                        GL Renderer:                { GL.GetString(StringName.Renderer)}
                        GL Shader Language version: { GL.GetString(StringName.ShadingLanguageVersion)}
                        GL Vendor:                  { GL.GetString(StringName.Vendor)}
                        GL Extensions:              { GL.GetString(StringName.Extensions)}
                        GL Context:                 { GraphicsMode}", LoggingTarget.Runtime, LogLevel.Important);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Cursor.Hide();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor.Show();
            base.OnMouseLeave(e);
        }

        internal bool CheckExtension(string extensionName)
        {
            try
            {
                if (string.IsNullOrEmpty(SupportedExtensions))
                    SupportedExtensions = GL.GetString(StringName.Extensions);

                return SupportedExtensions.Contains(extensionName);
            }
            catch { }

            return false;
        }

        public void Flush()
        {
            GL.Flush();
            GL.Finish();
        }
    }
}
