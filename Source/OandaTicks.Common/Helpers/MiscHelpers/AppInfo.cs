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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OandaTicks.Common.Helpers
{
    public class AppInfo
    {
        public AppInfo(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Company = GetCompany(assembly);
            Product = GetProduct(assembly);
            Version = assembly.GetName().Version;
            Copyright = GetCopyright(assembly).Replace("©", "(c)");
        }

        public string Product { get; private set; }
        public Version Version { get; private set; }
        public string Company { get; private set; }
        public string Copyright { get; private set; }

        public string Title
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append(Product);

                sb.Append(" v");
                sb.Append(Version.Major);
                sb.Append('.');
                sb.Append(Version.Minor);

                if ((Version.Build != 0) || (Version.Revision != 0))
                {
                    sb.Append('.');
                    sb.Append(Version.Build);
                }

                if (Version.Revision != 0)
                {
                    sb.Append('.');
                    sb.Append(Version.Revision);
                }

                return sb.ToString();
            }
        }

        private static string GetCompany(Assembly assembly) =>
            assembly.GetAttribute<AssemblyCompanyAttribute>().Company;

        private static string GetCopyright(Assembly assembly) =>
            assembly.GetAttribute<AssemblyCopyrightAttribute>().Copyright;

        private static string GetProduct(Assembly assembly) =>
            assembly.GetAttribute<AssemblyProductAttribute>().Product;

        private string CleanUp(string value)
        {
            return Path.GetInvalidFileNameChars().Aggregate(value,
                (current, c) => current.Replace(c.ToString(), " ")).Trim();
        }

        public string GetLocalAppDataPath(params string[] subFolders)
        {
            if (subFolders == null)
                throw new ArgumentNullException(nameof(subFolders));

            if (!subFolders.All(sf => sf.IsTrimmed()))
                throw new ArgumentOutOfRangeException(nameof(subFolders));

            var path = Path.Combine(Environment.GetFolderPath(
               Environment.SpecialFolder.LocalApplicationData),
               CleanUp(Company), CleanUp(Product.Replace("™", "")));

            foreach (var subFolder in subFolders)
                path = Path.Combine(path, CleanUp(subFolder));

            return path;
        }
    }
}
