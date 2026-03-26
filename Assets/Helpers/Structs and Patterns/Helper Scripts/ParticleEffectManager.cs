using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleEffectManager : Singleton<ParticleEffectManager>
{
    private static Dictionary<string, GameObject> prefabs = new ();
   
    public static void PlayAtLocation(string particleName, Vector3 position, Quaternion rotation)
    {
        Instance.Play(particleName, position, rotation);    
    }

    private static Dictionary<string, ComponentPool<ParticleSystem>> pools = new();

    void Awake()
    {
        gameObject.name = "ParticleEffectManager";
    }
    
    public void Play(string particleName, Vector3 position, Quaternion rotation)
    {
        StartCoroutine(PlayCoroutine(particleName, position, rotation));
    }

    IEnumerator PlayCoroutine(string particleName, Vector3 position, Quaternion rotation)
    {
        EnsurePoolHasObjects(particleName);
        var particle = pools[particleName].Take();
        particle.transform.position = position;
        particle.transform.rotation = rotation;
        particle.Play();
        while(particle.isPlaying)
            yield return null;
        pools[particleName].Return(particle);
    }

    private static void EnsurePoolHasObjects(string particleName)
    {
        if (!pools.ContainsKey(particleName))
        {
            if (!prefabs.ContainsKey(particleName))
                prefabs.Add(particleName,  Resources.Load<GameObject>("Particles/"+particleName));
            
            var particlePrefab = prefabs[particleName];
            ParticleSystem Create()
            {
                var newParticle = Instantiate(particlePrefab, Instance.transform);
                return newParticle.GetComponent<ParticleSystem>();
            }

            pools.Add(particleName, new(Create,32,true));
        }
    }
}
