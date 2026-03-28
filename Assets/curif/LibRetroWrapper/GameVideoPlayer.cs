/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
//#define DISABLE_VIDEO
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(TextureCache))]
public class GameVideoPlayer : MonoBehaviour
{

    Renderer display;
    VideoPlayer videoPlayer;
    bool invertx, inverty;
    ShaderScreenBase shader;

    //Video data
    public string videoPath;
    private bool isPreparing = false;
    private bool isReady = false;
    private bool loopEnabled = true;
    public string FirstTexturePath;
    //private Texture2D FirstTexture = null;
    private TextureCache textureCache;

    // Start is called before the first frame update
    void Start()
    {
        display = GetComponent<Renderer>();
        videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        isPreparing = false;
        textureCache = GetComponent<TextureCache>();
    }

    public GameVideoPlayer setVideo(string path, ShaderScreenBase shader, bool invertx, bool inverty)
    {
#if !DISABLE_VIDEO
        this.shader = shader;

        if (string.IsNullOrEmpty(path))
            return this;

        this.videoPath = path;
        this.invertx = invertx;
        this.inverty = inverty;

        shader.Invert(invertx, inverty);
        textureCache.Init(path);
        showCachedImage();

        ConfigManager.WriteConsole($"[videoPlayer] Start {videoPath} ====");
#endif
        return this;
    }

    public Texture texture
    {
        get
        {
            return videoPlayer.texture;
        }
    }

    public void showCachedImage()
    {
        if (textureCache.AlreadyCached())
            shader.Activate(textureCache.CachedTexture);
    }

    private void PrepareVideo()
    {
        videoPlayer.prepareCompleted += PrepareCompleted;
        videoPlayer.errorReceived += ErrorReceived;

        isPreparing = true;
        isReady = false;
        videoPlayer.Prepare();
    }

    public GameVideoPlayer Play()
    {
#if !DISABLE_VIDEO
        // ConfigManager.WriteConsole($"[videoPlayer.Play] prepared: {videoPlayer.isPrepared} playing: {videoPlayer.isPlaying} {videoPath}  ====");
        if (videoPlayer == null || string.IsNullOrEmpty(videoPath) || isPreparing)
            return this;

        if (videoPlayer.url != videoPath)
        {
            videoPlayer.url = videoPath;
            videoPlayer.playOnAwake = true;
            videoPlayer.isLooping = loopEnabled;
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.APIOnly;
        }

        // ConfigManager.WriteConsole($"[videoPlayer.Play] isPlaying: {videoPlayer.isPlaying} ====");
        if (!videoPlayer.isPrepared)
        {
            ConfigManager.WriteConsole($"[videoPlayer.Play] prepare {videoPath} ====");
            PrepareVideo();
            shader.Invert(invertx, inverty);
            if (textureCache.AlreadyCached())
                shader.Activate(textureCache.CachedTexture);
            else
                shader.Activate(ShaderScreenBase.StandByTexture);
        }
        else if (isReady && !videoPlayer.isPlaying)
        {
            ConfigManager.WriteConsole($"[videoPlayer.Play] PLAY {videoPath} ====");
            videoPlayer.isLooping = loopEnabled;
            if (videoPlayer.canSetSkipOnDrop)
                videoPlayer.skipOnDrop = true;
            shader.ApplyConfiguration();
            shader.Invert(invertx, inverty);

            videoPlayer.texture.filterMode = FilterMode.Bilinear;
            videoPlayer.texture.anisoLevel = 0;
            shader.Activate(videoPlayer.texture);
            videoPlayer.Play();
        }

        if (isReady && videoPlayer.isPlaying && !textureCache.AlreadyCached())
        {
            textureCache.Load(videoPlayer.texture);
        }

#endif
        return this;
    }

    public GameVideoPlayer Pause()
    {
#if !DISABLE_VIDEO
        if (videoPlayer == null || string.IsNullOrEmpty(videoPath)
            || !videoPlayer.isPrepared || isPreparing || videoPlayer.isPaused || !videoPlayer.isPlaying)
            return this;

        //is is necessary because the VideoPlayer.Pause method only works if isLooping is set to false. 
        // If isLooping is set to true, the Pause method will have no effect and the video will 
        // continue to play.
        ConfigManager.WriteConsole($"[videoPlayer.Pause] {videoPath} ====");
        videoPlayer.isLooping = false;
        videoPlayer.Pause();

        // ConfigManager.WriteConsole($"[videoPlayer.Pause] isPlaying: {videoPlayer.isPlaying} ====");
        // isReady = false;
#endif
        return this;
    }

    public GameVideoPlayer Stop()
    {
#if !DISABLE_VIDEO
        if (string.IsNullOrEmpty(videoPath) || videoPlayer == null)
            return this;

        // ConfigManager.WriteConsole($"[videoPlayer.Stop] {videoPath} ====");
        //destroy internal resources.
        videoPlayer.Stop();
        if (textureCache.AlreadyCached() && shader.Texture != textureCache.CachedTexture)
            shader.Texture = textureCache.CachedTexture;
        // shader.Activate(textureCache.CachedTexture);

#endif
        return this;
    }

    // ── AGEBasic video control additions ──────────────────────────────────────

    /// <summary>
    /// Change the video file without replacing the shader reference already set up
    /// by AGEBasicScreenController. Resets the prepared/ready state so the next
    /// Play() call will re-prepare the new clip.
    /// </summary>
    public GameVideoPlayer ChangeVideo(string path, bool invertX, bool invertY)
    {
#if !DISABLE_VIDEO
        if (string.IsNullOrEmpty(path))
            return this;

        this.videoPath = path;
        this.invertx = invertX;
        this.inverty = invertY;

        // Reset so Play() will call PrepareVideo() for the new clip.
        isPreparing = false;
        isReady = false;
        videoPlayer.Stop();

        ConfigManager.WriteConsole($"[videoPlayer.ChangeVideo] {videoPath}");
#endif
        return this;
    }

    /// <summary>
    /// Enable or disable looping. Takes effect on the next Play() call and
    /// immediately if the video is already prepared and playing.
    /// </summary>
    public GameVideoPlayer SetLoop(bool loop)
    {
        loopEnabled = loop;
#if !DISABLE_VIDEO
        if (videoPlayer != null && videoPlayer.isPrepared && !videoPlayer.isPaused)
            videoPlayer.isLooping = loop;
#endif
        return this;
    }

    /// <summary>Returns true when looping is enabled.</summary>
    public bool GetLoopStatus() => loopEnabled;

    /// <summary>Seek to an absolute position in seconds.</summary>
    public GameVideoPlayer SeekTo(double seconds)
    {
#if !DISABLE_VIDEO
        if (videoPlayer == null || !videoPlayer.isPrepared)
            return this;
        videoPlayer.time = seconds;
#endif
        return this;
    }

    /// <summary>Current playback position in seconds.</summary>
    public double GetCurrentTime()
    {
        return videoPlayer != null ? videoPlayer.time : 0;
    }

    /// <summary>Total video duration in seconds.</summary>
    public double GetDuration()
    {
        return videoPlayer != null ? (double)videoPlayer.length : 0;
    }

    /// <summary>
    /// 0 = not loaded/preparing · 1 = stopped/ready · 2 = playing · 3 = paused
    /// </summary>
    public int GetStatus()
    {
        if (videoPlayer == null || string.IsNullOrEmpty(videoPath)) return 0;
        if (isPreparing) return 0;
        if (videoPlayer.isPlaying) return 2;
        if (videoPlayer.isPaused) return 3;
        if (isReady) return 1;
        return 0;
    }

    // ─────────────────────────────────────────────────────────────────────────

    void PrepareCompleted(VideoPlayer vp)
    {
        ConfigManager.WriteConsole($"[videoPlayer.PrepareCompleted] {videoPath} ====");
        isPreparing = false;
        isReady = true;
        vp.prepareCompleted -= PrepareCompleted;

        // Start playing immediately — handles shader switch and works for both
        // attraction video and AGEBasic-controlled video (VIDEOPLAY is a one-shot call).
        Play();
    }

    void ErrorReceived(VideoPlayer vp, string message)
    {
        ConfigManager.WriteConsoleWarning($"[videoPlayer] ERROR {videoPath} - {message}");
        showCachedImage();
    }
}
