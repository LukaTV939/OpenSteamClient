using System.Diagnostics.CodeAnalysis;
using OpenSteamClient.DI;

namespace OpenSteamworks.Client.DI;

public class OverridingContainer : IContainer
{
	public IContainer WrapperContainer { get; }
	public IContainer UnderlyingContainer { get; }

	public OverridingContainer(IContainer wrapper, IContainer underlying) {
		this.WrapperContainer = wrapper;
		this.UnderlyingContainer = underlying;
	}

	// Registrations happen on the wrapper container
	public void RegisterFactoryMethod(Type type, Delegate factoryMethod)
		=> WrapperContainer.RegisterFactoryMethod(type, factoryMethod);

	public object RegisterInstance(Type registerableType, object instance)
		=> WrapperContainer.RegisterInstance(registerableType, instance);

	public bool TryGet(Type type, [NotNullWhen(true)] out object? obj)
	{
		// First try the wrapper container
		if (WrapperContainer.TryGet(type, out obj)) {
			return true;
		}

		return UnderlyingContainer.TryGet(type, out obj);
	}
}