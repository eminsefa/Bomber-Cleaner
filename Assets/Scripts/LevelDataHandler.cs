using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LevelData
{
    public int LevelNumber;
    public int GridWidth;
    public int GridHeight;
    public int Unlocked;
    public int Score;
    public int BombCount;
    public List<int> Grid = new List<int>();
    public List<int> FilledGrids;
}

public static class LevelDataHandler
{
    private const string MainUrl = "https://engineering-case-study.s3.eu-north-1.amazonaws.com/LS_Case_Level-";
    private const int LevelCount = 20;
    private static bool _dataChanged = true;
    public static int NextLevelToUnlock { get; private set; }
    public static int TotalStarCount { get; private set; }
    private static List<LevelData> _orderedLevelList;
    public static List<LevelData> GetOrderedList() => _dataChanged ? ReadLevelData() : _orderedLevelList;

    private static List<LevelData> ReadLevelData()
    {
        var persistentDataPath = Application.persistentDataPath + "/Levels/";
        var levelDataList = new List<LevelData>();
        TotalStarCount = 0;

        if (PlayerPrefs.GetInt("FirstLaunch", 1) == 1)
        {
            PlayerPrefs.SetInt("FirstLaunch", 0);
            PlayerPrefs.SetString("Level1", "10"); //string[0] is unlock data, string[1] is score data
            if (!Directory.Exists(persistentDataPath)) Directory.CreateDirectory(persistentDataPath);
            else //Clear data on clear player pref
            {
                foreach (var f in Directory.GetFiles(persistentDataPath)) File.Delete(f);
            }

            //Added first 6 levels manually to be able to play offline.
            //Here it writes them to persistant path to be in the same directory with downloaded levels
            var levelsDefault = Resources.LoadAll<TextAsset>("Levels");
            var assetFileInfo = levelsDefault.Where(x => !x.name.Contains("meta")).ToArray(); //Ignore meta files
            foreach (var f in assetFileInfo)
            {
                var result = f.text;
                File.WriteAllText(persistentDataPath + f.name, result);
            }
        }

        //Read existing files
        var info = new DirectoryInfo(persistentDataPath);
        var levelFiles = info.GetFiles().ToArray();
        foreach (var f in levelFiles)
        {
            var levelNumber = int.Parse(f.Name);
            var path = Application.persistentDataPath + "/Levels/" + levelNumber;
            var result = File.ReadAllLines(path);
            //Read Grid
            var grid = new List<int>();
            var filledList = new List<int>();
            var gridHeight = result.Length;
            var gridWidth = (result[0].Length + 1) / 2; //Ignore commas
            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    var cellType = result[gridHeight - 1 - i][j * 2] - '0';
                    grid.Add(cellType);
                }
            }

            for (int i = 0; i < grid.Count; i++)
            {
                if (grid[i] == 1) filledList.Add(i);
            }
            
            var d = PlayerPrefs.GetString("Level" + levelNumber, "00");
            //Convert data and create level data for each file
            var levelData = new LevelData
            {
                LevelNumber = levelNumber,
                GridWidth = gridWidth,
                GridHeight = gridHeight,
                Unlocked = d[0] - '0',
                Score = d[1] - '0',
                Grid = grid,
                FilledGrids = filledList
            };
            TotalStarCount += levelData.Score;
            levelData.BombCount = GridHelper.GetMinimumBombCount(levelData, filledList.ToList())+2;
            levelDataList.Add(levelData);
        }
        _orderedLevelList =
            levelDataList.OrderBy(x => x.LevelNumber).ToList(); //Order by level number for possible order bugs
        //Find the next locked level
        for (int i = 0; i < _orderedLevelList.Count; i++)
        {
            if (_orderedLevelList[i].Unlocked == 0)
            {
                NextLevelToUnlock = i + 1;
                break;
            }

            NextLevelToUnlock = levelFiles.Length; //If all levels are unlocked
        }

        _dataChanged = false;
        return _orderedLevelList;
    }

    public static void LevelEnded(int level, int score)
    {
        var prevScore = _orderedLevelList[level - 1].Score;
        if (score > prevScore)
        {
            TotalStarCount += score - prevScore;
            PlayerPrefs.SetString("Level" + level, "1" + score);
            _dataChanged = true;
        }

        for (int i = level; i < level + 5; i++) //Checks 5 levels if star check level is completed
        {
            if (_orderedLevelList.Count < i) return; //Return if all levels are already unlocked 
            if (i % 5 == 0)
            {
                if (_orderedLevelList[i - 1].Unlocked != 0 || _orderedLevelList[i - 2].Unlocked != 1)
                    continue; //Check if star level is next level
                var desiredStarCount = i * 2 - 2;
                if (TotalStarCount < desiredStarCount) PlayerPrefs.SetString("Level" + i, "00");
                else
                {
                    PlayerPrefs.SetString("Level" + i, "10");
                    _dataChanged = true;
                }

                break;
            }
            else if (i == level + 1 &&
                     _orderedLevelList[level].Unlocked != 1) //Unlock next level if is not star check level
            {
                PlayerPrefs.SetString("Level" + i, "10");
                _dataChanged = true;
            }
        }
    }

    public static IEnumerator DownloadLevels()
    {
        //This is called once on application start, retries every 5 second until device connects internet

        if (_orderedLevelList == null) _orderedLevelList = GetOrderedList();
        if (_orderedLevelList.Count == LevelCount && LevelCount != 0) yield break; //If all levels downloaded

        var persistentDataPath = Application.persistentDataPath + "/Levels/";
        var mainURL = MainUrl;
        var levelNames = Enumerable.Range(1, LevelCount + 1).ToArray();

        for (int i = _orderedLevelList.Count; i < levelNames.Length; i++) //Pass already downloaded levels
        {
            var path = persistentDataPath + levelNames[i];

            var www = UnityWebRequest.Get(mainURL + levelNames[i]);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                yield return new WaitForSeconds(5f);
                SceneHandler.Instance
                    .StartCoroutine(
                        DownloadLevels()); //Restarted coroutine instead of yield waiting to prevent some bugs
                yield break;
            }

            File.WriteAllText(path, www.downloadHandler.text);
        }

        _dataChanged = true;
    }
}