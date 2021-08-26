using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
///Manage Network player if isLocalPlayer variable is false
/// or Local player if isLocalPlayer variable is true.
/// </summary>
public class PlayerManager : MonoBehaviour {

public string	id;

	public string name;

	public int cont;

	public string avatar;

	public bool isOnline;

	public bool isLocalPlayer;

	Animator myAnim;

	public Rigidbody myRigidbody;

	public enum state : int {idle,walk,attack,damage,dead};

	public state currentState;

	//distances low to arrive close to the player
	[Range(1f, 200f)][SerializeField] float minDistanceToPlayer = 10f ;

	public float verticalSpeed = 3.0f;

	public float rotateSpeed = 150f;

	float m_GroundCheckDistance = 1f;

	public float jumpPower = 12f;

	public float jumpTime=0.4f;

	public float jumpdelay=0.4f;

	public bool m_jump;

	public bool isJumping;

	public bool onGrounded;

	public bool isAttack;

	public float attackTime;

	public float timeOut;

	public Transform cameraTotarget;

	float h ;
	
	float v;
	
	bool attack;

	public bool onMobileButton;

	// Use this for initialization
	void Start () {

		myAnim = GetComponent<Animator>();
		myRigidbody = GetComponent<Rigidbody> ();
		
	}

	void Awake()
	{
		myAnim = GetComponent<Animator>();
		myRigidbody = GetComponent<Rigidbody> ();
		
	}

	public void Set3DName(string name)
	{
		GetComponentInChildren<TextMesh> ().text = name;

	}

	// Update is called once per frame
	void FixedUpdate () {
		

		if (isLocalPlayer)
		{
			Attack ();
			Move();
		}
	
		Jump ();

	}

	void Move( )
	{

        if(!onMobileButton)
		{
		    // Store the input axes.
            h = Input.GetAxisRaw("Horizontal");
              
		    v = Input.GetAxisRaw("Vertical");


		}
		

		var x = h* Time.deltaTime *  verticalSpeed;
		var y = h * Time.deltaTime * rotateSpeed;
		var z = v * Time.deltaTime * verticalSpeed;



		transform.Rotate (0, y, 0);

		transform.Translate (0, 0, z);

		if (h != 0 || v != 0 || isJumping ) {
			currentState = state.walk;
			UpdateAnimator ("IsWalk");
			UpdateStatusToServer ();
		}
		else
		{
			if (currentState != state.idle)
			{
				NetworkManager.instance.EmitAnimation ("IsIdle");
			}
			currentState = state.idle;
			UpdateAnimator ("IsIdle");
		}
	
	

	}

	public void Jump()
	{
	    
		 
		RaycastHit hitInfo;

		onGrounded = Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance);

		jumpTime -= Time.deltaTime;



		if (isLocalPlayer) 
		{
		   
			if(!onMobileButton)
		    {
               m_jump = Input.GetButton("Jump");

		    }
		}
	


		if (jumpTime <= 0 && isJumping && onGrounded)
		{

			m_jump = false;
			isJumping = false;
		}



		if (m_jump && !isJumping) 
		{	


			myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);

			jumpTime = jumpdelay;

			isJumping = true;

		}
		
	
	}

	void Attack()
	{
	   

			if (isAttack || Input.GetKey (KeyCode.Space))
			{
			
				currentState = state.attack;
				UpdateAnimator ("isAttack");
				string msg = id;
				NetworkManager.instance.EmitAttack("ATTACK",msg);

				foreach(KeyValuePair<string, PlayerManager> enemy in NetworkManager.instance.networkPlayers)
				{

					if ( enemy.Key != id)
					{
					
						Vector3 meToEnemy = transform.position - enemy.Value.transform.position;
						//Debug.Log ("meToEnemy.sqrMagnitude: "+meToEnemy.sqrMagnitude);
						
					    //if i am close to any network player
						if (meToEnemy.sqrMagnitude < minDistanceToPlayer)
						{

						    //damage network player
							NetworkManager.instance.EmitPhisicstDamage (id, enemy.Key);
						}
					}
				}
			}

		
	}


	void UpdateStatusToServer ()
	{
		NetworkManager.instance.EmitPosAndRot(transform.position,transform.rotation.y.ToString());
	}


	public void UpdatePosition(Vector3 position) 
	{
		if (!isLocalPlayer) {

			if (!isJumping)
			{
				transform.position = new Vector3 (position.x, position.y, position.z);

				UpdateAnimator ("IsWalk");
			}
		}

	}

	public void UpdateRotation(Quaternion _rotation) 
	{
		if (!isLocalPlayer) 
		{
			transform.rotation = _rotation;
			UpdateAnimator ("IsWalk");

		}

	}



	public void UpdateAnimator(string _animation)
	{

	
			switch (_animation) { 
			case "IsWalk":
				if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Walk"))
				{
					myAnim.SetTrigger ("IsWalk");


				}
				break;

			case "IsIdle":

				if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				{
					myAnim.SetTrigger ("IsIdle");

				}
				break;

			case "IsDamage":
				if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Damage") ) 
				{
					myAnim.SetTrigger ("IsDamage");
				}
				break;

			case "isAttack":
				if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Atack"))
				{
					myAnim.SetTrigger ("isAttack");

				if (!isLocalPlayer)
				{
			
					StartCoroutine ("StopAttack");
				}
				}
				break;


			case "IsDead":
				if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
				{
					myAnim.SetTrigger ("IsDead");
				}
				break;

			}
	
	}



	public void UpdateJump()
	{
		m_jump = true;
	}

	// reload your weapon
	IEnumerator StopAttack()
	{
		if (isAttack)
		{
			yield break; // if already attack... exit and wait attack is finished
		}

		isAttack = true; // we are now attack

	
		yield return new WaitForSeconds(attackTime); // wait for set attack animation time
		isAttack = false;


	}

	public void EnableKey(string _key)
	 {
	 
	   onMobileButton = true;
	  
	   switch(_key)
	   {
	   
	     case "up":
		 v = 1;
		 break;
		 case "down":
		 v= -1;
		 break;
		 case "right":
		 h = 1;
		 break;
		 case "left":
		 h = -1;
		 break;
		case "jump":
		 m_jump = true;
		 break;
		 case "attack":
		 isAttack = true;
		 break;
	   }
	 }
	 
	 public void DisableKey(string _key)
	 {
	   onMobileButton = false;
	   switch(_key)
	   {
	    case "up":
		 v = 0;
		 break;
		 case "down":
		 v= 0;
		 break;
		 case "right":
		 h = 0;
		 break;
		 case "left":
		 h = 0;
		 break;
		 case "jump":
		 m_jump =  false;
		 break;
		  case "attack":
		 isAttack= true;
		 break;
	   }
	 }


}
