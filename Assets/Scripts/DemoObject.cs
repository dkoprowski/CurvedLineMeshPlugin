using UnityEngine;
using System.Collections;

public class DemoObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(transform.position.y < -10)
        {
            transform.position = new Vector3(Random.Range(-8f, 8f), 10, transform.position.z);
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
	}
}
