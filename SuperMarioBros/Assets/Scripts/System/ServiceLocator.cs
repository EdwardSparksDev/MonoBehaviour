using System;
using System.Collections.Generic;
using UnityEngine;


public class ServiceLocator : Singleton<ServiceLocator>
{
    #region Variables & Properties
    private Dictionary<string, IGameService> services = new Dictionary<string, IGameService>();
    #endregion


    #region Mono
    //Activates the Singleton awake function
    protected override void Awake()
    {
        base.Awake();
    }
    #endregion


    #region Methods
    /// <summary>
    /// Returns the registered service of type T if present, else throws an exception
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Invalid inputs cause method invoke failure</exception>
    public T Get<T>() where T : IGameService
    {
        string key = typeof(T).Name;
        if (!services.ContainsKey(key))
        {
            Debug.LogError("Service with key \"" + key + "\" not registered");
            throw new InvalidOperationException();
        }

        return (T)services[key];
    }


    /// <summary>
    /// Registers a service of type T if not already present, otherwise aborts
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <param name="service">The service to be registered</param>
    public void Register<T>(T service) where T : IGameService
    {
        string key = typeof(T).Name;
        if (services.ContainsKey(key))
        {
            Debug.LogError("Service with key \"" + key + "\" is already registered");
            return;
        }

        services.Add(key, service);
    }


    /// <summary>
    /// Unregisters a service of type T if present, otherwise aborts
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    public void Unregister<T>() where T : IGameService
    {
        string key = typeof(T).Name;
        if (!services.ContainsKey(key))
        {
            Debug.LogError("Service with key \"" + key + "\" is not registered");
            return;
        }

        services.Remove(key);
    }
    #endregion
}

