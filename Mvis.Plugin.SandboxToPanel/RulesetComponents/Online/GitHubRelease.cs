using Newtonsoft.Json;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Online
{
    public partial class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
