using UnityEngine;

namespace AOJ.Managers
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [SerializeField]
        private AudioClip exitGameSound; // AudioClip for the exit game sound effect

        private AudioSource audioSource; // Reference to the AudioSource component
        private bool isPlayingExitSound; // Private field to track if the exit sound is playing

        public bool IsPlayingExitSound // Public property to get the sound playing state
        {
            get { return isPlayingExitSound; }
            private set { isPlayingExitSound = value; }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                audioSource = GetComponent<AudioSource>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayExitGameSound()
        {
            if (audioSource != null && exitGameSound != null && !isPlayingExitSound)
            {
                audioSource.PlayOneShot(exitGameSound);
                isPlayingExitSound = true;
            }
        }

        public void StopExitGameSound()
        {
            if (audioSource.isPlaying && isPlayingExitSound)
            {
                audioSource.Stop();
                isPlayingExitSound = false;
            }
        }
    }
}
