
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace QuizModule
{
    /// <summary>
    /// Handle behaviour of eye-test
    /// </summary>
    public class ObjectClick : MonoBehaviour
    {
        public static ObjectClick instance;

        // Texts for debug
        public Text TimerText;
        public Text StatusText;
        public Text SequenceText;

        public AudioClip clickSound;
        public AudioClip beepCorrect;
        public AudioClip beepError;

        public GameObject CirclePrefab;
        public GameObject _instance;
        public GameObject previousSpot; // Used by Operator only

        private bool reflect_x = false; // when false, test left eye; when true, test right eye
        public int defaultPattern = 0;

        public Vector3 center;
        public Vector3 size;

        private float timer = 0f;
        private bool spawned = false;

        private int totalSpotCount = 0; // Count the number of spots created
        private int spotCount = 1; // Count the spot created for each quadrant, from 1 to 5
        private int quadCount = 1; // Order of quadrants in anti-clockwise
        private int error = 0;

        private const float MAX_CLICK_TIME = 4f;
        private const float MIN_CLICK_TIME = 1f;

        private List<int> quadTestOrder = new List<int> { 0, 1, 2, 3 };
        private List<bool> quadVisible = new List<bool> { true, true, true, true }; // when false, spots will be invisible located in that quadrant



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

        private void Update()
        {
            

            timer += Time.deltaTime;
            //UpdateLog();

            if (QuizManager.instance.user.isOperator) return;

            // End eye test 3 seconds later after 4 quadrants have been tested
            if (quadCount > 4 && !spawned && timer > 3f)
            {
                Reflect_X_Axis();
                DestroySpot();
                QuizManager.instance.GoNextUI();
                return;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                    Clicked();

                if (timer >= MIN_CLICK_TIME && !spawned)
                {
                    if (quadCount <= 4)
                    {
                        Vector3 vec = GetSpawnSpot();
                        SpotIncrement();
                        if (QuizManager.instance.user.emit_Spot)
                            QuizManager.instance.EmitSpotLocation(vec, true);
                    }
                }

                if (timer >= MAX_CLICK_TIME && spawned)
                    OverTimeClick();
            }
        }

        public void DestroySpot()
        {
            Destroy(_instance);
        }

        public void Reflect_X_Axis()
        {
            reflect_x = !reflect_x;
        }

        public void ResetTest()
        {
            timer = 0f;
            spawned = false;

            totalSpotCount = 0;
            spotCount = 1;
            quadCount = 1;
            error = 0;

            quadTestOrder = new List<int> { 0, 1, 2, 3 };
            quadVisible = new List<bool> { true, true, true, true };

            SetVisibleQuads();
            Shuffle(quadTestOrder);
            DestroySpot();

            if (reflect_x)
            {
                QuizManager.instance.LeftCamera.GetComponent<CameraRestriction>().DisableView();
                QuizManager.instance.RightCamera.GetComponent<CameraRestriction>().EnableView();
            }
            else
            {
                QuizManager.instance.LeftCamera.GetComponent<CameraRestriction>().EnableView();
                QuizManager.instance.RightCamera.GetComponent<CameraRestriction>().DisableView();
            }
        }

        public void Clicked()
        {


            // Clicking before the spot is spawned
            if (timer < MIN_CLICK_TIME)
            {
                Debug.Log("Clicked too fast");
                OverTimeClick();
            }
            else if (timer <= MAX_CLICK_TIME && spawned)
            {
                Debug.Log("Clicked on time");
                AudioPlayer.instance.PlayAudio(clickSound);
                DestroySpot();

                spawned = false;
                timer = 0f;
            }
        }

        private void OverTimeClick()
        {
            DestroySpot();
            timer = 0f;
            spawned = false;

            if (quadCount <=                                                                                                                                             quadTestOrder.Count)
            {
                AudioPlayer.instance.PlayAudio(beepError);
                int quad = quadTestOrder[quadCount - 1];
                if (quadVisible[quad])
                {
                    
                    error++;
                    switch (error)
                    {
                        case 1:
                            Debug.Log("A Chance is given.");
                            break;
                        case 2:
                            // Retest the quadrant with one chance
                            Debug.Log("Quadrant is retested.");
                            spotCount = 0;
                            break;
                        default:
                            Debug.Log("Quadrant is skipped");
                            spotCount = 0;
                            quadCount++;
                            error = 0;
                            break;
                    }
                }
                else
                {
                    if (spotCount == 5)
                    {
                        spotCount = 0;
                        quadCount++;
                    }
                    else
                    {
                        SpotIncrement();
                    }
                }
            }
        }

        private Vector3 GetSpawnSpot()
        {
            spawned = true;

            int quad = quadTestOrder[quadCount - 1];
            if (quadVisible[quad])
            {
                Vector3 pos = QuadrantVector(quad);

                // Create a light spot on the scene
                _instance = Instantiate(CirclePrefab, pos, CirclePrefab.transform.rotation);
                _instance.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);

                return pos;
            }
            else
            {
                DestroySpot();
                return Vector3.zero;
            }
        }

        public void CopySpotGlobal(Vector3 vec)
        {
            Destroy(_instance);

            Vector3 pos = vec;

            // Create a light spot on the scene
            _instance = Instantiate(CirclePrefab, pos, CirclePrefab.transform.rotation);
            _instance.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);

            if (previousSpot != null)
            {
                previousSpot.SetActive(false);
                previousSpot = _instance;
            }
        }

        public void SetEyePattern(int i)
        {
            defaultPattern = i;
        }

        private void SpotIncrement()
        {
            AudioPlayer.instance.PlayAudio(beepCorrect);

            spotCount++;
            if (spotCount > 5)
            {
                spotCount = 1;
                quadCount++;
            }
        }

        /// <summary>
        /// Randomly generate a vector inside the quadrant q
        /// </summary>
        private Vector3 QuadrantVector(int q)
        {
            /*
             * Pick (x, y) from a ring inside a cirlce
             * as 3D projection casues spots near the boundary appears to be out of the circle.
             */
            float x_random = Random.Range(0.05f * size.x, 0.95f * size.x);
            float y_max = Mathf.Sqrt(1f - x_random * x_random);
            float y_random = Random.Range(0, size.y);

            float z = -90f;

            if (reflect_x)
                x_random = -x_random;


            switch (q)
            {
                case 0:
                    return new Vector3(x_random, y_random, z);
                case 1:
                    return new Vector3(-x_random, y_random, z);
                case 2:
                    return new Vector3(-x_random, -y_random, z);
                case 3:
                    return new Vector3(x_random, -y_random, z);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Exclude particular quadrants to be tested to simulate visual defects.
        /// </summary>
        private void SetVisibleQuads()
        {
            // 0: Normal visual field.​ 
            // 1: Bitemporal hemianopia.​ (Q2, Q3 blind)
            // 2: Homonymous hemianopia.​ (Q1,Q4 blind)
            // 3: Superior quadrantanopia. (Q1 blind)
            // 4: Inferior quadrantanopia.​ (Q4 blind)
            switch (defaultPattern)
            {
                case 0:
                    break;
                case 1:
                    quadVisible[1] = false;
                    quadVisible[2] = false;
                    break;
                case 2:
                    quadVisible[0] = false;
                    quadVisible[3] = false;
                    break;
                case 3:
                    quadVisible[1] = false;
                    break;
                case 4:
                    quadVisible[3] = false;
                    break;
            }
        }

        private void Shuffle(List<int> L)
        {
            int count = L.Count;
            int last = count - 1;
            for (int i = 0; i < last; ++i)
            {
                int r = Random.Range(i, count);

                int temp = L[i];
                L[i] = L[r];
                L[r] = temp;
            }
        }
    }
}