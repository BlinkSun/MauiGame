namespace MauiGame.Core.Contracts;

/// <summary>
/// Abstract audio services for loading and playing sounds/music.
/// </summary>
public interface IAudio : IDisposable
{
    /// <summary>Loads an audio clip from the content system.</summary>
    Task<IAudioClip> LoadClipAsync(string path, CancellationToken cancellationToken);

    /// <summary>Creates a playback instance for the clip and starts playing if requested.</summary>
    IAudioInstance Play(IAudioClip clip, float volume = 1.0f, bool loop = false, bool autoStart = true);
}

/// <summary>Represents a loaded audio asset.</summary>
public interface IAudioClip : IDisposable
{
    /// <summary>Clip duration in seconds if known, otherwise null.</summary>
    double? DurationSeconds { get; }
}

/// <summary>Represents a controllable playback instance.</summary>
public interface IAudioInstance : IDisposable
{
    /// <summary>Gets or sets the volume [0..1].</summary>
    float Volume { get; set; }

    /// <summary>Gets or sets whether the instance should loop.</summary>
    bool Loop { get; set; }

    /// <summary>Returns whether the instance is currently playing.</summary>
    bool IsPlaying { get; }

    /// <summary>Starts playback.</summary>
    void Play();

    /// <summary>Pauses playback.</summary>
    void Pause();

    /// <summary>Stops playback and resets position.</summary>
    void Stop();
}