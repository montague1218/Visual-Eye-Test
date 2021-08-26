using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizModule
{
    /// <summary>
    /// Manager to output audio
    /// </summary>
    public class AudioPlayer : MonoBehaviour
    {
        public static AudioPlayer instance { get; private set; }

        public AudioSource source;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
            DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayAudio(AudioClip clip)
        {
            source.Stop();
            source.clip = clip;
            source.PlayOneShot(clip);
        }
    }
}