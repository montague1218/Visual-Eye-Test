using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCue : MonoBehaviour
{
  
   public GameObject startLineTrans;
	
   public void UpdatePosition(Vector3 position) 
   {
	 transform.position = new Vector3 (position.x, position.y, position.z);
   }

	public void UpdateRotation(Quaternion _rotation) 
	{
		transform.rotation = _rotation;
	}

}
