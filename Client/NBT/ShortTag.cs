namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */


    public class ShortTag : Tag
    {
        public short Data;

        public ShortTag(string name)
            : base(name)
        {
        }

        public ShortTag(string name, short data)
            : base(name)
        {
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteShort(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadShort();
        }

        public override byte GetID()
        {
            return TAG_Short;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new ShortTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ShortTag o = (ShortTag)obj;
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