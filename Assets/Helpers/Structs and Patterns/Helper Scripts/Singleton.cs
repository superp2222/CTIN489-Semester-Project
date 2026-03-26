using UnityEngine;
/*
 * Base class for monobehavior singletons. Inherit from this class to create a monobehavior singleton.
 * A singleton is a pattern where there is only one instance of an object, which is globally accessable.
 * Use this sparingly, but it is useful for manager classes and utilities (Audio Effect Manager is a good example of this)
 * 
 * Usage:
 * public class SomeManagerClass : Singleton<SomeManagerClass>
 * {
 * 	public bool someBool;
 * }
 * 
 * to get the value of someBool anywhere in code:
 * SomeManagerClass.Instance.someBool;
 * */

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T _instance;

	private static object _lock = new object();

	public static T Instance
	{
		get
		{

			lock (_lock)
			{
				if (_instance == null )
				{
					_instance = (T)FindObjectOfType(typeof(T));
				}

				if (_instance == null)
				{
					_instance = new GameObject().AddComponent<T>();
				}

				return _instance;
			}
		}
	}

	protected void InitializePersistentSingleton()
	{
		if (_instance != null && _instance != this as T)
		{
			Destroy(gameObject);
		}
		else if(_instance==null)
		{
			_instance = this as T;
		}
		DontDestroyOnLoad(gameObject);
	}

	//this method will do a getcomponent and then an addcomponent if the given field is null
	protected void InitComponentField<T>(ref T field) where T : Component
	{
		if (field == null)
		{
			field = GetComponent<T>();
			if (field == null)
				field = gameObject.AddComponent<T>();
		}
	}

}