using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AssetFinder
{
    // ********************************
    // CACHE KEY
    // ********************************
    readonly struct AssetCacheKey : IEquatable<AssetCacheKey>
    {
        public readonly string AddressKey;
        public readonly Type AssetType;

        public AssetCacheKey(string addressKey, Type assetType)
        {
            AddressKey = addressKey;
            AssetType = assetType;
        }

        public bool Equals(AssetCacheKey other)
        {
            return AddressKey == other.AddressKey && AssetType == other.AssetType;
        }

        public override bool Equals(object objectToCompare)
        {
            return objectToCompare is AssetCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AddressKey != null ? AddressKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssetType != null ? AssetType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    // ********************************
    // CACHE ENTRY
    // ********************************
    sealed class CachedHandleEntry
    {
        public AsyncOperationHandle CachedHandle;
        public int ReferenceCount;
    }

    static readonly Dictionary<AssetCacheKey, CachedHandleEntry> s_CachedHandles = new();

    // ********************************
    // FIND
    // ********************************
    public static AsyncOperationHandle<TAsset> Find<TAsset>(
        string addressKey,
        Action<AsyncOperationHandle<TAsset>> onHandleReady = null)
        where TAsset : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(addressKey))
        {
            Debug.LogError("[AssetFinder] Find called with a null or empty key.");
            return default;
        }

        var cacheKey = new AssetCacheKey(addressKey, typeof(TAsset));

        if (s_CachedHandles.TryGetValue(cacheKey, out var cachedEntry))
        {
            if (cachedEntry.CachedHandle.IsValid())
            {
                cachedEntry.ReferenceCount++;

                var typedHandle = cachedEntry.CachedHandle.Convert<TAsset>();
                onHandleReady?.Invoke(typedHandle);
                return typedHandle;
            }

            s_CachedHandles.Remove(cacheKey);
        }

        var newHandle = Addressables.LoadAssetAsync<TAsset>(addressKey);

        s_CachedHandles[cacheKey] = new CachedHandleEntry
        {
            CachedHandle = newHandle,
            ReferenceCount = 1
        };

        onHandleReady?.Invoke(newHandle);
        return newHandle;
    }

    // ********************************
    // DISPOSE (PER TYPE)
    // ********************************
    public static bool Dispose<TAsset>(string addressKey)
        where TAsset : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(addressKey))
        {
            Debug.LogWarning("[AssetFinder] Dispose called with a null or empty key.");
            return false;
        }

        var cacheKey = new AssetCacheKey(addressKey, typeof(TAsset));

        if (!s_CachedHandles.TryGetValue(cacheKey, out var cachedEntry))
            return false;

        cachedEntry.ReferenceCount--;

        if (cachedEntry.ReferenceCount > 0)
            return true;

        s_CachedHandles.Remove(cacheKey);

        if (cachedEntry.CachedHandle.IsValid())
            Addressables.Release(cachedEntry.CachedHandle);

        return true;
    }

    // ********************************
    // DISPOSE (ALL TYPES FOR KEY)
    // ********************************
    public static int DisposeAll(string addressKey)
    {
        if (string.IsNullOrEmpty(addressKey))
        {
            Debug.LogWarning("[AssetFinder] DisposeAll(key) called with a null or empty key.");
            return 0;
        }

        var keysToRelease = new List<AssetCacheKey>();

        foreach (var cachedPair in s_CachedHandles)
        {
            if (cachedPair.Key.AddressKey == addressKey)
                keysToRelease.Add(cachedPair.Key);
        }

        var releasedCount = 0;

        foreach (var cacheKey in keysToRelease)
        {
            var cachedEntry = s_CachedHandles[cacheKey];
            s_CachedHandles.Remove(cacheKey);

            if (cachedEntry.CachedHandle.IsValid())
                Addressables.Release(cachedEntry.CachedHandle);

            releasedCount++;
        }

        return releasedCount;
    }

    // ********************************
    // DISPOSE (EVERYTHING)
    // ********************************
    public static int DisposeAll()
    {
        var releasedCount = 0;

        foreach (var cachedEntry in s_CachedHandles.Values)
        {
            if (cachedEntry.CachedHandle.IsValid())
                Addressables.Release(cachedEntry.CachedHandle);

            releasedCount++;
        }

        s_CachedHandles.Clear();
        return releasedCount;
    }
}
