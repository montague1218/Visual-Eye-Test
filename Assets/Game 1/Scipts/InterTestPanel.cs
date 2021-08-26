using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizModule
{
    /// <summary>
    /// Handle behaviour during test of left and right eye test.
    /// Option: Start right eye test (after left eye test)
    /// </summary>
    public class InterTestPanel : MonoBehaviour
    {
        public static InterTestPanel instance;

        public AudioClip doneTestClip;
        public AudioClip rightEyeClip_Operator;
        public AudioClip rightEyeClip;

        public Button NextButton;
        public Button PromptButton;

        public Text GuideText;
        public Text PromptButtonText;

        private const float PROMPT_COUNTDOWN = 5f;
        private float timer = 0f;

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
            if (!QuizManager.instance.user.isOperator)
            {
                timer += Time.deltaTime;

                int time_remain = (int)(PROMPT_COUNTDOWN - timer);
                time_remain = Mathf.Max(time_remain, 0);

                PromptButtonText.text = string.Format("Press to reconnect ({0}s)", time_remain);
                PromptButton.interactable = timer >= PROMPT_COUNTDOWN;
            }
            DebugPanel.instance.SetLogger(2, QuizManager.instance.current_UI.ToString());
        }

        /// <summary>
        /// Display prompts after left/right-eye test.
        /// </summary>
        public void Display_Retest()
        {
            timer = 0f;

            int current_UI = QuizManager.instance.current_UI;
            if (QuizManager.instance.user.isOperator)
            {
                if (current_UI == 6)
                {
                    GuideText.text = "The subject has completed testing for the left eye";
                    AudioPlayer.instance.PlayAudio(rightEyeClip_Operator);

                    NextButton.gameObject.SetActive(true);
                    PromptButton.gameObject.SetActive(false);
                }
                else if (current_UI == 8)
                {
                    GuideText.text = "The subject has completed the eye test";
                    AudioPlayer.instance.PlayAudio(doneTestClip);

                    NextButton.gameObject.SetActive(true);
                    PromptButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (current_UI == 6)
                {
                    GuideText.text = "Now please test for your right eye";
                    AudioPlayer.instance.PlayAudio(rightEyeClip);

                    NextButton.gameObject.SetActive(false);
                    PromptButton.gameObject.SetActive(true);
                }
                else if (current_UI == 8)
                {
                    GuideText.text = "You have completed the eye test";
                    AudioPlayer.instance.PlayAudio(rightEyeClip);

                    NextButton.gameObject.SetActive(false);
                    PromptButton.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Initiate left or right-eye test
        /// </summary>
        public void Next()
        {
            QuizManager.instance.GoNextUI();
        }

        public void PromptOperator()
        {
            int current_UI = QuizManager.instance.current_UI;

            if (current_UI == 6)
            {
                TicTacNetworkManager.instance.EmitUI(6);
                PromptButton.interactable = false;
                timer = 0f;
            }
            if (current_UI == 8)
            {
                TicTacNetworkManager.instance.EmitUI(8);
                PromptButton.interactable = false;
                timer = 0f;
            }
        }
    }
}