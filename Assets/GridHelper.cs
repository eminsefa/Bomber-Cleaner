using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridHelper
{
    public static List<int> GetNeighbours(LevelData levelData, int gridNumber)
    {
        var gridWidth = levelData.GridWidth;
        var gridCount = gridWidth * levelData.GridHeight;
        var neighbors = new List<int>();
        var neighbourNumberDifference = new[]
        {
            -1, gridWidth, 1, -gridWidth
        };
        for (int i = 0; i < neighbourNumberDifference.Length; i++)
        {
            var dif = neighbourNumberDifference[i];
            var neighbourGrid = gridNumber + dif;
            if (Mathf.Abs(dif) == 1 && gridNumber / gridWidth != neighbourGrid / gridWidth)
                continue; //Check If Same Width For Horizontal Neighbours
            if (neighbourGrid < 0 || neighbourGrid >= gridCount) continue;
            neighbors.Add(neighbourGrid);
        }

        return neighbors;
    }

    public static int GetMinimumBombCount(LevelData data, List<int> filledGrids, int bombCount = 0)
    {
        if (filledGrids.Count <= 0) return bombCount;
        var levelData = CopyData(data);

        var possibleBombGrids = new List<int>();
        foreach (var filledGrid in filledGrids)
        {
            var neighbours = GetNeighbours(levelData, filledGrid);
            foreach (var n in neighbours)
            {
                if (!possibleBombGrids.Contains(n))
                {
                    if(levelData.Grid[n]==0) possibleBombGrids.Add(n);
                }
            }
        }

        var bombCountDictionary = new Dictionary<int, int>();
        foreach (var bombGrid in possibleBombGrids)
        {
            var neighbours = GetNeighbours(levelData, bombGrid);
            var destroyCount = neighbours.Count(n => levelData.Grid[n] == 1);
            bombCountDictionary.Add(bombGrid, destroyCount);
        }

        var maxDestroyer = bombCountDictionary.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        var bombNeighbours = GetNeighbours(levelData, maxDestroyer);

        foreach (var n in bombNeighbours)
        {
            if (filledGrids.Contains(n))
            {
                levelData.Grid[n] = 2;
                filledGrids.Remove(n);
            }
        }

        bombCount++;
        return GetMinimumBombCount(levelData, filledGrids, bombCount);
    }

    private static LevelData CopyData(LevelData data)
    {
        return new LevelData()
        {
            LevelNumber = data.LevelNumber,
            GridWidth = data.GridWidth,
            GridHeight = data.GridHeight,
            Unlocked = data.Unlocked,
            Score = data.Score,
            Grid = data.Grid.ToList(),
            FilledGrids = data.FilledGrids.ToList()
        };
    }
}