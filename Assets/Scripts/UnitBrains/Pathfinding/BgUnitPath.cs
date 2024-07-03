using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class BgUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, -1, 0, 1 };
        private Vector2Int _startPoint;
        private Vector2Int _endPoint;
        private const int MaxLength = 100;
        
        public BgUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        protected override void Calculate()
        {
            if (FindPath() != null)
            {
                path = FindPath().ToArray();
            } 
            else
            {
                path = null;
            }

            if (path == null)
            {
                path = new Vector2Int[] { StartPoint };
            }
        }

        public List<Vector2Int> FindPath()
        {

            Node startNode = new Node(_startPoint);
            Node targetNode = new Node(_endPoint);
            List<Node> openList = new List<Node>() { startNode };
            List<Node> closedList = new List<Node>();
          
            while (openList.Count > 0)
            {

                Node currentNode = openList[0];

                foreach (var node in openList)
                {
                    if (node.Value < currentNode.Value)
                        currentNode = node;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Point.x == targetNode.Point.x && currentNode.Point.y == targetNode.Point.y || openList.Count >= MaxLength)
                {
                    List<Vector2Int> path = new List<Vector2Int>();
                    while (currentNode != null)
                    {
                        path.Add(currentNode.Point);
                        currentNode = currentNode.Parent;
                    }

                    path.Reverse();
                    return path;
                }

                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentNode.Point.x + dx[i];
                    int newY = currentNode.Point.y + dy[i];
                    var newPoint = new Vector2Int(newX, newY);

                    if (!IsValid(newPoint) && newPoint != _endPoint)
                        continue;

                    Node neighbor = new Node(newPoint);

                    if (closedList.Contains(neighbor))
                        continue;

                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.Point.x, targetNode.Point.y);
                    neighbor.CalculateValue();

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            return null;
        }
        
        private bool IsValid(Vector2Int point)
        {
            bool containsY = point.y >= 0 && point.y < runtimeModel.RoMap.Height;
            bool containsX = point.x >= 0 && point.x < runtimeModel.RoMap.Width;
            return containsX && containsY && runtimeModel.IsTileWalkable(point);
        }
    }

    public class Node
    {
        public int Cost = 10;
        public int Estimate;
        public int Value;
        public Node Parent;
        public Vector2Int Point;

        public Node(Vector2Int point)
        {
            Point = point;
        }

        public void CalculateEstimate(int tarX, int tarY)
        {
            Estimate = Math.Abs(Point.x - tarX) + Math.Abs(Point.y - tarY);
        }

        public void CalculateValue()
        {
            Value = Cost + Estimate;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node node)
                return false;
            return Point.x == node.Point.x && Point.y == node.Point.y;
        }
    }
}