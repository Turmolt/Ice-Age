using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Query;
using NAudio.Wave;

namespace Ice_Age
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IAudioService audioService =  new NAudioService();// SoundFingerprintingAudioService();
        private NAudioSoundCaptureService captureService = new NAudioSoundCaptureService();
        private NAudioWaveFileUtility fileUtility = new NAudioWaveFileUtility();

        private WaveIn sourceStream = null;
        private WaveFileWriter waveFile = null;

        private MainWindow wnd;
        private string iceage_path = @"S:\Coding\C#\Hackaton\iceage_full.mp3";
        private string mic_path = @"S:\Coding\C#\Hackaton\sample.wav";
        private async void Start(object sender, StartupEventArgs e)
        {
            //setup window
            wnd = new MainWindow();
            wnd.ShowResult(false);
            wnd.Show();

            //create fingerprint for ice age
            StoreFingerprint(iceage_path);

            await ListenToMicrophone();
        }

        void CreateMicStream()
        {
            sourceStream = new WaveIn();
            sourceStream.DeviceNumber = 3;
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, 1);

            sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceStream_DataAvailable);
            sourceStream.RecordingStopped += new EventHandler<StoppedEventArgs>(sourceStream_RecordingStopped);

            waveFile = new WaveFileWriter(mic_path, sourceStream.WaveFormat);
        }

        async Task ListenToMicrophone()
        {
            QueryResult result = null;
            while (true)
            {
                //setup mic stream and file writer
                CreateMicStream();

                await CheckMicrophoneFeed();

                result = await QueryFingerprint(mic_path);

                ActOnResult(result);

                await Task.Delay(1);
            }
        }

        async Task CheckMicrophoneFeed()
        {

            sourceStream.StartRecording();
            await Task.Delay(10000);
            sourceStream.StopRecording();
        }

        void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        void sourceStream_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (sourceStream != null)
            {
                sourceStream.Dispose();
                sourceStream = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }

        public void ActOnResult(QueryResult result)
        {
            if (result.BestMatch == null)
            {
                Console.WriteLine("You are not watching Ice Age.");
                wnd.ShowResult(false);
            }
            else
            {
                Console.WriteLine("You are watching Ice Age!");
                wnd.ShowResult(true);
            }
        }

        public async Task StoreFingerprint(string path)
        {
            var track = new TrackInfo("0", "Ice Age", "Chris Wedge");

            var hashedFingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(path)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(track, hashedFingerprints);
        }

        public async Task<QueryResult> QueryFingerprint(string path)
        {
            int secondsToAnalyze = 7;
            int startAtSecond = 0;

            Console.WriteLine("Querying");

            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(path, secondsToAnalyze, startAtSecond)
                .UsingServices(modelService, audioService)
                .Query();

            return queryResult;
        }

        private void OnApplicationQuit(object sender, ExitEventArgs e)
        {
            modelService.DeleteTrack("0");
            
        }

    }
}
