using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public static Main Instance { get; private set; }


    [SerializeField] private List<Controller> controllers = new List<Controller>();


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        CacheControllers();
        LoadDataControllers();
        InitControllers();
    }
    private void CacheControllers()
    {
        controllers.AddRange(FindObjectsOfType<Controller>());

        foreach (var controller in controllers)
        {
            controller.Cache();
        }

    }

    private void LoadDataControllers()
    {
        foreach (var controller in controllers)
        {
            controller.LoadData();

        }
    }

    private void InitControllers() // Переписал эту функцию, чтобы BenchControl.Init()  вызывался в последнюю очередь
    {

        foreach (var controller in controllers)
        {
            controller.Init();

        }

    }

    public T GetController<T>() where T : Controller
    {
        return controllers.Find(c => c is T) as T;
    }
}
