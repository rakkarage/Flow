using strange.extensions.mediation.impl;
using TouchScript.Gestures;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class BumpView : View
	{
		public float ScaleTween = 1.1f;
		public float TimeTween = .333f;
		private Vector3 _originalScale;
		private int _tweenUp;
		private int _tweenDown;
		[Inject]
		public InertiaStopSignal InertiaStopSignal { get; set; }
		[PostConstruct]
		public void PostConstruct()
		{
			_originalScale = gameObject.transform.localScale;
		}
		public void OnEnable()
		{
			GetComponent<PressGesture>().StateChanged += HandlePress;
			GetComponent<ReleaseGesture>().StateChanged += HandleRelease;
		}
		public void OnDisable()
		{
			GetComponent<PressGesture>().StateChanged -= HandlePress;
			GetComponent<ReleaseGesture>().StateChanged -= HandleRelease;
		}
		private void HandlePress(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
		{
			if (e.State == Gesture.GestureState.Recognized)
			{
				InertiaStopSignal.Dispatch();
				PressGesture gesture = sender as PressGesture;
				ScaleUp(gesture.gameObject);
			}
		}
		private void ScaleUp(GameObject o)
		{
			LeanTween.cancel(o, _tweenUp);
			LeanTween.cancel(o, _tweenDown);
			Vector3 to = Vector3.Scale(_originalScale, new Vector3(ScaleTween, ScaleTween, 1f));
			_tweenUp = LeanTween.scale(o, to, TimeTween).setEase(LeanTweenType.easeSpring).id;
		}
		private void HandleRelease(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
		{
			if (e.State == Gesture.GestureState.Recognized)
			{
				ReleaseGesture gesture = sender as ReleaseGesture;
				ScaleDown(gesture.gameObject);
			}
		}
		private void ScaleDown(GameObject o)
		{
			LeanTween.cancel(o, _tweenUp);
			LeanTween.cancel(o, _tweenDown);
			_tweenDown = LeanTween.scale(o, _originalScale, TimeTween).setEase(LeanTweenType.easeSpring).id;
		}
	}
}
