using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game3Module
{
    public class Game3Manager : MonoBehaviour
    {
        public static Game3Manager instance { get; private set; }

        public GameObject mainCamera_parent;
        public GameObject mainCamera;
        public GameObject env;
        public bool rotateX, rotateY;
        public bool stop;
        public GameObject rod;
        public GameObject rod_test; //DEBUG

        public GameObject background_rotational; // Rotate with camera
        public GameObject background_static; // Do not rotate with camera
        public GameObject startPanel;

        public List<GameObject> CN_Buttons = new List<GameObject>();

        private List<bool> CN_active = new List<bool> { true, true, true };
        private List<string> CN_names = new List<string> { "CN III", "CN IV", "CN VI" };

        // According to user requirements, each direction takes 3 seconds
        private const float ROD_SPEED = 30f;

        // According to user requirements, rod waits 1 second for changing direction
        private const float ROD_WAIT_TIME = 1f;

        // Horizontal and vertical boundaries for the rod's movement from the orgin
        private const float ROD_BOUND_X = 140f;
        private const float ROD_BOUND_Y = 100f;

        // Maximum rotation along the axis X or Y
        private const float CN_III_X_MAX = 10f;
        private const float Y_MAX = 38f;

        private bool testing = false;
        public bool isRightEye = true;

        // In each direction, the rod moves this distance far
        private float unit_remain = ROD_BOUND_X;
        private float wait_remain = ROD_WAIT_TIME;

        // Index on direction_sequence
        private int direction_index = 0;


        private List<string> direction_left = new List<string>
        {
            "LEFT", "UP", "DOWN", "DOWN", "UP", "RIGHT"
        };

        private List<string> direction_right = new List<string>
        {
            "RIGHT", "UP", "DOWN", "DOWN", "UP", "LEFT"
        };

        private List<string> direction_sequence;

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
            CloseTest();
        }

        // Update is called once per frame
        void Update()
        {
            if (testing)
            {
                UpdateRodMovement();
                if (direction_index < direction_sequence.Count)
                    RestrictCameraRotation();
            }
            

            if (Input.GetKey(KeyCode.A) && testing)
            {
                rotateX = !rotateX;
                rotateY = !rotateY;
            }
        }

        public void StartTest()
        {
            testing = true;
            rod.SetActive(true);
            background_static.SetActive(true);
            startPanel.SetActive(false);

            direction_index = 0;
            direction_sequence = isRightEye ? direction_right : direction_left;
            unit_remain = GetDistanceByDirection(direction_sequence[direction_index]);
            wait_remain = 0f;
            //env.SetActive(true);
        }

        public void CloseTest()
        {
            testing = false;
            rod.SetActive(false);
            background_static.SetActive(false);
            background_rotational.SetActive(false);
            startPanel.SetActive(false);
            
            env.SetActive(false);

            DoctorPanel.instance.Reset();
        }

        // Triggered by Button's OnClick()
        public void SetActive_CN(int cn)
        {
            CN_active[cn] = !CN_active[cn];

            string cn_name = CN_names[cn];
            string status = CN_active[cn] ? "On" : "Off";
            CN_Buttons[cn].GetComponentInChildren<Text>().text = cn_name + " : " + status;
        }

        private void UpdateRodMovement()
        {
            if (direction_index < direction_sequence.Count)
            {
                if (wait_remain <= 0f)
                {
                    TranslateRod();
                    if (unit_remain <= 0f)
                    {
                        NextDirection();
                        AdjustRodStopPosition();
                    }
                }
                else
                {
                    wait_remain -= Time.deltaTime;
                }
            }
            else
            {
                CloseTest();
            }
        }

        private void TranslateRod()
        {
            Vector3 v = GetRodVelocity();
            rod.transform.position += v;
            unit_remain -= v.magnitude;
        }

        private void NextDirection()
        {
            direction_index++;
            wait_remain = ROD_WAIT_TIME;

            if (direction_index < direction_sequence.Count)
                unit_remain = GetDistanceByDirection(GetDirection());
            else
                CloseTest();
        }

        // Make the rod align with one of the X/Y axis
        private void AdjustRodStopPosition()
        {
            Vector3 pos = rod.transform.position;
            float x1 = Mathf.Round(pos.x);
            float y1 = Mathf.Round(pos.y);
            rod.transform.position = new Vector3(x1, y1, pos.z);
        }

        private string GetDirection() => direction_sequence[direction_index];

        private string GetPreviousDirection()
        {
            return direction_index - 1 >= 0 ? direction_sequence[direction_index - 1] : null;
        }

        private bool IsOnUpperPlane()
        {
            string previous = GetPreviousDirection();
            string current = GetDirection();

            if (previous == "LEFT" || previous == "RIGHT")
                if (current == "UP")
                    return true;
            if (previous == "UP" && current == "DOWN")
                return true;
            return false;
        }

        private float GetDistanceByDirection(string direction)
        {
            switch (direction)
            {
                case "UP":
                    if (!CN_active[1])
                        return IsOnUpperPlane() ? 125f : 75f;
                    return 100f;
                case "DOWN":
                    if (!CN_active[1])
                        return IsOnUpperPlane() ? 125f : 75f;
                    return 100f;
                case "LEFT":
                    return ROD_BOUND_X;
                case "RIGHT":
                    return ROD_BOUND_X;
                default:
                    return 0f;
            }
        }

        private Vector3 GetRodVelocity()
        {
            float dt = Time.deltaTime;
            float dx = 0f;
            float dy = 0f;

            switch (GetDirection())
            {
                case "UP":
                    dy = ROD_SPEED * dt;
                    break;
                case "DOWN":
                    dy = -ROD_SPEED * dt;
                    break;
                case "LEFT":
                    dx = -ROD_SPEED * dt;
                    break;
                case "RIGHT":
                    dx = ROD_SPEED * dt;
                    break;
            }

            // The rod only has linear velocity parallel with an axis
            return new Vector3(dx, dy, 0);
        }

        private void RestrictCameraRotation()
        {
            // CN III
            if (!CN_active[0])
            {
                background_static.SetActive(true);
                background_rotational.SetActive(false);
            }


            if (isRightEye)
            {
                if (!CN_active[0])
                {
                    switch (GetDirection())
                    {
                        case "UP":
                            ClampCameraRotation_CN_III();
                            break;
                        case "DOWN":
                            ClampCameraRotation_CN_III();
                            break;
                        case "LEFT":
                            SetCameraRotation(true, false);
                            break;
                        case "RIGHT":
                            SetCameraRotation(true, false);
                            break;
                    }
                }
                else if (!CN_active[1])
                {
                    switch (GetDirection())
                    {
                        case "UP":
                            SetCameraRotation(false, true, Y_MAX);
                            break;
                        case "DOWN":
                            SetCameraRotation(false, true, Y_MAX);
                            break;
                        case "LEFT":
                            SetCameraRotation(true, false);
                            break;
                        case "RIGHT":
                            SetCameraRotation(true, false);
                            break;
                    }
                }
                else if (!CN_active[2])
                {
                    switch (GetDirection())
                    {
                        case "UP":
                            SetCameraRotation(false, true, 0f);
                            break;
                        case "DOWN":
                            SetCameraRotation(false, true, 0f);
                            break;
                        case "LEFT":
                            SetCameraRotation(false, true, 0f);
                            break;
                        case "RIGHT":
                            SetCameraRotation(false, true, 0f);
                            break;
                    }
                }
            }
            else
            {
                switch (GetDirection())
                {
                    case "UP":
                        SetCameraRotation(false, true, -Y_MAX);
                        break;
                    case "DOWN":
                        SetCameraRotation(false, true, -Y_MAX);
                        break;
                    case "LEFT":
                        SetCameraRotation(true, false);
                        break;
                    case "RIGHT":
                        SetCameraRotation(true, false);
                        break;
                }
            }
        }

        // Preserve or disallow rotation in the direction of x/y-axis
        private void SetCameraRotation(bool preserveX, bool preserveY, float angle_y)
        {
            if (preserveX)
            {
                mainCamera.GetComponent<CameraRotation>().PreserveRotation_X();
            }

            if (preserveY)
            {
                mainCamera_parent.GetComponent<CameraRotation>().PreserveRotation_Y(angle_y);
            }
        }

        private void SetCameraRotation(bool preserveX, bool preserveY)
        {
            SetCameraRotation(preserveX, preserveY, 0f);
        }

        private void ClampCameraRotation_CN_III()
        {
            mainCamera_parent.GetComponent<CameraRotation>()
                .ClampRotation_Y(mainCamera.transform, CN_III_X_MAX, Y_MAX);
        }
    }
}
