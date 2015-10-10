//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace PathFind
//{

//    public delegate void MoveDoneCallback();


//    public class PathFinder : MonoBehaviour
//    {
//        public MapManager mapManager;

//        public bool isMoving
//        {
//            get;
//            private set;
//        }

//        Transform cachedTransform;

//        float moveSpeed = 0f;

//        float orgDegAngle = 0f;

//        bool startFollowPathCoroutine = false;

//        MoveDoneCallback moveDoneCallback;

//        public void SetDestination(MapManager mapManager, Vector2 targetPos, float moveSpeed, float orgDegAngle, MoveDoneCallback callback = null)
//        {
//            this.mapManager = mapManager;
//            isMoving = true;

//            movePosStack.Clear();
//            mapManager.GetPath(movePosStack, cachedTransform.position, targetPos);

//            this.moveSpeed = moveSpeed;
//            this.orgDegAngle = orgDegAngle;
//            this.moveDoneCallback = callback;

//            if (!startFollowPathCoroutine)
//            {
//                StartCoroutine(FollowPath());
//                startFollowPathCoroutine = true;
//            }
//        }

//        public Stack<Position> GetPath(Position targetPos)
//        {
//            var movePosStack = new Stack<Vector2>();

//            return mapManager.GetPath(movePosStack);
//        }

//        void Awake()
//        {
//            cachedTransform = base.transform;
//            isMoving = false;
//        }
//    }
//}