using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Basic physics manager capable of simulating a given ISimulable
/// implementation using diverse integration methods: explicit,
/// implicit, Verlet and semi-implicit.
/// </summary>
public class MassSpringCloth : MonoBehaviour 
{
	/// <summary>
	/// Default constructor. Zero all. 
	/// </summary>
	public MassSpringCloth()
	{
		this.Paused = true;
		this.TimeStep = 0.01f;
		this.Gravity = new Vector3 (0.0f, -9.81f, 0.0f);
		this.IntegrationMethod = Integration.Symplectic;
	}

	/// <summary>
	/// Integration method.
	/// </summary>
	public enum Integration
	{
		Explicit = 0,
		Symplectic = 1,
	};

	#region InEditorVariables

	public bool Paused;
	public float TimeStep;
    public Vector3 Gravity;
	public Integration IntegrationMethod;
    public int rigidez;
    public int rigidezFlexion;
    public float masa;
    public float fuerzaNodo;
    public float fuerzaMuelle;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    public List<Node> nodes = new List<Node>();
    public List<Spring> springs = new List<Spring>();
    public List<Fixer> fixers = new List<Fixer>();
    public List<Edge> edges = new List<Edge>();
    public List<Edge> muelleDeTraccion = new List<Edge>();
    public List<Edge> muelleDeFlexion = new List<Edge>();
    #endregion

    #region OtherVariables
    #endregion

    #region MonoBehaviour

    public void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        //Creamos todos los nodos
        for (int j = 0; j < vertices.Length; j++)
        {
            Node nodoAux = new Node(transform.TransformPoint(vertices[j]), masa, fuerzaNodo); 
            nodes.Add(nodoAux); 
        }

        //Comprobamos que nodos están fijados por el fixer
        foreach (Node nodo in nodes)
        {
            foreach (Fixer fixer in fixers)
            {
                if (fixer.isInside(nodo.pos) == true)
                {
                    nodo.isFixed = true;
                    break;
                }
            }
        }

        //Busqueda de todas las aristas de la malla
        for (int j = 0; j < triangles.Length; j += 3)
        {
            edges.Add(new Edge(triangles[j], triangles[j + 1], triangles[j + 2]));
            edges.Add(new Edge(triangles[j], triangles[j + 2], triangles[j + 1]));
            edges.Add(new Edge(triangles[j + 1], triangles[j + 2], triangles[j]));
        }

        //Ordenar la lista de aristas para que vertexA y vertexB siempre esten ordenados
        EdgeComparator comparer = new EdgeComparator();
        edges.Sort(comparer);

        //Bucle que elimina los muelles de tracción por arista duplicados
        for (int j = 0; j < edges.Count; j++)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[j].vertexA == edges[i].vertexA && edges[j].vertexB == edges[i].vertexB)
                {
                    muelleDeTraccion.Remove(edges[i]);
                    if (!muelleDeTraccion.Contains(edges[j]))
                    {
                        muelleDeTraccion.Add(edges[j]);
                    }
                }
            }
        }
        //print("Muelles de traccion " + muelleDeTraccion.Count);

        //Obteccion de las aristas de flexion
        for (int i = 0; i < (edges.Count - 1); i++)
        {
            if (edges[i].vertexA == edges[i + 1].vertexA && edges[i].vertexB == edges[i + 1].vertexB)
            {
                muelleDeFlexion.Add(new Edge(edges[i].vertexOther, edges[i + 1].vertexOther, 0));
                muelleDeFlexion.Add(new Edge(edges[i].vertexOther, edges[i].vertexA, 0));
                muelleDeFlexion.Add(new Edge(edges[i].vertexOther, edges[i].vertexB, 0));
            }
            else
            {
                muelleDeFlexion.Add(new Edge(edges[i].vertexOther, edges[i].vertexA, 0));
                muelleDeFlexion.Add(new Edge(edges[i].vertexOther, edges[i].vertexB, 0));
                muelleDeFlexion.Add(new Edge(edges[i].vertexA, edges[i].vertexB, 0));
            }
        }
        //print("Muelles de flexion " + muelleDeFlexion.Count);
        
        //Obtencion de los muelles de traccion
        for (int j = 0; j < muelleDeTraccion.Count; j++)
        {
            int aux1 = muelleDeTraccion[j].vertexA;
            int aux2 = muelleDeTraccion[j].vertexB;
            springs.Add(new Spring(nodes[aux1], nodes[aux2], rigidez, fuerzaMuelle));
        }

        //Obtencion de los muelles de flexion
        for (int j = 0; j < muelleDeFlexion.Count; j++)
        {
            int aux1 = muelleDeFlexion[j].vertexA;
            int aux2 = muelleDeFlexion[j].vertexB;
            springs.Add(new Spring(nodes[aux1], nodes[aux2], rigidezFlexion, fuerzaMuelle));
        }
    }

    public void Update()
	{
        //Si la simulacion esta pausada la activamos con la tecla P
		if (Input.GetKeyUp (KeyCode.P))
			this.Paused = !this.Paused;

        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = new Vector3[mesh.vertexCount];

        //Pasar la posicion de los vertices del sistema de referencia global al local
        int i = 0;
        foreach (Node nodo in nodes)
        {
            vertices[i] = transform.InverseTransformPoint(nodo.pos);
            i++;
        }

        //Actualizacion de los vertices del mesh
        mesh.vertices = vertices;
       
    }

    public void FixedUpdate()
    {
        if (this.Paused)
            return; // Not simulating

        // Select integration method
        switch (this.IntegrationMethod)
        {
            case Integration.Explicit: this.stepExplicit(); break;
            case Integration.Symplectic: this.stepSymplectic(); break;
            default:
                throw new System.Exception("[ERROR] Should never happen!");
        }

        if (Input.GetKeyUp(KeyCode.P))
            this.Paused = !this.Paused;
    }

    #endregion

    /// <summary>
    /// Performs a simulation step in 1D using Explicit integration.
    /// </summary>
    private void stepExplicit()
	{
        foreach (Node node in nodes)
        {

            node.force = Vector3.zero;
            node.ComputeForces(Gravity);

        }

        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {

                node.pos += TimeStep * node.vel;
                node.vel += TimeStep / node.mass * node.force;

            }
        }
        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }

    }

    /// <summary>
    /// Performs a simulation step in 1D using Symplectic integration.
    /// </summary>
    private void stepSymplectic()
	{
        foreach (Node node in nodes)
        {
            node.force = Vector3.zero;
            node.ComputeForces(Gravity);
        }
        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {
                node.vel += TimeStep / node.mass * node.force;
                node.pos += TimeStep * node.vel;
            }
        }

        foreach (Spring spring in springs)
        {
           spring.UpdateLength();
        }
    }
}
