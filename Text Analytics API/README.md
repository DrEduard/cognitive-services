# Text Analytics API

In order to run the three demos, you will need Node JS to be installed on your machine.
To try out a demo, edit the file and replace ***YOUR_KEY_HERE*** with your Text Analytics API key. Then you can simply run `node filename.js` from a Command Line opened inside this folder.

### 1. Language Detection (language.js)
The API returns the detected language and a numeric score between 0 and 1. Scores close to 1 indicate 100% certainty that the identified language is true. A total of 120 languages are supported.
 
### 2. Key Phrases (keyphrases.js)
The API returns a list of strings denoting the key talking points in the input text.

### 3. Sentiment (sentiment.js)
The API returns a numeric score between 0 and 1. Scores close to 1 indicate positive sentiment, while scores close to 0 indicate negative sentiment. A score of 0.5 indicates the lack of sentiment (e.g. a factoid statement).

**API info** https://westcentralus.dev.cognitive.microsoft.com/docs/services/TextAnalytics.V2.0/