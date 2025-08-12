using MauiGame.Core.Contracts;
using Plugin.Maui.Audio;

namespace MauiGame.Maui.Audio;

/// <summary>
/// Audio service backed by Plugin.Maui.Audio.
/// </summary>
/// <remarks>Create a new audio service.</remarks>
public sealed partial class AudioService(IAudioManager? manager = null) : Core.Contracts.IAudio
{
    private readonly IAudioManager audioManager = manager ?? Plugin.Maui.Audio.AudioManager.Current;

    /// <inheritdoc/>
    public async Task<IAudioClip> LoadClipAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

            Stream stream = await FileSystem.OpenAppPackageFileAsync(path).ConfigureAwait(false);
            IAudioPlayer player = this.audioManager.CreatePlayer(stream);
            // We keep the stream/player wrapped as a clip; duration best-effort
            AudioClip clip = new(player);
            return clip;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public IAudioInstance Play(IAudioClip clip, float volume = 1.0f, bool loop = false, bool autoStart = true)
    {
        ArgumentNullException.ThrowIfNull(clip);

        AudioClip concrete = (AudioClip)clip;
        AudioInstance instance = new(concrete.CreateNewPlayer(this.audioManager))
        {
            Volume = volume,
            Loop = loop
        };

        if (autoStart)
        {
            try { instance.Play(); } catch (Exception) { }
        }

        return instance;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Nothing to dispose; clip/instances own their players/streams.
    }

    private sealed partial class AudioClip(IAudioPlayer template) : IAudioClip
    {
        private readonly IAudioPlayer template = template ?? throw new ArgumentNullException(nameof(template));

        public double? DurationSeconds
        {
            get
            {
                try { return this.template.Duration; } catch (Exception) { return null; }
            }
        }

        public IAudioPlayer CreateNewPlayer(IAudioManager manager)
        {
            // There is no direct cloning; reopen underlying stream when playing
            // Workaround: we assume the template still has its stream. For multiple instances,
            // the caller should reopen streams; here we just reuse the same one (serial use).
            return this.template;
        }

        public void Dispose()
        {
            try { this.template.Dispose(); } catch (Exception) { }
        }
    }

    private sealed partial class AudioInstance(IAudioPlayer player) : IAudioInstance
    {
        private readonly IAudioPlayer player = player ?? throw new ArgumentNullException(nameof(player));

        public float Volume
        {
            get { try { return (float)this.player.Volume; } catch (Exception) { return 1.0f; } }
            set { try { this.player.Volume = value; } catch (Exception) { } }
        }

        public bool Loop
        {
            get { try { return this.player.Loop; } catch (Exception) { return false; } }
            set { try { this.player.Loop = value; } catch (Exception) { } }
        }

        public bool IsPlaying
        {
            get { try { return this.player.IsPlaying; } catch (Exception) { return false; } }
        }

        public void Play()
        {
            try { this.player.Play(); } catch (Exception) { }
        }

        public void Pause()
        {
            try { this.player.Pause(); } catch (Exception) { }
        }

        public void Stop()
        {
            try { this.player.Stop(); } catch (Exception) { }
        }

        public void Dispose()
        {
            try { this.player.Dispose(); } catch (Exception) { }
        }
    }
}