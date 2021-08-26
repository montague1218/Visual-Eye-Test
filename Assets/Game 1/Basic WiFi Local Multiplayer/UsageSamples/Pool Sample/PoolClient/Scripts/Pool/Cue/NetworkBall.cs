using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBall : MonoBehaviour
{
  public int id;
  
  Vector3 curPos;
  
  Vector3 lastPos;
  
  public bool alreadyPlay;
  
    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }
	
    public void UpdatePosition(Vector3 position) 
	{
	  transform.position = new Vector3 (position.x, position.y, position.z);
	}

	public void UpdateRotation(Quaternion _rotation) 
	{
	  transform.rotation = _rotation;
	}
	
	public void UpdatePosAndRot(Vector3 _position,Quaternion _rot) 
	{
		transform.position = new Vector3(_position.x,transform.position.y,_position.z);
		transform.rotation = _rot;
	}
	
	void OnTriggerExit(Collider collision) {
	
	 if(collision.gameObject.tag.Equals("Ball") && id ==0)
     {
	     PoolGameManager.instance.PlayBallColisionSound();
     }	
		
	}
}
