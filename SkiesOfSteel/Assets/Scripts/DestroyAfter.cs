using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : StateMachineBehaviour
{
    [SerializeField] private bool destroyParent = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (destroyParent) Destroy(animator.gameObject.transform.parent.gameObject);
        else Destroy(animator.gameObject);
    }
}
