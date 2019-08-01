﻿// <copyright file="Constants.cs" company="JP Dillingham">
//     Copyright (c) JP Dillingham. All rights reserved.
//
//     This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//     of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>

namespace Soulseek
{
    internal static class Constants
    {
        internal static class WaitKey
        {
            public const string Transfer = "Transfer";
            public const string DirectTransfer = "DirectTransfer";
            public const string IndirectTransfer = "IndirectTransfer";
            public const string SolicitedPeerConnection = "SolicitedPeerConnection";
            public const string SolicitedDistributedConnection = "SolicitedDistributedConnection";
            public const string SearchRequestMessage = "SearchRequestMessage";
            public const string ChildDepthMessage = "ChildDepthMessage";
            public const string BranchRootMessage = "BranchRootMessage";
            public const string BranchLevelMessage = "BranchLevelMessage";
        }

        internal static class ConnectionType
        {
            public const string Peer = "P";
            public const string Tranfer = "F";
            public const string Distributed = "D";
        }

        internal static class ConnectionMethod
        {
            public const string Direct = "Direct";
            public const string Indirect = "Indirect";
        }
    }
}