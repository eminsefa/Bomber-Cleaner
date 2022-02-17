using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

//Each level box sets texts and holds data to play
public class LevelBox : MonoBehaviour, IPointerClickHandler
{
    public event Action<int> PlayButtonTapped;
    public LevelData LevelData { get; private set; }
    [SerializeField] private TextMeshPro levelText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private GameObject progressBar;
    [SerializeField] private TextMeshPro progressBarText;
    [SerializeField] private SpriteRenderer fillBar;
    [SerializeField] private GameObject starsParent;
    [SerializeField] private List<SpriteRenderer> stars;

    public void SetData(LevelData levelData, int totalStarCount)
    {
        LevelData = levelData;
        var levelNumber = levelData.LevelNumber;
        levelText.text = "Level-" + levelNumber;
        
        starsParent.SetActive(true);
        progressBar.SetActive(false);
        for (int i = 0; i < stars.Count; i++) stars[i].color = new Color(0, 0, 0, 0.75f);
        if (levelData.Unlocked == 1)
        {
            playButton.SetActive(true);
            lockedImage.SetActive(false);
            for (int i = 0; i < levelData.Score; i++) stars[i].color = Color.white;
        }
        else
        {
            playButton.SetActive(false);
            lockedImage.SetActive(true);
            if (levelNumber % 5 == 0)
            {
                var desiredCount = levelNumber * 2 - 2;
                if (totalStarCount > desiredCount / 2)
                {
                    starsParent.SetActive(false);
                    progressBar.SetActive(true);
                    progressBarText.text = totalStarCount + "/" + desiredCount;
                    var xSize = Mathf.Lerp(0, 7.5f, (float) totalStarCount / desiredCount);
                    fillBar.size = new Vector2(xSize, fillBar.size.y);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayButtonTapped?.Invoke(LevelData.LevelNumber);
    }
}