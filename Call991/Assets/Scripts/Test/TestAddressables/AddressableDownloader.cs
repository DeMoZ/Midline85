using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ContentDelivery
{
    public interface IAddressableDownloader
    {
        Task<T> DownloadAsync<T>(string name, Action<float> onProgress, CancellationToken cancellationToken);
    }

    public class AddressableDownloader : IAddressableDownloader, IDisposable
    {

        public async Task<T> DownloadAsync<T>(string name, Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            var isLocationExists = await IsLocationExistsAsync(name, cancellationToken);
            if (!isLocationExists)
                return default;

            return await DownloadAssetAsync<T>(name, onProgress, cancellationToken);
        }

        public async Task<bool> IsDownloadedAsync(string name, CancellationToken cancellationToken)
        {
            var isExists = await IsLocationExistsAsync(name, cancellationToken);
            if (!isExists)
                return false;
            
            var handle = Addressables.GetDownloadSizeAsync(name);
            cancellationToken.Register(() =>
            {
                if (handle.IsDone)
                    return;

                Addressables.Release(handle);
                Debug.Log($"{nameof(AddressableDownloader)} {name} check location was cancelled.");
            });

            try
            {
                await handle.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{nameof(AddressableDownloader)} {name} check downloaded was cancelled.");
                return false;
            }
            
            Debug.Log($"{nameof(AddressableDownloader)} name {name} is Downloaded {handle.Result <= 0}");
            return handle.Result <= 0;
        }
        
        private async Task<bool> IsLocationExistsAsync(string name, CancellationToken cancellationToken)
        {
            var handle = Addressables.LoadResourceLocationsAsync(name);
            cancellationToken.Register(() =>
            {
                if (handle.IsDone)
                    return;

                Addressables.Release(handle);
                Debug.Log($"{nameof(AddressableDownloader)} {name} check location was cancelled.");
            });

            try
            {
                await handle.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{nameof(AddressableDownloader)} {name} check location  was cancelled.");
                return false;
            }

            var isExists = handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0;
            if (!isExists)
                Debug.LogWarning($"{nameof(AddressableDownloader)} {name} does not exists: {handle.Status}");

            return isExists;
        }

        private async Task<T> DownloadAssetAsync<T>(string name, Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            var handle = Addressables.LoadAssetAsync<T>(name);
            cancellationToken.Register(() =>
            {
                if (!handle.IsDone)
                {
                    Addressables.Release(handle);
                    Debug.Log($"{nameof(AddressableDownloader)} {name} download was cancelled.");
                }
            });

            try
            {
                while (!handle.IsDone)
                {
                    onProgress?.Invoke(handle.PercentComplete);
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"{nameof(AddressableDownloader)} {name} download was cancelled.");
                return default;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;

            Debug.LogError(
                $"{nameof(AddressableDownloader)} Failed to load {name}: {handle.Status}; {handle.OperationException}");
            return default;
        }

        public void Dispose()
        {
            
        }
    }
}