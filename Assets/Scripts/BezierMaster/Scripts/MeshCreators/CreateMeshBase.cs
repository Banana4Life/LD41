using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class CreateMeshBase {

    protected BezierSpline spline;
    
    protected int lenghtSegmentsCount = 20;
    public int LenghtSegmentsCount
    {
        get
        {
            return lenghtSegmentsCount;
        }
        set
        {
            if (value > 2 && value < 1000)
                lenghtSegmentsCount = value;
        }
    }
   
    protected int widhtSegmentsCount = 6;
    public int WidthSegmentsCount
    {
        get
        {
            return widhtSegmentsCount;
        }
        set
        {
            if (value > 2 && value < 1000)
                widhtSegmentsCount = value;
        }
    }

    public float radius1 = 5;

    protected Vector3[] vertices = new Vector3[0];
    protected Vector3[] normals = new Vector3[0];
    protected int[] triangles = new int[0];
    protected Vector2[] uv;
   
    public bool twoSided = false;
    public bool textureOrientation = false;
    
    public abstract Mesh CreateMesh();

    public int GetVertexCount()
    {
        return vertices.Length;
    }

    public int GetTrianglesCount()
    {
        return triangles.Length / 3;
    }
}
