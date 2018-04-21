using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Demo2._1
{
    using System.Media;
    using System.Net.Http;
    using System.Threading;
    using System.Windows.Threading;

    using CognitiveServicesTTS;

    using Microsoft.CognitiveServices.SpeechRecognition;
    using System.Web;

    using Newtonsoft.Json;
    using System.Net.Http.Headers;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MicrophoneRecognitionClient micClient;

        private bool showEmotion = false;

        public MainWindow()
        {
            InitializeComponent();
            this.micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.ShortPhrase,
                "en-US",
                "COPY-KEY-HERE");
            this.micClient.OnMicrophoneStatus += MicClient_OnMicrophoneStatus;
            this.micClient.OnResponseReceived += MicClient_OnResponseReceived;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.searchImage.Source = null;
            this.MySpeechSentiment.Text = string.Empty;
            this.MySpeechResponse.Text = string.Empty;
            this.MySpeechSentimentConfidence.Text = string.Empty;
            this.MySpeechResponseConfidence.Text = string.Empty;
            this.MySpeechIntent.Text = string.Empty;
            this.MySpeechIntentScore.Text = string.Empty;
            this.micClient.StartMicAndRecognition();
        }

        private void MicClient_OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        if (e.Recording)
                        {
                            this.status.Text = "Listening";
                            this.RecordingBar.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.status.Text = "Not Listening";
                            this.RecordingBar.Visibility = Visibility.Collapsed;
                        }
                    }));
        }

        private async void MicClient_OnResponseReceived(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length > 0)
            {
                await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal, new Action(() =>
                    {
                        this.MySpeechResponse.Text = $"'{e.PhraseResponse.Results[0].DisplayText}',";
                        this.MySpeechResponseConfidence.Text = $"confidence: { e.PhraseResponse.Results[0].Confidence}";
                    }));

                var intent = await this.GetLuisIntent(e.PhraseResponse.Results[0].DisplayText);
                await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal, new Action(() =>
                    {
                        this.MySpeechIntent.Text = $"Intent: '{intent.intents[0].intent}',";
                        this.MySpeechIntentScore.Text = $"score: {Convert.ToInt16(intent.intents[0].score * 100)}%";
                    }));

                if (intent.intents[0].intent.ToLower() == "thingpictures")
                {
                    this.showEmotion = false;
                    this.SearchImage(intent.query);
                }
                else if (intent.intents[0].intent.ToLower() == "peoplepictures")
                {
                    this.showEmotion = true;
                    this.SearchImage(intent.query);
                }
                else
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Action(() => { this.MySpeechSentiment.Text = $"I'm not sure what your intent is and will not search"; }));
                }
            }
        }

        private async Task<LuisIntent> GetLuisIntent(string utterance)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(utterance);

            // Request parameters
            var uri = "COPY-LUIS-ENDPOINT-URL-HERE" + queryString;

            var response = await client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            LuisIntent intent = JsonConvert.DeserializeObject<LuisIntent>(json);

            return intent;
        }


        private async Task Speak(string speech)
        {
            string accessToken;

            Authentication auth = new Authentication("COPY-KEY-HERE");
            accessToken = auth.GetAccessToken();
            string uri = "https://speech.platform.bing.com/synthesize";
            var speaker = new Synthesize();

            speaker.OnAudioAvailable += Speaker_OnAudioAvailable;
            var options = new Synthesize.InputOptions
            {
                RequestUri = new Uri(uri),
                Text = speech,
                VoiceType = Gender.Female,
                Locale = "en-US",
                VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken
            };
            await speaker.Speak(CancellationToken.None, options);
        }

        private void Speaker_OnAudioAvailable(object sender, GenericEventArgs<System.IO.Stream> e)
        {
            SoundPlayer player = new SoundPlayer(e.EventData);
            player.PlaySync();
            e.EventData.Dispose();
        }

        private async void SearchImage(string phraseToSearch)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "COPY-KEY-HERE");

            // Request parameters
            queryString["q"] = phraseToSearch;
            queryString["count"] = "1";
            queryString["offset"] = "0";
            queryString["mkt"] = "en-us";
            queryString["safeSearch"] = "Moderate";
            var uri = "https://api.cognitive.microsoft.com/bing/v5.0/images/search?" + queryString;

            var response = await client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            BingImageSearchResponse bingImageSearchResponse = JsonConvert.DeserializeObject<BingImageSearchResponse>(json);
            var uriSource = new Uri(bingImageSearchResponse.value[0].contentUrl, UriKind.Absolute);

            await Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal, new Action(() =>
                {
                    this.searchImage.Source = new BitmapImage(uriSource);

                }));

            if (this.showEmotion)
                await GetEmotion(bingImageSearchResponse.value[0].contentUrl);
        }

        private async Task GetEmotion(string imageUri)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "COPY-KEY-HERE");

            // Request parameters
            var uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?" + queryString;
            EmotionRequest request = new EmotionRequest();
            request.url = imageUri;
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                var json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<EmotionResponse> emotionResponse =
                        JsonConvert.DeserializeObject<List<EmotionResponse>>(json);

                    if (emotionResponse != null && emotionResponse.Count > 0)
                    {
                        var scores = emotionResponse[0].scores;
                        Dictionary<string, double> dScores = new Dictionary<string, double>();
                        dScores.Add("anger", scores.anger);
                        dScores.Add("contempt", scores.contempt);
                        dScores.Add("disgust", scores.disgust);
                        dScores.Add("fear", scores.fear);
                        dScores.Add("happiness", scores.happiness);
                        dScores.Add("neutral", scores.neutral);
                        dScores.Add("sadness", scores.sadness);
                        dScores.Add("surprise", scores.surprise);
                        var highestScore = dScores.Values.OrderByDescending(score => score).First();
                        //probably a more elegant way to do this.
                        var highestEmotion = dScores.Keys.First(key => dScores[key] == highestScore);

                        await Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            new Action(
                                () =>
                                {
                                    this.MySpeechSentiment.Text = $"Emotion: {highestEmotion},";
                                    this.MySpeechSentimentConfidence.Text =
                                        $"confidence: {Convert.ToInt16(highestScore * 100)}%";
                                }));
                        await
                            this.Speak(
                                $"I'm  {Convert.ToInt16(highestScore * 100)}% sure that this person's emotion is {highestEmotion}");
                    }
                    else
                    {
                        await
                            this.Speak(
                                $"I'm not able to get the emotion, sorry.");
                    }
                }
                else
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Action(() => { this.MySpeechSentiment.Text = "Could not get emotion from this image"; }));
                    await
                        this.Speak(
                            $"Could not get emotion from this image.");
                }
            }

        }


    }
}
