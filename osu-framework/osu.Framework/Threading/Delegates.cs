//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

namespace osu.Framework.Threading
{
    public delegate void VoidDelegate();
    public delegate void StringDelegate(string s);
    public delegate void BoolDelegate(bool b);
    public delegate bool BoolReturnDelegate(bool b);
    public delegate T Constraint<T>(T value);
}
