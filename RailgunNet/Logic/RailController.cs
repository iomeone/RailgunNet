﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Railgun
{
  public class RailController
  {
    public object UserData { get; set; }

    public IEnumerable<RailEntity> ControlledEntities
    {
      get { return this.controlledEntities; }
    }

    public Tick EstimatedRemoteTick
    {
      get { return this.peer.EstimatedRemoteTick; }
    }

    public IRailNetPeer NetPeer { get { return this.netPeer; } }

#if SERVER
    /// <summary>
    /// Used for setting the scope evaluator heuristics.
    /// </summary>
    public RailScopeEvaluator ScopeEvaluator
    {
      set { this.scope.Evaluator = value; }
    }

    /// <summary>
    /// Used for determining which entity updates to send.
    /// </summary>
    internal RailScope Scope { get { return this.scope; } }
#endif

    /// <summary>
    /// The entities controlled by this controller.
    /// </summary>
    private readonly HashSet<RailEntity> controlledEntities;

    /// <summary>
    /// The peer this controller represents.
    /// </summary>
    private readonly RailPeer peer;

    /// <summary>
    /// The network peer associated with this controller.
    /// </summary>
    private readonly IRailNetPeer netPeer;

#if SERVER
    private readonly RailScope scope;
#endif

    internal RailController(RailPeer peer)
    {
      this.controlledEntities = new HashSet<RailEntity>();
      this.peer = peer;
      if (peer != null)
        this.netPeer = peer.NetPeer;

#if SERVER
      this.scope = new RailScope();
#endif
    }

#if SERVER
    public void GrantControl(RailEntity entity)
    {
      this.GrantControlInternal(entity);
    }

    public void RevokeControl(RailEntity entity)
    {
      this.RevokeControlInternal(entity);
    }

    public void QueueEvent(RailEvent evnt, int attempts = 3)
    {
      this.peer.QueueEvent(evnt, attempts);
    }
#endif

    /// <summary>
    /// Detaches the controller from all controlled entities.
    /// </summary>
    internal void Shutdown()
    {
      foreach (RailEntity entity in this.controlledEntities)
        entity.AssignController(null);
      this.controlledEntities.Clear();
    }

    /// <summary>
    /// Adds an entity to be controlled by this peer.
    /// </summary>
    internal void GrantControlInternal(RailEntity entity)
    {
      RailDebug.Assert(entity.IsRemoving == false);
      if (entity.Controller == this)
        return;
      RailDebug.Assert(entity.Controller == null);

      this.controlledEntities.Add(entity);
      entity.AssignController(this);
    }

    /// <summary>
    /// Remove an entity from being controlled by this peer.
    /// </summary>
    internal void RevokeControlInternal(RailEntity entity)
    {
      RailDebug.Assert(entity.Controller == this);

      this.controlledEntities.Remove(entity);
      entity.AssignController(null);
    }
  }
}
