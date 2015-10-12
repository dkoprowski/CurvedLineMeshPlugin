using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour {
    public GameObject BoxPrefab;
    public GameObject CirclePrefab;
    public int seed;
    private System.Random _rnd;
    public void GenerateObjects()
    {
        _rnd = new System.Random();

        for (int i = 0; i < seed; i++)
        {
            var box = Instantiate(BoxPrefab) as GameObject;
            box.transform.position = new Vector3(_rnd.Next(-8, 8), _rnd.Next(8, 12));
            var circle = Instantiate(CirclePrefab) as GameObject;
            circle.transform.position = new Vector3(_rnd.Next(-8, 8), _rnd.Next(8, 12));
        }
    }
}
