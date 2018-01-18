using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics_CPU : MonoBehaviour 
{
	public GameObject TheBall;
	public int BallCounts = 100;
	public float AreaLimit = 4f;

	private Transform[] _balls;


	// Use this for initialization
	void Start () 
	{
		_balls = new Transform[BallCounts];

		for(int i=0;i<BallCounts;i++)
		{
			_balls[i] = Instantiate(TheBall, TheBall.transform.parent).transform;
			Vector3 randomPos = Vector3.zero;
			randomPos.x = Random.Range (-AreaLimit, AreaLimit);
			randomPos.y = TheBall.transform.localPosition.y;
			randomPos.z = Random.Range (-AreaLimit, AreaLimit);
			_balls [i].localPosition = randomPos;
			_balls [i].name = "sphere " + i;
			_balls [i].gameObject.SetActive (true);
		}
		
	}
		

	// Update is called once per frame
	void Update () 
	{
		
	}
}
