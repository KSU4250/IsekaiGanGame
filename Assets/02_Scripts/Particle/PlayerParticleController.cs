using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerParticleController : NetworkBehaviour
{
    [Serializable]
    private class PlayerParticleData
    {
        public string particleName = string.Empty;
        public GameObject particlePrefab = null;
        public bool forceDestroyable = false;
        public float autoDestroyTime = 1f;
    }

    [SerializeField]
    private List<PlayerParticleData> particleDataList = null;

    private List<PoolableParticle> forceReturnParticleList = null;
    private PlayerManager playerManager = null;

    private Dictionary<string, ObjectPoolManager<PoolableParticle>> particlePoolManagerMap = null;

    /// <summary>
    /// 플레이어의 위치에 파티클을 생성함.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToPlayer(string _particleName)
    {
        SpawnParticle(_particleName, transform.position, transform.rotation);
    }

    /// <summary>
    /// 플레이어가 마지막으로 지정한 스킬 위치에 파티클을 생성함.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToSkillUsedPoint(string _particleName)
    {
        SpawnParticle(_particleName, playerManager.InputManager.lastSkillUsePoint, transform.rotation);
    }

    /// <summary>
    /// 플레이어의 근접 무기 위치에 파티클을 생성함.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToMeleeWeapon(string _particleName)
    {
        SpawnParticle(
            _particleName,
            playerManager.AttackManager.GetMeleeWeaponPostion(),
            transform.rotation);
    }

    /// <summary>
    /// 플레이어의 원거리 공격 시작 위치에 파티클을 생성함.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToRangeWeapon(string _particleName)
    {
        SpawnParticle(_particleName,
            playerManager.AttackManager.RangeAttackTransform.position,
            playerManager.transform.rotation);
    } 

    public void ForceDestroyParticles()
    {
        foreach(var particle in forceReturnParticleList)
        {
            if (particle != null)
                particle.ReturnToPool();
        }

        forceReturnParticleList.Clear();
    }

    private void SpawnParticle(string _particleName, Vector3 _position, Quaternion _rotation)
    {
        if (GameManager.Instance.IsLocalGame)
            SpawnParticleLocal(_particleName, _position, _rotation);
        else if (playerManager.PlayerNetworkManager.IsClientPlayer())
            SpawnParticleServerRpc(_particleName, _position, _rotation);
    }

    private PlayerParticleData GetParticleData(string _particleName)
    {
        foreach(var particleData in particleDataList)
        {
            if (particleData.particleName == _particleName)
            {
                return particleData;
            }
        }

        return null;
    }

    [ServerRpc]
    private void SpawnParticleServerRpc(string _particleName, Vector3 _position, Quaternion _rotation)
    {
        SpawnParticleClientRpc(_particleName, _position, _rotation);
    }

    [ClientRpc]
    private void SpawnParticleClientRpc(string _particleName, Vector3 _position, Quaternion _rotation)
    {
        SpawnParticleLocal(_particleName, _position, _rotation);
    }

    private void SpawnParticleLocal(string _particleName, Vector3 _position, Quaternion _rotation)
    {
        var particleData = GetParticleData(_particleName);

        if (particleData == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        var poolableParticle = particlePoolManagerMap[_particleName].Get();

        poolableParticle.Init(_position, _rotation, particleData.autoDestroyTime);

        //var spawnedParticleObj = Instantiate(particleData.particlePrefab, _position, _rotation);
        //Destroy(spawnedParticleObj, particleData.autoDestroyTime);

        if (particleData.forceDestroyable)
            forceReturnParticleList.Add(poolableParticle);
    }

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();

        forceReturnParticleList = new List<PoolableParticle>();

        particlePoolManagerMap = new Dictionary<string, ObjectPoolManager<PoolableParticle>>();

        foreach(var particleData in particleDataList)
        {
            particlePoolManagerMap.Add(
                particleData.particleName,
                new ObjectPoolManager<PoolableParticle>
                (particleData.particlePrefab.GetComponent<PoolableParticle>())
                );
        }
    }
}
