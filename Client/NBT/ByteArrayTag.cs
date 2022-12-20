using System;
namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class ByteArrayTag : Tag
    {
        public byte[] Data;

        public ByteArrayTag(string name)
            : base(name)
        {
            // super(name);
        }

        public ByteArrayTag(string name, byte[] data)
            : base(name)
        {
            //   super(name);
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteInt(Data.Length);
            dos.WriteByteArray(Data);
        }

        public override void Load(DataInput dis)
        {
            int length = dis.ReadInt();
            Data = new byte[length];
            dis.ReadByteArray(Data);
        }

        public override byte GetID()
        {
            return TAG_Byte_Array;
        }

        public override string ToString()
        {
            return "[" + Data.Length + " bytes]";
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ByteArrayTag o = (ByteArrayTag)obj;
                return ((Data == null && o.Data == null) || (Data != null && Data.Equals(o.Data)));
            }
            return false;
        }

        public override Tag Copy()
        {
            byte[] cp = new byte[Data.Length];
            Buffer.BlockCopy(Data, 0, cp, 0, Data.Length);
            return new ByteArrayTag(GetName(), cp);
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