using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IcebergMaster : NetworkBehaviour
{
    [SyncVar] float serverTime = 0f;

    // Iceberg Object Pool
    [SerializeField] private int poolSize = 800;
    [SerializeField] private float icebergLifetime = 0f;
    [SerializeField] private float icebergHealth = 5f;
    [SerializeField] GameObject icebergPrefab;
    private float[] icebergDamage;
    private GameObject[] icebergs;

    public struct IcebergInfo
    {
        public float x;
        public float z;
        public float timeCreated;
        public int index;

        public IcebergInfo(float x, float z, float timeCreated, int index)
        {
            this.x = x;
            this.z = z;
            this.timeCreated = timeCreated;
            this.index = index;
        }
    };

    public class IcebergInfoList : SyncListStruct<IcebergInfo> { }
    IcebergInfoList icebergList = new IcebergInfoList();

    /*private void IcebergInfoChanged(SyncListStruct<IcebergInfo>.Operation op, int itemIndex)
     {
         if (op == SyncListStruct<IcebergInfo>.Operation.OP_ADD)
         {
             icebergs[icebergList[itemIndex].index].SetActive(true);
             icebergs[icebergList[itemIndex].index].transform.position = new Vector3(icebergList[itemIndex].x, -80f, icebergList[itemIndex].z);
             icebergs[icebergList[itemIndex].index].transform.localScale = new Vector3(1f, 0.4f, 1f);
             icebergs[icebergList[itemIndex].index].GetComponent<Iceberg>().Grow();
         }
         if (op == SyncListStruct<IcebergInfo>.Operation.OP_REMOVEAT)
         {
             icebergs[icebergList[itemIndex].index].GetComponent<Iceberg>().Shrink();
         }
    }*/

    private void Start()
    {
        // Self Identify
        gameObject.name = "IcebergMaster";
        GlobalVars.globalGameObjects["iceberg_master"] = this.gameObject;

        // Iceberg Object Pool
        //icebergList.Callback = IcebergInfoChanged;
        icebergs = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            icebergs[i] = Instantiate(icebergPrefab, transform);
            icebergs[i].GetComponent<Iceberg>().Setup(i);
            icebergs[i].SetActive(false);
        }

        if (isServer)
        {
            icebergDamage = new float[poolSize];
        }
    }
    
    private void Update()
    {
        if (isServer)
        {
            // Server Time
            serverTime += Time.deltaTime;

            // Server Iceberg Pool Management
            for (int i = 0; i < icebergList.Count; i++)
            {
                if (!icebergs[icebergList[i].index].activeSelf)
                {
                    GrowIceberg(icebergList[i].index, icebergList[i].x, icebergList[i].z, 2.8f);
                    RpcGrowIceberg(icebergList[i].index, icebergList[i].x, icebergList[i].z, 2.8f);
                }
                if ((icebergLifetime > 0f && serverTime > icebergList[i].timeCreated + icebergLifetime) || icebergDamage[icebergList[i].index] >= icebergHealth)
                {
                    float shrinkSpeed = 5.6f;
                    if (icebergDamage[icebergList[i].index] >= icebergHealth)
                        shrinkSpeed = 1.2f;

                    ShrinkIceberg(icebergList[i].index, shrinkSpeed);
                    RpcShrinkIceberg(icebergList[i].index, shrinkSpeed);
                    icebergList.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void CreateIceberg(float x, float z)
    {
        int freeID = -1;
        for (int i = 0; i < poolSize; i++)
        {
            if (!icebergs[i].activeSelf)
            {
                freeID = i;
                break;
            }
        }
        if (freeID != -1)
        {
            icebergList.Add(new IcebergInfo(x, z, serverTime, freeID));
            icebergDamage[freeID] = 0f;
        }
    }

    public void DamageIceberg(int icebergID, float damage)
    {
        icebergDamage[icebergID] += damage;
    }

    [ClientRpc(channel = 2)]
    private void RpcGrowIceberg(int index, float x, float z, float speed)
    {
        GrowIceberg(index, x, z, speed);
    }

    private void GrowIceberg(int index, float x, float z, float speed)
    {
        icebergs[index].SetActive(true);
        icebergs[index].transform.position = new Vector3(x, -40f, z);
        icebergs[index].transform.localScale = new Vector3(1f, 0.2f, 1f);
        icebergs[index].GetComponent<Iceberg>().Grow(speed);
    }

    [ClientRpc(channel = 2)]
    private void RpcShrinkIceberg(int index, float speed)
    {
        ShrinkIceberg(index, speed);
    }

    private void ShrinkIceberg(int index, float speed)
    {
        icebergs[index].GetComponent<Iceberg>().Shrink(speed);
    }

    public void ResetMatch()
    {
        if (isServer)
        {
            // Destroy all of those damn icebergs
            for (int i = icebergList.Count; i < 0; i--)
            {
                ShrinkIceberg(icebergList[0].index, 0.1f);
                RpcShrinkIceberg(icebergList[0].index, 0.1f);
                icebergList.RemoveAt(0);
            }
        }
    }
}
