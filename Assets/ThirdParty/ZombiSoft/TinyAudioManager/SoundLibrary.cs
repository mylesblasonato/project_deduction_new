﻿//==============================================================
// Sound Library (FX Audio)
//==============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour 
{
	public SoundGroup[] soundGroups;

	Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

	void Awake() 
	{
		foreach (SoundGroup soundGroup in soundGroups)
		{
			groupDictionary.Add (soundGroup.name, soundGroup.clip);
		}
	}

	public AudioClip GetClipFromName(string name) 
	{
		if (groupDictionary.ContainsKey (name)) 
		{
			AudioClip[] sounds = groupDictionary [name];
			return sounds [Random.Range (0, sounds.Length)];
		}
		return null;
	}

	[System.Serializable]
	public class SoundGroup
	{
		public string name;
		public AudioClip[] clip;
	}
}
