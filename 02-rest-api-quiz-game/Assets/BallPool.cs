using System.Collections.Generic;
using UnityEngine;

public class BallPool
{
    readonly Stack<Ball> pool = new Stack<Ball>();
    readonly GameObject prefab;
    readonly Transform parent;

    //볼 미리 생성해둬서 pool에 넣어두기
    public BallPool(GameObject prefab, Transform parent, int preWarm = 1)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < preWarm; i++)
            Return(Instantiate());
    }

    //공을 어쨌던 한번은 생성되어야 하니까 생성
    Ball Instantiate() => GameObject.Instantiate(prefab, parent).GetComponent<Ball>();
    
    //화면에 숨기고 pool 창고에 둠
    public void Return(Ball b)
    {
        b.gameObject.SetActive(false);
        pool.Push(b);
    }

    //pool에 대기중이면 꺼내고, 없으면 새로 만들기
    public Ball Get()
    {
        Ball b = pool.Count > 0 ? pool.Pop() : Instantiate();
        b.gameObject.SetActive(true);
        return b;
    }
}

public class PopPool
{
    readonly Stack<GameObject> pool = new Stack<GameObject>();
    readonly GameObject prefab;
    readonly Transform parent;

    public PopPool(GameObject prefab, Transform parent, int preWarm = 1)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < preWarm; i++)
            Return(Instantiate());
    }

    GameObject Instantiate() => GameObject.Instantiate(prefab, parent);

    //화면에 숨기고 pool 창고에 둠
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Push(obj);
    }

    public GameObject Get()
    {
        GameObject obj = pool.Count > 0 ? pool.Pop() : Instantiate();
        obj.SetActive(true);
        return obj;
    }
}
