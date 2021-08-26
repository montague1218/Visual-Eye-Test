using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizModule
{
    /// <summary>
    /// Handle behaviour for debugging
    /// Option: Skip eye-test, end eye-test, change user role (Player/Operator), display network log
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        public static DebugPanel instance { get; private set; }

        public Text topLogText;

        public Text logText;

        private List<string> loggers = new List<string> { "A", "B", "C", "D" };
        
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
            ClearAllLog();   
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void SetTopLogger(string log)
        {
            topLogText.text = log;
        }

        public void SetLogger(int i, string log)
        {
            loggers[i] = log;
            logText.text = string.Format("-- Debug Logger -- \n --{0}-- \n --{1}-- \n --{2}-- \n --{3}-- \n",
                loggers[0], loggers[1], loggers[2], loggers[3]);
        }

        private void ClearAllLog()
        {
            SetLogger(0, "A");
            SetLogger(1, "B");
            SetLogger(2, "C");
            SetLogger(3, "D");
        }
    }
}