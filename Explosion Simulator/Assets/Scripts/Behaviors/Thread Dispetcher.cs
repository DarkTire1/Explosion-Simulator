using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
    private static MainThreadDispatcher instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Оставляем объект при смене сцен
        }
        else
        {
            Destroy(gameObject); // Удаляем дублирующий объект
        }
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public static void Enqueue(Action action)
    {
        if (instance == null)
        {
            Debug.LogError("MainThreadDispatcher не инициализирован. Добавьте его в сцену.");
            return;
        }
        actions.Enqueue(action);
    }
}
