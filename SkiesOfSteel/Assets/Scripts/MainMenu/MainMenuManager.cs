using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // Resources
    [SerializeField] private int goldenCoins = 56021;
    [SerializeField] private int platinumGears = 562;
    [SerializeField] private int gems = 34;

    [SerializeField] private TMP_Text _coinsCounter;
    [SerializeField] private TMP_Text _gearsCounter;
    [SerializeField] private TMP_Text _gemsCounter;

    [SerializeField] private Button _coinButton;
    [SerializeField] private Button _gearButton;
    [SerializeField] private Button _gemButton;

    [SerializeField] private Canvas _profileCanvas;
    [SerializeField] private Button _openProfileButton;
    [SerializeField] private Button _closeProfileButton;
    private RectTransform _profileTransform;


    // Start is called before the first frame update
    void Start()
    {
        _profileTransform = _profileCanvas.GetComponent<RectTransform>();
        _openProfileButton.GetComponent<Button>().onClick.AddListener(ShowProfile);
        _closeProfileButton.GetComponent<Button>().onClick.AddListener(HideProfile);

        _coinButton.GetComponent<Button>().onClick.AddListener(AddCoins);
        _gearButton.GetComponent<Button>().onClick.AddListener(AddGears);
        _gemButton.GetComponent<Button>().onClick.AddListener(AddGems);

        UpdateCounters();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShowProfile()
    {
        _profileTransform.DOAnchorPosX(320, 0.3f);
    }

    private void HideProfile()
    {
        _profileTransform.DOAnchorPosX(-320, 0.3f);
    }

    private void AddCoins()
    {
        goldenCoins += 5000;
        UpdateCounters();
    }

    private void AddGears()
    {
        platinumGears += 320;
        UpdateCounters();
    }

    private void AddGems()
    {
        gems += 5;
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        _coinsCounter.text = goldenCoins.ToString("#,##0");
        _gearsCounter.text = platinumGears.ToString("#,##0");
        _gemsCounter.text = gems.ToString("#,##0");
    }
}
