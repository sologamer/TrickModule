using System;
using System.Collections;
using BeauRoutine;
using TrickModule.Game;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    private void Start()
    {
        Routine.StartLoopRoutine(Loop);

        IEnumerator Loop()
        {
            yield return Routine.WaitSeconds(1.0f);
            UIManager.Instance.TryShow<TestMenu>();
            yield return Routine.WaitSeconds(2.5f);
            UIManager.Instance.TryHide<TestMenu>();
            yield return Routine.WaitSeconds(2.5f); 
        }
    }
}