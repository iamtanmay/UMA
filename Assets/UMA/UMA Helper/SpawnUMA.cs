using UnityEngine;
using System.Collections;

public class SpawnUMA : MonoBehaviour {

	public string avatarname;
	public bool debugmode = false;
	public bool loadonstart = false;
	public GameObject myUma;
	public bool initonce = true;
	public MonoBehaviour[] scripts;

	// Use this for initialization
	void Start () {
		if (loadonstart)
			SpawnAvatar ();
	}
	
	// Update is called once per frame
	void Update () {
		if (initonce) {

			Transform tchild = transform.GetChild (2);
			BoxCollider tbox = tchild.gameObject.AddComponent<BoxCollider> ();
			CapsuleCollider tcap = tchild.gameObject.AddComponent<CapsuleCollider> ();
			tcap.direction = 2;
			tcap.radius = tbox.size.y;
			tcap.height = tbox.size.z;
			CapsuleCollider tcap2 = transform.GetComponent<CapsuleCollider> ();
			tcap2.radius = tcap.radius;
			tcap2.height = tcap.height;
			tcap2.center = tcap.center;

			Vector3 tvec = tcap2.center;
			tvec.y = tcap2.height/2f;
			tvec.z = 0f;
			tcap2.center = tvec;
			tbox.enabled = false;
			tcap.enabled = false;

			for (int i=0; i<scripts.Length; i++)
			{
				scripts[i].enabled = true;
			}

			initonce = false;
		}
	}

	public void SpawnAvatar()
	{
		if (myUma == null)
			myUma = new GameObject();

		Avatar.Utilities.LoadPlayerAvatar(myUma.transform, avatarname);
	}
}
