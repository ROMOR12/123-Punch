using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Este scrpt ya no hace nada, no lo borro por si en un futuro sirve, pero no creo
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    private static UnityMainThreadDispatcher _instance = null;

    public static UnityMainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            _instance = FindFirstObjectByType<UnityMainThreadDispatcher>();

            if (!_instance)
            {
                var obj = new GameObject("MainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
