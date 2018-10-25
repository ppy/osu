using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Screens;
using OpenTK;
using OpenTK.Graphics;
using Symcol.osu.Core.Containers.Shawdooow;
using Symcol.osu.Core.SymcolMods;
using Symcol.osu.Core.Wiki;
using Symcol.osu.Mods.Caster.Wiki;

namespace Symcol.osu.Mods.Caster
{
    public class CasterModSet : SymcolModSet
    {
        public override SymcolButton GetMenuButton() => new SymcolButton
        {
            ButtonName = "Caster",
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            ButtonColorTop = Color4.BlueViolet,
            ButtonColorBottom = Color4.Cyan,
            ButtonSize = 100,
            ButtonPosition = new Vector2(180, -160),
        };

        public override OsuScreen GetScreen() => new CasterScreen();

        public override WikiSet GetWikiSet() => new CasterWikiSet();

        public static ResourceStore<byte[]> CasterResources;
        public static TextureStore CasterTextures;

        public CasterModSet()
        {
            if (CasterResources == null)
            {
                CasterResources = new ResourceStore<byte[]>();
                CasterResources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore("Symcol.osu.Mods.Caster.dll"), "Assets"));
                CasterResources.AddStore(new DllResourceStore("Symcol.osu.Mods.Caster.dll"));
                CasterTextures = new TextureStore(new TextureLoaderStore(new NamespacedResourceStore<byte[]>(CasterResources, @"Textures")));
                CasterTextures.AddStore(new TextureLoaderStore(new OnlineStore()));
            }
        }
    }
}
