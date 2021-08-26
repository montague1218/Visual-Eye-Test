using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game3Module
{
    public class DoctorPanel : MonoBehaviour
    {
        public static DoctorPanel instance { get; private set; }

        public GameObject group_startPage;
        public GameObject group_introPage;
        public GameObject group_startTestPage;
        public GameObject finger_demo;
        public GameObject skip_button;
        public GameObject yes_button;
        public GameObject startTest_button;

        public Text introText;

        public List<AudioClip> clips = new List<AudioClip>();

        public AudioClip leftEye;

        private List<string> narrtion_text = new List<string>
        {
            "Hello! After the visual field test. We are going to perform another test on other cranial nerves controlling eyeball movement.",
            "Please listen to our instructions now!",
            "Please look straight forward and stare at my index finger, do you see it clearly?",
            "I will move the index finger horizontally or vertically.",
            "Please follow my finger but keep your head steady.",
            "We are going to start the test now.",
            "OK. Let’s see how your right eye moves."
        };

        private List<float> narration_duration = new List<float>
        {
            8.5f, 3f, 5f, 4f, 3f, 3f, 3f
        };

        private bool finger_moving = false;
        private bool play_intro = false;
        public float timer = 0f;
        public int current_audio = 0;

        private const int SHOW_FINGER = 2;
        private const int FINGER_MOVE = 3;

        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(gameObject);
            else
                instance = this;
            DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            UpdateAudioStatus();
            MoveFingerAround();
        }

        public void Reset()
        {
            group_startPage.SetActive(true);
            group_introPage.SetActive(false);
            group_startTestPage.SetActive(false);
            finger_demo.SetActive(false);

            timer = 0f;
            play_intro = false;
            finger_moving = false;
            current_audio = 0;
        }

        public void GoIntroPage()
        {
            group_startPage.SetActive(false);
            group_introPage.SetActive(true);
            skip_button.SetActive(true);
            yes_button.SetActive(false);
            startTest_button.SetActive(false);

            play_intro = true;
            PlayAudio(0);
        }

        public void UpdateAudioStatus()
        {
            if (play_intro)
            {
                timer += Time.deltaTime;
                if (current_audio < narration_duration.Count)
                {
                    if (timer >= narration_duration[current_audio])
                    {
                        timer -= narration_duration[current_audio];
                        current_audio++;
                        if (current_audio < narration_duration.Count)
                        {
                            PlayAudio(current_audio);
                        }
                        else
                        {
                            skip_button.SetActive(false);
                            startTest_button.SetActive(true);
                        }
                    }

                    if (current_audio == SHOW_FINGER)
                    {
                        play_intro = false;
                        finger_demo.SetActive(true);
                        skip_button.SetActive(false);
                        yes_button.SetActive(true);
                    }
                    else if (current_audio == FINGER_MOVE)
                    {
                        play_intro = false;
                        finger_moving = true;
                    }
                }
                else
                {
                    play_intro = false;
                }
            }
        }

        public void SkipIntroduction()
        {
            StartTest();
        }

        public void StartTest()
        {
            play_intro = false;
            
            group_introPage.SetActive(false);
            group_startTestPage.SetActive(true);
        }

        public void ConfirmSeenFinger()
        {
            skip_button.SetActive(true);
            yes_button.SetActive(false);
            timer += narration_duration[current_audio];
            play_intro = true;
        }

        private void PlayAudio(int index)
        {
            introText.text = narrtion_text[index];
            AudioPlayer.instance.PlayAudio(clips[index]);
        }

        private void MoveFingerAround()
        {
            if (finger_moving)
            {
                finger_demo.SetActive(true);

                Vector3 pos = finger_demo.transform.position;
                if (pos.x <= 50f)
                    finger_demo.transform.position += new Vector3(0.5f, 0, 0);
                else if(pos.y <= 40f)
                    finger_demo.transform.position += new Vector3(0, 0.5f, 0);
                else
                {
                    finger_moving = false;
                    play_intro = true;
                }
            }
        }
    }
}