using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This script is NOT a  monobehavior - you can't attach it to game objects in your scene
//See AudioEffectManager for an example of it's usage.

//it implements a simple object pool for components (of gameobjects)

//Object pooling is a technique that allows us to recycle objects rather than destroy them.
//we maintain a hidden 'pool' of disabled objects that we draw from, rather than creating / instantiating objects

public class ComponentPool<T> where T:Component
{
    public int Count => _available.Count;
    public int Capacity => _capacity;

    private Queue<T> _available = new Queue<T>();
    private HashSet<T> _taken = new HashSet<T>();
    private Func<T> _create;
    
    private int _capacity;
    private bool _dynamicResize;
    
    /// <summary>
    /// Constructor with optional parameters!
    /// Make sure to pass a createMethod (this could be an anonymous method, or just the name of a function that returns a component
    /// </summary>
    /// <param name="createMethod">Method to create an object for this pool</param>
    /// <param name="initialCapacity">how many objects to create at the start</param>
    /// <param name="dynamicResize">if enabled, capacity will double if the pool ever runs out</param>
    public ComponentPool(Func<T> createMethod, int initialCapacity = 32, bool dynamicResize = true )
    {
        _create = createMethod;
        _capacity = initialCapacity;
        _dynamicResize = dynamicResize;
        
        for (int i = 0; i < initialCapacity; i++)
        {
            var component = _create();
            component.gameObject.SetActive(false);
            _available.Enqueue(component);
        }
    }

    private void DoubleCapacity()
    {
        for (int i = 0; i < _capacity; i++)
        {
            var component = _create();
            component.gameObject.SetActive(false);
            _available.Enqueue(component);
        }

        _capacity *= 2;

    }

    /// <summary>
    /// Takes a component from the object pool and activates it (if an object is available).
    /// If dynamicResize enabled, this will double the size of pool if it ever runs out.
    /// MAKE SURE to eventually return objects you take!
    /// </summary>
    /// <returns>Component to take</returns>
    public T Take()
    {
        if (!_available.Any())
        {
            if (!_dynamicResize)
            {
                Debug.LogError("No more components available in the pool. Make sure to Return components when you are done, or enable dynamic resizing (optional parameter in the constructor).");
                return null;
            }

            DoubleCapacity();
        }
        
        var component = _available.Dequeue();
        component.gameObject.SetActive(true);
        _taken.Add(component);
        return component;
    }

    /// <summary>
    /// Returns the component to the pool and deactivates it's gameobject
    /// </summary>
    /// <param name="component"></param>
    public void Return(T component)
    {
        if (!_taken.Contains(component))
            return;
        
        _taken.Remove(component);
        _available.Enqueue(component);
        component.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this to return all objects to the pool immediately (useful when cleaning up)
    /// </summary>
    public void ReturnAll()
    {
        //can't modify the _taken hashset while iterating over it,
        //so I use.ToArray() to make a new temporary object to iterate over.
        foreach(var component in _taken.ToArray())
            Return(component);
    }
}
