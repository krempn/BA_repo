/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private Stopwatch timer = new Stopwatch();
    private TimeSpan span = new TimeSpan();
    private string elapsedTime;

    private string algo = "dij";
    private List<PathNode> allList;

    public const int INF = 99999999;
    int[,] graph;

    public static Pathfinding Instance { get; private set; }

    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height) {
        Instance = this;
        grid = new Grid<PathNode>(width, height, 10f, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    public Grid<PathNode> GetGrid() {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if (path == null) {
            return null;
        } else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY) {
        timer.Start();

        allList = new List<PathNode>();

        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (startNode == null || endNode == null)
        {
            // Invalid Path
            timer.Stop();
            return null;
        }

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                
                pathNode.gCost = 99999999;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;               
                allList.Add(pathNode);
            }
        }

        if (algo.Equals("floyd") || algo.Equals("bellman"))
        {
            graph = new int[grid.GetWidth() * grid.GetHeight(), grid.GetWidth() * grid.GetHeight()];

            for (int i = 0; i < (grid.GetWidth() * grid.GetHeight()); i++)
                for (int j = 0; j < (grid.GetWidth() * grid.GetHeight()); j++)
                {
                    PathNode node = allList[i];
                    PathNode node2 = allList[j];
                    List<PathNode> neighbourList = GetNeighbourList(node);

                    int firstX = node.x;
                    int fistY = node.y;

                    int secX = node2.x;
                    int secY = node2.y;

                    if (neighbourList.Contains(node2))
                        if (Math.Abs(firstX - secX) == 1 && Math.Abs(fistY - secY) == 1)
                            graph[i, j] = 14;
                        else
                            graph[i, j] = 10;
                    else
                        graph[i, j] = INF;

                    if (i == j)
                        graph[i, j] = 0;
                }
            foreach (PathNode gridNode in allList)
            {
                if (!gridNode.isWalkable)
                {
                    int index = allList.IndexOf(gridNode);

                    for (int i = 0; i < (grid.GetWidth() * grid.GetHeight()); i++)
                        for (int j = 0; j < (grid.GetWidth() * grid.GetHeight()); j++)
                            if (i == index || j == index)
                                graph[i, j] = INF;
                }              
            }           
        }

        if (algo.Equals("aStar") || algo.Equals("dij"))
        {
            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            PathfindingDebugStepVisual.Instance.ClearSnapshots();
            PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, startNode, openList, closedList);

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    // Reached final node
                    PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, currentNode, openList, closedList);
                    PathfindingDebugStepVisual.Instance.TakeSnapshotFinalPath(grid, CalculatePath(endNode));
                    timer.Stop();
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!neighbourNode.isWalkable)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        if (algo.Equals("aStar"))
                        {
                            neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        }
                        else if (algo.Equals("dij"))
                        {
                            neighbourNode.hCost = 0;
                        }

                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                    PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, currentNode, openList, closedList);
                }
            }
        }
        else if (algo.Equals("bellman"))
        {
            if (!startNode.isWalkable || !endNode.isWalkable)
            {
                timer.Stop();
                return null;
            }
                

            startNode.gCost = 0;
            /*
            for (int n = 0; n <= allList.Count - 1; n++)
            {
                foreach (PathNode wayPoint in allList)
                    foreach (PathNode wayPointToo in GetNeighbourList(wayPoint))
                    {
                        if (wayPoint.gCost + graph[allList.IndexOf(wayPoint), allList.IndexOf(wayPointToo)] < wayPointToo.gCost)
                        {
                            wayPointToo.gCost = wayPoint.gCost + graph[allList.IndexOf(wayPoint), allList.IndexOf(wayPointToo)];
                            wayPointToo.cameFromNode = wayPoint;
                        }
                    }
            }*/

            for (int n = 0; n <= allList.Count - 1; n++)
                for (int i = 0; i <= allList.Count - 1; i++)
                {
                    List<PathNode> neighbourList = GetNeighbourList(allList[i]);
                    for (int j = 0; j < neighbourList.Count; j++)
                    {
                        if (allList[i].gCost + graph[i, allList.IndexOf(neighbourList[j])] < neighbourList[j].gCost)
                        {
                            neighbourList[j].gCost = allList[i].gCost + graph[i, allList.IndexOf(neighbourList[j])];
                            neighbourList[j].cameFromNode = allList[i];
                        }
                    }
                }
            
            foreach (PathNode wayPoint in allList)
                foreach (PathNode wayPointToo in GetNeighbourList(wayPoint))
                {
                    if (wayPoint.gCost + graph[allList.IndexOf(wayPoint), allList.IndexOf(wayPointToo)] < wayPointToo.gCost)
                    {
                        timer.Stop();
                        return null; // negative circle
                    }                       
                }

            
            if (CalculatePath(endNode).Count <= 1)
            {
                timer.Stop();
                return null;
            }
            else
            {
                timer.Stop();
                return CalculatePath(endNode);
            }
        }
        else if (algo.Equals("floyd"))
        {
            if (!startNode.isWalkable || !endNode.isWalkable)
            {
                timer.Stop();
                return null;
            }
               

            graph = FloydWarshall(graph, grid.GetWidth() * grid.GetHeight());

            int from = allList.IndexOf(startNode);
            int to = allList.IndexOf(endNode);

            int costs = graph[from, to];

            List<PathNode> shortestPath = new List<PathNode>();

            shortestPath.Add(endNode);

            int pathLegnthReached = 0;

            while (shortestPath[shortestPath.Count - 1] != startNode && pathLegnthReached < grid.GetWidth() * grid.GetHeight())
            {   
                List<PathNode> nieghbours = GetNeighbourList(shortestPath[shortestPath.Count - 1]);               

                foreach (PathNode nieghbour in nieghbours)
                {                 
                    if (graph[from, allList.IndexOf(nieghbour)] <= costs)
                    {
                        shortestPath[shortestPath.Count - 1].cameFromNode = nieghbour;
                        costs = graph[from, allList.IndexOf(nieghbour)];
                    }                               
                }         

                if (!shortestPath.Contains(shortestPath[shortestPath.Count - 1].cameFromNode))
                    shortestPath.Add(shortestPath[shortestPath.Count - 1].cameFromNode);

                pathLegnthReached++;
            }

            if (!shortestPath.Contains(startNode))
            {
                timer.Stop();
                return null;
            }
                
            
            shortestPath.Reverse();
            timer.Stop();
            return CalculatePath(endNode); // alternative return shorthestPath List
        }



        // Out of nodes on the openList
        timer.Stop();
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode) {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0) {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        // Down
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    public PathNode GetNode(int x, int y) {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    public string getElapsedTime()
    {
        span = timer.Elapsed;
        elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds / 10);        
        timer.Reset();
        return elapsedTime;
    }

    public int[,] FloydWarshall(int[,] graph, int verticesCount)
    {
        int[,] distance = new int[verticesCount, verticesCount];

        for (int i = 0; i < verticesCount; ++i)
            for (int j = 0; j < verticesCount; ++j)
                distance[i, j] = graph[i, j];

        for (int k = 0; k < verticesCount; ++k)
        {
            for (int i = 0; i < verticesCount; ++i)
            {
                for (int j = 0; j < verticesCount; ++j)
                {
                    if (distance[i, k] + distance[k, j] < distance[i, j])
                        distance[i, j] = distance[i, k] + distance[k, j];
                       
                        
                }
            }
        }


        return distance;
    }

    public void SetAlgorithm(string algo)
    {
        if (algo.Equals("Dijkstra"))
        {
            this.algo = "dij";
        }
        if (algo.Equals("A*"))
        {
            this.algo = "aStar";
        }
        if (algo.Equals("Bellman-Ford"))
        {
            this.algo = "bellman";
        }
        if (algo.Equals("Floyd-Warshall"))
        {
            this.algo = "floyd";
        }
    }
}
