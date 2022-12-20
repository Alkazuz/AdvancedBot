namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class LongTag : Tag
    {
        public long Data;

        public LongTag(string name)
            : base(name)
        {
        }

        public LongTag(string name, long data)
            : base(name)
        {
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteLong(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadLong();
        }

        public override byte GetID()
        {
            return TAG_Long;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new LongTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                LongTag o = (LongTag)obj;
                return Data == o.Data;
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