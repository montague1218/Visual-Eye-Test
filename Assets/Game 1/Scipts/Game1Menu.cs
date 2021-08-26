using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TicTacToeServerModule;

namespace QuizModule
{
    /// <summary>
    /// Handle main menu in game 1
    /// </summary>
    public class Game1Menu : MonoBehaviour
    {
        public static Game1Menu instance { get; private set; }
        public Text logger;

        public AudioClip chooseRoleClip;
        public AudioClip startServerClip;
        public AudioClip connectServerClip;
        public AudioClip startGameClip;

        public GameObject instructionText_1;
        public GameObject instructionText_2;
        public Text networkGuideText;
        public Text networkButtonText;
        public Text startGameText;

        public Button startGameButton;

        public GameObject Group_Instruction;
        public GameObject Group_Role;
        public GameObject Group_Network;
        public GameObject Group_StartGame;

        public GameObject EnvironmentObject;

        public enum UserRole { Player, Operator };

        public UserRole myRole;

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
            //Group_Role.SetActive(true);
            //AudioPlayer.instance.PlayAudio(chooseRoleClip);

            Group_Instruction.SetActive(true);
            Group_Role.SetActive(false);
            Group_Network.SetActive(false);
            Group_StartGame.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void NextInstruction()
        {
            if (instructionText_2.activeSelf) {
                Group_Instruction.SetActive(false);
                Group_Role.SetActive(true);
            }
            else
            {
                instructionText_1.SetActive(false);
                instructionText_2.SetActive(true);
            }
        }

        // Triggered through button
        public void StartGame()
        {
            QuizManager.instance.MainMenu.transform.localScale = Vector3.zero;
            PlayEnvironmentAnime();

            StartCoroutine(StartTest());
        }

        // Triggered through button
        public void SelectRole(bool isOperator)
        {
            QuizManager.instance.UpdateRole(isOperator);
            
            Group_Role.SetActive(false);
            Group_Network.SetActive(true);

            if (QuizManager.instance.user.isOperator)
            {
                networkGuideText.text = "Start the server";
                networkButtonText.text = "Start";
                AudioPlayer.instance.PlayAudio(startServerClip);
            }
            else
            {
                networkGuideText.text = "Connect to the server";
                networkButtonText.text = "Connect";
                AudioPlayer.instance.PlayAudio(connectServerClip);
            }
        }

        public void StartNetwork()
        {
            if (QuizManager.instance.user.isOperator)
            {
                StartServer();
                
            }
            else
            {
                ConnectServer();
            }
        }

        private void StartServer()
        {
            try
            {
                TicTacToeServer.instance.CreateServer();
                if (TicTacToeServer.instance.serverRunning)
                {
                    DebugPanel.instance.SetLogger(3, "BUILD SER SUCCESS");

                    
                    //TicTacNetworkManager.instance.EmitJoinGame("001");
                }
            }
            catch
            {
                DebugPanel.instance.SetLogger(3, "BUILD SER FAILURE");
            }

            if (!TicTacNetworkManager.instance.serverFound)
            {
                Group_Network.SetActive(false);
                Group_StartGame.SetActive(true);

                startGameText.text = "Waiting subject.";
                AudioPlayer.instance.PlayAudio(startGameClip);
            }
            else
            {
                
                startGameText.text = "Server not started.";
                
            }
        }

        private void ConnectServer()
        {
            //TicTacNetworkManager.instance.EmitJoinGame("001");
            if (TicTacNetworkManager.instance.serverFound)
            {
                //DebugPanel.instance.SetLogger(3, "CONN SER SUCCESS");
                GoStartGame();
                
            }
            else
            {
                //DebugPanel.instance.SetLogger(3, "CONN SER FAILURE");

                networkGuideText.text = "Connect server again";
            }
        }

        public void GoStartGame()
        {
            startGameText.text = "Please start the game.";

            Group_Network.SetActive(false);
            Group_StartGame.SetActive(true);

            AudioPlayer.instance.PlayAudio(startGameClip);
        }

        // Triggered through button
        public void OpenDebugPanel()
        {
            //QuizManager.instance.DebugPanel_.SetActive(true);
        }

        // Triggered through button
        public void GoBackGameMenu()
        {
            SceneManager.LoadScene(1);
        }

        private void PlayEnvironmentAnime()
        {
            EnvironmentObject.SetActive(true);

            Animator anim = EnvironmentObject.GetComponent<Animator>();
            if (QuizManager.instance.user.isOperator)
            {
                anim.Play("Operator_Angle");
                StartCoroutine(Wait_Operator());
            }
            else
            {
                anim.Play("Subject_Angle");
                StartCoroutine(Wait_Subject());
            }
        }

        private IEnumerator StartTest()
        {
            yield return new WaitForSeconds(3);

            //TicTacNetworkManager.instance.EmitJoinGame("001");

            if (QuizManager.instance.user.emit_StartTest)
            {
            }
            else
            {
                
            }
            
        }

        private IEnumerator Wait_Operator()
        {
            yield return new WaitForSeconds(3);
            EnvironmentObject.SetActive(false);
        }

        private IEnumerator Wait_Subject()
        {
            yield return new WaitForSeconds(3);
            EnvironmentObject.SetActive(false);
            
        }
    }
}