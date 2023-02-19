using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.LuaFunctions
{
    public partial class BaseFunctions : CompositeDrawable
    {
        [Resolved]
        private LuaPlugin plugin { get; set; }

        [Resolved]
        private IImplementLLin llin { get; set; } = null!;

        public BaseFunctions(LuaPlugin pluginInstance)
        {
            this.plugin = pluginInstance;
        }

        public void Print(object str)
        {
            plugin.Log($"{str}");
        }

        public void PostNotification(string content)
        {
            llin.PostNotification(plugin, FontAwesome.Regular.QuestionCircle, content);
        }

        public void ClearConsole()
        {
            plugin.ClearConsole();
        }

        public void GenDoc()
        {
            throw new NotImplementedException();
        }
    }
}
