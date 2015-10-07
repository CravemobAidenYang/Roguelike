using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CastleMapGenerator : MapGenerator 
{
    public GameObject _RoomPrefab;
    public GameObject _TilePrefab;

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

    IEnumerator Generate_Internal(VoidCallback callback)
    {
        _RoomList = CreateRooms();

        Time.timeScale = 100f;
        while(true)
        {
            if(IsSeparationComplete())
            {
                break;
            }
            yield return null;
        }
        Time.timeScale = 1f;

        Size tileMapSize = AdjustRoomPos(_RoomList);

        var mainRoomList = GetMainRooms(_RoomList);

        CreateTiles(tileMapSize, _RoomList);

        _MST = GetMST(mainRoomList);

        CreateCorridors(_MST);

        callback();
    }

    //랜덤한 범위내에서 방을 생성
    List<Room> CreateRooms()
    {
        List<Room> roomList = new List<Room>();

        for (int i = 0; i < _MaxRoomNum; ++i)
        {
            var pos = Random.insideUnitSphere * radius;
            pos.z = 0f;

            int width = Random.Range(_MinRoomWidth, _MaxRoomWidth + 1);
            int height = Random.Range(_MinRoomHeight, _MaxRoomHeight + 1);

            var roomGO = Instantiate(_RoomPrefab) as GameObject;
            var room = roomGO.GetComponent<Room>();
            room.Init(new Position((int)pos.x, (int)pos.y), width, height);
            room.transform.SetParent(_RoomsTransform);

            if(i == 0)
            {
                room.CachedRigidbody.isKinematic = true;
            }

            roomList.Add(room);
        }

        return roomList;
    }

    //방의 리지드바디의 슬립여부를 판단 (방 분할이 완료되었는지 파악)
    bool IsSeparationComplete()
    {
        for (int i = 0; i < _RoomList.Count; ++i)
        {
            var room = _RoomList[i];

            if(!room.CachedRigidbody.IsSleeping())
            {
                return false;
            }
        }

        return true;
    }

    //방을 원점으로 밀어낸다.
    //리턴값은 방을 밀어낸뒤의 전체방을 덮는 사이즈(타일맵의 크기)
    Size AdjustRoomPos(List<Room> roomList)
    {
        for (int i = 0; i < roomList.Count; ++i)
        {
            roomList[i].CachedRigidbody.isKinematic = true;
        }

        Room leftRoom = roomList[0],
            topRoom = roomList[0],
            rightRoom = roomList[0],
            bottomRoom = roomList[0];

        for(int i = 1; i < roomList.Count; ++i)
        {
            var room = roomList[i];
            if(room.Min.x < leftRoom.Min.x)
            {
                leftRoom = room;
            }
            if (room.Max.x > rightRoom.Max.x)
            {
                rightRoom = room;
            }
            if (room.Min.y < bottomRoom.Min.y)
            {
                bottomRoom = room;
            }
            if (room.Max.y > topRoom.Max.y)
            {
                topRoom = room;
            }
        }

        //테두리는 벽으로 두르기 위해 한칸을 더밀어냄
        Position dist = new Position(-leftRoom.Min.x + 1, -bottomRoom.Min.y + 1);

        foreach(var room in roomList)
        {
            room.Move(dist);
        }

        //바깥쪽도 한칸을 더 만듬
        return new Size(rightRoom.Min.x + rightRoom.Width + 1, topRoom.Min.y + topRoom.Height + 1);
    }

    //조건에 맞는 메인 방들을 구함
    List<Room> GetMainRooms(List<Room> roomList)
    {
        var mainRoomList = new List<Room>();

        foreach (var room in roomList)
        {
            if (room.Width >= _MainRoomWidthLimit && room.Height >= _MainRoomHeightLimit)
            {
                mainRoomList.Add(room);
            }
        }

        return mainRoomList;
    }

    //타일을 세팅함.
    void CreateTiles(Size tileMapSize, List<Room> roomList)
    {
        TileManager.Instance.CreateTileMap(tileMapSize);
        var tileMap = TileManager.Instance.GetTileMap();
        for (int x = 0; x < tileMapSize.width; ++x)
        {
            for (int y = 0; y < tileMapSize.height; ++y)
            {
                var tileGO = Instantiate(_TilePrefab) as GameObject;
                var tile = tileGO.GetComponent<Tile>();

                tile.Init(x, y, TileState.WALL);
                tileMap[x, y] = tile;
            }
        }

        foreach(var room in roomList)
        {
            for (int x = room.Min.x; x <= room.Max.x; ++x)
            {
                for (int y = room.Min.y; y <= room.Max.y; ++y)
                {
                    //if(x == room.Min.x || x == room.Max.x - 1 || y == room.Min.y || y == room.Max.y - 1)
                    //{
                    //    continue;
                    //}
                    //else
                    {
                        tileMap[x, y].State = TileState.GROUND;
                    }
                }
            }
        }
    }

    //메인 방들을 연결한 최소 신장 트리를 구함.
    List<RoomEdge> GetMST(List<Room> mainRoomList)
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

    //씬뷰에 선을 그리기 위한 용도 (디버그용).
#if DEBUG
    List<Vector2> startList = new List<Vector2>();
    List<Vector2> endList = new List<Vector2>();
#endif

    //간선들을 복도로 연결
    void CreateCorridors(List<RoomEdge> edgeList)
    {
        var tileMap = TileManager.Instance.GetTileMap();

        foreach(var edge in edgeList)
        {
            var roomA = edge.A.Value;
            var roomB = edge.B.Value;

            var startPos = new Vector2(Random.Range(roomA.Min.x, roomA.Max.x + 1),
                                        Random.Range(roomA.Min.y, roomA.Max.y + 1));
            var endPos = new Vector2(Random.Range(roomB.Min.x, roomB.Max.x + 1),
                                        Random.Range(roomB.Min.y, roomB.Max.y + 1));


            //각도를 구한다
            Vector2 dirVector = startPos - endPos;
            float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg;

            //간선과 일정거리 떨어진 또다른 선을 만든다.
            var startPosArr = new Vector2[3];
            var endPosArr = new Vector2[3];

            startPosArr[0] = startPos;
            endPosArr[0] = endPos;

            Vector2 gap = new Vector2(Mathf.Cos((angle + 90f) * Mathf.Deg2Rad), Mathf.Sin((angle + 90f) * Mathf.Deg2Rad)) * 0.1f;
            startPosArr[1] = startPos + gap;
            endPosArr[1] = endPos + gap;

            startPosArr[2] = startPos - gap;
            endPosArr[2] = endPos - gap;

#if DEBUG
            startList.AddRange(startPosArr);
            endList.AddRange(endPosArr);
#endif

            while (true)
            {
                bool clear = true;

                for (int i = 0; i < 3; ++i)
                {
                    var hitInfo = Physics2D.Linecast(startPosArr[i], endPosArr[i], 1 << LayerMask.NameToLayer("Tile"));
                    if (hitInfo.collider)
                    {
                        clear = false;
                        var tile = hitInfo.collider.gameObject.GetComponent<Tile>();
                        tile.State = TileState.GROUND;
                    }
                }
                if(clear)
                {
                    break;
                }
            }
        }
    }

    void Start()
    {
        //Generate();
    }

    void Update()
    {
#if DEBUG
        if (_MST != null)
        {
            foreach (var edge in _MST)
            {
                Debug.DrawLine(edge.A.Value.CachedTransform.position, edge.B.Value.CachedTransform.position, Color.red);
            }
        }

        if (startList != null && endList != null)
            for (int i = 0; i < startList.Count; ++i)
            {
                Debug.DrawLine(startList[i], endList[i], Color.magenta);
            }
#endif
    }

	public override void Generate(VoidCallback callback)
    {
        Debug.Assert(_MainRoomWidthLimit <= _MaxRoomWidth);
        Debug.Assert(_MainRoomHeightLimit <= _MaxRoomHeight);

        StartCoroutine(Generate_Internal(callback));
    }
}
