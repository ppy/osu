using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Vitaru.Edit;
using osu.Game.Rulesets.Vitaru.Settings;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class EditorSection : WikiSection
    {
        public override string Title => "Editor";

        private Bindable<EditorConfiguration> editorConfiguration;

        private WikiOptionEnumExplanation<EditorConfiguration> editorDescription;

        [BackgroundDependencyLoader]
        private void load()
        {
            editorConfiguration = VitaruSettings.VitaruConfigManager.GetBindable<EditorConfiguration>(VitaruSetting.EditorConfiguration);
            Content.Add(editorDescription = new WikiOptionEnumExplanation<EditorConfiguration>(editorConfiguration));

            editorConfiguration.ValueChanged += scoring =>
            {
                switch (scoring)
                {
                    case EditorConfiguration.Simple:
                        editorDescription.Description.Text = "Use the provided patterns to easily and quickly get mapping";
                        break;
                    case EditorConfiguration.Complex:
                        editorDescription.Description.Text = "Fine tune EVERYTHING! Swapping to Complex to tweak a few things you mapped in Simple supported!";
                        break;
                }
            };
            editorConfiguration.TriggerChange();
        }
    }
}
