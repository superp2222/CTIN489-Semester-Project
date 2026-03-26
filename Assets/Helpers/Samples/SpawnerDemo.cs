using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class SpawnerDemo : MonoBehaviour
{
    //use serializefield to make private variables assignable in the editor!
    [SerializeField] private GameObject prefab;
    [SerializeField] private Renderer ground;
    
    private ComponentPool<Transform> pool;
    
    //keeps track of currently spawned objects (did this as a stack to make it easy to despawn the last spawned item)
    private Stack<Transform> spawned = new ();


    private const string despawnParticleName = "DespawnCubeParticle";
    private const string spawnSound = "menu-open";
    private const string despawnSound = "menu-close";
    private const string particleSound = "axe-impact-chop-wood";
    
    public bool useParticle = false;
    public bool useSound = false;
    public bool showErrorMessage = true;
    public bool useTweening = true;
    public float tweenDuration = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        pool = new(CreateMethod);
    }

    private Transform CreateMethod()
    {
        Transform t = Instantiate(prefab).transform;
        t.SetParent(transform);
        return t;
    }


    //this method is called by a button click in the scene - check out the buttons inside the canvas to see
    public void SpawnObject()
    {
        //in the case that we need to despawn an existing object, we need to wait for the possible despawn animation to finish first
        if(pool.Count==0 && spawned.Count>0)
            DespawnObject(SpawnImmediate);
        else
            SpawnImmediate();
    }

    private void SpawnImmediate()
    {
        var newTransform = pool.Take();
        spawned.Push(newTransform);
        //position the object above the ground
        newTransform.transform.position = ground.transform.position + new Vector3(
            Random.Range(-1f, 1f) * ground.bounds.extents.x,
            Random.Range(1f, 3f),
            Random.Range(-1f, 1f) * ground.bounds.extents.z);
        
        newTransform.transform.localScale = Vector3.one;
        
        if(useSound)
            AudioEffectManager.PlayAudio3D(spawnSound, newTransform.transform.position);

        if (useTweening)
        {
            
            //tween position
            Tween.Position(newTransform, newTransform.position + Vector3.up * 3, newTransform.position, tweenDuration)
                .SetEase(Tween.Easing.EaseOutBack)
                .Play();
            
            //tween scale
            Tween.Scale(newTransform, Vector3.zero, Vector3.one, tweenDuration)
                .SetEase(Tween.Easing.EaseOutBack)
                .Play();
            
            //pick randomized values for rotations
            Vector3 startRotation = Vector3.up * Random.Range(-180, 180f);
            Vector3 endRotation= Vector3.up * Random.Range(-180, 180f);
            
            //tween rotation
            Tween.Rotate(newTransform, startRotation, endRotation, tweenDuration)
                .SetEase(Tween.Easing.EaseOutBack)
                .Play();
        }
    }

    
    //this is the method that gets called by the button
    public void DespawnObject()=>DespawnObject(null);
    
    //this private version of despawn object allows us to use a callback - a method that will get called later when this method is done.
    void DespawnObject(Action onComplete)
    {

        if (spawned.Count == 0)
        {
            if(showErrorMessage) MessageDisplay.ShowMessage("No more objects to despawn!");
            return;
        }

        //remove the next transform to despawn from the stack right away, cache it in this variable
        Transform toDespawn = spawned.Pop();
        

        if (!useTweening)
        {
            //return the transform to the pool, and call the callback to signal we are done.
            pool.Return(toDespawn);
            onComplete?.Invoke();
            return;
        }
        
        if(useSound)
            AudioEffectManager.PlayAudio3D(despawnSound, toDespawn.transform.position);
        
        Tween.Scale(toDespawn, Vector3.one, Vector3.zero, tweenDuration)
            .SetEase(Tween.Easing.EaseInQuadratic)
            .OnComplete(() =>
            {
                pool.Return(toDespawn);//return the transform to the pool
                onComplete?.Invoke(); //raise onComplete to signal we are done!
                
                //play the particle effect
                if(useParticle)
                    ParticleEffectManager.PlayAtLocation(despawnParticleName, toDespawn.transform.position, toDespawn.transform.rotation);
                
                if(useSound)
                    AudioEffectManager.PlayAudio3D(particleSound, toDespawn.transform.position);
            })
            .Play();
        
        Vector3 startRotation = toDespawn.rotation.eulerAngles;
        Vector3 endRotation = startRotation + Vector3.up * -90f;
        
        Tween.Rotate(toDespawn, startRotation, endRotation, tweenDuration)
            .SetEase(Tween.Easing.EaseInQuadratic)
            .Play();
    }
}
