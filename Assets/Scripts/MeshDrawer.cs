using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MeshDrawer : MonoBehaviour {
    public bool DebugShowVerts;
    private MeshFilter _mf;
    private Mesh _mesh;
    public float stepWidth;
    public float lineWeight;
    public GameObject MeshLinePrefab;
    public GameObject VertNr;

    public GameObject ActiveMeshLine;
    public Vector3 LastClick;
    
    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GenerateMesh(GetMouseWorldPoint(Input.mousePosition));
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopDrawing();
        }

        if (ActiveMeshLine)
        {
            CreateMeshLine();
        }
    }

    private Vector3 GetMouseWorldPoint(Vector3 mouseScreenPoint)
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(mouseScreenPoint);
        return new Vector3(worldPoint.x, worldPoint.y, 0);
    }

    private void GenerateMesh(Vector3 lastClick)
    {
        ActiveMeshLine = Instantiate(MeshLinePrefab) as GameObject;
        ActiveMeshLine.transform.SetParent(transform);
        _mf = ActiveMeshLine.GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _mf.mesh = _mesh;
        LastClick = lastClick;
    }

    private void StopDrawing()
    {
        var vertPoints2D = new List<Vector2>();
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            vertPoints2D.Add(new Vector2(_mesh.vertices[i].x, _mesh.vertices[i].y));
        }
        DrawCollider(vertPoints2D);

        ActiveMeshLine = null;        
    }
    private void CreateMeshLine()
    {
        if (Input.GetKey(KeyCode.Mouse0) && (Vector3.Distance(GetMouseWorldPoint(Input.mousePosition), LastClick) > stepWidth))
        {
            var mouseWorldPoint = GetMouseWorldPoint(Input.mousePosition);
            int steps = (int)Mathf.Floor(Vector3.Distance(mouseWorldPoint, LastClick) / stepWidth);
            
            for (int i = 0; i < steps; i++)
            {
                var stepStart = Vector3.Lerp(LastClick, mouseWorldPoint, ((i*1f) / steps));
                var stepFinish = Vector3.Lerp(LastClick, mouseWorldPoint, ((i*1f + 1) / steps));
                DrawLine(stepStart, stepFinish);
              //  DrawCollider(stepStart, stepFinish);
            }

            LastClick = mouseWorldPoint;
        }
    }

    private void DrawCollider(List<Vector2> points)
    {
        var colliderGo = new GameObject("PolygonCollider2D", typeof(PolygonCollider2D));

        colliderGo.transform.SetParent(ActiveMeshLine.transform);

        colliderGo.transform.position = ActiveMeshLine.transform.position;

        if (DebugShowVerts)
        {
            //debug
            for (int i = 0; i < points.Count; i++)
            {
                var v = Instantiate(VertNr) as GameObject;
                v.GetComponentInChildren<TextMesh>().text = i.ToString();
                v.transform.position = points[i];
            }
        }


        var collider = colliderGo.GetComponent<PolygonCollider2D>();

        var evenVerts = new List<Vector2>();
        var oddVerts = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            if (i % 2 == 0)
                evenVerts.Add(points[i]);
            else
                oddVerts.Add(points[i]);
        }
        evenVerts.Reverse();

        var sortedpoints = oddVerts.Concat(evenVerts).ToArray();
        collider.SetPath(0, sortedpoints);
    }

    private void DrawLine(Vector3 startPoint, Vector3 finishPoint)
    {

        Vector3 firstNewVerticle = RevertInYBasedOnPoint(finishPoint, SetPositionOnCircle(finishPoint, lineWeight, -AngleBetweenTwoPoints(startPoint, finishPoint)));
        Vector3 secondNewVerticle = SetPositionOnCircle(finishPoint, lineWeight, AngleBetweenTwoPoints(startPoint, finishPoint));

        if (_mesh.vertices.Length >= 2)
        {
            var lenght = _mesh.vertices.Length;

            var copyOfVertices = _mesh.vertices;

            copyOfVertices[lenght - 1] = Vector3.Lerp(_mesh.vertices[lenght - 1], secondNewVerticle, .5f);
            copyOfVertices[lenght - 2] = Vector3.Lerp(_mesh.vertices[lenght - 2], firstNewVerticle, .5f);

            _mesh.vertices = copyOfVertices;
        }

        //verts
        Vector3[] verts = new Vector3[]
        {
            firstNewVerticle,
            secondNewVerticle
        };
        _mesh.vertices = AddArrays<Vector3>(_mesh.vertices, verts);

        //normals
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
            };
            _mesh.triangles = AddArrays<int>(_mesh.triangles, tris);
        }

        Vector2[] tempUvs = new Vector2[_mesh.vertices.Length];
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            switch (i % 4)
            {
                case 0:
                    tempUvs[i] = new Vector2(0, 0);
                    break;
                case 1:
                    tempUvs[i] = new Vector2(0, 1);
                    break;
                case 2:
                    tempUvs[i] = new Vector2(1, 0);
                    break;
                case 3:
                    tempUvs[i] = new Vector2(1, 1);
                    break;
            }
        }
        _mesh.SetUVs(0, tempUvs.ToList());

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="centerPoint"></param>
    /// <param name="baseVector"></param>
    /// <returns></returns>
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
}
