using UnityEngine;
using System.Collections;

public class cross : MonoBehaviour {

    public GameObject VectorA;
    public GameObject VectorB;

    public GameObject Prefab1;

    // Use this for initialization
    void Start () {
       // Instantiate(Prefab1);

        Prefab1.transform.position = Vector3.Cross(VectorA.transform.position, VectorB.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        Prefab1.transform.position = Vector3.Cross(VectorA.transform.position, VectorB.transform.position);

    }
}
