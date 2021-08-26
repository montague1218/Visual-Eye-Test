using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// script to manage the HUD containing the ball slots of the local player and the opposing player.
/// </summary>
/// <param name="_weapon">Weapon.</param>

public class Slot : MonoBehaviour {

	public int ball_id;
	public bool isFree;
	public Image ballImage;
	
	public GameObject[] ballsSprite;
	
	public GameObject EmptySprite;
	
	
	void Awake()
	{
	  ballImage.enabled = false;
	}
	/// <summary>
	/// Sets UI ball.
	/// </summary>
	public void SetBall(int _ball_id)
	{
		isFree = false;
		ballImage.enabled = true;
		ball_id = _ball_id+1;
		ballImage.sprite = ballsSprite[_ball_id].GetComponent<SpriteRenderer> ().sprite;
		
	}

	
	/// <summary>
	/// method called when the player pockets a ball to update the ball HUD.
	/// </summary>
	/// 
	public void ClearSlot()
	{
		isFree = true;
		ball_id = -1;
		ballImage.enabled = false;
		
	}
}
