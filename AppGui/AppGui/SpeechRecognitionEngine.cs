#region assembly Microsoft.Speech, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// C:\Program Files\Microsoft SDKs\Speech\v11.0\Assembly\Microsoft.Speech.dll
#endregion

using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Text;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Microsoft.Speech.Recognition
{
    public class SpeechRecognitionEngine : IDisposable
    {
        public SpeechRecognitionEngine();
        public SpeechRecognitionEngine(CultureInfo culture);
        public SpeechRecognitionEngine(string recognizerId);
        public SpeechRecognitionEngine(RecognizerInfo recognizerInfo);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan BabbleTimeout { get; set; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan EndSilenceTimeout { get; set; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan EndSilenceTimeoutAmbiguous { get; set; }
        public ReadOnlyCollection<Grammar> Grammars { get; }
        public RecognizerInfo RecognizerInfo { get; }
        public AudioState AudioState { get; }
        public int AudioLevel { get; }
        public TimeSpan RecognizerAudioPosition { get; }
        public TimeSpan AudioPosition { get; }
        public SpeechAudioFormatInfo AudioFormat { get; }
        public NormalizerCollection NormalizerCollection { get; }
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public TimeSpan InitialSilenceTimeout { get; set; }
        public int MaxAlternates { get; set; }

        public event EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted;
        public event EventHandler<EmulateRecognizeCompletedEventArgs> EmulateRecognizeCompleted;
        public event EventHandler<LoadGrammarCompletedEventArgs> LoadGrammarCompleted;
        public event EventHandler<SpeechDetectedEventArgs> SpeechDetected;
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;
        public event EventHandler<SpeechRecognitionRejectedEventArgs> SpeechRecognitionRejected;
        public event EventHandler<RecognizerUpdateReachedEventArgs> RecognizerUpdateReached;
        public event EventHandler<SpeechHypothesizedEventArgs> SpeechHypothesized;
        public event EventHandler<AudioSignalProblemOccurredEventArgs> AudioSignalProblemOccurred;
        public event EventHandler<AudioLevelUpdatedEventArgs> AudioLevelUpdated;
        public event EventHandler<AudioStateChangedEventArgs> AudioStateChanged;

        public static ReadOnlyCollection<RecognizerInfo> InstalledRecognizers();
        public void Dispose();
        public RecognitionResult EmulateRecognize(string inputText);
        public RecognitionResult EmulateRecognize(string inputText, EmulateOptions emulateOptions);
        public RecognitionResult EmulateRecognize(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions, EmulateOptions emulateOptions);
        public RecognitionResult EmulateRecognize(string inputText, CompareOptions compareOptions);
        public RecognitionResult EmulateRecognize(string inputText, CompareOptions compareOptions, EmulateOptions emulateOptions);
        public RecognitionResult EmulateRecognize(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions);
        public void EmulateRecognizeAsync(string inputText);
        public void EmulateRecognizeAsync(string inputText, CompareOptions compareOptions);
        public void EmulateRecognizeAsync(string inputText, CompareOptions compareOptions, EmulateOptions emulateOptions);
        public void EmulateRecognizeAsync(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions);
        public void EmulateRecognizeAsync(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions, EmulateOptions emulateOptions);
        public void EmulateRecognizeAsync(string inputText, EmulateOptions emulateOptions);
        public void LoadGrammar(Grammar grammar);
        public void LoadGrammarAsync(Grammar grammar);
        public object QueryRecognizerSetting(string settingName);
        public RecognitionResult Recognize();
        public RecognitionResult Recognize(TimeSpan initialSilenceTimeout);
        public void RecognizeAsync(RecognizeMode mode);
        public void RecognizeAsync();
        public void RecognizeAsyncCancel();
        public void RecognizeAsyncStop();
        public void RequestRecognizerUpdate();
        public void RequestRecognizerUpdate(object userToken, TimeSpan audioPositionAheadToRaiseUpdate);
        public void RequestRecognizerUpdate(object userToken);
        public void SetInputToAudioStream(Stream audioSource, SpeechAudioFormatInfo audioFormat);
        public void SetInputToDefaultAudioDevice();
        public void SetInputToNull();
        public void SetInputToWaveFile(string path);
        public void SetInputToWaveStream(Stream audioSource);
        public RecognitionResult SimulateRecognize(string inputText);
        public void UnloadAllGrammars();
        public void UnloadGrammar(Grammar grammar);
        public void UpdateRecognizerSetting(string settingName, string updatedValue);
        public void UpdateRecognizerSetting(string settingName, int updatedValue);
        protected virtual void Dispose(bool disposing);
    }
}