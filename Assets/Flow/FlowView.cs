using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class FlowView : View
	{
		public float TimeTween = 0.333f;
		public int Offset = 1;
		public bool Clamp = true;
		private int _clamp;
		private int _current;
		private const int _startAt = 111;
		private const int _startCount = 100;
		private List<GameObject> _views = new List<GameObject>(_startCount);
		private List<int> _data = Enumerable.Range(_startAt, _startCount).ToList();
		private List<int> _tweens = Enumerable.Repeat(-1, _startCount).ToList();
		private int _tweenInertia;
		[Inject]
		public IPool<GameObject> ItemViewPool { get; set; }
		[PostConstruct]
		public void PostConstruct()
		{
			ItemViewPool.instanceProvider = new ItemViewProvider(transform);
			ItemViewPool.size = 0;
			ItemViewPool.inflationType = PoolInflationType.INCREMENT;
			ItemViewPool.overflowBehavior = PoolOverflowBehavior.WARNING;
			Load();
		}
		public int GetClosestIndex()
		{
			int closestIndex = -1;
			float closestDistance = float.MaxValue;
			for (int i = 0; i < _views.Count; i++)
			{
				float distance = (Vector3.zero - _views[i].transform.localPosition).sqrMagnitude;
				if (distance < closestDistance)
				{
					closestIndex = i;
					closestDistance = distance;
				}
			}
			return closestIndex;
		}
		public void Flow()
		{
			FlowSnap(GetClosestIndex());
		}
		private int GetIndex(GameObject view)
		{
			int found = -1;
			for (int i = 0; i < _views.Count; i++)
			{
				if (view == _views[i])
				{
					found = i;
				}
			}
			return found;
		}
		public void FlowTo(GameObject target)
		{
			int found = GetIndex(target);
			if (found != -1)
			{
				FlowSnap(found);
			}
		}
		public void FlowSnap(int target)
		{
			for (int i = 0; i < _views.Count; i++)
			{
				int delta = (target - i) * -1;
				if (_tweens[i] != null) LeanTween.cancel(_views[i], _tweens[i]);
				Vector3 to = new Vector3(delta * Offset, 0.0f, Mathf.Abs(delta) * Offset);
				_tweens[i] = LeanTween.moveLocal(_views[i], to, TimeTween).setEase(LeanTweenType.easeSpring).id;
			}
			_current = target;
		}
		public void FlowPan(float offset)
		{
			for (int i = 0; i < _views.Count; i++)
			{
				Vector3 p = _views[i].transform.localPosition;
				float newX = p.x + offset;
				bool negative = newX < 0;
				Vector3 newP;
				if (Clamp)
				{
					float clampX = Mathf.Clamp(newX, ClampXMin(i, negative), ClampXMax(i, negative));
					float clampZ = Mathf.Clamp(Mathf.Abs(newX), 0.0f, ClampXMax(i, negative));
					newP = new Vector3(clampX, p.y, clampZ);
				}
				else
				{
					newP = new Vector3(newX, p.y, Mathf.Abs(newX));
				}
				if (_tweens[i] != null) LeanTween.cancel(_views[i], _tweens[i]);
				_views[i].transform.localPosition = newP;
			}
		}
		private float ClampXMin(int index, bool negative)
		{
			float newIndex = negative ? index : newIndex = _views.Count - index - 1;
			return -(_clamp - (Offset * newIndex));
		}
		private float ClampXMax(int index, bool negative)
		{
			float newIndex = negative ? index : newIndex = _views.Count - index - 1;
			return _clamp - (Offset * newIndex);
		}
		public void Inertia(float velocity)
		{
			_tweenInertia = LeanTween.value(gameObject, FlowPan, velocity, 0, 0.5f).setEase(LeanTweenType.easeInExpo).setOnComplete(Flow).id;
		}
		public void InertiaStop()
		{
			LeanTween.cancel(gameObject, _tweenInertia);
		}
		private void Load()
		{
			for (int i = 0; i < _data.Count; i++)
			{
				Add(i);
			}
		}
		private void Add()
		{
			_data.Add(_data.Count);
			Add(_data.Count - 1);
		}
		private void Add(int i)
		{
			GameObject itemView = ItemViewPool.GetInstance();
			itemView.GetComponentInChildren<TextMesh>().text = _data[i].ToString("X");
			_views.Add(itemView);
			if (_views.Count > _tweens.Capacity)
				_tweens.Capacity = _views.Count;
			_clamp = _views.Count * Offset + 1;
		}
		private void Remove()
		{
			// todo: !!!
		}
		public void Next()
		{
			if (_current < _views.Count - 1)
			{
				FlowSnap(_current + 1);
			}
		}
		public void Prev()
		{
			if (_current > 0)
			{
				FlowSnap(_current - 1);
			}
		}
		protected void OnGUI()
		{
			float size = 64.0f;
			Vector2 offset = new Vector2(10.0f, 10.0f);
			if (GUI.Button(new Rect(offset.x, offset.y, size, size), "<"))
			{
				Prev();
			}
			offset.y += size + offset.x;
			if (GUI.Button(new Rect(offset.x, offset.y, size, size), ">"))
			{
				Next();
			}
			offset.y += size + offset.x;
			var centeredStyle = GUI.skin.GetStyle("Box");
			centeredStyle.alignment = TextAnchor.MiddleCenter;
			string text = string.Format("{0}/{1}", _current + 1, _views.Count);
			GUI.Box(new Rect(offset.x, offset.y, size, size), text, centeredStyle);
			offset.y += size + offset.x;
			if (GUI.Button(new Rect(offset.x, offset.y, size, size), "+"))
			{
				Add();
			}
			offset.y += size + offset.x;
			if (GUI.Button(new Rect(offset.x, offset.y, size, size), "-"))
			{
				Remove();
			}
		}
	}
}
