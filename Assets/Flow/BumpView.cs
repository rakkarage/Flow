using strange.extensions.mediation.impl;
using TouchScript.Gestures;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class BumpView : View
	{
		public float Scale = 1.1f;
		public float TimeTween = 0.333f;
		private Vector3 _origiginalScale;
		private Vector3 _originalRotation;
		private int _tweenUp;
		private int _tweenDown;
		private int _tweenShakeX;
		private int _tweenShakeY;
		private int _tweenShakeZ;
		private int _tweenBack;
		[Inject]
		public InertiaStopSignal InertiaStopSignal { get; set; }
		[PostConstruct]
		public void PostConstruct()
		{
			_origiginalScale = gameObject.transform.localScale;
			_originalRotation = gameObject.transform.rotation.eulerAngles;
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
			LeanTween.cancel(o, _tweenBack);
			_tweenShakeX = LeanTween.rotateAround(o, Vector3.up, 1f, 0.2f).setEase(LeanTweenType.easeShake).setLoopClamp().setRepeat(-1).id;
			_tweenShakeY = LeanTween.rotateAround(o, Vector3.right, 1f, 0.3f).setEase(LeanTweenType.easeShake).setLoopClamp().setRepeat(-1).setDelay(0.05f).id;
			_tweenShakeZ = LeanTween.rotateAround(o, Vector3.forward, 1f, 0.4f).setEase(LeanTweenType.easeShake).setLoopClamp().setRepeat(-1).setDelay(0.1f).id;
			LeanTween.cancel(o, _tweenUp);
			LeanTween.cancel(o, _tweenDown);
			Vector3 to = Vector3.Scale(_origiginalScale, new Vector3(Scale, Scale, 1.0f));
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
			LeanTween.cancel(o, _tweenShakeX);
			LeanTween.cancel(o, _tweenShakeY);
			LeanTween.cancel(o, _tweenShakeZ);
			_tweenBack = LeanTween.rotate(o, _originalRotation, TimeTween).setEase(LeanTweenType.easeSpring).id;
			_tweenDown = LeanTween.scale(o, _origiginalScale, TimeTween).setEase(LeanTweenType.easeSpring).id;
		}
	}
}
