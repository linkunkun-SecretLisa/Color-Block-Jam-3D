using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemPositionMap<T>
{
    public event Action<T, Vector2Int> OnItemAdded;
    public event Action<T, Vector2Int> OnItemRemoved;
    public event Action<T, Vector2Int, Vector2Int> OnItemMoved;

    public ItemPositionMap(byte zero) 
    {
        map = new Dictionary<Vector2Int, T>();
    }

    // Standard 4 Directions + Diagonals
    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(-1, 0), // Left
        new Vector2Int(1, 0),  // Right
        new Vector2Int(-1, 1), // Top-Left
        new Vector2Int(1, 1),  // Top-Right
        new Vector2Int(-1, -1), // Bottom-Left
        new Vector2Int(1, -1)  // Bottom-Right
    };

    private Dictionary<Vector2Int, T> map;

    // Add an item and trigger the event
    public void AddItem(T item, Vector2Int gridPosition)
    {
        if (map.ContainsKey(gridPosition))
        {
            Debug.LogWarning($"Position {gridPosition} already contains an item!");
            return;
        }

        map[gridPosition] = item;
        OnItemAdded?.Invoke(item, gridPosition);
    }

    // Remove an item and trigger the event
    public void RemoveItem(T item, Vector2Int gridPosition)
    {
        if (!map.ContainsKey(gridPosition))
        {
            Debug.LogWarning($"Position {gridPosition} does not contain an item!");
            return;
        }

        map.Remove(gridPosition);
        OnItemRemoved?.Invoke(item, gridPosition);
    }

    // Update an item's position and trigger the events
    public void UpdateItemPosition(T item, Vector2Int oldPosition, Vector2Int newPosition)
    {
        if (!map.ContainsKey(oldPosition))
        {
            Debug.LogWarning($"Old position {oldPosition} does not contain the item!");
            return;
        }

        if (map.ContainsKey(newPosition))
        {
            Debug.LogWarning($"New position {newPosition} already contains another item!");
            return;
        }

        RemoveItem(item, oldPosition);
        AddItem(item, newPosition);
        OnItemMoved?.Invoke(item, oldPosition, newPosition);
    }

    // Try to get an item at a given position
    public bool TryGetItem(Vector2Int position, out T item)
    {
        var customPosition = new Vector2Int(position.x, position.y);
        return map.TryGetValue(customPosition, out item);
    }

    // Try to get the position of a given item
    public bool TryGetItemPosition(T item, out Vector2Int position)
    {
        foreach (var keyValuePair in map)
        {
            if (EqualityComparer<T>.Default.Equals(keyValuePair.Value, item))
            {
                position = new Vector2Int(keyValuePair.Key.x, keyValuePair.Key.y);
                return true;
            }
        }

        position = Vector2Int.zero;
        return false;
    }

    // Check if a position is empty
    public bool IsPositionEmpty(Vector2Int position)
    {
        return !map.ContainsKey(position);
    }

    // Get an item in a specific direction from a given position
    public T GetNeighbourItem(Vector2Int position, Vector2Int direction)
    {
        var neighbourPosition = position + direction;
        return map.GetValueOrDefault(neighbourPosition);
    }

    // Get all neighboring items (including diagonals)
    public List<T> GetNeighbors(Vector2Int position)
    {
        var neighbours = new List<T>();

        foreach (var direction in directions)
        {
            var neighbour = GetNeighbourItem(position, direction);
            if (neighbour == null) continue;
            neighbours.Add(neighbour);
        }

        return neighbours;
    }

    // Get all positions occupied by items
    public List<Vector2Int> GetAllOccupiedPositions()
    {
        return new List<Vector2Int>(map.Keys);
    }

    // Clear the map and reset
    public void Clear()
    {
        map.Clear();
    }

    // For Debugging purposes
    public void PrintAllItems()
    {
        foreach (var keyValuePair in map)
        {
            Debug.Log($"Position: {keyValuePair.Key}, Item: {keyValuePair.Value}");
        }
    }
}
