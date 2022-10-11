using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeComparator : IComparer<Edge>
{
    public int Compare(Edge edgeAux1, Edge edgeAux2)
    {
        if (edgeAux1.vertexA > edgeAux2.vertexB)
        {
            return 1;
        }
        else if (edgeAux1.vertexA < edgeAux2.vertexA)
        {
            return -1;
        }
        else if (edgeAux1.vertexA == edgeAux2.vertexA)
        {
            if (edgeAux1.vertexB > edgeAux2.vertexB)
            {
                return 1;
            }
            else if (edgeAux1.vertexB < edgeAux2.vertexB)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }
}