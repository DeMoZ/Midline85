public class MediaService
{
    private readonly Ctx _ctx;

    public struct Ctx
    {
        public WwiseAudio AudioManager;
        public ImageManager ImageManager;
        public VideoManager VideoManager;
        public FilmProjector FilmProjector;
    }

    public readonly WwiseAudio AudioManager;
    public readonly ImageManager ImageManager;
    public readonly VideoManager VideoManager;
    public readonly FilmProjector FilmProjector;

    public MediaService(Ctx ctx)
    {
        AudioManager = ctx.AudioManager;
        ImageManager = ctx.ImageManager;
        VideoManager = ctx.VideoManager;
        FilmProjector = ctx.FilmProjector;
    }
}