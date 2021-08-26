using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizModule
{
    /// <summary>
    /// Handle behaviour of selecting eye cases (total 5 cases)
    /// </summary>
    public class CasePanel : MonoBehaviour
    {
        public static CasePanel instance;
        
        public AudioClip pleaseSelectClip;

        public Text GuideText;

        public GameObject CircleFrame;
        public GameObject CaseButtonGroup;

        public GameObject OperatorPanel;
        public GameObject SubjectPanel;

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

        /// <summary>
        /// OnClick event for button when select eye cases.
        /// </summary>
        public void ButtonClickByIndex(int i)
        {
            int pattern = Random.Range(0, 4);

            ObjectClick.instance.SetEyePattern(pattern);
            QuestionPanel.instance.correctAnswer = pattern;
            DebugPanel.instance.SetLogger(2, "ANS: " + ObjectClick.instance.defaultPattern.ToString());

            TicTacNetworkManager.instance.EmitEyePattern(pattern);
            QuizManager.instance.GoNextUI();
        }

        public void Display_Select_Case()
        {
            if (QuizManager.instance.user.isOperator)
            {
                OperatorPanel.SetActive(true);
                SubjectPanel.SetActive(false);
                AudioPlayer.instance.PlayAudio(pleaseSelectClip);
            }
            else
            {
                OperatorPanel.SetActive(false);
                SubjectPanel.SetActive(true);
            }
        }
    }
}