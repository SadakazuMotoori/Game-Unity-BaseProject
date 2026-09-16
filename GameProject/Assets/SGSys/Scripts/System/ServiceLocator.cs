//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     ServiceLocator.cs
 *    @brief    サービスロケータ
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SGSys
{
    //==========================================================================
    /**
     *    @brief       ServiceLocatorの状態をロード時にリセットする初期化クラス.
     */
    //==========================================================================
    public static class ServiceLocatorRuntimeReset
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            ServiceLocator.Reset();
        }
    }

    //==========================================================================
    /**
     *    @brief       全サービスを一括管理する非ジェネリック実体.
     */
    //==========================================================================
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, object> s_instances = new();

        public static T Get<T>() where T : class
        {
            return s_instances.TryGetValue(typeof(T), out var obj) ? (T)obj : null;
        }

        public static void Register<T>(T instance) where T : class
        {
            s_instances[typeof(T)] = instance;
        }

        public static void Unregister<T>() where T : class
        {
            s_instances.Remove(typeof(T));
        }

        public static void Reset()
        {
            s_instances.Clear();
        }
    }

    //==========================================================================
    /**
     *    @brief       型ごとのサービス登録・取得窓口.
     */
    //==========================================================================
    public static class ServiceLocator<T> where T : class
    {
        // サービスを取得する.
        public static T Instance => ServiceLocator.Get<T>();

        // サービスを登録する.
        public static void Register(T instance)
        {
            ServiceLocator.Register(instance);
        }

        // サービスを解除する.
        public static void Unregister()
        {
            ServiceLocator.Unregister<T>();
        }
    }

    //==========================================================================
    /**
     *    @brief       ServiceLocatorから現在の登録内容を取得するためのInterface.
     */
    //==========================================================================
    public interface IService<T> where T : class
    {
        //==========================================================================
        /**
         *    @brief       インスタンスを取得する.
         *    @return      現在登録されているインスタンス.
         */
        //==========================================================================
        public static T Instance => ServiceLocator<T>.Instance;
    }
}