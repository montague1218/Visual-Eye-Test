using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PingCanvasManager : MonoBehaviour {

	public static  PingCanvasManager instance;

	public Canvas alertgameDialog;

	public Text alertDialogText;

	public Text txtSearchServerStatus;

	public int currentMenu;

	public Canvas loadingImg;

	public float delay = 0f;


	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);

			instance = this;

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
