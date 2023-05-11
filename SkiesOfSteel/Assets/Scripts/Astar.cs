using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;


// Super link: https://www.redblobgames.com/grids/hexagons/

public class Astar : MonoBehaviour
{
    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private Tilemap _objectsMap;

    [Space]

    [SerializeField]
    private bool _debugTileVertices;
    [SerializeField]
    private Vector3[] _verticesDiff;



    /// <summary>
    /// Astar search for path between start and goal
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public Node Search(Vector3Int start, Vector3Int goal)
    {
        if (start == goal) return new Node(goal, 0);

        if (_tilemap.GetTile(start) == null || _tilemap.GetTile(goal) == null)
        {
            Debug.Log("Can't start or arrive in a non existing tile!!!");
            return new Node(start, 0);
        }

        if (!(_tilemap.GetTile(start) as MapTile).IsWalkable || !(_tilemap.GetTile(goal) as MapTile).IsWalkable)
        {
            //TODO add check if current ship can walk on unwalkable tiles

            Debug.Log("Can't start or arrive in an Unwalkable cell!!!");
            return new Node(start, 0);
        }


        if (_objectsMap.GetTile(goal) != null)
        {
            //TODO add check if current ship can stay in an occupied destination

            Debug.Log("Occupied destination!!!");
            return new Node(start, 0);
        }

        List<Node> frontier = new List<Node>();
        List<Vector3Int> visited = new List<Vector3Int>();


        frontier.Add(new Node(start, 0));
        visited.Add(start);


        while(frontier.Count > 0)
        {
            frontier.Sort( (x, y) => x.GetDistance(goal).CompareTo(y.GetDistance(goal)) );

            Node currNode = frontier[0];
            frontier.RemoveAt(0);


            foreach (Vector3Int pos in Node.GetAdjacents(currNode.Position))
            {
                if (_tilemap.GetTile(pos) == null) continue; // If tile doesn't exist don't check

                if (pos == goal)
                {
                    Node newNode = new Node(pos, currNode.G + 1);

                    newNode.Parent = currNode;

                    return newNode;
                }
                else if (!visited.Contains(pos))
                {
                    if (!(_tilemap.GetTile(pos) as MapTile).IsWalkable)
                    {
                        // TODO add check if current ship can walk on unwalkable tiles
                        continue;
                    }
                    

                    Node newNode = new Node(pos, currNode.G + 1);

                    newNode.Parent = currNode;

                    frontier.Add(newNode);
                    visited.Add(pos);
                }
            }
        }


        Debug.Log("Unreachable destination!!!");
        return new Node(start, 0);
    }

    /// <summary>
    /// Retuns the set of cells that you can move to with the specified start center cell and movement range
    /// </summary>
    /// <param name="center"></param>
    /// <param name="movementRange"></param>
    /// <returns></returns>
    public List<Vector3Int> GetPossibleDestinations(Vector3Int center, int movementRange)
    {
        List<Vector3Int> visited = new List<Vector3Int>();

        visited.Add(center);

        List<Vector3Int>[] fringes = new List<Vector3Int>[movementRange + 1]; // fringes[k] is the set of cells that can be reached in k steps
        for (int k = 0; k <= movementRange; k++)
            fringes[k] = new List<Vector3Int>();

        fringes[0].Add(center);


        for (int k = 1; k <= movementRange; k++)
        {
            foreach(Vector3Int reachable in fringes[k-1])
            {
                foreach (Vector3Int cell in Node.GetAdjacents(reachable))
                {
                    if (_tilemap.GetTile(cell) != null && (_tilemap.GetTile(cell) as MapTile).IsWalkable && !visited.Contains(cell))
                    {
                        visited.Add(cell);
                        fringes[k].Add(cell);
                    }
                }
            }
        }

        return visited;

    }



    public List<Vector3Int> GetLine(Vector3 start, Vector3 goal)
    {
        int N = Node.HexManhattanDistance(_tilemap.WorldToCell(start), _tilemap.WorldToCell(goal)) * 3;

        float distance = Vector3.Distance(start, goal);
        float offset = distance / N;
        Vector3 dir = (goal - start).normalized;

        List<Vector3Int> result = new List<Vector3Int>();

        for (float i = 0; i <= distance; i += offset)
        {
            Vector3Int cell = _tilemap.WorldToCell(start + dir * i);
            if (!result.Contains(cell))
                result.Add(cell);
        }

        return result;

    }

    /// <summary>
    /// This method checks wheter there is a line of sight between cell start and cell goal
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns>null if there is no line of sight, returns the start world point and goal world point of there is sight
    /// This return is done like this for now just for debug, in future convert this return to a boolean</returns>
    public List<Vector3> GetLineOfSight(Vector3Int start, Vector3Int goal)
    {
        foreach(Vector3 diff in _verticesDiff)
        {
            foreach(Vector3 diff2 in _verticesDiff)
            {
                List<Vector3Int> straightPath = GetLine(_tilemap.GetCellCenterWorld(start) + diff, _tilemap.GetCellCenterWorld(goal) + diff2);

                bool lineOfSightExists = true;

                foreach (Vector3Int cell in straightPath)
                {
                    if (_tilemap.HasTile(cell) && (_tilemap.GetTile(cell) as MapTile).IsWalkable)
                        continue;
                    else
                    {
                        lineOfSightExists = false;
                        break;
                    }      
                }

                if (lineOfSightExists)
                    return new List<Vector3>
                    {
                        _tilemap.GetCellCenterWorld(start) + diff,
                        _tilemap.GetCellCenterWorld(goal) + diff2
                    };
            }
        }
        
        // If no line of sight return null
        return null;
    }

 

    private void OnDrawGizmos()
    {
        if (_debugTileVertices)
        {
            Gizmos.color = Color.yellow;

            foreach (Vector3 diff in _verticesDiff)
            {
                Vector3 position = _tilemap.GetCellCenterWorld(new Vector3Int(0, 0, 0)) + diff;

                Gizmos.DrawSphere(position, 0.05f);

            }
        }
    }

}






public class Node
{
    public int G { get;  } // cost so far
    private int F; // F = G + H
    public Node Parent { get; set; }
    public Vector3Int Position { get; }


    public Node(Vector3Int position, int g)
    {
        G = g;
        F = -1;
        Position = position;
    }


    public float GetDistance(Vector3Int goal)
    {
        if (F == -1)
            F = HexManhattanDistance(Position, goal) + G;
        
        return F;
    }


    /// <summary>
    /// Calculates the manhattan distance of two grid cells given the offset coordinates of flat top hex grid in unity
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int HexManhattanDistance(Vector3Int a, Vector3Int b)
    {
        Vector3Int aCubeCoord = GetCubeCoordinates(a);
        Vector3Int bCubeCoord = GetCubeCoordinates(b);

        return ( Mathf.Abs(aCubeCoord.x - bCubeCoord.x) + Mathf.Abs(aCubeCoord.y - bCubeCoord.y) + Mathf.Abs(aCubeCoord.z - bCubeCoord.z) ) / 2;
    }


    /// <summary>
    /// This method converts the input offset coordinate in a cube coordinate for the hex grid flat top of unity
    /// </summary>
    /// <param name="offsetCoord"></param>
    /// <returns></returns>
    public static Vector3Int GetCubeCoordinates(Vector3Int offsetCoord)
    {
        int q = offsetCoord.y;
        int r = offsetCoord.x - (offsetCoord.y - (offsetCoord.y & 1)) / 2;

        return new Vector3Int( q, r, -q-r );
    }


    /// <summary>
    /// This method converts the input cube coordinate in offset coordinates for the hex grid flat top of unity
    /// </summary>
    /// <param name="cubeCoordinates"></param>
    /// <returns></returns>
    public static Vector3Int GetOffsetCoordinates(Vector3Int cubeCoordinates)
    {
        int column = cubeCoordinates.x;
        int row = cubeCoordinates.y + (cubeCoordinates.x - (cubeCoordinates.x & 1)) / 2;
        return new Vector3Int(row, column, 0);
    }




    public static List<Vector3Int> GetAdjacents(Vector3Int pos)
    {
        int parity = pos.y & 1; // parity = 0 means we are on an even column, parity = 1 means we are on an odd column

        if (parity == 0)
        {
            return new List<Vector3Int>
            {
                pos + new Vector3Int(0, 1, 0),
                pos + new Vector3Int(-1, 1, 0),
                pos + new Vector3Int(-1, 0, 0),
                pos + new Vector3Int(-1, -1, 0),
                pos + new Vector3Int(0, -1, 0),
                pos + new Vector3Int(1, 0, 0),
            };

        }
        else
        {
            return new List<Vector3Int>
            {
                pos + new Vector3Int(1, 1, 0),
                pos + new Vector3Int(0, 1, 0),
                pos + new Vector3Int(-1, 0, 0),
                pos + new Vector3Int(0, -1, 0),
                pos + new Vector3Int(1, -1, 0),
                pos + new Vector3Int(1, 0, 0),
            };
        }
    }




    public static Vector3 CubeLerp(Vector3Int a, Vector3Int b, float t)
    {
        return new Vector3(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t), Mathf.Lerp(a.z, b.z, t));
    }

    public static Vector3Int CubeRound(Vector3 fractional)
    {

        float q = Mathf.Round(fractional.x);
        float r = Mathf.Round(fractional.y);
        float s = Mathf.Round(fractional.z);

        float qDiff = Mathf.Abs(q - fractional.x);
        float rDiff = Mathf.Abs(r - fractional.y);
        float sDiff = Mathf.Abs(s - fractional.z);

        if (qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if (rDiff > sDiff)
            r = -q - s;
        else
            s = -q - r;


        return new Vector3Int((int) q, (int) r, (int) s);
    }

}
