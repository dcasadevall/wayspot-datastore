using System;
using System.Threading.Tasks;

namespace VPSTour.lib.KVStore.io {
    public interface IKvStore {
        /// <summary>
        /// Gets the value for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if an error is received from the server</exception>
        public Task<string> GetValue(string key);

        /// <summary>
        /// Sets the value for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception">Thrown if an error is received from the server</exception>
        public Task SetValue(string key, string value);
    }
}