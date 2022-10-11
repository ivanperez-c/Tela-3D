using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public Vector3 pos;
    public Vector3 vel;
    public Vector3 force;

    public float mass = 0.6f;
    public bool isFixed = false;

    public float damping = 0.2f;
    // Use this for initialization
   /* private void Awake()
    {
        pos = transform.position;
        vel = Vector3.zero;
    }*/

    /*void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = pos;
	}*/

    public Node(Vector3 posicion, float masa, float fuerzaNodo) //FuerzaNodo = Alpha
    {
        this.pos = posicion;
        this.mass = masa;
        this.damping = fuerzaNodo * this.mass;
    }
    public void ComputeForces(Vector3 Gravity)
    {
        force = mass * Gravity - damping * vel;
    }
}
