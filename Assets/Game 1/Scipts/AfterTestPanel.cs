using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace QuizModule
{
    /// <summary>
    /// Handle post-test behaviour
    /// Option: wait for operator, repeat test, quit game
    /// </summary>
    public class AfterTestPanel : MonoBehaviour
    {
        public static AfterTestPanel instance { get; private set; }

        // Clip for operator
        public AudioClip subjectDoneClip;

        // Clip for player
        public AudioClip waitClip;
        public AudioClip repeatClip;
        public AudioClip quitClip;

        public Text mainText;

        public GameObject OKButton;
        public GameObject ButtonGroupRepeat;
        public GameObject ButtonGroupQuit;

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

        public void RepeatTest(bool repeat)
        {
            if (repeat)
            {
                TicTacNetworkManager.instance.EmitDisconnect();
            }
            else
            {
                HideAllButtons();
                ButtonGroupQuit.SetActive(false);
                AskQuit();
            }
        }

        public void QuitTest(bool quit)
        {
            if (quit)
            {
                TicTacNetworkManager.instance.EmitDisconnect();
                SceneManager.LoadScene(0);
            }
            else
            {
                // Change to game 1-3 menu later
                TicTacNetworkManager.instance.EmitDisconnect();
            }
        }

        public void AskRepeatTest()
        {
            mainText.text = "Good job! Do you want to test other cases?";
            AudioPlayer.instance.PlayAudio(repeatClip);

            HideAllButtons();
            ButtonGroupRepeat.SetActive(true);
        }

        private void AskQuit()
        {
            mainText.text = "Do you want to quit?";
            AudioPlayer.instance.PlayAudio(quitClip);

            HideAllButtons();
            ButtonGroupQuit.SetActive(true);
        }

        private void HideAllButtons()
        {
            OKButton.SetActive(false);
            ButtonGroupRepeat.SetActive(false);
            ButtonGroupQuit.SetActive(false);
        }
    }
}