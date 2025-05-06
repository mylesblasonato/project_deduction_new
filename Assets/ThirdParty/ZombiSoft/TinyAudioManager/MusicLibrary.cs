//==============================================================
// Sound Library (Music)
//==============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLibrary : MonoBehaviour 
{
	public MusicGroup[] musicGroups;

	Dictionary<string, AudioClip> groupDictionary = new Dictionary<string, AudioClip>();

	void Awake() 
	{
		foreach (MusicGroup musicGroup in musicGroups)
		{
			groupDictionary.Add(musicGroup.name, musicGroup.clip);
		}
	}

	public AudioClip GetClipFromName(string name) 
	{
		if (groupDictionary.ContainsKey (name)) 
		{
			AudioClip music = groupDictionary[name];
			return music;
		}
		return null;
	}

	[System.Serializable]
	public class MusicGroup
	{
		public string name;
		public AudioClip clip;
	}
}