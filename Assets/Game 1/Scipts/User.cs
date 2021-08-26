using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizModule
{
    /// <summary>
    /// Handle the acceptance of network behaviours for Operator and Player
    /// </summary>
    public class User
    {
        public bool isOperator;

        public readonly bool emit_Intro;
        public readonly bool emit_InterTest;
        public readonly bool emit_RepeatTest;
        public readonly bool emit_StartTest;
        public readonly bool emit_StartGame;
        public readonly bool emit_Spot;

        public readonly bool receive_UI;
        public readonly bool receive_Answer;
        public readonly bool receive_InterTest;
        public readonly bool receive_Intro;
        public readonly bool receive_JoinGame;
        public readonly bool receive_StartGame;
        public readonly bool receive_Retest;
        public readonly bool receive_Select_Case;
        public readonly bool receive_Spot;

        public static User Subject = new User(false);
        public static User Operator = new User(true);


        public User(bool isOperator_)
        {
            isOperator = isOperator_;
            if (isOperator)
            { 
                emit_Intro = true;
                emit_InterTest = true;
                emit_RepeatTest = true;
                emit_StartTest = true;
                emit_StartGame = false;
                emit_Spot = false;
                
                receive_UI = true;
                receive_Answer = false;
                receive_InterTest = true;
                receive_Intro = false;
                receive_JoinGame = true;
                receive_StartGame = true;
                receive_Retest = false;
                receive_Select_Case = false;
                receive_Spot = true;
            }
            else
            {
                emit_Intro = false;
                emit_InterTest = false;
                emit_RepeatTest = false;
                emit_StartTest = false;
                emit_StartGame = true;
                emit_Spot = true;

                receive_UI = true;
                receive_Answer = true;
                receive_InterTest = false;
                receive_Intro = true;
                receive_JoinGame = false;
                receive_Retest = true;
                receive_StartGame = false;
                receive_Select_Case = true;
                receive_Spot = false;
            }
        }

        
    }
}