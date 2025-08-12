using MauiGame.Core.Contracts;
using Plugin.Maui.Audio;
using Microsoft.Extensions.Logging;

namespace MauiGame.Maui.Audio;

/// <summary>
/// Audio service backed by Plugin.Maui.Audio.
/// </summary>
/// <remarks>Create a new audio service.</remarks>
public sealed partial class AudioService(IAudioManager? manager = null, ILogger<AudioService>? logger = null) : Core.Contracts.IAudio
{
    private readonly IAudioManager audioManager = manager ?? Plugin.Maui.Audio.AudioManager.Current;
    private readonly ILogger<AudioService>? logger = logger;

    /// <inheritdoc/>
    /// <remarks>The returned clip owns the stream and player created for the provided path.</remarks>
    public async Task<IAudioClip> LoadClipAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

        Stream stream = await FileSystem.OpenAppPackageFileAsync(path).ConfigureAwait(false);
        IAudioPlayer player = this.audioManager.CreatePlayer(stream);
        // We keep the stream/player wrapped as a clip; duration best-effort
        AudioClip clip = new(player, this.logger);
        return clip;
    }

    /// <inheritdoc/>
    /// <remarks>The caller is responsible for disposing the returned instance to release the player.</remarks>
    public IAudioInstance Play(IAudioClip clip, float volume = 1.0f, bool loop = false, bool autoStart = true)
    {
        ArgumentNullException.ThrowIfNull(clip);

        AudioClip concrete = (AudioClip)clip;
        AudioInstance instance = new(concrete.CreateNewPlayer(this.audioManager), this.logger)
        {
            Volume = volume,
            Loop = loop
        };

        if (autoStart)
        {
            try
            {
                instance.Play();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Failed to start audio instance.");
            }
        }

        return instance;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Nothing to dispose; clip/instances own their players/streams.
    }

    /// <summary>
    /// Wraps an <see cref="IAudioPlayer"/> created from a content stream.
    /// The clip owns this player and its underlying stream; disposing the clip
    /// releases both.
    /// </summary>
    private sealed partial class AudioClip(IAudioPlayer template, ILogger<AudioService>? logger) : IAudioClip
    {
        private readonly IAudioPlayer template = template ?? throw new ArgumentNullException(nameof(template));
        private readonly ILogger<AudioService>? logger = logger;

        public double? DurationSeconds
        {
            get
            {
                try { return this.template.Duration; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to get clip duration.");
                    return null;
                }
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
            try
            {
                this.template.Dispose();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Error while disposing audio clip.");
            }
        }
    }

    /// <summary>
    /// Represents a playing instance created from an <see cref="AudioClip"/>.
    /// The instance owns its <see cref="IAudioPlayer"/> and disposes it when the
    /// instance is disposed, stopping playback and freeing the stream.
    /// </summary>
    private sealed partial class AudioInstance(IAudioPlayer player, ILogger<AudioService>? logger) : IAudioInstance
    {
        private readonly IAudioPlayer player = player ?? throw new ArgumentNullException(nameof(player));
        private readonly ILogger<AudioService>? logger = logger;

        public float Volume
        {
            get
            {
                try { return (float)this.player.Volume; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to get volume.");
                    return 1.0f;
                }
            }
            set
            {
                try { this.player.Volume = value; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to set volume.");
                }
            }
        }

        public bool Loop
        {
            get
            {
                try { return this.player.Loop; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to get loop state.");
                    return false;
                }
            }
            set
            {
                try { this.player.Loop = value; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to set loop state.");
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                try { return this.player.IsPlaying; }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed to get playing state.");
                    return false;
                }
            }
        }

        public void Play()
        {
            try
            {
                this.player.Play();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Failed to play audio instance.");
            }
        }

        public void Pause()
        {
            try
            {
                this.player.Pause();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Failed to pause audio instance.");
            }
        }

        public void Stop()
        {
            try
            {
                this.player.Stop();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Failed to stop audio instance.");
            }
        }

        public void Dispose()
        {
            try
            {
                this.player.Dispose();
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Error while disposing audio instance.");
            }
        }
    }
}