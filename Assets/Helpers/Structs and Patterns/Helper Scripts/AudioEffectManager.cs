using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Singleton class for playing audio files on the fly from code with object pooling
 * USAGE:
 * --------------------------------------
 * put audio you want to play in Assets/Resources/Sound/
 * put a copy of this script on a gameobject in your scene (better yet, make a prefab)
 * then in a script call

 *	AudioEffectManager.PlayAudio("nameOfFileWithNoExtension"); //to play as a 2D sound
 *	AudioEffectManager.PlayAudio("nameOfFileWithNoExtension", SomeVector3); //to play as a 3D sound

 * PITCH AND VOLUME:
 * -----------------------------------
 * optional argument v sets the volume
 * optional argument p sets the pitch
 * 
 * to vary the pitch randomly (for something like footsteps) try something like this: 
 *	AudioEffectManager.playAudio("footstep", transform.position, 1f, Random.Range(0.8f, 1.1f));


 * CALLBACKS:
 * ----------------------------------
 * optional argument callback allows you to specify a function/method that gets called when the sound is done
 *
 * to call HandleSoundDone() when the sound is done, try something like this:
 *		AudioEffectManager.playAudio("footstep", transform.position, 1f, Random.Range(0.8f, 1.1f), HandleSoundDone);
 *
 * or use an "anonymous method", which would look like this:
 *		AudioEffectManager.playAudio("footstep", transform.position, 1f, Random.Range(0.8f, 1.1f), ()=>{ Debug.Log("footstep sound is done!"); });
 
  
 * OBJECT POOLING:
 *------------------------------------
 * This version uses object pooling to reuse AudioSource GameObjects instead of constantly
 * creating and destroying them, which improves performance.
 * You should see a new transform with several audio source objects nested inside when the game starts.
 * See ComponentPool for details on how it works.
*/

public class AudioEffectManager : Singleton<AudioEffectManager>
{
	//load audioclips once into this dictionary, access later by string keys
	static Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
	
	private readonly Dictionary<AudioSource, IEnumerator> _coroutines = new Dictionary<AudioSource, IEnumerator>();
	
	private ComponentPool<AudioSource> _audioSourcePool;
	
	// Parent transform to keep hierarchy clean
	private Transform _poolParent;

	//clipname: exact name of the clip
	//position: worldspace position you want the sound to play from
	//v: volume
	//p: pan
	//callback: callback for when the sound ends. leave null or you can pass a method name or anonymous function
	//AudioEffectManager.PlayAudio3D("name of clip", worldpos, 0.9f, 1f, ()=> {/* code you want to have execute when the sound ends */})
	public static void PlayAudio3D(string clipName, Vector3 position, float v = 1f, float p = 1f, System.Action callback = null)
		=> Instance._PlayAudio3D(clipName, position, v,p, callback);
	private void _PlayAudio3D(string name, Vector3 aPos, float v = 1f, float p = 1f,  System.Action callback = null)
	{
		AudioSource audioPlayer = _audioSourcePool.Take();
		if (audioPlayer == null) return;
		StopAssociatedCoroutine(audioPlayer);

		
		_coroutines[audioPlayer] = Play3D(name, audioPlayer, aPos, v, p, callback);
		StartCoroutine(_coroutines[audioPlayer]);
	}
	
	
	//clipname: exact name of the clip
	//v: volume
	//p: pan
	//callback: callback for when the sound ends. leave null or you can pass a method name or anonymous function, which would look like this:
	//AudioEffectManager.PlayAudio3D("name of clip", worldpos, 0.9f, 1f, ()=> {/* code you want to have execute when the sound ends */})
	public static void PlayAudio(string clipName, float v = 1f, float p = 1f, System.Action callback = null)
		=> Instance._PlayAudio(clipName, v, p, callback);
	void _PlayAudio(string name, float v = 1f, float p = 1f, System.Action callback = null)
	{
		AudioSource audioPlayer = _audioSourcePool.Take();
		if (audioPlayer == null) return;
		StopAssociatedCoroutine(audioPlayer);

		_coroutines[audioPlayer] = Play(name, audioPlayer, v, p, callback);
		
		StartCoroutine(_coroutines[audioPlayer]);
	}
	
	void Awake()
	{
		_poolParent = new GameObject("AudioPlayerPool").transform;
		_poolParent.SetParent(transform);
		
		// Create parent object to organize pooled audio 
		//NOTICE how I pass just the name of the method I want to use to create audio players
		_audioSourcePool = new ComponentPool<AudioSource>(CreateNewAudioPlayer);
	}

	private AudioSource CreateNewAudioPlayer()
	{
		GameObject audioPlayer = new GameObject("PooledAudioPlayer");
		var component = audioPlayer.AddComponent<AudioSource>();
		audioPlayer.transform.SetParent(_poolParent);
		audioPlayer.SetActive(false);
		return component;
	}

	private void StopAssociatedCoroutine(AudioSource aSource)
	{
		if (_coroutines.TryGetValue(aSource, out IEnumerator coroutine))
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			
			_coroutines.Remove(aSource);
		}
	}

	private void ReturnAudioPlayer(AudioSource audioSource)
	{
		if (audioSource == null) return;
		
		StopAssociatedCoroutine(audioSource);
		
		audioSource.transform.position = Vector3.zero;
		audioSource.transform.SetParent(_poolParent);
		
		// Reset AudioSource to default state
		if (audioSource != null)
		{
			audioSource.Stop();
			audioSource.clip = null;
			audioSource.volume = 1f;
			audioSource.pitch = 1f;
			audioSource.spatialBlend = 0f;
		}
		_audioSourcePool.Return(audioSource);
	}

	IEnumerator Play(string name, AudioSource audioSource, float volume, float pitch, System.Action callback)
	{
		AudioClip clip = GetClip(name);
		
		if (clip == null)
		{
			ReturnAudioPlayer(audioSource);
			yield break;
		}
		
		audioSource.spatialBlend = 0f; // 2D sound
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.PlayOneShot(clip);
		
		//it is possible for pitch to be negative, so need to use the absolute value here
		yield return new WaitForSeconds(Mathf.Abs(clip.length / pitch) + 0.1f);
		
		callback?.Invoke();
		ReturnAudioPlayer(audioSource);
	}
	
	IEnumerator Play3D(string name, AudioSource audioSource, Vector3 position, float volume, float pitch, System.Action callback)
	{
		audioSource.transform.position = position;
		AudioClip clip = GetClip(name);
		
		if (clip == null)
		{
			ReturnAudioPlayer(audioSource);
			yield break;
		}
		
		audioSource.spatialBlend = 1f;
		audioSource.minDistance = 0;
		audioSource.maxDistance = 200;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.spread = 0;
		audioSource.PlayOneShot(clip);
		
		//it is possible for pitch to be negative, so need to use the absolute value here
		yield return new WaitForSeconds(Mathf.Abs(clip.length / pitch) + 0.1f);
		
		callback?.Invoke();
		ReturnAudioPlayer(audioSource);
	}

	//lazy loads audio clips into the dictionary
	static AudioClip GetClip(string name)
	{
		if (sounds.ContainsKey(name))
			return sounds[name];
			
		AudioClip clip = Resources.Load<AudioClip>("Sound/" + name);
		if (clip != null)
			sounds.Add(name, clip);
		else
			Debug.LogError("no AudioClip named " + name + " in Resources/Sound");
			
		return clip;
	}
}