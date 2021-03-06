﻿using strange.extensions.mediation.impl;
using strange.extensions.pool.api;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class FlowView : View
	{
		public float TimeTween = .333f;
		public float TimeInertia = .5f;
		public int Offset = 1;
		private float _current;
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
			for (int i = 0; (i < _data.Count) && (i < _limitSide); i++)
			{
				int viewIndex = GetViewIndex(GetDelta(_current, i));
				_views[viewIndex] = Enter(i);
				UpdateName(_views[viewIndex], viewIndex, i);
			}
		}
		public void Flow()
		{
			FlowSnap(GetClosestViewIndex());
		}
		public void FlowTo(GameObject o)
		{
			FlowSnap(GetViewIndex(o));
		}
		private void FlowSnapItemCancel(int viewIndex)
		{
			LeanTween.cancel(_views[viewIndex], _tweens[viewIndex]);
		}
		private void FlowSnapItem(GameObject view, float delta, bool instant)
		{
			Vector3 to = new Vector3(delta * Offset, 0f, Mathf.Abs(delta) * Offset);
			if (instant)
				view.transform.position = to;
			else
				LeanTween.moveLocal(view, to, TimeTween).setEase(LeanTweenType.easeSpring);
		}
		public void FlowSnap(int target)
		{
			List<GameObject> newViews = Enumerable.Repeat((GameObject)null, _limit).ToList();
			for (int i = 0; i < _data.Count; i++)
			{
				float delta = GetDelta(target, i);
				int viewIndex = GetViewIndex(delta);
				float oldDelta = GetDelta(_current, i);
				int oldViewIndex = GetViewIndex(oldDelta);
				bool isVisible = IsVisible(delta);
				bool wasVisible = IsVisible(oldDelta);
				if (wasVisible && !isVisible)
				{
					Exit(oldViewIndex);
				}
				else if (isVisible && !wasVisible)
				{
					newViews[viewIndex] = Enter(i);
					FlowSnapItem(newViews[viewIndex], delta, true);
				}
				else if (isVisible)
				{
					FlowSnapItemCancel(oldViewIndex);
					newViews[viewIndex] = _views[oldViewIndex];
					FlowSnapItem(newViews[viewIndex], delta, false);
				}
				if (isVisible)
					UpdateName(newViews[viewIndex], viewIndex, i);
			}
			_views = newViews;
			_current = target;
		}
		private float ClampX(int dataIndex, bool negative)
		{
			float newIndex = negative ? dataIndex : newIndex = _data.Count - dataIndex - 1;
			int clamp = _data.Count * Offset + 1;
			return clamp - (Offset * newIndex);
		}
		private Vector3 FlowPanItem(int dataIndex, float delta)
		{
			bool negative = delta < 0;
			float clampX = Mathf.Clamp(delta, -ClampX(dataIndex, negative), ClampX(dataIndex, negative));
			float clampZ = Mathf.Clamp(Mathf.Abs(delta), 0f, ClampX(dataIndex, negative));
			return new Vector3(clampX, transform.position.y, clampZ);
		}
		public void FlowPan(float offset)
		{
			float target = _current - offset;
			List<GameObject> newViews = Enumerable.Repeat((GameObject)null, _limit).ToList();
			for (int i = 0; i < _data.Count; i++)
			{
				float delta = GetDelta(target, i);
				int viewIndex = GetViewIndex(delta);
				float oldDelta = GetDelta(_current, i);
				int oldViewIndex = GetViewIndex(oldDelta);
				bool isVisible = IsVisible(delta);
				bool wasVisible = IsVisible(oldDelta);
				if (wasVisible && !isVisible)
				{
					Exit(viewIndex);
				}
				else if (isVisible && !wasVisible)
				{
					newViews[viewIndex] = Enter(i);
					newViews[viewIndex].transform.localPosition = FlowPanItem(i, delta);
				}
				else if (isVisible)
				{
					FlowSnapItemCancel(viewIndex);
					newViews[viewIndex] = _views[oldViewIndex];
					newViews[viewIndex].transform.localPosition = FlowPanItem(i, delta);
				}
				if (isVisible)
					UpdateName(newViews[viewIndex], viewIndex, i);
			}
			_views = newViews;
			_current = target;
		}
		public void Inertia(float velocity)
		{
			_tweenInertia = LeanTween.value(gameObject, FlowPan, velocity, 0, TimeInertia).setEase(LeanTweenType.easeInCirc).setOnComplete(Flow).id;
		}
		public void InertiaStop()
		{
			LeanTween.cancel(gameObject, _tweenInertia);
		}
		private void UpdateName(GameObject view, int viewIndex, int dataIndex)
		{
			string text = string.Format("{0}[{1:X}]", viewIndex, _data[dataIndex]);
			view.name = text;
			view.GetComponentInChildren<TextMesh>().text = text;
		}
		private GameObject Enter(int dataIndex)
		{
			GameObject view = ItemViewPool.GetInstance();
			view.SetActive(true);
			return view;
		}
		private void Exit(int viewIndex)
		{
			GameObject view = _views[viewIndex];
			LeanTween.cancel(view);
			view.SetActive(false);
			view.name = string.Empty;
			ItemViewPool.ReturnInstance(view);
		}
		private bool IsVisible(float delta)
		{
			return Mathf.Abs(delta) < _limitSide;
		}
		private int GetDataIndex(int viewIndex)
		{
			return Mathf.RoundToInt(viewIndex - _limitSide + _current);
		}
		private int GetViewIndex(float delta)
		{
			return Mathf.RoundToInt(delta + _limitSide);
		}
		private float GetDelta(float target, int dataIndex)
		{
			return (target - dataIndex) * -1;
		}
		public int GetClosestViewIndex()
		{
			int closestIndex = -1;
			float closestDistance = float.MaxValue;
			for (int i = 0; i < _views.Count; i++)
			{
				if (_views[i])
				{
					float distance = (gameObject.transform.position - _views[i].transform.localPosition).sqrMagnitude;
					if (distance < closestDistance)
					{
						closestIndex = i;
						closestDistance = distance;
					}
				}
			}
			return GetDataIndex(closestIndex);
		}
		private int GetViewIndex(GameObject view)
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
			return GetDataIndex(found);
		}
		public void Next()
		{
			if (_current < _data.Count - 1)
			{
				FlowSnap(Mathf.RoundToInt(_current) + 1);
			}
		}
		public void Prev()
		{
			if (_current > 0)
			{
				FlowSnap(Mathf.RoundToInt(_current) - 1);
			}
		}
		protected void OnGUI()
		{
			const float size = 64f;
			Vector2 offset = new Vector2(10f, 10f);
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
			string text = string.Format("{0}/{1}", Mathf.RoundToInt(_current) + 1, _data.Count);
			GUI.Box(new Rect(offset.x, offset.y, size, size), text, centeredStyle);
		}
	}
}
