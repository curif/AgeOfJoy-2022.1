/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GameVideoPlayer : MonoBehaviour {

    Renderer Display;
    VideoPlayer videoPlayer;

    //Video data
    public string videoPath;
    public bool invertx = false; 
    public bool inverty = false;
    private Renderer videoPlayerRenderer;
    private bool isPreparing = false;

    // Start is called before the first frame update
    void Start() {
        Display = GetComponent<Renderer>();
        videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = Display;
        videoPlayerRenderer = Display.GetComponent<Renderer>();
        isPreparing = false;
    }

    public GameVideoPlayer setVideo(string path, bool invertX = false, bool invertY = false) {
        if (string.IsNullOrEmpty(path)) 
            return this;

        videoPath = path;
        invertx = invertX;
        inverty = invertY;
        //gameObject.GetComponent<MeshRenderer>().material =  Resources.Load<Material>("Cabinets/Materials/Screen");
        //videoPlayer.url = videoPath;
        //videoPlayer.Prepare();
        ConfigManager.WriteConsole($"[videoPlayer] Start {videoPath} ====");
        return this;
    }

    private void PrepareVideo()
    {
        isPreparing = true;
        videoPlayer.Prepare();
    }

    public GameVideoPlayer Play() {
        if (videoPlayer == null || string.IsNullOrEmpty(videoPath) || isPreparing)
            return this;
        
        Display.materials[1].SetFloat("MirrorX", invertx? 1f:0f);
        Display.materials[1].SetFloat("MirrorY", inverty? 1f:0f);

        if (videoPlayer.url != videoPath)
            videoPlayer.url = videoPath;

        if (! videoPlayer.isPrepared) {
            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.errorReceived += ErrorReceived;
            ConfigManager.WriteConsole($"[videoPlayer] prepare {videoPath} ====");
            PrepareVideo();
        }
        else if (videoPlayer.isPaused) {
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        return this;
    }

    public bool isVisible() {
        // don't works very well, some time you are looking at the video and its paused.
        return videoPlayerRenderer.isVisible;
    }

    public GameVideoPlayer Pause() {
        if (videoPlayer == null || string.IsNullOrEmpty(videoPath) || ! videoPlayer.isPrepared || videoPlayer.isPaused || isPreparing)
            return this;

        //is is necessary because the VideoPlayer.Pause method only works if isLooping is set to false. If isLooping is set to true, the Pause method will have no effect and the video will continue to play.
        //ConfigManager.WriteConsole($"[videoPlayer] pause {videoPath} ====");
        videoPlayer.isLooping = false;
        videoPlayer.Pause();
        return this;
    }

    public GameVideoPlayer Stop() {
        if (string.IsNullOrEmpty(videoPath) || ! videoPlayer.isPrepared)
            return this;

        ConfigManager.WriteConsole($"[videoPlayer] stop {videoPath} ====");
        videoPlayer.Stop();

        return this;
    }
    
    void PrepareCompleted(VideoPlayer vp)
    {
        ConfigManager.WriteConsole($"[videoPlayer] PrepareCompleted play {videoPath} ====");
        vp.Play();
        // The video is ready to play
        isPreparing = false;
    }

    void ErrorReceived(VideoPlayer vp, string message)
    {
        ConfigManager.WriteConsole($"[videoPlayer] ERROR {videoPath} - {message}");
    }

/*    
    void Update() {
        if (Display != null && videoPlayer != null && videoPlayer.isPlaying && !videoPlayer.isPaused ) {
            Display.materials[1].SetFloat("u_time", Time.fixedTime);
        }
    }
*/
}
