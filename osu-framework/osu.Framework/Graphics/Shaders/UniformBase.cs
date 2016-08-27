//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Graphics.Shaders
{
    internal class UniformBase
    {
        public string Name { get; private set; }

        private object value;
        public object Value
        {
            get { return value; }
            set
            {
                if (value == this.value)
                    return;

                this.value = value;
                hasChanged = true;

                if (owner.IsBound)
                    Update();
            }
        }

        private int location;
        private ActiveUniformType type;

        private bool hasChanged = true;

        private Shader owner;

        public UniformBase(Shader owner, string name, int uniformLocation, ActiveUniformType type)
        {
            this.owner = owner;
            this.Name = name;
            this.location = uniformLocation;
            this.type = type;
        }

        public void Update()
        {
            if (!hasChanged)
                return;
            hasChanged = false;

            if (Value == null)
                return;

            switch (type)
            {
                case ActiveUniformType.Bool:
                    GL.Uniform1(location, (bool)Value ? 1 : 0);
                    break;
                case ActiveUniformType.Int:
                    GL.Uniform1(location, (int)Value);
                    break;
                case ActiveUniformType.Float:
                    GL.Uniform1(location, (float)Value);
                    break;
                case ActiveUniformType.BoolVec2:
                case ActiveUniformType.IntVec2:
                case ActiveUniformType.FloatVec2:
                    GL.Uniform2(location, (Vector2)Value);
                    break;
                case ActiveUniformType.FloatMat2:
                    {
                        Matrix2 mat = (Matrix2)Value;
                        GL.UniformMatrix2(location, false, ref mat);
                    }
                    break;
                case ActiveUniformType.BoolVec3:
                case ActiveUniformType.IntVec3:
                case ActiveUniformType.FloatVec3:
                    GL.Uniform3(location, (Vector3)Value);
                    break;
                case ActiveUniformType.FloatMat3:
                    {
                        Matrix3 mat = (Matrix3)Value;
                        GL.UniformMatrix3(location, false, ref mat);
                    }
                    break;
                case ActiveUniformType.BoolVec4:
                case ActiveUniformType.IntVec4:
                case ActiveUniformType.FloatVec4:
                    GL.Uniform4(location, (Vector4)Value);
                    break;
                case ActiveUniformType.FloatMat4:
                    {
                        Matrix4 mat = (Matrix4)Value;
                        GL.UniformMatrix4(location, false, ref mat);
                    }
                    break;
                case ActiveUniformType.Sampler2D:
                    GL.Uniform1(location, (int)Value);
                    break;
            }
        }
    }
}
