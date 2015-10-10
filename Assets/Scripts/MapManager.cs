//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace PathFind
//{

//    public enum TileState { BEFORE_INQUIRY, OPENED, CLOSED, WALL };

//    class Tile : IComparable<Tile>
//    {
//        public Tile parent;

//        public TileState state;
//        public int col, row;
//        public int gCost;
//        public int hCost;
//        public int fCost
//        {
//            get
//            {
//                return gCost + hCost;
//            }
//        }

//        public Tile(int col, int row)
//        {
//            this.col = col;
//            this.row = row;

//            parent = null;
//            gCost = hCost = 0;
//            state = TileState.BEFORE_INQUIRY;
//        }

//        public void Reset()
//        {
//            state = TileState.BEFORE_INQUIRY;
//            gCost = hCost = 0;
//            parent = null;
//        }

//        public bool IsNonWalkable()
//        {
//            return (state == TileState.CLOSED || state == TileState.WALL);
//        }

//        public int CompareTo(Tile tile)
//        {
//            return this.fCost - tile.fCost;
//        }
//    }


//    public class MapManager : MonoBehaviour
//    {
//        public GameObject obstaclesParent;
//        public bool showGrid = true;

//        Transform cachedTransform;
//        List<BoxCollider2D> obstacleColliders = new List<BoxCollider2D>();
//        const int TILE_SIZE = 21;
//        const int MAP_IMAGE_SIZE = 2562;

//        const int TILE_COL = MAP_IMAGE_SIZE / TILE_SIZE, TILE_ROW = MAP_IMAGE_SIZE / TILE_SIZE;
//        Tile[,] tiles = new Tile[TILE_COL, TILE_ROW];

//        const int DIAGONAL_G_COST = 14;
//        const int NON_DIAGONAL_G_COST = 10;

//        List<Tile> openTileList = new List<Tile>();

//        public bool InTheSameTile(Vector3 a, Vector3 b)
//        {
//            a.x -= cachedTransform.position.x;
//            a.y -= cachedTransform.position.y;
//            b.x -= cachedTransform.position.x;
//            b.y -= cachedTransform.position.y;

//            int aRow = Mathf.Clamp((int)a.x / TILE_SIZE, 0, TILE_COL - 1);
//            int aCol = Mathf.Clamp((int)a.y / TILE_SIZE, 0, TILE_ROW - 1);
//            int bRow = Mathf.Clamp((int)b.x / TILE_SIZE, 0, TILE_COL - 1);
//            int bCol = Mathf.Clamp((int)b.y / TILE_SIZE, 0, TILE_ROW - 1);


//            return (aRow == bRow && aCol == bCol);
//        }

//        public void SetTileState(int col, int row, TileState state)
//        {
//            tiles[col, row].state = state;
//        }


//        public void GetPath(Stack<Vector2> outStack, Vector2 startPos, Vector2 targetPos)
//        {
//            MapReset();

//            startPos.x -= cachedTransform.position.x;
//            startPos.y -= cachedTransform.position.y;
//            targetPos.x -= cachedTransform.position.x;
//            targetPos.y -= cachedTransform.position.y;

//            int startCol = Mathf.Clamp((int)startPos.x / TILE_SIZE, 0, TILE_COL - 1);
//            int startRow = Mathf.Clamp((int)startPos.y / TILE_SIZE, 0, TILE_ROW - 1);
//            int targetCol = Mathf.Clamp((int)targetPos.x / TILE_SIZE, 0, TILE_COL - 1);
//            int targetRow = Mathf.Clamp((int)targetPos.y / TILE_SIZE, 0, TILE_ROW - 1);

//            Tile startTile = tiles[startCol, startRow];
//            Tile targetTile = tiles[targetCol, targetRow];

//            startTile.state = TileState.OPENED;
//            openTileList.Add(startTile);

//            while (openTileList.Count > 0)
//            {
//                Tile curTile = openTileList[0];

//                if (curTile == targetTile)
//                {
//                    for (Tile tile = curTile; (tile != null && tile.parent != null); tile = tile.parent)
//                    {
//                        Vector2 pos = (Vector2.right * tile.col + Vector2.up * tile.row) * TILE_SIZE;
//                        pos.x += cachedTransform.position.x;
//                        pos.y += cachedTransform.position.y;
//                        outStack.Push(pos);
//                    }
//                    return;
//                }

//                openTileList.Remove(curTile);
//                curTile.state = TileState.CLOSED;

//                SearchNeighborTile(curTile, targetTile);
//            }
//        }

//        인접한 타일을 검사하여 g,h,f값 계산, 부모 지정, 열린노드지정 처리
//        void SearchNeighborTile(Tile stdTile, Tile targetTile)
//        {
//            int minCol = Mathf.Clamp(stdTile.col - 1, 0, TILE_COL - 1);
//            int maxCol = Mathf.Clamp(stdTile.col + 1, 0, TILE_COL - 1);
//            int minRow = Mathf.Clamp(stdTile.row - 1, 0, TILE_ROW - 1);
//            int maxRow = Mathf.Clamp(stdTile.row + 1, 0, TILE_ROW - 1);

//            인접한 타일들을 검사한다.
//            for (int col = minCol; col <= maxCol; ++col)
//            {
//                for (int row = minRow; row <= maxRow; ++row)
//                {
//                    인접한 타일을 현재 타일로 설정
//                    Tile curTile = tiles[col, row];

//                    만약 현재 타일이 기준 타일과 같거나 이동 불가 타일이라면
//                    if ((col == stdTile.col && row == stdTile.row) || curTile.IsNonWalkable())
//                    {
//                        현재 타일 검사를 생략한다.
//                        continue;
//                    }

//                    임시로 기준 타일을 통해 이동했을 때의 g값을 계산한다.
//                    int tempGCost = stdTile.gCost;
//                    tempGCost += (stdTile.col - col != 0 && stdTile.row - row != 0) ? DIAGONAL_G_COST : NON_DIAGONAL_G_COST;

//                    h값 계산을 위한 가로 길이 계산
//                    int widthDist;
//                    if (targetTile.col > curTile.col)
//                    {
//                        widthDist = targetTile.col - curTile.col;
//                    }
//                    else
//                    {
//                        widthDist = curTile.col - targetTile.col;
//                    }

//                    h값 계산을 위한 세로 길이 계산
//                    int heightDist;
//                    if (targetTile.row > curTile.row)
//                    {
//                        heightDist = targetTile.row - curTile.row;
//                    }
//                    else
//                    {
//                        heightDist = curTile.row - targetTile.row;
//                    }


//                    int hCost = (widthDist + heightDist) * NON_DIAGONAL_G_COST;
//                    curTile.hCost = hCost;

//                    switch (curTile.state)
//                    {
//                        타일이 검사 전 상태라면 현재 타일을 오픈 리스트에 추가한 뒤, 현재 타일의 g값을 임시 g값으로 한다.
//                        case TileState.BEFORE_INQUIRY:
//                            openTileList.Add(curTile);
//                            curTile.state = TileState.OPENED;
//                            curTile.parent = stdTile;
//                            curTile.gCost = tempGCost;

//                            break;

//                        이미 열린 타일이라면 임시 g값과 기준 노드의 g값을 비교하여 임시 g값이 더 작으면 
//                        기준 노드를 통해 가는게 더 좋은 길이라는 의미이므로 현재 타일의 g값과 부모를 재설정한다.
//                        case TileState.OPENED:
//                            if (/*curTile.parent != stdTile ||*/ tempGCost < curTile.gCost)
//                            {
//                                curTile.gCost = tempGCost;
//                                curTile.parent = stdTile;
//                            }
//                            break;
//                    }
//                }
//            }

//            openTileList.Sort();
//        }

//        void MapInit()
//        {
//            타일 전체를 탐색 전으로 초기화하고
//            for (int tileIdx = 0; tileIdx < TILE_COL * TILE_ROW; ++tileIdx)
//            {
//                int col = tileIdx % TILE_COL;
//                int row = tileIdx / TILE_COL;

//                tiles[col, row] = new Tile(col, row);
//            }

//            벽을 불러온다.
//            Transform obsParentTransform = obstaclesParent.transform;
//            int childCnt = obsParentTransform.childCount;

//            for (int childIdx = 0; childIdx < childCnt; ++childIdx)
//            {
//                obstacleColliders.Add(obsParentTransform.GetChild(childIdx).gameObject.GetComponent<BoxCollider2D>());
//            }

//            벽에 포함되는 타일들을 벽으로 설정한다.
//            for (int obsIdx = 0; obsIdx < obstacleColliders.Count; ++obsIdx)
//            {
//                Bounds bounds = obstacleColliders[obsIdx].bounds;
//                Vector3 min = bounds.min;
//                Vector3 max = bounds.max;

//                min.x -= cachedTransform.position.x;
//                min.y -= cachedTransform.position.y;
//                max.x -= cachedTransform.position.x;
//                max.y -= cachedTransform.position.y;

//                int minCol = Mathf.Clamp((int)min.x / TILE_SIZE, 0, TILE_COL - 1);
//                int maxCol = Mathf.Clamp((int)max.x / TILE_SIZE, 0, TILE_COL - 1);
//                int minRow = Mathf.Clamp((int)min.y / TILE_SIZE, 0, TILE_ROW - 1);
//                int maxRow = Mathf.Clamp((int)max.y / TILE_SIZE, 0, TILE_ROW - 1);

//                for (int col = minCol; col <= maxCol; ++col)
//                {
//                    for (int row = minRow; row <= maxRow; ++row)
//                    {
//                        tiles[col, row].state = TileState.WALL;
//                    }
//                }
//            }
//        }

//        void MapReset()
//        {
//            openTileList.Clear();

//            벽이 아닌 타일들을 모두 조사 전 상태로 되돌린다.
//            for (int tileIdx = 0; tileIdx < TILE_COL * TILE_ROW; ++tileIdx)
//            {
//                int col = tileIdx % TILE_COL;
//                int row = tileIdx / TILE_COL;

//                if (tiles[col, row].state != TileState.WALL)
//                {
//                    tiles[col, row].Reset();

//                }
//            }
//        }

//        void Awake()
//        {
//            cachedTransform = base.transform;
//            MapInit();
//        }

//        void OnDrawGizmos()
//        {
//            if (!showGrid)
//                return;

//            for (int tileIdx = 0; tileIdx < TILE_COL * TILE_ROW; ++tileIdx)
//            {
//                int col = tileIdx % TILE_COL;
//                int row = tileIdx / TILE_COL;

//                if (tiles[col, row] != null)
//                {
//                    Vector3 pos = (Vector3.right * col + Vector3.up * row) * TILE_SIZE;

//                    Gizmos.color = tiles[col, row].IsNonWalkable() ? Color.red :
//                        tiles[col, row].state == TileState.OPENED ? Color.green :
//                                                                    Color.white;

//                    Vector3 halfSize = Vector3.one * ((float)TILE_SIZE * 0.5f);

//                    Gizmos.DrawCube(cachedTransform.position + pos + halfSize, halfSize);
//                }
//            }
//        }
//    }
//}