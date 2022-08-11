/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/


#define _debug_

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;


//[AddComponentMenu("curif/LibRetroWrapper/VideoPlayer")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GameVideoPlayer))]
public class LibretroScreenController : MonoBehaviour
{
  [SerializeField]
  public string GameFile = "1942.zip";

  [SerializeField]
  public string GameVideoFile;
  [SerializeField]
  public bool GameVideoInvertX = false;
  [SerializeField]
  public bool GameVideoInvertY = false;

  [SerializeField]
  public bool GameInvertX = false;
  [SerializeField]
  public bool GameInvertY = false;

  [SerializeField]
  public GameObject Player;
  [Tooltip("The minimal distance between the player and the screen to be active.")]
  [SerializeField]
  public float DistanceMinToPlayerToActivate = 2f;
  [Tooltip("The time in secs that the player has to look to another side to exit the game and recover mobility.")]
  [SerializeField]
  public int SecondsToWaitToExitGame = 2;

  [SerializeField]
  public int SecondsToWaitToFinishLoad = 2;

  [Tooltip("Adjust Gamma from 1.0 to 2.0")]
  [SerializeField]
  public LibretroMameCore.GammaOptions Gamma = LibretroMameCore.GammaOptions.GAMA_1;
  [Tooltip("Adjust bright from 0.2 to 2.0")]
  [SerializeField]
  public LibretroMameCore.BrightnessOptions Brightness = LibretroMameCore.BrightnessOptions.BRIGHT_1;

  private CoinSlotController CoinSlot;
  private GameObject Camera;
  private Renderer Display;
  private bool isVisible = false;
  private GameVideoPlayer videoPlayer;

  private CoinSlotController getCoinSlotController()
  {
    Transform coinslot = gameObject.transform.parent.Find("coin-slot-added");

    if (!coinslot)
      return null;

    return coinslot.gameObject.GetComponent<CoinSlotController>();
  }

  // Start is called before the first frame update
  void Start()
  {
    LibretroMameCore.WriteConsole($"{gameObject.name} Start");

    Display = GetComponent<Renderer>();

    Player = GameObject.Find("OVRPlayerControllerGalery");

    CoinSlot = getCoinSlotController();
    if (CoinSlot == null)
      Debug.LogError("Coin Slot not found in cabinet !!!! no one can play this game.");

    videoPlayer = gameObject.GetComponent<GameVideoPlayer>();
    videoPlayer.setVideo(GameVideoFile, invertX: GameVideoInvertX, invertY: GameVideoInvertY);

    StartCoroutine(loop());

    return;
  }

  public void Update()
  {
    // LibretroMameCore.WriteConsole($"MAME {GameFile} Libretro {LibretroMameCore.GameFileName} loaded: {LibretroMameCore.GameLoaded}");
    LibretroMameCore.Run(name, GameFile); //only runs if this game is running
    return;
  }

  void playVideo()
  {
    if (LibretroMameCore.isRunning(name, GameFile))
      //stop video when game is running
      videoPlayer?.Stop();
    else if (!enabled)
      //pause when not visibile
      videoPlayer?.Pause();
    else if (LibretroMameCore.GameLoaded)
      //pause when other game is running
      videoPlayer?.Pause();
    else
      videoPlayer?.Play();
    return;
  }

  IEnumerator loop()
  {
    while (true)
    {
      LibretroMameCore.WriteConsole($" LOOP {name} {GameFile}: enabled:{enabled} is running: {LibretroMameCore.isRunning(name, GameFile)} has coins: {CoinSlot.hasCoins()} some Game is Loaded: {LibretroMameCore.GameLoaded} ");
      StartIfHasCoin();
      playVideo();
      yield return new WaitForSeconds(0.5f);
    }
  }

  private void StartIfHasCoin()
  {

    if (!enabled)
      return;

    if (LibretroMameCore.GameLoaded)
      return;

    // doing nothing without payment!
    if (CoinSlot == null || !CoinSlot.hasCoins())
      return;

    Display.materials[1].SetFloat("MirrorX", GameInvertX ? 1f : 0f);
    Display.materials[1].SetFloat("MirrorY", GameInvertY ? 1f : 0f);

    //start mame
    LibretroMameCore.WriteConsole($"MAME Start game: {GameFile} in screen {name} +_+_+_+_+_+_+_+__+_+_+_+_+_+_+_+_+_+_+_+_");
    // LibretroMameCore.DistanceMinToPlayerToStartGame = DistanceMinToPlayerToActivate;
    LibretroMameCore.Speaker = GetComponent<AudioSource>();
    LibretroMameCore.Player = Player;
    LibretroMameCore.Display = Display;
    // LibretroMameCore.Camera = Camera;
    LibretroMameCore.SecondsToWaitToExitGame = SecondsToWaitToExitGame;
    LibretroMameCore.SecondsToWaitToFinishLoad = SecondsToWaitToFinishLoad;
    LibretroMameCore.Brightness = Brightness;
    LibretroMameCore.Gamma = Gamma;
    LibretroMameCore.CoinSlot = CoinSlot;
    LibretroMameCore.Start(name, GameFile);

    return;
  }

  private void OnAudioFilterRead(float[] data, int channels)
  {
    LibretroMameCore.MoveAudioStreamTo(GameFile, data);
  }

  private void OnDestroy()
  {
    LibretroMameCore.End(name, GameFile);
  }

  void OnBecameVisible()
  {
    enabled = true;
    playVideo();
  }
  void OnBecameInvisible()
  {
    enabled = false;
    playVideo();
  }
}