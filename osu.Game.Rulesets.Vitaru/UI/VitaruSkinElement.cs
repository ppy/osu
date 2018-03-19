using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Rulesets.Vitaru.Settings;
using Symcol.Rulesets.Core.Skinning;

namespace osu.Game.Rulesets.Vitaru.UI
{
    public class VitaruSkinElement : SkinElement
    {
        public static Texture LoadSkinElement(string fileName, Storage storage)
        {
            Bindable<string> skin = VitaruSettings.VitaruConfigManager.GetBindable<string>(VitaruSetting.Skin);
            Texture texture = GetSkinElement(VitaruRuleset.VitaruTextures, skin, fileName, storage);
            return texture;
        }

        public static Texture CheckForSkinElement(string fileName, Storage storage)
        {
            Bindable<string> skin = VitaruSettings.VitaruConfigManager.GetBindable<string>(VitaruSetting.Skin);
            Texture texture = GetElement(skin, fileName, storage);
            return texture;
        }
    }
}
