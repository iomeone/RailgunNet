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
using System.Collections;
using System.Collections.Generic;

using CommonTools;

namespace Railgun
{
  public struct EventId
  {
    public class EventIdComparer : IEqualityComparer<EventId>
    {
      public bool Equals(EventId x, EventId y)
      {
        return (x.idValue == y.idValue);
      }

      public int GetHashCode(EventId x)
      {
        return x.idValue;
      }
    }

    internal static readonly EventId UNRELIABLE = new EventId(-1);
    internal static readonly EventId INVALID = new EventId(0);

    internal static readonly IntEncoder Encoder =
      new IntEncoder(
        0, 
        RailConfig.MAX_EVENT_COUNT + 1); // ID 0 is invalid

    internal static readonly EventIdComparer Comparer = new EventIdComparer();

    internal static EventId Increment(ref EventId current)
    {
      // TODO: Wrap-around arithmetic
      current = new EventId(current.idValue + 1);
      return current;
    }

    public bool IsValid
    {
      get { return (this.idValue > 0) || (this.idValue == -1); }
    }

    public bool IsReliable
    {
      get { return this.idValue > 0; }
    }

    private readonly int idValue;

    internal EventId(int EventId)
    {
      this.idValue = EventId;
    }

    public override int GetHashCode()
    {
      return this.idValue;
    }

    internal bool IsNewerThan(EventId other)
    {
      CommonDebug.Assert(this.IsReliable);
      CommonDebug.Assert(other.IsReliable);

      // TODO: Wrap-around arithmetic
      return (this.idValue > other.idValue);
    }

    public override bool Equals(object obj)
    {
      if (obj is EventId)
        return (((EventId)obj).idValue == this.idValue);
      return false;
    }

    internal int GetCost()
    {
      return EventId.Encoder.GetCost(this.idValue);
    }

    internal void Write(BitBuffer buffer)
    {
      EventId.Encoder.Write(buffer, this.idValue);
    }

    internal static EventId Read(BitBuffer buffer)
    {
      return new EventId(EventId.Encoder.Read(buffer));
    }

    internal static EventId Peek(BitBuffer buffer)
    {
      return new EventId(EventId.Encoder.Peek(buffer));
    }
  }
}