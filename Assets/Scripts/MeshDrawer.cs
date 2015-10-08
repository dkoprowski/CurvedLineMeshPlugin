using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(MeshFilter))]
public class MeshDrawer : MonoBehaviour {

    private MeshFilter _mf;
    Mesh _mesh;
    public float stepWidth;
    public float lineWeight;

    public Transform StartPoint;
    public Transform FinishPoint;
    public Transform ThirdPoint;

    public GameObject VertPrefab;
    public GameObject Vert2Prefab;
    public GameObject PointPrefab;

    public GameObject CollidersParent;
    public Vector3 LastClick;
    private void Start()
    {
        GenerateMesh();

        //DrawLine(StartPoint.position, FinishPoint.position);
        //DrawLine(FinishPoint.position, ThirdPoint.position);

    }

    private void Update()
    {
        CreateMeshLine();
    }

    private Vector3 GetMouseWorldPoint(Vector3 mouseScreenPoint)
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(mouseScreenPoint);
        return new Vector3(worldPoint.x, worldPoint.y, 0);
    }

    private void GenerateMesh()
    {
        _mf = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _mf.mesh = _mesh;

    }
    private void CreateMeshLine()
    {
        if (Input.GetKey(KeyCode.Mouse0) && Vector3.Distance(GetMouseWorldPoint(Input.mousePosition), LastClick) > stepWidth)
        {
            var mouseWorldPoint = GetMouseWorldPoint(Input.mousePosition);
            int steps = (int)Mathf.Floor(Vector3.Distance(mouseWorldPoint, LastClick) / stepWidth);
            
            for (int i = 0; i < steps; i++)
            {
                var stepStart = Vector3.Lerp(LastClick, mouseWorldPoint, ((i*1f) / steps));
                var stepFinish = Vector3.Lerp(LastClick, mouseWorldPoint, ((i*1f + 1) / steps));
                DrawLine(stepStart, stepFinish);
                DrawCollider(stepStart, stepFinish);
            }

            LastClick = mouseWorldPoint;
        }
    }
    private void DrawCollider(Vector3 startPoint, Vector3 finishPoint)
    {
        var colliderGo = new GameObject("BoxCollider2D", typeof(BoxCollider2D));
        colliderGo.transform.SetParent(CollidersParent.transform);

        colliderGo.transform.position = Vector2.Lerp(startPoint, finishPoint, .5f);
        colliderGo.transform.localEulerAngles = new Vector3(colliderGo.transform.localEulerAngles.x, colliderGo.transform.localEulerAngles.y,
            AngleBetweenTwoPoints(startPoint, finishPoint));
        var collider = colliderGo.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(Vector2.Distance(startPoint, finishPoint), lineWeight * 2);
    }
    private void DrawLine(Vector3 startPoint, Vector3 finishPoint)
    {
        
        var p =Instantiate(PointPrefab);
        p.transform.position = startPoint;/*
        var redVert = Instantiate(VertPrefab);
        var orangeVert = Instantiate(Vert2Prefab);

        var redPos = SetPositionOnCircle(finishPoint, 1f, -AngleBetweenTwoPoints(StartPoint.transform.position, finishPoint));
        redVert.transform.position =  RevertInYBasedOnPoint(p.transform.position, redPos);
        orangeVert.transform.position = SetPositionOnCircle(finishPoint, 1f, AngleBetweenTwoPoints(StartPoint.transform.position, finishPoint));
        */

        Vector3 firstNewVerticle = RevertInYBasedOnPoint(finishPoint, SetPositionOnCircle(finishPoint, lineWeight, -AngleBetweenTwoPoints(startPoint, finishPoint)));
        Vector3 secondNewVerticle = SetPositionOnCircle(finishPoint, lineWeight, AngleBetweenTwoPoints(startPoint, finishPoint));

        if (_mesh.vertices.Length >= 2)
        {
            var lenght = _mesh.vertices.Length;

            var copyOfVertices = _mesh.vertices;

            copyOfVertices[lenght - 1] = Vector3.Lerp(_mesh.vertices[lenght - 1], secondNewVerticle,.5f);
            copyOfVertices[lenght - 2] = Vector3.Lerp(_mesh.vertices[lenght - 2], firstNewVerticle, .5f);

            _mesh.vertices = copyOfVertices;
        }

        //verts
        Vector3[] verts = new Vector3[]
        {
           // new Vector3(startPoint.x ,startPoint.y - (height/2f),startPoint.z),
           // new Vector3(startPoint.x , startPoint.y + (height/2f),startPoint.z),

       //     SetPositionOnCircle(startPoint, .2f, AngleBetweenTwoPoints(finishPoint, startPoint)),
       //     RevertInYBasedOnPoint(startPoint, SetPositionOnCircle(startPoint, .2f, -AngleBetweenTwoPoints(finishPoint, startPoint))),

            //new Vector3(finishPoint.x ,finishPoint.y -(height/2f),finishPoint.z),
            //new Vector3(finishPoint.x, finishPoint.y + (height/2f),finishPoint.z)
            firstNewVerticle,
            secondNewVerticle
        };
        _mesh.vertices = AddArrays<Vector3>(_mesh.vertices, verts);

        //normals
        //Vector3[] normals = new Vector3[]
        //{
        //    new Vector3(0,0,-1),
        //    new Vector3(0,0,-1)
        //};
        if (_mesh.normals.Length > 1)
        {
            _mesh.normals[_mesh.normals.Length - 1] = new Vector3(0, 0, -1);
            _mesh.normals[_mesh.normals.Length - 2] = new Vector3(0, 0, -1);
        }

        //tris

        int vertLenght = _mesh.vertices.Length;
        if (vertLenght >= 4)
        {
            int[] tris = new int[]
            {
            vertLenght-4,vertLenght-3,vertLenght-2,
            vertLenght-2,vertLenght-3,vertLenght-1

          //  vertLenght-2,vertLenght-4,vertLenght-2,
          //  vertLenght-2,vertLenght-1,vertLenght-3
            };
            _mesh.triangles = AddArrays<int>(_mesh.triangles, tris);
        }


        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        
    }

    private Vector3 RevertInYBasedOnPoint(Vector3 centerPoint,Vector3 baseVector)
    {
        
       if(centerPoint.y > baseVector.y)
        {
            return new Vector3(baseVector.x, centerPoint.y + (centerPoint.y-baseVector.y), baseVector.z);
        }
        else
        {
            return new Vector3(baseVector.x, centerPoint.y - (baseVector.y-centerPoint.y), baseVector.z);

        }
    }

    private T[] AddArrays<T>(T[] array1, T[] array2)
    {
        T[] array3 = new T[array1.Length + array2.Length];
        array1.CopyTo(array3, 0);
        array2.CopyTo(array3, array1.Length);

        return array3;
    }

    public float AngleBetweenTwoPoints(Vector3 position1, Vector3 position2)
    {
        float dy = Mathf.Abs(position2.y - position1.y);
        float dx = Mathf.Abs(position2.x - position1.x);

        float alfa = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        if ((position1.x > position2.x))
            alfa = (90 - alfa) + 90;

        if ((position1.y > position2.y))
            alfa *= -1;

        return alfa;
    }
    private Vector3 SetPositionOnCircle(Vector3 circleMiddle, float radious, float angle)
    {
        Vector3 onCirclePosition = new Vector3();

        float alfa = 90 - angle;
        if (alfa > 270)
        {
            alfa = 360 - alfa;
            alfa *= -1;
        }

        float yDifference = radious * Mathf.Sin(alfa * Mathf.Deg2Rad);
        float xDifference = radious * Mathf.Cos(alfa * Mathf.Deg2Rad);

        onCirclePosition.x = circleMiddle.x - xDifference;

        onCirclePosition.y = circleMiddle.y + yDifference;

        return onCirclePosition;
    }
    //private Vector3 SetPositionOnCircle(Vector3 touchPosition, Vector3 circleMiddle, float radious)
    //{
    //    Vector3 onCirclePosition = new Vector3();

    //    float alfa = 180 - AngleBetweenTwoPoints(circleMiddle, touchPosition);
    //    if (alfa > 270)
    //    {
    //        alfa = 360 - alfa;
    //        alfa *= -1;
    //    }

    //    float yDifference = radious * Mathf.Sin(alfa * Mathf.Deg2Rad);
    //    float xDifference = radious * Mathf.Cos(alfa * Mathf.Deg2Rad);

    //    onCirclePosition.x = circleMiddle.x - xDifference;

    //    onCirclePosition.y = circleMiddle.y + yDifference;

    //    return onCirclePosition;
    //}
}
