using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum AnimationToShow
{
    HIT_ATTACK = 0,
    MISSED_ATTACK = 1,
    HIT_STATS_CHANGE = 2,
    MISSED_STATS_CHANGE = 3,
    CRIT = 4,
}


public class AnimationManager : Singleton<AnimationManager>
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject magicShield;
    [SerializeField] private GameObject missText;
    [SerializeField] private GameObject critText;
    [SerializeField] private GameObject simpleLightExplosion;


    public void PlayAnimation(AnimationToShow animationToShow, Transform shipTransform)
    {
        switch (animationToShow)
        {
            case AnimationToShow.HIT_ATTACK:
                Instantiate(explosion, shipTransform);
                break;

            case AnimationToShow.MISSED_ATTACK:
                Instantiate(missText, shipTransform);
                break;

            case AnimationToShow.HIT_STATS_CHANGE: 
                Instantiate(magicShield, shipTransform);
                break;

            case AnimationToShow.CRIT:
                Instantiate(critText, shipTransform);
                break;

            default:
                break;
        }
    }
}
