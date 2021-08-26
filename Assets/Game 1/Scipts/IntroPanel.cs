using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace QuizModule
{
    /// <summary>
    /// Handling behaviour for displaying eye-test introduction.
    /// </summary>
    public class IntroPanel : MonoBehaviour
    {
        public static IntroPanel instance { get; private set; }

        public Button NextButton;
        
        public Text mainText;

        public VideoPlayer video_player;

        public VideoClip clip_operator;

        public VideoClip clip_subject;

        private List<string> IntroText_Player = new List<string>(TOTAL_INTRO)
        {
            "You are the subject.",
            "Please face the dome.",
            "Focus your LEFT eye on the light spot at the center.",
            "In a moment there will be light spots on the dome.\n\n Please press the button once you see the light every time.",
            "The test will be repeated with the RIGHT eye."
        };

        // Same order, and corresponds to each of the string in IntroText_Player
        private List<string> IntroText_Operator = new List<string>(TOTAL_INTRO)
        {
            "You are the operator. \nInstruct the subject to face the dome.",
            "Instrcut the subject to focus their LEFT eye on the light spot at the center.",
            "In a moment there will be light spots on the dome.\n\n Instruct the subject to press the button once they sees the light every time."
        };

        private const int TOTAL_INTRO = 4;

        public int introCount = 0;

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

            QuizManager.instance.SpotSample.SetActive(false);
            if (video_player.isPlaying)
            {
                if (QuizManager.instance.user.isOperator)
                {
                    if (video_player.time <= 4f)
                    {
                        mainText.text = IntroText_Operator[0];
                    }
                    else if (video_player.time <= 8f)
                    {
                        mainText.text = IntroText_Operator[1];
                    }
                    else if (video_player.time <= 15f)
                    {
                        mainText.text = IntroText_Operator[2];
                        QuizManager.instance.SpotSample.SetActive(true);
                    }

                    
                }
                else
                {
                    if (video_player.time <= 2f)
                    {
                        mainText.text = IntroText_Player[0];
                    }
                    else if (video_player.time <= 3f)
                    {
                        mainText.text = IntroText_Player[1];
                    }
                    else if (video_player.time <= 6f)
                    {
                        mainText.text = IntroText_Player[2];
                    }
                    else if (video_player.time <= 12f)
                    {
                        mainText.text = IntroText_Player[3];
                        QuizManager.instance.SpotSample.SetActive(true);
                    }
                    else if (video_player.time <= 14f)
                    {
                        mainText.text = IntroText_Player[4];
                    }
                }
            }

            if (!video_player.isPlaying && QuizManager.instance.user.isOperator && video_player.time > 14f)
                NextButton.gameObject.SetActive(true);
        }

        public void Restart()
        {
            NextButton.gameObject.SetActive(false);
            if (QuizManager.instance.user.isOperator)
            {
                video_player.clip = clip_operator;
            }
            else
            {
                video_player.clip = clip_subject;
            }
            video_player.Play();
        }
    }
}