using System.Collections.Generic;
using UnityEngine;

namespace Immersive_Toolkit.Runtime
{
    public class ReferenceManager : MonoBehaviour, IReferenceManager
    {
        [SerializeField] private string _path = "Resources/Prefabs/";
        [SerializeField] private List<GameObject> _initialObjectsToSpawn;
        private List<GameObject> _references;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            LoadPrefabs();
            SpawnPrefabs();
        }

        private void OnDisable()
        {
            foreach (var obj in _initialObjectsToSpawn)
            {
                Destroy(obj);
            }
        }

        private void SpawnPrefabs()
        {
            if (_initialObjectsToSpawn == null) return;

            for (int i = 0; i < _initialObjectsToSpawn.Count; i++)
            {
                var originalName = _initialObjectsToSpawn[i].name;
                _initialObjectsToSpawn[i] = Instantiate(_initialObjectsToSpawn[i]);
                _initialObjectsToSpawn[i].name = originalName;
            }
        }

        private void LoadPrefabs()
        {
            _references = new List<GameObject>();
            var assets = Resources.LoadAll(_path, typeof(GameObject));

            if (assets.Length == 0)
            {
                Debug.LogWarning($"No assets found in path: {_path}");
                return;
            }

            ProcessLoadedGameObject(assets);
        }

        private void ProcessLoadedGameObject(Object[] assets)
        {
            foreach (var asset in assets)
            {
                if (asset is GameObject gameObject)
                {
                    _references.Add(gameObject);
                    Debug.Log($"Loaded asset: {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"Failed to load asset at path '{_path}' as GameObject.");
                }
            }
        }
    }
}