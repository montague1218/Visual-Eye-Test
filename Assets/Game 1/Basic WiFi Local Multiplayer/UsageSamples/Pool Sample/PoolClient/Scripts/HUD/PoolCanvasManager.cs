using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolCanvasManager : MonoBehaviour {

	public static PoolCanvasManager instance;

	public Canvas  HUDLobby;
	
	public Canvas  gameCanvas;
	
	public Canvas  howToPlayPanel;
	
	public Slot[] localPlayerUISlotBalls;
	
	public Slot[] networkPlayerUISlotBalls;

	public Canvas alertgameDialog;

	public Text alertDialogText;

	public Text txtSearchServerStatus;
	
	public Text messageText;
	
	public Text txtLog;
	
	public Text txtHeader;
	
	public Text txtFooter;
	
	public Text txtScore;
	
	public Slider powerSlider;
	
	public InputField localPlayerIF;
	
	public InputField networkPlayerIF;

	
	public Text txtOpponentScore;

	public GameObject lobbyCamera;

	public int currentMenu;

	public Canvas loadingImg;

	public  AudioClip buttonSound;

	public AudioClip fallSound;

	public AudioClip victorySound;


	public float delay = 0f;

	
	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);

			instance = this;
			
			localPlayerIF.text = "You";
			networkPlayerIF.text = "Opponent";

			OpenScreen(0);

			CloseAlertDialog ();

			CloseLoadingImg();
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
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 2:
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 3:
			 Application.Quit ();
			break;

		 }//END_SWITCH

	 }//END_IF
}
	/// <summary>
	/// Opens the current HUD.
	/// </summary>
	/// <param name="_current">Current.</param>
	public void  OpenScreen(int _current)
	{
		switch (_current)
		{
		    //lobby menu
		    case 0:
			Screen.orientation = ScreenOrientation.Landscape;
			currentMenu = _current;
			HUDLobby.enabled = true;
			gameCanvas.enabled = false;
			howToPlayPanel.enabled = false;
			break;


		    case 1:
			Screen.orientation = ScreenOrientation.Landscape;
			currentMenu = _current;
			HUDLobby.enabled = false;
			gameCanvas.enabled = true;
			howToPlayPanel.enabled = false;
		
			break;

	
		}

	}

	public void OpenHowToPlayPanel()
	{
	  howToPlayPanel.enabled = true;
	}
	
	public void closeHowToPlayPanel()
	{
	  howToPlayPanel.enabled = false;
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
	
		/// <summary>
	/// Shows the alert dialog.Debug.Log
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowMessage(string _message)
	{
		messageText.text = _message;
		messageText.enabled = true;
		StartCoroutine (CloseMessage() );
	}
	
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>

	IEnumerator CloseMessage() 
	{

		yield return new WaitForSeconds(4);
		messageText.text = "";
		messageText.enabled = false;
	} 




	public void PlayAudio(AudioClip _audioclip)
	{
		
	   GetComponent<AudioSource> ().PlayOneShot (_audioclip);

	}
	
	public void SetUIBallsSlots(string _ballsTipe)
	{
	  if(_ballsTipe.Equals("solid"))
	  {
	    int i = 0;
	    foreach(Slot s in localPlayerUISlotBalls)
		{
		   s.SetBall(i);
		   i+=1;
		}
		i = 8;
		foreach(Slot s in networkPlayerUISlotBalls)
		{
		   s.SetBall(i);
		    i+=1;
		}
	  }
	  else
	  {
	    int i = 8;
	    foreach(Slot s in localPlayerUISlotBalls)
		{
		   s.SetBall(i);
		    i+=1;
		}
		i = 0;
		foreach(Slot s in networkPlayerUISlotBalls)
		{
		   s.SetBall(i);
		   i+=1;
		}
	  }
	  
	}
	public void HideUIBallSlot(int _ball_id)
	{
	
	  foreach(Slot s in localPlayerUISlotBalls)
		{
		   if(s.ball_id.Equals(_ball_id))
		   {
		     s.ClearSlot();
		   }
		}
		foreach(Slot s in networkPlayerUISlotBalls)
		{
		   if(s.ball_id.Equals(_ball_id))
		   {
		     s.ClearSlot();
		   }
		}
	}
}
