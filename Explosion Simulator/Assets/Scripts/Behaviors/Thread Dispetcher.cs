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
            DontDestroyOnLoad(gameObject); // ��������� ������ ��� ����� ����
        }
        else
        {
            Destroy(gameObject); // ������� ����������� ������
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
            Debug.LogError("MainThreadDispatcher �� ���������������. �������� ��� � �����.");
            return;
        }
        actions.Enqueue(action);
    }
}
