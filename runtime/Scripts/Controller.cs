using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public static T Get<T>() where T : Controller
    {
        return Main.Instance.GetController<T>();
    }

    public virtual void Cache() { }
    public virtual void Init() { }

    public virtual void LoadData()
    {

    }
}
