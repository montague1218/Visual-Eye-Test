using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player2DManager : MonoBehaviour
{

    public string	id;

	public string name;

	public int cont;

	public string avatar;

	public bool isOnline;

	public bool isLocalPlayer;
	
	Animator anim;
	public enum statebehaviour : int {IDLE,JUMP,RUN,PUNCH,SHOOT,DAMAGE,DEAD};
	public statebehaviour currentStateBehaviour;
	public float horizontalSpeed;
	public float verticalSpeed;
	BoxCollider2D myBoxCollider2d;
	public Rigidbody2D myRigidbody;
	public bool isMove;
	public bool stop;
	public Transform footer;
	public float MaxDistanceToGo = .1f;
	public float MaxDistance = .1f;
	bool m_jump;
	public float jumpForce;
	public float jumpTime = 0.4f;
	public float jumpDelay = 0.4f;
	public bool jumped = false;
	public bool isGrounded;
	
	public float minDistanceToPlayer = 1f;
	
	public float lastVelocityX =0f;
	
	public float lastVelocityY = 0f;
	
	public Transform foot;
	
	Animator myAnim;
	
	public float punchAnimTime;
	public float shootAnimTime;
	public float jumpAnimTime;
	public bool OnCorroutine;
	
	public bool isMoving;
	
	float h ;
	float v;
	bool attack;

	public bool onMobileButton;



	
	// Use this for initialization
	void Start () {

		horizontalSpeed= 4f;
		verticalSpeed= 4f;
		anim = GetComponent<Animator> ();
		myBoxCollider2d = GetComponent<BoxCollider2D> ();
		myRigidbody = GetComponent<Rigidbody2D> ();
		myAnim = GetComponent<Animator>();
	}
	

	// Update is called once per frame
	void FixedUpdate () {

	 
        if(!onMobileButton)
		{
		    // Store the input axes.
            h = Input.GetAxisRaw("Horizontal");
              
		    v = Input.GetAxisRaw("Vertical");


		}
    
	    //Jump();
		
		if(isLocalPlayer)
		{
		 Move ();
		
		 Attack();
		 
		 if (IsMoving() || jumped) {
		   
		   UpdateStatusToServer ();
		}
		else
		{
		  
		  
		  if (currentStateBehaviour != statebehaviour.IDLE)
		  {
			ShooterNetworkManager.instance.EmitAnimation ("OnIdle");
		  }
			
		}
	   }
		else
		{
			if (myRigidbody.velocity.x != lastVelocityX) {
				lastVelocityX = myRigidbody.velocity.x;
				
			}
			
			if(myRigidbody.velocity.x == lastVelocityX && myRigidbody.velocity.y == lastVelocityY &&!jumped)
			{
			   UpdateAnimator ("OnIdle");
			}
		}
		
		
		
	}
	
	void Attack()
	{
	
	
	  if(attack|| Input.GetKey (KeyCode.Space))
	  {
	   UpdateAnimator("OnShoot");
	   ShooterNetworkManager.instance.EmitAnimation("OnShoot");
	   
	  
	  }
	 
	 
	}

	void Jump()
	{
	
	
	  if (isLocalPlayer) 
	  {

		if(!onMobileButton)
		{
          	m_jump = Input.GetButton("Jump");

		}
		
		 if(m_jump)
		 {
		   ShooterNetworkManager.instance.EmitJump();
		 }
		
	  }
	
	  
	 isGrounded = Physics2D.Linecast (this.transform.position,foot.position, 1 << LayerMask.NameToLayer ("Ground"));
	 
	  if (m_jump && isGrounded && !jumped) {
			
			myRigidbody.AddForce(transform.up * jumpForce);
			jumpTime = jumpDelay;
			UpdateAnimator("OnJump");
			jumped = true;
			
			
		}
		
		jumpTime -= Time.deltaTime;
		
		
		if (jumpTime <= 0 && isGrounded && jumped) {
			
			jumped = false;
			
		}
		
	}

	void Move()
	{
	  
		stop = false;
		isMove = false;

        Vector3 lastPosition2 = transform.position;

		if(h==0)
		{
		  UpdateAnimator("OnIdle");
		}
		if (h > 0 ) 
		{
		
			Vector3 lastPosition = transform.position;
			transform.Translate(Vector2.right *horizontalSpeed * Time.deltaTime);
			transform.eulerAngles = new Vector2(0,0);
			isMove = true;
			
			UpdateAnimator("OnRun");

		}

		if (h < 0 ) 
		{
			
            Vector3 lastPosition = transform.position;
			transform.Translate(Vector2.right *horizontalSpeed * Time.deltaTime);
			transform.eulerAngles = new Vector2(0,180);
			isMove = true;
			UpdateAnimator("OnRun");
		}

		if (v > 0 ) 
		{
		
			Vector3 lastPosition = transform.position;
		
			transform.Translate(Vector2.up *horizontalSpeed * Time.deltaTime);

			
			isMove = true;
			
			UpdateAnimator("OnRun");

		}

		if (v < 0 ) 
		{
			
            Vector3 lastPosition = transform.position;
			transform.Translate(Vector2.up *-horizontalSpeed * Time.deltaTime);

		
			isMove = true;
			UpdateAnimator("OnRun");
		}



		
		

	    GameManager.instance.minXPoint.position = new Vector3(GameManager.instance.minXPoint.position.x, transform.position.y,
		GameManager.instance.minXPoint.position.z);
		
	    var minDistanceSqr = (transform.position - GameManager.instance.minXPoint.position).sqrMagnitude;
       
		if (minDistanceSqr < MaxDistance * MaxDistance)
		{
		   transform.position = lastPosition2;
	    }
		
		GameManager.instance.maxXPoint.position = new Vector3(GameManager.instance.maxXPoint.position.x, transform.position.y,
		GameManager.instance.maxXPoint.position.z);
		
	    minDistanceSqr = (transform.position - GameManager.instance.maxXPoint.position).sqrMagnitude;
		
		if (minDistanceSqr < MaxDistance * MaxDistance)
		{
		   transform.position = lastPosition2;
	    }
		
	
		
	}//END_MOVE
	
	void UpdateStatusToServer ()
	{
		//hash table <key, value>
		Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = id;

		data["position"] = transform.position.x+";"+transform.position.y+";"+transform.position.z;
		
		data["rotation"] = transform.rotation.x+";"+transform.rotation.y+";"+transform.rotation.z+";"+transform.rotation.w;
		
		string msg = data["local_player_id"]+":"+data["position"]+":"+data["rotation"];

		ShooterNetworkManager.instance.EmitPosAndRot(msg);
		
		
		if(IsMoving()&&!jumped)
		{
		 ShooterNetworkManager.instance.EmitAnimation("OnRun");
		}
		

	}
	
	
	bool IsMoving()
	{
	
	  
	  
	  if(h!=0 || v!=0)
	  {
		 return true;
	  }
	  else
	  {
		 return false;
	  }
	 
	
	}
	
	/// <summary>
	/// method for managing player animations
	/// </summary>
	/// <param name="_animation">Animation.</param>
	public void UpdateAnimator(string _animation)
	{

		if (myAnim != null) {
			
			switch (_animation) { 


			   case "OnIdle":
				//check if the player is already in the current animation
				
				if(!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Idle")&& !OnCorroutine)
				{
				   myAnim.SetTrigger ("OnIdle");
				   
				   currentStateBehaviour = statebehaviour.IDLE;
				}
			
				break;

			    case "OnRun":
				//check if the player is already in the current animation
				 if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Run") && !OnCorroutine) {
					
					myAnim.SetTrigger ("OnRun");

					currentStateBehaviour = statebehaviour.RUN;

				 }
				break;

			    case "OnShoot":
				//check if the player is already in the current animation
				if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Shoot")&& !OnCorroutine) {
					
					 StartCoroutine ("RunShootAnim");  
					
				  }
				 break;

			     case "OnJump":
				  //check if the player is already in the current animation
				  if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Jump")&& !OnCorroutine) {
				
					  StartCoroutine ("RunJumpAnim");  
					

				  }
				 break;
	
			     case 	"OnDamage":
				   if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Damage")&& !OnCorroutine) {
					
					     myAnim.SetTrigger ("OnDamage");

					     currentStateBehaviour = statebehaviour.DAMAGE;
				   }
				  break;

			      case"IsDead":
				    if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Dead")) {
					
					   myAnim.SetTrigger ("IsDead");

					   currentStateBehaviour = statebehaviour.DEAD;
				    }
				   break;

			       }//END_SWITCH
		      }//END_IF

	}//END_UPDATE_ANIMATOR
	
	

	/// <summary>
	/// auxiliary coroutine for punch animation
	/// </summary>
	/// <returns>The punch animation.</returns>
	IEnumerator RunJumpAnim()
	{ 



		OnCorroutine = true;
		myAnim.SetTrigger ("OnJump");
		currentStateBehaviour = statebehaviour.JUMP;
		yield return new WaitForSeconds(jumpAnimTime); // wait for set reload time
		OnCorroutine = false;
		m_jump = false;
		UpdateAnimator("OnIdle");

	}
	
	/// <summary>
	/// auxiliary coroutine for punch animation
	/// </summary>
	/// <returns>The punch animation.</returns>
	IEnumerator RunShootAnim()
	{ 



		OnCorroutine = true;
		myAnim.SetTrigger ("OnShoot");
		currentStateBehaviour = statebehaviour.SHOOT;
		yield return new WaitForSeconds(shootAnimTime); // wait for set reload time
		GetComponentInChildren<Gun>().m_Shoot = true;
		OnCorroutine = false;
		UpdateAnimator("OnIdle");

	}
	

	public void UpdatePosition(Vector3 position) 
	{
	
	   transform.position = new Vector3 (position.x, position.y, position.z);
	}
	

	public void UpdateRotation(Quaternion _rotation) 
	{
		if (!isLocalPlayer) 
		{
			transform.rotation = _rotation;

		}

	}
	
	public void UpdateJump()
	{
		m_jump = true;
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
		 attack = true;
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
		 attack=  false;
		 break;
	   }
	 }
	

}
