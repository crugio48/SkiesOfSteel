using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class GachaManager : MonoBehaviour
{
    [SerializeField] GachaPull[] gacha3stars;
    [SerializeField] GachaPull[] gacha4stars;
    [SerializeField] GachaPull[] gacha5stars;

    // Gacha Management

    [SerializeField] private RawImage _gachaRenderImage;
    [SerializeField] private VideoPlayer _gachaVideo;
    [SerializeField] private VideoClip _3Stars;
    [SerializeField] private VideoClip _4Stars;
    [SerializeField] private VideoClip _5Stars;

    [SerializeField] private Image _gachaResult;
    private RectTransform _gachaTransform;

    void Start()
    {
        _gachaTransform = _gachaResult.gameObject.GetComponent<RectTransform>();
    }

    public void Wish()
    {
        GachaPull wish;

        float r = Random.Range(0f, 1.0f);

        if (r < 0.75) wish = gacha3stars[Random.Range(0, gacha3stars.Length)];
        else if (r < 0.9) wish = gacha4stars[Random.Range(0, gacha4stars.Length)];
        else wish = gacha5stars[Random.Range(0, gacha5stars.Length)];

        SetVideo(wish.stars);

        _gachaResult.color = Color.black;
        _gachaTransform.anchoredPosition = new Vector2(-200, _gachaTransform.anchoredPosition.y);
        _gachaResult.sprite = wish.image;
    }

    // Gacha methods

    private void SetVideo(int stars)
    {
        _gachaVideo.targetTexture.Release();

        if (stars == 3) _gachaVideo.clip = _3Stars;
        else if (stars == 4) _gachaVideo.clip = _4Stars;
        else if (stars == 5) _gachaVideo.clip = _5Stars;
        
        _gachaRenderImage.gameObject.SetActive(true);

        _gachaVideo.Play();

        StartCoroutine(StopVideoCoroutine());
    }

    private IEnumerator StopVideoCoroutine()
    {
        yield return new WaitForSeconds(4.6f);
        _gachaRenderImage.gameObject.SetActive(false);

        _gachaResult.gameObject.SetActive(true);
        _gachaResult.DOColor(Color.white, 0.8f);
        _gachaTransform.DOAnchorPosX(0, 0.5f);
    }
}
