using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace QuizModule
{
    /// <summary>
    /// Handle Q&A of eye-defect patterns
    /// </summary>
    public class QuestionPanel : MonoBehaviour
    {
        public static QuestionPanel instance { get; private set; }

        public AudioClip identifyClip;
        public AudioClip incorrectClip;
        public AudioClip doneTestClip;
        public AudioClip waitClip;

        public Button NextButton;

        public Image AnswerImage;
        public Image TestResultImage_Operator;
        public Image TestResultImage_Subject;
        public List<Sprite> AnswerImageList;

        public Text GuideText;

        public GameObject TestResultGroup;
        public GameObject OperatorPanel;
        public GameObject SubjectWaitPanel;
        public GameObject SubjectAnswerPanel;

        public List<AudioClip> correctClips;

        public GameObject AnswerButtonGroup;

        public int correctAnswer;

        private bool correct;


        private readonly List<string> EyePatterAnswers = new List<string>()
        {
            "This could be normal visual eye field!",
            "This could be bitemporal hemianopia as the optic chiasm is blocked",
            "This could be homonymous hemianopia as the optic nerve posterior to optic chiasm is blocked",
            "This could be superior quadrantanopia as the superior optic radiations is blocked",
            "This could be inferior quadrantanopia as the inferior optic radiations is blocked"
        };

        private readonly List<string> EyePatternOptions = new List<string>()
        {
            "Normal visual field",
            "Bitemporal hemianopia",
            "Homonymous hemianopia",
            "Superior qadrantanopia",
            "Inferior quadrantanopia"
        };

        // Sequencial ordering of eye patterns
        private readonly List<string> letters = new List<string>()
        {
            "A", "B", "C", "D", "E"
        };

        // Texts for UI display
        private const string TEXT_INCORRECT = "Your answer is incorrect. Please try again!";
        private const string TEXT_IDENTIFY = "Please identify what visual pattern the subject has";
        private const string TEXT_DONE_TEST = "The subject has completed the eye test";
        private const string TEXT_WAIT = "Please wait for a moment, as we are analyzing your test results";

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

        public void Display_Answer()
        {
            Display_Answer(correctAnswer);
        }

        public void Display_Answer(int i)
        {
            if (!QuizManager.instance.user.isOperator)
            {
                SubjectWaitPanel.SetActive(false);
                SubjectAnswerPanel.SetActive(true);
            }

            TestResultGroup.SetActive(false);
            AnswerImage.gameObject.SetActive(true);

            GuideText.text = GetAnswerText(i);
            AudioPlayer.instance.PlayAudio(correctClips[i]);

            // Trigger this fucntion at subject's device through network
            if (!QuizManager.instance.user.receive_Answer)
                TicTacNetworkManager.instance.EmitDisplayAnswer();
        }

        /// <summary>
        /// Behaviour of NextButton: Go to the next UI page
        /// </summary>
        public void Next()
        {
            NextButton.gameObject.SetActive(true);
            TestResultGroup.SetActive(true);
            AnswerImage.gameObject.SetActive(false);
            AnswerButtonGroup.SetActive(true);

            if (GuideText.text == TEXT_DONE_TEST) // Transit UI from done_test to identify_eye_case
            {
                ShowAnswerButtons(true);

                GuideText.text = TEXT_IDENTIFY;
                AudioPlayer.instance.PlayAudio(identifyClip);
            }
            else if (GuideText.text == TEXT_INCORRECT) // Transit UI from incorrect_answer to identify_eye_case
            {
                ShowAnswerButtons(true);

                GuideText.text = TEXT_IDENTIFY;
                AudioPlayer.instance.PlayAudio(identifyClip);
            }
            else if (correct)
            {
                QuizManager.instance.GoNextUI();
            }
        }

        public void OnClickAnswer(int answer)
        {
            ShowAnswerButtons(false);

            correct = answer == correctAnswer;
            if (correct)
            {
                Display_Answer();

                if (QuizManager.instance.user.isOperator)
                    QuizManager.instance.UpdateAnswer(correctAnswer, "QUESTION_PANEL");
            }
            else
            {
                GuideText.text = TEXT_INCORRECT;
                AudioPlayer.instance.PlayAudio(incorrectClip);

                NextButton.interactable = true;

                // If clicked the wrong answer, set answer-buttons non-interactable
                foreach (Text txt in AnswerButtonGroup.GetComponentsInChildren<Text>())
                    if (txt.text == EyePatternOptions[answer])
                        txt.gameObject.GetComponent<Button>().interactable = false;
            }
        }

        /// <summary>
        /// Reset the panel and go to its first step
        /// </summary>
        public void Initilization()
        {
            correct = false;

            TestResultGroup.SetActive(false);
            OperatorPanel.SetActive(false);
            SubjectWaitPanel.SetActive(false);
            SubjectAnswerPanel.SetActive(false);

            NextButton.gameObject.SetActive(false);
            AnswerButtonGroup.SetActive(false);
            AnswerImage.gameObject.SetActive(false);
            TestResultImage_Operator.sprite = AnswerImageList[correctAnswer];
            TestResultImage_Subject.sprite = AnswerImageList[correctAnswer];

            // Add eye pattern's names to button's text
            int i = 0;
            foreach (Text txt in AnswerButtonGroup.GetComponentsInChildren<Text>())
            {
                txt.text = GetOptionName(i);
                i++;
            }

            if (QuizManager.instance.user.isOperator)
            {
                OperatorPanel.SetActive(true);

                GuideText.text = TEXT_IDENTIFY;
                AudioPlayer.instance.PlayAudio(identifyClip);

                NextButton.gameObject.SetActive(true);
                NextButton.interactable = false;
                AnswerButtonGroup.SetActive(true);

                TestResultGroup.SetActive(true);
                TestResultImage_Operator.gameObject.SetActive(true);
            }
            else
            {
                SubjectWaitPanel.SetActive(true);

                GuideText.text = TEXT_WAIT;
                AudioPlayer.instance.PlayAudio(waitClip);

                NextButton.interactable = false;
            }
        }

        private string GetAnswerText(int i)
        {
            return EyePatterAnswers[i];
        }

        private string GetOptionName(int i)
        {
            return letters[i] + ". " + EyePatternOptions[i];
        }

        private void ShowAnswerButtons(bool isShow)
        {
            foreach (Button button in AnswerButtonGroup.GetComponentsInChildren<Button>())
                button.interactable = isShow;
            NextButton.interactable = !isShow;
        }
    }
}