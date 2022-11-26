using System.Collections.Generic;

using OpenTK;

namespace Taco.Classes
{
    class SolarSystemPathFinder
    {

        protected int Size;
        protected Dictionary<int, bool> IsBlocked;
        private SolarSystemNode _startNode;

        public SolarSystemNode StartNode
        {
            get
            {
                return _startNode;
            }
        }

        private SolarSystemData[] m_vSolarSystemData;


        public SolarSystemPathFinder(SolarSystemData[] systemData)
        {
            Size = systemData.Length;
            IsBlocked = new Dictionary<int, bool>(Size);
            m_vSolarSystemData = systemData;
        }
 
        protected double GetSquaredDistance(Vector2 start, Vector2 end)
        {
            Vector2 temp = end - start;
            return temp.LengthSquared;
        }

        public void SetBlocked(int index, bool value)
        {
            IsBlocked[index] = value;
        }

        public void SetBlocked(int index) { SetBlocked(index, true); }


        public PathInfo FindPath(int start, int end)
        {         
            return FindPathReversed(end, start);
        }
 
        private PathInfo FindPathReversed(int startingSystemID, int endSystemID)
        {
            var startSystem = System.Array.Find(m_vSolarSystemData, delegate (SolarSystemData s)
            {
                return startingSystemID == s.NativeId;
            });
            _startNode = new SolarSystemNode(startingSystemID, new Vector2((float)startSystem.X, (float)startSystem.Y), 0, 0, startSystem.ConnectedTo, null);

            HeapPriorityQueue<SolarSystemNode> openList = new HeapPriorityQueue<SolarSystemNode>(Size);
            openList.Enqueue(_startNode, 0);

            Dictionary<int, bool> brWorld = new Dictionary<int, bool>();
            brWorld[startingSystemID] = true;
//             bool[] brWorld = new bool[Size];
//             brWorld[start] = true;

            //Vector2 endPosition = new Vector2((float)m_vSolarSystemData[end].X, (float)m_vSolarSystemData[end].Y);
 
            while (openList.Count != 0)
            {
                SolarSystemNode current = openList.Dequeue();

                if (current.SystemId == endSystemID)
                    return GetPathInfo(current);
                //return new SolarSystemNode(end, endPosition, current.PathCost + 1, current.Cost + 1, m_vSolarSystemData[end].ConnectedTo, current);

                SolarSystemConnectionData[] surrounding = current.ConnectTo;

                if (surrounding != null)
                {
                    foreach (var surroundingSystem in surrounding)
                    {
                        var targetSystemId = surroundingSystem.ToSystemNativeId;
                        var targetSystem = System.Array.Find(m_vSolarSystemData, delegate (SolarSystemData s)
                        {
                            return targetSystemId == s.NativeId;
                        });
                        var tmp = new Vector2((float)targetSystem.X, (float)targetSystem.Y);
                        var brWorldIdx = targetSystemId;

                        var bWorldIdx = false;
                        if (brWorld.ContainsKey(brWorldIdx))
                        {
                            bWorldIdx = brWorld[brWorldIdx];
                        }
                        if (!PositionIsFree(brWorldIdx) || bWorldIdx == true)
                            continue;
                        brWorld[brWorldIdx] = true;

                        var pathCost = current.PathCost;
                        var cost = pathCost + 1;
                        var node = new SolarSystemNode(targetSystemId, tmp, cost, pathCost, targetSystem.ConnectedTo, current);
                        openList.Enqueue(node, cost);
                    }
                }
                else if (openList.Count == 0)
                {
                    return GetEmptyPath(_startNode.SystemId);
                }
            }
            return GetEmptyPath(_startNode.SystemId); //no path found
        }

        private PathInfo GetEmptyPath(int startingSystemId)
        {
            var tempInfo = new PathInfo
            {
                TotalJumps = 0,
                PathSystems = new[] {startingSystemId}
            };


            return tempInfo;
        }

        private PathInfo GetPathInfo(SolarSystemNode endNode)
        {
            var tempInfo = new PathInfo();

            var tempNode = endNode;

            var pathSystems = new List<int>();

            var i = 0;

            while (tempNode.HasParent)
            {
                pathSystems.Add(tempNode.SystemId);
                tempNode = tempNode.Parent;
                i++;
            }
            i++;

            var jumpCount = i;

            var pathIDs = new int[jumpCount + 1];

            i = 0;
            foreach (var tempId in pathSystems)
            {
                pathIDs[i] = tempId;
                i++;
            }
            i++;
            pathIDs[i] = _startNode.SystemId;

            tempInfo.PathSystems = pathIDs;
            tempInfo.TotalJumps = jumpCount;

            return tempInfo;
        }
 
        private bool PositionIsFree(int index)
        {
            if (IsBlocked.ContainsKey(index))
            {
                return !IsBlocked[index];
            }
            return true;
        }
    }
}
