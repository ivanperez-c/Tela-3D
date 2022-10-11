using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{

    public int vertexA;
    public int vertexB;
    public int vertexOther;
    public Edge(int verticeA, int verticeB, int verticeC)
    {
        this.vertexOther = verticeC;
        if (verticeA < verticeB)
        {
            this.vertexA = verticeA;
            this.vertexB = verticeB;
            
        }
        else if (verticeB < verticeA)
        {
            this.vertexA = verticeB;
            this.vertexB = verticeA;
            this.vertexOther = verticeC;
        }
    }
}