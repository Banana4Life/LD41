using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum Using
{
    Objects,
    Mesh,
    None
}

public enum MeshType
{
    Line,
    Cylinder,
    Tube
}

[SelectionBase]
[RequireComponent(typeof(BezierSpline))]
public class BezierMaster : MonoBehaviour
{
#if UNITY_EDITOR
    public BezierSpline spline;

    public bool showCurveEditor;
    public bool showObjectsOptions;
    public bool showAnimationOptions;
    public bool showObjRandomisationOptions;
    public bool showRotationsOptions;
    public bool autoUpdate;

    public bool Scale3d = false;
    public bool randomise;
    public int seed = 0;

    public Using usingOfSpline;
    public MeshType meshType;



    //
    //   -----    objects creating variables   -----
    //
    [SerializeField]
    private int objectsCount = 5; 
    public int ObjectsCount {

        get{
            return objectsCount;
        }
        set{
            if (value > 1 && value < 300)
                objectsCount = value;
        }
    }

    [SerializeField]
    public GameObject[] objectsPrefabs = new GameObject[0];
    int objectN = 0;

    [SerializeField]
    int[] objectsRandomIndexes;
    [SerializeField]
    Vector3[] objectRandomScales;
    [SerializeField]
    Vector3[] objectRandomOffsets;
    [SerializeField]
    Quaternion[] objectRandomRotation;


    public Vector3 scaleRandomMaximum = Vector3.one;
    public Vector3 offsetRandomMaximum = Vector3.one;
    public Vector3 rotationRandomMaximum = Vector3.zero;

    [SerializeField]
    public GameObject[] instantiatedObjects = new GameObject[0];
    [SerializeField]
    Vector3[] objectsScales;

    public bool ApplyRotationX = false;
    public bool ApplyRotationY = false;
    public bool ApplyRotationZ = false;
    public Quaternion addRotation = Quaternion.identity;

    //
    //   -----    mesh creating variables   -----
    //
    [SerializeField]
    private int lenghtSegmentsCount = 10;
    public int LenghtSegmentsCount
    {
        get
        {
            return lenghtSegmentsCount;
        }
        set
        {
            if (value > 2 && value < 300)
                lenghtSegmentsCount = value;
        }
    }
    
    [SerializeField]
    private int widhtSegmentsCount = 5;
    public int WidthSegmentsCount
    {
        get
        {
            return widhtSegmentsCount;
        }
        set
        {
            if (value > 2 && value < 300)
                widhtSegmentsCount = value;
        }
    }
   
    [SerializeField]
    public float radius1 = 3;
    [SerializeField]
    public float radius2 = 1;

    [SerializeField]
    public GameObject meshGO;

    private Mesh mesh;

    [SerializeField]
    public bool twoSided = false;
    [SerializeField]
    public bool capStart = true;
    [SerializeField]
    public bool capEnd = true;
    [SerializeField]
    public bool textureOrientation = false;
    public CreateMeshBase meshCreator;

    public int verticesCount = 0;
    public int trianglesCount = 0;

    private void OnEnable()
    {
        //Reset();
        //Debug.Log("OnEnable");
    }

    public void Reset()
    {
        if (spline == null)
            spline = GetComponent<BezierSpline>();

        showCurveEditor = true;
        showObjectsOptions = true;
        showAnimationOptions = false;
        showObjRandomisationOptions = false;
        showRotationsOptions = false;
        randomise = false;
        autoUpdate = true;

        objectN = 0;
        objectsPrefabs = new GameObject[] { };

        Clear(true);

        objectsCount = 5;
        ApplyRotationX = false;
        ApplyRotationY = false;
        ApplyRotationZ = false;

        addRotation = Quaternion.identity;

        lenghtSegmentsCount = 10;

        widhtSegmentsCount = 5;

        radius1 = 10;
        radius2 = 5;
        twoSided = false;
        capStart = true;
        capEnd = true;
        textureOrientation = false;

        Vector3 scaleRandomMaximum = Vector3.one;
        Vector3 offsetRandomMaximum = Vector3.zero;
        Vector3 rotationRandomMaximum = Vector3.zero;
    }

    public void Clear(bool destroy)
    {
        if (destroy)
            for (int i = 0; i < instantiatedObjects.Length; i++)
            {
                if (instantiatedObjects[i] != null)
                    DestroyImmediate(instantiatedObjects[i]);
            }

        instantiatedObjects = new GameObject[objectsCount];
        objectsRandomIndexes = new int[objectsCount];
        objectsScales = new Vector3[objectsCount];
        objectRandomScales = new Vector3[objectsCount];
        objectRandomOffsets = new Vector3[objectsCount];
        objectRandomRotation = new Quaternion[objectsCount];

    }

    public void UpdateMaster(bool updateRandom)
    {
        switch (usingOfSpline)
        {
            case Using.Mesh:
                UpdateMesh();
                break;
            case Using.Objects:
                UpdateObjects(updateRandom);
                break;
        }

        if (updateRandom && randomise)
            InitRandom();

        //Debug.Log("update!");
    }

    public void UpdateRandom()
    {
        for (int i = 0; i < objectsCount; i++)
        {
           
    
            }
    }

    public void DetachObjects()
    {
        switch (usingOfSpline)
        {
            case Using.Mesh:
                meshGO.transform.parent = null;
                meshGO = null;
                break;

            case Using.Objects:
                var parent = new GameObject("Parent").transform;
                parent.position = transform.position;
                parent.rotation = transform.rotation;

                for (int i = 0; i < instantiatedObjects.Length; i++)
                {
                    instantiatedObjects[i].transform.SetParent(parent);
                }

                Clear(false);
                break;
        }

       
    }



    void UpdateObjects(bool updateRandom)
    {
        if (objectsPrefabs.Length == 0 || objectsPrefabs[0] == null)
            return;

        if (instantiatedObjects == null || instantiatedObjects.Length != objectsCount || instantiatedObjects[0] == null || updateRandom)
        {
            Clear(true);

            for (int i = 0; i < objectsCount; i++)
            {
                float t = i / (float)(objectsCount - 1);

                if (spline.Loop)
                    t = i / (float)(objectsCount);

                instantiatedObjects[i] = Instantiate(GetObject(), transform.TransformPoint(spline.GetPoint(t)), GetRotation(i, t), transform) as GameObject;
                
                objectsScales[i] = instantiatedObjects[i].transform.localScale;
                instantiatedObjects[i].transform.localScale = GetScale(i, t);

                if (updateRandom)
                {
                    instantiatedObjects[i].transform.position = transform.TransformPoint(spline.GetPoint(t)) + objectRandomOffsets[i];
                    instantiatedObjects[i].transform.localScale = GetScale(i, t);
                    instantiatedObjects[i].transform.rotation = GetRotation(i, t);
                }

                instantiatedObjects[i].name += " (" + (i + 1) + ")";
            }

        }

        else if (instantiatedObjects.Length > 0 && instantiatedObjects[0] != null)
        {
            for (int i = 0; i < objectsCount; i++)
            {
                float t = i / (float)(objectsCount - 1);
                if (spline.Loop)
                    t = i / (float)(objectsCount);

                instantiatedObjects[i].transform.position = transform.TransformPoint(spline.GetPoint(t)) + objectRandomOffsets[i];
                instantiatedObjects[i].transform.localScale = GetScale(i, t);
                instantiatedObjects[i].transform.rotation = GetRotation(i, t);

               // Debug.DrawLine(transform.TransformPoint(spline.GetPoint(t)), transform.TransformPoint(spline.GetPoint(t)) + spline.GetDirection(t));
            }
        }

    }

    void UpdateMesh()
    {
        if (meshGO == null)
        {
            meshGO = new GameObject("Mesh");
            meshGO.transform.position = transform.position;
            meshGO.transform.rotation = transform.rotation;

            meshGO.transform.SetParent(transform);
            var mr = meshGO.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));

        }
        mesh = meshCreator.CreateMesh();

        verticesCount = meshCreator.GetVertexCount();
        trianglesCount = meshCreator.GetTrianglesCount();

        var mf = meshGO.GetComponent<MeshFilter>();
        if (!mf)
            mf = meshGO.AddComponent<MeshFilter>();
        
        mf.mesh = mesh;

        
    }

    Quaternion GetRotation(int i, float t)
    {        
        Quaternion rotation;
        if (ApplyRotationX || ApplyRotationY || ApplyRotationZ)
        {
            rotation = Quaternion.LookRotation(spline.GetDirection(t)) ;
            rotation = Quaternion.Euler(ApplyRotationX ? rotation.eulerAngles.x : 0, 
                                            ApplyRotationY ? rotation.eulerAngles.y : 0, 
                                                 ApplyRotationZ ? rotation.eulerAngles.z + spline.GetRotationZ(t) : 0);
        }
        else
            rotation = Quaternion.identity;

        rotation = rotation  * addRotation;
        rotation = transform.rotation * rotation;
        
        if (randomise)
            rotation *= objectRandomRotation[i];
            
        return rotation;
    }

    Vector3 GetScale(int i, float t)
    {
        Vector3 scale = new Vector3(objectsScales[i].x * spline.GetScale(t).x, objectsScales[i].y * spline.GetScale(t).y, objectsScales[i].z * spline.GetScale(t).z);

        if (randomise)
            scale += objectRandomScales[i];

        return scale;
    }

    public void InitRandom()
    {       
        

        for (int i = 0; i < objectsCount; i++)
        {
            objectsScales[i] = instantiatedObjects[i].transform.localScale;
            

            objectsRandomIndexes[i] = (int)(Random.value * (objectsPrefabs.Length - 1));
            objectRandomRotation[i] = Quaternion.Euler(Random.value * rotationRandomMaximum.x, Random.value * rotationRandomMaximum.y, Random.value * rotationRandomMaximum.z);
            objectRandomOffsets[i] = new Vector3(Random.value * offsetRandomMaximum.x, Random.value * offsetRandomMaximum.y, Random.value * offsetRandomMaximum.z);        
            objectRandomScales[i] = new Vector3(Random.value * scaleRandomMaximum.x, Random.value * scaleRandomMaximum.y, Random.value * scaleRandomMaximum.z);
        }
    }

    GameObject GetObject()
    {
        if (randomise)
        {

            if (objectsPrefabs.Length == 0 || objectsPrefabs[0] == null)
                return null;

            if (objectN > objectsRandomIndexes.Length - 1)
                objectN = 0;

            //Debug.Log(objectsRandomIndexes[objectN++]);

            return objectsPrefabs[objectsRandomIndexes[objectN++]];

        }
        else
        {
            if (objectN > objectsPrefabs.Length - 1)
                objectN = 0;

            return objectsPrefabs[objectN++];

        }

    }

    [MenuItem("Bezier Master/Create Bezier")]
    public static void CreateBezierMaster()
    {
        var master = new GameObject("Bezier Master");
        
        master.AddComponent<BezierMaster>().Reset();


    }
#endif

    /// <summary>
    /// Return array of points positions along curve.
    /// </summary>
    /// <param name="pointsCount"></param>
    /// <returns></returns>
    public Vector3[] GetPath(int pointsCount)
    {
        if (spline == null || pointsCount <= 0)
            return null;

        Vector3[] path = new Vector3[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            float t = i / (float)(pointsCount - 1);

            if (spline.Loop)
                t = i / (float)(pointsCount);

            path[i] = transform.TransformPoint(spline.GetPoint(t));
        }

        return path;
    }
    

}
