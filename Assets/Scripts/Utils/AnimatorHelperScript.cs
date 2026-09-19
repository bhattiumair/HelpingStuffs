using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHelperScript : MonoBehaviour
{
    public System.Action<int> OnFunctionCalled;
    public Animator animator;

    [Header("Testing")]
    public string stateName;


    private void Awake()
    {
        if (animator == null)
        {
            animator = this.gameObject.GetComponent<Animator>();
        }
    }

    public void PlayState(string state,System.Action<int> OnFunction)
    {
        if (OnFunction != null)
            OnFunctionCalled = OnFunction;
        animator.Play(state);
    }

    public void SetTrigger(string triggerName,System.Action<int> OnFunction)
    {
        if (OnFunction != null)
            OnFunctionCalled = OnFunction;
        animator.SetTrigger(triggerName);
    }
    public void OnFunctionCall(int index)
    {
        OnFunctionCalled?.Invoke(index);
    }

    [ContextMenu("Test Play State")]
    private void TestPlayState()
    {
        PlayState(stateName, null);
    }
}