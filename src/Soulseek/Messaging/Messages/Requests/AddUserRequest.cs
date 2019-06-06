﻿// <copyright file="AddUserRequest.cs" company="JP Dillingham">
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

namespace Soulseek.Messaging.Messages
{
    /// <summary>
    ///     Adds a peer to the server-side watch list.
    /// </summary>
    public class AddUserRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddUserRequest"/> class.
        /// </summary>
        /// <param name="username">The username of the peer to add.</param>
        public AddUserRequest(string username)
        {
            Username = username;
        }

        /// <summary>
        ///     Gets the username of the peer to add.
        /// </summary>
        public string Username { get; }

        /// <summary>
        ///     Constructs a <see cref="Message"/> from this request.
        /// </summary>
        /// <returns>The constructed message.</returns>
        public Message ToMessage()
        {
            return new MessageBuilder()
                .Code(MessageCode.ServerAddUser)
                .WriteString(Username)
                .Build();
        }
    }
}