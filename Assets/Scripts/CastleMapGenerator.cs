using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CastleMapGenerator : MapGenerator 
{
    public GameObject _RoomPrefab;

    //생성된 방을 한데 모아두기 위한 용도.
    public Transform _RoomsTransform;

    public int radius;

    public int _MaxRoomNum;

    public int _MinRoomWidth;
    public int _MaxRoomWidth;

    public int _MinRoomHeight;
    public int _MaxRoomHeight;

    public int _MainRoomWidthLimit;
    public int _MainRoomHeightLimit;

    List<Room> _RoomList;

    //메인 방들을 연결한 최소 신장 트리
    List<RoomEdge> _MST;

    IEnumerator Generate_Internal()
    {
        _RoomList = CreateRooms();

        while(true)
        {
            if(IsSeparationComplete())
            {
                break;
            }
            yield return null;
        }

        var mainRoomList = GetMainRooms();

        _MST = MakeMST(mainRoomList);
    }

    //랜덤한 범위내에서 방을 생성
    List<Room> CreateRooms()
    {
        List<Room> roomList = new List<Room>();

        for (int i = 0; i < _MaxRoomNum; ++i)
        {
            var pos = Random.insideUnitSphere * radius;
            pos.z = 0f;

            int width = Random.Range(_MinRoomWidth, _MaxRoomWidth);
            int height = Random.Range(_MinRoomHeight, _MaxRoomHeight);

            var roomGO = Instantiate(_RoomPrefab) as GameObject;
            var room = roomGO.GetComponent<Room>();
            room.Init(new Position((int)pos.x, (int)pos.y), width, height);
            room.transform.SetParent(_RoomsTransform);

            roomList.Add(room);
        }

        return roomList;
    }

    //겹친 방이 없는지 조사 (방 분할이 완료되었는지 파악)
    bool IsSeparationComplete()
    {
        for (int i = 0; i < _RoomList.Count - 1; ++i)
        {
            var roomA = _RoomList[i];
            for (int j = i + 1; j < _RoomList.Count; ++j)
            {
                var roomB = _RoomList[j];

                if (roomA.CachedBoxCollider.bounds.Intersects(roomB.CachedBoxCollider.bounds))
                {
                    return false;
                }
            }
        }

        return true;
    }

    //조건에 맞는 메인 방들을 구함
    List<Room> GetMainRooms()
    {
        var mainRoomList = new List<Room>();

        foreach (var room in _RoomList)
        {
            if (room.Width >= _MainRoomWidthLimit && room.Height >= _MainRoomHeightLimit)
            {
                mainRoomList.Add(room);
            }
        }

        return mainRoomList;
    }

    //메인 방들을 연결한 최소 신장 트리를 구함.
    List<RoomEdge> MakeMST(List<Room> mainRoomList)
    {
        List<RoomVertex> roomVertexList = new List<RoomVertex>();
        List<RoomEdge> roomEdgeList = new List<RoomEdge>();

        //정점을 만듬
        for (int i = 0; i < mainRoomList.Count; ++i)
        {
            var vertex = new RoomVertex(mainRoomList[i]);
            roomVertexList.Add(vertex);
        }

        //모든 정점을 간선으로 연결
        for (int i = 0; i < roomVertexList.Count - 1; ++i)
        {
            for (int j = i + 1; j < roomVertexList.Count; ++j)
            {
                RoomVertex vertexA = roomVertexList[i];
                RoomVertex vertexB = roomVertexList[j];

                float weight = (mainRoomList[i].CachedTransform.position - mainRoomList[j].CachedTransform.position).sqrMagnitude;
                RoomEdge edge = new RoomEdge(vertexA, vertexB, (int)weight);

                roomEdgeList.Add(edge);
            }
        }

        //그래프로 만든뒤 MST를 구한다.
        RoomGraph graph = new RoomGraph(roomVertexList, roomEdgeList);
        return graph.GetMST();
    }

    //간선들을 복도로 연결
    void MakeCorridors(List<RoomEdge> edgeList)
    {
        //복도로 연결
        //가로 혹은 세로로 한번에 그어서 닿을 수 있으면 그렇게 함.
        //그게 안되면 ㄱ,ㄴ자로 그음

        foreach(var edge in edgeList)
        {
            var roomA = edge.A.Value;
            var roomB = edge.B.Value;

            //복도를 세로로 그을 수 있으면
            if (roomA.Min.x <= roomB.Max.x && roomB.Max.x <= roomA.Max.x)
            {
                int randomX = Random.Range(roomA.Min.x, roomB.Max.x);
                //A부터 출발할거임
                //int dirY = (roomA.Min.y )
            }
            else if (roomB.Min.x <= roomA.Max.x && roomA.Max.x <= roomB.Max.x)
            {

            }
        }
    }

    void Start()
    {
        Generate();
    }

    void Update()
    {
        if(_MST != null)
        {
            foreach(var edge in _MST)
            {
                Debug.DrawLine(edge.A.Value.CachedTransform.position, edge.B.Value.CachedTransform.position, Color.red);
            }
        }

    }

	public override void Generate()
    {
        Debug.Assert(_MainRoomWidthLimit <= _MaxRoomWidth);
        Debug.Assert(_MainRoomHeightLimit <= _MaxRoomHeight);

        StartCoroutine(Generate_Internal());
    }
}
