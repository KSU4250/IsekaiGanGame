using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestForRefer : MonoBehaviour
{
    public GameObject A;
    public GameObject B;
    public GameObject C;

    public GameObject[] refer1 = new GameObject[3];
    public GameObject[] refer2;

    public GameObject refer3;
    public GameObject refer4;

    public int[] refer5 = new int[3];
    public int[] refer6;

    public GameObject[] refer7 = new GameObject[3];
    public GameObject[] refer8;

    public List<GameObject> referList;
    public List<GameObject> referList2;

    public List<int> referList3;
    public List<int> referList4;
    private void Start()
    {
        refer2 = (GameObject[])refer1.Clone();
        refer4 = refer3;
        refer6 = refer5;
        refer8 = refer7;

        referList = new List<GameObject>() { A, B, C };
        referList2 = referList.ToList();

        referList3 = new List<int>() { 1, 2, 3 };
        referList4 = referList3.ToList();
    }

    private void Update()
    {
        // 리스트 참조 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            refer1[0] = null;
        }

        // 그냥 게임오브젝트 참조 테스트
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            refer3 = null;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            refer5 = null;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            refer8[0] = null;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            referList.Remove(A);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            referList3.Remove(1);
        }
    }
}
