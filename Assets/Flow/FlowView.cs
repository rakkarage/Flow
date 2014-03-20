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
		public float TimeInertia = 0.5f;
		public int Offset = 1;
		public bool Clamp = true;
		private int _clamp;
		private int _current;
		private const int _limitSide = 4;
		private const int _limit = (_limitSide * 2) + 1;
		private List<int> _data = Enumerable.Range(111, 10).ToList();
		private List<GameObject> _views = Enumerable.Repeat((GameObject)null, _limit).ToList();
		private List<int> _tweens = Enumerable.Repeat(0, _limit).ToList();
		private int _tweenInertia;
		[Inject]
		public IPool<GameObject> ItemViewPool { get; set; }
		[PostConstruct]
		public void PostConstruct()
		{
			ItemViewPool.instanceProvider = new ItemViewProvider(transform, Offset);
			ItemViewPool.inflationType = PoolInflationType.INCREMENT;
			Load();
			_clamp = _data.Count * Offset + 1;
		}
		public int GetClosestIndex()
		{
			int closestIndex = -1;
			float closestDistance = float.MaxValue;
			for (int i = 0; i < _views.Count; i++)
			{
				if (_views[i])
				{
					float distance = (Vector3.zero - _views[i].transform.localPosition).sqrMagnitude;
					if (distance < closestDistance)
					{
						closestIndex = i;
						closestDistance = distance;
					}
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
				if (_views[i])
				{
					if (view == _views[i])
					{
						found = i;
					}
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
		private void FlowSnapItemCancel(int viewIndex)
		{
			LeanTween.cancel(_views[viewIndex], _tweens[viewIndex]);
		}
		private void FlowSnapItem(int viewIndex, int delta)
		{
			Vector3 to = new Vector3(delta * Offset, 0.0f, Mathf.Abs(delta) * Offset);
			_tweens[viewIndex] = LeanTween.moveLocal(_views[viewIndex], to, TimeTween).setEase(LeanTweenType.easeSpring).id;
		}
		public void FlowSnap(int target)
		{
			List<GameObject> newViews = Enumerable.Repeat((GameObject)null, _limit).ToList();
			for (int i = 0; i < _data.Count; i++)
			{
				int delta = GetDelta(target, i);
				int viewIndex = GetViewIndex(delta);
				int oldDelta = GetDelta(_current, i);
				int oldViewIndex = GetViewIndex(oldDelta);
				bool isVisible = IsVisible(delta);
				bool wasVisible = IsVisible(oldDelta);
				if (wasVisible && !isVisible)
				{
					FlowSnapItemCancel(oldViewIndex);
					Exit(_views[oldViewIndex]);
					_views[oldViewIndex] = null;
				}
				else if (isVisible && !wasVisible)
				{
					newViews[viewIndex] = Enter(_data[i]);
				}
				else if (isVisible)
				{
					FlowSnapItemCancel(viewIndex);
					newViews[viewIndex] = _views[oldViewIndex];
				}
			}
			_views = newViews;
			for (int i = 0; i < _data.Count; i++)
			{
				int delta = GetDelta(target, i);
				int viewIndex = GetViewIndex(delta);
				bool isVisible = IsVisible(delta);
				if (isVisible)
				{
					FlowSnapItem(viewIndex, delta);
				}
			}
			_current = target;
		}
		private void FlowPanItem(int i, float y, float delta, bool negative)
		{
			Vector3 newP;
			if (Clamp)
			{
				float clampX = Mathf.Clamp(delta, ClampXMin(i, negative), ClampXMax(i, negative));
				float clampZ = Mathf.Clamp(Mathf.Abs(delta), 0.0f, ClampXMax(i, negative));
				newP = new Vector3(clampX, y, clampZ);
			}
			else
			{
				newP = new Vector3(delta, y, Mathf.Abs(delta));
			}
			_views[i].transform.localPosition = newP;
		}
		public void FlowPan(float offset)
		{
			GameObject[] newViews = new GameObject[] { null };
			for (int i = 0; i < _data.Count; i++)
			{
				//Vector3 p = _views[i].transform.localPosition;
				//float newX = p.x + offset;
				//bool negative = newX < 0;
				//bool wasVisible = IsVisible(i);
				//bool isVisible = IsVisible(newX);

				if (IsVisible(i))
				{
					int delta = ((int)offset - i) * -1;
					int viewIndex = delta + _limitSide;
					int oldDelta = (_current - i) * -1;
					int oldViewIndex = oldDelta + _limitSide;
					Debug.Log(i + ":" + delta + ":" + viewIndex + ":" + oldDelta + ":" + oldViewIndex);
				}
				//if (wasVisible && !isVisible)
				//{
				//	LeanTween.cancel(_views[i], _tweens[i]);
				//	Exit(i);
				//}
				//else if (isVisible && !wasVisible)
				//{
				//	Enter(i, newOrder);
				//	FlowPanItem(newOrder, p.y, newX, negative);
				//}
				//if (isVisible)
				//{
				//	LeanTween.cancel(_views[i], _tweens[i]);
				//	FlowPanItem(newOrder, p.y, newX, negative);
				//}
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
			_tweenInertia = LeanTween.value(gameObject, FlowPan, velocity, 0, TimeInertia).setEase(LeanTweenType.easeInCirc).setOnComplete(Flow).id;
		}
		public void InertiaStop()
		{
			LeanTween.cancel(gameObject, _tweenInertia);
		}
		private GameObject Enter(int data)
		{
			GameObject view = ItemViewPool.GetInstance();
			view.SetActive(true);
			view.GetComponentInChildren<TextMesh>().text = data.ToString("X");
			return view;
		}
		private void Exit(GameObject view)
		{
			view.SetActive(false);
			ItemViewPool.ReturnInstance(view);
		}
		private void Load()
		{
			for (int i = 0; i < _data.Count && i < _limitSide; i++)
			{
				_views[GetViewIndex(GetDelta(0, i))] = Enter(_data[i]);
			}
		}
		private bool IsVisible(int dataIndex)
		{
			return (System.Math.Abs(dataIndex) < _limitSide);
		}
		private int GetViewIndex(int delta)
		{
			return delta + _limitSide;
		}
		private int GetDelta(int target, int dataIndex)
		{
			return (target - dataIndex) * -1;
		}
		public void Next()
		{
			if (_current < _data.Count - 1)
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
			const float size = 64.0f;
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
			string text = string.Format("{0}/{1}", _current + 1, _data.Count);
			GUI.Box(new Rect(offset.x, offset.y, size, size), text, centeredStyle);
		}
	}
}
