using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private float _slideLimitYPos;
    private bool _lockInput;
    private int _activeLevelCount;
    private Tweener _twEndDragSlide;
    private List<LevelBox> _levelBoxes;
    [SerializeField] private Transform levelSlideParent;
    [SerializeField] private float sensitivity;
    [SerializeField] private float threshold;

    private void Awake()
    {
        var boxes = new List<LevelBox>();
        foreach (var box in GetComponentsInChildren<LevelBox>())
        {
            boxes.Add(box);
            box.PlayButtonTapped += PlayButtonTapped;
        }

        _levelBoxes = boxes.OrderBy(x => -x.transform.position.y).ToList();
    }

    private void Start()
    {
        SetLevelsFromData();
        transform.DOScale(1, 0.5f)
            .From(0)
            .SetEase(Ease.OutBack);
    }
    private void OnDisable()
    {
        foreach (var box in _levelBoxes) box.PlayButtonTapped -= PlayButtonTapped;
    }
    private void SetLevelsFromData()
    {
        var orderedList = LevelDataHandler.GetOrderedList();
        var totalStarCount = LevelDataHandler.TotalStarCount;
        _activeLevelCount = orderedList.Count;

        for (int i = 0; i < _levelBoxes.Count; i++)
        {
            _levelBoxes[i].SetData(orderedList[i], totalStarCount);
        }

        SetLevelMenu();
    }

    private void SetLevelMenu()
    {
        _slideLimitYPos = 0.55f + (_activeLevelCount - 5) * 0.9f;
        var unlockedLevelListNumber = LevelDataHandler.NextLevelToUnlock - 1;
        var yPos = 0.5f + (unlockedLevelListNumber - 4) * 0.9f;
        yPos = Mathf.Clamp(yPos, 0, _slideLimitYPos);
        var slideTime = Mathf.Lerp(0.25f, 1, yPos / 3);

        if (unlockedLevelListNumber < 0 || unlockedLevelListNumber < 4) return;

        //Move menu down to first unlocked level
        _twEndDragSlide = levelSlideParent.DOLocalMoveY(yPos, slideTime)
            .SetDelay(0.5f)
            .OnUpdate(CheckToRepositionBoxes)
            .OnComplete(() => _lockInput = false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_lockInput) return;
        var dif = eventData.delta;
        var p = levelSlideParent.localPosition;
        var newY = p.y + dif.y * sensitivity / 100;
        newY = Mathf.Clamp(newY, 0, _slideLimitYPos + 0.5f);
        levelSlideParent.localPosition = new Vector3(p.x, newY, p.z);
        CheckToRepositionBoxes();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_lockInput) return;
        var localYPos = levelSlideParent.localPosition.y;
        if (Mathf.Abs(eventData.delta.y) < threshold)
        {
            if (localYPos > _slideLimitYPos) SlideToEndPos(localYPos);
            return;
        }

        var endY = localYPos + eventData.delta.y * sensitivity / 5;
        SlideToEndPos(endY);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_lockInput) return;
        if (_twEndDragSlide.IsActive()) _twEndDragSlide.Kill();
    }

    private void SlideToEndPos(float endY)
    {
        endY = Mathf.Clamp(endY, 0, _slideLimitYPos);
        var moveY = Mathf.Abs(levelSlideParent.localPosition.y - endY);
        var time = Mathf.Lerp(0.25f, 1, moveY / 3);
        _twEndDragSlide = levelSlideParent.DOLocalMoveY(endY, time)
            .SetEase(Ease.OutQuad)
            .OnUpdate(CheckToRepositionBoxes)
            .SetUpdate(UpdateType.Fixed);
    }

    private void CheckToRepositionBoxes()
    {
        var firstBox = _levelBoxes[0];
        var lastBox = _levelBoxes[5];
        var orderedList = LevelDataHandler.GetOrderedList();
        var totalStarCount = LevelDataHandler.TotalStarCount;
        if (firstBox.transform.position.y >= 2.7f)
        {
            var nextNumber = lastBox.LevelData.LevelNumber;
            if (orderedList.Count <= nextNumber) return;
            _levelBoxes.Remove(firstBox);
            firstBox.transform.position = new Vector3(0, lastBox.transform.position.y - 0.9f, 0);
            firstBox.SetData(orderedList[nextNumber], totalStarCount);
            _levelBoxes.Add(firstBox);
        }
        else if (lastBox.transform.position.y < -2.7f)
        {
            var nextNumber = firstBox.LevelData.LevelNumber - 2;
            if (nextNumber < 0) return;
            _levelBoxes.Remove(lastBox);
            lastBox.transform.position = new Vector3(0, firstBox.transform.position.y + 0.9f, 0);
            lastBox.SetData(orderedList[nextNumber], totalStarCount);
            _levelBoxes.Insert(0, lastBox);
        }
    }

    private void PlayButtonTapped(int levelNumber)
    {
        if (_lockInput) return;
        _lockInput = true;
        if (_twEndDragSlide.IsActive()) _twEndDragSlide.Kill();
        transform.DOScale(0, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => SceneHandler.Instance.LoadLevel(levelNumber));
    }
}