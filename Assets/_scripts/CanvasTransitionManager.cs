using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class CanvasTransitionManager : MonoBehaviour
{
    public static CanvasTransitionManager Instance;

    [SerializeField] private Animator mAnimator;
    AnimatorStateInfo stateInfo;

    [AnimatorParam("mAnimator"), SerializeField]
    private string enterParam;
    [AnimatorParam("mAnimator"), SerializeField]
    private string exitParam;
    private Action OnAnimationEnd;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        mAnimator = GetComponent<Animator>();
        stateInfo = mAnimator.GetCurrentAnimatorStateInfo(0); // 0 est l'index du premier layer
    }

    public void PlayTransition(Action callback)
    {
        Transitions();
        OnAnimationEnd = callback;
    }

    public void TransitionOn()
    {
        mAnimator.SetTrigger(enterParam);
    }

    public void TransitionOff()
    {
        mAnimator?.SetTrigger(exitParam);
        OnAnimationEnd?.Invoke();
    }

    private void Transitions()
    {
        TransitionOn();
        TransitionOff();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transitions();
        }

        if (stateInfo.normalizedTime >= 1 && stateInfo.IsName(enterParam) || stateInfo.IsName(exitParam))
        {
            Debug.Log("L'animation est en cours.");
        }
    }
}
