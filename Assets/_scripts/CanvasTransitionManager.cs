using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CanvasTransitionManager : MonoBehaviour
{
    public static CanvasTransitionManager Instance;
    private GameManager gameManager;

    [SerializeField] private Animator mAnimator;

    [AnimatorParam("mAnimator"), SerializeField]
    private string enterParam;
    [AnimatorParam("mAnimator"), SerializeField]
    private string exitParam;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        mAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        gameManager.OnLoadScene += TransitionOn;
        gameManager.OnLoadedScene += TransitionOff;
    }

    public void TransitionOn()
    {
        mAnimator.SetTrigger(enterParam);
    }

    public void TransitionOff()
    {
        mAnimator?.SetTrigger(exitParam);
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
    }
}
