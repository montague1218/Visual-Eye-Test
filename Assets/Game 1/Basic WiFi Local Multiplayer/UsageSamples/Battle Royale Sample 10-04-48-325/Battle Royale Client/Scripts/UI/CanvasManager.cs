using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

	public static  CanvasManager instance;

	public Canvas pLobby;

	public Canvas mainMenu;

	public Canvas uiNewGame;

	public Canvas uijoin;

	public Canvas gameCanvas;

	public InputField inputLogin;

	public InputField inputHostName;

	public Canvas alertgameDialog;

	public Text alertDialogText;

	public Text txtSearchServerStatus;

	public GameObject lobbyCamera;

	public int currentMenu;

	public Canvas loadingImg;

	public Slider healthSlider;

	public Text txtHealth;

	public float delay = 0f;

	public Canvas mobileButtons;



	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);

			instance = this;

			OpenScreen(0);

			CloseAlertDialog ();
		}
		else
		{
			Destroy(this.gameObject);
		}



	}

	void Update()
	{
		delay += Time.deltaTime;

		if (Input.GetKey ("escape") && delay > 1f) {

		  switch (currentMenu) {

			case 0:
			 Application.Quit ();
			break;

			case 1:
			 OpenScreen (0);
			 delay = 0f;
			break;
			
			case 2:
			 OpenScreen (0);
			 delay = 0f;
			break;
			
			case 3:
			 Application.Quit ();
			break;

		 }//END_SWITCH

	 }//END_IF
}
	/// <summary>
	/// Opens the screen.
	/// </summary>
	/// <param name="_current">Current.</param>
	public void  OpenScreen(int _current)
	{
		switch (_current)
		{
		    //lobby menu
		    case 0:
			currentMenu = _current;
			pLobby.enabled = true;
			mainMenu.enabled = true;
			uiNewGame.enabled = false;
			uijoin.enabled = false;	
			gameCanvas.enabled = false;
			lobbyCamera.GetComponent<Camera> ().enabled = true;
			break;


		    case 1:
			currentMenu = _current;
			uiNewGame.enabled = true;
			mainMenu.enabled = false;
			uijoin.enabled = false;
			gameCanvas.enabled = false;
			lobbyCamera.GetComponent<Camera> ().enabled = true;
			break;

		    case 2:
			currentMenu = _current;
			uijoin.enabled = true;
			uiNewGame.enabled = false;
			mainMenu.enabled = false;
			gameCanvas.enabled = false;
			lobbyCamera.GetComponent<Camera> ().enabled = true;
			break;
	
			//no lobby menu
		case 3:
			currentMenu = _current;
			pLobby.enabled = false;
			gameCanvas.enabled = true;
			
			#if UNITY_ANDROID 

				mobileButtons.enabled = true;	

			
					
			#else
			    mobileButtons.enabled = false;	

			#endif

			break;

		}

	}


	/// <summary>
	/// Shows the alert dialog.
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowAlertDialog(string _message)
	{
		alertDialogText.text = _message;
		alertgameDialog.enabled = true;
	}

	public void ShowLoadingImg()
	{
		loadingImg.enabled = true;


	}
	public void CloseLoadingImg()
	{
		loadingImg.enabled = false;

	}

	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	public void CloseAlertDialog()
	{
		alertgameDialog.enabled = false;
	}
}
