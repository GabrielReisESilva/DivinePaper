using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPoint : MonoBehaviour {

	private bool active = false;

    public bool IsActive { get { return active; }}
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D col)
    {
		active = true;
	}

	public void Reset()
    {
		active = false;
	}
}
