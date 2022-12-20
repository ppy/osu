using osu.Framework.IO.Network;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Online
{
    public partial class GetLatestReleaseRequest : JsonWebRequest<GitHubRelease>
    {
        public GetLatestReleaseRequest()
            : base("https://api.github.com/repos/evast9919/lazer-sandbox/releases/latest")
        {
        }
    }
}
