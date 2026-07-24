using MediaLib.Models;

namespace MediaRipper.Models.Sources;

public class VideoSourceModel : StreamSourceModel
{
    public VideoSourceModel(StreamInfo stream) : base(stream)
    {
    }
}