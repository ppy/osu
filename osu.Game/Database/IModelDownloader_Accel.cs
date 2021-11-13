namespace osu.Game.Database
{
    public partial interface IModelDownloader<TModel>
    {
        bool AccelDownload(TModel model, bool minimiseDownloadSize);
    }
}
