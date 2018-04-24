// Copyright 2017 Louis S. Berman.
//
// This file is part of OandaTicks.
//
// OandaTicks is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OandaTicks is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OandaTicks.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace OandaTicks.Common.Helpers
{
    public class SafeConfig
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();

        private string fileName = ".\\Settings.safeconfig";
        private byte[] entropy = null;
        private DataProtectionScope scope = DataProtectionScope.CurrentUser;

        public SafeConfig WithEntropy(byte[] entropy)
        {
            this.entropy = entropy;

            return this;
        }

        public SafeConfig WithFileName(string fileName)
        {
            try
            {
                if (!fileName.IsFileName())
                    throw new ArgumentOutOfRangeException(nameof(fileName));

                this.fileName = fileName;

                fileName.EnsurePathExists();

                return this;
            }
            catch (Exception error)
            {
                throw new SafeConfigException(
                    "Cannot set the SafeConfig folder", error);
            }
        }

        public SafeConfig WithCurrentUserScope()
        {
            scope = DataProtectionScope.CurrentUser;

            return this;
        }

        public SafeConfig WithLocalMachineScope()
        {
            scope = DataProtectionScope.LocalMachine;

            return this;
        }

        public SafeConfig Load()
        {
            try
            {
                if (!File.Exists(fileName))
                    return this;

                var protectedBuffer = File.ReadAllBytes(fileName);

                var unprotectedBuffer = ProtectedData.Unprotect(protectedBuffer, entropy, scope);

                var binFormatter = new BinaryFormatter();

                using (var stream = new MemoryStream(unprotectedBuffer))
                    values = (Dictionary<string, object>)binFormatter.Deserialize(stream);

                return this;
            }
            catch (Exception error)
            {
                throw new SafeConfigException(
                    $"The \"{fileName}\" SafeConfig file could not be loaded.", error);
            }
        }

        public SafeConfig Set<T>(string key, T value)
        {
            values[key] = value;

            return this;
        }

        public T Get<T>(string key) =>
            !values.ContainsKey(key) ? default(T) : (T)values[key];

        public SafeConfig Save()
        {
            try
            {
                var formatter = new BinaryFormatter();

                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, values);

                    var protectedData = ProtectedData.Protect(
                        stream.GetBuffer(), entropy, scope);

                    File.WriteAllBytes(fileName, protectedData);
                }

                return this;
            }
            catch (Exception error)
            {
                throw new SafeConfigException(
                    $"The \"{fileName}\" SafeConfig file could not be saved.", error);
            }
        }
    }
}
