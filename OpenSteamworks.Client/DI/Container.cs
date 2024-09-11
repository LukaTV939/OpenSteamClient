using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OpenSteamworks.Client;
using OpenSteamworks.Client.Managers;
using OpenSteamworks.Client.Utils;
using Profiler;
using OpenSteamworks.Client.DI;
using OpenSteamClient.DI;
using OpenSteamClient.Logging;
using OpenSteamClient.DI.Lifetime;
using OpenSteamClient.DI.Attributes;

namespace OpenSteamworks.Client.DI;
public class Container : IContainer
{
    private readonly object factoryPlaceholderObject = new object();
    internal Dictionary<Type, object> registeredObjects { get; init; } = new();
    internal Dictionary<Type, Delegate> factories { get; init; } = new();
    private readonly ILogger? logger;

    public Container(params object[] initialServices)
    {
		this.RegisterInstance(this);
		this.RegisterInstance<IContainer>(this);

		foreach (var item in initialServices)
		{
			this.RegisterInstance(item.GetType(), item);
		}

		if (this.TryGet(out ILoggerFactory? loggerFactory)) {
			this.logger = loggerFactory.CreateLogger("Container");     
		}
    }

    public void RegisterFactoryMethod(Type type, Delegate factoryMethod)
    {
        logger?.Debug("Attempting to register factory for type '" + type.Name + "'");
        if (this.registeredObjects.ContainsKey(type))
        {
            logger?.Error("Type '" + type.Name + "' already registered.");
            throw new InvalidOperationException("Type '" + type + "' already registered.");
        }

        if (this.factories.ContainsKey(type))
        {
            logger?.Error("Factory for type '" + type.Name + "' already registered.");
            throw new InvalidOperationException("Factory for type '" + type + "' already registered.");
        }

        this.factories.Add(type, factoryMethod);
        this.registeredObjects.Add(type, factoryPlaceholderObject);
        logger?.Debug("Registered factory for type '" + type.Name + "'");
        var implementedInterfacesAttrs = type.GetCustomAttributes(typeof(DIRegisterInterfaceAttribute<>));
        foreach (var ifaceAttr in implementedInterfacesAttrs)
        {
            Type interfaceType = ifaceAttr.GetType().GetGenericArguments().First();
            logger?.Debug("Registered implemented interface factory for type '" + interfaceType.Name + "'");
            this.factories.Add(interfaceType, factoryMethod);
            this.registeredObjects.Add(interfaceType, factoryPlaceholderObject);
        }

		if (IContainerExtensions.TryGet(this, out LifetimeManager? lifetimeManager)) {
			lifetimeManager.RegisterContainerType(type);
		}
    }

    private object RunFactoryFor(Type type)
    {
        logger?.Debug("Attempting to run factory for type '" + type.Name + "'");

        if (!this.factories.ContainsKey(type))
        {
            logger?.Error("No factory for type '" + type.Name + "'");
            throw new InvalidOperationException("Factory '" + type + "' not registered");
        }

        var factoryMethod = factories[type];
        object? ret = factoryMethod.DynamicInvoke(factoryMethod.GetMethodInfo().GetParameters().Select(p => this.Get(p.ParameterType)).ToArray());
        if (ret == null)
        {
            logger?.Error("Factory for type '" + type.Name + "' returned null");
            throw new NullReferenceException("Factory for " + type + " returned null.");
        }

        List<Type> toRemove = new();
        foreach (var f in this.factories)
        {
            if (f.Value == factoryMethod) {
                toRemove.Add(f.Key);
            }
        }

        foreach (var item in toRemove)
        {
            this.factories.Remove(item);

            if (this.registeredObjects.ContainsKey(item))
            {
                if (object.ReferenceEquals(this.registeredObjects[item], factoryPlaceholderObject))
                {
                    this.registeredObjects.Remove(item);
                }
                else
                {
                    logger?.Error("Type '" + item + "' already registered (and not factory placeholder)");
                    throw new InvalidOperationException("Type '" + item + "' already registered (and not factory placeholder)");
                }
            }
        }

        logger?.Debug("Factory for type '" + type.Name + "' ran successfully. Registering result");
        this.RegisterInstance(type, ret);
        return ret;
    }

    public object RegisterInstance(Type registerableType, object instance)
    {
		logger?.Debug("Attempting to register type '" + registerableType.Name + "'");
        if (this.registeredObjects.ContainsKey(registerableType))
        {
            logger?.Error("Type '" + registerableType.Name + "' already registered.");
            throw new InvalidOperationException("Type '" + registerableType + "' already registered.");
        }

        if (instance == null)
        {
            logger?.Error("Component is null");
            throw new NullReferenceException("component is null");
        }


        this.registeredObjects.Add(registerableType, instance);
        logger?.Debug("Registered type '" + registerableType.Name + "'");

        if (IContainerExtensions.TryGet(this, out LifetimeManager? lifetimeManager)) {
			lifetimeManager.RegisterContainerType(registerableType);
		}

		var implementedInterfacesAttrs = registerableType.GetCustomAttributes(typeof(DIRegisterInterfaceAttribute<>));
		foreach (var ifaceAttr in implementedInterfacesAttrs)
		{
			Type interfaceType = ifaceAttr.GetType().GetGenericArguments().First();
			RegisterInstance(interfaceType, instance);
		}

        return instance;
    }
	
    public bool TryGet(Type type, [NotNullWhen(true)] out object? obj)
    {
		if (!this.registeredObjects.TryGetValue(type, out obj))
		{
			return false;
		}
		
		// Run the factory if it's a factory
		if (object.ReferenceEquals(obj, factoryPlaceholderObject))
		{
			obj = RunFactoryFor(type);
		}

		return true;
	}
}