using MediaLib.Models;

namespace MediaRipper.Models.Sources;

public class AudioSourceModel : StreamSourceModel
{
    public AudioSourceModel(StreamInfo stream) : base(stream)
    {
    }
}