using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public enum AnimationToShow
{
    HIT_ATTACK = 0,
    MISSED_ATTACK = 1,
    HIT_STATS_CHANGE = 2,
    MISSED_STATS_CHANGE = 3
}


public class AnimationManager : Singleton<AnimationManager>
{



    public void PlayAnimation(AnimationToShow animationToShow)
    {
        //TODO implement showing of animations
    }
}
