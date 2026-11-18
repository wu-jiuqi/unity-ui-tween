using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery.Recipes
{
    public enum MotionChannel { Position, Scale, Rotation, Size, Alpha, Color, Fill, TextValue }
    public readonly struct MotionBinding : IEquatable<MotionBinding>
    {
        public readonly UnityEngine.Object target;public readonly MotionChannel channel;
        public MotionBinding(UnityEngine.Object target,MotionChannel channel){this.target=target;this.channel=channel;}
        public bool Equals(MotionBinding other)=>ReferenceEquals(target,other.target)&&channel==other.channel;
        public override bool Equals(object other)=>other is MotionBinding binding&&Equals(binding);
        public override int GetHashCode()=>((ReferenceEquals(target,null)?0:target.GetInstanceID())*397)^(int)channel;
    }
    /// <summary>Atomic per-object/property leases. The displaced owner's cancellation must be silent.</summary>
    public sealed class MotionChannelArbiter
    {
        sealed class Lease {public object owner;public Action cancel;public readonly HashSet<MotionBinding> bindings=new HashSet<MotionBinding>();}
        readonly Dictionary<MotionBinding,Lease> channels=new Dictionary<MotionBinding,Lease>();
        readonly Dictionary<object,Lease> owners=new Dictionary<object,Lease>();
        public int Count=>channels.Count;
        public void Acquire(object owner,IEnumerable<MotionBinding> requested,Action superseded) {
            if(owner==null)throw new ArgumentNullException(nameof(owner));
            var keys=new HashSet<MotionBinding>();var displaced=new HashSet<Lease>();
            foreach(var key in requested){if(key.target==null)throw new ArgumentException("Destroyed or missing channel target");if(!keys.Add(key))throw new ArgumentException("Duplicate channel "+key.channel);if(channels.TryGetValue(key,out var old)&&!ReferenceEquals(old.owner,owner))displaced.Add(old);}
            foreach(var old in displaced){Release(old.owner);old.cancel?.Invoke();}
            Release(owner);var lease=new Lease{owner=owner,cancel=superseded};foreach(var key in keys){lease.bindings.Add(key);channels[key]=lease;}owners[owner]=lease;
        }
        public void Release(object owner){if(!owners.TryGetValue(owner,out var lease))return;foreach(var key in lease.bindings)if(channels.TryGetValue(key,out var current)&&ReferenceEquals(current,lease))channels.Remove(key);owners.Remove(owner);}
        public bool Owns(object owner,MotionBinding key)=>channels.TryGetValue(key,out var lease)&&ReferenceEquals(lease.owner,owner);
    }
}
