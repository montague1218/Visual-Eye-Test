using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{


	public bool isLocalPlayer;

	public string	id;

	public string name;

	public bool isOnline;


	void Update()
	{

		if(isLocalPlayer)
		{
			var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
			var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

			transform.Rotate(0, x, 0);
			transform.Translate(0, 0, z);

			if(x!=0|| z!=0)
			{
				UpdateStatusToServer();
			}


		}
	
	}


	void UpdateStatusToServer ()
	{
	
		BasicNetworkManager.instance.EmitPosAndRot(transform.position,transform.rotation.y.ToString() );

	}



	public void UpdatePosition(Vector3 position) 
	{
	
		transform.position = new Vector3 (position.x, position.y, position.z);

	}

	public void UpdateRotation(Quaternion _rotation) 
	{
		
	   transform.rotation = _rotation;
			
	}

}
