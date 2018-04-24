﻿// Copyright 2017 Louis S. Berman.
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

using OandaTicks.Common.Helpers;
using System.IO;

namespace OandaTicks.WinService
{
    internal static class WellKnown
    {
        public const string AuthInfoName = "AuthInfo";

        public static AppInfo AppInfo { get; } = new AppInfo(typeof(WellKnown).Assembly);

        public static string SettingsFileName => Path.Combine(
            AppInfo.GetLocalAppDataPath("Settings"), $"{AppInfo.Product}.safeconfig");
    }
}
