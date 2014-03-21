using strange.framework.api;
using System;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	internal class ItemViewProvider : IInstanceProvider
	{
		private Transform _parent;
		private GameObject _prefab;
		private const string _name = "ItemView";
		private int _id;
		private int _offset;
		public ItemViewProvider(Transform parent, int offset)
		{
			_parent = parent;
			_offset = offset;
		}
		public T GetInstance<T>()
		{
			object instance = GetInstance(typeof(T));
			T value = (T)instance;
			return value;
		}
		public object GetInstance(Type key)
		{
			if (_prefab == null)
			{
				_prefab = Resources.Load<GameObject>(_name);
			}
			GameObject instance = GameObject.Instantiate(_prefab) as GameObject;
			instance.name = _name + _id;
			instance.transform.parent = _parent.transform;
			instance.transform.position = new Vector3(_id * _offset, 0.0f, _id * _offset);
			_id++;
			return instance;
		}
	}
}
