using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Mods.Rulesets.Core.Skinning
{
    public abstract class SkinElement
    {
        private static string loadedSkin;

        private static ResourceStore<byte[]> skinResources;
        private static TextureStore skinTextures;

        /// <summary>
        /// Will attempt to get a skin element fron the skin, if no element is found return the default element
        /// </summary>
        /// <param name="stockTextures"></param>
        /// <param name="skin"></param>
        /// <param name="fileName"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static Texture GetSkinElement(TextureStore stockTextures, Bindable<string> skin, string fileName, Storage storage)
        {
            Texture texture = null;

            string fileNameHd = fileName + "@2x";

            Storage skinStorage = storage.GetStorageForDirectory("Skins\\" + skin);

            if (skin.Value == "default")
            {
                texture = stockTextures.Get(fileName + ".png");

                if (texture == null)
                    texture = stockTextures.Get(fileNameHd + ".png");

                return texture;
            }

            if (loadedSkin != skin.ToString())
            {
                loadedSkin = skin.ToString();
                skinResources = new ResourceStore<byte[]>(new StorageBackedResourceStore(skinStorage));
                skinTextures = new TextureStore(new TextureLoaderStore(skinResources));
            }

            if (skinStorage.Exists(fileNameHd + ".png"))
                texture = skinTextures.Get(fileNameHd + ".png");
            else if (skinStorage.Exists(fileName + ".png"))
            {
                texture = skinTextures.Get(fileName + ".png");
                texture.ScaleAdjust = 1f;
            }
            else
                texture = stockTextures.Get(fileNameHd + ".png");

            return texture;
        }

        /// <summary>
        /// Will attempt to get a skin element from the skin, if no element is found return null
        /// </summary>
        /// <param name="skin"></param>
        /// <param name="fileName"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static Texture GetElement(Bindable<string> skin, string fileName, Storage storage)
        {
            Texture texture = null;

            string fileNameHd = fileName + "@2x";

            Storage skinStorage = storage.GetStorageForDirectory("Skins\\" + skin);

            if (loadedSkin != skin.ToString())
            {
                loadedSkin = skin.ToString();
                skinResources = new ResourceStore<byte[]>(new StorageBackedResourceStore(skinStorage));
                skinTextures = new TextureStore(new TextureLoaderStore(skinResources));
            }

            if (skinStorage.Exists(fileNameHd + ".png"))
                texture = skinTextures.Get(fileNameHd + ".png");
            else if (skinStorage.Exists(fileName + ".png"))
            {
                texture = skinTextures.Get(fileName + ".png");
                texture.ScaleAdjust = 1f;
            }
            else
                texture = null;

            return texture;
        }
    }
}
