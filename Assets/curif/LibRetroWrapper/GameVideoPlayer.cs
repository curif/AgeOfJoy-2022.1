/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVideoPlayer : MonoBehaviour {

    Renderer Display;
    UnityEngine.Video.VideoPlayer videoPlayer;

    //Video data
    public string videoPath;
    public bool invertx = false; 
    public bool inverty = false;

    // Start is called before the first frame update
    void Start() {
        Display = GetComponent<Renderer>();
        videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = Display;
    }

    public GameVideoPlayer setVideo(string path, bool invertX = false, bool invertY = false) {
        videoPath = path;
        invertx = invertX;
        inverty = invertY;
        if (videoPlayer != null && Display != null && !string.IsNullOrEmpty(path)) {
            //gameObject.GetComponent<MeshRenderer>().material =  Resources.Load<Material>("Cabinets/Materials/Screen");
            videoPlayer.url = videoPath;
            videoPlayer.Prepare();
            ConfigManager.WriteConsole($"video player: {videoPath} ====");
        }
        return this;
    }

    public GameVideoPlayer Play() {

        Display.materials[1].SetFloat("MirrorX", invertx? 1f:0f);
        Display.materials[1].SetFloat("MirrorY", inverty? 1f:0f);

        videoPlayer?.Play();
        return this;
    }

    public GameVideoPlayer Pause() {
        videoPlayer?.Pause();
        return this;
    }

    public GameVideoPlayer Stop() {
        videoPlayer?.Stop();
        return this;
    }

    // Update is called once per frame
    void Update() {
        if (Display != null && videoPlayer != null && videoPlayer.isPlaying && !videoPlayer.isPaused ) {
            Display.materials[1].SetFloat("u_time", Time.fixedTime);
        }
    }

}
