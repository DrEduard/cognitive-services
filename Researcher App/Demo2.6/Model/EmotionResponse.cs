using Newtonsoft.Json;

public partial class Face
{
    [JsonProperty("faceId")]
    public string FaceId { get; set; }

    [JsonProperty("faceRectangle")]
    public FaceRectangle FaceRectangle { get; set; }

    [JsonProperty("faceAttributes")]
    public FaceAttributes FaceAttributes { get; set; }
}

public partial class FaceAttributes
{
    [JsonProperty("emotion")]
    public Emotion Emotion { get; set; }
}

public partial class Emotion
{
    [JsonProperty("anger")]
    public long Anger { get; set; }

    [JsonProperty("contempt")]
    public double Contempt { get; set; }

    [JsonProperty("disgust")]
    public long Disgust { get; set; }

    [JsonProperty("fear")]
    public long Fear { get; set; }

    [JsonProperty("happiness")]
    public double Happiness { get; set; }

    [JsonProperty("neutral")]
    public double Neutral { get; set; }

    [JsonProperty("sadness")]
    public double Sadness { get; set; }

    [JsonProperty("surprise")]
    public long Surprise { get; set; }
}

public partial class FaceRectangle
{
    [JsonProperty("top")]
    public long Top { get; set; }

    [JsonProperty("left")]
    public long Left { get; set; }

    [JsonProperty("width")]
    public long Width { get; set; }

    [JsonProperty("height")]
    public long Height { get; set; }
}