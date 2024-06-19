using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

struct Area
{
    int m_VertexStart;
    int m_Size;
}

enum Mode
{
    Normal,
    Graph,
    Area,
}

public class Graph : MonoBehaviour
{
    // List<GameObject> m_Graphics = new List<GameObject>();
    List<Vector3> m_Nodes = new List<Vector3>();
    List<int> m_Edges = new List<int>();

    List<Vector3> m_AreaVertices = new List<Vector3>();
    List<Area> m_Areas = new List<Area>();

    [SerializeField]
    GameObject m_PointGraphics;
    [SerializeField]
    MeshFilter m_EdgePreview;

    MeshFilter m_MeshFilter;

    GameObject m_HoveredPoint;
    int? m_FromPoint;

    Mode m_Mode = Mode.Normal;


    void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();

        m_MeshFilter.mesh = new Mesh();

        var nodes = new List<Vector3> { Vector3.right * 5f, Vector3.forward, Vector3.back };
        foreach (var pos in nodes)
        {
            AddNode(pos);
        }

        AddEdge(0, 1);
        AddEdge(0, 2);
    }

    int AddNode(Vector3 position)
    {
        int index = m_Nodes.Count;
        m_Nodes.Add(position);
        var point = Instantiate(m_PointGraphics, transform);
        point.GetComponent<VertexHandle>().m_VertexIndex = index;
        point.transform.localPosition = position;

        return index;
    }

    void AddEdge(int p1, int p2)
    {
        m_Edges.Add(p1);
        m_Edges.Add(p2);

        UpdateMesh();

        // Vector3 pos1 = m_Nodes[p1];
        // Vector3 pos2 = m_Nodes[p2];

        // var dir = (pos2 - pos1).normalized;
        // var left = new Vector3(-dir.z, 0, dir.x) * 0.5f;

        // Debug.DrawLine(m_Nodes[p2], m_Nodes[p1], Color.white, float.MaxValue);
        // Debug.DrawLine(m_Nodes[p1], m_Nodes[p1] + dir, Color.red, float.MaxValue);
        // Debug.DrawLine(m_Nodes[p1], m_Nodes[p1] + left, Color.green, float.MaxValue);

        // int startIndex = m_MeshFilter.mesh.vertices.Length;
        // var verts = new List<Vector3>(m_MeshFilter.mesh.vertices) ;
        // verts.AddRange(new Vector3[4] {pos1 + left, pos1 - left, pos2 + left, pos2 - left});


        // var newTris = new int[6] {0,2,3,0,3,1};
        // for(int i = 0; i < newTris.Length; i++) {
        //     newTris[i] += startIndex;
        // }

        // var tris = new List<int>(m_MeshFilter.mesh.triangles);
        // tris.AddRange(newTris);

        // m_MeshFilter.mesh.vertices = verts.ToArray();
        // m_MeshFilter.mesh.triangles = tris.ToArray();
    }


    void UpdateMesh()
    {
        var verts = new Vector3[m_Edges.Count * 2];
        var tris = new int[m_Edges.Count * 3];

        var addTris = new int[6] { 0, 2, 3, 0, 3, 1 };

        for (int i = 0; i < m_Edges.Count / 2; i++)
        {
            var offset = i * 2;
            Vector3 pos1 = m_Nodes[m_Edges[offset]];
            Vector3 pos2 = m_Nodes[m_Edges[offset + 1]];

            var dir = (pos2 - pos1).normalized;
            var left = new Vector3(-dir.z, 0, dir.x) * 0.5f;

            int startIndex = m_MeshFilter.mesh.vertices.Length;

            var addVerts = new Vector3[4] { pos1 + left, pos1 - left, pos2 + left, pos2 - left };
            var vertOffset = i * addVerts.Length;

            for (int j = 0; j < addVerts.Length; j++)
            {
                verts[vertOffset + j] = addVerts[j];
            }

            var triOffset = i * addTris.Length;
            for (int j = 0; j < addTris.Length; j++)
            {
                tris[triOffset + j] = addTris[j] + vertOffset;
            }
        }

        m_MeshFilter.mesh.vertices = verts;
        m_MeshFilter.mesh.triangles = tris;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        m_HoveredPoint = null;
        if (Physics.Raycast(ray, out hit))
        {
            m_HoveredPoint = hit.transform.gameObject;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)) {
            m_Mode = Mode.Normal;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            m_Mode = Mode.Graph;
        }

        switch (m_Mode)
        {
            case Mode.Normal:
                IDK();
                break;
            case Mode.Graph:
                UpdateGraph();
                break;

            case Mode.Area:
                break;
        }

    }
    
    void IDK() {
        if (m_HoveredPoint != null && Input.GetMouseButton(0)) {
            Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorPos.y = 0;
            m_HoveredPoint.transform.position = cursorPos;
        }
    }

    void UpdateGraph()
    {
        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPos.y = 0;

        Mesh edgePreviewMesh = new Mesh();

        if (m_FromPoint.HasValue)
        {
            var tris = new int[6] { 0, 2, 3, 0, 3, 1 };

            Vector3 pos1 = m_Nodes[m_FromPoint.Value];
            Vector3 pos2 = cursorPos;

            var dir = (pos2 - pos1).normalized;
            var left = new Vector3(-dir.z, 0, dir.x) * 0.5f;


            var verts = new Vector3[4] { pos1 + left, pos1 - left, pos2 + left, pos2 - left };

            edgePreviewMesh.vertices = verts;
            edgePreviewMesh.triangles = tris;
        }

        m_EdgePreview.mesh = edgePreviewMesh;

        if (m_HoveredPoint != null)
        {
            m_HoveredPoint.GetComponent<MeshRenderer>().material.color = Color.white;
        }


        else
        {
            m_HoveredPoint = null;
        }

        if (m_HoveredPoint != null)
        {
            m_HoveredPoint.GetComponent<MeshRenderer>().material.color = Color.red;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_FromPoint = null;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_HoveredPoint == null)
            {
                int newPoint = AddNode(cursorPos);

                if (m_FromPoint.HasValue)
                {
                    AddEdge(m_FromPoint.Value, newPoint);
                }

                m_FromPoint = newPoint;

            }
            else
            {
                int index = m_HoveredPoint.GetComponent<VertexHandle>().m_VertexIndex;
                if (!m_FromPoint.HasValue)
                {
                    m_FromPoint = index;
                }
                else
                {
                    if (m_FromPoint != index)
                    {
                        AddEdge(m_FromPoint.Value, index);
                    }

                    m_FromPoint = index;
                }
            }
        }

    }
}
