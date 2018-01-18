using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.GetComponent<Rigidbody> ().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}

}
