using System;
namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */


    public class IntArrayTag : Tag
    {
        public int[] Data;

        public IntArrayTag(string name)
            : base(name)
        {

        }

        public IntArrayTag(string name, int[] data)
            : base(name)
        {
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteInt(Data.Length);
            for (int i = 0; i < Data.Length; i++)
            {
                dos.WriteInt(Data[i]);
            }
        }

        public override void Load(DataInput dis)
        {
            int length = dis.ReadInt();
            Data = new int[length];
            for (int i = 0; i < length; i++)
            {
                Data[i] = dis.ReadInt();
            }
        }

        public override byte GetID()
        {
            return TAG_Int_Array;
        }

        public override string ToString()
        {
            return "[" + Data.Length + " integers]";
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                IntArrayTag o = (IntArrayTag)obj;
                return ((Data == null && o.Data == null) || (Data != null && Data.Equals(o.Data)));
            }
            return false;
        }

        public override Tag Copy()
        {
            int[] cp = new int[Data.Length];
            Buffer.BlockCopy(Data, 0, cp, 0, Data.Length * 4);
            return new IntArrayTag(GetName(), cp);
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