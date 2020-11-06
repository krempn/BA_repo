/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using CodeMonkey;
using UnityEngine.SceneManagement;

public class Testing : MonoBehaviour {

    [SerializeField] private PathfindingDebugStepVisual pathfindingDebugStepVisual;
    [SerializeField] private PathfindingVisual pathfindingVisual;
    [SerializeField] private CharacterPathfindingMovementHandler characterPathfinding;
    private Pathfinding pathfinding;

    private int offsteX = 85;
    private int offsetY = 555;

    private string widthString = " ";
    private string heightString = " ";


    private void Start() {
        pathfinding = new Pathfinding(10, 10);
        pathfindingDebugStepVisual.Setup(pathfinding.GetGrid());
        pathfindingVisual.SetGrid(pathfinding.GetGrid());
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            List<PathNode> path = pathfinding.FindPath(0, 0, x, y);
            if (path != null)
            {
                Debug.Log(pathfinding.getElapsedTime());
            }
            
           
            if (path != null) {
                for (int i=0; i<path.Count - 1; i++) {
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 5f, new Vector3(path[i+1].x, path[i+1].y) * 10f + Vector3.one * 5f, Color.green, 5f);
                }
            }
            characterPathfinding.SetTargetPosition(mouseWorldPosition);
        }

        if (Input.GetMouseButtonDown(1)) {
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0 + offsteX, offsetY, 80, 20), "Dijkstra"))
        {
            pathfinding.SetAlgorithm("Dijkstra");
            Debug.Log("Dijkstra");
        }
        if (GUI.Button(new Rect(80 + offsteX, offsetY, 80, 20), "A*"))
        {
            pathfinding.SetAlgorithm("A*");
            Debug.Log("A*");
        }
        if (GUI.Button(new Rect(160 + offsteX, offsetY, 100, 20), "Bellman-Ford"))
        {
            pathfinding.SetAlgorithm("Bellman-Ford");
            Debug.Log("Bellman-Ford");
        }
        if (GUI.Button(new Rect(260 + offsteX, offsetY, 100, 20), "Floyd-Warshall"))
        {
            pathfinding.SetAlgorithm("Floyd-Warshall");
            Debug.Log("Floyd-Warshall");
        }
        if (GUI.Button(new Rect(360 + offsteX, offsetY, 100, 20), "Reload"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
