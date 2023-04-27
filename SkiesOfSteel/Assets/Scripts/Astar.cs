using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;


// Super link: https://www.redblobgames.com/grids/hexagons/

public class Astar : MonoBehaviour
{
    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private Tilemap _objectsMap;



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


            foreach (Vector3Int pos in currNode.GetAdjacents())
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
    private int HexManhattanDistance(Vector3Int a, Vector3Int b)
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
    private Vector3Int GetCubeCoordinates(Vector3Int offsetCoord)
    {
        int q = offsetCoord.y;
        int r = offsetCoord.x - (offsetCoord.y - (offsetCoord.y & 1)) / 2;

        return new Vector3Int( q, r, -q-r );
    }




    public List<Vector3Int> GetAdjacents()
    {
        int parity = Position.y & 1; // parity = 0 means we are on an even column, parity = 1 means we are on an odd column

        if (parity == 0)
        {
            return new List<Vector3Int>
            {
                Position + new Vector3Int(0, 1, 0),
                Position + new Vector3Int(-1, 1, 0),
                Position + new Vector3Int(-1, 0, 0),
                Position + new Vector3Int(-1, -1, 0),
                Position + new Vector3Int(0, -1, 0),
                Position + new Vector3Int(1, 0, 0),
            };

        }
        else
        {
            return new List<Vector3Int>
            {
                Position + new Vector3Int(1, 1, 0),
                Position + new Vector3Int(0, 1, 0),
                Position + new Vector3Int(-1, 0, 0),
                Position + new Vector3Int(0, -1, 0),
                Position + new Vector3Int(1, -1, 0),
                Position + new Vector3Int(1, 0, 0),
            };
        }
    }

}
