using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    private float _screenAspect;
    private const float CamDefaultSize = 5;
    private LevelData _currentLevelData;
    public LevelData GetCurrentLevelData() => LevelDataHandler.GetOrderedList()[_currentLevelData.LevelNumber - 1];
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        //Calls this only on application start
        StartCoroutine(LevelDataHandler.DownloadLevels());
        _screenAspect = (float) Screen.width / Screen.height;
        SetCameraSizeForMenu();
    }

    private void SetCameraSizeForMenu()
    {
        var cam = Camera.main;
        if (_screenAspect > 1) cam.orthographicSize = CamDefaultSize / 2 * _screenAspect;
        else cam.orthographicSize = CamDefaultSize / 2 / _screenAspect;
    }

    public void SetCameraSizeForGrid()
    {
        var gridWidth = _currentLevelData.GridWidth;
        var gridHeight = _currentLevelData.GridHeight;
        var cam = Camera.main;
        var gridAspect = (float) gridWidth / gridHeight;

        if (gridAspect > _screenAspect) cam.orthographicSize = (gridWidth / (2 * _screenAspect)) + 1;
        else cam.orthographicSize = (gridHeight + 2) / 2f;
    }

    public void LoadLevel(int levelNumber)
    {
        var levelDatas = LevelDataHandler.GetOrderedList();
        _currentLevelData=levelDatas[levelNumber-1];
        SceneManager.LoadScene($"LevelScene");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene($"MenuScene");
        SetCameraSizeForMenu();
    }


    private void OnApplicationQuit()
    {
        StopAllCoroutines(); //If download coroutine creates a bug
    }
}