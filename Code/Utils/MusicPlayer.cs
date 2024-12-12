namespace ChromaCore.Code.Utils
{
    public class MusicPlayer
    {
        private MusicTrack currentTrack;
        private MusicTrack oldTrack;
        private DateTime startTime;
        private DateTime oldTrackStartTime;
        private int fadeTimer;
        private int fadeDuration;
        private bool intro = false;

        public MusicTrack CurrentTrack => currentTrack;

        public MusicPlayer() { CheckLoop(); }

        async void CheckLoop()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    if (currentTrack != null)
                    {
                        lock (currentTrack)
                        {
                            if (currentTrack is CompoundMusicTrack c && intro)
                            {
                                if ((DateTime.Now - startTime).TotalSeconds >= c.introDuration)
                                {
                                    c.PlayLoopingPart();
                                    c.SetVolume(Game.MusicVolume);
                                    intro = false;
                                    startTime = DateTime.Now;
                                }
                            }
                            else if ((DateTime.Now - startTime).TotalSeconds >= currentTrack.duration)
                            {
                                currentTrack.Loop();
                                startTime = DateTime.Now;
                            }
                        }
                    }
                });
                Thread.Sleep(1);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (fadeTimer > 0)
            {
                fadeTimer--;
                if (currentTrack != null)
                {
                    currentTrack.SetVolume(Game.MusicVolume * (1 - (float)fadeTimer / fadeDuration));
                    currentTrack.Update();
                }

                if (oldTrack != null)
                {
                    if ((DateTime.Now - oldTrackStartTime).TotalSeconds >= oldTrack.duration || oldTrack.State == SoundState.Stopped)
                    {
                        oldTrack.Loop();
                        oldTrackStartTime = DateTime.Now;
                    }
                    oldTrack.SetVolume(Math.Min(Game.musicVolumeMultiplier * fadeTimer / fadeDuration, oldTrack.Volume));
                    oldTrack.Update();
                }
            }
            else
            {
                if (currentTrack != null)
                {
                    currentTrack.SetVolume(Game.MusicVolume);
                    currentTrack.Update();
                }

                //Dispose of the old track
                if (oldTrack != null) oldTrack.Stop();
            }
        }

        public void FadeTo(MusicTrack newMusic, int fadeDuration = 240)
        {
            //Dispose of any lingering track
            if (currentTrack == null || currentTrack.soundFile != newMusic.soundFile)
            {
                if (oldTrack != null && fadeTimer > 0)
                {
                    oldTrack.Stop();
                    if (oldTrack.soundFile == newMusic.soundFile && fadeTimer > 0)
                    {
                        newMusic = oldTrack;
                    }
                }
                //Store the current track as the old track
                oldTrack = currentTrack;
                oldTrackStartTime = startTime;

                //Store the new track as the current track
                currentTrack = newMusic;
                //if (newMusic is CompoundMusicTrack c) trackInstance = c.introSoundFile.CreateInstance();
                //else trackInstance = currentTrack.soundFile.CreateInstance();
                //trackInstance.Play();
                startTime = DateTime.Now;
                currentTrack.Play();
                currentTrack.SetVolume(0);

                //Handle fade tiemrs
                this.fadeDuration = fadeDuration;
                fadeTimer = fadeDuration;

                if (newMusic is CompoundMusicTrack) intro = true;
            }
        }

        public void FadeOut(int fadeDuration = 240)
        {
            //Dispose of any lingering track
            if (oldTrack != null) oldTrack.Stop();
            //Store the current track as the old track
            oldTrack = currentTrack;
            oldTrackStartTime = startTime;

            currentTrack = null;

            this.fadeDuration = fadeDuration;
            fadeTimer = fadeDuration;
        }
    }

    public class MusicTrack
    {
        public readonly SoundEffect soundFile;
        public SoundEffectInstance soundInstance;
        public double duration;

        public MusicTrack(string soundFileURL, double trackDuration)
        {
            soundFile = Game.LoadAsset<SoundEffect>(soundFileURL);
            duration = trackDuration;
        }

        public virtual void Play()
        {
            soundInstance = soundFile.CreateInstance();
            soundInstance?.Play();
        }

        public virtual void Update() { }

        public virtual void SetVolume(float volume)
        {
            if (soundInstance != null) soundInstance.Volume = volume;
        }

        public virtual float Volume => soundInstance == null ? 0 : soundInstance.Volume;
        public virtual SoundState State => soundInstance == null ? SoundState.Stopped : soundInstance.State;

        public virtual void Loop()
        {
            soundInstance?.Stop();
            soundInstance?.Play();
        }

        public virtual void Stop()
        {
            soundInstance?.Stop();
            soundInstance?.Dispose();
            soundInstance = null;
        }

        public static int CalculateDuration(int tempo, int measures, int beatsPerMeasure = 4)
        {
            int beats = measures * beatsPerMeasure;
            return (int)(beats * (float)3600 / tempo);
        }
    }

    public class LayeredMusicTrack : MusicTrack
    {
        private readonly SoundEffect[] soundFiles;
        private SoundEffectInstance[] soundInstances;
        private bool[] active;
        private float[] volumes;

        public LayeredMusicTrack(double trackDuration, params string[] files) : base(files[0], trackDuration)
        {
            soundFiles = new SoundEffect[files.Length];
            soundInstances = new SoundEffectInstance[files.Length];
            active = new bool[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                soundFiles[i] = Game.LoadAsset<SoundEffect>(files[i]);
                active[i] = i == 0;
            }
        }

        public override void Play()
        {
            for (int i = 0; i < soundFiles.Length; i++)
            {
                soundInstances[i] = soundFiles[i].CreateInstance();
                soundInstances[i].Play();
                soundInstances[i].Volume = active[i] ? 1 : 0;
            }
        }

        public override void SetVolume(float volume)
        {
            if (soundInstances[0] != null) soundInstances[0].Volume = volume;
        }

        public override void Update()
        {
            for (int i = 1; i < soundFiles.Length; i++)
            {
                if (soundInstances[i].Volume > soundInstances[0].Volume) soundInstances[i].Volume = soundInstances[0].Volume;
                if (soundInstances[i] != null && active[i]) soundInstances[i].Volume = HelperFunctions.FloatApproach(soundInstances[i].Volume, soundInstances[0].Volume, 0.025f * soundInstances[0].Volume);
                else if (soundInstances[i] != null) soundInstances[i].Volume = HelperFunctions.FloatApproach(soundInstances[i].Volume, 0, 0.01f * soundInstances[0].Volume);
            }
        }

        public override float Volume => soundInstances[0] == null ? 0 : soundInstances[0].Volume;
        public override SoundState State => soundInstances[0] == null ? SoundState.Stopped : soundInstances[0].State;

        public override void Loop()
        {
            for (int i = 0; i < soundFiles.Length; i++)
            {
                if (soundInstances != null)
                {
                    soundInstances[i]?.Stop();
                    soundInstances[i]?.Play();
                }
            }
        }

        public override void Stop()
        {
            for (int i = 0; i < soundFiles.Length; i++)
            {
                soundInstances[i]?.Stop();
                soundInstances[i]?.Dispose();
                soundInstances[i] = null;
            }
        }

        public void SetActive(int layer, bool active)
        {
            if (layer >= 1 && layer < soundFiles.Length)
            {
                this.active[layer] = active;
                if (soundInstances[layer] != null) soundInstances[layer].Volume = MathHelper.Lerp(soundInstances[layer].Volume, this.active[layer] ? soundInstances[0].Volume : 0, 0.05f);
            }
        }
    }

    public class CompoundMusicTrack : MusicTrack
    {
        public readonly SoundEffect introSoundFile;
        public double introDuration;

        public CompoundMusicTrack(string introURL, double introDuration, string loopURL, double loopDuration) : base(loopURL, loopDuration)
        {
            introSoundFile = Game.LoadAsset<SoundEffect>(introURL);
            this.introDuration = introDuration;
        }

        public override void Play()
        {
            soundInstance = introSoundFile.CreateInstance();
            soundInstance.Play();
        }
        public void PlayLoopingPart()
        {
            soundInstance.Stop();
            soundInstance.Dispose();
            soundInstance = soundFile.CreateInstance();
            soundInstance.Play();
        }
    }
}
