using System;
namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */


    public class StringTag : Tag
    {
        public string Data;

        public StringTag(string name)
            : base(name)
        {
        }

        public StringTag(string name, string data)
            : base(name)
        {
            this.Data = data;
            if (data == null) throw new ArgumentNullException("Empty string not allowed");
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteUTF(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadUTF();
        }

        public override byte GetID()
        {
            return TAG_String;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new StringTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                StringTag o = (StringTag)obj;
                return ((Data == null && o.Data == null) || (Data != null && Data.Equals(o.Data)));
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Data.GetHashCode();
                hash = (hash * 16777619) ^ GetID().GetHashCode();
                hash = (hash * 16777619) ^ GetName().GetHashCode();
                return hash;
            }
        }
    }
}