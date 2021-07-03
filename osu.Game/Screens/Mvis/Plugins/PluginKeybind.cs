using System;
using osuTK.Input;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class PluginKeybind
    {
        public readonly Key Key;
        public readonly Action Action;
        public string Name;

        internal int Id;

        public PluginKeybind(Key key, Action action, string name = "???")
        {
            Key = key;
            Action = action;
            Name = name;
        }

        public override string ToString() => "按键 " + Key + $" 上的键位绑定(Id: {Id}, {Action})";
    }
}
