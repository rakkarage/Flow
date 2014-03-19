using strange.extensions.mediation.impl;
using TouchScript.Gestures;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class BumpView : View
	{
		public float ScaleTween = 1.1f;
		public float RotateTween = 5f;
		public float TimeTween = .333f;
		private Vector3 _originalScale;
		private Vector3 _originalRotation;
		private int _tweenScaleUp;
		private int _tweenScaleDown;
		private int _tweenRotateUp;
		private int _tweenRotateDown;
		[Inject]
		public InertiaStopSignal InertiaStopSignal { get; set; }
		[PostConstruct]
		public void PostConstruct()
		{
			_originalScale = gameObject.transform.localScale;
			_originalRotation = gameObject.transform.localRotation.eulerAngles;
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
			CancelTweens(o);
			Vector3 rotateTo = _originalRotation +
				new Vector3(Random.Range(-RotateTween, RotateTween), Random.Range(-RotateTween, RotateTween), Random.Range(-RotateTween, RotateTween));
			_tweenRotateUp = LeanTween.rotateLocal(o, rotateTo, TimeTween).setEase(LeanTweenType.easeInOutCubic).setLoopPingPong().id;
			Vector3 to = Vector3.Scale(_originalScale, new Vector3(ScaleTween, ScaleTween, 1f));
			_tweenScaleUp = LeanTween.scale(o, to, TimeTween).setEase(LeanTweenType.easeSpring).id;
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
			CancelTweens(o);
			_tweenScaleDown = LeanTween.rotateLocal(o, _originalRotation, TimeTween).setEase(LeanTweenType.easeSpring).id;
			_tweenRotateDown = LeanTween.scale(o, _originalScale, TimeTween).setEase(LeanTweenType.easeSpring).id;
		}
		private void CancelTweens(GameObject o)
		{
			LeanTween.cancel(o, _tweenScaleUp);
			LeanTween.cancel(o, _tweenScaleDown);
			LeanTween.cancel(o, _tweenRotateUp);
			LeanTween.cancel(o, _tweenScaleDown);
		}
	}
}
