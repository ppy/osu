//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Graphics.Transformations
{
    public class Transformation : IComparable<Transformation>
    {
        public float StartFloat;
        public Vector2 StartVector;
        public Color4 StartColour;

        public float EndFloat;
        public Vector2 EndVector;
        public Color4 EndColour;

        public double Time1;
        public double Time2;
        public double LoopDelay;
        public int MaxLoopCount;
        public int CurrentLoopCount;

        public TransformationType Type;
        public EasingTypes Easing;

        public byte TagNumeric;
        public bool Loop;
        public bool IsLoopStatic;

        public Transformation() { }

        public Transformation(TransformationType type, float startFloat, float endFloat, double time1, double time2, EasingTypes easing = EasingTypes.None)
        {
            Type = type;
            StartFloat = startFloat;
            EndFloat = endFloat;
            Time1 = time1;
            Time2 = time2;
            Easing = easing;
        }

        public Transformation(Vector2 startPosition, Vector2 endPosition, double time1, double time2, EasingTypes easing = EasingTypes.None)
        {
            Type = TransformationType.Movement;
            StartVector = startPosition;
            EndVector = endPosition;
            Easing = easing;
            Time1 = time1;
            Time2 = time2;
        }

        public Transformation(TransformationType type, Vector2 startVector, Vector2 endVector, double time1, double time2, EasingTypes easing = EasingTypes.None)
        {
            Type = type;
            StartVector = startVector;
            EndVector = endVector;
            Time1 = time1;
            Time2 = time2;
            Easing = easing;
        }

        public Transformation(Color4 startColour, Color4 endColour, double time1, double time2, EasingTypes easing = EasingTypes.None)
        {
            Type = TransformationType.Colour;
            StartColour = startColour;
            EndColour = endColour;
            Time1 = time1;
            Time2 = time2;
            Easing = easing;
        }

        public double Duration => Time2 - Time1;

        #region IComparable<Transformation> Members

        public int CompareTo(Transformation other)
        {
            int compare = Time1.CompareTo(other.Time1);
            if (compare != 0) return compare;
            compare = Time2.CompareTo(other.Time2);
            if (compare != 0) return compare;
            return Type.CompareTo(other.Type);
        }

        #endregion

        public Transformation Clone()
        {
            return (Transformation)MemberwiseClone();
        }

        public Transformation CloneReverse()
        {
            Transformation t = Clone();

            t.StartFloat = EndFloat;
            t.StartColour = EndColour;
            t.StartVector = EndVector;

            t.EndFloat = StartFloat;
            t.EndColour = StartColour;
            t.EndVector = StartVector;

            switch (Easing)
            {
                case EasingTypes.Out:
                    t.Easing = EasingTypes.In;
                    break;
                case EasingTypes.In:
                    t.Easing = EasingTypes.Out;
                    break;
                default:
                    t.Easing = EasingTypes.None;
                    break;
            }

            return t;
        }

        public override string ToString()
        {
            switch (Type)
            {
                default:
                    return string.Format("{2} {0}-{1} {3}->{4}", Time1, Time2, Type, StartFloat, EndFloat);
                case TransformationType.Movement:
                    return string.Format("{2} {0}-{1} {3}->{4}", Time1, Time2, Type, StartVector, EndVector);
                case TransformationType.Colour:
                    return string.Format("{2} {0}-{1} {3}->{4}", Time1, Time2, Type, StartColour, EndColour);
            }
        }
    }
}
