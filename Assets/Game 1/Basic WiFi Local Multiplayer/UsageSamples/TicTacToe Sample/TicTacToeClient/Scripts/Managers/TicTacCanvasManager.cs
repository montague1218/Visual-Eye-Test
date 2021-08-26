using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using QuizModule;

public class TicTacCanvasManager : MonoBehaviour
{

	public static TicTacCanvasManager instance;

	public Canvas HUDLobby;

	public Canvas gameCanvas;


	public GameObject mainMenu;

	public GameObject waitlistPanel;

	public Canvas alertgameDialog;

	public GameObject instructionGroup;

	public GameObject instructionMenu;

	public GameObject networkMenu;

	public Text instruction;


	public Button JoinServerButton;

	public Button StartServerButton;

	public Button NextButton;

	public Text alertDialogText;

	public Text txtSearchServerStatus;

	public Text txtLog;

	public Text txtHeader;

	public Text txtFooter;

	public GameObject lobbyCamera;

	public int currentMenu;

	public Canvas loadingImg;

	public AudioClip buttonSound;

	public AudioClip fallSound;

	public AudioClip victorySound;

	public VideoPlayer video_player;

	public VideoClip clip1;
	public VideoClip clip2;
	public VideoClip clip3;

	private static string instruction1_1 = "Before you start the game, one of you need to set up a Wifi hotspot: \n 1. Go to 'Setting' of your mobile devic \n 2. Share hotspot to another device \n 3. Restart the game for any technical error(optional)";
	private static string instruction1_2 = "The quiz consists of one operator and one subject.";
	private static string instruction2 = "Join the server first to be the operator. \n\nThe second player will play the role of the subject";
	private static string instruction3 = "Please start the server";

	public float delay = 0f;


	// Use this for initialization
	void Start()
	{

		if (instance == null)
		{

			DontDestroyOnLoad(this.gameObject);

			instance = this;

			OpenScreen(-1);

			CloseAlertDialog();

			CloseLoadingImg();
		}
		else
		{
			Destroy(this.gameObject);
		}

		instructionMenu.SetActive(true);
		video_player.clip = clip1;
		video_player.Play();
	}

	void Update()
	{
		delay += Time.deltaTime;

		if (video_player.clip == clip1)
		{
			if (video_player.time > 15.5f)
			{
				if (video_player.time < 17.5f)
				{
					instruction.text = instruction1_2;
					NextButton.interactable = false;
				}
				else
				{
					NextButton.interactable = true;
				}
			}
			else
			{
				instruction.text = instruction1_1;
				NextButton.interactable = false;
			}
		}
		else if (video_player.clip == clip2)
		{
			if (video_player.time >= 5.5f)
			{
				NextButton.interactable = true;
			}
		}
		else if (video_player.clip == clip3)
		{
			if (video_player.time >= 1f)
            {
				StartServerButton.interactable = true;
				JoinServerButton.interactable = true;
            }
            else
            {
				StartServerButton.interactable = false;
				JoinServerButton.interactable = false;
            }
		}
	}

	public void SkipInstruction()
    {
		instructionMenu.SetActive(false);
		networkMenu.SetActive(true);

		video_player.clip = clip3;
		video_player.Play();
	}

	public void OpenScreen(int _current)
	{
		currentMenu = _current;
		switch (_current)
		{
			case -1:
				instructionGroup.SetActive(true);
				mainMenu.SetActive(true);
				HUDLobby.enabled = false;
				gameCanvas.enabled = false;
				break;

			//lobby menu
			case 0:
				HUDLobby.enabled = true;
				gameCanvas.enabled = false;
				break;

			case 1:
				HUDLobby.enabled = false;
				break;


		}
	}

	public void ShowNextInstruction()
	{
		video_player.Stop();
		if (instruction.text == instruction1_2)
		{
			NextButton.interactable = false;
			instruction.text = instruction2;
			video_player.clip = clip2;
			video_player.Play();
		}
		else if (instruction.text == instruction2)
		{
			instructionMenu.SetActive(false);
			networkMenu.SetActive(true);

			video_player.clip = clip3;
			video_player.Play();
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

	public void PlayAudio(AudioClip _audioclip)
	{
		GetComponent<AudioSource>().PlayOneShot(_audioclip);
	}
}
