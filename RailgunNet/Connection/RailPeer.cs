﻿/*
 *  RailgunNet - A Client/Server Network State-Synchronization Layer for Games
 *  Copyright (c) 2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;

namespace Railgun
{
  public delegate void RailPeerEvent(RailPeer peer);

  public class RailPeer
  {
    internal event RailPeerEvent MessagesReady;

    internal INetPeer NetPeer { get { return this.NetPeer; } }
    internal int LastAckedTick { get; set; }

    private readonly INetPeer netPeer;

    internal RailPeer(INetPeer netPeer)
    {
      this.netPeer = netPeer;
      this.netPeer.MessagesReady += this.OnMessagesReady;
      this.LastAckedTick = RailClock.INVALID_TICK;
    }

    internal IEnumerable<int> ReadReceived(byte[] buffer)
    {
      return this.netPeer.ReadReceived(buffer);
    }

    internal void EnqueueSend(byte[] buffer, int length)
    {
      this.netPeer.EnqueueSend(buffer, length);
    }

    private void OnMessagesReady(INetPeer peer)
    {
      if (this.MessagesReady != null)
        this.MessagesReady.Invoke(this);
    }
  }
}