using UnityEngine;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {
    public List<GameObject> objs = new List<GameObject>();

    Vector3 min = new Vector3(-5, 0, -20);
    Vector3 max = new Vector3(4, 0, -4);

	// Use this for initialization
	void Start () {
	
	}
	
    void Spawn()
    {
        float x = Random.Range(min.x, max.x);
        float z = Random.Range(min.z, max.z);
        int r = Random.Range(0, objs.Count);

        GameObject go = GameObject.Instantiate(objs[r]);
        go.transform.position = new Vector3(x, 0, z);
    }

    // Update is called once per frame
    int i = 0;
	void Update () {
        if (i < 175)
            Spawn();

        i++;
	}
}
