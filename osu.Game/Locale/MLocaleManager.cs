using System;
using System.IO;
using JetBrains.Annotations;
using M.Resources;
using NGettext;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;

namespace osu.Game.Locale
{
    public class MLocaleManager : Component
    {
        [Resolved]
        private LocalisationManager localeManager { get; set; }

        private IBindable<string> localeString = new Bindable<string>();
        private ICatalog current;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            localeString = config.GetBindable<string>(FrameworkSetting.Locale);

            localeString.BindValueChanged(onLocaleChanged, true);
        }

        private void onLocaleChanged(ValueChangedEvent<string> v)
        {
            string locale = v.NewValue;

            Stream stream = getMoStream(locale) ?? getMoStream("zh-Hans");

            if (stream == null)
            {
                Logger.Log("无法从M.Resources取得任何mo文件");

                foreach (var s in MResources.ResourceAssembly.GetManifestResourceNames())
                {
                    Logger.Log(s);
                }

                Logger.Log($"尝试获取: M.Resources.Locales.{locale.Replace("-", "_")}.M.mo");

                throw new ArgumentNullException($"stream");
            }

            if (current != null)
                localeManager.RemoveCatalog(current);

            current = localeManager.CreateCatalog(stream);
            localeManager.AddCatalog(current);
        }

        [CanBeNull]
        private Stream getMoStream(string localeCode)
        {
            localeCode = localeCode.Replace("-", "_");
            string path = $"M.Resources.Locales.{localeCode}.M.mo";
            return MResources.ResourceAssembly.GetManifestResourceStream(path);
        }
    }
}
