using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring  {

    public Node nodeA, nodeB;

    public float Length0;
    public float Length;

    public float stiffness;

    public Vector3 posicion;
    public float damping = 0.01f;

    // Use this for initialization
    /*void Start () {
        UpdateLength();
        Length0 = Length;
    }
	
	// Update is called once per frame
	void Update () {
        transform.localScale = new Vector3(transform.localScale.x, Length / 2.0f, transform.localScale.z);
        transform.position = 0.5f * (nodeA.pos + nodeB.pos);

        Vector3 u = nodeA.pos - nodeB.pos;
        u.Normalize();
        transform.rotation = Quaternion.FromToRotation(Vector3.up, u);
    }*/

    public Spring(Node node1, Node node2, float rigidez, float fuerzaMuelle) //fuerzaMuelle = Beta
    {
        this.nodeA = node1;
        this.nodeB = node2;
        this.stiffness = rigidez;
        Length = (nodeA.pos - nodeB.pos).magnitude;
        Length0 = (nodeA.pos - nodeB.pos).magnitude;
        this.damping = fuerzaMuelle * this.stiffness;

    }
    public void UpdateLength ()
    {
        Length = (nodeA.pos - nodeB.pos).magnitude;
    }

    public void ComputeForces()
    {
        Vector3 u = nodeA.pos - nodeB.pos;
        u.Normalize();
        Vector3 force = -stiffness * (Length - Length0) * u - damping * Vector3.Dot(u, (nodeA.vel - nodeB.vel)) * u;
        nodeA.force += force;
        nodeB.force -= force;
    }
}
