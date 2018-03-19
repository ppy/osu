using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;

namespace Symcol.Rulesets.Core.VectorVideos
{
    public class VectorVideo : BeatSyncedContainer
    {
        public const string FILE_NAME = "VectorVideo.symcol";

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
        }

        protected void LoadContent(string args)
        {
            string[] parameters = args.Split(',');

            ObjectType objectType = ObjectType.LogoVisualizer;
            Anchor anchor = Anchor.Centre;
            Anchor origin = Anchor.Centre;

            bool checkingType = false;

            foreach (string parameter in parameters)
            {
                string[] subParameters = parameter.Split('=');

                foreach (string subParameter in subParameters)
                {
                    if (subParameter == "Type")
                        checkingType = true;

                    if (checkingType)
                        switch (subParameter)
                        {
                            case "LogoVisualizer":
                                objectType = ObjectType.LogoVisualizer;
                                break;
                        }
                }
            }
        }

        private void loadLogoVisualizer()
        {

        }
    }

    public enum ObjectType
    {
        LogoVisualizer
    }
}
