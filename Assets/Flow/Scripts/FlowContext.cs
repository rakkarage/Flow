using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.impl;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class FlowContext : MVCSContext
	{
		public FlowContext(MonoBehaviour contextView)
			: base(contextView)
		{
		}
		public override void Launch()
		{
			base.Launch();

			StartSignal startSignal = (StartSignal)injectionBinder.GetInstance<StartSignal>();
			startSignal.Dispatch();
		}
		protected override void addCoreComponents()
		{
			base.addCoreComponents();

			injectionBinder.Unbind<ICommandBinder>();
			injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
		}
		protected override void mapBindings()
		{
			base.mapBindings();

			commandBinder.Bind<StartSignal>().To<StartCommand>().Once();

			injectionBinder.Bind<NextSignal>().ToSingleton();
			injectionBinder.Bind<PrevSignal>().ToSingleton();
			injectionBinder.Bind<FlowSignal>().ToSingleton();
			injectionBinder.Bind<FlowToSignal>().ToSingleton();
			injectionBinder.Bind<FlowSnapSignal>().ToSingleton();
			injectionBinder.Bind<FlowPanSignal>().ToSingleton();
			injectionBinder.Bind<InertiaSignal>().ToSingleton();
			injectionBinder.Bind<InertiaStopSignal>().ToSingleton();

			injectionBinder.Bind<GameObject>().To<GameObject>();
			injectionBinder.Bind<IPool<GameObject>>().To<Pool<GameObject>>().ToSingleton();

			mediationBinder.Bind<FlowView>().To<FlowMediator>();
		}
	}
}
