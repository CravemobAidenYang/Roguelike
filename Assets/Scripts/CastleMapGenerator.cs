using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CastleMapGenerator : MapGenerator 
{
    //static CastleMapGenerator _Instance;

    public static CastleMapGenerator Instance
    {
        get;
        private set;
    }

    public GameObject _RoomPrefab;
    //public GameObject _TilePrefab;

    //생성된 방을 한데 모아두기 위한 용도.
    public Transform _RoomsTransform;

    public int radius;

    public int _MinRoomNum;
    public int _MaxRoomNum;

    public int _MinRoomWidth;
    public int _MaxRoomWidth;

    public int _MinRoomHeight;
    public int _MaxRoomHeight;

    public int _MainRoomWidthLimit;
    public int _MainRoomHeightLimit;

    public int _MinRoomMonsterNum;
    public int _MaxRoomMonsterNum;

    public int _MinBossRoomMonsterNum;
    public int _MaxBossRoomMonsterNum;

    public int _MinRoomFoodNum, _MaxRoomFoodNum;

    List<Room> _RoomList, _MainRoomList;

    //플레이어가 시작하는 방과 보스방
    Room _PlayerRoom, _BossRoom;

    public Room PlayerRoom
    {
        get
        {
            return _PlayerRoom;
        }
    }

    public Room BossRoom
    {
        get
        {
            return _BossRoom;
        }
    }

    //메인 방들을 연결한 최소 신장 트리
    List<RoomEdge> _MST;

    IEnumerator Generate_Internal(VoidCallback callback)
    {
        _RoomList = CreateRooms();

        Time.timeScale = 100f;
        while(true)
        {
            if (IsSeparationComplete(_RoomList))
            {
                break;
            }
            yield return null;
        }
        Time.timeScale = 1f;

        Size tileMapSize = AdjustRoomPos(_RoomList);

        _MainRoomList = GetMainRooms(_RoomList);

        CreateTiles(tileMapSize, _RoomList);

        _MST = GetMST(_MainRoomList);

        CreateCorridors(_MST);

        CreateMonsters(_MainRoomList);

        callback();

        CreateFoods(_MainRoomList);

    }

    //랜덤한 범위내에서 방을 생성
    List<Room> CreateRooms()
    {
        List<Room> roomList = new List<Room>();

        int roomNum = Random.Range(_MinRoomNum, _MaxRoomNum + 1);
        for (int i = 0; i < roomNum; ++i)
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
    bool IsSeparationComplete(List<Room> roomList)
    {
        for (int i = 0; i < roomList.Count; ++i)
        {
            var room = roomList[i];

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

        foreach(var room in roomList)
        {
            for (int x = room.Min.x; x <= room.Max.x; ++x)
            {
                for (int y = room.Min.y; y <= room.Max.y; ++y)
                {
                    tileMap[x, y].SetState(TileState.GROUND);
                    //tileMap[x, y].State = TileState.GROUND;
                    //tileMap[x, y].Invalidate();
                }
            }
        }
    }

    //메인 방들을 연결한 최소 신장 트리를 구하고 보스방과 플레이어방을 선택.
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
        var mst =  graph.GetMST();

        //플레이어 방과 보스방을고름
        SelectPlayerRoomAndBossRoom(mst, roomVertexList);

        return mst;
    }

    //씬뷰에 선을 그리기 위한 용도 (디버그용).
#if DEBUG
    List<Vector2> startList = new List<Vector2>();
    List<Vector2> endList = new List<Vector2>();
#endif

    //간선들을 복도로 연결
    void CreateCorridors(List<RoomEdge> edgeList)
    {

        foreach(var edge in edgeList)
        {
            var roomA = edge.A.Value;
            var roomB = edge.B.Value;

            Vector2 startPos, endPos;
            Vector2[] startPosArr = new Vector2[3];
            Vector2[] endPosArr = new Vector2[3];

            while (true)
            {
                startPos.x = Random.Range(roomA.Min.x, roomA.Max.x + 1);
                startPos.y = Random.Range(roomA.Min.y, roomA.Max.y + 1);
                endPos.x = Random.Range(roomB.Min.x, roomB.Max.x + 1);
                endPos.y = Random.Range(roomB.Min.y, roomB.Max.y + 1);

                //각도를 구한다
                Vector2 dirVector = startPos - endPos;
                float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg;

                //간선과 일정거리 떨어진 또다른 선을 만든다.
                startPosArr[0] = startPos;
                endPosArr[0] = endPos;

                Vector2 gap = new Vector2(Mathf.Cos((angle + 90f) * Mathf.Deg2Rad), Mathf.Sin((angle + 90f) * Mathf.Deg2Rad)) * 0.1f;
                startPosArr[1] = startPos + gap;
                endPosArr[1] = endPos + gap;

                startPosArr[2] = startPos - gap;
                endPosArr[2] = endPos - gap;

                //먼저 엉뚱한 방과 연결되진 않는지 검사하고 그렇다면 다시 새로운 선을 긋는다.

                bool wrongWay = false;
                for (int i = 0; i < 3; ++i)
                {
                    var hitInfoArr = Physics2D.LinecastAll(startPosArr[i], endPosArr[i], 1 << LayerMask.NameToLayer("Room"));

                    if(IsConnectToWrongRoom(hitInfoArr, roomA, roomB))
                    {
                        wrongWay = true;
                        break;
                    }
                }

                if(!wrongWay)
                {
                    break;
                }
            }
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
                    if (hitInfo.collider != null)
                    {
                        clear = false;
                        var tile = hitInfo.collider.gameObject.GetComponent<Tile>();
                        tile.SetState(TileState.GROUND);
                        //tile.State = TileState.GROUND;
                        //tile.Invalidate();
                    }
                }
                if(clear)
                {
                    break;
                }
            }
        }
    }

    //A와 B가 아닌 방과 레이가 충돌했는지를 판단
    bool IsConnectToWrongRoom(RaycastHit2D[] hitInfoArr, Room roomA, Room roomB)
    {
        for (int i = 0; i < hitInfoArr.Length; ++i)
        {
            var hitInfo = hitInfoArr[i];

            if (hitInfo.collider != null)
            {
                var room = hitInfo.collider.gameObject.GetComponent<Room>();
                if (room != roomA && room != roomB)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void SelectPlayerRoomAndBossRoom(List<RoomEdge> mst, List<RoomVertex> vertexList)
    {
        //최소신장트리가 주어졌을때 말단에 위치한 정점들을 찾는다.
        //그래프를 탐색하며 말단에서 말단끼리 가는 깊이와 가중치를 계산한다.
        //가장 깊이가 깊고 가중치가 높은 간선에 연결된 방을 시작방과 보스방으로둔다.

        var visitedEdgeList = new List<RoomEdge>();
        var leafList = new List<RoomVertex>();
        
        //말단 노드를 찾는다.
        foreach(var vertex in vertexList)
        {
            int edgeCount = 0;
            foreach(var edge in mst)
            {
                if(edge.A == vertex || edge.B == vertex)
                {
                    ++edgeCount;
                }
            }
            if(edgeCount == 1)
            {
                leafList.Add(vertex);
            }
        }

        WeightInfo maxWeightInfo = new WeightInfo(0, 0);

        for (int i = 0; i < leafList.Count; ++i)
        {
            var vertexA = leafList[i];
            for (int j = 0; j < leafList.Count; ++j)
            {
                var vertexB = leafList[j];

                if (vertexA == vertexB)
                {
                    continue;
                }

                WeightInfo? weightInfo = StartSearch(mst, visitedEdgeList, vertexA, vertexB);

                if(weightInfo != null)
                {
                    //뎁스를 우선적으로 판별함
                    if (weightInfo.Value.totalDepth > maxWeightInfo.totalDepth)
                    {
                        maxWeightInfo.totalDepth = weightInfo.Value.totalDepth;

                        if ((vertexA.Value.Width + vertexA.Value.Height) > (vertexB.Value.Width + vertexB.Value.Height))
                        {
                            _BossRoom = vertexA.Value;
                            _PlayerRoom = vertexB.Value;
                        }
                        else
                        {
                            _BossRoom = vertexB.Value;
                            _PlayerRoom = vertexA.Value;
                        }

                        print("max depth : " + maxWeightInfo.totalDepth);
                    }
                    //뎁스가 같으면 가중치를 봄
                    else if (weightInfo.Value.totalDepth == maxWeightInfo.totalDepth)
                    {
                        if(weightInfo.Value.totalWeight > maxWeightInfo.totalDepth)
                        {
                            if ((vertexA.Value.Width + vertexA.Value.Height) > (vertexB.Value.Width + vertexB.Value.Height))
                            {
                                _BossRoom = vertexA.Value;
                                _PlayerRoom = vertexB.Value;
                            }
                            else
                            {
                                _BossRoom = vertexB.Value;
                                _PlayerRoom = vertexA.Value;
                            }
                        }
                    }
                }

                //재탐색을 위해 초기화한다.
                visitedEdgeList.Clear();
            }
        }
    }

    //방 사이의 간선의 깊이와 비용 합산 정보를 담아둘 용도
    struct WeightInfo
    {
        public int totalDepth;
        public float totalWeight;

        public WeightInfo(int totalDepth, float totalWeight)
        {
            this.totalDepth = totalDepth;
            this.totalWeight = totalWeight;
        }
    }

    //리턴값이 널이면 못찾은거
    WeightInfo? StartSearch(List<RoomEdge> edgeList, List<RoomEdge> visitedEdgeList, RoomVertex startVertex, RoomVertex endVertex)
    {
        WeightInfo weightInfo = new WeightInfo(0, 0);

        if(startVertex == endVertex)
        {
            return weightInfo;
        }

        var connectedEdgeList = new List<RoomEdge>();
        //연결된 간선을 찾아온다
        foreach (var edge in edgeList)
        {
            if (edge.A == startVertex || edge.B == startVertex)
            {
                connectedEdgeList.Add(edge);
            }
        }

        foreach (var edge in connectedEdgeList)
        {
            if(visitedEdgeList.Contains(edge))
            {
                continue;
            }
            else
            {
                weightInfo.totalDepth = 1;
                weightInfo.totalWeight = edge.Weight;
                var anotherVertex = (startVertex == edge.A) ? edge.B : edge.A;
                visitedEdgeList.Add(edge);

                if(anotherVertex == endVertex)
                {
                    return weightInfo;
                }

                var foundWeightInfo = StartSearch(edgeList, visitedEdgeList, anotherVertex, endVertex);
                if (foundWeightInfo != null)
                {
                    weightInfo.totalDepth += foundWeightInfo.Value.totalDepth;
                    weightInfo.totalWeight += foundWeightInfo.Value.totalWeight;

                    return weightInfo;
                }
            }
        }
        return null;
    }

    void CreateMonsters(List<Room> roomList)
    {
        int bossRoomMonsterNum = Random.Range(_MinBossRoomMonsterNum, _MaxBossRoomMonsterNum + 1);

        for (int i = 0; i < bossRoomMonsterNum; ++i)
        {
            var pos = _BossRoom.GetGroundPosInRoom();
            if(pos != null)
            {
                var monster = MonsterManager.Instance.CreateMonster(0);
                monster.Init(0, pos.Value);
            }
        }


        foreach (var room in roomList)
        {
            if(room == _BossRoom || room == _PlayerRoom)
            {
                continue;
            }

            int monsterNum = Random.Range(_MinRoomMonsterNum, _MaxRoomMonsterNum + 1);

            for (int i = 0; i < monsterNum; ++i)
            {
                var pos = room.GetGroundPosInRoom();
                if (pos != null)
                {
                    var monster = MonsterManager.Instance.CreateMonster(0);
                    monster.Init(0, pos.Value);
                }
            }
        }
    }

    void CreateFoods(List<Room> roomList)
    {
        foreach (var room in roomList)
        {
            int roomFoodNum = Random.Range(_MinRoomFoodNum, _MaxRoomFoodNum + 1);
            for (int i = 0; i < roomFoodNum; ++i)
            {
                var pos = room.GetGroundPosInRoom();
                if (pos != null)
                {
                    if (FoodManager.Instance.GetFoodFromPos(pos.Value) != null)
                    {
                        i--;
                        continue;
                    }
                    var food = FoodManager.Instance.CreateFood();
                    food.Init(pos.Value);
                }
            }
        }
    }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
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
        {
            for (int i = 0; i < startList.Count; ++i)
            {
                Debug.DrawLine(startList[i], endList[i], Color.magenta);
            }
        }

        if (_PlayerRoom != null && _BossRoom != null)
        {
            Debug.DrawLine(_PlayerRoom.CachedTransform.position, _BossRoom.CachedTransform.position, Color.yellow);
        }

        if (!GameManager.Instance.IsPause && Input.GetKeyDown(KeyCode.F5))
        {
            Reset();
        }
#endif
    }

	public override void Generate(VoidCallback callback)
    {
        Debug.Assert(_MainRoomWidthLimit <= _MaxRoomWidth);
        Debug.Assert(_MainRoomHeightLimit <= _MaxRoomHeight);

        TileManager.Instance.DestroyTileMap();
        Cleanup();

        StartCoroutine(Generate_Internal(callback));
    }

    public void Reset()
    {
        MonsterManager.Instance.Cleanup();
        FoodManager.Instance.Cleanup();
        Player.Instance.gameObject.SetActive(false);
        Generate(() =>
        {
            Player.Instance.Init();
            Player.Instance.gameObject.SetActive(true);
        });
    }

    void Cleanup()
    {
        if (_MainRoomList != null)
        {
            for (int i = 0; i < _MainRoomList.Count; ++i)
            {
                if (_MainRoomList[i] != null)
                {
                    Destroy(_MainRoomList[i].gameObject);
                }
                _MainRoomList[i] = null;
            }
        }
        _MainRoomList = null;

        if(_MST != null)
        {
            for (int i = 0; i < _MST.Count; ++i)
            {
                _MST[i] = null;
            }
        }
        _MST = null;

        startList.Clear();
        endList.Clear();

        _PlayerRoom = null;
        _BossRoom = null;

        System.GC.Collect();
    }

}
