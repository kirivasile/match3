using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour {

	private static T _Instance;

	public static T Instance {
		get {
			if (_Instance == null) {
				_Instance = (T)FindObjectOfType(typeof(T));
			}

			if (_Instance == null) {
				var singletonObject = new GameObject();
				_Instance = singletonObject.AddComponent<T>();

				DontDestroyOnLoad(singletonObject);
			}

			return _Instance;
		}
	}
}
