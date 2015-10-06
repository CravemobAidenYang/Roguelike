using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnionFind
{
    public static void Union(RoomVertex a, RoomVertex b)
    {
        var aRoot = Find(a);
        var bRoot = Find(b);
        if (aRoot == bRoot)
        {
            return;
        }

        bRoot.parent = aRoot;
    }
    public static RoomVertex Find(RoomVertex vertex)
    {
        var root = vertex;

        while(true)
        {
            if(root.parent == null)
            {
                break;
            }
            root = root.parent;
        }

        return root;
    }
}

public class RoomVertex
{
    public RoomVertex parent = null;

    public Room Value { get; set; }

    public bool IsVisted { get; set; }

    public RoomVertex(Room value)
    {
        IsVisted = false;
        Value = value;
    }
}

public class RoomEdgeComp : IComparer<RoomEdge>
{
    public int Compare(RoomEdge a, RoomEdge b)
    {
        return (int)(a.Weight - b.Weight);
    }
}

public class RoomEdge
{
    public RoomVertex A { get; set; }
    public RoomVertex B { get; set; }

    public float Weight { get; set; }

    public RoomEdge(RoomVertex a, RoomVertex b, float weight)
    {
        A = a;
        B = b;
        Weight = weight;
    }

}

//양방향 아님
public class RoomGraph
{
    List<RoomVertex> _VertexList = new List<RoomVertex>();
    List<RoomEdge> _EdgeList = new List<RoomEdge>();
    public RoomGraph(List<RoomVertex> vertexList, List<RoomEdge> edgeList)
    {
        _VertexList.AddRange(vertexList);
        _EdgeList.AddRange(edgeList);
    }

    public List<RoomEdge> GetMST()
    {
        //모든 간선의 리스트를 가져와서 정렬한다.
        //간선을 앞에서 부터 꺼내며 mst에 집어넣는다.
        //이미 집어넣은 정점 목록에 집어넣으려는 간선에 연결된 정점 2개가 다 들어있으면 그 간선은 제외한다.
        //반복한다.

        var mst = new List<RoomEdge>();
        
        var edgeList = new List<RoomEdge>();
        edgeList.AddRange(_EdgeList);
        edgeList.Sort(new RoomEdgeComp());

        UnionFind unionFind = new UnionFind();

        while(true)
        {
            if(mst.Count == _VertexList.Count - 1)
            {
                break;
            }

            if(edgeList.Count == 0)
            {
                break;
            }
            var edge = edgeList[0];
            edgeList.RemoveAt(0);

            if(UnionFind.Find(edge.A) == UnionFind.Find(edge.B))
            {
                continue;
            }
            else
            {
                UnionFind.Union(edge.A, edge.B);
                mst.Add(edge);
            }
        }

        return mst;
    }
}
