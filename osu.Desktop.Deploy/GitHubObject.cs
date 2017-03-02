using Newtonsoft.Json;

namespace osu.Desktop.Deploy
{
    internal class GitHubObject
    {
        [JsonProperty(@"id")]
        public int Id;

        [JsonProperty(@"name")]
        public string Name;
    }
}