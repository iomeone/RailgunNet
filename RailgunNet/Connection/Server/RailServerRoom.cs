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

#if SERVER
using System.Collections.Generic;

namespace Railgun
{
  internal class RailServerRoom : RailRoom
  {
    /// <summary>
    /// Used for creating new entities and assigning them unique ids.
    /// </summary>
    private EntityId nextEntityId = EntityId.START;

    /// <summary>
    /// All controllers involved in this room.
    /// </summary>
    private readonly HashSet<RailController> controllers;

    /// <summary>
    /// The local Railgun server.
    /// </summary>
    private readonly RailServer server;

    internal RailServerRoom(RailServer server) : base(server)
    {
      this.controllers = new HashSet<RailController>();
      this.server = server;
    }

    public override T AddNewEntity<T>()
    {
      T entity = this.CreateEntity<T>();
      this.RegisterEntity(entity);
      return entity;
    }

    /// <summary>
    /// Removes an entity from the world and destroys it.
    /// </summary>
    public override void RemoveEntity(RailEntity entity)
    {
      if (entity.IsRemoving == false)
      {
        entity.Shutdown(); // Also handles the controller
        this.server.LogDestroyedEntity(entity);
      }
    }

    public override void BroadcastEvent(RailEvent evnt, int attempts = 3)
    {
       foreach (RailController controller in this.controllers)
        controller.QueueEvent(evnt, attempts);
    }

    internal void AddController(RailController controller)
    {
      this.controllers.Add(controller);
      this.OnControllerJoined(controller);
    }

    internal void RemoveController(RailController controller)
    {
      this.controllers.Remove(controller);
      this.OnControllerLeft(controller);
    }

    internal void ServerUpdate()
    {
      this.Tick = this.Tick.GetNext();
      this.OnPreRoomUpdate(this.Tick);

      foreach (RailEntity entity in this.GetAllEntities())
      {
        Tick removedTick = entity.RemovedTick;
        if (removedTick.IsValid && (removedTick <= this.Tick))
          this.toRemove.Add(entity.Id);
        else
          entity.ServerUpdate();
      }

      // Cleanup all entities marked for removal
      foreach (EntityId id in this.toRemove)
        this.RemoveEntity(id);
      this.toRemove.Clear();

      this.OnPostRoomUpdate(this.Tick);
    }

    internal void StoreStates()
    {
      foreach (RailEntity entity in this.Entities)
        entity.StoreRecord();
    }

    private T CreateEntity<T>() where T : RailEntity
    {
      T entity = RailEntity.Create<T>();
      entity.AssignId(this.nextEntityId);
      this.nextEntityId = this.nextEntityId.GetNext();
      return (T)entity;
    }
  }
}
#endif
