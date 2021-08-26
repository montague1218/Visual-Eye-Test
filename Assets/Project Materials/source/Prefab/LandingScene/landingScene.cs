using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class landingScene : MonoBehaviour {
	public GameObject Background;
	public GameObject CU_Logo;

    public float landingTime = 3.0f;

	// Use this for initialization
	void Start () {
        StartCoroutine(loadLevelLate());
	}
	
	
	IEnumerator loadLevelLate() {
        yield return new WaitForSeconds(landingTime);
		Background.SetActive(false);
		CU_Logo.SetActive(false);
        //SceneManager.LoadScene("1_StartScene");
	}
}
