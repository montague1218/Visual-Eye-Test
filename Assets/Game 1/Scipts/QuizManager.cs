using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace QuizModule
{
    /// <summary>
    /// Main Manager to handle flow of game-1 (UI flow, network flow)
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager instance { get; private set; }

        
        public GameObject LeftCamera;
        public GameObject RightCamera;

        public GameObject ClickCircle;
        public GameObject Quad;
        public GameObject SpotSample;

        public GameObject MainMenu;
        public GameObject CasePanel_;
        public GameObject DebugPanel_;
        public GameObject InterTestPanel_;
        public GameObject IntroPanel_;
        public GameObject AfterTestPanel_;
        public GameObject QuestionPanel_;

        public GameObject EnvironmentObject;

        public enum UserRole { Player, Operator };

        public UserRole myRole; // For debug only

        public User user = User.Subject;
        
        public int current_UI = 2;

        public int current_intro = 0;

        public int current_spot = 0;

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
            //DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            OpenAllPanels();
            CloseAllPanels();
            //DebugPanel_.SetActive(true);
            //MainMenu.SetActive(true);
            //EnvironmentObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (current_UI == 4 || current_UI == 6)
            {
                LeftCamera.transform.rotation = Quaternion.Euler(Vector3.zero);
                RightCamera.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

        public void BackMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void Reset()
        {
            current_UI = 2;
            current_intro = 0;
            current_spot = 0;

            CloseAllPanels();
            TicTacCanvasManager.instance.OpenScreen(-1);
        }

        public void UpdateRole(bool isOperator)
        {
            if (isOperator)
            {
                user = User.Operator;
                myRole = UserRole.Operator;
            }
            else
            {
                user = User.Subject;
                myRole = UserRole.Player;
            }
        }

        /// <summary>
        /// Swap user role (Player, Operator)
        /// </summary>
        public void UpdateRole()
        {
            if (myRole == UserRole.Operator)
            {
                UpdateRole(false);
            }
            else
            {
                UpdateRole(true);
            }
        }

        public void UpdateAnswer(int i, string panel)
        {
            DebugPanel.instance.SetLogger(0, "EMIT_ANSWER");
        }

        public void UpdateRetest(bool repeat)
        {
            DebugPanel.instance.SetLogger(0, "EMIT_REPEAT");
        }

        public void EmitSpotLocation(Vector3 vec, bool visible)
        {
            if (user.emit_Spot)
            {
                TicTacNetworkManager.instance.EmitSpotLocation(vec, visible, current_spot + 1);

                current_spot++;
            }
        }

        public void ReceiveSpotLocation(Vector3 vec, bool visible, int order)
        {
            
            if (user.receive_Spot)
            {
                if (current_spot < order)
                {
                    DebugPanel.instance.SetLogger(3, string.Format("{0} {1}", current_spot, order));
                    current_spot = order;
                    if (visible)
                    {
                        ObjectClick.instance.CopySpotGlobal(vec);
                    }
                    else
                    {
                        ObjectClick.instance.CopySpotGlobal(Vector3.zero);
                    }
                }
            }
        }

        public void GoNextUI()
        {
            TicTacNetworkManager.instance.EmitUI(current_UI + 1);
            UpdateCurrentUI(current_UI + 1);
        }

        public void UpdateCurrentUI(int new_UI)
        {
            if (current_UI + 1 <= new_UI)
            {
                DebugPanel.instance.SetLogger(2, "CURRENT UI " + new_UI);
                current_UI = new_UI;
                Update_UI(new_UI);
            }
        }

        private void Update_UI(int UI)
        {
            current_UI = UI;
            CloseAllPanels();

            switch (UI)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    Display_SelectCase();
                    break;
                case 3:
                    Display_IntroPanel();
                    break;
                case 4:
                    Display_Animation();
                    break;
                case 5:
                    Display_StartTest();
                    break;
                case 6:
                    Display_InterTest();
                    break;
                case 7:
                    Display_StartTest();
                    break;
                case 8:
                    Display_InterTest();
                    break;
                case 9:
                    Display_QuestionPanel();
                    break;
                case 10:
                    Display_AfterTest();
                    break;
                default:
                    break;
            }
        }

        public void UpdateIntroPanel(int intro)
        {
            if (current_UI == 3)
            {
                Quad.SetActive(true);

                IntroPanel_.SetActive(true);
                if (current_intro == intro && intro == 0)
                {
                    IntroPanel.instance.Restart();
                }
                else if (current_intro + 1 == intro)
                {
                    IntroPanel.instance.Restart();
                    current_intro = intro;
                }
            }
        }

        private void CloseAllPanels()
        {
            AfterTestPanel_.SetActive(false);
            CasePanel_.SetActive(false);
            DebugPanel_.SetActive(false);
            InterTestPanel_.SetActive(false);
            IntroPanel_.SetActive(false);
            MainMenu.SetActive(false);
            QuestionPanel_.SetActive(false);

            ClickCircle.SetActive(false);
            EnvironmentObject.SetActive(false);
            SpotSample.SetActive(false);
            Quad.SetActive(false);
        }

        private void OpenAllPanels()
        {
            AfterTestPanel_.SetActive(true);
            CasePanel_.SetActive(true);
            InterTestPanel_.SetActive(true);
            IntroPanel_.SetActive(true);
            MainMenu.SetActive(true);
            QuestionPanel_.SetActive(true);
            DebugPanel_.SetActive(true);
            
            ClickCircle.SetActive(true);
        }

        private void Display_AfterTest()
        {
            if (user.isOperator)
            {
                AfterTestPanel_.SetActive(true);
                AfterTestPanel.instance.AskRepeatTest();
            }
            else
            {
                QuestionPanel_.SetActive(true);
            }
        }

        public void Display_Animation()
        {
            EnvironmentObject.SetActive(true);
            Animator anim = EnvironmentObject.GetComponent<Animator>();
            LeftCamera.GetComponent<CameraRestriction>().ResetRotation();
            RightCamera.GetComponent<CameraRestriction>().ResetRotation();
            if (user.isOperator)
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

        private IEnumerator Wait_Operator()
        {
            yield return new WaitForSeconds(3);
            EnvironmentObject.SetActive(false);
            GoNextUI();
        }

        private IEnumerator Wait_Subject()
        {
            yield return new WaitForSeconds(3);
            EnvironmentObject.SetActive(false);
        }

        private void Display_InterTest()
        {
            LeftCamera.GetComponent<CameraRestriction>().EnableView();
            RightCamera.GetComponent<CameraRestriction>().EnableView();

            if (user.receive_InterTest)
            {
                ObjectClick.instance.Reflect_X_Axis();
                ObjectClick.instance.DestroySpot();
            }

            InterTestPanel_.SetActive(true);
            InterTestPanel.instance.Display_Retest();
        }

        private void Display_IntroPanel()
        {
            SpotSample.SetActive(true);
            Quad.SetActive(true);

            IntroPanel_.SetActive(true);
            IntroPanel.instance.Restart();
        }

        private void Display_QuestionPanel()
        {
            LeftCamera.GetComponent<CameraRestriction>().EnableView();
            RightCamera.GetComponent<CameraRestriction>().EnableView();

            if (user.receive_InterTest)
            {
                ObjectClick.instance.Reflect_X_Axis();
                ObjectClick.instance.DestroySpot();
            }

            QuestionPanel_.SetActive(true);
            QuestionPanel.instance.Initilization();
        }

        private void Display_SelectCase()
        {
            CasePanel_.SetActive(true);
            CasePanel.instance.Display_Select_Case();
        }

        private void Display_StartTest()
        {
            try
            {
                SpotSample.SetActive(false);
                Quad.SetActive(true);
                //EnvironmentObject.SetActive(false);
                
                ClickCircle.SetActive(true);

                if (ObjectClick.instance.gameObject.activeInHierarchy)
                {
                    ObjectClick.instance.ResetTest();
                }
            }
            catch(Exception e)
            {
                DebugPanel.instance.SetLogger(3, e.ToString());
            }
        }
    }
}