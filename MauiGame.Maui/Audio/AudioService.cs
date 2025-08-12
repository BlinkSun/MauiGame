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
    /// <remarks>The returned clip stores the audio data so a fresh player can be created per playback.</remarks>
    public async Task<IAudioClip> LoadClipAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(path).ConfigureAwait(false);
        using MemoryStream buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        byte[] data = buffer.ToArray();

        double? duration = null;
        MemoryStream durationStream = new MemoryStream(data, writable: false);
        IAudioPlayer durationPlayer = this.audioManager.CreatePlayer(durationStream);
        try
        {
            duration = durationPlayer.Duration;
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Failed to get clip duration.");
        }
        finally
        {
            durationPlayer.Dispose();
            durationStream.Dispose();
        }

        AudioClip clip = new AudioClip(data, duration, this.logger);
        return clip;
    }

    /// <inheritdoc/>
    /// <remarks>The caller is responsible for disposing the returned instance to release the player.</remarks>
    public IAudioInstance Play(IAudioClip clip, float volume = 1.0f, bool loop = false, bool autoStart = true)
    {
        ArgumentNullException.ThrowIfNull(clip);

        AudioClip concrete = (AudioClip)clip;
        IAudioPlayer player;
        try
        {
            player = concrete.CreateNewPlayer(this.audioManager);
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Failed to create audio player.");
            throw;
        }

        AudioInstance instance = new AudioInstance(player, this.logger)
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
                instance.Dispose();
                throw;
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
    /// Represents audio data that can create new <see cref="IAudioPlayer"/> instances on demand.
    /// </summary>
    private sealed partial class AudioClip : IAudioClip
    {
        private readonly byte[] data;
        private readonly double? durationSeconds;
        private readonly ILogger<AudioService>? logger;

        public AudioClip(byte[] data, double? durationSeconds, ILogger<AudioService>? logger)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.durationSeconds = durationSeconds;
            this.logger = logger;
        }

        public double? DurationSeconds => this.durationSeconds;

        public IAudioPlayer CreateNewPlayer(IAudioManager manager)
        {
            ArgumentNullException.ThrowIfNull(manager);
            MemoryStream stream = new MemoryStream(this.data, writable: false);
            IAudioPlayer player = manager.CreatePlayer(stream);
            return player;
        }

        public void Dispose()
        {
            // Nothing to dispose; clip only holds audio bytes.
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